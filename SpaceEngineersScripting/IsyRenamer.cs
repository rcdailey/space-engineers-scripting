using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

// ReSharper disable UnusedType.Global
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global

// @formatter:off
namespace IsyRenamer  {
public class Program : MyGridProgram  {
// @formatter:on


// Block Renaming Script by Isy
// =======================
// Version: 1.6.1
// Date: 2020-04-27

// Guide: https://steamcommunity.com/sharedfiles/filedetails/?id=2066808115

// =======================================================================================
//                                                                            --- Configuration ---
// =======================================================================================

// Define the character that will be added between names and numbers when sorting or adding strings.
// Default (whitespace): char spaceCharacter = ' ';
        const char spaceCharacter = ' ';

// Add an additional space character after/before addfront/addback?
        bool extraSpace = true;

// Use custom number length?
// By default, the script will determine automatically, how long your numbers should be by getting the total
// amount of blocks of your current filter. If you don't like that, enable this option and adjust your wanted length.
// The length of a number is the amount of digits and will be filled with zeros, if needed.
// Example: Automatic mode in a set of 1000 blocks would produce: 0001, 0010, 0100, 1000
// Example: A custom length of 3 in the same set would produce: 001, 010, 100, 1000
        bool useCustomNumberLength = false;
        int customNumberLength = 0;


// --- Modifier Keywords ---
// =======================================================================================

// Keyword for sorting after performing another operation
        const string sortKeyword = "!sort";

// Keyword for test mode (changes will only be shown but not executed)
        const string testKeyword = "!test";

// Keyword for help (can be added to any command to see the in-script help, example: 'rename,!help')
        const string helpKeyword = "!help";


// --- LCD Panels ---
// =======================================================================================

// Keword for LCD output
// Everything the script writes to the terminal, will also be written on the LCD containing the keyword.
// The keyword will be transformed to my universal [IsyLCD] keyword, once the script has recognized it. That way,
// it's also possible to show the contents on block LCDs. The screen can be changed in the custom data (see guide).
        string mainLCDKeyword = "[IBR-main]";

// Default screen font, fontsize and padding, when a screen is first initialized. Fonts: "Debug" or "Monospace"
        string defaultFont = "Debug";
        float defaultFontSize = 0.6f;
        float defaultPadding = 2f;


// =======================================================================================
//                                                                      --- End of Configuration ---
//                                                        Don't change anything beyond this point!
// =======================================================================================


        bool ć, Ć, ą, Ĉ;
        string Ą = @"( |" + spaceCharacter + @")\d+\b";
        List<IMyTerminalBlock> ĉ = new List<IMyTerminalBlock>();

        List<string> ę
            = new List<string>();

        List<string> ė = new List<string>();
        List<IMyTerminalBlock> Ė = new List<IMyTerminalBlock>();

        List<
            IMyTerminalBlock> ĕ = new List<IMyTerminalBlock>();

        List<IMyCubeGrid> Ĕ = new List<IMyCubeGrid>();
        HashSet<String> ē = new HashSet<String>();

        List<
            string> Ē = new List<string>();

        int đ = 0;
        int Đ = 0;
        int ď = 0;
        DateTime Ď = DateTime.Now;
        int č = 0;

        Program()
        {
            Č();
            J();
            Ĩ();
        }

        void Č()
        {
            ę = new List
                <string>() {"Isy's Block Renaming Script\n======================\n"};
        }

        void Main(string ċ)
        {
            ď++;
            if (Ė.Count > 0)
            {
                ĭ();
                if (!Ĉ)
                    L(
                        "Creating sort list");
                č += Runtime.CurrentInstructionCount;
                Ė.Clear();
                return;
            }

            if (Ĕ.Count > 0)
            {
                į();
                L("Collecting unique names");
                č += Runtime.CurrentInstructionCount;
                if (Đ < Ĕ.Count)
                {
                    return;
                }
                else
                {
                    Ĕ.Clear();
                    Đ = 0;
                    return;
                }
            }

            if (ĕ.Count > 0)
            {
                ĳ();
                if (đ < ĕ.Count)
                {
                    L("Renaming blocks: " + ė.Count);
                    č +=
                        Runtime.CurrentInstructionCount;
                    return;
                }
                else
                {
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    ĕ.Clear();
                    đ = 0;
                    Ĩ();
                    return;
                }
            }

            Č();
            Ď =
                DateTime.Now;
            ď = 0;
            č = 0;
            List<string> Ċ = ċ.Split(',').ToList();
            ć = false;
            Ć = false;
            ą = false;
            Ĉ = false;
            ĉ.Clear();
            ė.Clear();
            if (ċ.Contains(
                testKeyword))
            {
                ć = true;
                Ċ.Remove(testKeyword);
            }

            if (ċ.Contains(sortKeyword))
            {
                Ć = true;
                Ċ.Remove(sortKeyword);
            }

            if (ċ.Contains(helpKeyword))
            {
                Ĉ
                    = true;
                Ċ.Remove(helpKeyword);
            }

            if (Ċ[0] == "undo") ą = true;
            if (Ċ.Count == 0)
            {
                J();
                Ĩ();
                return;
            }

            if (!ć && !ą && !Ĉ && ċ != String.Empty)
                Storage
                    = "";
            if (ċ == String.Empty)
            {
                J();
            }
            else if (Ĉ)
            {
                J(Ċ[0]);
            }
            else if (Ċ[0] == "rename")
            {
                if (Ċ.Count == 3)
                {
                    Ę(Ċ[1], Ċ[2]);
                }
                else if (Ċ.Count >= 4)
                {
                    Ę(Ċ[1], Ċ[2], Ċ[3]);
                }
                else
                {
                    w(Ċ[0], 2);
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "replace")
            {
                if (Ċ.Count == 3)
                {
                    Ā(Ċ[1], Ċ[2]);
                }
                else if (Ċ.Count >= 4)
                {
                    Ā(
                        Ċ[1], Ċ[2], Ċ[3]);
                }
                else
                {
                    w(Ċ[0], 2);
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "remove")
            {
                if (Ċ.Count == 2)
                {
                    ü(Ċ[1]);
                }
                else if (Ċ.Count >= 3)
                {
                    ü(Ċ[1], Ċ[2]
                    );
                }
                else
                {
                    w(Ċ[0], 1);
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "removenumbers")
            {
                if (Ċ.Count == 1)
                {
                    ă();
                }
                else if (Ċ.Count >= 2)
                {
                    ă(Ċ[1]);
                }
            }
            else if (Ċ[0]
                     == "sort")
            {
                if (Ċ.Count >= 2)
                {
                    Ă(Ċ[1]);
                }
                else
                {
                    w(Ċ[0], 1);
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "sortbygrid")
            {
                if (Ċ.Count >= 2)
                {
                    ā(Ċ[1]);
                }
                else
                {
                    w(Ċ[0]
                        , 1);
                    J("sort");
                }
            }
            else if (Ċ[0] == "autosort")
            {
                if (Ċ.Count == 1)
                {
                    Ĳ();
                }
                else if (Ċ.Count >= 2)
                {
                    Ĳ(Ċ[1]);
                }

                if (Ĕ.Count > 0)
                {
                    L(
                        "Getting list of grids");
                    č += Runtime.CurrentInstructionCount;
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    return;
                }
            }
            else if (Ċ[0] == "addfront" ||
                     Ċ[0] == "addback")
            {
                if (Ċ.Count == 2)
                {
                    ĺ(Ċ[0], Ċ[1]);
                }
                else if (Ċ.Count >= 3)
                {
                    ĺ(Ċ[0], Ċ[1], "", Ċ[2]);
                }
                else
                {
                    w(Ċ[0], 1);
                    J(Ċ[0]);
                }
            }
            else if (
                Ċ[0] == "addfrontgrid" || Ċ[0] == "addbackgrid")
            {
                if (Ċ.Count == 3)
                {
                    ĺ(Ċ[0], Ċ[1], Ċ[2]);
                }
                else if (Ċ.Count >= 4)
                {
                    ĺ(Ċ[0], Ċ[1], Ċ[2], Ċ[3]);
                }
                else
                {
                    w(Ċ[0], 2);
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "renamegrid")
            {
                if (Ċ.Count == 2)
                {
                    ķ(Ċ[1]);
                }
                else if (Ċ.Count >= 3)
                {
                    ķ(Ċ[1], Ċ[2]);
                }
                else
                {
                    w(Ċ[0], 1
                    );
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "copydata")
            {
                if (Ċ.Count == 3)
                {
                    ĵ(Ċ[1], Ċ[2]);
                }
                else if (Ċ.Count >= 4)
                {
                    ĵ(Ċ[1], Ċ[2], Ċ[3]);
                }
                else
                {
                    w(Ċ[0], 2);
                    J(Ċ[0]);
                }
            }
            else if (Ċ[0] == "deletedata")
            {
                if (Ċ.Count == 2)
                {
                    Ġ(Ċ[1]);
                }
                else if (Ċ.Count >= 3)
                {
                    Ġ(Ċ[1], Ċ[2]);
                }
                else
                {
                    w(Ċ[0], 1);
                    J(Ċ[0]);
                }
            }
            else if (ą)
            {
                ğ();
            }
            else
            {
                ė.Add("Error!\nUnknown Command!\n");
                J();
            }

            if (Ć)
            {
                ě(ĉ);
            }

            if (Ċ[0] == "renamegrid")
            {
                Ĩ("grids");
            }
            else if (Ċ[0].Contains("data"))
            {
                Ĩ("data");
            }
            else
            {
                Ĩ();
            }
        }

        void Ę(string M, string y, string q = "")
        {
            var t = s(M, q);
            foreach (var m in t)
            {
                string ÿ = m.CustomName;
                string Ã = Ä(ÿ);
                string þ = y + Ã;
                ĉ.Add(m);
                À(ÿ, þ);
                z(m, þ);
            }
        }

        void Ā(string ý, string û, string q = "")
        {
            var t = s(ý, q);
            foreach (var m in
                t)
            {
                string M = m.CustomName;
                string y = M.Replace(ý, û);
                ĉ.Add(m);
                À(M, y);
                z(m, y);
            }
        }

        void ü(string ú, string q = "")
        {
            var t = s(ú, q);
            foreach (var m in t)
            {
                string M = m.CustomName;
                StringBuilder y = new StringBuilder(M);
                y.Replace(ú + " ", "").Replace(" " + ú, "").Replace(ú +
                                                                    spaceCharacter, "").Replace(spaceCharacter + ú, "");
                ĉ.Add(m);
                À(M, y.ToString());
                z(m, y.ToString());
            }
        }

        void ă(string q = "")
        {
            var t = s("", q);
            foreach (var m in t)
            {
                string M = m.CustomName;
                string y = M;
                string Ã = Ä(M);
                if (Ã != "")
                {
                    y = y.Replace(Ã, "");
                }

                ĉ.Add(m);
                À(M, y);
                z(m, y);
            }
        }

        void Ă
            (string q, string p = "")
        {
            var t = s("", q, p);
            ě(t, true);
        }

        void ā(string q)
        {
            var t = s("", q);
            HashSet<IMyCubeGrid> Ě = new HashSet<
                IMyCubeGrid>();
            foreach (var m in t)
            {
                Ě.Add(m.CubeGrid);
            }

            foreach (var p in Ě)
            {
                Ă(q, p.CustomName);
            }
        }

        void Ĳ(string ı = "")
        {
            if (ı != "")
            {
                HashSet
                    <IMyCubeGrid> İ = new HashSet<IMyCubeGrid>();
                List<IMyTerminalBlock> t = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(t);
                foreach (var m in t)
                {
                    if (ı == "all")
                    {
                        İ.Add(m.CubeGrid);
                    }
                    else
                    {
                        if (m.CubeGrid.CustomName.Contains(ı))
                        {
                            İ.Add(m.CubeGrid);
                        }
                    }
                }

                Ĕ = İ.ToList();
            }
            else
            {
                Ĕ = new List<IMyCubeGrid>() {Me.CubeGrid};
            }
        }

        void į()
        {
            ē.Clear();
            Ē.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Ė, C => C.CubeGrid == Ĕ[Đ]);
            Đ++;
            foreach (var m in Ė)
            {
                string Į = Â(m.CustomName);
                Ē.Add(Į);
                ē.Add(Į);
            }
        }

        void ĭ()
        {
            List<string> Ĭ = ē.OrderByDescending(r => r).ToList();
            foreach (var r in Ĭ)
            {
                for (int g = Ē.Count - 1; g >= 0; g--)
                {
                    if (Ē[g] == r)
                    {
                        ĕ.Add(Ė[g]);
                        Ė
                            .RemoveAt(g);
                        Ē.RemoveAt(g);
                    }
                }

                ĕ.Add(null);
                if (Runtime.CurrentInstructionCount > 45000)
                {
                    ė.Add(
                        "Error!\nAutosort had to be stopped! You have too many different block names. Try another filter or simplify your blocknames first!"
                    );
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    Ė.Clear();
                    ĕ.Clear();
                    Ĕ.Clear();
                    ē.Clear();
                    Ē.Clear();
                    đ = 0;
                    Đ = 0;
                    Ĉ = true;
                    Ĩ();
                    return;
                }
            }
        }

        void ĳ()
        {
            List<IMyTerminalBlock> Ĵ = new List<IMyTerminalBlock>();
            for (int g = đ; g < ĕ.Count; g++)
            {
                đ++;
                if (ĕ[g] != null)
                {
                    Ĵ.Add(ĕ[
                        g]);
                }
                else
                {
                    break;
                }
            }

            ě(Ĵ, true);
        }

        void ĺ(string Ĺ, string ĸ, string p = "", string q = "")
        {
            var t = s("", q, p);
            foreach (var m in t)
            {
                string
                    y = "";
                string M = m.CustomName;
                if (Ĺ.Contains("addfront"))
                {
                    y = ĸ + (extraSpace ? spaceCharacter.ToString() : "") + M;
                }

                if (Ĺ.Contains(
                    "addback"))
                {
                    y = M + (extraSpace ? spaceCharacter.ToString() : "") + ĸ;
                }

                ĉ.Add(m);
                À(M, y);
                z(m, y);
            }
        }

        void ķ(string y, string Ķ = "!null")
        {
            if (Ķ ==
                "!null")
            {
                À(Me.CubeGrid.CustomName, y);
                if (!ć) Me.CubeGrid.CustomName = y;
            }
            else
            {
                List<IMyTerminalBlock> t = new List<IMyTerminalBlock>();
                List<IMyCubeGrid> Ě = new List<IMyCubeGrid>();
                GridTerminalSystem.GetBlocks(t);
                foreach (var m in t)
                {
                    if (m.CustomName.Contains(Ķ) &&
                        !Ě.Contains(m.CubeGrid))
                    {
                        Ě.Add(m.CubeGrid);
                        À(m.CubeGrid.CustomName, y);
                        if (!ć) m.CubeGrid.CustomName = y;
                    }
                }
            }
        }

        void ĵ(string Ĝ,
            string q, string p = "")
        {
            var µ = s(Ĝ);
            if (µ.Count == 0)
            {
                ę.Add("Source block not found:\n'" + Ĝ + "'\n");
                return;
            }

            var t = s("", q, p);
            foreach (
                var m in t)
            {
                if (m == µ) continue;
                º(µ[0].CustomName, m.CustomName);
                if (!ć) m.CustomData = µ[0].CustomData;
            }
        }

        void Ġ(string q, string p =
            "")
        {
            var t = s("", q, p);
            foreach (var m in t)
            {
                º("", m.CustomName);
                if (!ć) m.CustomData = "";
            }
        }

        void ğ()
        {
            var ĝ = Storage.Split('\n');
            if (
                Storage.Length == 0)
            {
                ę.Add("No saved operations to undo!\n");
            }
            else
            {
                for (int g = ĝ.Length - 1; g >= 0; g--)
                {
                    var Ĝ = ĝ[g].Split(';');
                    if (Ĝ.Length != 2) continue;
                    long N;
                    if (!long.TryParse(Ĝ[0], out N)) continue;
                    IMyTerminalBlock m = GridTerminalSystem.GetBlockWithId(N);
                    if (m
                        != null)
                    {
                        À(m.CustomName, Ĝ[1]);
                        if (!ć) m.CustomName = Ĝ[1];
                    }
                }
            }
        }

        void ě(List<IMyTerminalBlock> Ğ, bool x = false)
        {
            if (Ğ.Count == 0)
            {
                ė.Add
                    ("Nothing to sort here..");
                return;
            }

            int ġ = Ğ.Count.ToString().Length;
            if (useCustomNumberLength) ġ = customNumberLength;
            Ğ.Sort((
                ī, C) => ī.CustomName.CompareTo(C.CustomName));
            for (int g = 0; g < Ğ.Count; g++)
            {
                string M = Ğ[g].CustomName;
                string Ã = Ä(M);
                string y = M;
                string Ī = "";
                if (Ğ.Count > 1)
                {
                    int ĩ = (g + 1).ToString().Length;
                    Ī = spaceCharacter + l('0', ġ - ĩ) + (g + 1);
                }

                if (Ã != "")
                {
                    y = y.Replace(Ã, Ī);
                }
                else
                {
                    y =
                        y + Ī;
                }

                À(M, y);
                z(Ğ[g], y, x);
            }
        }

        void Ĩ(string ħ = "blocks")
        {
            List<string> Ħ = new List<string>(ę);
            Ħ = Ħ.Concat(ė).ToList();
            int ĥ = ė.Count;
            if (!Ĉ)
            {
                if (ĥ == 0) Ħ.Add("No " + ħ + " found!");
                if (ć)
                {
                    if (ħ == "data")
                    {
                        Ħ.Add("\nTest Mode. No custom data was changed!");
                    }
                    else
                    {
                        Ħ.Add("\nTest Mode. No " + ħ + " were renamed!");
                    }
                }
                else if (ą)
                {
                    Ħ.Add("\nUndid renaming of " + ĥ + " " + ħ + "!");
                }
                else if (ħ == "data")
                {
                    Ħ.Add
                        ("\nChanged the custom data of " + ĥ + " blocks!");
                }
                else
                {
                    Ħ.Add("\nRenamed " + (Ć ? ĥ / 2 : ĥ) + " " + ħ + "!");
                }
            }

            Ħ.Add(
                "\nThis operation took " + (DateTime.Now - Ď).TotalMilliseconds + "ms and " +
                (č + Runtime.CurrentInstructionCount) + " instructions!");
            if (ď > 0)
                Ħ.Add(
                    "The script was restartet " + ď + " times to split the load.");
            string U = String.Join("\n", Ħ);
            var Ĥ = H(mainLCDKeyword);
            for (int g = 0; g < Ĥ.Count; g++)
            {
                var ģ = Ĥ[
                    g].È(mainLCDKeyword);
                foreach (var Ģ in ģ)
                {
                    var V = Ģ.Key;
                    var Õ = Ģ.Value;
                    if (!V.GetText().EndsWith("\a"))
                    {
                        V.Font = defaultFont;
                        V.FontSize = defaultFontSize;
                        V.TextPadding = defaultPadding;
                        V.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
                        V.ContentType =
                            VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    }

                    StringBuilder á = new StringBuilder(U);
                    á = V.ò(á, ð: false);
                    V.WriteText(á.Append("\a"));
                }
            }

            if (Ħ.Count > 100)
            {
                for (int g = 0; g < 50; g++)
                {
                    Echo(Ħ[g]);
                }

                Echo(".\n.\n.");
                for (int g = Ħ.Count - 50; g < Ħ.Count; g++)
                {
                    Echo(Ħ[g
                    ]);
                }
            }
            else
            {
                Echo(U);
            }
        }

        List<IMyTerminalBlock> s(string r = "", string q = "", string p = "")
        {
            List<IMyTerminalBlock> t = new List<
                IMyTerminalBlock>();
            if (q.StartsWith("G:"))
            {
                var o = GridTerminalSystem.GetBlockGroupWithName(q.Substring(2));
                if (o != null)
                {
                    o.GetBlocksOfType<
                        IMyTerminalBlock>(t, C => C.CustomName.Contains(r) && C.CubeGrid.CustomName.Contains(p));
                    ę.Add("Filtered by group:\n" + q.Substring(2) + "\n");
                }
                else
                {
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(t,
                        C => C.CustomName.Contains(r) && C.CubeGrid.CustomName.Contains(p));
                    ę.Add("Not filtered - Group not found!\n");
                }
            }
            else if (q.StartsWith("T:"))
            {
                GridTerminalSystem.GetBlocksOfType<
                    IMyTerminalBlock>(t,
                    C => C.BlockDefinition.ToString().ToLower().Contains(q.Substring(2).ToLower()) &&
                         C.CustomName.Contains(r) && C.CubeGrid.CustomName.Contains(p));
                if (t.Count != 0)
                {
                    HashSet<string> n = new HashSet<string>();
                    ę.Add("Filtered by type:");
                    foreach (var m in t)
                    {
                        if (n.Contains(m.BlockDefinition.ToString())) continue;
                        n.Add(m.BlockDefinition.ToString());
                        ę.Add(m.BlockDefinition.ToString());
                    }

                    ę.Add(
                        "");
                }
                else
                {
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(t,
                        C => C.CustomName.Contains(r) && C.CubeGrid.CustomName.Contains(p));
                    ę.Add("Not filtered - Type not found!\n");
                }
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(t,
                    C => C.CustomName.Contains(r) && C.CustomName.Contains(q) && C.CubeGrid.CustomName.Contains(p));
            }

            return t;
        }

        string l(char v, int Æ)
        {
            if (Æ <= 0)
            {
                return "";
            }

            return new string(v, Æ);
        }

        string Ä(string Á)
        {
            string Ã = System.Text.RegularExpressions.Regex.Match(Á, Ą).Value;
            return Ã ==
                   String.Empty
                ? ""
                : Ã;
        }

        string Â(string Á)
        {
            try
            {
                return Á.Replace(Ä(Á), "");
            }
            catch
            {
                return Á;
            }
        }

        void À(string M, string y)
        {
            ė.Add((ė.Count
                   + 1) + ". " + M + "\n   -> " + y);
        }

        void º(string µ, string ª)
        {
            if (µ == "")
            {
                ė.Add((ė.Count + 1) + ". Data deleted:\n   -> " + ª);
            }
            else
            {
                ė.Add(
                    (ė.Count + 1) + ". Data copied: " + µ + "\n   -> " + ª);
            }
        }

        void z(IMyTerminalBlock m, string y, bool x = true)
        {
            if (!ć)
            {
                if (x)
                    h(m.EntityId,
                        m.CustomName);
                m.CustomName = y;
            }
        }

        void w(string Å, int k)
        {
            ė.Add("Error!\n'" + Å + "' needs at least " + k + " additional parameter" + (
                k > 1 ? "s" : "") + "!\n");
        }

        void h(long N, string M)
        {
            Storage += N + ";" + M + "\n";
        }

        void L(string K)
        {
            Echo(ę[0] + "\n" + K);
        }

        void J(string I = ""
        )
        {
            bool O = false;
            if (I == "")
            {
                ė.Add("Instructions:\n");
                O = true;
            }
            else
            {
                ė.Add("Usage:\n");
            }

            if (I == "" || I == "rename")
            {
                ė.Add(
                    "--- Rename ---");
                ė.Add("Rename a block containing OLDNAME:");
                ė.Add("rename,OLDNAME,NEWNAME[,FILTER]\n");
                O = true;
            }

            if (I == "" || I == "replace")
            {
                ė.Add("--- Replace ---");
                ė.Add("Replace a string with another one:");
                ė.Add("replace,OLDSTRING,NEWSTRING[,FILTER]\n");
                O =
                    true;
            }

            if (I == "" || I == "remove")
            {
                ė.Add("--- Remove ---");
                ė.Add("Remove a string:");
                ė.Add("remove,STRING[,FILTER]\n");
                O = true;
            }

            if (
                I == "" || I == "removenumbers")
            {
                ė.Add("--- Remove Numbers ---");
                ė.Add("Remove numbers from blocknames:");
                ė.Add(
                    "removenumbers[,FILTER]\n");
                O = true;
            }

            if (I == "" || I == "sort")
            {
                ė.Add("--- Sort ---");
                ė.Add("Create new continuous numbers:");
                ė.Add("sort,FILTER\n");
                ė.Add("Create new numbers based on the grid:");
                ė.Add("sortbygrid,FILTER\n");
                O = true;
            }

            if (I == "" || I == "autosort")
            {
                ė.Add(
                    "--- Autosort ---");
                ė.Add("Autosort all blocks on your grid with automatic numbers:");
                ė.Add("autosort\n");
                ė.Add(
                    "Autosort all blocks on a specific grid:");
                ė.Add("autosort,GRIDNAME\n");
                ė.Add("Autosort every connected grid:");
                ė.Add("autosort,all\n");
                O = true;
            }

            if (I == "" || I ==
                "addfront" || I == "addback")
            {
                ė.Add("--- Add strings ---");
                ė.Add("Add a string at the front or back:");
                ė.Add(
                    "addfront,STRING[,FILTER]");
                ė.Add("addback,STRING[,FILTER]\n");
                O = true;
            }

            if (I == "" || I == "addfrontgrid" || I == "addbackgrid")
            {
                ė.Add(
                    "--- Add strings on a grid ---");
                ė.Add("Add a string at the front or back of blocknames on a grid:");
                ė.Add("addfrontgrid,STRING,GRIDNAME[,FILTER]");
                ė.Add("addbackgrid,STRING,GRIDNAME[,FILTER]\n");
                O = true;
            }

            if (I == "" || I == "renamegrid")
            {
                ė.Add("--- Rename grid ---");
                ė.Add(
                    "Rename a grid:");
                ė.Add("renamegrid,NEWNAME[,BLOCKONGRID]\n");
                O = true;
            }

            if (I == "" || I == "copydata")
            {
                ė.Add("--- Copy Custom Data ---");
                ė.Add(
                    "Copy the custom data from one block to another:");
                ė.Add("copydata,BLOCKNAME,FILTER[,GRIDNAME]\n");
                O = true;
            }

            if (I == "" || I == "deletedata")
            {
                ė.Add("--- Delete Custom Data ---")
                    ;
                ė.Add("Delete the custom data of on or more blocks:");
                ė.Add("deletedata,FILTER[,GRIDNAME]\n");
                O = true;
            }

            if (I == "" || I ==
                "undo")
            {
                ė.Add("--- Undo last operation ---");
                ė.Add("Mistakes are made. Undo your last operation with this:");
                ė.Add("undo\n");
                O
                    = true;
            }

            if (!O)
            {
                ė.Add("--- Error ---");
                ė.Add("No topic with the given name exists!");
            }

            if (O)
            {
                ė.Add("Filters:");
                ė.Add(
                    "Can be either a part of a blockname");
                ė.Add("Or a group with the group token 'G:'");
                ė.Add("Or a type with the type token 'T:'\n");
                ė.Add(
                    "Additional parameters:");
                ė.Add("Test before renaming:\n" + testKeyword);
                ė.Add("Sort after renaming:\n" + sortKeyword);
            }

            Ĉ = true;
        }

        List<
            IMyTerminalBlock> H(string G, string[] F = null)
        {
            string E = "[IsyLCD]";
            var D = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<
                IMyTextSurfaceProvider>(D,
                C => C.IsSameConstructAs(Me) &&
                     (C.CustomName.Contains(G) || (C.CustomName.Contains(E) && C.CustomData.Contains(G))));
            var B =
                D.FindAll(C => C.CustomName.Contains(G));
            foreach (var A in B)
            {
                A.CustomName = A.CustomName.Replace(G, "").Replace(" " + G, "").TrimEnd(' ');
                bool Q = false;
                if (A is IMyTextSurface)
                {
                    if (!A.CustomName.Contains(E)) Q = true;
                    if (!A.CustomData.Contains(G))
                        A.CustomData
                            = "@0 " + G + (F != null ? "\n" + String.Join("\n", F) : "");
                }
                else if (A is IMyTextSurfaceProvider)
                {
                    if (!A.CustomName.Contains(E)) Q = true;
                    int j = (A as IMyTextSurfaceProvider).SurfaceCount;
                    for (int g = 0; g < j; g++)
                    {
                        if (!A.CustomData.Contains("@" + g))
                        {
                            A.CustomData += (A.CustomData == "" ? "" : "\n\n") + "@" + g + " " + G +
                                            (F != null ? "\n" + String.Join("\n", F) : "");
                            break;
                        }
                    }
                }
                else
                {
                    D.Remove(A);
                }

                if (Q) A.CustomName += " " + E;
            }

            return D;
        }
    }

    public static partial class f
    {
        private static Dictionary<char, float> e = new Dictionary<char, float>();

        public static void d(
            string Z, float Y)
        {
            foreach (char R in Z)
            {
                e[R] = Y;
            }
        }

        public static void X()
        {
            if (e.Count > 0) return;
            d(
                "3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ",
                18);
            d("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 22);
            d("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 20);
            d(
                "￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 21);
            d("！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 9);
            d("？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 17);
            d(
                "（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț", 10);
            d("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 19);
            d("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 16);
            d("\"-rª­ºŀŕŗř", 11);
            d("WÆŒŴ—…‰", 32);
            d("'|¦ˉ‘’‚", 7)
                ;
            d("@©®мшњ", 26);
            d("mw¼ŵЮщ", 28);
            d("/ĳтэє", 15);
            d("\\°“”„", 13);
            d("*²³¹", 12);
            d("¾æœЉ", 29);
            d("%ĲЫ", 25);
            d("MМШ", 27);
            d("½Щ", 30);
            d("ю", 24);
            d("ј", 8);
            d("љ", 23);
            d("ґ", 14);
            d("™", 31);
        }

        public static Vector2 W(this IMyTextSurface V, StringBuilder U)
        {
            X();
            Vector2 T = new Vector2();
            if (V.Font == "Monospace")
            {
                float S = V.FontSize;
                T.X = (float) (U.Length * 19.4 * S);
                T.Y = (float) (28.8 * S);
                return T;
            }
            else
            {
                float S = (float) (V.FontSize * 0.779);
                foreach (char R in U.ToString())
                {
                    try
                    {
                        T.X += e[R] * S;
                    }
                    catch
                    {
                    }
                }

                T.Y = (float) (28.8 * V.FontSize)
                    ;
                return T;
            }
        }

        public static float P(this IMyTextSurface A, StringBuilder U)
        {
            Vector2 ã = A.W(U);
            return ã.X;
        }

        public static float
            P(this IMyTextSurface A, string U)
        {
            Vector2 ã = A.W(new StringBuilder(U));
            return ã.X;
        }

        public static float ê(this
            IMyTextSurface A, char é)
        {
            float è = P(A, new string(é, 1));
            return è;
        }

        public static int ç(this IMyTextSurface A)
        {
            Vector2 æ = A.SurfaceSize;
            float å = A.TextureSize.Y;
            æ.Y *= 512 / å;
            float ä = æ.Y * (100 - A.TextPadding * 2) / 100;
            Vector2 ã = A.W(new StringBuilder("T"));
            return (int) (ä /
                          ã.Y);
        }

        public static float â(this IMyTextSurface A)
        {
            Vector2 æ = A.SurfaceSize;
            float å = A.TextureSize.Y;
            æ.X *= 512 / å;
            return æ.X *
                (100 - A.TextPadding * 2) / 100;
        }

        public static StringBuilder ù(this IMyTextSurface A, char ø, double ö)
        {
            int õ = (int) (ö / ê(A, ø));
            if (
                õ < 0) õ = 0;
            return new StringBuilder().Append(ø, õ);
        }

        private static DateTime ô = DateTime.Now;

        private static Dictionary<int, List
            <int>> ó = new Dictionary<int, List<int>>();

        public static StringBuilder ò(this IMyTextSurface A, StringBuilder U, int ñ = 3, bool
            ð = true, int ï = 0)
        {
            int î = A.GetHashCode();
            if (!ó.ContainsKey(î))
            {
                ó[î] = new List<int> {1, 3, ñ, 0};
            }

            int í = ó[î][0];
            int ì = ó[î][1];
            int
                ë = ó[î][2];
            int à = ó[î][3];
            var Ò = U.ToString().TrimEnd('\n').Split('\n');
            List<string> Þ = new List<string>();
            if (ï == 0) ï = A.ç();
            float Ð = A.â();
            StringBuilder Ï, Î = new StringBuilder();
            for (int g = 0; g < Ò.Length; g++)
            {
                if (g < ñ || g < ë || Þ.Count - ë > ï || A.P(Ò[g]) <= Ð)
                {
                    Þ.Add
                        (Ò[g]);
                }
                else
                {
                    try
                    {
                        Î.Clear();
                        float Í, Ì;
                        var Ñ = Ò[g].Split(' ');
                        string Ã = System.Text.RegularExpressions.Regex.Match(Ò[g],
                            @"\d+(\.|\:)\ ").Value;
                        Ï = A.ù(' ', A.P(Ã));
                        foreach (var Ë in Ñ)
                        {
                            Í = A.P(Î);
                            Ì = A.P(Ë);
                            if (Í + Ì > Ð)
                            {
                                Þ.Add(Î.ToString());
                                Î = new StringBuilder(Ï + Ë +
                                                      " ");
                            }
                            else
                            {
                                Î.Append(Ë + " ");
                            }
                        }

                        Þ.Add(Î.ToString());
                    }
                    catch
                    {
                        Þ.Add(Ò[g]);
                    }
                }
            }

            if (ð)
            {
                if (Þ.Count > ï)
                {
                    if (DateTime.Now.Second != à)
                    {
                        à =
                            DateTime.Now.Second;
                        if (ì > 0) ì--;
                        if (ì <= 0) ë += í;
                        if (ë + ï - ñ >= Þ.Count && ì <= 0)
                        {
                            í = -1;
                            ì = 3;
                        }

                        if (ë <= ñ && ì <= 0)
                        {
                            í = 1;
                            ì = 3;
                        }
                    }
                }
                else
                {
                    ë = ñ;
                    í = 1;
                    ì = 3;
                }

                ó[î][
                    0] = í;
                ó[î][1] = ì;
                ó[î][2] = ë;
                ó[î][3] = à;
            }
            else
            {
                ë = ñ;
            }

            StringBuilder Ê = new StringBuilder();
            for (var É = 0; É < ñ; É++)
            {
                Ê.Append(Þ[É] + "\n"
                );
            }

            for (var É = ë; É < Þ.Count; É++)
            {
                Ê.Append(Þ[É] + "\n");
            }

            return Ê;
        }

        public static Dictionary<IMyTextSurface, string> È(this
            IMyTerminalBlock m, string G, Dictionary<string, string> Ç = null)
        {
            var Ó = new Dictionary<IMyTextSurface, string>();
            if (m is IMyTextSurface)
            {
                Ó[m
                    as IMyTextSurface] = m.CustomData;
            }
            else if (m is IMyTextSurfaceProvider)
            {
                var ß = System.Text.RegularExpressions.Regex.Matches(m
                    .CustomData, @"@(\d) *(" + G + @")");
                int Ý = (m as IMyTextSurfaceProvider).SurfaceCount;
                foreach (System.Text.RegularExpressions.
                    Match Ü in ß)
                {
                    int Û = -1;
                    if (int.TryParse(Ü.Groups[1].Value, out Û))
                    {
                        if (Û >= Ý) continue;
                        string u = m.CustomData;
                        int Ú = u.IndexOf("@" + Û
                        );
                        int Ù = u.IndexOf("@", Ú + 1) - Ú;
                        string Õ = Ù <= 0 ? u.Substring(Ú) : u.Substring(Ú, Ù);
                        Ó[(m as IMyTextSurfaceProvider).GetSurface(Û)]
                            = Õ;
                    }
                }
            }

            return Ó;
        }

        public static bool Ø(this string Õ, string Ô)
        {
            var u = Õ.Replace(" ", "").Split('\n');
            foreach (var É in u)
            {
                if (É
                    .StartsWith(Ô + "="))
                {
                    try
                    {
                        return Convert.ToBoolean(É.Replace(Ô + "=", ""));
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
            string Ö(this string Õ, string Ô)
        {
            var u = Õ.Replace(" ", "").Split('\n');
            foreach (var É in u)
            {
                if (É.StartsWith(Ô + "="))
                {
                    return É.Replace(Ô + "=", "");
                }
            }

            return "";
        }

// @formatter:off
}}
// @formatter:on
