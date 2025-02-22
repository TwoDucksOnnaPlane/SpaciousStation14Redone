using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Stylishness;

/// <summary>
/// Equipping an item with this component will increase mob's slickness cap and/or slickness gain.
/// The mob has to have MobStylishComponent on itself for this to work.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed partial class StylishComponent : Component
{
    [DataField]
    public float SlicknessCapIncrease = 1.2f;

    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField]
    public float SlicknessGainIncrease = 0f;

    /// <summary>
    /// Which slots the item needs to be equipped to for it to affect mob stylishness.
    /// </summary>
    [DataField]
    public SlotFlags ApplicableSlots = SlotFlags.WITHOUT_POCKET & ~SlotFlags.SUITSTORAGE;

}

