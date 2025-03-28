using System.Linq;
using Content.Client.GameTicking.Managers;
using Content.Client.Lobby;
using Content.Shared._ERRORGATE.Hearing;
using Content.Shared.Audio.Events;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared._Finster.Audio;
using Linguini.Bundle.Errors;
using Robust.Client;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Finster.Audio;

// Part of ContentAudioSystem that is responsible for lobby music playing/stopping and round-end sound-effect.
public sealed partial class AudioEffectsSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    private static readonly ProtoId<AudioPresetPrototype> EffectPreset = "SewerPipe";

    public void OnDeafInit(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        //if (!_timing.IsFirstTimePredicted)
        //    return;

        //if (!Exists(ent))
        //    return;

        if (TryComp<DeafComponent>(_player.LocalEntity, out var comp) &&
                comp.BlockSounds &&
                TryComp<TransformComponent>(ent, out var xformComp) &&
                !ent.Comp.Global)
        {
            TryAddEffect(ent, EffectPreset);
            _audio.SetVolume(ent, -30f, ent.Comp);
        }
    }
}
