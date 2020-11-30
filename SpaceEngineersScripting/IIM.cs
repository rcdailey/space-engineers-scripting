// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IIM  {
public class Program : MyGridProgram  {
// @formatter:on


// Isy's Inventory Manager
// ===================
// Version: 2.8.2
// Date: 2020-06-01

// Guide: http://steamcommunity.com/sharedfiles/filedetails/?id=1226261795

//  =======================================================================================
//                                                                            --- Configuration ---
//  =======================================================================================

// --- Sorting ---
// =======================================================================================

// Define the keyword a cargo container has to contain in order to be recognized as a container of the given type.
        private const string oreContainerKeyword = "Ores";
        private const string ingotContainerKeyword = "Ingots";
        private const string componentContainerKeyword = "Components";
        private const string toolContainerKeyword = "Tools";
        private const string ammoContainerKeyword = "Ammo";
        private const string bottleContainerKeyword = "Bottles";


// --- Special Loadout Containers ---
// =======================================================================================

// Keyword an inventory has to contain to be filled with a special loadout (see in it's custom data after you renamed it!)
// Special containers will be filled with your wanted amount of items and never be drained by the auto sorting!
        private const string specialContainerKeyword = "Special";

        private const string _objectBuilderPrefix = "MyObjectBuilder_";
        private const string Ȅ = "Ore";
        private const string ȃ = "Ingot";

        private const string Ȃ =
            "Component";

        private const string _ammoMagazine = "AmmoMagazine";
        private const string _oxygenContainerObject = "OxygenContainerObject";
        private const string _gasContainerObject = "GasContainerObject";

        private const string Ș
            = "PhysicalGunObject";

        private const string ȡ = "PhysicalObject";
        private const string ȟ = "ConsumableItem";
        private const string Ȟ = "Datapad";

        private const
            string ȝ = _objectBuilderPrefix + "BlueprintDefinition/";

        private int Ǎ, ǌ;

        private readonly Dictionary<MyDefinitionId, double> Ȁ
            = new Dictionary<MyDefinitionId, double>();


//  =======================================================================================
//                                                                      --- End of Configuration ---
//                                                        Don't change anything beyond this point!
//  =======================================================================================


        private readonly List<IMyTerminalBlock> ɐ = new List<IMyTerminalBlock>();

        private readonly List<IMyTerminalBlock> ɑ = new List<IMyTerminalBlock>();
        private int ɒ;

// Are special containers allowed to 'steal' items from other special containers with a lower priority?
        private bool allowSpecialSteal = true;

// If you want an assembler to only assemble or only disassemble, use the following keywords in its name.
// A assembler without a keyword will do both tasks
        private readonly string assembleKeyword = "!assemble-only";

// Margins for assembling or disassembling items in percent based on the wanted amount (default: 0 = exact value).
// Examples:
// assembleMargin = 5 with a wanted amount of 100 items will only produce new items, if less than 95 are available.
// disassembleMargin = 10 with a wanted amount of 1000 items will only disassemble items if more than 1100 are available.
        private readonly double assembleMargin;

// Assign new containers if a type is full or not present?
        private bool assignNewContainers;


// --- Automated container assignment ---
// =======================================================================================

// Master switch. If this is set to false, automated container un-/assignment is disabled entirely.
        private bool autoContainerAssignment;

// A LCD with the keyword "Autocrafting" is required where you can set the wanted amount!
// This has multi LCD support. Just append numbers after the keyword, like: "LCD Autocrafting 1", "LCD Autocrafting 2", ..
        private readonly string autocraftingKeyword = "Autocrafting";
        private List<IMyTerminalBlock> ɓ = new List<IMyTerminalBlock>();

// Balance items between containers of the same type? This will result in an equal amount of all items in all containers of that type.
        private bool balanceTypeContainers;
        private List<IMyTerminalBlock> ɔ = new List<IMyTerminalBlock>();
        private int ɕ;

// Enable connection check for inventories (needed for [No Conveyor] info)?
        private bool connectionCheck;
        private string ɗ = "";

// Default screen font, fontsize and padding, when a screen is first initialized. Fonts: "Debug" or "Monospace"
        private readonly string defaultFont = "Debug";
        private readonly float defaultFontSize = 0.6f;
        private readonly float defaultPadding = 2f;

// Disable autocrafting in survival kits when you have regular assemblers?
        private bool disableBasicAutocrafting = true;
        private readonly string disassembleKeyword = "!disassemble-only";
        private readonly double disassembleMargin;
        private readonly string ǲ = "ship_mode;\n";

        private readonly Dictionary<MyDefinitionId, MyDefinitionId> Ǳ = new Dictionary<
            MyDefinitionId, MyDefinitionId>();

        private readonly List<double> ǅ = new List<double>(new double[600]);
        private readonly string[] ɖ = {"/", "-", "\\", "|"};

        private readonly Dictionary<string, double> Ȇ = new Dictionary<string, double>
        {
            {"Cobalt", 0.3},
            {
                "Gold", 0.01
            },
            {"Iron", 0.7}, {"Magnesium", 0.007}, {"Nickel", 0.4}, {"Platinum", 0.005}, {"Silicon", 0.7},
            {"Silver", 0.1}, {"Stone", 0.014}, {"Uranium", 0.01}
        };

        private List<
            IMyTerminalBlock> ə = new List<IMyTerminalBlock>();

        private int ɚ;
        private int ɘ;


// --- Assembler Cleanup ---
// =======================================================================================

// This cleans up assemblers, if they have no queue and puts the contents back into a cargo container.
        private bool enableAssemblerCleanup = true;


// --- Autocrafting ---
// =======================================================================================

// Enable autocrafting or autodisassembling (disassembling will disassemble everything above the wanted amounts)
// All assemblers will be used. To use one manually, add the manualMachineKeyword to it (by default: "!manual")
        private bool enableAutocrafting = true;
        private bool enableAutodisassembling = true;

// Autocraft ingots from stone in survival kits until you have proper refineries?
        private bool enableBasicIngotCrafting = true;


// --- O2/H2 generator handling ---
// =======================================================================================

// Enable balancing of ice in O2/H2 generators?
// All O2/H2 generators will be used. To use one manually, add the manualMachineKeyword to it (by default: "!manual")
        private bool enableIceBalancing = true;


// --- Internal item sorting ---
// =======================================================================================

// Sort the items inside all containers?
// Note, that this could cause inventory desync issues in multiplayer, so that items are invisible
// or can't be taken out. Use at your own risk!
        private bool enableInternalSorting;

// Enable name correction? This option will automtically correct capitalization, e.g.: !iim-main -> !IIM-main
        private bool enableNameCorrection = true;


// --- Refinery handling ---
// =======================================================================================

// By enabling ore balancing, the script will balance the ores between all refinieres so that every refinery has the same amount of ore in it.
// To still use a refinery manually, add the manualMachineKeyword to it (by default: "!manual")
        private bool enableOreBalancing = true;

// Enable script assisted refinery filling? This will move in the most needed ore and will make room, if the refinery is already full
// Also, the script puts as many ores into the refinery as possible and will pull ores even from other refineries if needed.
        private bool enableScriptRefineryFilling = true;


// --- Reactor handling ---
// =======================================================================================

// Enable balancing of uranium in reactors? (Note: conveyors of reactors are turned off to stop them from pulling more)
// All reactors will be used. To use one manually, add the manualMachineKeyword to it (by default: "!manual")
        private bool enableUraniumBalancing = true;
        private bool excludeGrinders;

// Exclude welders or grinders from sorting? Set this to true, if you have huge welder or grinder walls!
        private bool excludeWelders;


// --- Settings for enthusiasts ---
// =======================================================================================

// Extra breaks between script methods in ticks (1 tick = 16.6ms).
        private readonly double extraScriptTicks = 0;
        private int ɛ;
        private int ɜ;
        private int ɝ;
        private int ɞ;

        private readonly SortedSet<MyDefinitionId> Ȝ = new SortedSet<MyDefinitionId>(new Ŧ());

// Fill bottles before storing them in the bottle container?
        private bool fillBottles = true;

// Put ice into O2/H2 generators that are turned off? (default: false)
        private bool fillOfflineGenerators;

// Put uranium into reactors that are turned off? (default: false)
        private bool fillOfflineReactors;

// If you want an ore to always be refined first, simply remove the two // in front of the ore name to enable it.
// Enabled ores are refined in order from top to bottom so if you removed several // you can change the order by
// copying and pasting them inside the list. Just be careful to keep the syntax correct: "OreName",
// By default stone is enabled and will always be refined first.
        private readonly List<string> fixedRefiningList = new List<string>
        {
            "Stone"
            //"Iron",
            //"Nickel",
            //"Cobalt",
            //"Silicon",
            //"Uranium",
            //"Silver",
            //"Gold",
            //"Platinum",
            //"Magnesium",
            //"Scrap",
        };

        private readonly List<IMyTerminalBlock> ɢ = new List<
            IMyTerminalBlock>();

        private readonly string Ǵ = "IIM Autocrafting";

        private DateTime ǧ;

        private readonly string[] Ǧ =
        {
            "Get inventory blocks", "Find new items", "Create item lists", "Name correction",
            "Assign containers", "Fill special containers", "Sort items", "Container balancing", "Internal sorting",
            "Add fill level to names",
            "Get global item amount", "Get assembler queue", "Autocrafting", "Sort assembler queue",
            "Clean up assemblers", "Learn unknown blueprints",
            "Fill refineries", "Ore balancing", "Ice balancing", "Uranium balancing"
        };

        private readonly Dictionary<MyDefinitionId, int> ǥ = new Dictionary<MyDefinitionId, int>();
        private int ɠ;

        private List<IMyTerminalBlock> ɣ = new List<IMyTerminalBlock>();
        private int ɤ;
        private int ɡ;
        private bool ɦ = true;
        private bool ɥ;

// Add the header to every screen when using multiple autocrafting LCDs?
        private bool headerOnEveryScreen;

// Keyword a block name has to contain to be excluded from item counting (used by autocrafting and inventory panels)
// This list is expandable - just separate the entries with a ",". But it's also language specific, so adjust it if needed.
// Default: string[] hiddenContainerKeywords = { "Hidden" };
        private readonly string[] hiddenContainerKeywords = {"Hidden"};
        private int _scriptStep;
        private readonly HashSet<string> ɪ = new HashSet<string>();
        private int ɨ;

        private readonly HashSet<string> ȉ = new HashSet<string>
        {
            "Uranium",
            "Silicon", "Silver", "Gold", "Platinum", "Magnesium", "Iron", "Nickel", "Cobalt", "Stone", "Scrap"
        };

        private List<MyItemType> Ȉ = new List<MyItemType>();
        private string ȋ = "";
        private MyDefinitionId Ȋ;
        private int ɩ;

// Ice fill level in percent in order to be able to fill bottles? (default: 90)
// Note: O2/H2 generators will pull more ice automatically if value is below 60%
        private readonly double iceFillLevelPercentage = 90;

// To display current item amounts of different types, add the following keyword to any LCD name
// and follow the on screen instructions.
        private readonly string inventoryLCDKeyword = "!IIM-inventory";

        private readonly Dictionary<MyDefinitionId, MyDefinitionId>
            ǰ = new Dictionary<MyDefinitionId, MyDefinitionId>();

        private int ɟ;

        private readonly List<IMyTerminalBlock> ʄ = new List<IMyTerminalBlock>();
        private string ǩ = "";

        private string
            Ǩ = "";

        private StringBuilder ɫ = new StringBuilder();

        private bool ɭ;
        private HashSet<string> ɬ = new HashSet<string>();

        private bool ɮ;

// You can teach the script new crafting recipes, by adding one of the following tags to an assembler's name.
// This is needed if the autocrafting screen shows [NoBP] for an item. There are two tag options to teach new blueprints:
// !learn will learn one item and then remove the tag so that the assembler is part of the autocrafting again.
// !learnMany will learn everything you queue in it and will never be part of the autorafting again until you remove the tag.
// To learn an item, queue it up about 100 times (Shift+Klick) and wait until the script removes it from the queue.
        private readonly string learnKeyword = "!learn";
        private readonly string learnManyKeyword = "!learnMany";

        private readonly string[] Ǉ =
        {
            "showHeading=true", "showWarnings=true", "showContainerStats=true", "showManagedBlocks=true",
            "showLastAction=true", "scrollTextIfNeeded=true"
        };

// Keyword a block name has to contain to be skipped by the sorting (= no items will be taken out).
// This list is expandable - just separate the entries with a ",". But it's also language specific, so adjust it if needed.
// Default: string[] lockedContainerKeywords = { "Locked", "Seat", "Control Station" };
        private readonly string[] lockedContainerKeywords = {"Locked", "Seat", "Control Station"};

        private readonly string[] ɱ = {"showHeading=true", "scrollTextIfNeeded=true"};
        private string ɯ = "";
        private int ɰ;

// Internal sorting can also be set per inventory. Just use '(sort:PATTERN)' in the block's name.
// Example: Small Cargo Container 3 (sort:Ad)
// Note: Using this method, internal sorting will always be activated for this container, even if the main switch is turned off!


// --- LCD panels ---
// =======================================================================================

// To display the main script informations, add the following keyword to any LCD name (default: !IIM-main).
// You can enable or disable specific informations on the LCD by editing its custom data.
        private readonly string mainLCDKeyword = "!IIM-main";

// If you want to use a machine manually, append the keyword to it.
// This works for assemblers, refineries, reactors and O2/H2 generators
        private readonly string manualMachineKeyword = "!manual";
        private readonly double maxCurrentMs = 0.5;
        private int _containerTypeProcessingStep;
        private string Ņ;
        private bool ɲ;
        private readonly SortedSet<string> Ƞ = new SortedSet<string>();
        private int ɳ;
        private double Ǌ, ǒ, ǐ, Ǐ, ǎ;

// Keyword for connectors to disable IIM completely for a ship, that is docked to that connector.
        private readonly string noIIMKeyword = "[No IIM]";

// Keyword for connectors to disable sorting of a grid, that is docked to that connector.
// This also disables the usage of refineries, arc furnaces and assemblers on that grid.
// Special containers, reactors and O2/H2 generators will still be filled.
        private readonly string noSortingKeyword = "[No Sorting]";
        private readonly string ǫ = "[PROTECTED] ";
        private string Ǫ = "";

        private readonly List<IMyReactor> ɵ = new List<IMyReactor>();

        private readonly Dictionary<MyDefinitionId, double> ǿ = new Dictionary<MyDefinitionId, double>();

        private bool ǭ;
        private readonly string Ǭ = "station_mode;\n";

        private readonly char[] ȍ = {'=', '>', '<'};
        private IMyAssembler Ȍ;

        private List<MyItemType> ȏ = new List<MyItemType>
            ();

        private readonly string Ȏ =
            "Remove a line to show this item on the LCD again!\nAdd an amount to manage the item without being on the LCD.\nExample: '-SteelPlate=1000'";

        private readonly List<
            IMyGasTank> ɶ = new List<IMyGasTank>();

        private readonly List<IMyGasGenerator> ɷ = new List<IMyGasGenerator>();

// Assign ores and ingots containers as one?
        private bool oresIngotsInOne;

        private readonly List<IMyAssembler> ɸ = new List<IMyAssembler>();

// To display the script performance (PB terminal output), add the following keyword to any LCD name (default: !IIM-performance).
        private readonly string performanceLCDKeyword = "!IIM-performance";

// Protect type containers when docking to another grid running the script?
        private bool protectTypeContainers = true;

        private readonly List<IMyShipConnector> ʀ = new List<IMyShipConnector>();

        private List<
            IMyTerminalBlock> ɍ = new List<IMyTerminalBlock>();

        private readonly Dictionary<string, string> Ɍ = new Dictionary<
            string, string>
        {
            {"oreContainer", oreContainerKeyword}, {"ingotContainer", ingotContainerKeyword},
            {
                "componentContainer",
                componentContainerKeyword
            },
            {"toolContainer", toolContainerKeyword}, {"ammoContainer", ammoContainerKeyword},
            {
                "bottleContainer",
                bottleContainerKeyword
            },
            {"specialContainer", specialContainerKeyword}, {"oreBalancing", "true"}, {"iceBalancing", "true"},
            {
                "uraniumBalancing",
                "true"
            }
        };

        private readonly List<IMyRefinery> ɽ = new List<IMyRefinery>();

        private readonly Dictionary<MyDefinitionId, double> ȑ = new Dictionary<MyDefinitionId, double>();
        private readonly SortedSet<string> ȓ = new SortedSet<string>();

        private readonly SortedSet<string> Ȓ = new
            SortedSet<string>();

        private readonly List<IMyAssembler> ɹ = new
            List<IMyAssembler>();

        private readonly List<IMyAssembler> ɺ = new List<IMyAssembler>();
        private readonly List<IMyTextPanel> ɻ = new List<IMyTextPanel>();

        private readonly List<IMyAssembler> ɼ = new List<IMyAssembler>();

        private readonly List<IMyRefinery> ɾ = new List<IMyRefinery>();

        private readonly List<IMyRefinery> ɿ = new List<
            IMyRefinery>();

        private readonly List<
            IMyTerminalBlock> ʁ = new List<IMyTerminalBlock>();

        private readonly List<IMyTerminalBlock> ʂ = new List<IMyTerminalBlock>();

        private readonly SortedSet<string> ș = new
            SortedSet<string>();

        private readonly List<IMyRefinery> ʃ = new List<
            IMyRefinery>();

        private int ʆ;

        private readonly List<
            string> ʅ = new List<string>();

// Script mode: "ship", "station" or blank for autodetect
        private readonly string scriptMode = "";

// Show available modifiers help on the last screen?
        private readonly bool showAutocraftingModifiers = true;

// Show a fill level in the container's name?
        private bool showFillLevel;

// Tag inventories, that have no access to the main type containers with [No Conveyor]?
// This only works if the above setting connectionCheck is set to true!
        private bool showNoConveyorTag = true;

// Sort the assembler queue based on the most needed components?
        private bool sortAssemblerQueue = true;

// Internal sorting pattern. Always combine one of each category, e.g.: 'Ad' for descending item amount (from highest to lowest)
// 1. Quantifier:
// A = amount
// N = name
// T = type (alphabetical)
// X = type (number of items)

// 2. Direction:
// a = ascending
// d = descending

        private readonly string sortingPattern = "Na";

// Sort the refinery queue based on the most needed ingots?
        private bool sortRefiningQueue = true;
        private bool ʈ;

        private readonly SortedSet<string> ț = new
            SortedSet<string>();

        private readonly SortedSet<string> Ț = new SortedSet<string>();
        private int ʇ;

// Assign tool, ammo and bottle containers as one?
        private bool toolsAmmoBottlesInOne;
        private IMyTerminalBlock ʉ;

        private readonly SortedSet<string> ȕ = new
            SortedSet<string>();

        private readonly SortedSet<string> Ȕ = new SortedSet<string>();

        private readonly SortedSet<string> ȗ = new SortedSet<string>();
        private readonly SortedSet<string> Ȗ = new SortedSet<string>();
        private IMyInventory ʊ;

// Unassign empty type containers that aren't needed anymore (at least one of each type always remains).
// This doesn't touch containers with manual priority tokens, like [P1].
        private bool unassignEmptyContainers;

// Amount of uranium in each reactor? (default: 100 for large grid reactors, 25 for small grid reactors)
        private readonly double uraniumAmountLargeGrid = 100;
        private readonly double uraniumAmountSmallGrid = 25;

// Use dynamic script speed? The script will slow down automatically if the current runtime exceeds a set value (default: 0.5ms)
        private bool useDynamicScriptSpeed = true;
        private IMyTerminalBlock ʋ;

        private string ʌ = "";

        private readonly HashSet<IMyCubeGrid> ʍ = new HashSet<IMyCubeGrid>();

// To display all current warnings and problems, add the following keyword to any LCD name (default: IIM-warnings).
        private readonly string warningsLCDKeyword = "!IIM-warnings";

        private readonly List<IMyTerminalBlock> _listOfBlocks1 = new List<IMyTerminalBlock>();
        private readonly List<IMyTerminalBlock> ɏ = new List<IMyTerminalBlock>();
        private readonly string Ɏ = "itemID;blueprintID";

        private string[] ʎ =
        {
            oreContainerKeyword, ingotContainerKeyword, componentContainerKeyword, toolContainerKeyword,
            ammoContainerKeyword,
            bottleContainerKeyword
        };

        private readonly List<IMyTerminalBlock> ʐ = new List<
            IMyTerminalBlock>();

        private readonly List<IMyTerminalBlock> ʑ = new List<IMyTerminalBlock>();

        private readonly List<
            IMyTerminalBlock> ʒ = new List<IMyTerminalBlock>();

        private readonly Dictionary
            <string, MyDefinitionId> ǯ = new Dictionary<string, MyDefinitionId>();

        private readonly Dictionary<string, string> Ǯ = new Dictionary<string, string>
            ();

        private StringBuilder ƹ
            = new StringBuilder("No performance Information available!");

        private readonly Dictionary<string, int> Ƹ = new Dictionary<string, int>();
        private readonly List<IMyTerminalBlock> ʓ = new List<IMyTerminalBlock>();

        private readonly List<int
        > ƾ = new List<int>(new int[600]);

        private readonly List<IMyTerminalBlock> ʔ = new List<IMyTerminalBlock>();
        private readonly List<IMyTerminalBlock> ʕ = new List<IMyTerminalBlock>();
        private readonly HashSet<IMyCubeGrid> ʖ = new HashSet<IMyCubeGrid>();

        private Program()
        {
            Echo("Script ready to be launched..\n");
            assembleMargin /=
                100;
            disassembleMargin /= 100;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private void Main(string ǳ)
        {
            if (ɰ >= 10)
            {
                throw new
                    Exception("Too many errors in script step " + _scriptStep + ":\n" + Ǧ[_scriptStep] +
                              "\n\nPlease recompile!\nScript stoppped!\n\nLast error:\n" + ɯ + "\n");
            }

            try
            {
                if (ɦ)
                {
                    if (_scriptStep > 0)
                    {
                        Echo("Initializing script.. (" + (_scriptStep + 1) + "/10) \n");
                    }

                    if (_scriptStep >= 2)
                    {
                        Echo("Getting inventory blocks..");
                        if (_scriptStep == 2)
                        {
                            ɋ();
                        }

                        if (ʈ)
                        {
                            return;
                        }
                    }

                    if (_scriptStep >= 3)
                    {
                        Echo("Loading saved items..");
                        if (_scriptStep == 3)
                        {
                            if (!Ð())
                            {
                                ɮ = true;
                                enableAutocrafting = false;
                                enableAutodisassembling = false
                                    ;
                            }
                        }

                        if (ɮ)
                        {
                            Echo("-> No assemblers found!");
                            Echo("-> Autocrafting deactivated!");
                        }
                    }

                    if (_scriptStep >= 4)
                    {
                        Echo(
                            "Clearing assembler queues..");
                        if (_scriptStep == 4 && (enableAutocrafting || enableAutodisassembling))
                        {
                            GridTerminalSystem.GetBlocksOfType(ɻ,
                                q => q.IsSameConstructAs(Me) && q.CustomName.Contains(autocraftingKeyword));
                            if (ɻ.Count > 0)
                            {
                                foreach (var ƞ in ɼ)
                                {
                                    ƞ.Mode = MyAssemblerMode.Disassembly;
                                    ƞ
                                        .ClearQueue();
                                    ƞ.Mode = MyAssemblerMode.Assembly;
                                    ƞ.ClearQueue();
                                }
                            }
                        }
                    }

                    if (_scriptStep >= 5)
                    {
                        Echo("Checking blueprints..");
                        if (_scriptStep == 5)
                        {
                            foreach (
                                var K in Ȝ)
                            {
                                ƭ(K);
                            }
                        }
                    }

                    if (_scriptStep >= 6)
                    {
                        Echo("Checking type containers..");
                        if (_scriptStep == 6)
                        {
                            Ȯ();
                        }

                        if (_scriptStep == 6)
                        {
                            Ⱥ();
                        }
                    }

                    if (_scriptStep >= 7)
                    {
                        if (scriptMode == "station")
                        {
                            ǭ = true;
                        }
                        else if (Me.CubeGrid.IsStatic && scriptMode != "ship")
                        {
                            ǭ = true;
                        }

                        Echo("Setting script mode to: " + (ǭ ? "station.." : "ship.."));
                        if (_scriptStep == 7)
                        {
                            Me.CustomData = (ǭ ? Ǭ : ǲ) + Me.CustomData.Replace(Ǭ, "").Replace(ǲ, "");
                        }
                    }

                    if (_scriptStep >= 8)
                    {
                        Echo("Starting script..");
                    }

                    if (_scriptStep >= 9)
                    {
                        _scriptStep = 0;
                        ɦ = false;
                        return;
                    }

                    _scriptStep++;
                    return;
                }

                if (ǳ != "")
                {
                    Ǩ = ǳ;
                    _scriptStep = 1;
                    ǩ = "";
                    ǧ = DateTime.Now;
                }

                if (useDynamicScriptSpeed)
                {
                    if (ɩ > 0)
                    {
                        ǋ(
                            "Dynamic script speed control");
                        Î("..");
                        ɩ--;
                        return;
                    }
                }

                if (ɨ < extraScriptTicks)
                {
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    ɨ++;
                    return;
                }

                if (ɥ)
                {
                    if (ɤ == 0)
                    {
                        Ń();
                    }

                    if (ɤ == 1)
                    {
                        Ŀ();
                    }

                    if (ɤ == 2)
                    {
                        ņ();
                    }

                    if (ɤ == 3)
                    {
                        ľ();
                    }

                    if (ɤ > 3)
                    {
                        ɤ = 0;
                    }

                    ɥ = false;
                    return;
                }

                if (_scriptStep == 0 || ɭ || ʈ)
                {
                    if (!ɲ)
                    {
                        ɋ();
                    }

                    if (ʈ)
                    {
                        return;
                    }

                    ɭ = false;
                    ɲ = false;
                    if (!ǉ(30))
                    {
                        ɔ = ǈ(mainLCDKeyword, Ǉ);
                        ɓ = ǈ(warningsLCDKeyword, ɱ);
                        ə = ǈ(performanceLCDKeyword, ɱ);
                        ɣ = ǈ(inventoryLCDKeyword);
                    }
                    else
                    {
                        ɭ =
                            true;
                        ɲ = true;
                    }

                    if (_scriptStep == 0)
                    {
                        ǋ(Ǧ[_scriptStep]);
                        Î();
                        _scriptStep++;
                    }

                    return;
                }

                if (!ǭ)
                {
                    Ǹ();
                }

                if (ǽ(Ǩ))
                {
                    return;
                }

                ɨ = 0;
                Runtime.UpdateFrequency = UpdateFrequency.Update10
                    ;
                ɥ = true;
                if (_scriptStep == 1)
                {
                    Ò();
                }

                if (_scriptStep == 2)
                {
                    è();
                }

                if (_scriptStep == 3)
                {
                    if (enableNameCorrection)
                    {
                        ȸ();
                    }
                }

                if (_scriptStep == 4)
                {
                    if (autoContainerAssignment)
                    {
                        if (
                            unassignEmptyContainers)
                        {
                            ȴ();
                        }

                        if (assignNewContainers)
                        {
                            ȥ();
                        }
                    }
                }

                if (_scriptStep == 5)
                {
                    if (ʕ.Count != 0)
                    {
                        ͼ();
                    }
                }

                if (_scriptStep == 6)
                {
                    ProcessContainerTypes();
                }

                if (_scriptStep == 7)
                {
                    if (balanceTypeContainers)
                    {
                        ʪ();
                    }
                }

                if (_scriptStep
                    == 8)
                {
                    Ε();
                }

                if (_scriptStep == 9)
                {
                    β(ʄ);
                    β(ʕ);
                }

                if (_scriptStep == 10)
                {
                    Ʋ();
                }

                if (_scriptStep == 11)
                {
                    if (enableAutocrafting || enableAutodisassembling)
                    {
                        ƚ();
                    }
                }

                if (_scriptStep == 12)
                {
                    if (
                        enableAutocrafting || enableAutodisassembling)
                    {
                        Ϋ();
                    }
                }

                if (_scriptStep == 13)
                {
                    if (sortAssemblerQueue)
                    {
                        ˮ();
                    }
                }

                if (_scriptStep == 14)
                {
                    if (enableAssemblerCleanup)
                    {
                        ˏ();
                    }

                    if (
                        enableBasicIngotCrafting)
                    {
                        if (ɿ.Count > 0)
                        {
                            enableBasicIngotCrafting = false;
                        }
                        else
                        {
                            ʲ();
                        }
                    }
                }

                if (_scriptStep == 15)
                {
                    Õ();
                }

                if (_scriptStep == 16)
                {
                    ʢ();
                }

                if (_scriptStep == 17)
                {
                    if (
                        enableOreBalancing)
                    {
                        ʠ();
                    }

                    if (sortRefiningQueue)
                    {
                        ʟ(ɽ, ȏ);
                        ʟ(ʃ, Ȉ);
                    }
                }

                if (_scriptStep == 18)
                {
                    if (enableIceBalancing)
                    {
                        ĕ();
                    }
                }

                if (_scriptStep == 19)
                {
                    if (enableUraniumBalancing)
                    {
                        ß(
                            "uraniumBalancing", "true");
                        ę();
                    }
                    else if (!enableUraniumBalancing && á("uraniumBalancing") == "true")
                    {
                        ß("uraniumBalancing", "false");
                        foreach (
                            var Ċ in ɵ)
                        {
                            Ċ.UseConveyorSystem = true;
                        }
                    }
                }

                ǋ(Ǧ[_scriptStep]);
                Î();
                ɩ = (int) Math.Floor((Ǌ > 20 ? 20 : Ǌ) / maxCurrentMs);
                if (_scriptStep >= 19)
                {
                    _scriptStep = 0;
                    ɬ = new HashSet
                        <string>(ɪ);
                    ɪ.Clear();
                    if (ɰ > 0)
                    {
                        ɰ--;
                    }

                    if (ɬ.Count == 0)
                    {
                        Ņ = null;
                    }
                }
                else
                {
                    _scriptStep++;
                }
            }
            catch (NullReferenceException e)
            {
                ɰ++;
                ɭ = true;
                ʈ = false;
                ɯ = e.ToString();
                Log("Execution of script step aborted:\n" + Ǧ[_scriptStep] + " (ID: " + _scriptStep + ")\n\nCached block not available..");
            }
            catch (Exception e)
            {
                ɰ++;
                ɭ = true;
                ʈ = false;
                ɯ = e.ToString();
                Log("Critical error in script step:\n" + Ǧ[_scriptStep] + " (ID: " + _scriptStep + ")\n\n" + e);
            }
        }

        private bool ǽ(string ǳ)
        {
            if (ǳ.Contains("pauseThisPB"))
            {
                Echo("Script execution paused!\n");
                var Ǽ = ǳ.Split(';');
                if (Ǽ.Length == 3)
                {
                    Echo("Found:");
                    Echo("'" + Ǽ[1] + "'")
                        ;
                    Echo("on grid:");
                    Echo("'" + Ǽ[2] + "'");
                    Echo("also running the script.\n");
                    Echo("Type container protection: " + (
                        protectTypeContainers ? "ON" : "OFF") + "\n");
                    Echo("Everything else is managed by the other script.");
                }

                return true;
            }

            var ǻ = true;
            var Ǿ = true;
            var Ǻ =
                false;
            if (ǳ != "reset" && ǳ != "msg")
            {
                if (!ǳ.Contains(" on") && !ǳ.Contains(" off") && !ǳ.Contains(" toggle"))
                {
                    return false;
                }

                if (ǳ.Contains(
                    " off"))
                {
                    Ǿ = false;
                }

                if (ǳ.Contains(" toggle"))
                {
                    Ǻ = true;
                }
            }

            if (ǳ == "reset")
            {
                Ƴ();
                return true;
            }

            if (ǳ == "msg")
            {
            }
            else if (ǳ.StartsWith(
                "balanceTypeContainers"))
            {
                Ǫ = "Balance type containers";
                if (Ǻ)
                {
                    Ǿ = !balanceTypeContainers;
                }

                balanceTypeContainers = Ǿ;
            }
            else if (ǳ.StartsWith(
                "showFillLevel"))
            {
                Ǫ = "Show fill level";
                if (Ǻ)
                {
                    Ǿ = !showFillLevel;
                }

                showFillLevel = Ǿ;
            }
            else if (ǳ.StartsWith("autoContainerAssignment"))
            {
                Ǫ =
                    "Auto assign containers";
                if (Ǻ)
                {
                    Ǿ = !autoContainerAssignment;
                }

                autoContainerAssignment = Ǿ;
            }
            else if (ǳ.StartsWith("assignNewContainers"))
            {
                Ǫ =
                    "Assign new containers";
                if (Ǻ)
                {
                    Ǿ = !assignNewContainers;
                }

                assignNewContainers = Ǿ;
            }
            else if (ǳ.StartsWith("unassignEmptyContainers"))
            {
                Ǫ =
                    "Unassign empty containers";
                if (Ǻ)
                {
                    Ǿ = !unassignEmptyContainers;
                }

                unassignEmptyContainers = Ǿ;
            }
            else if (ǳ.StartsWith("oresIngotsInOne"))
            {
                Ǫ =
                    "Assign ores and ingots as one";
                if (Ǻ)
                {
                    Ǿ = !oresIngotsInOne;
                }

                oresIngotsInOne = Ǿ;
            }
            else if (ǳ.StartsWith("toolsAmmoBottlesInOne"))
            {
                Ǫ =
                    "Assign tools, ammo and bottles as one";
                if (Ǻ)
                {
                    Ǿ = !toolsAmmoBottlesInOne;
                }

                toolsAmmoBottlesInOne = Ǿ;
            }
            else if (ǳ.StartsWith("fillBottles"))
            {
                Ǫ = "Fill bottles";
                if (Ǻ)
                {
                    Ǿ = !
                        fillBottles;
                }

                fillBottles = Ǿ;
            }
            else if (ǳ.StartsWith("enableAutocrafting"))
            {
                Ǫ = "Autocrafting";
                if (Ǻ)
                {
                    Ǿ = !enableAutocrafting;
                }

                enableAutocrafting = Ǿ;
            }
            else if (ǳ.StartsWith("enableAutodisassembling"))
            {
                Ǫ = "Autodisassembling";
                if (Ǻ)
                {
                    Ǿ = !enableAutodisassembling;
                }

                enableAutodisassembling = Ǿ;
            }
            else if (ǳ.StartsWith("headerOnEveryScreen"))
            {
                Ǫ = "Show header on every autocrafting screen";
                if (Ǻ)
                {
                    Ǿ = !
                        headerOnEveryScreen;
                }

                headerOnEveryScreen = Ǿ;
            }
            else if (ǳ.StartsWith("sortAssemblerQueue"))
            {
                Ǫ = "Sort assembler queue";
                if (Ǻ)
                {
                    Ǿ = !sortAssemblerQueue;
                }

                sortAssemblerQueue = Ǿ;
            }
            else if (ǳ.StartsWith("enableBasicIngotCrafting"))
            {
                Ǫ = "Basic ingot crafting";
                if (Ǻ)
                {
                    Ǿ = !enableBasicIngotCrafting;
                }

                enableBasicIngotCrafting = Ǿ;
            }
            else if (ǳ.StartsWith("disableBasicAutocrafting"))
            {
                Ǫ = "Disable autocrafting in survival kits";
                if (Ǻ)
                {
                    Ǿ = !
                        disableBasicAutocrafting;
                }

                disableBasicAutocrafting = Ǿ;
            }
            else if (ǳ.StartsWith("allowSpecialSteal"))
            {
                Ǫ = "Allow special container steal";
                if (Ǻ)
                {
                    Ǿ = !
                        allowSpecialSteal;
                }

                allowSpecialSteal = Ǿ;
            }
            else if (ǳ.StartsWith("enableOreBalancing"))
            {
                Ǫ = "Ore balancing";
                if (Ǻ)
                {
                    Ǿ = !enableOreBalancing;
                }

                enableOreBalancing = Ǿ;
            }
            else if (ǳ.StartsWith("enableScriptRefineryFilling"))
            {
                Ǫ = "Script assisted refinery filling";
                if (Ǻ)
                {
                    Ǿ = !
                        enableScriptRefineryFilling;
                }

                enableScriptRefineryFilling = Ǿ;
            }
            else if (ǳ.StartsWith("sortRefiningQueue"))
            {
                Ǫ = "Sort refinery queue";
                if (Ǻ)
                {
                    Ǿ = !
                        sortRefiningQueue;
                }

                sortRefiningQueue = Ǿ;
            }
            else if (ǳ.StartsWith("enableIceBalancing"))
            {
                Ǫ = "Ice balancing";
                if (Ǻ)
                {
                    Ǿ = !enableIceBalancing;
                }

                enableIceBalancing = Ǿ;
            }
            else if (ǳ.StartsWith("fillOfflineGenerators"))
            {
                Ǫ = "Fill offline O2/H2 generators";
                if (Ǻ)
                {
                    Ǿ = !fillOfflineGenerators;
                }

                fillOfflineGenerators = Ǿ;
            }
            else if (ǳ.StartsWith("enableUraniumBalancing"))
            {
                Ǫ = "Uranium balancing";
                if (Ǻ)
                {
                    Ǿ = !enableUraniumBalancing;
                }

                enableUraniumBalancing = Ǿ;
            }
            else if (ǳ.StartsWith("fillOfflineReactors"))
            {
                Ǫ = "Fill offline reactors";
                if (Ǻ)
                {
                    Ǿ = !fillOfflineReactors;
                }

                fillOfflineReactors = Ǿ;
            }
            else if (ǳ.StartsWith("enableAssemblerCleanup"))
            {
                Ǫ = "Assembler cleanup";
                if (Ǻ)
                {
                    Ǿ = !enableAssemblerCleanup;
                }

                enableAssemblerCleanup = Ǿ;
            }
            else if (ǳ.StartsWith("enableInternalSorting"))
            {
                Ǫ = "Internal sorting";
                if (Ǻ)
                {
                    Ǿ = !enableInternalSorting;
                }

                enableInternalSorting = Ǿ;
            }
            else if (ǳ.StartsWith("useDynamicScriptSpeed"))
            {
                Ǫ = "Dynamic script speed";
                if (Ǻ)
                {
                    Ǿ = !useDynamicScriptSpeed;
                }

                useDynamicScriptSpeed = Ǿ;
            }
            else if (ǳ.StartsWith("excludeWelders"))
            {
                Ǫ = "Exclude welders";
                if (Ǻ)
                {
                    Ǿ = !excludeWelders;
                }

                excludeWelders = Ǿ;
            }
            else if (ǳ.StartsWith("excludeGrinders"))
            {
                Ǫ = "Exclude grinders";
                if (Ǻ)
                {
                    Ǿ = !excludeGrinders;
                }

                excludeGrinders = Ǿ;
            }
            else if (ǳ.StartsWith(
                "connectionCheck"))
            {
                Ǫ = "Connection check";
                if (Ǻ)
                {
                    Ǿ = !connectionCheck;
                }

                connectionCheck = Ǿ;
                Ⱥ();
            }
            else if (ǳ.StartsWith("showNoConveyorTag"))
            {
                Ǫ =
                    "Show no conveyor access";
                if (Ǻ)
                {
                    Ǿ = !showNoConveyorTag;
                }

                showNoConveyorTag = Ǿ;
                Ⱥ();
            }
            else if (ǳ.StartsWith("protectTypeContainers"))
            {
                Ǫ =
                    "Protect type containers";
                if (Ǻ)
                {
                    Ǿ = !protectTypeContainers;
                }

                protectTypeContainers = Ǿ;
            }
            else if (ǳ.StartsWith("enableNameCorrection"))
            {
                Ǫ =
                    "Name correction";
                if (Ǻ)
                {
                    Ǿ = !enableNameCorrection;
                }

                enableNameCorrection = Ǿ;
            }
            else
            {
                ǻ = false;
            }

            if (ǻ)
            {
                var ǹ = DateTime.Now - ǧ;
                if (ǩ == "")
                {
                    ǩ = Ǫ +
                        " temporarily " + (Ǿ ? "enabled" : "disabled") + "!\n";
                }

                Echo(ǩ);
                Echo("Continuing in " + Math.Ceiling(3 - ǹ.TotalSeconds) + " seconds..");
                Ǩ = "msg";
                if (ǹ.TotalSeconds >= 3)
                {
                    Ǫ = "";
                    ǩ = "";
                    Ǩ = "";
                }
            }

            return ǻ;
        }

        private void Ǹ()
        {
            var Ƿ = new List<IMyProgrammableBlock>();
            GridTerminalSystem
                .GetBlocksOfType(Ƿ, Ƕ => Ƕ != Me);
            if (Ǩ.StartsWith("pauseThisPB") || Ǩ == "")
            {
                Ǩ = "";
                foreach (var ǵ in Ƿ)
                {
                    if (ǵ.CustomData.Contains(Ǭ)
                        || ǵ.CustomData.Contains(ǲ) && GetBlockPriorityFromName(ǵ) < GetBlockPriorityFromName(Me))
                    {
                        Ǩ = "pauseThisPB;" + ǵ.CustomName + ";" + ǵ.CubeGrid.CustomName;
                        foreach (var X in ʄ)
                        {
                            if (
                                protectTypeContainers && !X.CustomName.Contains(ǫ) && X.IsSameConstructAs(Me))
                            {
                                X.CustomName = ǫ + X.CustomName;
                            }
                        }

                        return;
                    }
                }

                if (Ǩ == "")
                {
                    foreach (var X in ʂ)
                    {
                        X.CustomName = X.CustomName.Replace(ǫ, "");
                    }
                }
            }
        }

        private void Ȣ()
        {
            ʍ.Clear();
            GridTerminalSystem.GetBlocksOfType(ʀ, ŭ
                => ŭ.CustomName.Contains(noSortingKeyword));
            foreach (var Ʉ in ʀ)
            {
                if (Ʉ.Status != MyShipConnectorStatus.Connected)
                {
                    continue;
                }

                if (Ʉ.OtherConnector.CubeGrid.IsSameConstructAs(Me.CubeGrid))
                {
                    continue;
                }

                ʍ.Add(Ʉ.OtherConnector.CubeGrid);
            }

            ʖ.Clear();
            GridTerminalSystem.GetBlocksOfType(ʀ, ŭ => ŭ.CustomName.Contains(noIIMKeyword));
            foreach (var Ʉ in ʀ)
            {
                if (Ʉ.Status != MyShipConnectorStatus.Connected)
                {
                    continue;
                }

                if (Ʉ.OtherConnector.CubeGrid.IsSameConstructAs(Me.CubeGrid))
                {
                    continue;
                }

                ʖ.Add(Ʉ.OtherConnector.CubeGrid);
            }
        }

        private void Ƀ
            ()
        {
            if (ʊ != null)
            {
                try
                {
                    ʊ = ʋ.GetInventory(0);
                }
                catch
                {
                    ʊ = null;
                }
            }

            if (ʊ == null)
            {
                try
                {
                    foreach (var X in ʄ)
                    {
                        foreach (var e in ɏ)
                        {
                            if (X == e)
                            {
                                continue;
                            }

                            if (X.GetInventory(0).IsConnectedTo(e.GetInventory(0)))
                            {
                                ʋ = ʄ[0];
                                ʊ = ʋ.GetInventory(0);
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    ʊ = null;
                }
            }
        }

        private void ɂ(
            IMyTerminalBlock e)
        {
            foreach (var È in ʖ)
            {
                if (e.CubeGrid.IsSameConstructAs(È))
                {
                    return;
                }
            }

            if (e.BlockDefinition.SubtypeId.Contains("Locker") || e.BlockDefinition.SubtypeId == "VendingMachine" ||
                e.BlockDefinition.TypeIdString.Contains("Parachute"))
            {
                return;
            }

            if (e is IMyShipWelder &&
                excludeWelders)
            {
                return;
            }

            if (e is IMyShipGrinder && excludeGrinders)
            {
                return;
            }

            var ȳ = e.CustomName;
            if (ȳ.Contains(ǫ))
            {
                ʂ.Add(e);
                return;
            }

            bool ɀ = ȳ
                    .Contains(specialContainerKeyword),
                ȿ = false,
                Ⱦ = ȳ.Contains(manualMachineKeyword),
                Ƚ = false,
                ȼ = ȳ.Contains(learnKeyword) || ȳ.Contains(learnManyKeyword),
                Ɂ = true,
                Ʌ = false;
            foreach (var ê in lockedContainerKeywords)
            {
                if (ȳ.Contains(ê))
                {
                    ȿ = true;
                    break;
                }
            }

            foreach (var
                ê in hiddenContainerKeywords)
            {
                if (ȳ.Contains(ê))
                {
                    Ƚ = true;
                    break;
                }
            }

            if (!ɀ && !(e is IMyReactor) && !(e is IMyGasGenerator))
            {
                foreach (var È in ʍ)
                {
                    if (e.CubeGrid.IsSameConstructAs(È))
                    {
                        return;
                    }
                }
            }

            if (!Ƚ)
            {
                ɏ.Add(e);
            }

            if (connectionCheck)
            {
                if (ʊ != null)
                {
                    if (!e.GetInventory(0).IsConnectedTo(ʊ))
                    {
                        Ɂ = false;
                    }
                }

                if (!Ɂ)
                {
                    if (showNoConveyorTag)
                    {
                        ɇ(e, "[No Conveyor]");
                    }

                    return;
                }

                ɇ(e, "[No Conveyor]", false)
                    ;
            }

            if (ȳ.Contains(oreContainerKeyword))
            {
                ʔ.Add(e);
                Ʌ = true;
            }

            if (ȳ.Contains(ingotContainerKeyword))
            {
                ʓ.Add(e);
                Ʌ = true;
            }

            if (ȳ.Contains(componentContainerKeyword))
            {
                ʒ.Add(e);
                Ʌ = true;
            }

            if (ȳ.Contains(toolContainerKeyword))
            {
                ʑ.Add(e);
                Ʌ = true;
            }

            if (ȳ.Contains(
                ammoContainerKeyword))
            {
                ʐ.Add(e);
                Ʌ = true;
            }

            if (ȳ.Contains(bottleContainerKeyword))
            {
                _listOfBlocks1.Add(e);
                Ʌ = true;
            }

            if (ɀ)
            {
                ʕ.Add(e);
                if (e.CustomData.Length < 200)
                {
                    í(
                        e);
                }
            }

            if (Ʌ)
            {
                ʄ.Add(e);
            }

            if (e.GetType().ToString().Contains("Weapon"))
            {
                return;
            }

            if (e is IMyRefinery)
            {
                if (e.IsSameConstructAs(Me) && !ɀ
                                            && !Ⱦ && e.IsWorking)
                {
                    (e as IMyRefinery).UseConveyorSystem = true;
                    ɿ.Add(e as IMyRefinery);
                    if (e.BlockDefinition.SubtypeId ==
                        "Blast Furnace")
                    {
                        ʃ.Add(e as IMyRefinery);
                    }
                    else
                    {
                        ɽ.Add(e as IMyRefinery);
                    }
                }

                if (!ȿ && e.GetInventory(1).ItemCount > 0)
                {
                    ɾ.Add(e as IMyRefinery);
                }
            }
            else if (e is IMyAssembler)
            {
                if (e.IsSameConstructAs(Me) && !Ⱦ && !ȼ && e.IsWorking)
                {
                    ɼ.Add(e as IMyAssembler);
                    if (e.BlockDefinition.SubtypeId.Contains("Survival"))
                    {
                        ɸ.Add(e as IMyAssembler);
                    }
                }

                if (!ȿ && !ȼ && e.GetInventory(1).ItemCount > 0)
                {
                    ɺ.Add(e as IMyAssembler);
                }

                if (ȼ)
                {
                    ɹ
                        .Add(e as IMyAssembler);
                }
            }
            else if (e is IMyGasGenerator)
            {
                if (!ɀ && !Ⱦ && e.IsFunctional)
                {
                    if (fillOfflineGenerators && !(e as
                        IMyGasGenerator).Enabled)
                    {
                        ɷ.Add(e as IMyGasGenerator);
                    }
                    else if ((e as IMyGasGenerator).Enabled)
                    {
                        ɷ.Add(e as IMyGasGenerator);
                    }
                }
            }
            else if (e
                is IMyGasTank)
            {
                if (!ɀ && !Ⱦ && !ȿ && e.IsWorking && e.IsSameConstructAs(Me))
                {
                    ɶ.Add(e as IMyGasTank);
                }
            }
            else if (e is IMyReactor)
            {
                if (!
                    ɀ && !Ⱦ && e.IsFunctional)
                {
                    if (fillOfflineReactors && !(e as IMyReactor).Enabled)
                    {
                        ɵ.Add(e as IMyReactor);
                    }
                    else if ((e as
                        IMyReactor).Enabled)
                    {
                        ɵ.Add(e as IMyReactor);
                    }
                }
            }
            else if (e is IMyCargoContainer)
            {
                if (e.IsSameConstructAs(Me) && !Ʌ && !ȿ && !ɀ)
                {
                    ʁ.Add(e);
                }
            }

            if
                (e.InventoryCount == 1 && !ɀ && !ȿ && !(e is IMyReactor))
            {
                if (e.GetInventory(0).ItemCount > 0)
                {
                    ɑ.Add(e);
                }

                if (!e.BlockDefinition.TypeIdString.Contains("Oxygen"))
                {
                    if (e.IsSameConstructAs(Me))
                    {
                        ɢ.Insert(0, e);
                    }
                    else
                    {
                        ɢ.Add(e);
                    }
                }
            }
        }

        private void ɋ()
        {
            if (!ʈ)
            {
                Ȣ();
                if (connectionCheck
                )
                {
                    Ƀ();
                }

                try
                {
                    for (var E = 0; E < ʕ.Count; E++)
                    {
                        if (!ʕ[E].CustomName.Contains(specialContainerKeyword))
                        {
                            ʕ[E].CustomData = "";
                        }
                    }
                }
                catch
                {
                }

                ʄ.Clear();
                ʔ.Clear();
                ʓ.Clear();
                ʒ.Clear();
                ʑ.Clear();
                ʐ.Clear();
                _listOfBlocks1.Clear();
                ʕ.Clear();
                ʁ.Clear();
                ʂ.Clear();
                ɏ.Clear();
                ɑ.Clear();
                ɢ.Clear
                    ();
                ɿ.Clear();
                ɽ.Clear();
                ʃ.Clear();
                ɾ.Clear();
                ɼ.Clear();
                ɸ.Clear();
                ɺ.Clear();
                ɹ.Clear();
                ɷ.Clear();
                ɶ.Clear();
                ɵ.Clear();
                ʇ = 0;
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(ɐ, ũ => ũ.HasInventory);
            }

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            for (var E = ʇ;
                E < ɐ
                    .Count;
                E++)
            {
                ɂ(ɐ[E]);
                ʇ++;
                if (E % 200 == 0)
                {
                    ʈ = true;
                    return;
                }
            }

            if (ʆ == 0)
            {
                Ɋ(ʔ);
            }

            if (ʆ == 1)
            {
                Ɋ(ʓ);
            }

            if (ʆ == 2)
            {
                Ɋ(ʒ);
            }

            if (ʆ == 3)
            {
                Ɋ(ʑ);
            }

            if (ʆ == 4)
            {
                Ɋ(ʐ);
            }

            if (
                ʆ == 5)
            {
                Ɋ(ʕ);
            }

            if (ʆ == 6)
            {
                Ɋ(_listOfBlocks1);
            }

            ʆ++;
            if (ʆ > 6)
            {
                ʆ = 0;
            }
            else
            {
                ʈ = true;
                return;
            }

            if (disableBasicAutocrafting && ɼ.Count != ɸ.Count)
            {
                ɼ.RemoveAll(Ĩ =>
                    Ĩ.BlockDefinition.SubtypeId.Contains("Survival"));
            }

            if (fillBottles)
            {
                ɑ.Sort((Ɉ, ũ) => ũ.BlockDefinition.TypeIdString.Contains(
                    "Oxygen").CompareTo(Ɉ.BlockDefinition.TypeIdString.Contains("Oxygen")));
            }

            ʈ = false;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private void Ɋ(List<IMyTerminalBlock> ɉ)
        {
            if (ɉ.Count >= 2 && ɉ.Count <= 500)
            {
                ɉ.Sort((Ɉ, ũ) => GetBlockPriorityFromName(Ɉ).CompareTo(GetBlockPriorityFromName(ũ)));
            }

            if (!ǉ())
            {
                ʆ++;
            }
        }

        private void ɇ(
            IMyTerminalBlock e, string Ɇ, bool Ȼ = true)
        {
            if (Ȼ)
            {
                if (e.CustomName.Contains(Ɇ))
                {
                    return;
                }

                e.CustomName += " " + Ɇ;
            }
            else
            {
                if (!e.CustomName.Contains(Ɇ)
                )
                {
                    return;
                }

                e.CustomName = e.CustomName.Replace(" " + Ɇ, "").Replace(Ɇ, "").TrimEnd(' ');
            }
        }

        private void Ⱥ()
        {
            for (var E = 0; E < ɏ.Count; E++)
            {
                ɇ(ɏ[
                    E], "[No Conveyor]", false);
            }
        }

        private void Ȯ()
        {
            var ȭ = false;
            var Ȭ = á("oreContainer");
            var ȫ = á("ingotContainer");
            var Ȫ = á(
                "componentContainer");
            var ȩ = á("toolContainer");
            var ȯ = á("ammoContainer");
            var Ȩ = á("bottleContainer");
            var Ȧ = á("specialContainer");
            if (oreContainerKeyword != Ȭ)
            {
                ȭ = true;
            }
            else if (ingotContainerKeyword != ȫ)
            {
                ȭ = true;
            }
            else if (componentContainerKeyword != Ȫ)
            {
                ȭ = true;
            }
            else if (toolContainerKeyword != ȩ)
            {
                ȭ = true;
            }
            else if (ammoContainerKeyword != ȯ)
            {
                ȭ = true;
            }
            else if (bottleContainerKeyword != Ȩ)
            {
                ȭ =
                    true;
            }
            else if (specialContainerKeyword != Ȧ)
            {
                ȭ = true;
            }

            if (ȭ)
            {
                for (var E = 0; E < ɏ.Count; E++)
                {
                    if (ɏ[E].CustomName.Contains(Ȭ))
                    {
                        ɏ[E].CustomName = ɏ[E].CustomName.Replace(Ȭ, oreContainerKeyword);
                    }

                    if (ɏ[E].CustomName.Contains(ȫ))
                    {
                        ɏ[E].CustomName = ɏ[E].CustomName.Replace
                            (ȫ, ingotContainerKeyword);
                    }

                    if (ɏ[E].CustomName.Contains(Ȫ))
                    {
                        ɏ[E].CustomName = ɏ[E].CustomName.Replace(Ȫ,
                            componentContainerKeyword);
                    }

                    if (ɏ[E].CustomName.Contains(ȩ))
                    {
                        ɏ[E].CustomName = ɏ[E].CustomName.Replace(ȩ, toolContainerKeyword);
                    }

                    if (ɏ[E].CustomName.Contains(ȯ))
                    {
                        ɏ[E].CustomName = ɏ[E].CustomName.Replace(ȯ, ammoContainerKeyword);
                    }

                    if (ɏ[E].CustomName.Contains(Ȩ))
                    {
                        ɏ[E].CustomName = ɏ[
                            E].CustomName.Replace(Ȩ, bottleContainerKeyword);
                    }

                    if (ɏ[E].CustomName.Contains(Ȧ))
                    {
                        ɏ[E].CustomName = ɏ[E].CustomName.Replace(
                            Ȧ, specialContainerKeyword);
                    }
                }

                ß("oreContainer", oreContainerKeyword);
                ß("ingotContainer", ingotContainerKeyword);
                ß(
                    "componentContainer", componentContainerKeyword);
                ß("toolContainer", toolContainerKeyword);
                ß("ammoContainer", ammoContainerKeyword);
                ß(
                    "bottleContainer", bottleContainerKeyword);
                ß("specialContainer", specialContainerKeyword);
            }
        }

        private void ȥ()
        {
            for (var E = 0; E < ʁ.Count; E++)
            {
                var Ȥ =
                    false;
                var ȣ = false;
                var ȧ = ʁ[E].CustomName;
                var Ȱ = "";
                if (ʔ.Count == 0 || ʌ == "Ore")
                {
                    if (oresIngotsInOne)
                    {
                        ȣ = true;
                    }
                    else
                    {
                        ʁ[E].CustomName += " " + oreContainerKeyword;
                        ʔ.Add(ʁ[E]);
                        Ȱ = "Ores";
                    }
                }
                else if (ʓ.Count == 0 || ʌ == "Ingot")
                {
                    if (oresIngotsInOne)
                    {
                        ȣ = true;
                    }
                    else
                    {
                        ʁ[E].CustomName += " " + ingotContainerKeyword;
                        ʓ.Add(ʁ[E]);
                        Ȱ = "Ingots";
                    }
                }
                else if (ʒ.Count == 0 || ʌ == "Component")
                {
                    ʁ[E].CustomName += " " +
                                       componentContainerKeyword;
                    ʒ.Add(ʁ[E]);
                    Ȱ = "Components";
                }
                else if (ʑ.Count == 0 || ʌ == "PhysicalGunObject")
                {
                    if (toolsAmmoBottlesInOne)
                    {
                        Ȥ = true;
                    }
                    else
                    {
                        ʁ[E].CustomName += " " + toolContainerKeyword;
                        ʑ.Add(ʁ[E]);
                        Ȱ = "Tools";
                    }
                }
                else if (ʐ.Count == 0 || ʌ == "AmmoMagazine")
                {
                    if (toolsAmmoBottlesInOne)
                    {
                        Ȥ =
                            true;
                    }
                    else
                    {
                        ʁ[E].CustomName += " " + ammoContainerKeyword;
                        ʐ.Add(ʁ[E]);
                        Ȱ = "Ammo";
                    }
                }
                else if (_listOfBlocks1.Count == 0 || ʌ == "OxygenContainerObject" ||
                         ʌ == "GasContainerObject")
                {
                    if (toolsAmmoBottlesInOne)
                    {
                        Ȥ = true;
                    }
                    else
                    {
                        ʁ[E].CustomName += " " + bottleContainerKeyword;
                        _listOfBlocks1.Add(ʁ[E]);
                        Ȱ
                            = "Bottles";
                    }
                }

                if (ȣ)
                {
                    ʁ[E].CustomName += " " + oreContainerKeyword + " " + ingotContainerKeyword;
                    ʔ.Add(ʁ[E]);
                    ʓ.Add(ʁ[E]);
                    Ȱ =
                        "Ores and Ingots";
                }

                if (Ȥ)
                {
                    ʁ[E].CustomName += " " + toolContainerKeyword + " " + ammoContainerKeyword + " " +
                                       bottleContainerKeyword;
                    ʑ.Add(ʁ[E]);
                    ʐ.Add(
                        ʁ[E]);
                    _listOfBlocks1.Add(ʁ[E]);
                    Ȱ = "Tools, Ammo and Bottles";
                }

                if (Ȱ != "")
                {
                    ɗ = "Assigned '" + ȧ + "' as a new container for type '" + Ȱ + "'.";
                }

                ʌ = "";
            }
        }

        private void ȴ()
        {
            ȹ(ʔ, oreContainerKeyword);
            ȹ(ʓ, ingotContainerKeyword);
            ȹ(ʒ, componentContainerKeyword);
            ȹ(ʑ, toolContainerKeyword);
            ȹ
                (ʐ, ammoContainerKeyword);
            ȹ(_listOfBlocks1, bottleContainerKeyword);
        }

        private void ȹ(List<IMyTerminalBlock> ă, string ȷ)
        {
            if (ă.Count > 1)
            {
                var ȶ = false
                    ;
                foreach (var X in ă)
                {
                    if (X.CustomName.Contains("[P"))
                    {
                        continue;
                    }

                    if (X.GetInventory(0).ItemCount == 0)
                    {
                        if (ȶ)
                        {
                            continue;
                        }

                        X.CustomName = X.CustomName.Replace(ȷ, ȷ + "!");
                        ȶ = true;
                        if (X.CustomName.Contains(ȷ + "!!!"))
                        {
                            var ȵ = Regex.Replace(X.CustomName, @"(" + ȷ + @")(!+)",
                                "");
                            ȵ = Regex.Replace(ȵ, @"\(\d+\.?\d*\%\)", "");
                            ȵ = ȵ.Replace(
                                "  ", " ");
                            X.CustomName = ȵ.TrimEnd(' ');
                            ʄ.Remove(X);
                            ɗ = "Unassigned '" + ȵ + "' from being a container for type '" + ȷ + "'.";
                        }
                    }
                    else
                    {
                        if (
                            X.CustomName.Contains(ȷ + "!"))
                        {
                            var ȵ = Regex.Replace(X.CustomName, @"(" + ȷ + @")(!+)",
                                ȷ);
                            ȵ = ȵ.Replace("  ", " ");
                            X.CustomName = ȵ.TrimEnd(' ');
                        }
                    }
                }
            }
        }

        private void ȸ()
        {
            for (var E = 0; E < ɏ.Count; E++)
            {
                var ȳ = ɏ[E].CustomName;
                var Ȳ = ȳ.ToLower();
                var ȱ = new List<string>();
                if (Ȳ.Contains(oreContainerKeyword.ToLower()) && !ȳ.Contains(oreContainerKeyword))
                {
                    ȱ.Add
                        (oreContainerKeyword);
                }

                if (Ȳ.Contains(ingotContainerKeyword.ToLower()) && !ȳ.Contains(ingotContainerKeyword))
                {
                    ȱ.Add(
                        ingotContainerKeyword);
                }

                if (Ȳ.Contains(componentContainerKeyword.ToLower()) && !ȳ.Contains(componentContainerKeyword))
                {
                    ȱ.Add(
                        componentContainerKeyword);
                }

                if (Ȳ.Contains(toolContainerKeyword.ToLower()) && !ȳ.Contains(toolContainerKeyword))
                {
                    ȱ.Add(toolContainerKeyword);
                }

                if (Ȳ.Contains(ammoContainerKeyword.ToLower()) && !ȳ.Contains(ammoContainerKeyword))
                {
                    ȱ.Add(ammoContainerKeyword);
                }

                if (Ȳ.Contains(
                        bottleContainerKeyword.ToLower()) &&
                    !ȳ.Contains(bottleContainerKeyword))
                {
                    ȱ.Add(bottleContainerKeyword);
                }

                foreach (var ê in lockedContainerKeywords)
                {
                    if (Ȳ.Contains(ê.ToLower()) && !ȳ.Contains(ê))
                    {
                        ȱ.Add(ê);
                        break;
                    }
                }

                foreach (var ê in hiddenContainerKeywords)
                {
                    if (Ȳ.Contains(ê.ToLower()) && !ȳ.Contains(ê))
                    {
                        ȱ.Add(ê);
                        break;
                    }
                }

                if (Ȳ.Contains(specialContainerKeyword.ToLower()) && !ȳ.Contains(
                    specialContainerKeyword))
                {
                    ȱ.Add(specialContainerKeyword);
                }

                if (Ȳ.Contains(noSortingKeyword.ToLower()) && !ȳ.Contains(noSortingKeyword))
                {
                    ȱ.Add(
                        noSortingKeyword);
                }

                if (Ȳ.Contains(manualMachineKeyword.ToLower()) && !ȳ.Contains(manualMachineKeyword))
                {
                    ȱ.Add(manualMachineKeyword);
                }

                if (Ȳ.Contains(autocraftingKeyword.ToLower()) && !ȳ.Contains(autocraftingKeyword))
                {
                    ȱ.Add(autocraftingKeyword);
                }

                if (Ȳ.Contains(
                    assembleKeyword.ToLower()) && !ȳ.Contains(assembleKeyword))
                {
                    ȱ.Add(assembleKeyword);
                }

                if (Ȳ.Contains(disassembleKeyword.ToLower()) && !ȳ.Contains(disassembleKeyword))
                {
                    ȱ.Add(disassembleKeyword);
                }

                if (Ȳ.Contains(learnKeyword.ToLower()) && !ȳ.Contains(learnKeyword))
                {
                    ȱ.Add(
                        learnKeyword);
                }

                if (Ȳ.Contains(learnManyKeyword.ToLower()) && !ȳ.Contains(learnManyKeyword))
                {
                    ȱ.Add(learnManyKeyword);
                }

                if (Ȳ.Contains(
                    inventoryLCDKeyword.ToLower()) && !ȳ.Contains(inventoryLCDKeyword))
                {
                    ȱ.Add(inventoryLCDKeyword);
                }

                if (Ȳ.Contains(mainLCDKeyword.ToLower()) && !ȳ.Contains(mainLCDKeyword))
                {
                    ȱ.Add(mainLCDKeyword);
                }

                if (Ȳ.Contains(warningsLCDKeyword.ToLower()) && !ȳ.Contains(warningsLCDKeyword))
                {
                    ȱ.Add(warningsLCDKeyword);
                }

                if (Ȳ.Contains(performanceLCDKeyword.ToLower()) && !ȳ.Contains(performanceLCDKeyword))
                {
                    ȱ.Add(
                        performanceLCDKeyword);
                }

                if (Ȳ.Contains("[p") && !ȳ.Contains("[P"))
                {
                    ȱ.Add("[P");
                }

                if (Ȳ.Contains("[pmax]") && !ȳ.Contains("[PMax]"))
                {
                    ȱ.Add("[PMax]");
                }

                if (Ȳ
                    .Contains("[pmin]") && !ȳ.Contains("[PMin]"))
                {
                    ȱ.Add("[PMin]");
                }

                foreach (var Ì in ȱ)
                {
                    ɏ[E].CustomName = ɏ[E].CustomName.Ŷ(Ì, Ì);
                    ɗ =
                        "Corrected name\nof: '" + ȳ + "'\nto: '" + ɏ[E].CustomName + "'";
                }
            }

            var Ơ = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(Ơ);
            for (var E = 0; E < Ơ.Count; E++)
            {
                var ȳ = Ơ[E].CustomName;
                var Ȳ = ȳ.ToLower();
                var ȱ = new List<string>();
                if (Ȳ.Contains(
                    mainLCDKeyword.ToLower()) && !ȳ.Contains(mainLCDKeyword))
                {
                    ȱ.Add(mainLCDKeyword);
                }

                if (Ȳ.Contains(inventoryLCDKeyword.ToLower()) && !ȳ.Contains
                    (inventoryLCDKeyword))
                {
                    ȱ.Add(inventoryLCDKeyword);
                }

                if (Ȳ.Contains(warningsLCDKeyword.ToLower()) && !ȳ.Contains(
                    warningsLCDKeyword))
                {
                    ȱ.Add(warningsLCDKeyword);
                }

                if (Ȳ.Contains(performanceLCDKeyword.ToLower()) && !ȳ.Contains(performanceLCDKeyword))
                {
                    ȱ.Add(
                        performanceLCDKeyword);
                }

                foreach (var Ì in ȱ)
                {
                    Ơ[E].CustomName = Ơ[E].CustomName.Ŷ(Ì, Ì);
                    ɗ = "Corrected name\nof: '" + ȳ + "'\nto: '" + Ơ[E].CustomName + "'";
                }
            }
        }

        private void ProcessContainerTypes()
        {
            if (_containerTypeProcessingStep == 0)
            {
                GetContainerForType(Ȅ, ʔ, oreContainerKeyword);
            }

            if (_containerTypeProcessingStep == 1)
            {
                GetContainerForType(ȃ, ʓ, ingotContainerKeyword);
            }

            if (_containerTypeProcessingStep == 2)
            {
                GetContainerForType(Ȃ, ʒ,
                    componentContainerKeyword);
            }

            if (_containerTypeProcessingStep == 3)
            {
                GetContainerForType(Ș, ʑ, toolContainerKeyword);
            }

            if (_containerTypeProcessingStep == 4)
            {
                GetContainerForType(_ammoMagazine, ʐ, ammoContainerKeyword);
            }

            if (_containerTypeProcessingStep == 5)
            {
                GetContainerForType(_oxygenContainerObject, _listOfBlocks1, bottleContainerKeyword);
            }

            if (_containerTypeProcessingStep == 6)
            {
                GetContainerForType(_gasContainerObject, _listOfBlocks1, bottleContainerKeyword);
            }

            if (_containerTypeProcessingStep == 7)
            {
                GetContainerForType(ȡ, ʑ, toolContainerKeyword);
            }

            if (_containerTypeProcessingStep == 8)
            {
                GetContainerForType(ȟ, ʑ, toolContainerKeyword);
            }

            if (_containerTypeProcessingStep == 9)
            {
                GetContainerForType(Ȟ, ʑ,
                    toolContainerKeyword);
            }

            _containerTypeProcessingStep++;
            if (_containerTypeProcessingStep > 9)
            {
                _containerTypeProcessingStep = 0;
            }
        }

        private void GetContainerForType(string Ί, List<IMyTerminalBlock> listOfBlocks, string containerKeyword)
        {
            if (listOfBlocks.Count == 0)
            {
                Log(
                    "There are no containers for type '" + containerKeyword + "'!\nBuild new ones or add the tag to existing ones!");
                ʌ = Ί;
                return;
            }

            IMyTerminalBlock theBlock = null;
            var inventoryPriority = int.MaxValue;
            for (var
                blockIndex = 0;
                blockIndex < listOfBlocks.Count;
                blockIndex++)
            {
                if (Ί == _oxygenContainerObject && listOfBlocks[blockIndex].BlockDefinition.TypeIdString.Contains("OxygenTank") &&
                    listOfBlocks[blockIndex].BlockDefinition.SubtypeId.Contains("Hydrogen"))
                {
                    continue;
                }

                if (Ί == _gasContainerObject && listOfBlocks[blockIndex].BlockDefinition.TypeIdString.Contains("OxygenTank") &&
                    !listOfBlocks[blockIndex].BlockDefinition.SubtypeId.Contains("Hydrogen"))
                {
                    continue;
                }

                var æ = listOfBlocks[blockIndex].GetInventory(0);
                if ((float) æ.CurrentVolume < (float) æ.MaxVolume * 0.98f)
                {
                    theBlock = listOfBlocks[blockIndex];
                    inventoryPriority
                        = GetBlockPriorityFromName(listOfBlocks[blockIndex]);
                    break;
                }
            }

            if (theBlock == null)
            {
                Log("All containers for type '" + containerKeyword + "' are full!\nYou should build new cargo containers!");
                ʌ = Ί;
                return;
            }

            IMyTerminalBlock Ά = null;
            if (fillBottles && (Ί == _oxygenContainerObject || Ί == _gasContainerObject))
            {
                Ά = ΐ(Ί);
            }

            for (var E = 0; E < ɑ.Count; E++)
            {
                if (ɑ[E] == theBlock || ɑ[E].CustomName.Contains(containerKeyword) && GetBlockPriorityFromName(ɑ[E]) <= inventoryPriority ||
                    Ί == "Ore" && ɑ[E].GetType().ToString().Contains("MyGasGenerator"))
                {
                    continue;
                }

                if (ɑ[E].CustomName.Contains(containerKeyword) && balanceTypeContainers &&
                    !ɑ[E].BlockDefinition.TypeIdString.Contains("OxygenGenerator") &&
                    !ɑ[E].BlockDefinition.TypeIdString.Contains("OxygenTank"))
                {
                    continue;
                }

                if (!Ζ(ɑ[E]))
                {
                    continue;
                }

                if (Ά != null)
                {
                    if (!ɑ[E].BlockDefinition.TypeIdString.Contains(
                        "Oxygen"))
                    {
                        TransferItemsBetweenInventories(Ί, ɑ[E], 0, Ά, 0);
                        continue;
                    }
                }

                TransferItemsBetweenInventories(Ί, ɑ[E], 0, theBlock, 0);
            }

            for (var E = 0; E < ɾ.Count; E++)
            {
                if (ɾ[E] == theBlock || ɾ[E].CustomName.Contains(containerKeyword) && GetBlockPriorityFromName(ɾ[
                    E]) <= inventoryPriority)
                {
                    continue;
                }

                if (!Ζ(ɾ[E]))
                {
                    continue;
                }

                TransferItemsBetweenInventories(Ί, ɾ[E], 1, theBlock, 0);
            }

            for (var E = 0; E < ɺ.Count; E++)
            {
                if (ɺ[E].Mode == MyAssemblerMode.Disassembly && ɺ[E].IsProducing || ɺ[E] == theBlock ||
                    ɺ[E].CustomName.Contains(containerKeyword) && GetBlockPriorityFromName(ɺ[E]) <= inventoryPriority)
                {
                    continue;
                }

                if (!Ζ(ɺ[E]))
                {
                    continue;
                }

                if (Ά != null)
                {
                    TransferItemsBetweenInventories(Ί,
                        ɺ[E], 1, Ά, 0);
                    continue;
                }

                TransferItemsBetweenInventories(Ί, ɺ[E], 1, theBlock, 0);
            }

            if (!ǉ())
            {
                _containerTypeProcessingStep++;
            }
        }

        private IMyTerminalBlock ΐ(string Ί)
        {
            var Μ = new List<IMyGasTank>(ɶ
            );
            if (Ί == _oxygenContainerObject)
            {
                Μ.RemoveAll(Κ => Κ.BlockDefinition.SubtypeId.Contains("Hydrogen"));
            }

            if (Ί == _gasContainerObject)
            {
                Μ.RemoveAll(Κ => !Κ.BlockDefinition.SubtypeId.Contains("Hydrogen"));
            }

            foreach (var Ι in Μ)
            {
                if (Ι.FilledRatio > 0)
                {
                    Ι.AutoRefillBottles = true;
                    return Ι;
                }
            }

            List<IMyGasGenerator>
                Θ = ɷ.Where(Η => Η.IsSameConstructAs(Me) && Η.Enabled == true).ToList();
            MyDefinitionId ē = MyItemType.MakeOre("Ice");
            foreach (var Λ
                in Θ)
            {
                if (h(ē, Λ) > 0)
                {
                    Λ.AutoRefill = true;
                    return Λ;
                }
            }

            return null;
        }

        private bool Ζ(IMyTerminalBlock e)
        {
            if (e.GetOwnerFactionTag() != Me.GetOwnerFactionTag())
            {
                Log("'" + e.CustomName + "'\nhas a different owner/faction!\nCan't move items from there!");
                return false;
            }

            return true;
        }

        private void Ε()
        {
            var Δ = '0';
            var Γ = '0';
            char[] Β = {'A', 'N', 'T', 'X'};
            char[] Α = {'a', 'd'};
            if (sortingPattern.Length == 2)
            {
                Δ = sortingPattern[0];
                Γ = sortingPattern[1];
            }

            ɍ = new List<IMyTerminalBlock>(ɑ);
            ɍ.AddRange(ʕ);
            if (enableInternalSorting)
            {
                if (Δ.ToString().IndexOfAny(Β
                ) < 0 || Γ.ToString().IndexOfAny(Α) < 0)
                {
                    Log("You provided the invalid sorting pattern '" + sortingPattern +
                      "'!\nCan't sort the inventories!");
                    return;
                }
            }
            else
            {
                ɍ = ɍ.FindAll(E => E.CustomName.ToLower().Contains("(sort:"));
            }

            for (var ƕ = ɳ; ƕ < ɍ.Count; ƕ++)
            {
                if (ǉ())
                {
                    return;
                }

                if (ɳ
                    >= ɍ.Count - 1)
                {
                    ɳ = 0;
                }
                else
                {
                    ɳ++;
                }

                var æ = ɍ[ƕ].GetInventory(0);
                var M = new List<MyInventoryItem>();
                æ.GetItems(M);
                if (M.Count > 200)
                {
                    continue;
                }

                char ʹ=Δ;
                char ͳ=Γ;
                string Ͳ=Regex.Match(ɍ[ƕ].CustomName, @"(\(sort:)(.{2})",
                    RegexOptions.IgnoreCase).Groups[2].Value;
                if (Ͳ.Length == 2){
                    Δ = Ͳ[
                    0];
                    Γ = Ͳ[
                    1];
                    if (Δ.ToString().IndexOfAny(Β) < 0 || Γ.ToString().IndexOfAny(Α) < 0)
                    {
                        Log("You provided an invalid sorting pattern in\n'" + ɍ[ƕ].CustomName +
                          "'!\nUsing global pattern!");
                        Δ = ʹ;
                        Γ = ͳ;
                    }
                }
                var ͱ =
                new List<MyInventoryItem>();
                æ.GetItems(ͱ);
                if (Δ == 'A')
                {
                    if (Γ == 'd')
                    {ͱ.Sort((Ɉ, ũ) => ũ.Amount.ToIntSafe().CompareTo(Ɉ.Amount.ToIntSafe()));
                    }
                    else
                    {ͱ.Sort((Ɉ, ũ) => Ɉ.Amount.ToIntSafe().CompareTo(ũ.Amount.ToIntSafe()));
                    }
                }
                else if (Δ == 'N')
                {
                    if (Γ == 'd')
                    {ͱ.Sort((Ɉ, ũ)
                        => ũ.Type.SubtypeId.ToString().CompareTo(Ɉ.Type.SubtypeId.ToString()));
                    }
                    else
                    {ͱ.Sort((Ɉ, ũ) => Ɉ.Type.SubtypeId.ToString().CompareTo(ũ.Type.SubtypeId.ToString()));
                    }
                }
                else if (Δ == 'T')
                {
                    if (Γ == 'd')
                    {ͱ.Sort((Ɉ, ũ) => ũ.Type.ToString().CompareTo(Ɉ.Type.ToString())
                        );
                    }
                    else
                    {ͱ.Sort((Ɉ, ũ) => Ɉ.Type.ToString().CompareTo(ũ.Type.ToString()));
                    }
                }
                else if (Δ == 'X')
                {
                    if (Γ == 'd')
                    {ͱ.Sort((Ɉ, ũ) =>
                        (ũ.Type.TypeId.ToString() + ũ.Amount.ToIntSafe().ToString(@"000000000")).CompareTo(
                            Ɉ.Type.TypeId.ToString() + Ɉ.Amount.ToIntSafe().ToString(@"000000000")));
                    }
                    else
                    {ͱ.Sort((Ɉ, ũ) =>
                        (Ɉ.Type.TypeId.ToString() + Ɉ.Amount.ToIntSafe().ToString(@"000000000")).CompareTo(
                            ũ.Type.TypeId.ToString() + ũ.Amount.ToIntSafe().ToString(@"000000000")));
                    }
                }

                if (ͱ.SequenceEqual(M, new Ť()))continue;
                foreach (
                    var Ì in ͱ)
                {
                    string ͽ = Ì.ToString();
                    for (var E = 0; E < M.Count; E++)
                    {
                        if (M[E].ToString() == ͽ)
                        {
                            æ.TransferItemTo(æ, E, M.Count, false);
                            M.Clear();
                            æ.GetItems(M);
                            break;
                        }
                    }
                }

                Δ = ʹ;
                Γ = ͳ;
            }
        }

        private void ͼ()
        {
            for (var ƕ = ɒ; ƕ < ʕ.Count; ƕ++)
            {
                if (ǉ())
                {
                    return;
                }

                ɒ++;
                í(ʕ[ƕ]);
                var d = 0;
                if (ʕ[ƕ].BlockDefinition.SubtypeId.Contains("Assembler"))
                {
                    var ƞ = ʕ[ƕ] as IMyAssembler;
                    if (ƞ.Mode == MyAssemblerMode.Disassembly)
                    {
                        d = 1;
                    }
                }

                var Ï = ʕ
                    [ƕ].CustomData.Split('\n');
                var ͻ = new List<string>();
                foreach (var À in Ï)
                {
                    if (!À.Contains("="))
                    {
                        continue;
                    }

                    MyDefinitionId K;
                    double ͺ = 0;
                    var Ͷ =À.Split('=');
                    if (Ͷ.Length >= 2){
                        if (!MyDefinitionId.TryParse(_objectBuilderPrefix +Ͷ[0], out K))continue;
                        double.TryParse(Ͷ[1]
                            , out ͺ);
                        if (Ͷ[
                        1].ToLower().Contains("all")){
                            ͺ = int.MaxValue;
                        }
                    }else{
                        continue;
                    }
                    double ͷ=h(K, ʕ[ƕ], d);
                    double ʗ = 0;
                    if (ͺ >= 0)
                    {
                        ʗ = ͺ -ͷ
                            ;
                    }
                    else
                    {
                        ʗ = Math.Abs(ͺ) -ͷ;
                    }

                    if (ʗ >= 1 && ͺ >= 0)
                    {
                        var æ = ʕ[ƕ].GetInventory(d);
                        if ((float) æ.CurrentVolume > (float) æ.MaxVolume * 0.98f)
                        {
                            continue;
                        }

                        if (ʗ > h(K) && ͺ != int.MaxValue)
                        {
                            ͻ.Add(ʗ - h(K) + " " + K.SubtypeName);
                        }

                        IMyTerminalBlock f = null;
                        if (allowSpecialSteal)
                        {
                            f = Z(K, true, ʕ
                                [ƕ]);
                        }
                        else
                        {
                            f = Z(K);
                        }

                        if (f != null)
                        {
                            TransferItemsBetweenInventories(K.ToString(), f, 0, ʕ[ƕ], d, ʗ, true);
                        }
                    }
                    else if (ʗ < 0)
                    {
                        var Ä = W(ʕ[ƕ], ʁ);
                        if (Ä != null)
                        {
                            TransferItemsBetweenInventories
                                (K.ToString(), ʕ[ƕ], d, Ä, 0, Math.Abs(ʗ), true);
                        }
                    }
                }

                if (ͻ.Count > 0)
                {
                    Log(ʕ[ƕ].CustomName +
                      "\nis missing the following items to match its quota:\n" + string.Join(", ", ͻ));
                }
            }

            ɒ = 0;
        }

        private void β(List<IMyTerminalBlock> ă)
        {
            foreach (var X in ă)
            {
                var α = X.CustomName;
                var ȵ = "";
                var ΰ
                    = Regex.Match(α, @"\(\d+\.?\d*\%\)").Value;
                if (ΰ != "")
                {
                    ȵ = α.Replace(ΰ, "").TrimEnd(' ');
                }
                else
                {
                    ȵ =
                        α;
                }

                var æ = X.GetInventory(0);
                var ǝ = ((float) æ.CurrentVolume).ƀ((float) æ.MaxVolume);
                if (showFillLevel)
                {
                    ȵ += " (" + ǝ + ")";
                    ȵ = ȵ.Replace("  ", " ");
                }

                if (ȵ != α)
                {
                    X.CustomName = ȵ;
                }
            }
        }

        private StringBuilder ί()
        {
            if (ɻ.Count > 1)
            {
                var ή = @"(" + autocraftingKeyword + @" *)(\d*)";
                ɻ.Sort((Ɉ, ũ) => Regex.Match(Ɉ.CustomName, ή).Groups[2].Value
                    .CompareTo(Regex.Match(ũ.CustomName, ή).Groups[2].Value));
            }

            StringBuilder ł = new StringBuilder();
            if (!ɻ[0].GetText().Contains(Ǵ))
            {
                ɻ[0]
                    .Font = defaultFont;
                ɻ[0].FontSize = defaultFontSize;
                ɻ[0].TextPadding = defaultPadding;
            }

            foreach (var q in ɻ)
            {
                ł.Append(q.GetText()
                         + "\n");
                q.WritePublicTitle("Craft item manually once to show up here");
                q.Font = ɻ[0].Font;
                q.FontSize = ɻ[0].FontSize;
                q.TextPadding = ɻ[0].TextPadding;
                q.Alignment = TextAlignment.LEFT;
                q.ContentType = ContentType.TEXT_AND_IMAGE;
            }

            var γ = new List<string>(ł.ToString().Split('\n'));
            var Ω = new List<string>();
            var λ = new HashSet<string>();
            string κ;
            foreach (var À in γ)
            {
                if (À.IndexOfAny(ȍ) <= 0)
                {
                    continue;
                }

                κ = À.Remove(À.IndexOf(" "));
                if (!λ.Contains(κ))
                {
                    Ω.Add(À);
                    λ.Add(κ);
                }
            }

            List<string> Ï = ɻ[0].CustomData.Split('\n').ToList();
            foreach (var I in ʅ)
            {
                var ι = false;
                if (λ.Contains(I))
                {
                    continue;
                }

                foreach (var À in Ï)
                {
                    if (!À.StartsWith("-"))
                    {
                        continue;
                    }

                    var θ = "";
                    try
                    {
                        if (À.Contains("="))
                        {
                            θ = À.Substring(1, À.IndexOf("=") - 1);
                        }
                        else
                        {
                            θ = À.Substring(1);
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (θ == I)
                    {
                        ι = true;
                        break;
                    }
                }

                if (!ι)
                {
                    MyDefinitionId K = ǔ(I);
                    var η = Math.Ceiling(h(K));
                    Ω.Add(I + " " + η + " = " + η);
                }
            }

            foreach (var À in Ï)
            {
                if (!À.StartsWith("-"
                ))
                {
                    continue;
                }

                if (À.Contains("="))
                {
                    Ω.Add(À);
                }
            }

            StringBuilder ƌ = new StringBuilder();
            try
            {
                IOrderedEnumerable<string> ζ;
                ζ = Ω.OrderBy
                    (Ɉ => Ɉ);
                bool ε;
                string δ, I, Σ;
                foreach (var À in ζ)
                {
                    ε = false;
                    if (À.StartsWith("-"))
                    {
                        I = À.Remove(À.IndexOf("=")).TrimStart('-');
                        δ =
                            "-";
                    }
                    else
                    {
                        I = À.Remove(À.IndexOf(" "));
                        δ = "";
                    }

                    Σ = À.Replace(δ + I, "");
                    foreach (var Ì in ʅ)
                    {
                        if (Ì == I)
                        {
                            ε = true;
                            break;
                        }
                    }

                    if (ε)
                    {
                        ƌ.Append(δ +
                                 I + Σ + "\n");
                    }
                }
            }
            catch
            {
            }

            return ƌ;
        }

        private void Π(StringBuilder ł)
        {
            if (ł.Length == 0)
            {
                ł.Append(
                    "Autocrafting error!\n\nNo items for crafting available!\n\nIf you hid all items, check the custom data of the first autocrafting panel and reenable some of them.\n\nOtherwise, store or build new items manually!"
                );
                ł = ɻ[0].Ŝ(ł, 2, false);
                ɻ[0].WriteText(ł);
                return;
            }

            var Ļ = ł.ToString().TrimEnd('\n').Split('\n');
            int ĺ = Ļ.Length;
            var Ĺ = 0;
            float
                Ρ = 0;
            foreach (var q in ɻ)
            {
                var ő = q.Ŋ();
                var ĸ = q.ō();
                var ķ = 0;
                var ƌ = new List<string>();
                if (q == ɻ[0] ||
                    headerOnEveryScreen)
                {
                    var Ξ = Ǵ;
                    if (headerOnEveryScreen && ɻ.Count > 1)
                    {
                        Ξ += " " + (ɻ.IndexOf(q) + 1) + "/" + ɻ.Count;
                        try
                        {
                            Ξ += " [" + Ļ[Ĺ][0] + "-#]";
                        }
                        catch
                        {
                            Ξ +=
                                " [Empty]";
                        }
                    }

                    ƌ.Add(Ξ);
                    ƌ.Add(q.Ň('=', q.ū(Ξ)).ToString() + "\n");
                    var Ν = "Component ";
                    var Ο = "Current | Wanted ";
                    Ρ = q.ū("Wanted ");
                    string Ǔ = q.Ň(' ', ő - q.ū(Ν) - q.ū(Ο)).ToString();
                    ƌ.Add(Ν + Ǔ + Ο + "\n");
                    ķ = 5;
                }

                while (Ĺ < ĺ && ķ < ĸ || q == ɻ[ɻ.Count - 1] && Ĺ < ĺ)
                {
                    var À = Ļ[Ĺ].Split
                        (' ');
                    À[0] += " ";
                    À[1] = À[1].Replace('$', ' ');
                    string Ǔ = q.Ň(' ', ő - q.ū(À[0]) - q.ū(À[1]) - Ρ).ToString();
                    var έ = À[0] + Ǔ + À[1] + À[2]
                        ;
                    ƌ.Add(έ);
                    Ĺ++;
                    ķ++;
                }

                if (headerOnEveryScreen && ɻ.Count > 1)
                {
                    ƌ[0] = ƌ[0].Replace('#', Ļ[Ĺ - 1][0]);
                }

                q.WriteText(string.Join("\n", ƌ));
            }

            if (showAutocraftingModifiers)
            {
                var ά = "\n\n---\n\nModifiers (append after wanted amount):\n" + "'A' - Assemble only\n" +
                        "'D' - Disassemble only\n" + "'P' - Always queue first (priority)\n" +
                        "'H' - Hide and manage in background\n" + "'I' - Hide and ignore\n";
                ɻ[ɻ.Count - 1].WriteText(ά, true);
            }
        }

        private void Ϋ()
        {
            ɻ.Clear();
            GridTerminalSystem.GetBlocksOfType(ɻ,
                q => q.IsSameConstructAs(Me) && q.CustomName.Contains(autocraftingKeyword));
            if (ɻ.Count == 0)
            {
                return;
            }

            if (ɼ.Count == 0)
            {
                Log(
                    "No assemblers found!\nBuild assemblers to enable autocrafting!");
                return;
            }

            ˋ();
            List<MyDefinitionId> Ϊ = new List<MyDefinitionId>();
            var Ω = ί().ToString().TrimEnd('\n').Split('\n');
            StringBuilder ƌ = new StringBuilder();
            foreach (var À in Ω)
            {
                var I = "";
                var Ψ = true;
                if (À.StartsWith("-"))
                {
                    Ψ = false;
                    try
                    {
                        I = À.Substring(1, À.IndexOf("=") - 1);
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        I = À.Substring(0, À.IndexOf(" "));
                    }
                    catch
                    {
                        continue;
                    }
                }

                MyDefinitionId K = ǔ(I);
                if (K == null)
                {
                    continue;
                }

                var Χ = Math.Ceiling(h(K));
                string Φ = À.Substring(À.IndexOfAny(ȍ) + 1).ToLower();
                double Υ = 0;
                double.TryParse(Regex.Replace(Φ, @"\D", ""), out Υ);
                var Τ = Χ.ToString();
                string Ͱ=Υ.ToString();
                var ʳ = "";
                var ã = false;
                if (Φ.Contains("h"
                ))
                {
                    if (!ɻ[0].CustomData.StartsWith(Ȏ))
                    {
                        ɻ[0].CustomData = Ȏ;
                    }

                    ɻ[0].CustomData += "\n-" + I + "=" + Υ;
                    continue;
                }

                if (Φ.Contains("i"))
                {
                    if (!ɻ[0].CustomData.StartsWith(Ȏ))
                    {
                        ɻ[0].CustomData = Ȏ;
                    }

                    ɻ[0].CustomData += "\n-" + I;
                    continue;
                }

                if (Φ.Contains("a"))
                {
                    if (Χ > Υ)
                    {
                        Υ = Χ;
                    }

                    ʳ +=
                        "A";
                }

                if (Φ.Contains("d"))
                {
                    if (Χ < Υ)
                    {
                        Υ = Χ;
                    }

                    ʳ += "D";
                }

                if (Φ.Contains("p"))
                {
                    ã = true;
                    ʳ += "P";
                }

                ƣ(K, Υ);
                var ʱ = Math.Abs(Υ - Χ);
                bool ʰ;
                MyDefinitionId Û = ƶ(K, out ʰ);
                var ʯ = Ƥ(Û);
                if (Χ >= Υ + Υ * assembleMargin && ʯ > 0 && ƥ(Û) > 0)
                {
                    ˍ(Û);
                    Ƣ(Û, 0);
                    ʯ = 0;
                    ɗ = "Removed '" + K.SubtypeId.ToString() +
                        "' from the assembling queue.";
                }

                if (Χ <= Υ - Υ * disassembleMargin && ʯ > 0 && ƥ(Û) < 0)
                {
                    ˍ(Û);
                    Ƣ(Û, 0);
                    ʯ = 0;
                    ɗ = "Removed '" + K.SubtypeId.ToString() +
                        "' from the disassembling queue.";
                }

                var Ã = "";
                if (ʯ > 0 || ʱ > 0)
                {
                    if (enableAutodisassembling && Χ > Υ + Υ * disassembleMargin)
                    {
                        Ƣ(Û, -1);
                        Ã = "$[D:";
                    }
                    else if (
                        enableAutocrafting && Χ < Υ - Υ * assembleMargin)
                    {
                        Ƣ(Û, 1);
                        Ã = "$[A:";
                    }

                    if (Ã != "")
                    {
                        if (ʯ == 0)
                        {
                            Ã += "Wait]";
                        }
                        else
                        {
                            Ã += Math.Round(ʯ) + "]";
                        }
                    }
                }
                else
                {
                    Ƣ(Û, 0);
                }

                if (!ʰ)
                {
                    Ã = "$[NoBP!]";
                }

                if (ʰ && ã)
                {
                    Ϊ.Add(Û);
                }

                var ʮ = "$=$ ";
                if (Χ > Υ)
                {
                    ʮ = "$>$ ";
                }

                if (Χ < Υ)
                {
                    ʮ = "$<$ ";
                }

                if (Ψ)
                {
                    ƌ.Append(I + " " + Τ + Ã + ʮ +Ͱ+ʳ + "\n");
                }

                if (Ã.Contains("[D:Wait]"))
                {
                    ʿ(Û, ʱ);
                }
                else if (Ã.Contains("[A:Wait]"))
                {
                    ʾ(Û, ʱ, ã);
                    ɗ = "Queued " + ʱ + " '" + K.SubtypeId.ToString() +
                        "' in the assemblers.";
                }
                else if (Ã.Contains("[NoBP!]") && Υ > Χ)
                {
                    Log("Can't craft\n'" + K.SubtypeId.ToString() +
                      "'\nThere's no blueprint stored for this item!\nTag an assembler with the '" + learnKeyword +
                      "' keyword and queue\nit up about 100 times to learn the blueprint.");
                }
            }

            ˌ();
            ˣ(Ϊ);
            Π(ƌ);
        }

        private void ʲ()
        {
            if (ɿ.Count
                > 0)
            {
                return;
            }

            MyDefinitionId ʥ = MyItemType.MakeOre("Stone");
            MyDefinitionId Û = MyDefinitionId.Parse(ȝ + "StoneOreToIngotBasic");
            var ʭ = h(ʥ);
            if (ʭ > 0)
            {
                var ʬ = Math.Floor(ʭ / 500 / ɸ.Count);
                if (ʬ < 1)
                {
                    return;
                }

                foreach (var ʫ in ɸ)
                {
                    if (ʫ.IsQueueEmpty)
                    {
                        ʫ.AddQueueItem(Û,
                            ʬ);
                    }
                }
            }
        }

        private void ʪ()
        {
            if (ɠ == 0)
            {
                ɠ += ʩ(ʔ, Ȅ, true);
            }

            if (ɠ == 1)
            {
                ɠ += ʩ(ʓ, ȃ, true);
            }

            if (ɠ == 2)
            {
                ɠ += ʩ(ʒ, Ȃ, true);
            }

            if (ɠ == 3)
            {
                ɠ += ʩ(ʑ, Ș, true);
            }

            if (ɠ == 4)
            {
                ɠ += ʩ(ʐ
                    , _ammoMagazine, true);
            }

            if (ɠ == 5)
            {
                ɠ += ʩ(_listOfBlocks1, "ContainerObject", true);
            }

            ɠ++;
            if (ɠ > 5)
            {
                ɠ = 0;
            }
        }

        private int ʩ(List<IMyTerminalBlock> ɉ, string ʴ = "", bool ʸ = false)
        {
            if (ʸ)
            {
                ɉ.RemoveAll(ŭ =>
                    ŭ.InventoryCount == 2 || ŭ.BlockDefinition.TypeIdString.Contains("OxygenGenerator") ||
                    ŭ.BlockDefinition.TypeIdString.Contains("OxygenTank"));
            }

            if (ɉ.Count < 2)
            {
                return 1;
            }

            var ʻ = new Dictionary<MyItemType, double>();
            for (
                var E = 0;
                E < ɉ.Count;
                E++)
            {
                var M = new List<MyInventoryItem>();
                ɉ[E].GetInventory(0).GetItems(M);
                foreach (var Ì in M)
                {
                    if (!Ì.Type.TypeId.Contains(ʴ))
                    {
                        continue;
                    }

                    var K = Ì.Type;
                    if (ʻ.ContainsKey(K))
                    {
                        ʻ[K] += (double) Ì.Amount;
                    }
                    else
                    {
                        ʻ[K] = (double) Ì.Amount;
                    }
                }
            }

            var ʺ = new Dictionary<MyItemType, double>();
            foreach (var Ì in ʻ)
            {
                ʺ[Ì.Key] = (int) (Ì.Value / ɉ.Count);
            }

            for (var ʹ = 0; ʹ < ɉ.Count; ʹ++)
            {
                if (ǉ())
                {
                    return 0;
                }

                var ʷ = new List<MyInventoryItem>();
                ɉ[ʹ].GetInventory(0).GetItems(ʷ);
                var ʶ = new Dictionary<MyItemType, double>();
                foreach (var Ì in ʷ)
                {
                    var K = Ì.Type;
                    if (ʶ.ContainsKey(K))
                    {
                        ʶ[
                            K] += (double) Ì.Amount;
                    }
                    else
                    {
                        ʶ[K] = (double) Ì.Amount;
                    }
                }

                double Ģ = 0;
                foreach (var Ì in ʻ)
                {
                    ʶ.TryGetValue(Ì.Key, out Ģ);
                    var ʵ = ʺ[Ì
                        .Key];
                    if (Ģ <= ʵ + 1)
                    {
                        continue;
                    }

                    for (var ʨ = 0; ʨ < ɉ.Count; ʨ++)
                    {
                        if (ɉ[ʹ] == ɉ[ʨ])
                        {
                            continue;
                        }

                        var ģ = h(Ì.Key, ɉ[ʨ]);
                        if (ģ >= ʵ - 1)
                        {
                            continue;
                        }

                        var ʗ = ʵ - ģ;
                        if (ʗ > Ģ - ʵ)
                        {
                            ʗ = Ģ - ʵ;
                        }

                        if (ʗ > 0)
                        {
                            Ģ -= TransferItemsBetweenInventories(Ì.Key.ToString(), ɉ[ʹ], 0, ɉ[ʨ], 0, ʗ, true);
                            if (Ģ.Ɠ(ʵ - 1, ʵ + 1))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return ǉ() ? 0 : 1;
        }

        private void
            ʢ()
        {
            if (ɿ.Count == 0)
            {
                return;
            }

            if (ɚ == 0)
            {
                ȏ = ˑ(ɽ);
            }

            if (ɚ == 1)
            {
                Ȉ = ˑ(ʃ);
            }

            if (enableScriptRefineryFilling)
            {
                if (ɚ == 2)
                {
                    ʜ(ɽ, ȏ);
                }

                if (ɚ == 3)
                {
                    ʜ(ʃ, Ȉ);
                }

                if (ɚ
                    == 4)
                {
                    ʦ(ɽ, ȏ);
                }

                if (ɚ == 5)
                {
                    ʦ(ʃ, Ȉ);
                }

                if (ɚ == 6 && ɽ.Count > 0 && ʃ.Count > 0)
                {
                    var ʡ = false;
                    ʡ = ˀ(ɽ, ʃ, ȏ);
                    if (!ʡ)
                    {
                        ˀ(ʃ, ɽ, Ȉ);
                    }
                }
            }
            else
            {
                if (ɚ > 1)
                {
                    ɚ = 6;
                }
            }

            ɚ++;
            if (
                ɚ > 6)
            {
                ɚ = 0;
            }
        }

        private void ʠ()
        {
            if (ɘ == 0)
            {
                ɘ += ʩ(ɽ.ToList<IMyTerminalBlock>());
            }

            if (ɘ == 1)
            {
                ɘ += ʩ(ʃ.ToList<IMyTerminalBlock>());
            }

            ɘ++;
            if (ɘ > 1)
            {
                ɘ = 0;
            }
        }

        private void ʟ(List<IMyRefinery> ʞ, List<MyItemType> ʝ)
        {
            foreach (var Ù in ʞ)
            {
                var æ = Ù.GetInventory(0);
                var M = new List<
                    MyInventoryItem>();
                æ.GetItems(M);
                if (M.Count < 2)
                {
                    continue;
                }

                var ʛ = false;
                var ʚ = 0;
                var ʙ = "";
                foreach (var ʘ in ʝ)
                {
                    for (var E = 0; E < M.Count; E++)
                    {
                        if (M[E].Type == ʘ)
                        {
                            ʚ = E;
                            ʙ = ʘ.SubtypeId;
                            ʛ = true;
                            break;
                        }
                    }

                    if (ʛ)
                    {
                        break;
                    }
                }

                if (ʚ != 0)
                {
                    æ.TransferItemTo(æ, ʚ, 0, true);
                    ɗ =
                        "Sorted the refining queue.\n'" + ʙ + "' is now at the front of the queue.";
                }
            }
        }

        private void ʜ(List<IMyRefinery> ʣ, List<MyItemType> ʝ)
        {
            if (ʣ.Count == 0)
            {
                ɚ++;
                return;
            }

            var ʧ = new MyItemType();
            var ʥ = MyItemType.MakeOre("Stone");
            foreach (var ʘ in ʝ)
            {
                if (h(ʘ) > 100)
                {
                    ʧ = ʘ;
                    break;
                }
            }

            if (!ʧ.ToString
                ().Contains(Ȅ))
            {
                return;
            }

            for (var E = 0; E < ʣ.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                var æ = ʣ[E].GetInventory(0);
                if ((float) æ.CurrentVolume > (
                    float) æ.MaxVolume * 0.75f)
                {
                    var M = new List<MyInventoryItem>();
                    æ.GetItems(M);
                    foreach (var Ì in M)
                    {
                        if (Ì.Type == ʧ)
                        {
                            return;
                        }
                    }

                    var Ä = W(ʣ[E], ʔ);
                    if (Ä != null)
                    {
                        TransferItemsBetweenInventories("", ʣ[E], 0, Ä, 0);
                    }
                }
            }

            if (!ǉ())
            {
                ɚ++;
            }
        }

        private void ʦ(List<IMyRefinery> ʣ, List<MyItemType> ʝ)
        {
            if (ʣ.Count == 0)
            {
                ɚ
                    ++;
                return;
            }

            var ă = new List<IMyTerminalBlock>();
            ă.AddRange(ɑ);
            ă.AddRange(ʕ);
            var ʥ = MyItemType.MakeOre("Stone");
            foreach
                (var ʘ in ʝ)
            {
                if (h(ʘ) == 0)
                {
                    continue;
                }

                var ʤ = Z(ʘ, true);
                if (ʤ == null)
                {
                    continue;
                }

                for (var E = 0; E < ʣ.Count; E++)
                {
                    if (ǉ())
                    {
                        return;
                    }

                    var æ = ʣ[E].GetInventory(0);
                    if ((float) æ.CurrentVolume > (float) æ.MaxVolume * 0.98f)
                    {
                        continue;
                    }

                    TransferItemsBetweenInventories(ʘ.ToString(), ʤ, 0, ʣ[E], 0);
                }
            }

            if (
                !ǉ())
            {
                ɚ++;
            }
        }

        private bool ˀ(List<IMyRefinery> ˡ, List<IMyRefinery> ˠ, List<MyItemType> ʝ)
        {
            for (var E = 0; E < ˡ.Count; E++)
            {
                if ((float) ˡ[E].GetInventory(0).CurrentVolume > 0.05f)
                {
                    continue;
                }

                for (var Ɵ = 0; Ɵ < ˠ.Count; Ɵ++)
                {
                    if ((float) ˠ[Ɵ].GetInventory(0).CurrentVolume > 0)
                    {
                        foreach (var
                            ʘ in ʝ)
                        {
                            TransferItemsBetweenInventories(ʘ.ToString(), ˠ[Ɵ], 0, ˡ[E], 0, -0.5);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private List<MyItemType> ˑ(List<IMyRefinery> ʣ)
        {
            if (ʣ.Count
                == 0)
            {
                ɚ++;
                return null;
            }

            var ː = new List<string>(ȉ);
            ː.Sort((Ɉ, ũ) => (h(MyItemType.MakeIngot(Ɉ)) / Ɯ(Ɉ)).CompareTo(h(
                MyItemType.MakeIngot(ũ)) / Ɯ(ũ)));
            ː.InsertRange(0, fixedRefiningList);
            var ˎ = new List<MyItemType>();
            MyItemType K;
            foreach (
                var Ì in ː)
            {
                K = MyItemType.MakeOre(Ì);
                foreach (var Ù in ʣ)
                {
                    if (Ù.GetInventory(0).CanItemsBeAdded(1, K))
                    {
                        ˎ.Add(K);
                        break;
                    }
                }
            }

            if (!ǉ(
            ))
            {
                ɚ++;
            }

            return ˎ;
        }

        private void ˏ()
        {
            foreach (var ƞ in ɼ)
            {
                if (ƞ.GetOwnerFactionTag() == Me.GetOwnerFactionTag())
                {
                    var æ = ƞ.GetInventory(0);
                    if ((float) æ.CurrentVolume == 0)
                    {
                        continue;
                    }

                    if (ƞ.IsQueueEmpty || ƞ.Mode == MyAssemblerMode.Disassembly || (float) æ.CurrentVolume > (
                        float) æ.MaxVolume * 0.98f)
                    {
                        var Ä = W(ƞ, ʓ);
                        if (Ä != null)
                        {
                            TransferItemsBetweenInventories("", ƞ, 0, Ä, 0);
                        }
                    }
                }
            }
        }

        private void ˮ()
        {
            foreach (var ƞ in ɼ)
            {
                if (ƞ.Mode == MyAssemblerMode.Disassembly)
                {
                    continue;
                }

                if (ƞ.CustomData.Contains("skipQueueSorting"))
                {
                    ƞ.CustomData = "";
                    continue;
                }

                var Ã = new
                    List<MyProductionItem>();
                ƞ.GetQueue(Ã);
                if (Ã.Count < 2)
                {
                    continue;
                }

                double ˬ=double.MaxValue;
                var ʚ = 0;
                var ʙ = "";
                for (var E = 0; E < Ã.Count; E++)
                {
                    MyDefinitionId K = ǘ(Ã[E].BlueprintId);
                    var ˤ = h(K);
                    if (ˤ <ˬ)
                    {ˬ =ˤ;
                        ʚ = E;
                        ʙ = K.SubtypeId.ToString();
                    }
                }

                if (ʚ != 0)
                {
                    ƞ.MoveQueueItemRequest(Ã[ʚ].ItemId, 0);
                    ɗ = "Sorted the assembling queue.\n'" + ʙ + "' is now at the front of the queue.";
                }
            }
        }

        private void ˣ(List<
            MyDefinitionId> ˢ)
        {
            if (ˢ.Count == 0)
            {
                return;
            }

            if (ˢ.Count > 1)
            {
                ˢ.Sort((Ɉ, ũ) => h(ǘ(Ɉ)).CompareTo(h(ǘ(ũ))));
            }

            foreach (var ƞ in ɼ)
            {
                var Ã = new List<
                    MyProductionItem>();
                ƞ.GetQueue(Ã);
                if (Ã.Count < 2)
                {
                    continue;
                }

                foreach (var Û in ˢ)
                {
                    var ƕ = Ã.FindIndex(E => E.BlueprintId == Û);
                    if (ƕ == -1)
                    {
                        continue;
                    }

                    if (
                        ƕ == 0)
                    {
                        ƞ.CustomData = "skipQueueSorting";
                        break;
                    }

                    ƞ.MoveQueueItemRequest(Ã[ƕ].ItemId, 0);
                    ƞ.CustomData = "skipQueueSorting";
                    ɗ =
                        "Sorted the assembler queue by priority.\n'" + ǘ(Û).SubtypeId.ToString() +
                        "' is now at the front of the queue.";
                    break;
                }
            }
        }

        private void ʾ(MyDefinitionId Û, double k, bool ã)
        {
            var ʼ = new List<IMyAssembler>();
            foreach (var ƞ in ɼ)
            {
                if (ƞ.CustomName.Contains(disassembleKeyword))
                {
                    continue;
                }

                if (ã ==
                    false && ƞ.Mode == MyAssemblerMode.Disassembly && !ƞ.IsQueueEmpty)
                {
                    continue;
                }

                if (ƞ.Mode == MyAssemblerMode.Disassembly)
                {
                    ƞ.ClearQueue();
                    ƞ
                        .Mode = MyAssemblerMode.Assembly;
                }

                if (ƞ.CanUseBlueprint(Û))
                {
                    ʼ.Add(ƞ);
                }
            }

            if (ʼ.Count == 0)
            {
                Log(
                    "There's no assembler available to produce '" + Û.SubtypeName +
                    "'. Make sure, that you have at least one assembler with no tags or the !assemble-only tag!");
            }

            ʽ(ʼ, Û, k);
        }

        private void ʿ(MyDefinitionId Û, double k)
        {
            var ʼ = new List<IMyAssembler>();
            foreach (var ƞ in ɼ)
            {
                if (ƞ.CustomName.Contains(assembleKeyword))
                {
                    continue;
                }

                if (ƞ.Mode == MyAssemblerMode.Assembly && ƞ.IsProducing)
                {
                    continue;
                }

                if (ƞ.Mode == MyAssemblerMode.Assembly)
                {
                    ƞ.ClearQueue();
                    ƞ.Mode = MyAssemblerMode.Disassembly;
                }

                if (ƞ.Mode == MyAssemblerMode.Assembly)
                {
                    continue;
                }

                if (ƞ.CanUseBlueprint(Û
                ))
                {
                    ʼ.Add(ƞ);
                }
            }

            if (ʼ.Count == 0)
            {
                Log("There's no assembler available to dismantle '" + Û.SubtypeName +
                  "'. Make sure, that you have at least one assembler with no tags or the !disassemble-only tag!");
            }

            ʽ(ʼ, Û, k);
        }

        private void ʽ(List<IMyAssembler> ʼ, MyDefinitionId Û, double k)
        {
            if (ʼ.Count == 0)
            {
                return;
            }

            var ˁ = Math.Ceiling(k / ʼ.Count);
            foreach (var ƞ in ʼ)
            {
                if (ˁ > k)
                {
                    ˁ = Math.Ceiling(k);
                }

                if (k > 0)
                {
                    ƞ.InsertQueueItem(0, Û, ˁ);
                    k -= ˁ;
                }
                else
                {
                    break;
                }
            }
        }

        private void ˍ(
            MyDefinitionId Û)
        {
            foreach (var ƞ in ɼ)
            {
                var Ã = new List<MyProductionItem>();
                ƞ.GetQueue(Ã);
                for (var E = 0; E < Ã.Count; E++)
                {
                    if (Ã[E].BlueprintId == Û)
                    {
                        ƞ.RemoveQueueItem(E, Ã[E].Amount);
                    }
                }
            }
        }

        private void ˋ()
        {
            foreach (var ƞ in ɼ)
            {
                ƞ.UseConveyorSystem = true;
                ƞ.CooperativeMode
                    = false;
                ƞ.Repeating = false;
            }
        }

        private void ˌ()
        {
            var ˊ = new List<IMyAssembler>(ɼ);
            ˊ.RemoveAll(Ɉ => Ɉ.IsQueueEmpty);
            if (ˊ.Count == 0)
            {
                return;
            }

            var ˉ = new List<IMyAssembler>(ɼ);
            ˉ.RemoveAll(Ɉ => !Ɉ.IsQueueEmpty);
            foreach (var ˈ in ˊ)
            {
                if (ˉ.Count
                    == 0)
                {
                    return;
                }

                var Ã = new List<MyProductionItem>();
                ˈ.GetQueue(Ã);
                var ˇ = (double) Ã[0].Amount;
                if (ˇ <= 10)
                {
                    continue;
                }

                var ˆ = Math.Ceiling(ˇ / 2);
                foreach (var Ǥ in ˉ)
                {
                    if (!Ǥ.CanUseBlueprint(Ã[0].BlueprintId))
                    {
                        continue;
                    }

                    if (ˈ.Mode == MyAssemblerMode.Assembly && Ǥ.CustomName.Contains(disassembleKeyword))
                    {
                        continue;
                    }

                    if (ˈ.Mode == MyAssemblerMode.Disassembly && Ǥ.CustomName.Contains(assembleKeyword))
                    {
                        continue;
                    }

                    Ǥ.Mode = ˈ.Mode;
                    if (Ǥ.Mode != ˈ.Mode)
                    {
                        continue;
                    }

                    Ǥ.AddQueueItem(Ã[0].BlueprintId, ˆ);
                    ˈ.RemoveQueueItem(0, ˆ);
                    ˉ.Remove(Ǥ);
                    break;
                }
            }
        }

        private void ĕ()
        {
            if (ɷ.Count == 0)
            {
                return;
            }

            var Ô = iceFillLevelPercentage / 100;
            MyDefinitionId ē = MyItemType.MakeOre("Ice");
            string Ē = ē.ToString();
            var đ = 0.00037;
            foreach (var Ď in ɷ)
            {
                var æ = Ď.GetInventory(0);
                var Đ = h(ē, Ď);
                var Ĕ = Đ * đ;
                var ď = (
                    double) æ.MaxVolume;
                if (Ĕ > ď * (Ô + 0.001))
                {
                    var Ä = W(Ď, ʔ);
                    if (Ä != null)
                    {
                        var û = (Ĕ - ď * Ô) / đ;
                        TransferItemsBetweenInventories(Ē, Ď, 0, Ä, 0, û);
                    }
                }
                else if (Ĕ < ď * (Ô -
                                  0.001))
                {
                    var f = Z(ē, true);
                    if (f != null)
                    {
                        var û = (ď * Ô - Ĕ) / đ;
                        TransferItemsBetweenInventories(Ē, f, 0, Ď, 0, û);
                    }
                }
            }

            double č = 0;
            double Č = 0;
            foreach (var Ď in
                ɷ)
            {
                č += h(ē, Ď);
                var æ = Ď.GetInventory(0);
                Č += (double) æ.MaxVolume;
            }

            var ĝ = č * đ / Č;
            foreach (var Ĥ in ɷ)
            {
                var O = Ĥ.GetInventory(0)
                    ;
                var Ģ = h(ē, Ĥ);
                var ġ = Ģ * đ;
                var Ġ = (double) O.MaxVolume;
                if (ġ > Ġ * (ĝ + 0.001))
                {
                    foreach (var ğ in ɷ)
                    {
                        if (Ĥ == ğ)
                        {
                            continue;
                        }

                        var N = ğ
                            .GetInventory(0);
                        var ģ = h(ē, ğ);
                        var Ğ = ģ * đ;
                        var Ĝ = (double) N.MaxVolume;
                        if (Ğ < Ĝ * (ĝ - 0.001))
                        {
                            var ě = (Ĝ * ĝ - Ğ) / đ;
                            if ((Ģ - ě
                                ) * đ >= Ġ * ĝ && ě > 5)
                            {
                                Ģ -= TransferItemsBetweenInventories(Ē, Ĥ, 0, ğ, 0, ě);
                                continue;
                            }

                            if ((Ģ - ě) * đ < Ġ * ĝ && ě > 5)
                            {
                                var Ě = (Ģ * đ - Ġ * ĝ) / đ;
                                TransferItemsBetweenInventories(Ē, Ĥ, 0, ğ, 0, Ě);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void ę()
        {
            if (ɵ.Count == 0)
            {
                return;
            }

            MyDefinitionId Ę = MyItemType.MakeIngot("Uranium");
            string ė = Ę.ToString();
            double Ė = 0;
            double ċ = 0;
            foreach (
                var Ċ in ɵ)
            {
                Ċ.UseConveyorSystem = false;
                var ñ = h(Ę, Ċ);
                var ü = uraniumAmountLargeGrid;
                if (Ċ.CubeGrid.GridSize == 0.5f)
                {
                    ü =
                        uraniumAmountSmallGrid;
                }

                ċ += ü;
                if (ñ > ü + 0.05)
                {
                    var Ä = W(Ċ, ʓ);
                    if (Ä != null)
                    {
                        var û = ñ - ü;
                        TransferItemsBetweenInventories(ė, Ċ, 0, Ä, 0, û);
                    }
                }
                else if (ñ < ü - 0.05)
                {
                    var f = Z(Ę, true);
                    if (f != null)
                    {
                        var û = ü - ñ;
                        TransferItemsBetweenInventories(ė, f, 0, Ċ, 0, û);
                    }
                }

                Ė += h(Ę, Ċ);
            }

            var ú = Ė / ċ;
            foreach (var ý in ɵ)
            {
                var ù = h(Ę, ý);
                var ø = ú * uraniumAmountLargeGrid;
                if (ý.CubeGrid.GridSize == 0.5f)
                {
                    ø = ú * uraniumAmountSmallGrid;
                }

                if (ù > ø + 0.05)
                {
                    foreach (var ö in ɵ)
                    {
                        if (
                            ý == ö)
                        {
                            continue;
                        }

                        var õ = h(Ę, ö);
                        var ô = ú * uraniumAmountLargeGrid;
                        if (ö.CubeGrid.GridSize == 0.5f)
                        {
                            ô = ú * uraniumAmountSmallGrid;
                        }

                        if (õ < ô - 0.05)
                        {
                            ù = h(Ę, ý);
                            var ó = ô - õ;
                            if (ù - ó >= ø)
                            {
                                TransferItemsBetweenInventories(ė, ý, 0, ö, 0, ó);
                                continue;
                            }

                            if (ù - ó < ø)
                            {
                                ó = ù - ø;
                                TransferItemsBetweenInventories(ė, ý, 0, ö, 0, ó);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private StringBuilder ò(IMyTextSurface q, bool ÿ = true, bool Ă = true, bool ĉ = true, bool Ĉ = true,
            bool ć = true)
        {
            var Ć = false;
            StringBuilder É = new
                StringBuilder();
            if (ÿ)
            {
                É.Append("Isy's Inventory Manager\n");
                É.Append(q.Ň('=', q.ū(É))).Append("\n\n");
            }

            if (Ă && Ņ != null)
            {
                É.Append(
                    "Warning!\n" + Ņ + "\n\n");
                Ć = true;
            }

            if (ĉ)
            {
                É.Append(Ą(q, ʔ, "Ores"));
                É.Append(Ą(q, ʓ, "Ingots"));
                É.Append(Ą(q, ʒ, "Components"));
                É.Append(Ą(q, ʑ,
                    "Tools"));
                É.Append(Ą(q, ʐ, "Ammo"));
                É.Append(Ą(q, _listOfBlocks1, "Bottles"));
                É.Append("=> " + ʄ.Count + " type containers: Balancing " + (
                    balanceTypeContainers ? "ON" : "OFF") + "\n\n");
                Ć = true;
            }

            if (Ĉ)
            {
                É.Append("Managed blocks:\n");
                var ą = q.ū(ɏ.Count.ToString());
                É.Append(ɏ.Count +
                         " Inventories (total) / " + ɑ.Count + " have items to sort\n");
                if (ʕ.Count > 0)
                {
                    É.Append(q.Ň(' ', ą - q.ū(ʕ.Count.ToString())).ToString() + ʕ.Count +
                             " Special Containers\n");
                }

                if (ɿ.Count > 0)
                {
                    É.Append(q.Ň(' ', ą - q.ū(ɿ.Count.ToString())).ToString() + ɿ.Count + " Refineries: ");
                    É.Append(
                        "Ore Balancing " + (enableOreBalancing ? "ON" : "OFF") + "\n");
                }

                if (ɷ.Count > 0)
                {
                    É.Append(q.Ň(' ', ą - q.ū(ɷ.Count.ToString())).ToString() + ɷ.Count +
                             " O2/H2 Generators: ");
                    É.Append("Ice Balancing " + (enableIceBalancing ? "ON" : "OFF") + "\n");
                }

                if (ɵ.Count > 0)
                {
                    É.Append(q.Ň(' ', ą - q.ū(ɵ.Count.ToString
                        ())).ToString() + ɵ.Count + " Reactors: ");
                    É.Append("Uranium Balancing " + (enableUraniumBalancing ? "ON" : "OFF") + "\n");
                }

                if (ɼ.Count > 0)
                {
                    É.Append(q.Ň(' ', ą - q.ū(ɼ.Count.ToString())).ToString() + ɼ.Count + " Assemblers: ");
                    É.Append("Craft " + (
                        enableAutocrafting ? "ON" : "OFF") + " | ");
                    É.Append("Uncraft " + (enableAutodisassembling ? "ON" : "OFF") + " | ");
                    É.Append("Cleanup " + (
                        enableAssemblerCleanup ? "ON" : "OFF") + "\n");
                }

                if (ɸ.Count > 0)
                {
                    É.Append(q.Ň(' ', ą - q.ū(ɸ.Count.ToString())).ToString() + ɸ.Count + " Survival Kits: ");
                    É.Append("Ingot Crafting " + (enableBasicIngotCrafting ? "ON" : "OFF") +
                             (ɿ.Count > 0 ? " (Auto OFF - refineries exist)" : "") + "\n");
                }

                É.Append
                    ("\n");
                Ć = true;
            }

            if (ć && ɗ != "")
            {
                É.Append("Last Action:\n" + ɗ);
                Ć = true;
            }

            if (!Ć)
            {
                É.Append("-- No informations to show --");
            }

            return
                É;
        }

        private StringBuilder Ą(IMyTextSurface q, List<IMyTerminalBlock> ă, string P)
        {
            double ā = 0, Ā = 0;
            foreach (var X in ă)
            {
                var æ = X.GetInventory(0);
                ā += (double) æ.CurrentVolume;
                Ā += (double) æ.MaxVolume;
            }

            var Ë = ă.Count + "x " + P + ":";
            var þ = ā.ž();
            var ĥ = Ā.ž();
            StringBuilder Ĵ = ǖ(q, Ë, ā, Ā, þ, ĥ);
            return Ĵ;
        }

        private void Ń(string ł = null)
        {
            if (ɔ.Count == 0)
            {
                ɤ++;
                return;
            }

            for (var E = ɟ; E < ɔ.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                ɟ
                    ++;
                var į = ɔ[E].Ɗ(mainLCDKeyword);
                foreach (var Į in į)
                {
                    var ĭ = Į.Key;
                    var ĵ = Į.Value;
                    if (!ĭ.GetText().EndsWith("\a"))
                    {
                        ĭ.Font =
                            defaultFont;
                        ĭ.FontSize = defaultFontSize;
                        ĭ.TextPadding = defaultPadding;
                        ĭ.Alignment = TextAlignment.LEFT;
                        ĭ.ContentType = ContentType.TEXT_AND_IMAGE;
                    }

                    var ÿ = ĵ.Ɩ("showHeading");
                    var Ă = ĵ.Ɩ("showWarnings");
                    var ĉ = ĵ.Ɩ("showContainerStats");
                    var Ĉ = ĵ.Ɩ("showManagedBlocks");
                    var ć = ĵ.Ɩ("showLastAction");
                    var ŀ = ĵ.Ɩ("scrollTextIfNeeded");
                    StringBuilder É = new StringBuilder();
                    if (ł != null)
                    {
                        É.Append(ł);
                    }
                    else
                    {
                        É = ò(ĭ, ÿ, Ă, ĉ, Ĉ, ć);
                    }

                    É = ĭ.Ŝ(É, ÿ ? 3 : 0, ŀ);
                    ĭ.WriteText(É.Append("\a"));
                }
            }

            ɤ++;
            ɟ = 0;
        }

        private void Ŀ()
        {
            if (ɓ.Count == 0)
            {
                ɤ++;
                return;
            }

            StringBuilder Ł = new StringBuilder();
            if (ɬ.Count == 0)
            {
                Ł.Append("- No problems detected -");
            }
            else
            {
                var ń = 1;
                foreach (var Ņ in ɬ)
                {
                    Ł.Append(ń +
                             ". " + Ņ.Replace("\n", " ") + "\n");
                    ń++;
                }
            }

            for (var E = ɞ; E < ɓ.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                ɞ++;
                var į = ɓ[E].Ɗ(warningsLCDKeyword);
                foreach (
                    var Į in į)
                {
                    var ĭ = Į.Key;
                    var ĵ = Į.Value;
                    if (!ĭ.GetText().EndsWith("\a"))
                    {
                        ĭ.Font = defaultFont;
                        ĭ.FontSize = defaultFontSize;
                        ĭ.TextPadding = defaultPadding;
                        ĭ.Alignment = TextAlignment.LEFT;
                        ĭ.ContentType = ContentType.TEXT_AND_IMAGE;
                    }

                    var ÿ = ĵ.Ɩ("showHeading");
                    var
                        ŀ = ĵ.Ɩ("scrollTextIfNeeded");
                    StringBuilder É = new StringBuilder();
                    if (ÿ)
                    {
                        É.Append("Isy's Inventory Manager Warnings\n");
                        É.Append(ĭ.Ň('=', ĭ.ū(É))).Append("\n\n");
                    }

                    É.Append(Ł);
                    É = ĭ.Ŝ(É, ÿ ? 3 : 0, ŀ);
                    ĭ.WriteText(É.Append("\a"));
                }
            }

            ɤ++;
            ɞ = 0;
        }

        private void ņ()
        {
            if (ə.Count == 0)
            {
                ɤ++;
                return;
            }

            for (var E = ɝ; E < ə.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                ɝ++;
                var į = ə[E].Ɗ(performanceLCDKeyword);
                foreach (var Į in į)
                {
                    var ĭ = Į.Key;
                    var ĵ = Į.Value;
                    if (!ĭ.GetText().EndsWith("\a"))
                    {
                        ĭ.Font = defaultFont;
                        ĭ.FontSize = defaultFontSize;
                        ĭ.TextPadding =
                            defaultPadding;
                        ĭ.Alignment = TextAlignment.LEFT;
                        ĭ.ContentType = ContentType.TEXT_AND_IMAGE;
                    }

                    var ÿ = ĵ.Ɩ("showHeading");
                    var ŀ = ĵ.Ɩ(
                        "scrollTextIfNeeded");
                    StringBuilder É = new StringBuilder();
                    if (ÿ)
                    {
                        É.Append("Isy's Inventory Manager Performance\n");
                        É.Append(ĭ.Ň('=', ĭ.ū(É))).Append("\n\n");
                    }

                    É.Append(ɫ);
                    É = ĭ.Ŝ(É, ÿ ? 3 : 0, ŀ);
                    ĭ.WriteText(É.Append("\a"));
                }
            }

            ɤ++;
            ɝ = 0;
        }

        private void ľ()
        {
            if (ɣ.Count == 0)
            {
                ɤ++;
                return;
            }

            var Ħ = new Dictionary<IMyTextSurface, string>();
            var Ĳ = new Dictionary<
                IMyTextSurface, string>();
            var ı = new List<IMyTextSurface>();
            var İ = new List<IMyTextSurface>();
            foreach (var
                e in ɣ)
            {
                var į = e.Ɗ(inventoryLCDKeyword);
                foreach (var Į in į)
                {
                    if (Į.Value.Contains(inventoryLCDKeyword + ":"))
                    {
                        Ħ[Į.Key] = Į.Value
                            ;
                        ı.Add(Į.Key);
                    }
                    else
                    {
                        Ĳ[Į.Key] = Į.Value;
                        İ.Add(Į.Key);
                    }
                }
            }

            var ĳ = new HashSet<string>();
            foreach (var ĭ in Ħ)
            {
                ĳ.Add(
                    Regex.Match(ĭ.Value, inventoryLCDKeyword + @":[A-Za-z]+").Value);
            }

            ĳ.RemoveWhere(ī => ī == "");
            List<
                string> Ī = ĳ.ToList();
            for (var E = ɜ; E < Ī.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                ɜ++;
                var ĩ = Ħ.Where(Ĩ => Ĩ.Value.Contains(Ī[E]));
                var ħ = from pair in ĩ
                    orderby Regex.Match(pair.Value, inventoryLCDKeyword + @":\w+").Value
                    select pair;
                IMyTextSurface Ĭ = ħ.ElementAt(0).Key;
                string ĵ = ħ.ElementAt(0).Value;
                StringBuilder É = Ķ(Ĭ, ĵ);
                if (!ĵ.ToLower().Contains("noscroll"))
                {
                    var Ľ = 0
                        ;
                    foreach (var ļ in ħ)
                    {
                        Ľ += ļ.Key.ō();
                    }

                    É = Ĭ.Ŝ(É, 0, true, Ľ);
                }

                var Ļ = É.ToString().Split('\n');
                int ĺ = Ļ.Length;
                var Ĺ = 0;
                int ĸ, ķ;
                foreach (var ļ in ħ)
                {
                    IMyTextSurface ĭ = ļ.Key;
                    ĭ.FontSize = Ĭ.TextureSize.Y / ĭ.TextureSize.Y * Ĭ.FontSize;
                    ĭ.Font = Ĭ.Font;
                    ĭ.TextPadding = Ĭ.TextPadding;
                    ĭ.Alignment = Ĭ.Alignment;
                    ĭ.ContentType = ContentType.TEXT_AND_IMAGE;
                    ĸ = ĭ.ō();
                    ķ = 0;
                    É.Clear();
                    while (Ĺ < ĺ && ķ < ĸ)
                    {
                        É.Append(Ļ[Ĺ] +
                                 "\n");
                        Ĺ++;
                        ķ++;
                    }

                    ĭ.WriteText(É);
                }
            }

            for (var E = ɛ; E < İ.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                ɛ++;
                var ĭ = İ[E];
                var ĵ = Ĳ[ĭ];
                StringBuilder É = Ķ(ĭ, ĵ);
                if (!ĵ.ToLower().Contains("noscroll"))
                {
                    É = ĭ.Ŝ(É, 0);
                }

                ĭ.WriteText(É);
                ĭ.Alignment = TextAlignment.LEFT;
                ĭ.ContentType =
                    ContentType.TEXT_AND_IMAGE;
            }

            ɤ++;
            ɜ = 0;
            ɛ = 0;
        }

        private StringBuilder Ķ(IMyTextSurface q, string ĵ)
        {
            StringBuilder É = new StringBuilder();
            var ð = ĵ.Split('\n').ToList();
            ð.RemoveAll(S => S.StartsWith("@") || S.Length <= 1);
            var Á = true;
            try
            {
                if (ð[0].Length <= 1)
                {
                    Á = false;
                }
            }
            catch
            {
                Á = false;
            }

            if (!Á)
            {
                É.Append("Put an item, type name or Echo command in the custom data.\n\n" +
                         "Examples:\nComponent\nIngot\nSteelPlate\nEcho My cool text\n\n" +
                         "Optionally, add a max amount for the bars as a 2nd parameter.\n\n" +
                         "Example:\nIngot 100000\n\n" +
                         "At last, add any of these 5 modifiers (optional):\n\n" + "'noHeading' to hide the heading\n" +
                         "'singleLine' to force one line per item\n" + "'noBar' to hide the bars\n" +
                         "'noScroll' to prevent the screen from scrolling\n" +
                         "'hideEmpty' to hide items that have an amount of 0\n\n" +
                         "Examples:\nComponent 100000 noBar\nSteelPlate noHeading noBar hideEmpty\n\n" +
                         "To display multiple different items, use a new line for every item!\n" +
                         "Full guide: https://steamcommunity.com/sharedfiles/filedetails/?id=1226261795");
                q.Font = defaultFont;
                q.FontSize = defaultFontSize;
                q.TextPadding = defaultPadding;
            }
            else
            {
                foreach (var À in ð)
                {
                    var º = À.Split(' '
                    );
                    double w = -1;
                    var µ = false;
                    var ª = false;
                    var Â = false;
                    var z = false;
                    if (º.Length >= 2)
                    {
                        try
                        {
                            w = Convert.ToDouble(º[1]);
                        }
                        catch
                        {
                            w = -
                                1;
                        }
                    }

                    string v = À.ToLower();
                    if (v.Contains("noheading"))
                    {
                        µ = true;
                    }

                    if (v.Contains("nobar"))
                    {
                        ª = true;
                    }

                    if (v.Contains("hideempty"))
                    {
                        Â =
                            true;
                    }

                    if (v.Contains("singleline"))
                    {
                        z = true;
                    }

                    if (v.StartsWith("echoc"))
                    {
                        string u = À.Ŷ("echoc ", "").Ŷ("echoc", "");
                        É.Append(q.Ň(' ', (
                            q.Ŋ() - q.ū(u)) / 2)).Append(u + "\n");
                    }
                    else if (v.StartsWith("echor"))
                    {
                        string u = À.Ŷ("echor ", "").Ŷ("echor", "");
                        É.Append(q.Ň(' '
                            , q.Ŋ() - q.ū(u))).Append(u + "\n");
                    }
                    else if (v.StartsWith("echo"))
                    {
                        É.Append(À.Ŷ("echo ", "").Ŷ("echo", "") + "\n");
                    }
                    else
                    {
                        É.Append(
                            r(q, º[0], w, µ, ª, Â, z));
                    }
                }
            }

            return É.Replace("\n", "", 0, 2);
        }

        private StringBuilder r(IMyTextSurface q, string o, double w, bool µ = false,
            bool ª = false, bool Â = false, bool z = false)
        {
            StringBuilder É = new StringBuilder();
            var Í = w == -1 ? true : false;
            foreach (var Ì in Ȝ)
            {
                if (Ì
                    .ToString().ToLower().Contains(o.ToLower()))
                {
                    if (É.Length == 0 && !µ)
                    {
                        var Ë = "Items containing '" + char.ToUpper(o[0]) + o.Substring(1).ToLower() + "'";
                        É.Append("\n" + q.Ň(' ', (q.Ŋ() - q.ū(Ë)) / 2)).Append(Ë + "\n\n");
                    }

                    var k = h(Ì);
                    if (k == 0 && Â)
                    {
                        continue;
                    }

                    if (Í)
                    {
                        w = ơ(
                            Ì);
                    }

                    É.Append(ǖ(q, Ì.SubtypeId.ToString(), k, w, k.Ƃ(), w.Ƃ(), ª, z));
                }
            }

            if (É.Length == 0 && !Â)
            {
                É.Append("Error!\n\n");
                É.Append(
                    "No items containing '" + o +
                    "' found!\nCheck the custom data of this LCD and enter a valid type or item name!\n");
            }

            return É;
        }

        private void Î(string Ê = "")
        {
            ɕ = ɕ >= 3 ? 0 : ɕ + 1;
            Echo("Isy's Inventory Manager " + ɖ[ɕ] + "\n====================\n");
            if (Ņ != null)
            {
                Echo("Warning!\n" + Ņ + "\n");
            }

            StringBuilder É = new StringBuilder();
            É.Append("Script is running in " + (ǭ ? "station" : "ship") + " mode\n\n");
            É.Append("Task: " + Ǧ[_scriptStep] + Ê + "\n")
                ;
            É.Append("Script step: " + _scriptStep + " / " + (Ǧ.Length - 1) + "\n\n");
            É.Append(ƹ);
            if (ʍ.Count > 0)
            {
                É.Append(
                    "Excluded grids:\n============\n\n");
                foreach (var È in ʍ)
                {
                    É.Append(È.CustomName + "\n");
                }
            }

            ɫ = É;
            Echo(É.ToString());
            if (ɔ.Count == 0)
            {
                Echo(
                    "Hint:\nBuild a LCD and add the main LCD\nkeyword '" + mainLCDKeyword +
                    "' to its name to get\nmore informations about your base\nand the current script actions.\n");
            }
        }

        private double TransferItemsBetweenInventories
            (string Æ, IMyTerminalBlock sourceBlock, int Å, IMyTerminalBlock destinationBlock, int m, double amount = -1, bool A = false)
        {
            var sourceInv = sourceBlock.GetInventory(Å);
            var destinationInv = destinationBlock.GetInventory(m);
            if (!sourceInv.IsConnectedTo(destinationInv))
            {
                Log("'" + sourceBlock.CustomName + "'\nis not connected to '" + destinationBlock.CustomName + "'\nItem transfer aborted!");
                return 0;
            }

            if ((float) destinationInv.CurrentVolume > (float) destinationInv.MaxVolume * 0.98f)
            {
                return 0;
            }

            var sourceInvItems = new List<MyInventoryItem>();
            sourceInv.GetItems(sourceInvItems);
            if (sourceInvItems.Count == 0)
            {
                return 0;
            }

            double totalItemAmountTransferred = 0;
            MyDefinitionId itemTypeTemp = new MyDefinitionId();
            MyDefinitionId itemType = new MyDefinitionId();
            var P = "";
            var I
                = "";
            var G = false;
            var inventoryFillType = "";
            if (amount == -0.5)
            {
                inventoryFillType = "halfInventory";
            }

            if (amount == -1)
            {
                inventoryFillType = "completeInventory";
            }

            for (var sourceItemIndex = sourceInvItems.Count - 1; sourceItemIndex >= 0; sourceItemIndex--)
            {
                itemTypeTemp =
                    sourceInvItems[sourceItemIndex].Type;
                if (A ? itemTypeTemp.ToString() == Æ : itemTypeTemp.ToString().Contains(Æ))
                {
                    if (inventoryFillType != "" && itemTypeTemp != itemType)
                    {
                        totalItemAmountTransferred = 0;
                    }

                    itemType = itemTypeTemp;
                    P = itemTypeTemp.TypeId.ToString().Replace(_objectBuilderPrefix, "");
                    I = itemTypeTemp.SubtypeId.ToString();
                    G = true;
                    // if (!sourceInv.CanTransferItemTo(destinationInv, itemTypeTemp))
                    // {
                    //     ƴ("'" + I + "' couldn't be transferred\nfrom '" + f.CustomName + "'\nto '" + Ä.CustomName +
                    //       "'\nThe conveyor type is too small!");
                    //     return 0;
                    // }

                    var sourceInvItemAmount1 = (double) sourceInvItems[sourceItemIndex].Amount;
                    double sourceInvItemAmount2 = 0;
                    if (inventoryFillType == "completeInventory")
                    {
                        sourceInv.TransferItemTo(destinationInv, sourceItemIndex, null, true);
                    }
                    else if (inventoryFillType == "halfInventory")
                    {
                        var sourceInvItemsAmountHalf = Math.Ceiling((double) sourceInvItems[sourceItemIndex].Amount / 2);
                        sourceInv.TransferItemTo(destinationInv, sourceItemIndex, null, true
                            , (MyFixedPoint) sourceInvItemsAmountHalf);
                    }
                    else
                    {
                        if (!P.Contains(ȃ))
                        {
                            amount = Math.Ceiling(amount);
                        }

                        sourceInv.TransferItemTo(destinationInv, sourceItemIndex, null, true, (MyFixedPoint) amount);
                    }

                    sourceInvItems.Clear();
                    sourceInv.GetItems(sourceInvItems);
                    try
                    {
                        if ((MyDefinitionId) sourceInvItems[sourceItemIndex].Type == itemTypeTemp)
                        {
                            sourceInvItemAmount2 = (double) sourceInvItems[sourceItemIndex].Amount;
                        }
                    }
                    catch
                    {
                        sourceInvItemAmount2 = 0;
                    }

                    var itemAmountDelta = sourceInvItemAmount1 - sourceInvItemAmount2;
                    totalItemAmountTransferred += itemAmountDelta;
                    amount -= itemAmountDelta;
                    if
                        (amount <= 0 && inventoryFillType == "")
                    {
                        break;
                    }
                }
            }

            if (!G)
            {
                return 0;
            }

            if (totalItemAmountTransferred > 0)
            {
                var R = Math.Round(totalItemAmountTransferred, 2) + " " + I + " " + P;
                ɗ = "Moved: " + R + "\nfrom: '" + sourceBlock.CustomName +
                    "'\nto: '" + destinationBlock.CustomName + "'";
            }
            else
            {
                var R = Math.Round(amount, 2) + " " + Æ.Replace(_objectBuilderPrefix, "");
                if (inventoryFillType == "completeInventory")
                {
                    R = "all items";
                }

                if (inventoryFillType ==
                    "halfInventory")
                {
                    R = "half of the items";
                }

                Log("Couldn't move '" + R + "'\nfrom '" + sourceBlock.CustomName + "'\nto '" + destinationBlock.CustomName +
                  "'\nCheck conveyor connection and owner/faction!");
            }

            return totalItemAmountTransferred;
        }

        private double h(MyDefinitionId K, IMyTerminalBlock e, int d = 0)
        {
            return (double) e.GetInventory(d).GetItemAmount(K);
            ;
        }

        private IMyTerminalBlock Z(MyDefinitionId K, bool Y = false, IMyTerminalBlock f = null)
        {
            try
            {
                if (ʉ.GetInventory(0).FindItem(K) != null && ʉ != f)
                {
                    return ʉ;
                }
            }
            catch
            {
            }

            foreach (var X in ɑ)
            {
                if (K.SubtypeId.ToString() == "Ice" && X.GetType().ToString().Contains("MyGasGenerator"))
                {
                    continue;
                }

                if (X.GetInventory(0).FindItem(K) != null)
                {
                    ʉ = X;
                    return X;
                }
            }

            if (Y)
            {
                foreach (var X in ʕ)
                {
                    if (f != null)
                    {
                        if (GetBlockPriorityFromName(X) <= GetBlockPriorityFromName(f))
                        {
                            continue;
                        }
                    }

                    if (X.GetInventory(0)
                        .FindItem(K) != null)
                    {
                        ʉ = X;
                        return X;
                    }
                }
            }

            return null;
        }

        private IMyTerminalBlock W(IMyTerminalBlock Q, List<IMyTerminalBlock> V)
        {
            IMyTerminalBlock U = null;
            U = T(Q, V);
            if (U != null)
            {
                return U;
            }

            U = T(Q, ɢ);
            if (U == null)
            {
                Log("'" + Q.CustomName +
                  "'\nhas no empty containers to move its items!");
            }

            return U;
        }

        private IMyTerminalBlock T(IMyTerminalBlock Q, List<IMyTerminalBlock> V)
        {
            var Ó = Q.GetInventory(0);
            foreach (var X in V)
            {
                if (X == Q)
                {
                    continue;
                }

                var æ = X.GetInventory(0);
                if ((float) æ.CurrentVolume < (float) æ.MaxVolume * 0.95f)
                {
                    if (!X.GetInventory(0).IsConnectedTo(Ó))
                    {
                        continue;
                    }

                    return X;
                }
            }

            return null;
        }

        private int GetBlockPriorityFromName(IMyTerminalBlock e)
        {
            var regexMatch = Regex
                .Match(e.CustomName, @"\[p(\d+|max|min)\]", RegexOptions.IgnoreCase)
                .Groups[1].Value.ToLower();
            var priority = 0;
            var successful =
                true;
            if (regexMatch == "max")
            {
                priority = int.MinValue;
            }
            else if (regexMatch == "min")
            {
                priority = int.MaxValue;
            }
            else
            {
                successful = int.TryParse(regexMatch, out priority);
            }

            if (!successful)
            {
                var È = e.IsSameConstructAs(Me) ? "" : "1";
                int.TryParse(È + e.EntityId.ToString().Substring(0, 4), out priority);
            }

            return priority;
        }

        private string á(string Þ)
        {
            ï();
            var à = Storage
                .Split('\n');
            foreach (var À in à)
            {
                if (À.Contains(Þ))
                {
                    return À.Replace(Þ + "=", "");
                }
            }

            return "";
        }

        private void ß(string Þ, string Ý = "")
        {
            ï(
            );
            var à = Storage.Split('\n');
            var ç = "";
            foreach (var À in à)
            {
                if (À.Contains(Þ))
                {
                    ç += Þ + "=" + Ý + "\n";
                }
                else
                {
                    ç += À + "\n";
                }
            }

            Storage = ç
                .TrimEnd('\n');
        }

        private void ï()
        {
            var à = Storage.Split('\n');
            if (à.Length != Ɍ.Count)
            {
                var ç = "";
                foreach (var Ì in Ɍ)
                {
                    ç += Ì.Key + "=" + Ì.Value + "\n";
                }

                Storage = ç.TrimEnd('\n');
            }
        }

        private void í(IMyTerminalBlock X)
        {
            foreach (var ì in Ǯ.Keys.ToList())
            {
                Ǯ[ì] = "0";
            }

            List<string> î = X.CustomData.Replace(" ", "").TrimEnd('\n').Split('\n').ToList();
            î.RemoveAll(À => !À.Contains("=") || À.Length < 8);
            var ë = false;
            foreach (
                var À in î)
            {
                var ê = À.Split('=');
                if (!Ǯ.ContainsKey(ê[0]))
                {
                    MyDefinitionId K;
                    if (MyDefinitionId.TryParse(_objectBuilderPrefix + ê[0], out K))
                    {
                        Ü(K);
                        ë =
                            true;
                    }
                }

                Ǯ[ê[0]] = ê[1];
            }

            if (ë)
            {
                Ð();
            }

            var é = new List<string>
            {
                "Special Container modes:", "",
                "Positive number: stores wanted amount, removes excess (e.g.: 100)",
                "Negative number: doesn't store items, only removes excess (e.g.: -100)",
                "Keyword 'all': stores all items of that subtype (like a type container)", ""
            };
            foreach (var Ì in Ǯ)
            {
                é.Add(Ì.Key + "=" + Ì.Value);
            }

            X.CustomData = string.Join("\n", é);
        }

        private void è()
        {
            ʅ.Clear();
            ʅ.AddRange(Ƞ);
            ʅ.AddRange(ș);
            ʅ.AddRange(ȗ);
            ʅ.AddRange(Ȗ);
            ʅ.AddRange(ȕ);
            ʅ.AddRange(Ȓ);
            Ǯ.Clear();
            foreach (var Ì in Ƞ)
            {
                Ǯ[Ȃ + "/" + Ì] = "0";
            }

            foreach (var Ì
                in ț)
            {
                Ǯ[Ȅ + "/" + Ì] = "0";
            }

            foreach (var Ì in Ț)
            {
                Ǯ[ȃ + "/" + Ì] = "0";
            }

            foreach (var Ì in ș)
            {
                Ǯ[_ammoMagazine + "/" + Ì] = "0";
            }

            foreach (var Ì in ȗ)
            {
                Ǯ[_oxygenContainerObject + "/" +
                  Ì] = "0";
            }

            foreach (var Ì in Ȗ)
            {
                Ǯ[_gasContainerObject + "/" + Ì] = "0";
            }

            foreach (var Ì in ȕ)
            {
                Ǯ[Ș + "/" + Ì] = "0";
            }

            foreach (var Ì in Ȕ)
            {
                Ǯ[ȡ + "/" + Ì] = "0";
            }

            foreach (var Ì in ȓ)
            {
                Ǯ[ȟ + "/" + Ì] = "0";
            }

            foreach (var Ì in Ȓ)
            {
                Ǯ[Ȟ + "/" + Ì] = "0";
            }
        }

        private void Ò()
        {
            for (var E = ɡ; E < ɏ.Count; E++)
            {
                if (ǉ())
                {
                    return;
                }

                if (ɡ
                    >= ɏ.Count - 1)
                {
                    ɡ = 0;
                }
                else
                {
                    ɡ++;
                }

                var M = new List<MyInventoryItem>();
                ɏ[E].GetInventory(0).GetItems(M);
                foreach (var Ì in M)
                {
                    MyDefinitionId K = Ì.Type;
                    if (Ȝ.Contains(K))
                    {
                        continue;
                    }

                    ɗ = "Found new item!\n" + K.SubtypeId.ToString() + " (" + K.TypeId.ToString().Replace(_objectBuilderPrefix, "") +
                        ")";
                    Ú(K);
                    Ü(K);
                    ƭ(K);
                }
            }
        }

        private bool Ð()
        {
            Ö();
            var Ï = Me.CustomData.Split('\n');
            GridTerminalSystem.GetBlocksOfType(ɼ);
            var Ñ = false;
            foreach (var À in Ï)
            {
                var Ø = À.Split(';');
                if (Ø.Length < 2)
                {
                    continue;
                }

                MyDefinitionId K;
                if (!MyDefinitionId.TryParse(Ø[0], out K))
                {
                    continue
                        ;
                }

                if (ɼ.Count == 0)
                {
                    Ñ = true;
                }
                else
                {
                    MyDefinitionId Û;
                    if (MyDefinitionId.TryParse(Ø[1], out Û))
                    {
                        if (Ʈ(Û))
                        {
                            Ǚ(K, Û);
                        }
                        else
                        {
                            Ƶ(K);
                            continue
                                ;
                        }
                    }
                }

                Ú(K);
                Ǖ(K);
            }

            if (Ñ)
            {
                return false;
            }

            return true;
        }

        private void Ú(MyDefinitionId K)
        {
            string P = K.TypeId.ToString().Replace(_objectBuilderPrefix, "");
            string
                I = K.SubtypeId.ToString();
            if (P == Ȅ)
            {
                ț.Add(I);
                Ɲ(I);
                if (!I.Contains("Ice"))
                {
                    foreach (var Ù in ɿ)
                    {
                        if (Ù.GetInventory(0).CanItemsBeAdded(1, K))
                        {
                            ȉ.Add(I);
                            break;
                        }
                    }
                }
            }
            else if (P == ȃ)
            {
                Ț.Add(I);
            }
            else if (P == Ȃ)
            {
                Ƞ.Add(I);
            }
            else if (P == _ammoMagazine)
            {
                ș.Add(I);
            }
            else if (P == _oxygenContainerObject)
            {
                ȗ.Add(I)
                    ;
            }
            else if (P == _gasContainerObject)
            {
                Ȗ.Add(I);
            }
            else if (P == Ș)
            {
                ȕ.Add(I);
            }
            else if (P == ȡ)
            {
                Ȕ.Add(I);
            }
            else if (P == ȟ)
            {
                ȓ.Add(I);
            }
            else if (P == Ȟ)
            {
                Ȓ.Add(I);
            }
        }

        private void Ü(MyDefinitionId K)
        {
            Ö();
            var Ï = Me.CustomData.Split('\n').ToList();
            foreach (var À in Ï)
            {
                try
                {
                    if (À.Substring(0, À.IndexOf(";")) == K.ToString())
                    {
                        return;
                    }
                }
                catch
                {
                }
            }

            for (var E = Ï.Count - 1; E >= 0; E--)
            {
                if (Ï[E].Contains(";"))
                {
                    Ï.Insert(E + 1, K + ";noBP");
                    break;
                }
            }

            Me.CustomData = string.Join("\n", Ï);
            Ǖ(K);
        }

        private void Ö()
        {
            if (!Me.CustomData.Contains(Ɏ))
            {
                Me.CustomData = (ǭ ? Ǭ : ǲ) + Ɏ;
            }
        }

        private void Õ()
        {
            if (Ȍ
                != null)
            {
                var M = new List<MyInventoryItem>();
                Ȍ.GetInventory(1).GetItems(M);
                var Ã = new List<MyProductionItem>();
                Ȍ.GetQueue(Ã);
                if (M.Count == 0)
                {
                    return;
                }

                Ȍ.CustomName = ȋ;
                MyDefinitionId Û = Ã[0].BlueprintId;
                MyDefinitionId K = M[0].Type;
                if (M.Count == 1 && Ã.Count == 1
                                 && Ȍ.Mode == MyAssemblerMode.Assembly && Û == Ȋ)
                {
                    if (ȋ.Contains(learnKeyword) && !ȋ.Contains(learnManyKeyword))
                    {
                        Ȍ.CustomName = ȋ.Replace(" " + learnKeyword, "").Replace(learnKeyword + " ", "");
                    }

                    Ȍ.ClearQueue();
                    Ȋ = new MyDefinitionId();
                    ɗ = "Learned new Blueprint!\n'" + Û
                        .ToString().Replace(_objectBuilderPrefix, "") + "'\nproduces: '" + K.ToString().Replace(_objectBuilderPrefix, "") + "'";
                    Ǖ(K);
                    Ú(K);
                    Ǚ(K, Û);
                    Ü(K);
                    ư(K, Û);
                    Ư(Ȍ);
                    Ȍ = null;
                    return
                        ;
                }

                if (Ã.Count != 1)
                {
                    Log("Blueprint learning aborted!\nExactly 1 itemstack in the queue is needed to learn new recipes!");
                }
            }

            Ȍ = null;
            Ȋ = new MyDefinitionId();
            foreach (var ƞ in ɹ)
            {
                var Ã = new List<MyProductionItem>();
                ƞ.GetQueue(Ã);
                if (Ã.Count == 1 && ƞ.Mode == MyAssemblerMode.Assembly)
                {
                    if (!Ư(ƞ))
                    {
                        return;
                    }

                    Ȍ = ƞ;
                    Ȋ = Ã[0].BlueprintId;
                    ȋ = ƞ.CustomName;
                    ƞ.CustomName = "Learning " + Ȋ.SubtypeName
                                               + " in: " + ƞ.CustomName;
                    return;
                }
            }
        }

        private bool Ư(IMyAssembler ƞ)
        {
            if (ƞ.GetInventory(1).ItemCount != 0)
            {
                var Ä = W(ƞ, ʒ);
                if (Ä
                    != null)
                {
                    TransferItemsBetweenInventories("", ƞ, 1, Ä, 0);
                    return true;
                }

                Log(
                    "Can't learn blueprint!\nNo free containers to clear the output inventory found!");
                return false;
            }

            return true;
        }

        private bool Ʈ(MyDefinitionId Û)
        {
            try
            {
                foreach (var ƞ in ɼ)
                {
                    if (ƞ.CanUseBlueprint(Û))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void ƭ(MyDefinitionId K)
        {
            if (ɼ.Count == 0)
            {
                return;
            }

            if (K.TypeId.ToString() == _objectBuilderPrefix + Ȅ || K.TypeId.ToString() == _objectBuilderPrefix + ȃ)
            {
                return;
            }

            MyDefinitionId Û;
            var ƫ = Ǳ.TryGetValue(K, out Û);
            if (ƫ)
            {
                ƫ = Ʈ(Û);
            }

            if (!ƫ)
            {
                var ƪ = new List<string>
                {
                    "BP", "",
                    "Component", "Magazine", "_Blueprint"
                };
                var Ʃ = false;
                foreach (var ƨ in ƪ)
                {
                    var Ƭ = ȝ + K.SubtypeId.ToString().Replace("Item", "") + ƨ;
                    MyDefinitionId.TryParse(Ƭ, out Û);
                    Ʃ = Ʈ(Û);
                    if (Ʃ)
                    {
                        Ǚ(K, Û);
                        ư(K, Û);
                        ƫ = true;
                        return;
                    }
                }
            }
        }

        private void ư(MyDefinitionId K, MyDefinitionId Û)
        {
            Ö();
            var Ï = Me.CustomData.Split('\n');
            for (var E = 0; E < Ï.Length; E++)
            {
                if (Ï[E].Substring(0, Ï[E].IndexOf(";")) != K.ToString())
                {
                    continue;
                }

                var Ø = Ï[E].Split(
                    ';');
                Ï[E] = Ø[0] + ";" + Û.ToString();
                Me.CustomData = string.Join("\n", Ï);
                return;
            }
        }

        private void Ƶ(MyDefinitionId K)
        {
            Ö();
            var Ï = Me.CustomData
                .Split('\n').ToList();
            Ï.RemoveAll(E => E.Contains(K.ToString() + ";"));
            Me.CustomData = string.Join("\n", Ï);
        }

        private void Log(string ł)
        {
            ɬ.Add(ł);
            ɪ.Add(ł);
            Ņ = ɬ.ElementAt(0);
        }

        private void Ƴ()
        {
            Me.CustomData = "";
            foreach (var X in ʕ)
            {
                List<string> Ï = X.CustomData.Replace(" ", "").TrimEnd('\n').Split('\n').ToList();
                Ï.RemoveAll(À => !À.Contains("=") || À.Contains("=0"));
                X.CustomData = string.Join("\n", Ï);
            }

            Echo(
                "Stored items deleted!\n");
            if (ʕ.Count > 0)
            {
                Echo("Also deleted itemlists of " + ʕ.Count + " Special containers!\n");
            }

            Echo(
                "Please hit 'Recompile'!\n\nScript stopped!");
        }

        private void Ʋ()
        {
            ȑ.Clear();
            List<IMyTerminalBlock> Ʊ = ɺ.ToList<IMyTerminalBlock>();
            List<IMyTerminalBlock> Ƨ = ɾ.ToList<
                IMyTerminalBlock>();
            Ʀ(ɏ, 0);
            Ʀ(Ʊ, 1);
            Ʀ(Ƨ, 1);
        }

        private void Ʀ(List<IMyTerminalBlock> ƙ, int d)
        {
            for (var E = 0; E < ƙ.Count; E++)
            {
                var M = new List<
                    MyInventoryItem>();
                ƙ[E].GetInventory(d).GetItems(M);
                for (var Ɵ = 0; Ɵ < M.Count; Ɵ++)
                {
                    MyDefinitionId K = M[Ɵ].Type;
                    if (ȑ.ContainsKey(K))
                    {
                        ȑ[K] += (
                            double) M[Ɵ].Amount;
                    }
                    else
                    {
                        ȑ[K] = (double) M[Ɵ].Amount;
                    }
                }
            }
        }

        private double h(MyDefinitionId K)
        {
            double Ų;
            ȑ.TryGetValue(K, out Ų);
            return Ų;
        }

        private void Ɲ(string ƛ)
        {
            if (!Ȇ.ContainsKey(ƛ))
            {
                Ȇ[ƛ] = 0.5;
            }
        }

        private double Ɯ(string ƛ)
        {
            double Ų;
            ƛ = ƛ.Replace(_objectBuilderPrefix + Ȅ + "/", "");
            Ȇ.TryGetValue(ƛ, out Ų)
                ;
            return Ų != 0 ? Ų : 0.5;
        }

        private void ƚ()
        {
            Ȁ.Clear();
            foreach (var ƞ in ɼ)
            {
                var Ã = new List<MyProductionItem>();
                ƞ.GetQueue(Ã);
                if (Ã
                    .Count > 0 && !ƞ.IsProducing)
                {
                    if (ƞ.Mode == MyAssemblerMode.Assembly)
                    {
                        Log("'" + ƞ.CustomName +
                          "' has a queue but is currently not assembling!\nAre there enough ingots for the craft?");
                    }

                    if (ƞ.Mode == MyAssemblerMode.Disassembly)
                    {
                        Log("'" + ƞ.CustomName +
                          "' has a queue but is currently not disassembling!\nAre the items to disassemble missing?");
                    }
                }

                foreach (var Ì in Ã)
                {
                    MyDefinitionId Û = Ì.BlueprintId;
                    if (Ȁ.ContainsKey(Û))
                    {
                        Ȁ[Û] += (double) Ì.Amount;
                    }
                    else
                    {
                        Ȁ[Û] = (double) Ì.Amount;
                    }
                }
            }
        }

        private double Ƥ(MyDefinitionId Û)
        {
            double Ų;
            Ȁ.TryGetValue(Û, out Ų);
            return Ų;
        }

        private void ƣ(MyDefinitionId K, double k)
        {
            ǿ[K] = k;
        }

        private double ƥ(MyDefinitionId Û)
        {
            int Ų;
            if (!ǥ.TryGetValue(Û, out Ų))
            {
                Ų = 0;
            }

            return Ų;
        }

        private void Ƣ(MyDefinitionId K, int Ý)
        {
            ǥ[K] = Ý;
        }

        private double ơ(
            MyDefinitionId Û)
        {
            double Ų;
            if (!ǿ.TryGetValue(Û, out Ų))
            {
                Ų = 100000;
            }

            return Ų;
        }

        private MyDefinitionId ƶ(MyDefinitionId K, out bool ƫ)
        {
            MyDefinitionId
                Û;
            ƫ = Ǳ.TryGetValue(K, out Û);
            return Û;
        }

        private MyDefinitionId ǘ(MyDefinitionId Û)
        {
            MyDefinitionId K;
            ǰ.TryGetValue(Û, out K);
            return K;
        }

        private bool Ǘ(MyDefinitionId Û)
        {
            return ǰ.ContainsKey(Û);
        }

        private void Ǚ(MyDefinitionId K, MyDefinitionId Û)
        {
            Ǳ[K] = Û;
            ǰ[Û] = K;
        }

        private void Ǖ(
            MyDefinitionId K)
        {
            Ȝ.Add(K);
            ǯ[K.SubtypeId.ToString()] = K;
        }

        private MyDefinitionId ǔ(string I)
        {
            MyDefinitionId K = new MyDefinitionId();
            ǯ.TryGetValue
                (I, out K);
            return K;
        }

        private StringBuilder ǖ(IMyTextSurface q, string Ë, double Ý, double ǣ, string Ǣ = null, string ǡ = null,
            bool ª = false,
            bool Ǡ = false)
        {
            var þ = Ý.ToString();
            var ĥ = ǣ.ToString();
            if (Ǣ != null)
            {
                þ = Ǣ;
            }

            if (ǡ != null)
            {
                ĥ = ǡ;
            }

            var Ů = q.FontSize;
            var ő = q.Ŋ()
                ;
            var ǟ = ' ';
            var Ǟ = q.ş(ǟ);
            StringBuilder ǝ = new StringBuilder(" " + Ý.ƀ(ǣ));
            ǝ = q.Ň(ǟ, q.ū("99999.9%") - q.ū(ǝ)).Append(ǝ);
            StringBuilder ǜ = new StringBuilder(þ + " / " + ĥ);
            StringBuilder Ǜ = new StringBuilder();
            StringBuilder ǚ = new StringBuilder();
            StringBuilder Ǔ;
            if (ǣ == 0)
            {
                Ǜ.Append(Ë + " ");
                Ǔ = q.Ň(ǟ, ő - q.ū(Ǜ) - q.ū(þ));
                Ǜ.Append(Ǔ).Append(þ);
                return Ǜ.Append("\n");
            }

            double ǃ = 0;
            if (ǣ > 0)
            {
                ǃ = Ý / ǣ >= 1 ? 1 : Ý / ǣ;
            }

            if (Ǡ && !ª)
            {
                if (Ů <= 0.5 || Ů <= 1 && ő > 512)
                {
                    Ǜ.Append(Ʒ(q, ő * 0.25f, ǃ) + " " + Ë);
                    Ǔ = q.Ň(ǟ, ő * 0.75 - q.ū(Ǜ) - q.ū(þ + " /"));
                    Ǜ.Append(Ǔ).Append(ǜ);
                    Ǔ = q.Ň(ǟ, ő - q.ū(Ǜ) - q.ū(ǝ));
                    Ǜ.Append(Ǔ);
                    Ǜ.Append(ǝ);
                }
                else
                {
                    Ǜ.Append(Ʒ(q, ő * 0.3f, ǃ) + " " + Ë);
                    Ǔ = q.Ň(ǟ, ő - q.ū(Ǜ) - q.ū(ǝ));
                    Ǜ.Append(Ǔ);
                    Ǜ.Append(ǝ);
                }
            }
            else
            {
                Ǜ.Append(Ë + " ");
                if (Ů <= 0.6 || Ů <= 1 && ő > 512)
                {
                    Ǔ = q.Ň(ǟ, ő * 0.5 - q.ū(Ǜ) - q.ū(þ + " /"));
                    Ǜ.Append(Ǔ).Append(ǜ)
                        ;
                    Ǔ = q.Ň(ǟ, ő - q.ū(Ǜ) - q.ū(ǝ));
                    Ǜ.Append(Ǔ).Append(ǝ);
                    if (!ª)
                    {
                        ǚ = Ʒ(q, ő, ǃ).Append("\n");
                    }
                }
                else
                {
                    Ǔ = q.Ň(ǟ, ő - q.ū(Ǜ) - q.ū(ǜ));
                    Ǜ.Append(Ǔ
                    ).Append(ǜ);
                    if (!ª)
                    {
                        ǚ = Ʒ(q, ő - q.ū(ǝ), ǃ);
                        ǚ.Append(ǝ).Append("\n");
                    }
                }
            }

            return Ǜ.Append("\n").Append(ǚ);
        }

        private StringBuilder Ʒ(
            IMyTextSurface q, float ů, double ǃ)
        {
            StringBuilder ǂ, ǁ;
            var ǀ = '[';
            var Ǆ = ']';
            var ƿ = 'I';
            var ƽ = '.';
            var Ƽ = q.ş(ǀ);
            var ƻ = q.ş(Ǆ);
            var ƺ
                = ů - Ƽ - ƻ;
            ǂ = q.Ň(ƿ, ƺ * ǃ);
            ǁ = q.Ň(ƽ, ƺ - q.ū(ǂ));
            return new StringBuilder().Append(ǀ).Append(ǂ).Append(ǁ).Append(Ǆ);
        }

        private void ǋ(string Ǒ
        )
        {
            ǌ = ǌ >= 599 ? 0 : ǌ + 1;
            Ǎ = Runtime.CurrentInstructionCount;
            if (Ǎ > ǒ)
            {
                ǒ = Ǎ;
            }

            ƾ[ǌ] = Ǎ;
            Ǐ = ƾ.Sum() / ƾ.Count;
            ƹ.Clear();
            ƹ.Append(
                "Instructions: " + Ǎ + " / " + Runtime.MaxInstructionCount + "\n");
            ƹ.Append("Max. Instructions: " + ǒ + " / " + Runtime.MaxInstructionCount + "\n");
            ƹ.Append("Avg. Instructions: " + Math.Floor(Ǐ) + " / " + Runtime.MaxInstructionCount + "\n\n");
            Ǌ = Runtime.LastRunTimeMs;
            if (Ǌ > ǐ && Ƹ.ContainsKey(Ǒ))
            {
                ǐ = Ǌ;
            }

            ǅ[ǌ] = Ǌ;
            ǎ = ǅ.Sum() / ǅ.Count;
            ƹ.Append("Last runtime: " + Math.Round(Ǌ, 4) + " ms\n");
            ƹ.Append("Max. runtime: " + Math.Round
                (ǐ, 4) + " ms\n");
            ƹ.Append("Avg. runtime: " + Math.Round(ǎ, 4) + " ms\n\n");
            ƹ.Append("Instructions per Method:\n");
            Ƹ[Ǒ] = Ǎ;
            foreach
                (var Ì in Ƹ.OrderByDescending(E => E.Value))
            {
                ƹ.Append("- " + Ì.Key + ": " + Ì.Value + "\n");
            }

            ƹ.Append("\n");
        }

        private bool ǉ(double Ý = 10)
        {
            return Runtime.CurrentInstructionCount > Ý * 1000;
        }

        private List<IMyTerminalBlock> ǈ(string Ɖ, string[] Ǉ = null)
        {
            var ǆ = "[IsyLCD]";
            var Ơ = new
                List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextSurfaceProvider>(Ơ,
                ũ => ũ.IsSameConstructAs(Me) &&
                     (ũ.CustomName.Contains(Ɖ) || ũ.CustomName.Contains(ǆ) && ũ.CustomData.Contains(Ɖ)));
            var œ = Ơ.FindAll(ũ => ũ.CustomName.Contains(Ɖ));
            foreach (var q in œ)
            {
                q.CustomName = q.CustomName.Replace(Ɖ, "").Replace(" " + Ɖ, "").TrimEnd(' ');
                var Ũ = false;
                if (q is IMyTextSurface)
                {
                    if (!q.CustomName.Contains(ǆ))
                    {
                        Ũ = true;
                    }

                    if (!q.CustomData.Contains(Ɖ))
                    {
                        q.CustomData = "@0 " + Ɖ + (Ǉ != null ? "\n" + string.Join("\n", Ǉ) : "");
                    }
                }
                else if (q is IMyTextSurfaceProvider)
                {
                    if (!q.CustomName.Contains(ǆ))
                    {
                        Ũ = true;
                    }

                    var ŧ = (q as IMyTextSurfaceProvider).SurfaceCount;
                    for (var E = 0; E < ŧ; E++)
                    {
                        if (!q.CustomData.Contains("@" + E))
                        {
                            q.CustomData += (q.CustomData == "" ? "" : "\n\n") + "@" + E + " " + Ɖ +
                                            (Ǉ != null ? "\n" + string.Join("\n", Ǉ) : "");
                            break;
                        }
                    }
                }
                else
                {
                    Ơ.Remove(q);
                }

                if (Ũ)
                {
                    q.CustomName += " " + ǆ;
                }
            }

            return Ơ;
        }
    }

    internal class Ŧ : IComparer<MyDefinitionId>
    {
        public int Compare(MyDefinitionId ţ, MyDefinitionId Ţ)
        {
            return ţ.ToString().CompareTo(Ţ.ToString());
        }
    }

    internal class Ť : IEqualityComparer<MyInventoryItem>
    {
        public bool Equals(MyInventoryItem ţ, MyInventoryItem Ţ)
        {
            return ţ.ToString() == Ţ.ToString();
        }

        public int GetHashCode(MyInventoryItem Ì)
        {
            return Ì.ToString().GetHashCode();
        }
    }

    public static partial
        class š
    {
        private static readonly Dictionary<char, float> Š = new Dictionary<char, float>();

        private static DateTime Ş = DateTime.Now;

        private static readonly Dictionary<int, List
            <int>> ŝ = new Dictionary<int, List<int>>();

        public static void ť(string Ū, float Ŭ)
        {
            foreach (
                var ŭ in Ū)
            {
                Š[ŭ] = Ŭ;
            }
        }

        public static void ű()
        {
            if (Š.Count > 0)
            {
                return;
            }

            ť(
                "3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ",
                18);
            ť("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 22);
            ť("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 20);
            ť(
                "￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 21);
            ť("！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 9);
            ť("？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 17);
            ť(
                "（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț", 10);
            ť("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 19);
            ť("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 16);
            ť("\"-rª­ºŀŕŗř", 11);
            ť("WÆŒŴ—…‰", 32);
            ť("'|¦ˉ‘’‚", 7)
                ;
            ť("@©®мшњ", 26);
            ť("mw¼ŵЮщ", 28);
            ť("/ĳтэє", 15);
            ť("\\°“”„", 13);
            ť("*²³¹", 12);
            ť("¾æœЉ", 29);
            ť("%ĲЫ", 25);
            ť("MМШ", 27);
            ť("½Щ", 30);
            ť("ю", 24);
            ť("ј", 8);
            ť("љ", 23);
            ť("ґ", 14);
            ť("™", 31);
        }

        public static Vector2 Ű(this IMyTextSurface ĭ, StringBuilder ł)
        {
            ű();
            var ů = new Vector2();
            if (ĭ.Font == "Monospace")
            {
                var Ů = ĭ.FontSize;
                ů.X = (float) (ł.Length * 19.4 * Ů);
                ů.Y = (float) (28.8 * Ů);
                return ů;
            }
            else
            {
                var Ů = (float) (ĭ.FontSize * 0.779);
                foreach (char ŭ in ł.ToString())
                {
                    try
                    {
                        ů.X += Š[ŭ] * Ů;
                    }
                    catch
                    {
                    }
                }

                ů.Y = (float) (28.8 * ĭ.FontSize)
                    ;
                return ů;
            }
        }

        public static float ū(this IMyTextSurface q, StringBuilder ł)
        {
            var Ō = q.Ű(ł);
            return Ō.X;
        }

        public static float
            ū(this IMyTextSurface q, string ł)
        {
            var Ō = q.Ű(new StringBuilder(ł));
            return Ō.X;
        }

        public static float ş(this
            IMyTextSurface q, char ŏ)
        {
            var Ŏ = ū(q, new string(ŏ, 1));
            return Ŏ;
        }

        public static int ō(this IMyTextSurface q)
        {
            var ŉ = q.SurfaceSize;
            var ň = q.TextureSize.Y;
            ŉ.Y *= 512 / ň;
            var Ő = ŉ.Y * (100 - q.TextPadding * 2) / 100;
            var Ō = q.Ű(new StringBuilder("T"));
            return (int) (Ő /
                          Ō.Y);
        }

        public static float Ŋ(this IMyTextSurface q)
        {
            var ŉ = q.SurfaceSize;
            var ň = q.TextureSize.Y;
            ŉ.X *= 512 / ň;
            return ŉ.X *
                (100 - q.TextPadding * 2) / 100;
        }

        public static StringBuilder Ň(this IMyTextSurface q, char ŋ, double Œ)
        {
            var ř = (int) (Œ / ş(q, ŋ));
            if (
                ř < 0)
            {
                ř = 0;
            }

            return new StringBuilder().Append(ŋ, ř);
        }

        public static StringBuilder Ŝ(this IMyTextSurface q, StringBuilder ł, int ś = 3, bool
            ŀ = true, int ĸ = 0)
        {
            var Ś = q.GetHashCode();
            if (!ŝ.ContainsKey(Ś))
            {
                ŝ[Ś] = new List<int> {1, 3, ś, 0};
            }

            var Ř = ŝ[Ś][0];
            var ŗ = ŝ[Ś][1];
            var
                Ŗ = ŝ[Ś][2];
            var ŕ = ŝ[Ś][3];
            var Ŕ = ł.ToString().TrimEnd('\n').Split('\n');
            var Ļ = new List<string>();
            if (ĸ == 0)
            {
                ĸ = q.ō();
            }

            var ő = q.Ŋ();
            StringBuilder ą, º = new StringBuilder();
            for (var E = 0; E < Ŕ.Length; E++)
            {
                if (E < ś || E < Ŗ || Ļ.Count - Ŗ > ĸ || q.ū(Ŕ[E]) <= ő)
                {
                    Ļ.Add
                        (Ŕ[E]);
                }
                else
                {
                    try
                    {
                        º.Clear();
                        float Ƒ, Ɛ;
                        var Ə = Ŕ[E].Split(' ');
                        var Ǝ = Regex.Match(Ŕ[E],
                            @"\d+(\.|\:)\ ").Value;
                        ą = q.Ň(' ', q.ū(Ǝ));
                        foreach (var ƍ in Ə)
                        {
                            Ƒ = q.ū(º);
                            Ɛ = q.ū(ƍ);
                            if (Ƒ + Ɛ > ő)
                            {
                                Ļ.Add(º.ToString());
                                º = new StringBuilder(ą + ƍ +
                                                      " ");
                            }
                            else
                            {
                                º.Append(ƍ + " ");
                            }
                        }

                        Ļ.Add(º.ToString());
                    }
                    catch
                    {
                        Ļ.Add(Ŕ[E]);
                    }
                }
            }

            if (ŀ)
            {
                if (Ļ.Count > ĸ)
                {
                    if (DateTime.Now.Second != ŕ)
                    {
                        ŕ =
                            DateTime.Now.Second;
                        if (ŗ > 0)
                        {
                            ŗ--;
                        }

                        if (ŗ <= 0)
                        {
                            Ŗ += Ř;
                        }

                        if (Ŗ + ĸ - ś >= Ļ.Count && ŗ <= 0)
                        {
                            Ř = -1;
                            ŗ = 3;
                        }

                        if (Ŗ <= ś && ŗ <= 0)
                        {
                            Ř = 1;
                            ŗ = 3;
                        }
                    }
                }
                else
                {
                    Ŗ = ś;
                    Ř = 1;
                    ŗ = 3;
                }

                ŝ[Ś][
                    0] = Ř;
                ŝ[Ś][1] = ŗ;
                ŝ[Ś][2] = Ŗ;
                ŝ[Ś][3] = ŕ;
            }
            else
            {
                Ŗ = ś;
            }

            StringBuilder ƌ = new StringBuilder();
            for (var À = 0; À < ś; À++)
            {
                ƌ.Append(Ļ[À] + "\n"
                );
            }

            for (var À = Ŗ; À < Ļ.Count; À++)
            {
                ƌ.Append(Ļ[À] + "\n");
            }

            return ƌ;
        }

        public static Dictionary<IMyTextSurface, string> Ɗ(this
            IMyTerminalBlock e, string Ɖ, Dictionary<string, string> ƈ = null)
        {
            var Ƈ = new Dictionary<IMyTextSurface, string>();
            if (e is IMyTextSurface)
            {
                Ƈ[e
                    as IMyTextSurface] = e.CustomData;
            }
            else if (e is IMyTextSurfaceProvider)
            {
                var Ɔ = Regex.Matches(e
                    .CustomData, @"@(\d) *(" + Ɖ + @")");
                var Ƌ = (e as IMyTextSurfaceProvider).SurfaceCount;
                foreach (Match ƒ in Ɔ)
                {
                    var ƕ = -1;
                    if (int.TryParse(ƒ.Groups[1].Value, out ƕ))
                    {
                        if (ƕ >= Ƌ)
                        {
                            continue;
                        }

                        var Ï = e.CustomData;
                        var Ƙ = Ï.IndexOf("@" + ƕ
                        );
                        var Ɨ = Ï.IndexOf("@", Ƙ + 1) - Ƙ;
                        var ĵ = Ɨ <= 0 ? Ï.Substring(Ƙ) : Ï.Substring(Ƙ, Ɨ);
                        Ƈ[(e as IMyTextSurfaceProvider).GetSurface(ƕ)]
                            = ĵ;
                    }
                }
            }

            return Ƈ;
        }

        public static bool Ɩ(this string ĵ, string Þ)
        {
            var Ï = ĵ.Replace(" ", "").Split('\n');
            foreach (var À in Ï)
            {
                if (À
                    .StartsWith(Þ + "="))
                {
                    try
                    {
                        return Convert.ToBoolean(À.Replace(Þ + "=", ""));
                    }
                    catch
                    {
                        return true;
                    }
                }
            }

            return true;
        }

        public static
            string Ɣ(this string ĵ, string Þ)
        {
            var Ï = ĵ.Replace(" ", "").Split('\n');
            foreach (var À in Ï)
            {
                if (À.StartsWith(Þ + "="))
                {
                    return À.Replace(Þ + "=", "");
                }
            }

            return "";
        }
    }

    public static partial class š
    {
        public static bool Ɠ(this double Ý, double ƅ, double ĥ, bool ż = false,
            bool Ż = false)
        {
            var ź = Ż ? Ý > ƅ : Ý >= ƅ;
            var Ź = ż ? Ý < ĥ : Ý <= ĥ;
            return ź && Ź;
        }
    }

    public static partial class š
    {
        public static string Ÿ(this
            char Ž, int ŷ)
        {
            if (ŷ <= 0)
            {
                return "";
            }

            return new string(Ž, ŷ);
        }
    }

    public static partial class š
    {
        public static string Ŷ(this string ŵ
            , string Ŵ, string ų)
        {
            var Ų = Regex.Replace(ŵ, Regex.Escape(Ŵ
            ), ų, RegexOptions.IgnoreCase);
            return Ų;
        }
    }

    public static partial class š
    {
        public static string
            ž(this float Ý)
        {
            var Ƅ = "kL";
            if (Ý < 1)
            {
                Ý *= 1000;
                Ƅ = "L";
            }
            else if (Ý >= 1000 && Ý < 1000000)
            {
                Ý /= 1000;
                Ƅ = "ML";
            }
            else if (Ý >= 1000000 && Ý <
                1000000000)
            {
                Ý /= 1000000;
                Ƅ = "BL";
            }
            else if (Ý >= 1000000000)
            {
                Ý /= 1000000000;
                Ƅ = "TL";
            }

            return Math.Round(Ý, 1) + " " + Ƅ;
        }

        public static string ž(
            this double Ý)
        {
            var ƃ = (float) Ý;
            return ƃ.ž();
        }
    }

    public static partial class š
    {
        public static string Ƃ(this double Ý)
        {
            var Ƅ =
                "";
            if (Ý >= 1000 && Ý < 1000000)
            {
                Ý /= 1000;
                Ƅ = " k";
            }
            else if (Ý >= 1000000 && Ý < 1000000000)
            {
                Ý /= 1000000;
                Ƅ = " M";
            }
            else if (Ý >= 1000000000)
            {
                Ý /=
                    1000000000;
                Ƅ = " B";
            }

            return Math.Round(Ý, 1) + Ƅ;
        }
    }

    public static partial class š
    {
        public static string ƀ(this double ſ, double Ɓ)
        {
            var
                Ô = Math.Round(ſ / Ɓ * 100, 1);
            if (Ɓ == 0)
            {
                return "0%";
            }

            return Ô + "%";
        }

        public static string ƀ(this float ſ, float Ɓ)
        {
            var Ô =
                Math.Round(ſ / Ɓ * 100, 1);
            if (Ɓ == 0)
            {
                return "0%";
            }

            return Ô + "%";
        }

//
// @formatter:off
}}
// @formatter:on
