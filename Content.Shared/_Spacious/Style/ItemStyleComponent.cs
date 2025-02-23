using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._Spacious.Style;

/// <summary>
/// Equipping an item with this component will increase mob's slickness cap and/or slickness gain.
/// The mob has to have MobStylishComponent on itself for this to work.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed partial class ItemStyleComponent : Component
{
    [DataField("styleCap")]
    public float StyleCapIncrease = 0f;

    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField("styleGain")]
    public float StyleGainIncrease = 0f;

    [DataField]
    public SlotFlags Slots = SlotFlags.WITHOUT_POCKET & ~SlotFlags.SUITSTORAGE; // without pocket and suit storage
}

