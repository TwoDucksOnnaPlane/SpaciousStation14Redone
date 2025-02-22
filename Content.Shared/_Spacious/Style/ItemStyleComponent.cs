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
    [DataField]
    public float StyleCapIncrease = 1.2f;

    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField]
    public float StyleGainIncrease = 0f;
}

