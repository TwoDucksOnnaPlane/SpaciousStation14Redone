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

namespace Content.Shared._Spacious.Style;

public sealed class StyleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MobStyleComponent, ComponentInit>(OnMobStyleInit);

        SubscribeLocalEvent<MobStyleComponent, ClothingDidEquippedEvent>(ClothingDidEquippedHandler);
        SubscribeLocalEvent<MobStyleComponent, ClothingDidUnequippedEvent>(ClothingDidUnequippedHandler);
        SubscribeLocalEvent<MobStyleComponent, ProjectileReflectAttemptEvent>(ProjectileReflectAttemptHandler);
        SubscribeLocalEvent<MobStyleComponent, HitScanReflectAttemptEvent>(HitscanReflectAttemptHandler);
        SubscribeLocalEvent<MobStyleComponent, GetExplosionResistanceEvent>(GetExplosionResistanceHandler);
    }

    private void OnMobStyleInit(EntityUid uid, MobStyleComponent comp, ComponentInit args)
    {
        comp.StyleCap = comp.OriginalStyleCap;
        comp.StyleGain = comp.OriginalStyleGain;

        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return;

        var slotenum = _inventory.GetSlotEnumerator(uid);
        while (slotenum.MoveNext(out var slot))
        {
            if (!TryComp<ClothingComponent>(slot.ContainedEntity, out var clothingComp) ||
                !TryComp<ItemStyleComponent>(slot.ContainedEntity, out var itemStyle) ||
                !_inventory.InSlotWithFlags(slot.ContainedEntity.Value, clothingComp.Slots))
                continue;
                
            comp.StyleCap = MathF.Max(comp.StyleCap + itemStyle.StyleCapIncrease, 0);
            comp.StyleGain += itemStyle.StyleGainIncrease;
        }
    }

    private void ClothingDidEquippedHandler(EntityUid uid, MobStyleComponent comp, ClothingDidEquippedEvent args)
    {
        if (!TryComp<ItemStyleComponent>(args.Clothing, out var stylishComp))
            return;
        UpdateMobStyle(comp);
        comp.StyleCap = MathF.Max(comp.StyleCap + stylishComp.StyleCapIncrease, 0);
        comp.StyleGain += stylishComp.StyleGainIncrease;
    }

    private void ClothingDidUnequippedHandler(EntityUid uid, MobStyleComponent comp, ClothingDidUnequippedEvent args)
    {
        if (!TryComp<ItemStyleComponent>(args.Clothing, out var stylishComp))
            return;
        UpdateMobStyle(comp);
        comp.StyleCap -= MathF.Max(comp.StyleCap - stylishComp.StyleCapIncrease, 0);
        comp.StyleGain -= stylishComp.StyleGainIncrease;
    }

    // These events are only fired server-side.
    // Here's to hoping they will be predicted one day.
    private void HitscanReflectAttemptHandler(EntityUid uid, MobStyleComponent comp, ref HitScanReflectAttemptEvent args)
    {
        var mul = 1f;
        if (TryComp<GunComponent>(args.SourceItem, out var gun))
            mul = gun.StyleCostMultiplier;
        args.Reflected = TryDodge(uid, comp, args.Prototype.Damage?.GetTotalPositive() ?? 0, mul);
    }

    private void ProjectileReflectAttemptHandler(EntityUid uid, MobStyleComponent comp, ref ProjectileReflectAttemptEvent args)
    {
        if (!TryComp<ProjectileComponent>(args.ProjUid, out var projComp))
            return; // wtf?

        args.Cancelled = TryDodge(uid, comp, projComp.Damage.GetTotalPositive(), projComp.StyleCostMultiplier);
    }

    private bool TryDodge(EntityUid uid, MobStyleComponent comp, FixedPoint2 totalDamage, float dodgeCostMultiplier = 1f)
    {
        var slicknessConsumed = (float) totalDamage * comp.StyleConsumedPerDamage * dodgeCostMultiplier;

        UpdateMobStyle(comp);

        if (slicknessConsumed > comp.CurrentStyle + comp.StyleCap * 3 ||   // Allows to "overdraft" style points by going into negatives.
            comp.CurrentStyle < 0)                                         // Dodges are disabled until style regenerates back to zero and above.
            return false; // ain't slick enuff pal'

        comp.CurrentStyle -= slicknessConsumed;
        _popup.PopupEntity(Loc.GetString("style-dodged-message"), uid);
        _audio.PlayPvs(comp.DodgeSound, uid);
        return true;
    }

    private void GetExplosionResistanceHandler(EntityUid uid, MobStyleComponent comp, ref GetExplosionResistanceEvent args)
    {
        UpdateMobStyle(comp);
        var coeff = 1f - comp.CurrentStyle / comp.StyleCap;
        args.DamageCoefficient = coeff;
        comp.CurrentStyle /= 2; // todo change for something better
    }

    public bool UpdateSlickness(EntityUid mobUid, MobStyleComponent? comp = null)
    {
        if (!Resolve(mobUid, ref comp))
            return false;
        UpdateMobStyle(comp);
        return true;
    }

    public void UpdateMobStyle(MobStyleComponent comp)
    {
        var gained = comp.StyleGain * (float)(_timing.CurTime - comp.LastStyleUpdate).TotalSeconds;
        comp.CurrentStyle = MathF.Min(comp.CurrentStyle + gained, comp.StyleCap);
        comp.LastStyleUpdate = _timing.CurTime;
    }
}

