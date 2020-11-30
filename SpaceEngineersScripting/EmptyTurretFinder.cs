// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off

using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace EmptyTurretFinder  {
public class Program : MyGridProgram  {
// @formatter:on

private const string GroupName = "Wyvern Interior Turrets";
private const int MinimumAmount = 10;

//====================================================================================

private static readonly MyItemType Ammo45mm = new MyItemType("MyObjectBuilder_AmmoMagazine", "NATO_5p56x45mm");
private readonly List<MyInventoryItem> _items = new List<MyInventoryItem>();
private readonly IMyBlockGroup _turretGroup;
private readonly List<IMyLargeInteriorTurret> _turrets = new List<IMyLargeInteriorTurret>();

public Program()
{
    _turretGroup = GridTerminalSystem.GetBlockGroupWithName(GroupName);
    if (_turretGroup == null)
    {
        Echo("Didn't find group");
        return;
    }

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main()
{
    Echo($"Group: {GroupName}");
    Echo($"Min Amount: {MinimumAmount}");
    Echo($"Num Turrets: {_turrets.Count}");

    var totalLowTurrets = 0;

    _turrets.Clear();
    _turretGroup.GetBlocksOfType(_turrets);
    foreach (var t in _turrets)
    {
        _items.Clear();
        t.GetInventory().GetItems(_items, i => i.Type == Ammo45mm);
        MyFixedPoint totalAmmo = 0;
        foreach (var i in _items)
        {
            totalAmmo += i.Amount;
        }

        if (totalAmmo < MinimumAmount)
        {
            ++totalLowTurrets;
            t.ShowOnHUD = true;
            Echo($"Turret '{t.CustomName}' is low, with {totalAmmo} total ammo");
        }
        else
        {
            t.ShowOnHUD = false;
        }
    }

    Echo($"Total Low Turrets: {totalLowTurrets}");
}

// @formatter:off
}}
// @formatter:on
