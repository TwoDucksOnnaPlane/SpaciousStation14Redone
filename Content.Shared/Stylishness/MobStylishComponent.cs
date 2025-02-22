using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Stylishness;

/// <summary>
/// 
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MobStylishComponent : Component
{
    [DataField("slicknessCap"), AutoNetworkedField]
    public float OriginalSlicknessCap = 0f;

    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField("slicknessGain"), AutoNetworkedField]
    public float OriginalSlicknessGain = 0.05f;

    /// <summary>
    /// How much slickness is consumed every time the user dodges.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StylishnessConsumedPerDamage = 1f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier DodgeSound = new SoundPathSpecifier("/Audio/_Spacious/dodge.ogg");

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float SlicknessCap;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float SlicknessGain;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Slickness = 0f;

    /// <summary>
    /// This tracks when was the last time slickness was consumed.
    /// Same principle as with GunSystem's LastFired, this should
    /// help avoid mispredictions. I think.
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastSlicknessUpdate;
}

