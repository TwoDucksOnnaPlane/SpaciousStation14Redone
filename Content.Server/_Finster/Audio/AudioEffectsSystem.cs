using Content.Shared._Finster.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Finster.Audio;

public sealed partial class AudioEffectsSystem : SharedAudioEffectsSystem
{
    [Dependency] protected readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, ComponentInit>(OnInit, before: [typeof(SharedAudioSystem)]);
    }

    private void OnInit(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        OnEchoInit(ent, ref args);
    }
}

