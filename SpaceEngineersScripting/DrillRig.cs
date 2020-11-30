// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off

using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace DrillRig  {
public class Program : MyGridProgram  {
// @formatter:on

        private const float DrillPistonSpeedDrilling = 0.5f; //0.1f;
        private const float DrillPistonSpeedAssembling = 1.00f;
        private const float DrillPiston2MaxDistanceRetract = 8.3f;
        private const float DrillPiston2MaxDistanceExtend = 8.3f;

        private static readonly List<IMyPistonBase> GroupLookupPiston = new List<IMyPistonBase>();
        private static readonly List<IMyLandingGear> GroupLookupLandingGear = new List<IMyLandingGear>();
        private static readonly List<IMyShipMergeBlock> GroupLookupMergeBlock = new List<IMyShipMergeBlock>();
        private static readonly List<IMyShipConnector> GroupLookupConnector = new List<IMyShipConnector>();

        private readonly Stack<IEnumerator<object>> _callstack = new Stack<IEnumerator<object>>();
        private readonly IMyShipConnector _drillConnector;
        private readonly IMyBlockGroup _drillGroup;
        private readonly IMyShipMergeBlock _drillMergeBlock;
        private readonly IMyBlockGroup _drillPistonGroup;
        private readonly IMyProjector _drillProjector;
        private readonly IMyShipMergeBlock _drillSupportMergeBlock;
        private readonly IMyBlockGroup _drillWelderGroup;
        private readonly IMyMotorStator _grindArmHinge;
        private readonly IMyBlockGroup _grindArmLandingGearGroup;
        private readonly IMyPistonBase _grindArmPistonHoriz;
        private readonly IMyPistonBase _grindArmPistonPark;
        private readonly IMyPistonBase _grindArmPistonVert;
        private readonly IMyMotorStator _grindArmRotor;
        private readonly IMyTextPanel _lcd;
        private IEnumerator<object> _nextState;
        private int _numberOfShaftSections;
        private DrillState _stateDrill = DrillState.None;
        private ExtendDrillState _stateDrillExtend = ExtendDrillState.Start;

        private double _timeToWait;

        public Program()
        {
            Load();

            // Grinder Arm Blocks
            _grindArmRotor = GetBlockOrThrow<IMyMotorStator>("Grinder Rotor");
            _grindArmHinge = GetBlockOrThrow<IMyMotorStator>("Grinder Hinge");
            _grindArmPistonVert = GetBlockOrThrow<IMyPistonBase>("Grinder Piston Vert");
            _grindArmPistonHoriz = GetBlockOrThrow<IMyPistonBase>("Grinder Piston Horiz");
            _grindArmPistonPark = GetBlockOrThrow<IMyPistonBase>("Grinder Park Piston");
            _grindArmLandingGearGroup = GetGroupOrThrow("Grinder Landing Gear");

            // Drill Blocks
            _drillPistonGroup = GetGroupOrThrow("Drill Pistons");
            _drillGroup = GetGroupOrThrow("Drills");
            _drillWelderGroup = GetGroupOrThrow("Welders");
            _drillMergeBlock = GetBlockOrThrow<IMyShipMergeBlock>("Drill Merge Block");
            _drillConnector = GetBlockOrThrow<IMyShipConnector>("Drill Connector");
            _drillSupportMergeBlock = GetBlockOrThrow<IMyShipMergeBlock>("Support Merge Block");
            _drillProjector = GetBlockOrThrow<IMyProjector>("Drill Projector");

            // LCD setup
            _lcd = GetBlockOrThrow<IMyTextPanel>("Drill Rig LCD");
            if (_lcd != null)
            {
                Echo("Found LCD");
                _lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                _lcd.FontSize = 0.5f;
                _lcd.FontColor = new Color(200, 200, 200); // less bright white is also less blurry
                _lcd.Alignment = TextAlignment.LEFT;
                _lcd.WriteText("Drill Rig Debug Logs"); // Clear all text
            }
        }

        private string LatestShaftMergeBlockSideName => $"Merge Block Side {_numberOfShaftSections}";
        private string LatestShaftMergeBlockName => $"Merge Block Bottom {_numberOfShaftSections}";
        private string PreviousShaftMergeBlockName => $"Merge Block Top {_numberOfShaftSections - 1}";
        private string LatestShaftConnectorName => $"Shaft Connector Bottom {_numberOfShaftSections}";

        private IMyShipMergeBlock PreviousShaftMergeBlock =>
            GridTerminalSystem.GetBlockWithName(PreviousShaftMergeBlockName) as IMyShipMergeBlock;

        private IMyShipMergeBlock LatestShaftMergeBlock =>
            GridTerminalSystem.GetBlockWithName(LatestShaftMergeBlockName) as IMyShipMergeBlock;

        private IMyShipMergeBlock LatestShaftMergeBlockSide =>
            GridTerminalSystem.GetBlockWithName(LatestShaftMergeBlockSideName) as IMyShipMergeBlock;

        private IMyShipConnector LatestShaftConnector =>
            GridTerminalSystem.GetBlockWithName(LatestShaftConnectorName) as IMyShipConnector;

        public void Main(string argument)
        {
            if (_callstack.Count == 0)
            {
                switch (argument)
                {
                    case "park_grind_arm":
                        BeginStateMachine(ParkGrindArm());
                        break;
                    case "release_grind_arm":
                        BeginStateMachine(ReleaseGrindArm());
                        break;
                    case "extend_drill_shaft":
                        BeginStateMachine(ExtendDrillShaft());
                        break;
                }
            }

            if (_callstack.Count == 0)
            {
                return;
            }

            UpdateStateMachine();
        }

        public void Save()
        {
            Storage = _numberOfShaftSections.ToString();
        }

        private void Load()
        {
            _numberOfShaftSections = !string.IsNullOrEmpty(Storage) ? Convert.ToInt32(Storage) : 0;
            // _numberOfShaftSections = 0;
        }

        private void EnableBlock(IMyTerminalBlock block, bool enable)
        {
            block.ApplyAction(enable ? "OnOff_On" : "OnOff_Off");
            // block.Enabled = enable;
        }

        private IEnumerator<object> ExtendDrillShaft()
        {
            Log("Start Extend Drill Shaft");

            switch (_stateDrillExtend)
            {
                // todo remove this after testing
                case ExtendDrillState.Start:
                    if (!_drillMergeBlock.IsReallyConnected(LatestShaftMergeBlock))
                    {
                        EnableBlock(_drillMergeBlock, false);
                    }

                    yield return 0;

                    // Disable the drills so they do not collect ore while the conveyor is disconnected.
                    // EnableBlocksOfType<IMyShipDrill>(_drillGroup, false);

                    // Support merge block is engaged. Disconnect the drill from the shaft
                    // Log("-Shaft connected to support. Disengage drill");
                    // _drillConnector.Disconnect();
                    // yield return 0;

                    _stateDrillExtend = ExtendDrillState.Welding;
                    goto case ExtendDrillState.Welding;

                case ExtendDrillState.Welding:
                    Log("-Welding Start");

                    // If not already, the drills should be moved all the way down
                    yield return MoveDrillPistonsAndWait(-1, DrillPistonSpeedAssembling);

                    Log("-Enable welders & projector, move drill up");

                    // Turn on the projector and welders
                    EnableBlocksOfType<IMyShipWelder>(_drillWelderGroup, true);
                    _drillProjector.Enabled = true;

                    // If the projector is "Green", that means the "Keep Projection" option isn't
                    // working (it's flaky). This supposedly "unsticks" it...
                    _drillProjector.UpdateOffsetAndRotation();

                    // Raise the drill up, at a faster speed, to allow the welder to build the projection.
                    // We first raise the piston, and after half a second, enable the drill merge block again
                    // so that the projection is attached to something before welding starts. The half second
                    // delay is needed so that the merge block doesn't re-connect to the shaft we just
                    // disconnected from.
                    MoveDrillPistons(1, DrillPistonSpeedAssembling);
                    yield return 0.2;

                    Log("-Enable drill merge block, wait on pistons");
                    EnableBlock(_drillMergeBlock, true);

                    // Wait until all pistons are fully extended. This means the projection is done welding
                    // as well.
                    while (!AllPistonsHaveStatus(_drillPistonGroup, PistonStatus.Extended))
                    {
                        yield return 0;
                    }

                    // Give the welders a bit of time to finish the last blocks
                    yield return 0.2;

                    // Disable the projector and welders
                    EnableBlocksOfType<IMyShipWelder>(_drillWelderGroup, false);
                    _drillProjector.Enabled = false;
                    yield return 1;

                    _stateDrillExtend = ExtendDrillState.RenameShaftBlocks;
                    goto case ExtendDrillState.RenameShaftBlocks;

                case ExtendDrillState.RenameShaftBlocks:
                    Log("-Rename Shaft Blocks");

                    // Find the shaft piece bottom connector & merge block and rename them
                    // This is used to find these later to disconnect them from the shaft when we retract it.
                    ++_numberOfShaftSections;
                    Save();

                    RenameBlockForShaftSection(GroupLookupMergeBlock, "Merge Block Bottom",
                        LatestShaftMergeBlockName);
                    RenameBlockForShaftSection(GroupLookupMergeBlock, "Merge Block Top",
                        PreviousShaftMergeBlockName);
                    RenameBlockForShaftSection(GroupLookupMergeBlock, "Merge Block Side",
                        LatestShaftMergeBlockSideName);
                    RenameBlockForShaftSection(GroupLookupConnector, "Shaft Connector Bottom",
                        LatestShaftConnectorName);
                    yield return 0;

                    _stateDrillExtend = ExtendDrillState.ReconnectDrillShaft;
                    goto case ExtendDrillState.ReconnectDrillShaft;

                case ExtendDrillState.ReconnectDrillShaft:
                    Log("-Reconnect drill shaft");

                    // Send the drill pistons down and wait for it to stop. This means it's made contact with
                    // the merge block of the previous shaft piece.
                    MoveDrillPistons(-1, DrillPistonSpeedAssembling);

                    // Wait for the merge blocks to connect
                    while (!LatestShaftMergeBlock.IsReallyConnected(PreviousShaftMergeBlock))
                    {
                        yield return 0;
                    }

                    // Stop the pistons after they are connected.
                    EnableDrillPistons(false);

                    // Connect the bottom connector of the new shaft section
                    // Also connect the top connector on the drill itself
                    yield return ConnectorSafetyConnect(LatestShaftConnector);
                    yield return ConnectorSafetyConnect(_drillConnector);
                    yield return 0;

                    _stateDrillExtend = ExtendDrillState.BeginDrilling;
                    goto case ExtendDrillState.BeginDrilling;

                case ExtendDrillState.BeginDrilling:
                    Log("-Begin drilling");

                    // Enable the drills again
                    EnableBlocksOfType<IMyShipDrill>(_drillGroup, true);
                    EnableBlock(_drillSupportMergeBlock, false);
                    yield return 0.5;

                    // Move the drill pistons again, this time using a slower speed.
                    // Add a delay before enabling the support merge block so that it doesn't
                    // reconnect to the bottom side merge block.
                    MoveDrillPistons(-1, DrillPistonSpeedDrilling);
                    yield return 1;

                    // Enable the drills and the support merge block so it snaps to the shaft. This will hold the shaft
                    // in place before disconnecting the drill to weld another section on to it.
                    EnableBlock(_drillSupportMergeBlock, true);
                    // _drillSupportMergeBlock.Enabled = false;
                    // yield return 0.2;
                    // _drillSupportMergeBlock.Enabled = true;

                    // Wait for either the pistons to be fully retracted or the side merge blocks to engage.
                    while (!_drillSupportMergeBlock.IsReallyConnected(LatestShaftMergeBlockSide) &&
                           !AllPistonsHaveStatus(_drillPistonGroup, PistonStatus.Retracted, PistonStatus.Stopped))
                    {
                        yield return 0;
                    }

                    yield return 0.2;

                    _stateDrillExtend = ExtendDrillState.DisconnectDrillShaft;
                    goto case ExtendDrillState.DisconnectDrillShaft;

                case ExtendDrillState.DisconnectDrillShaft:
                    Log("-Disconnect drill shaft");

                    // If the merge block still hasn't attached, move the drill up & down until it snaps in.
                    // Merge blocks can be finicky when you enable them; they don't always connect.
                    while (!_drillSupportMergeBlock.IsReallyConnected(LatestShaftMergeBlockSide))
                    {
                        Log("-Retry shaft connection");
                        MoveDrillPistons(1, DrillPistonSpeedAssembling/2);
                        EnableBlock(_drillSupportMergeBlock, false);
                        yield return 2;
                        EnableBlock(_drillSupportMergeBlock, true);
                        MoveDrillPistons(-1, DrillPistonSpeedAssembling/2);

                        // If the drill pistons are fully retracted and the support merge block is still not connected,
                        // We must have missed the chance to connect it. Extend and retract the pistons again until we
                        // convince the merge block to connect to it.
                        while (!AllPistonsHaveStatus(_drillPistonGroup, PistonStatus.Retracted) &&
                               !_drillSupportMergeBlock.IsReallyConnected(LatestShaftMergeBlockSide))
                        {
                            yield return 3;
                        }

                        // todo: Add some fail safe if the merge block never connects? Probably a retry counter?
                    }

                    // Disable the drills so they do not collect ore while the conveyor is disconnected.
                    EnableBlocksOfType<IMyShipDrill>(_drillGroup, false);
                    _drillConnector.Disconnect();
                    yield return 0.5;

                    // Support merge block is engaged. Disconnect the drill from the shaft
                    EnableBlock(_drillMergeBlock, false);
                    _drillConnector.Disconnect();
                    break;
            }


            // Ready to start drilling. We loop at this point because the start of this process disconnects
            // the support merge block.
            _stateDrillExtend = ExtendDrillState.Start;
            yield return new ChangeState(ExtendDrillShaft());
        }

        private IEnumerator<object> ConnectorSafetyConnect(IMyShipConnector connector)
        {
            // The connector isn't allowed to connect for some reason.
            // This would be unexpected. Stop the whole process.
            if (connector.Status != MyShipConnectorStatus.Connectable)
            {
                throw new InvalidOperationException(
                    $"{connector.CustomName} is not ready to be connected to the conveyor system");
            }

            // Connect the connector and wait a tiny bit to make sure we aren't checking block status too fast.
            connector.Connect();
            yield return 0.2;

            // If it didn't connect for whatever reason, bail out.
            if (connector.Status != MyShipConnectorStatus.Connected)
            {
                throw new InvalidOperationException(
                    $"{connector.CustomName} was not properly connected");
            }
        }

        private void RenameBlockForShaftSection<T>(List<T> lookupList, string name, string expectedName)
            where T : class, IMyFunctionalBlock
        {
            lookupList.Clear();
            GridTerminalSystem.GetBlocksOfType(lookupList,
                b => b.CubeGrid == _drillMergeBlock.CubeGrid && b.CustomName == name);

            if (lookupList.Count == 0 && GridTerminalSystem.GetBlockWithName(expectedName) == null)
            {
                throw new InvalidOperationException($"Did not find block for shaft piece {name}");
            }

            if (lookupList.Count > 1)
            {
                throw new InvalidOperationException($"Found more than 1 block for shaft piece: {name}");
            }

            lookupList[0].CustomName += $" {_numberOfShaftSections}";
        }

        private void EnableDrillPistons(bool enabled)
        {
            _drillPistonGroup.GetBlocksOfType<IMyPistonBase>(null, piston =>
            {
                piston.Enabled = enabled;
                return false;
            });
        }

        private void MoveDrillPistons(int direction, float speed)
        {
            EnableDrillPistons(true);
            SetPistonSpeed(_drillPistonGroup, speed);
            SetPistonDirection(_drillPistonGroup, direction);
        }

        private IEnumerator<object> MoveDrillPistonsAndWait(int direction, float speed)
        {
            MoveDrillPistons(direction, speed);

            var expectedStatus = direction < 0 ? PistonStatus.Retracted : PistonStatus.Extended;

            // The drill should be moved all the way downward first.
            while (!AllPistonsHaveStatus(_drillPistonGroup, expectedStatus, PistonStatus.Stopped))
            {
                yield return 0;
            }
        }

        private static void EnableBlocksOfType<T>(IMyBlockGroup group, bool enable) where T : class, IMyFunctionalBlock
        {
            group.GetBlocksOfType<T>(null, b =>
            {
                b.Enabled = enable;
                return false;
            });
        }

        private bool AllPistonsHaveStatus(IMyBlockGroup group, PistonStatus status, PistonStatus? optionalStatus = null)
        {
            GroupLookupPiston.Clear();
            group.GetBlocksOfType(GroupLookupPiston);
            foreach (var piston in GroupLookupPiston)
            {
                // Log($"Piston ({piston.CustomName}) Status: {piston.Status}");
                if (piston.Status != status && (optionalStatus == null || piston.Status != optionalStatus))
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator<object> ParkGrindArm()
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

        private IEnumerator<object> ReleaseGrindArm()
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

        private static void SetPistonSpeed(IMyBlockGroup group, float direction)
        {
            group.GetBlocksOfType<IMyPistonBase>(null, piston =>
            {
                SetPistonSpeed(piston, direction);
                return false;
            });
        }

        private static void SetPistonSpeed(IMyPistonBase piston, float velocity)
        {
            var positiveNegative = piston.Velocity < 0 ? -1 : 1;
            piston.Velocity = velocity * positiveNegative;
        }

        private static void SetPistonDirection(IMyBlockGroup group, int direction)
        {
            group.GetBlocksOfType<IMyPistonBase>(null, piston =>
            {
                SetPistonDirection(piston, direction);
                return false;
            });
        }

        private static void SetPistonDirection(IMyPistonBase piston, int direction)
        {
            if (direction > 0)
            {
                piston.Extend();
            }
            else
            {
                piston.Retract();
            }
        }

        private static void SetRotorDirection(IMyMotorStator rotor, int direction)
        {
            var velocity = Math.Abs(rotor.TargetVelocityRPM);
            rotor.TargetVelocityRPM = direction < 0 ? -velocity : velocity;
        }

        private static bool IsAnyLandingGearLocked(IMyBlockGroup group)
        {
            GroupLookupLandingGear.Clear();
            group.GetBlocksOfType(GroupLookupLandingGear);
            foreach (var gear in GroupLookupLandingGear)
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
            group.GetBlocksOfType<IMyLandingGear>(null, gear =>
            {
                if (shouldLock)
                {
                    gear.Lock();
                }
                else
                {
                    gear.Unlock();
                }

                return false;
            });
        }

        private static void EnableLandingGear(IMyBlockGroup group, bool shouldEnable)
        {
            group.GetBlocksOfType<IMyLandingGear>(null, gear =>
            {
                gear.Enabled = shouldEnable;
                return false;
            });
        }

        private void BeginStateMachine(IEnumerator<object> fsm)
        {
            _callstack.Push(fsm);
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
            ClearLogs();
        }

        private T GetBlockOrThrow<T>(string name) where T : class, IMyTerminalBlock
        {
            var block = GridTerminalSystem.GetBlockWithName(name) as T;
            if (block == null)
            {
                throw new ArgumentNullException($"Can not find block with name: {name}");
            }

            return block;
        }

        private IMyBlockGroup GetGroupOrThrow(string name)
        {
            var group = GridTerminalSystem.GetBlockGroupWithName(name);
            if (group == null)
            {
                throw new ArgumentNullException($"Can not find block with name: {name}");
            }

            return group;
        }

        private void Log(string message)
        {
            if (_lcd != null)
            {
                _lcd.WriteText(message + '\n', true);
            }

            Echo(message);
        }

        private void ClearLogs()
        {
            _lcd?.WriteText("");
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

            try
            {
                var top = _callstack.Peek();
                if (top.MoveNext())
                {
                    if (top.Current is double || top.Current is float || top.Current is int)
                    {
                        _timeToWait = Convert.ToDouble(top.Current);
                    }
                    else if (top.Current is IEnumerator<object>)
                    {
                        _callstack.Push((IEnumerator<object>) top.Current);
                    }
                    else if (top.Current is ChangeState)
                    {
                        _nextState?.Dispose(); // If one was set previously, discard it
                        _nextState = ((ChangeState) top.Current).NewStateMachine;
                    }

                    return;
                }

                _callstack.Pop();
                top.Dispose();
            }
            catch (Exception e)
            {
                Log($"Exception: {e.Message}");
                Log(e.ToString());
                while (_callstack.Count > 0)
                {
                    var top = _callstack.Pop();
                    top.Dispose();
                }
            }

            if (_nextState != null)
            {
                BeginStateMachine(_nextState);
                _nextState = null;
            }

            if (_callstack.Count == 0)
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Log("Done");
            }
        }

        private enum DrillState
        {
            None,
            Extending,
            Retracting
        }

        private enum ExtendDrillState
        {
            Start,
            Welding,
            RenameShaftBlocks,
            ReconnectDrillShaft,
            BeginDrilling,
            DisconnectDrillShaft
        }

        private class ChangeState
        {
            public ChangeState(IEnumerator<object> newStateMachine)
            {
                NewStateMachine = newStateMachine;
            }

            public IEnumerator<object> NewStateMachine { get; }
        }
    }

    internal static class BlockExtensions
    {
        public static bool IsReallyConnected(this IMyShipMergeBlock source, IMyShipMergeBlock target)
        {
            return target != null && source.CubeGrid == target.CubeGrid;
        }


//
// @formatter:off
}}
// @formatter:on
