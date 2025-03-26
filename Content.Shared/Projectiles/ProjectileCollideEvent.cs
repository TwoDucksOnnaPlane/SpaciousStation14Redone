using Content.Shared.Projectiles;
using Robust.Shared.Physics.Components;

public sealed class ProjectileCollideEvent(EntityUid projectile, ProjectileComponent component, EntityUid target, PhysicsComponent body) : EntityEventArgs
{
    public EntityUid Projectile = projectile;
    public ProjectileComponent Component = component;
    public EntityUid Target = target;
    public PhysicsComponent Body = body;
}