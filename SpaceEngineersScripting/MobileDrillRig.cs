using System;
using System.Collections.Generic;
using BulletXNA;
using Sandbox.ModAPI.Ingame;
using VRageMath;

// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
namespace MobileDrillRig  {
public class Program : MyGridProgram  {
// @formatter:on

private const float PistonRetractVelocity = -0.25f;
private const float PistonExtendVelocity = 0.004f;
private const float DrillHeadRotorVelocity = 0.5f;

//===================================================================

private readonly List<IMyPistonBase> _pistons = new List<IMyPistonBase>();
private readonly List<IMyShipDrill> _drills = new List<IMyShipDrill>();
private readonly IMyMotorAdvancedStator _drillHeadRotor;

public Program()
{
    var pistonGroup = GridTerminalSystem.GetBlockGroupWithName("Drill Rig Pistons");
    pistonGroup.GetBlocksOfType(_pistons);
    var drillGroup = GridTerminalSystem.GetBlockGroupWithName("Drill Rig Drills");
    drillGroup.GetBlocksOfType(_drills);
    _drillHeadRotor = GridTerminalSystem.GetBlockWithName("Rotor Drill Head") as IMyMotorAdvancedStator;
}

public void Main(string argument)
{
    switch (argument)
    {
        case "retract":
            RetractDrill();
            break;
        case "extend":
            ExtendDrill();
            break;
    }
}

private void ExtendDrill()
{
    foreach (var piston in _pistons)
    {
        piston.Velocity = PistonExtendVelocity;
    }

    _drillHeadRotor.TargetVelocityRPM = DrillHeadRotorVelocity;
    // _drillHeadRotor.RotorLock = false;
    _drillHeadRotor.UpperLimitDeg = float.MaxValue;
    _drillHeadRotor.LowerLimitDeg = float.MinValue;

    foreach (var drill in _drills)
    {
        drill.Enabled = true;
    }
}

private void RetractDrill()
{
    foreach (var piston in _pistons)
    {
        piston.Velocity = PistonRetractVelocity;
    }

    // _drillHeadRotor.RotorLock = true;
    _drillHeadRotor.UpperLimitRad = (float)Math.Ceiling(_drillHeadRotor.Angle / MathHelper.PiOver2) * MathHelper.PiOver2;
    // _drillHeadRotor.LowerLimitDeg = 0;

    foreach (var drill in _drills)
    {
        drill.Enabled = false;
    }
}

// @formatter:off
}}
// @formatter:on
