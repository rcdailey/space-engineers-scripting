// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.ModAPI.Ingame;

namespace GridRenamer  {
public class Program : MyGridProgram  {
// @formatter:on
    private const int MaxIterations = 100;

    private readonly StateMachine _fsm;
    private readonly Logger _logger;
    private readonly RenamerLogic _renamer;
    private List<string> _args;

    public Program()
    {
        _logger = new Logger(this);
        _fsm = new StateMachine(this, _logger, UpdateFrequency.Update1);
        _renamer = new RenamerLogic(this, _logger);

        _fsm.OnEndState += _renamer.RefreshBlockList;

        _fsm.RegisterState("rename", () => _renamer.ActionRename(_args));

        _renamer.RefreshBlockList(true);
    }

    public void Main(string argument)
    {
        string action = null;
        if (argument.Length > 0)
        {
            var args = argument.Split(',').Select(s => s.ToLowerInvariant()).ToArray();
            if (args[0] == "stop")
            {
                _fsm.Interrupt();
                _logger.Log("Renamer was interrupted");
                return;
            }

            action = args[0];
            _args = args.Skip(1).ToList();
        }

        try
        {
            var processing = _fsm.Tick(action);
            if (!processing)
            {
                return;
            }

            for (var i = 0; i < _renamer.Changelog.Count; ++i)
            {
                _logger.Log($"{i + 1}. {_renamer.Changelog[i]}");
            }
        }
        catch (Exception e)
        {
            if (e.Message.Length > 0)
            {
                _logger.Log("------");
                _logger.Log($"ERROR: {e.Message}");
                // _logger.Log($"\n{e.StackTrace}");
            }
        }
    }

    public class RenamerLogic
    {
        private readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
        private readonly ILogger _logger;

        private readonly MyGridProgram _program;
        // private readonly List<IMyTerminalBlock> _toRename = new List<IMyTerminalBlock>();

        public RenamerLogic(MyGridProgram program, ILogger logger)
        {
            _program = program;
            _logger = logger;
        }

        public List<string> Changelog { get; } = new List<string>();

        public IEnumerator<object> IterateBlocks(Action<IMyTerminalBlock> callback)
        {
            var iterations = 0;
            while (iterations < _blocks.Count)
            {
                var chunk = Math.Min(MaxIterations, _blocks.Count - iterations);
                for (var i = 0; i < chunk; ++i)
                {
                    callback(_blocks[iterations + i]);
                }

                iterations += chunk;
                yield return 0;
            }
        }

        public IEnumerator<object> ActionRename(string[] args)
        {
            if (args.Length < 2)
            {
                _logger.Log("Rename requires at least 2 arguments");
                _logger.Log("USAGE: rename,OLD NAME,NEW NAME");
                throw new ArgumentException("Insufficient number of arguments");
            }

            yield return IterateBlocks(block =>
            {
                var blockName = block.CustomName;
                if (!blockName.Contains(args[0]))
                {
                    return;
                }

                Changelog.Add($"{blockName} => {args[1]}");
                // var oldName = block.CustomName;
                block.CustomName = args[1] + System.Text.RegularExpressions.Regex
                    .Match(blockName, @" \d+$", System.Text.RegularExpressions.RegexOptions.RightToLeft).Value;
            });

            yield return 0;
        }

        public void RefreshBlockList(bool succeeded)
        {
            _blocks.Clear();
            _program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_blocks,
                b => b.CubeGrid == _program.Me.CubeGrid);
            _blocks.Sort((block1, block2) =>
                string.Compare(block1.CustomName, block2.CustomName, StringComparison.Ordinal));

            // _toRename.Clear();
            Changelog.Clear();

            _logger.Clear();

            if (succeeded)
            {
                _logger.Log("Ready to work");
            }
        }
    }

    public interface ILogger
    {
        void Log(string message);
        void Clear();
    }

    private class Logger : ILogger
    {
        private readonly MyGridProgram _program;

        public Logger(MyGridProgram program)
        {
            _program = program;
        }

        public void Log(string message)
        {
            // if (_lcd != null)
            // {
            //     _lcd.WriteText(message + '\n', true);
            // }

            _program.Echo(message);
        }

        public void Clear()
        {
            // _lcd?.WriteText("");
        }
    }

    private class ChangeState
    {
        public ChangeState(IEnumerator<object> newStateMachine)
        {
            NewStateMachine = newStateMachine;
        }

        public IEnumerator<object> NewStateMachine { get; }
    }

    public class StateMachine
    {
        private readonly Stack<IEnumerator<object>> _callstack = new Stack<IEnumerator<object>>();
        private readonly UpdateFrequency _desiredUpdateFrequency;
        private readonly ILogger _logger;
        private readonly MyGridProgram _program;

        private readonly Dictionary<string, Func<IEnumerator<object>>> _stateMap =
            new Dictionary<string, Func<IEnumerator<object>>>();

        private IEnumerator<object> _nextState;
        private double _timeToWait;

        public StateMachine(MyGridProgram program, ILogger logger, UpdateFrequency desiredUpdateFrequency)
        {
            _logger = logger;
            _desiredUpdateFrequency = desiredUpdateFrequency;
            _program = program;
        }

        private IMyGridProgramRuntimeInfo Runtime => _program.Runtime;
        public bool IsBusy => _callstack.Count != 0;

        // Bool parameter represents failure (false) or success (true).
        public event Action<bool> OnEndState;

        public void RegisterState(string name, Func<IEnumerator<object>> state)
        {
            _stateMap.Add(name, state);
        }

        public bool Tick(string action = null)
        {
            if (_callstack.Count == 0 && !string.IsNullOrEmpty(action))
            {
                Func<IEnumerator<object>> state;
                if (!_stateMap.TryGetValue(action, out state))
                {
                    throw new ArgumentException($"Acton '{action}' is unknown");
                }

                BeginStateMachine(state.Invoke());
            }

            if (_callstack.Count == 0)
            {
                return false;
            }

            UpdateStateMachine();
            return true;
        }

        private void BeginStateMachine(IEnumerator<object> fsm)
        {
            _callstack.Push(fsm);
            Runtime.UpdateFrequency |= _desiredUpdateFrequency;
            // ClearLogs();
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
            catch (Exception)
            {
                Interrupt();
                throw;
            }

            if (_nextState != null)
            {
                BeginStateMachine(_nextState);
                _nextState = null;
            }

            if (_callstack.Count == 0)
            {
                Cleanup();
                _logger.Log("Done");
            }
        }

        public void Interrupt()
        {
            while (_callstack.Count > 0)
            {
                var top = _callstack.Pop();
                top.Dispose();
            }

            Cleanup();
        }

        private void Cleanup()
        {
            OnEndState?.Invoke(false);
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }
    }

//
// @formatter:off
}}
// @formatter:on
