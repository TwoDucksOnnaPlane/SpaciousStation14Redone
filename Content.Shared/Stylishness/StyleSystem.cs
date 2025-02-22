using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Explosion;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Stylishness;

public sealed class StyleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    //private FrozenDictionary<string, DamageGroupPrototype> _damageGroups = null!; // lmao

    public override void Initialize()
    {
        SubscribeLocalEvent<MobStylishComponent, ComponentInit>(OnMobStyleInit);

        SubscribeLocalEvent<MobStylishComponent, ClothingDidEquippedEvent>(ClothingDidEquippedHandler);
        SubscribeLocalEvent<MobStylishComponent, ClothingDidUnequippedEvent>(ClothingDidUnequippedHandler);
        SubscribeLocalEvent<MobStylishComponent, ProjectileReflectAttemptEvent>(ProjectileReflectAttemptHandler);
        SubscribeLocalEvent<MobStylishComponent, HitScanReflectAttemptEvent>(HitscanReflectAttemptHandler);
        SubscribeLocalEvent<MobStylishComponent, GetExplosionResistanceEvent>(GetExplosionResistanceHandler);

        //_damageGroups = _prot.GetInstances<DamageGroupPrototype>();
        //_prot.PrototypesReloaded += _ => _damageGroups = _prot.GetInstances<DamageGroupPrototype>();
    }

    private void OnMobStyleInit(EntityUid uid, MobStylishComponent comp, ComponentInit args)
    {
        comp.SlicknessCap = comp.OriginalSlicknessCap;
        comp.SlicknessGain = comp.OriginalSlicknessGain;

        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return;

        var slotenum = _inventory.GetSlotEnumerator(uid);
        while (slotenum.MoveNext(out var slot))
        {
            if (!TryComp<ClothingComponent>(slot.ContainedEntity, out var clothingComp) ||
                !TryComp<StylishComponent>(slot.ContainedEntity, out var itemStyle) ||
                !_inventory.InSlotWithFlags(slot.ContainedEntity.Value, clothingComp.Slots))
                continue;
                
            comp.SlicknessCap = MathF.Max(comp.SlicknessCap + itemStyle.SlicknessCapIncrease, 0);
            comp.SlicknessGain += itemStyle.SlicknessGainIncrease;
        }
    }

    private void ClothingDidEquippedHandler(EntityUid uid, MobStylishComponent comp, ClothingDidEquippedEvent args)
    {
        if (!TryComp<StylishComponent>(args.Clothing, out var stylishComp))
            return;
        UpdateSlickness(comp);
        comp.SlicknessCap = MathF.Max(comp.SlicknessCap + stylishComp.SlicknessCapIncrease, 0);
        comp.SlicknessGain += stylishComp.SlicknessGainIncrease;
    }

    private void ClothingDidUnequippedHandler(EntityUid uid, MobStylishComponent comp, ClothingDidUnequippedEvent args)
    {
        if (!TryComp<StylishComponent>(args.Clothing, out var stylishComp))
            return;
        UpdateSlickness(comp);
        comp.SlicknessCap -= MathF.Max(comp.SlicknessCap - stylishComp.SlicknessCapIncrease, 0);
        comp.SlicknessGain -= stylishComp.SlicknessGainIncrease;
    }

    // These events are only fired server-side.
    // Here's to hoping they will be predicted one day.
    private void HitscanReflectAttemptHandler(EntityUid uid, MobStylishComponent comp, ref HitScanReflectAttemptEvent args)
    {
        float mul = 1f;
        if (TryComp<GunComponent>(args.SourceItem, out var gun))
            mul = gun.StyleCostMultiplier;
        args.Reflected = TryDodge(uid, comp, args.Prototype.Damage?.GetTotalPositive() ?? 0, mul);
    }

    private void ProjectileReflectAttemptHandler(EntityUid uid, MobStylishComponent comp, ref ProjectileReflectAttemptEvent args)
    {
        if (!TryComp<ProjectileComponent>(args.ProjUid, out var projComp))
            return; // wtf?

        args.Cancelled = TryDodge(uid, comp, projComp.Damage.GetTotalPositive(), projComp.StyleCostMultiplier);
    }

    private bool TryDodge(EntityUid uid, MobStylishComponent comp, FixedPoint2 totalDamage, float dodgeCostMultiplier = 1f)
    {
        float slicknessConsumed = (float) totalDamage * comp.StylishnessConsumedPerDamage * dodgeCostMultiplier;

        UpdateSlickness(comp);

        if (slicknessConsumed > comp.Slickness + comp.SlicknessCap * 3 ||   // Allows to "overdraft" style points by going into negatives.
            comp.Slickness < 0)                                             // Dodges are disabled until style regenerates back to zero and above.
            return false; // ain't slick enuff pal'

        comp.Slickness -= slicknessConsumed;
        _popup.PopupEntity(Loc.GetString("style-dodged-message"), uid);
        _audio.PlayPvs(comp.DodgeSound, uid);
        return true;
    }

    private void GetExplosionResistanceHandler(EntityUid uid, MobStylishComponent comp, ref GetExplosionResistanceEvent args)
    {
        UpdateSlickness(comp);
        float coeff = 1f - comp.Slickness / comp.SlicknessCap;
        args.DamageCoefficient = coeff;
        comp.Slickness /= 2; // todo change for something better
    }



    public bool UpdateSlickness(EntityUid mobUid, MobStylishComponent? comp = null)
    {
        if (!Resolve(mobUid, ref comp))
            return false;
        UpdateSlickness(comp);
        return true;
    }

    public void UpdateSlickness(MobStylishComponent comp)
    {
        float gained = comp.SlicknessGain * (float)(_timing.CurTime - comp.LastSlicknessUpdate).TotalSeconds;
        comp.Slickness = MathF.Min(comp.Slickness + gained, comp.SlicknessCap);
        comp.LastSlicknessUpdate = _timing.CurTime;
    }
}

