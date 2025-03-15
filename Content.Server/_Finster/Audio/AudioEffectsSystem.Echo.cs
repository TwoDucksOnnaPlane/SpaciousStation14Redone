using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Finster.Audio;

public sealed partial class AudioEffectsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly ProtoId<AudioPresetPrototype> EchoEffectPreset = "SewerPipe";

    private void OnEchoInit(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        ApplyEcho(ent);
    }

    public void ApplyEcho(Entity<AudioComponent> sound, ProtoId<AudioPresetPrototype>? preset = null)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!Exists(sound))
            return;

        // Фоновая музыка не должна подвергаться эффектам эха
        if (sound.Comp.Global)
            return;

        TryAddEffect(sound, preset ?? EchoEffectPreset);
    }
}
