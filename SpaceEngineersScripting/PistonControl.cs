using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;


// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
namespace PistonControl {
public class Program : MyGridProgram {
// @formatter:on

private enum PistonAction
{
    Extend,
    Retract,
    Reverse,
    Unknown
}

private struct RunningData
{
    public string Group { get; set; }
    public PistonAction Action { get; set; }
}

private RunningData? _runningData = null;
private PistonAction _lastCommand = PistonAction.Unknown;

public void Main(string argument)
{
    PistonAction action;
    string group;

    if (string.IsNullOrEmpty(argument))
    {
        if (!string.IsNullOrEmpty(_runningArguments))
        {
            argument = _runningArguments;
        }
        else
        {
            Echo("No arguments. Exiting.");
            return;
        }
    }
    else
    {
        var args = argument.Split(' ');
        if (args.Length < 2)
        {
            Echo("ERROR: Insufficient number of arguments (Expects [GROUP_NAME] [COMMAND])");
            return;
        }

        action = ParsePistonAction(args[1]);
        if (action == PistonAction.Unknown)
        {
            Echo($"ERROR: Invalid argument: {args[1]}");
            return;
        }

        if (action == PistonAction.Reverse)
        {
            action = _lastCommand == PistonAction.Extend ? PistonAction.Retract : PistonAction.Extend;
        }

        _lastCommand = action;
    }

    Echo($"Start: {action} on group {args[0]}");

    var pistonGroup = GridTerminalSystem.GetBlockGroupWithName(args[0]);
    if (pistonGroup == null)
    {
        Echo($"ERROR: Group '{args[0]}' does not exist");
        return;
    }

    var pistons = new List<IMyPistonBase>();
    pistonGroup.GetBlocksOfType(pistons);
    if (pistons.Count == 0)
    {
        Echo($"ERROR: Group '{args[0]}' has no pistons in it");
        return;
    }

    var sortedPistons = SortPistonsByAssignedNumber(pistons);
    Echo($"Number of pistons in group is {sortedPistons.Count}");

    var done = true;
    foreach (var piston in GetPistons(sortedPistons, action))
    {
        if (!piston.Enabled || !piston.IsWorking)
        {
            continue;
        }

        switch (action)
        {
            case PistonAction.Extend:
                done = ProcessExtend(piston);
                break;
            case PistonAction.Retract:
                done = ProcessRetract(piston);
                break;
        }

        if (!done)
        {
            break;
        }
    }

    if (done)
    {
        _runningArguments = "";
        Runtime.UpdateFrequency = UpdateFrequency.None;
        Echo("Done");
    }
    else
    {
        _runningArguments = argument;
        // Only ask for updates during chain operation
        Runtime.UpdateFrequency = UpdateFrequency.Update10;
    }
}

private static IEnumerable<IMyPistonBase> GetPistons(Dictionary<int, IMyPistonBase> sortedPistons,
    PistonAction action)
{
    return action == PistonAction.Extend ? sortedPistons.Values : sortedPistons.Values.Reverse();
}

private static PistonAction ParsePistonAction(string argument)
{
    PistonAction action;
    if (!Enum.TryParse(argument, out action))
    {
        action = PistonAction.Unknown;
    }

    return action;
}

private bool ProcessExtend(IMyPistonBase piston)
{
    switch (piston.Status)
    {
        case PistonStatus.Extended:
        {
            Echo($"{piston.DisplayNameText} already extended");
            return true;
        }

        case PistonStatus.Extending:
        {
            Echo($"{piston.DisplayNameText} is extending");
            return false;
        }

        default:
        {
            Echo($"{piston.DisplayNameText} extended");
            piston.Extend();
            return false;
        }
    }
}

private bool ProcessRetract(IMyPistonBase piston)
{
    switch (piston.Status)
    {
        case PistonStatus.Retracted:
        {
            Echo($"{piston.DisplayNameText} already retracted");
            return true;
        }

        case PistonStatus.Retracting:
        {
            Echo($"{piston.DisplayNameText} is retracting");
            return false;
        }

        default:
        {
            Echo($"{piston.DisplayNameText} retracted");
            piston.Retract();
            return false;
        }
    }
}

private Dictionary<int, IMyPistonBase> SortPistonsByAssignedNumber(
    IEnumerable<IMyPistonBase> pistons)
{
    var sortedPistons = new Dictionary<int, IMyPistonBase>();
    foreach (var p in pistons)
    {
        var numberLength = 0;
        foreach (var c in p.DisplayNameText.Reverse())
        {
            if (!char.IsDigit(c))
            {
                break;
            }

            ++numberLength;
        }

        int pistonIndex;
        if (int.TryParse(p.DisplayNameText.Substring(p.DisplayNameText.Length - numberLength, numberLength),
            out pistonIndex))
        {
            sortedPistons.Add(pistonIndex, p);
        }
        else
        {
            Echo($"Ignoring piston with name {p.DisplayNameText}");
        }
    }

    return sortedPistons;
}

// @formatter:off
}}
// @formatter:on
