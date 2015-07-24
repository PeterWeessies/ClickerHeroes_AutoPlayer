using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clickerheroes.autoplayer
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private int top, bot, left, right;
        Thread ClickerThread = null;
        StreamWriter sw = null;
        string loggingDirectory;
        string currentLoggingString;
        DateTime TimeToNextLog;
        ClickerHeroesPosition clickerHeroesPositionForm = new ClickerHeroesPosition();
        TaskList taskListForm = new TaskList();
        OtherSettings otherSettingsForm = new OtherSettings();

        private void button1_Click(object sender, EventArgs e)
        {
            ToggleAutoplayer(label1.ForeColor == Color.FromArgb(255, 0, 0, 0) || label1.ForeColor == Color.Black);
        }

        public void ToggleAutoplayer(bool state)
        {
            if (state && !GameEngine.ValidatePlayableArea())
            {
                MessageBox.Show("Can't find game, please check your settings");
                return;
            }

            if (ClickerThread == null)
            {
                ClickerThread = new Thread(new ThreadStart(PlayerEngine.ClickThread));
                ClickerThread.Start();
                // Sets the culture to English (US)
                ClickerThread.CurrentCulture = new CultureInfo("en-US");
                ClickerThread.CurrentUICulture = new CultureInfo("en-US");
            }

            label1.ForeColor = state ? Color.Red : Color.Black;
            button1.Text = state ? "Stop ( CTRL + SHIFT + D )" : "Start";
            PlayerEngine.SetThreadActive(state ? 1 : 0);
            parsegame.Enabled = state;
            useskills.Enabled = state;
            toolStripMenuItem1.Enabled = !state;

            if (state && Properties.Settings.Default.logging)
            {
                loggingDirectory = string.Format("{0}\\logs", Application.StartupPath);
                if (!Directory.Exists(loggingDirectory))
                {
                     Directory.CreateDirectory(loggingDirectory);
                }

                currentLoggingString = DateTime.Now.ToString("MM-dd-yyyy HH mm ss");
                Directory.CreateDirectory(string.Format("{0}\\{1}", loggingDirectory, currentLoggingString));
                Directory.CreateDirectory(string.Format("{0}\\{1}\\screenshots", loggingDirectory, currentLoggingString));
                sw = File.AppendText(string.Format("{0}\\{1}\\{1}.csv", loggingDirectory, currentLoggingString));
                TimeToNextLog = DateTime.Now;
            }
            else
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw = null;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Imports.POINT lpPoint;
            bool b = Imports.GetCursorPos(out lpPoint);
            if (b)
            {
                label1.Text = string.Format("{0}, {1}", lpPoint.X, lpPoint.Y);
            }
        }

        private void clickyclicky_Tick(object sender, EventArgs e)
        {
            Stopwatch t = new Stopwatch();
            t.Start();

            double money = GameEngine.GetMoney();
            ParsedHeroes ph = GameEngine.GetHeroes();

            if (ph != null)
            {
                if (Properties.Settings.Default.useTaskList)
                {
                    label14.Text = PlayerEngine.TryNextTask(ph, money);
                }
                else
                {
                    label14.Text = "None, tasks turned off";
                }

                StringBuilder sb = new StringBuilder();
                if (ph.HeroStats != null)
                {
                    foreach (HeroStats ss in ph.HeroStats)
                    {
                        sb.AppendLine(string.Format("{0}: Lvl {1} Upgrades {2}", ss.Hero.Name, ss.Level, Convert.ToString(ss.UpgradeBitfield, 2)));
                    }
                }
                curHeroesTxt.Text = sb.ToString();
            }
            else
            {
                curHeroesTxt.Text = string.Empty;
            }

            label9.Text = money.ToString();

            if (Properties.Settings.Default.logging && DateTime.Now > TimeToNextLog)
            {
                Stopwatch imgsw = new Stopwatch();
                imgsw.Start();
                sw.WriteLine(string.Format("{0},{1}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), money));

                Rectangle playableArea = GameEngine.GetPlayableArea();
                using (Bitmap bitmap = new Bitmap(playableArea.Width, playableArea.Height)) {
                    using (Graphics g = Graphics.FromImage(bitmap)) {
                        g.CopyFromScreen(new Point(playableArea.Left, playableArea.Top), Point.Empty, playableArea.Size);
                    }

                    bitmap.Save(string.Format("{0}\\{1}\\screenshots\\{2}.png", loggingDirectory, currentLoggingString, DateTime.Now.ToString("MM-dd-yyyy HH mm ss")));
                }
                TimeToNextLog = TimeToNextLog.AddMinutes(1);
                imgsw.Stop();
                label15.Text = string.Format("Image captured at {0} in {1} ms", DateTime.Now.ToString("hh:mm:ss"), imgsw.ElapsedMilliseconds);
            }
            else if (!Properties.Settings.Default.logging)
            {
                label15.Text = "Logging turned off";
            }

            t.Stop();
            label8.Text = string.Format("{0} ms", t.ElapsedMilliseconds);
        }

        class GlobalHotkey
        {
            private int modifier;
            private int key;
            private IntPtr hWnd;
            private int id;

            public override int GetHashCode()
            {
                return modifier ^ key ^ hWnd.ToInt32();
            }

            public bool Register()
            {
                return Imports.RegisterHotKey(hWnd, id, modifier, key);
            }

            public bool Unregiser()
            {
                return Imports.UnregisterHotKey(hWnd, id);
            }

            public GlobalHotkey(int modifier, Keys key, Form form)
            {
                this.modifier = modifier;
                this.key = (int)key;
                this.hWnd = form.Handle;
                id = this.GetHashCode();
            }

            public static class Constants
            {
                //modifiers
                public const int NOMOD = 0x0000;
                public const int ALT = 0x0001;
                public const int CTRL = 0x0002;
                public const int SHIFT = 0x0004;
                public const int WIN = 0x0008;

                //windows message id for hotkey
                public const int WM_HOTKEY_MSG_ID = 0x0312;
            }
        }

        private void HandleHotkey()
        {
            ToggleAutoplayer(label1.ForeColor == Color.FromArgb(255, 0, 0, 0) || label1.ForeColor == Color.Black);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == GlobalHotkey.Constants.WM_HOTKEY_MSG_ID)
                HandleHotkey();
            base.WndProc(ref m);
        }

        private GlobalHotkey ghk;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load Play Area
            top = Properties.Settings.Default.top;
            bot = Properties.Settings.Default.bot;
            left = Properties.Settings.Default.left;
            right = Properties.Settings.Default.right;

            GameEngine.SetPlayableArea(new Rectangle(left, top, right - left, bot - top));

            // By moving here, we do not reload the tasklist every time we stop the program with the GHK
            // Load Tasks
            string ret = PlayerEngine.ParseTasklist(Properties.Settings.Default.taskList);
            if (ret != null)
            {
                MessageBox.Show(string.Format("Error parsing task list: {0}", ret));
                return;
            }

            // Set Discount
            GameEngine.SetHeroDiscount(1.0 - 0.02 * Properties.Settings.Default.dogcog);

            // Set Hotkey
            ghk = new GlobalHotkey(GlobalHotkey.Constants.CTRL + GlobalHotkey.Constants.SHIFT, Keys.D, this);
            if (!ghk.Register())
            {
                throw new Exception("can't register");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ClickerThread != null)
            {
                ClickerThread.Abort();
            }
            ghk.Unregiser();
            Application.Exit();
        }

        //Flag for skill usage
        private static bool skillFlag = false;

        //Stopwatch obj for keeping track of skill cooldown
        private static Stopwatch skillTimer1 = new Stopwatch();
        private static Stopwatch skillTimer2 = new Stopwatch();

        /// <summary>
        /// Tries to use skills (and also toggle off progress mode, if it is on).
        /// Lots of room for optimization here.
        /// Skill usage is not optimal, taken out until can figure them out.
        /// This function is called every 2 seconds by a Timer in Main.Designer.cs 'useSkills'
        /// Will have to check to see how this works if you stop the program in the middle of this,
        /// not sure how the stopwatch will work in that case
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void useskills_Tick(object sender, EventArgs e)
        {
            //Check PlayerEngine.useSkils - Only if true execute skills
            if (PlayerEngine.getUseSkils())
            {
                // First set of skill usage
                // Clickstorm -> Powersurge -> Lucky Strikes -> Metal Detector -> Golden Clicks ->
                // Super Clicks -> Energize -> Dark Ritual -> Reload
                // Check flag and if skillTimer2 is currently not running or it has been running for 15 min
                if (!skillFlag && (!skillTimer2.IsRunning || skillTimer2.ElapsedMilliseconds>900000))
                {
                    //Stop skillTimer2
                    if(skillTimer2.IsRunning)
                        skillTimer2.Stop();

                    //First run of Skill Usage - skillFlag = false
                    PlayerEngine.PressKey(Imports.VK_1);
                    PlayerEngine.PressKey(Imports.VK_2);
                    PlayerEngine.PressKey(Imports.VK_3);
                    PlayerEngine.PressKey(Imports.VK_4);
                    PlayerEngine.PressKey(Imports.VK_5);
                    PlayerEngine.PressKey(Imports.VK_7);
                    PlayerEngine.PressKey(Imports.VK_8);
                    PlayerEngine.PressKey(Imports.VK_6);
                    PlayerEngine.PressKey(Imports.VK_9);

                    //Start skillTimer1
                    skillTimer1.Start();

                    //Toggle flag
                    skillFlag = true;
                }

                // Second set of skill usage
                // Energize -> Reload -> Clickstorm -> Powersurge -> Lucky Strikes -> Metal Detector ->
                // Golden Clicks -> Super Clicks
                // Check flag and if skillTimer1 has been running for 15 min (900,000 milliseconds)
                else if(skillFlag && (skillTimer1.ElapsedMilliseconds>900000))
                {
                    //Stop skillTimer1
                    if(skillTimer1.IsRunning)
                        skillTimer1.Stop();

                    //Second run of Skill Usage - skillFlag = true
                    PlayerEngine.PressKey(Imports.VK_8);
                    PlayerEngine.PressKey(Imports.VK_9);
                    PlayerEngine.PressKey(Imports.VK_1);
                    PlayerEngine.PressKey(Imports.VK_2);
                    PlayerEngine.PressKey(Imports.VK_3);
                    PlayerEngine.PressKey(Imports.VK_4);
                    PlayerEngine.PressKey(Imports.VK_5);
                    PlayerEngine.PressKey(Imports.VK_7);

                    //Start skillTimer2
                    skillTimer2.Start();

                    //Toggle flag
                    skillFlag = false;
                }
            }

            /*
            // Dark Ritual
            PlayerEngine.PressKey(Imports.VK_6);

            // Golden Clicks
            PlayerEngine.PressKey(Imports.VK_8);
            PlayerEngine.PressKey(Imports.VK_5);
            PlayerEngine.PressKey(Imports.VK_4);

            // DPS
            // PlayerEngine.PressKey(Imports.VK_1);
            PlayerEngine.PressKey(Imports.VK_2);
            PlayerEngine.PressKey(Imports.VK_3);
            PlayerEngine.PressKey(Imports.VK_7);
            */
            /* Moved to First task in tasklist
            if (!GameEngine.IsProgressModeOn())
            {
                PlayerEngine.AddAction(new Action(GameEngine.GetProgressButton(), 0));
            }
            */
        }

        private void clickerHeroesPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clickerHeroesPositionForm.ShowDialog(this);
        }

        private void heroBuyOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            taskListForm.ShowDialog(this);
        }

        private void otherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            otherSettingsForm.ShowDialog(this);
        }

    }
}
