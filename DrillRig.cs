
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off

using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace DrillRig  {
public class Program : MyGridProgram  {
// @formatter:on
//
    private readonly IMyMotorStator _grindArmHinge;
    private readonly IMyBlockGroup _grindArmLandingGearGroup;
    private readonly IMyPistonBase _grindArmPistonHoriz;
    private readonly IMyPistonBase _grindArmPistonPark;
    private readonly IMyPistonBase _grindArmPistonVert;
    private readonly IMyMotorStator _grindArmRotor;
    private readonly IMyShipMergeBlock _drillMergeBlock;

    private IEnumerator<double> _fsm;
    private double _timeToWait;
    private IMyBlockGroup _drillPistonGroup;
    private IMyShipMergeBlock _drillSupportMergeBlock;

    public Program()
    {
        // Grinder Arm Blocks
        _grindArmRotor = GridTerminalSystem.GetBlockWithName("Grinder Rotor") as IMyMotorStator;
        _grindArmHinge = GridTerminalSystem.GetBlockWithName("Grinder Hinge") as IMyMotorStator;
        _grindArmPistonVert = GridTerminalSystem.GetBlockWithName("Grinder Piston Vert") as IMyPistonBase;
        _grindArmPistonHoriz = GridTerminalSystem.GetBlockWithName("Grinder Piston Horiz") as IMyPistonBase;
        _grindArmPistonPark = GridTerminalSystem.GetBlockWithName("Grinder Park Piston") as IMyPistonBase;
        _grindArmLandingGearGroup = GridTerminalSystem.GetBlockGroupWithName("Grinder Landing Gear");

        // Drill Blocks
        _drillPistonGroup = GridTerminalSystem.GetBlockGroupWithName("Drill Pistons");
        _drillMergeBlock = GridTerminalSystem.GetBlockWithName("Drill Merge Block") as IMyShipMergeBlock;
        _drillSupportMergeBlock = GridTerminalSystem.GetBlockWithName("Support Merge Block") as IMyShipMergeBlock;
    }

    private IEnumerator<double> ExtendDrillShaft()
    {
        // _drillMergeBlock.Enabled = false;
        yield return 0;
    }

    private IEnumerator<double> ParkGrindArm()
    {
        // If it's already parked, just break immediately
        if (IsAnyLandingGearLocked(_grindArmLandingGearGroup))
        {
            yield break;
        }

        // First, turn the arm to face its parking direction
        SetRotorDirection(_grindArmRotor, 1);
        while (_grindArmRotor.Angle < MathHelper.PiOver2)
        {
            yield return 0;
        }

        // At this time, also extend the piston used to lock the landing gear onto
        _grindArmPistonPark.Extend();

        // Next, tilt the arm forward, while also raising its vertical piston,
        // which gives it enough room to avoid hitting its base blocks with
        // the bottom tips of the landing gears.
        _grindArmPistonVert.Extend();
        SetRotorDirection(_grindArmHinge, 1);
        while (_grindArmHinge.Angle < MathHelper.PiOver2)
        {
            yield return 0;
        }

        // Once the arm is down in locking position, wait 1 second before activating
        // the landing gears.
        yield return 1;

        // Enable & Lock the landing gear with a delay between
        EnableLandingGear(_grindArmLandingGearGroup, true);
        yield return 1;

        LockLandingGear(_grindArmLandingGearGroup, true);
        yield return 0;
    }

    private IEnumerator<double> ReleaseGrindArm()
    {
        // If it's already released, just break immediately
        if (_grindArmRotor.Angle <= 0)
        {
            yield break;
        }

        // Unlock and release the landing gear with a delay between
        LockLandingGear(_grindArmLandingGearGroup, false);
        yield return 1;

        EnableLandingGear(_grindArmLandingGearGroup, false);
        SetRotorDirection(_grindArmHinge, -1);
        yield return 4;

        _grindArmPistonVert.Retract();
        _grindArmPistonPark.Retract();
        while (_grindArmHinge.Angle > 0)
        {
            yield return 0;
        }

        SetRotorDirection(_grindArmRotor, -1);
        while (_grindArmRotor.Angle > 0)
        {
            yield return 0;
        }
    }

    private static void SetRotorDirection(IMyMotorStator rotor, int direction)
    {
        var velocity = Math.Abs(rotor.TargetVelocityRPM);
        rotor.TargetVelocityRPM = direction < 0 ? -velocity : velocity;
    }

    private static bool IsAnyLandingGearLocked(IMyBlockGroup group)
    {
        var landingGears = new List<IMyLandingGear>();
        group.GetBlocksOfType(landingGears);
        foreach (var gear in landingGears)
        {
            if (gear.IsLocked)
            {
                return true;
            }
        }

        return false;
    }

    private static void LockLandingGear(IMyBlockGroup group, bool shouldLock)
    {
        var landingGears = new List<IMyLandingGear>();
        group.GetBlocksOfType(landingGears);
        foreach (var gear in landingGears)
        {
            if (shouldLock)
            {
                gear.Lock();
            }
            else
            {
                gear.Unlock();
            }
        }
    }

    private static void EnableLandingGear(IMyBlockGroup group, bool shouldEnable)
    {
        var landingGears = new List<IMyLandingGear>();
        group.GetBlocksOfType(landingGears);
        foreach (var gear in landingGears)
        {
            gear.Enabled = shouldEnable;
        }
    }

    public void Main(string argument)
    {
        if (_fsm == null)
        {
            switch (argument)
            {
                case "park_grind_arm":
                    _fsm = ParkGrindArm();
                    Runtime.UpdateFrequency |= UpdateFrequency.Update10;
                    break;
                case "release_grind_arm":
                    _fsm = ReleaseGrindArm();
                    Runtime.UpdateFrequency |= UpdateFrequency.Update10;
                    break;
            }
        }

        if (_fsm == null)
        {
            return;
        }

        UpdateStateMachine();
    }

    private void UpdateStateMachine()
    {
        if (_timeToWait > 0)
        {
            _timeToWait -= Runtime.TimeSinceLastRun.TotalSeconds;
            if (_timeToWait > 0)
            {
                return;
            }

            _timeToWait = 0;
        }

        if (_fsm.MoveNext())
        {
            _timeToWait = _fsm.Current;
            return;
        }

        _fsm.Dispose();
        _fsm = null;
        Runtime.UpdateFrequency = UpdateFrequency.None;
        Echo("Done");
    }

//
// @formatter:off
}}
// @formatter:on
