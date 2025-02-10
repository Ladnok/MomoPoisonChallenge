using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.Model;
using System.Drawing.Drawing2D;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.UI.Components
{
    public class MomoPoisonChallenge : IComponent
    {
        #region Variables
        #region Basic
        private SimpleLabel Label;
        private LiveSplitState CurrentState { get; set; }
        private MomoPoisonChallengeSettings Settings { get; set; }

        public string ComponentName => "Momodora Poison Challenge";

        public float HorizontalWidth { get; set; }
        public float MinimumHeight => 10;
        public float VerticalHeight { get; set; }
        public float MinimumWidth => 200;

        public float PaddingTop => 1;
        public float PaddingBottom => 1;
        public float PaddingLeft => 1;
        public float PaddingRight => 1;
        #endregion

        #region Component
        private const string VERSION_1_05b = "1.05b", VERSION_1_07 = "1.07";
        private const string PROCESS_NAME = "MomodoraRUtM";
        private Process GameProcess = null;
        private string GameVersion = "";
        private MemoryWatcherList Watchers = new MemoryWatcherList();
        private Dictionary<string, int[]> Offsets = new Dictionary<string, int[]>();
        private Dictionary<(double, double), string[]> MapToBoss = new Dictionary<(double, double), string[]>()
        {
            { (36, 17), new[] { "Edea" }},
            { (37, 17), new[] { "Edea" }},
            { (45, 18), new[] { "Moka", "Lubella_One" }},
            { (45, 20), new[] { "Moka", "Lubella_One" }},
            { (43, 22), new[] { "Frida" }},
            { (44, 22), new[] { "Frida" }},
            { (58, 29), new[] { "Lubella_Two" }},
            { (66, 34), new[] { "Arsonist" }},
            { (67, 34), new[] { "Arsonist" }},
            { (71, 16), new[] { "Fennel" }},
            { (48, 12), new[] { "Lupiar", "Magnolia" }},
            { (64, 1),  new[] { "Queen" }},
            { (66, 20), new[] { "Choir" }},
            { (67, 20), new[] { "Choir" }},
            { (68, 20), new[] { "Choir" }},
        };
        private List<string> currentBoss = new List<string>();
        #endregion
        #endregion

        #region Logic
        #region Events
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (IsProcessRunning(GameProcess, invalidator, width, height))
            {
                var NewGameVersion = GetGameVersion(invalidator, width, height);
                if (NewGameVersion == "")
                {
                    return;
                }

                // Generate new watchers if game version changed
                if (GameVersion != NewGameVersion)
                {
                    GameVersion = NewGameVersion;

                    PrepareOffsets();
                    CreateMemoryWatchers();
                }

                // If In_Game is not on the list or the value is different than 1 exit function
                var inGameWatcher = Watchers.FirstOrDefault(w => w.Name == "In_Game");
                if (inGameWatcher == null)
                {
                    return;
                }

                inGameWatcher.Update(GameProcess);

                if (!(inGameWatcher.Current is double inGame) || inGame != 1)
                {
                    GameVersion = "";
                    return;
                }

                // If player is in game update the other Watchers
                Watchers.UpdateAll(GameProcess);

                // If Inventory_Open is not on the list or the value is different than 0 exit function
                var inventoryOpenWatcher = Watchers.FirstOrDefault(w => w.Name == "Inventory_Open");
                if (inventoryOpenWatcher == null || !(inventoryOpenWatcher.Current is double inventoryOpen) || inventoryOpen != 0)
                {
                    return;
                }

                // 500 is an arbitrary number, any number greater than 0 works (but numbers close to 0 cause issues)
                SetValue("Poison_Remaining", 500.0);

                // Update boss reward if the player wasn't hit (has to be here to avoid issues with cutscenes)
                if (!Watchers["Player_Health"].Enabled)
                {
                    return;
                }

                foreach (var boss in currentBoss)
                {
                    SetValue(boss, 1.0);
                }
            }
            else
            {
                GameProcess = GetProcess(PROCESS_NAME);
                GameVersion = "";
            }
        }

        public void Dispose() { }
        #endregion

        #region Basic
        public IDictionary<string, Action> ContextMenuControls => null;

        public MomoPoisonChallenge(LiveSplitState state)
        {
            Label = new SimpleLabel();
            Settings = new MomoPoisonChallengeSettings();

            CurrentState = state;
        }

        public void prepareDraw(LiveSplitState state)
        {
            Label.Font = Settings.OverrideTextFont ? Settings.TextFont : state.LayoutSettings.TextFont;
            Label.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            Label.OutlineColor = Settings.OverrideTextColor ? Settings.OutlineColor : state.LayoutSettings.TextOutlineColor;
            Label.ShadowColor = Settings.OverrideTextColor ? Settings.ShadowColor : state.LayoutSettings.ShadowsColor;

            Label.VerticalAlignment = StringAlignment.Center;
            Label.HorizontalAlignment = StringAlignment.Center;
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) => throw new NotImplementedException();

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            var textHeight = g.MeasureString("A", state.LayoutSettings.TextFont).Height;
            VerticalHeight = textHeight * 1.5f;

            prepareDraw(state);
            Label.SetActualWidth(g);
            Label.Width = Label.ActualWidth;
            Label.Height = VerticalHeight;
            Label.X = width - PaddingRight - Label.Width;
            Label.Y = 3f;

            DrawBackground(g, width, VerticalHeight);

            Label.Draw(g);
        }

        private void DrawBackground(Graphics g, float width, float height)
        {
            if (Settings.BackgroundColor.A > 0
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.A > 0)
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public XmlNode GetSettings(XmlDocument document) => Settings.GetSettings(document);

        public Control GetSettingsControl(LayoutMode mode) => Settings;

        public void SetSettings(XmlNode settings) => this.Settings.SetSettings(settings);

        /// <summary>
        /// Changes the text of the RandomizerLabel and invalidates the state if it's different.
        /// </summary>
        /// <param name="newString">The text to set RandomizerLabel to.</param>
        /// <param name="invalidator">Component Invalidator.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        private void SetSimpleLabelText(string newString, IInvalidator invalidator = null, float width = 0, float height = 0)
        {
            if (Label.Text != newString)
            {
                Label.Text = newString;
                invalidator?.Invalidate(0, 0, width, height);
            }
        }

        /// <summary>
        /// Checks if a process with the specified name is currently running and returns a reference.
        /// </summary>
        /// <param name="processName">The name of the process to check.</param>
        /// <returns>
        ///     A <see cref="Process"/> reference if the process is running; otherwise, <c>null</c>.
        /// </returns>
        private Process GetProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            Process process = null;

            if (processes.Length > 0)
            {
                process = processes[0];
            }

            return process;
        }

        /// <summary>
        /// Checks if the specified process is currently running.
        /// </summary>
        /// <param name="process">The reference to the process to check.</param>
        /// <param name="invalidator">Component Invalidator.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        ///     <c>true</c> if the process is running; otherwise, <c>false</c>.
        /// </returns>
        private bool IsProcessRunning(Process process, IInvalidator invalidator = null, float width = 0, float height = 0)
        {
            bool Running = process != null && !process.HasExited;

            if (invalidator != null && !Running)
            {
                SetSimpleLabelText("Game not running", invalidator, width, height);
            }

            return Running;
        }
        #endregion

        #region Component
        /// <summary>
        /// Adds a collection of offsets to a dictionary based on the current game version.
        /// </summary>
        private void PrepareOffsets()
        {
            // Clear Offsets list
            Offsets.Clear();

            switch (GameVersion)
            {
                case VERSION_1_07:
                    Offsets.Add("In_Game",              new int[] { 0x2379600, 0x0,   0x4,   0x5B0 });
                    Offsets.Add("Inventory_Open",       new int[] { 0x2371EA8, 0x4,   0xAD0  });
                    Offsets.Add("Map_X",                new int[] { 0x2371EA8, 0x4,   0x7B0  });
                    Offsets.Add("Map_Y",                new int[] { 0x2371EA8, 0x4,   0x7C0  });
                    Offsets.Add("Player_Health",        new int[] { 0x2371EA8, 0x4,   0x0    });
                    Offsets.Add("Poison_Remaining",     new int[] { 0x25A2B3C, 0xC,   0xBC,  0x8,   0x4,   0xAC0 });
                    Offsets.Add("Edea",                 new int[] { 0x237E54C, 0x4,   0x140, 0x4,   0x1460 });
                    Offsets.Add("Moka",                 new int[] { 0x237C39C, 0x84,  0x140, 0x4,   0x1460 });
                    Offsets.Add("Lubella_One",          new int[] { 0x237E54C, 0xC,   0x13C, 0x4,   0x1460 });
                    Offsets.Add("Frida",                new int[] { 0x237E54C, 0x34,  0x13C, 0x4,   0x1460 });
                    Offsets.Add("Lubella_Two",          new int[] { 0x236FE44, 0x0,   0x0,   0x4,   0x1460 });
                    Offsets.Add("Arsonist",             new int[] { 0x2332CB4, 0x318, 0xC,   0x13C, 0x13C, 0x13C, 0x4, 0x1460 });
                    Offsets.Add("Fennel",               new int[] { 0x236FE44, 0x0,   0x0,   0x4,   0x1460 });
                    Offsets.Add("Lupiar",               new int[] { 0x2332CB4, 0x8E0, 0xC,   0x13C, 0x4,   0x1460 });
                    Offsets.Add("Magnolia",             new int[] { 0x236FE44, 0x0,   0x0,   0x4,   0x1460 });
                    Offsets.Add("Queen",                new int[] { 0x236FE44, 0x0,   0x4C,  0x298, 0x13C, 0x298, 0x140, 0x140, 0x4, 0x1460 });
                    Offsets.Add("Choir",                new int[] { 0x236FE44, 0x0,   0x0,   0x4,   0x1460 });
                    break;
                case VERSION_1_05b:
                    //Offsets.Add("In_Game",              new int[] { 0x230C440, 0x0,   0x4,   0x780 });
                    //Offsets.Add("Inventory_Open",       new int[] { 0x2304CE8, 0x4,   0xAC0  });
                    //Offsets.Add("Map_X",                new int[] { 0x2304CE8, 0x4,   0x7C0  });
                    //Offsets.Add("Map_Y",                new int[] { 0x2304CE8, 0x4,   0x7B0  });
                    //Offsets.Add("Player_Health",        new int[] { 0x2304CE8, 0x4,   0x0    });
                    //Offsets.Add("Poison_Remaining",     new int[] { 0x253597C, 0xC,   0xBC,  0x8,   0x4,   0xAC0 });
                    //Offsets.Add("Edea",                 new int[] { 0x231138C, 0x4,   0x140, 0x4,   0x1450 });
                    //Offsets.Add("Moka",                 new int[] { 0x230F1DC, 0x84,  0x140, 0x4,   0x1450 });
                    //Offsets.Add("Lubella_One",          new int[] { 0x231138C, 0xC,   0x13C, 0x4,   0x1450 });
                    //Offsets.Add("Frida",                new int[] { 0x231138C, 0x34,  0x13C, 0x4,   0x1450 });
                    //Offsets.Add("Lubella_Two",          new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    //Offsets.Add("Arsonist",             new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    //Offsets.Add("Fennel",               new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    //Offsets.Add("Lupiar",               new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    //Offsets.Add("Magnolia",             new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    //Offsets.Add("Queen",                new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    //Offsets.Add("Choir",                new int[] { 0x2302C84, 0x0,   0x0,   0x4,   0x1450 });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Create memory watchers to keep track of values in the games memory, and execute code based on those values
        /// </summary>
        private void CreateMemoryWatchers()
        {
            string WatcherName;

            // Clear Watchers list
            Watchers.Clear();

            // InGame and InventoryOpen are only used to check if the poison should be applied. No action needed when the values are updated
            WatcherName = "In_Game";
            Watchers.Add(WatcherUtility.CreateWatcher<double>(WatcherName, WatcherUtility.CreateDeepPointer(GameProcess, Offsets[WatcherName])));

            WatcherName = "Inventory_Open";
            Watchers.Add(WatcherUtility.CreateWatcher<double>(WatcherName, WatcherUtility.CreateDeepPointer(GameProcess, Offsets[WatcherName])));

            // Player location is needed to ensure the "get item" pointers are checked at the correct time. We only enable the correct pointer to avoid possible interference between each other
            WatcherName = "Map_X";
            Watchers.Add(WatcherUtility.CreateWatcher<double>(WatcherName, WatcherUtility.CreateDeepPointer(GameProcess, Offsets[WatcherName]), (old, current) =>
            {
                var Map_Y = Watchers["Map_Y"];
                Debug.WriteLine("Map_Current: " + current + ", " + Map_Y.Current + ", Map_Old: " + old + ", " + Map_Y.Old);
                SwitchBossTracker(current, (double)Map_Y.Current);
            }));

            WatcherName = "Map_Y";
            Watchers.Add(WatcherUtility.CreateWatcher<double>(WatcherName, WatcherUtility.CreateDeepPointer(GameProcess, Offsets[WatcherName]), (old, current) =>
            {
                var Map_X = Watchers["Map_X"];
                Debug.WriteLine("Map_Current: " + Map_X.Current + ", " + current + ", Map_Old: " + Map_X.Old + ", " + old);
                SwitchBossTracker((double)Map_X.Current, current);
            }));

            // Player health is needed to know if they have been hit. If the player gets hit disable the Watcher to avoid writing to the boss pointer
            WatcherName = "Player_Health";
            Watchers.Add(WatcherUtility.CreateWatcher<double>(WatcherName, WatcherUtility.CreateDeepPointer(GameProcess, Offsets[WatcherName]), false, (old, current) =>
            {
                Debug.WriteLine("Health difference (current-old): " + (current - old));
                if (old - current > 1)
                {
                    Debug.WriteLine("Disabling player health update");
                    Watchers["Player_Health"].Enabled = false;
                }
            }));
        }

        /// <summary>
        /// Write in memory the value passed at the location of the offset given
        /// </summary>
        /// <param name="offsetName">name of the offest to use</param>
        /// <param name="value">value to write to memory</param>
        /// <returns>if the value was writen correctly in memory</returns>
        private bool  SetValue<T>(string offsetName, T value) where T : struct
        {
            return WatcherUtility.WriteValue<T>(GameProcess, WatcherUtility.CreateIntPointer(GameProcess, Offsets[offsetName]), value);
        }

        /// <summary>
        /// Enables the health watcher and stores the current boss item reward offsets for writing to memory
        /// </summary>
        /// <param name="X">X position on the map</param>
        /// <param name="Y">Y position on the map</param>
        private void SwitchBossTracker(double X, double Y)
        {
            var newBoss = MapToBoss.Where(entry => entry.Key == (X, Y))
                        .SelectMany(entry => entry.Value)
                        .ToList();

            if(currentBoss.ToHashSet().SetEquals(newBoss))
            {
                return;
            }

            if (newBoss.Any())
            {
                Debug.WriteLine("Enabling player health update, currently in " + string.Join(" & ", newBoss));
                currentBoss = newBoss;
                Watchers["Player_Health"].Enabled = true;
                Watchers["Player_Health"].Reset();
            }
            else
            {
                Debug.WriteLine("Leaving " + string.Join(" & ", currentBoss) + ", disabling player health update");
                Watchers["Player_Health"].Enabled = false;
                currentBoss.Clear();
            }
        }

        /// <summary>
        ///     Gets the version of the Game based on the module memory size.
        /// </summary>
        /// <param name="invalidator">Component Invalidator</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>
        ///     A <c>string</c> with the version value if it's supported; otherwise, an empty string.
        /// </returns>
        private string GetGameVersion(IInvalidator invalidator = null, float width = 0, float height = 0)
        {
            string text, version;

            switch (GameProcess.MainModule.ModuleMemorySize)
            {
                case 40222720:
                    text = "Supported version detected: " + VERSION_1_07;
                    version = VERSION_1_07;
                    break;
                case 39690240:
                    //text = "Supported version detected: " + VERSION_1_05b;
                    //version = VERSION_1_05b;
                    //break;
                default:
                    text = "Version not supported";
                    version = "";
                    break;
            }

            if (invalidator != null)
            {
                SetSimpleLabelText(text, invalidator, width, height);
            }

            return version;
        }
        #endregion
        #endregion
    }
}