using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Sandbox.ModAPI;
using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;

namespace GridRenamer
{
    [TestFixture]
    public class Tests
    {
        private class Context
        {
            public Context()
            {
                Logger = Substitute.For<Program.ILogger>();
                GridProgram = Substitute.For<IMyGridProgram>();
                Renamer = new Program.RenamerLogic(GridProgram, Logger);
                Fsm = new Program.StateMachine(GridProgram, Logger);
            }

            public Program.ILogger Logger { get; }
            public IMyGridProgram GridProgram { get; }
            public Program.RenamerLogic Renamer { get; }
            public Program.StateMachine Fsm { get; }
        }

        private static IMyTerminalBlock NewMockBlock(string name, string expectedName = null)
        {
            var block = Substitute.For<IMyTerminalBlock>();
            block.CustomName.Returns(name);
            block.Received(expectedName != null ? 1 : 0).CustomName = expectedName;
            return block;
        }

        [Test]
        public void Rename_ListOfBlocks_CorrectNames()
        {
            // Throw in some blocks that SHOULD NOT be changed to exercise the callback (cb) and make sure it
            // is functioning correctly.
            var mockBlocks = new[]
            {
                NewMockBlock("old", "new"),
                NewMockBlock("old 1", "new 1"),
                NewMockBlock("old2"), // Should be ignored
                NewMockBlock("unrelated") // Should be ignored
            };

            var context = new Context();
            context.GridProgram.GridTerminalSystem
                .WhenForAnyArgs(o => o.GetBlocksOfType<IMyTerminalBlock>(default))
                .Do(args =>
                {
                    var blocks = args.Arg<List<IMyTerminalBlock>>();
                    var callback = args.Arg<Func<IMyTerminalBlock, bool>>();
                    blocks.AddRange(mockBlocks.Where(b => callback(b)));
                });

            context.Fsm.RegisterState("action", () => context.Renamer.ActionRename(new[] {"rename", "old", "new"}));
            context.Fsm.Tick("action");
            for (var i = 0; context.Fsm.Tick() && i < 100; ++i)
            {
            }
        }
    }
}
