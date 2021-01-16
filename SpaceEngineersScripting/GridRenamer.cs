// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox.ModAPI.Ingame;

namespace GridRenamer  {
public class Program : MyGridProgram  {
// @formatter:on

    private readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
    private readonly List<IMyTerminalBlock> _toRename = new List<IMyTerminalBlock>();
    private readonly List<string> _changelog = new List<string>();
    private readonly StateMachine _fsm;
    private readonly Logger _logger;
    private string[] _args;
    private int _totalIterations = 0;
    private int _currentIterations = 0;

    public Program()
    {
        _logger = new Logger(this);
        _fsm = new StateMachine(this, _logger);

        _fsm.OnEndState += RefreshBlockList;

        _fsm.RegisterState("rename", ActionRename);
    }

    public void Main(string argument)
    {
        _args = argument.Split(',');
        if (_args.Length == 0)
        {
            _logger.Log("No argument specified");
        }

        _fsm.Tick(_args[0].ToLower());
    }

    private IEnumerator<object> ActionRename()
    {
        RefreshBlockList();

        foreach (var block in _blocks)
        {
            if (block.CustomName.Contains(_args[1]))
            {
                _changelog.Add($"{block.CustomName} => {_args[2]}");
                var oldName = block.CustomName;
                block.CustomName = _args[2] + Regex.Match(block.CustomName, @" \d+$").Value;
            }
        }
    }

    private void RefreshBlockList()
    {
        _blocks.Clear();
        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_blocks, b => b.CubeGrid == Me.CubeGrid);
        _blocks.Sort((block1, block2) =>
            string.Compare(block1.CustomName, block2.CustomName, StringComparison.Ordinal));

        _toRename.Clear();
        _changelog.Clear();
    }

    private class Logger
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

    private class StateMachine
    {
        private readonly Stack<IEnumerator<object>> _callstack = new Stack<IEnumerator<object>>();
        private readonly Logger _logger;
        private readonly Dictionary<string, Func<IEnumerator<object>>> _stateMap =
            new Dictionary<string, Func<IEnumerator<object>>>();
        private IEnumerator<object> _nextState;
        private double _timeToWait;

        public event Action OnEndState;

        public StateMachine(MyGridProgram program, Logger logger)
        {
            _logger = logger;
            Runtime = program.Runtime;
        }

        private IMyGridProgramRuntimeInfo Runtime { get; }

        public void RegisterState(string name, Func<IEnumerator<object>> state)
        {
            _stateMap.Add(name, state);
        }



        public void Tick(string action)
        {
            if (_callstack.Count == 0)
            {
                Func<IEnumerator<object>> state;
                if (_stateMap.TryGetValue(action, out state))
                {
                    BeginStateMachine(state.Invoke());
                }
            }

            if (_callstack.Count == 0)
            {
                return;
            }

            UpdateStateMachine();
        }

        private void BeginStateMachine(IEnumerator<object> fsm)
        {
            _callstack.Push(fsm);
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
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
                while (_callstack.Count > 0)
                {
                    var top = _callstack.Pop();
                    top.Dispose();
                }

                throw;
            }

            if (_nextState != null)
            {
                BeginStateMachine(_nextState);
                _nextState = null;
            }

            if (_callstack.Count == 0)
            {
                OnEndState?.Invoke();
                Runtime.UpdateFrequency = UpdateFrequency.None;
                _logger.Log("Done");
            }
        }
    }

    // public void Save()
    // {
    // }

    // private void Load()
    // {
    // }


//
// @formatter:off
}}
// @formatter:on
