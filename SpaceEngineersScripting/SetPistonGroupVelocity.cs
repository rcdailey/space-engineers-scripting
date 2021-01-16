using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
namespace SorterControl  {
public class Program : MyGridProgram  {
// @formatter:on

private const float SlowSpeed = 0.03f;
private const float FastSpeed = 1.00f;
private bool _flip;

public void Main()
{
    var pistonGroup = GridTerminalSystem.GetBlockGroupWithName("PistonsExtend");
    if (pistonGroup == null)
    {
        Echo("ERROR: Group does not exist");
        return;
    }

    var pistons = new List<IMyPistonBase>();
    pistonGroup.GetBlocksOfType(pistons);
    if (pistons.Count == 0)
    {
        Echo("ERROR: Group has no sorters in it");
        return;
    }

    var desiredVelocity = _flip ? SlowSpeed : FastSpeed;
    _flip = !_flip;

    foreach (var p in pistons)
    {
        p.Velocity = p.Velocity < 0 ? -desiredVelocity : desiredVelocity;
    }
}

// @formatter:off
}}
// @formatter:on
