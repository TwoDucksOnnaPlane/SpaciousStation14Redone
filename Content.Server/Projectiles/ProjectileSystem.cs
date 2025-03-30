using Content.Server.Administration.Logs;
using Content.Server.Damage.Systems;
using Content.Server.Effects;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Database;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Projectiles;

public sealed class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ColorFlashEffectSystem _color = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileCollideEvent>(OnProjectileCollide);
        SubscribeLocalEvent<EmbeddableProjectileComponent, DamageExamineEvent>(OnDamageExamine, after: [typeof(DamageOtherOnHitSystem)]);
    }

    private void OnProjectileCollide(ProjectileCollideEvent ev)
    {
        var (uid, component, target, body) = (ev.Projectile, ev.Component, ev.Target, ev.Body);

        // Server-only logic (damage, sound, logging)
        var attemptEv = new ProjectileReflectAttemptEvent(uid, component, false);
        RaiseLocalEvent(target, ref attemptEv);
        if (attemptEv.Cancelled)
        {
            SetShooter(uid, component, target);
            return;
        }

        var hitEv = new ProjectileHitEvent(component.Damage, target, component.Shooter);
        RaiseLocalEvent(uid, ref hitEv);

        var modifiedDamage = _damageableSystem.TryChangeDamage(target, hitEv.Damage, component.IgnoreResistances, origin: component.Shooter);
        var deleted = Deleted(target);

        if (modifiedDamage is not null && EntityManager.EntityExists(component.Shooter))
        {
            if (modifiedDamage.AnyPositive() && !deleted)
                _color.RaiseEffect(Color.Red, [target], Filter.Pvs(target, entityManager: EntityManager));

            _adminLogger.Add(
            LogType.BulletHit,
            HasComp<ActorComponent>(target) ? LogImpact.Extreme : LogImpact.High,
            $"Projectile {ToPrettyString(uid):projectile} shot by {ToPrettyString(component.Shooter!.Value):user} " +
            $"hit {ToPrettyString(target):target} and dealt {modifiedDamage?.GetTotal() ?? 0:damage} damage"
        );
        }

        if (!deleted)
        {
            _guns.PlayImpactSound(target, modifiedDamage, component.SoundHit, component.ForceSound);
            if (!body.LinearVelocity.IsLengthZero())
                _sharedCameraRecoil.KickCamera(target, body.LinearVelocity.Normalized());
        }

        if (component.ImpactEffect != null && TryComp(uid, out TransformComponent? xform))
            RaiseNetworkEvent(new ImpactEffectEvent(component.ImpactEffect, GetNetCoordinates(xform.Coordinates)), Filter.Pvs(xform.Coordinates, entityMan: EntityManager));
}

    private void OnDamageExamine(EntityUid uid, EmbeddableProjectileComponent component, ref DamageExamineEvent args)
    {
        if (!component.EmbedOnThrow)
            return;

        if (!args.Message.IsEmpty)
            args.Message.PushNewline();

        var isHarmful = TryComp<EmbedPassiveDamageComponent>(uid, out var passiveDamage) && passiveDamage.Damage.AnyPositive();
        var loc = isHarmful
            ? "damage-examine-embeddable-harmful"
            : "damage-examine-embeddable";

        var staminaCostMarkup = FormattedMessage.FromMarkupOrThrow(Loc.GetString(loc));
        args.Message.AddMessage(staminaCostMarkup);
    }
}
