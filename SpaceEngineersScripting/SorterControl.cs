using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
namespace SorterControl  {
public class Program : MyGridProgram  {
// @formatter:on

public void Main(string argument)
{
    var pistonGroup = GridTerminalSystem.GetBlockGroupWithName(argument);
    if (pistonGroup == null)
    {
        Echo($"ERROR: Group '{argument}' does not exist");
        return;
    }

    var sorters = new List<IMyConveyorSorter>();
    pistonGroup.GetBlocksOfType(sorters);
    if (sorters.Count == 0)
    {
        Echo($"ERROR: Group '{argument}' has no sorters in it");
        return;
    }

    var filterList = new List<MyInventoryItemFilter>
    {
        new MyInventoryItemFilter("MyObjectBuilder_Ore/Stone", false),
        new MyInventoryItemFilter("MyObjectBuilder_Ore/Ice", false),
    };

    foreach (var s in sorters)
    {
        s.DrainAll = true;
        s.SetFilter(MyConveyorSorterMode.Whitelist, filterList);
    }

    Echo($"Configured {sorters.Count} sorters");
}

// @formatter:off
}}
// @formatter:on
