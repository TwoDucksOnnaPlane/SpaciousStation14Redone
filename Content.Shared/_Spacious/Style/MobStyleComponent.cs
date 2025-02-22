using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._Spacious.Style;

/// <summary>
/// 
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MobStyleComponent : Component
{
    [DataField("styleCap"), AutoNetworkedField]
    public float OriginalStyleCap = 0f;

    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField("styleGain"), AutoNetworkedField]
    public float OriginalStyleGain = 0.5f;

    /// <summary>
    /// How much slickness is consumed every time the user dodges.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StyleConsumedPerDamage = 1f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier DodgeSound = new SoundPathSpecifier("/Audio/_Spacious/dodge.ogg");

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float StyleCap;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float StyleGain;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float CurrentStyle = 0f;

    /// <summary>
    /// This tracks when was the last time slickness was consumed.
    /// Same principle as with GunSystem's LastFired, this should
    /// help avoid mispredictions. I think.
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastStyleUpdate;
}

