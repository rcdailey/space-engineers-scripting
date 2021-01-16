using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
namespace PistonPercentage  {
public class Program : MyGridProgram  {
// @formatter:on

//===================================================================

private readonly List<IMyPistonBase> _pistons = new List<IMyPistonBase>();
private IMyTextPanel _lcd;

public Program()
{
    var pistonGroup = GridTerminalSystem.GetBlockGroupWithName("Drill Rig Pistons");
    pistonGroup.GetBlocksOfType(_pistons);
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main()
{
    float totalDistance = 0;
    float totalMovement = 0;
    foreach (var piston in _pistons)
    {
        totalDistance += piston.HighestPosition - piston.LowestPosition;
        totalMovement += piston.CurrentPosition - piston.LowestPosition;
    }

    var percentageText = $"Pistons: {totalMovement / totalDistance * 100:0.#}%";

    _lcd = GridTerminalSystem.GetBlockWithName("LCD Piston Status") as IMyTextPanel;
    if (_lcd != null)
    {
        _lcd.ContentType = ContentType.TEXT_AND_IMAGE;
        _lcd.WriteText(percentageText);
    }

    Echo(percentageText);
}

// @formatter:off
}}
// @formatter:on
