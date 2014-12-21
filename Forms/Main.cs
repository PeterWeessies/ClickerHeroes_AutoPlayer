using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (GameEngine.ValidatePlayableArea())
            {
                ClickerThread = new Thread(new ThreadStart(PlayerEngine.ClickThread));
                ClickerThread.Start();
                ToggleAutoplayer(true);
            }
            else
            {
                MessageBox.Show("Can't find game, please check your settings");
            }

        }

        public void ToggleAutoplayer(bool state)
        {
            label1.ForeColor = state ? Color.Red : Color.Black;
            checkBox1.Enabled = !state;
            checkBox2.Enabled = !state;
            textBox2.Enabled = !state;
            PlayerEngine.SetThreadActive((state && !checkBox1.Checked) ? 1 : 0);
            parsegame.Enabled = (state && !checkBox2.Checked);
            useskills.Enabled = (state && !checkBox1.Checked && !checkBox2.Checked);

            if (checkBox1.Checked)
            {
                label14.Text = "Test Mode";
            }
            
            if (state && checkBox3.Checked)
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
                int discountlevel;
                if (Int32.TryParse(textBox2.Text, out discountlevel))
                {
                    GameEngine.SetHeroDiscount(1.0 - 0.02 * discountlevel);
                }
                else
                {
                    textBox2.Text = "0";
                    GameEngine.SetHeroDiscount(1.0);
                }
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

        public void DrawBoundingRectangle(Bitmap b, Rectangle r)
        {
            for (int x = r.Left; x <= r.Right; x++)
            {
                b.SetPixel(x, r.Top, Color.Red);
                b.SetPixel(x, r.Bottom, Color.Red);
            }

            for (int y = r.Top; y <= r.Bottom; y++)
            {
                b.SetPixel(r.Left, y, Color.Red);
                b.SetPixel(r.Right, y, Color.Red);
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
 
            // To make the autoplayer work with many levels of Iris, we intentionally do not click any candies
            // unless we've just ascended (which is handled in the PlayerEngine)
            /*Point p;
            if (GameEngine.GetActiveCandy(out p) && !checkBox1.Checked)
            {
                PlayerEngine.AddAction(new Action(p, 0));
            }*/

            if (ph != null)
            {
                if (!checkBox1.Checked)
                {
                    label14.Text = PlayerEngine.TryNextTask(ph, money);
                }

                StringBuilder sb = new StringBuilder();
                if (ph.HeroStats != null)
                {
                    foreach (HeroStats ss in ph.HeroStats)
                    {
                        sb.AppendLine(string.Format("{0}: Lvl {1} Upgrades {2}", ss.Hero.Name, ss.Level, Convert.ToString(ss.UpgradeBitfield, 2)));
                    }
                }
                textBox1.Text = sb.ToString();
            }
            else
            {
                textBox1.Text = string.Empty;
            }

            label9.Text = money.ToString();

            if (checkBox3.Checked && DateTime.Now > TimeToNextLog)
            {
                Stopwatch imgsw = new Stopwatch();
                imgsw.Start();
                sw.WriteLine(string.Format("{0},{1}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), money));

                Rectangle playableArea = GameEngine.GetPlayableArea();
                using (Bitmap bitmap = new Bitmap(playableArea.Width, playableArea.Height)) {
                    using (Graphics g = Graphics.FromImage(bitmap)) {
                        g.CopyFromScreen(new Point(playableArea.Left, playableArea.Top), Point.Empty, playableArea.Size);
                    }

                    bitmap.Save(string.Format("{0}\\{1}\\screenshots\\{2}.bmp", loggingDirectory, currentLoggingString, DateTime.Now.ToString("MM-dd-yyyy HH mm ss")));
                }
                TimeToNextLog = TimeToNextLog.AddMinutes(1);
                imgsw.Stop();
                label15.Text = string.Format("Image captured at {0} in {1} ms", DateTime.Now.ToString("hh:mm:ss"), imgsw.ElapsedMilliseconds);
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

            // Load Tasks
            string ret = PlayerEngine.ParseTasklist(Properties.Settings.Default.taskList);
            if (ret != null)
            {
                MessageBox.Show(string.Format("Error parsing task list: {0}", ret));
            }

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

        /// <summary>
        /// Tries to use skills (and also toggle off progress mode, if it is on).
        /// Lots of room for optimization here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void useskills_Tick(object sender, EventArgs e)
        {
            PlayerEngine.PressKey(Imports.VK_6);
            PlayerEngine.PressKey(Imports.VK_8);
            PlayerEngine.PressKey(Imports.VK_4);
            PlayerEngine.PressKey(Imports.VK_1);
            PlayerEngine.PressKey(Imports.VK_2);
            PlayerEngine.PressKey(Imports.VK_3);
            PlayerEngine.PressKey(Imports.VK_5);
            PlayerEngine.PressKey(Imports.VK_7);

            if (!GameEngine.IsProgressModeOn())
            {
                PlayerEngine.AddAction(new Action(GameEngine.GetProgressButton(), 0));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Text box to the left: This is the task list. Put in the order in which you want to level heroes and buy upgrades. The format is (Hero, Level, Upgrades, Wait, Verify). For hero, Cid = 0, Treebeast = 1, etc. For level, this is the minimum level you want to raise it to. For upgrades, 0 = buy the 1st upgrade, 1 = buy the 1st and 2nd upgrades, etc. Set wait to true if you want to wait until you have enough money to buy all levels before leveling. Set verify to true if you want to verify the hero's level (will be slower but will prevent under-leveling heroes)");
            MessageBox.Show("Once the autoplayer starts, you won't be able to edit the task list anymore");
            MessageBox.Show("Test Mode: Set to true if you want the program to read your money and heroes on screen, but don't want it to click anything. Use it to test that the money and heroes are being read correctly.");
            MessageBox.Show("Autoclick only: only clicks the current monsters, and doesn't use any skills or buy anything");
            MessageBox.Show("Log output: If enabled, will write some output to the app log folder");
            MessageBox.Show("Dogcog Level: Your level of dogcog. If you don't know what this is, it's probably 0 for you.");
            MessageBox.Show("How to start: click 'start' and put your mouse in the upper-left corner of your game area (but don't click anything). Press spacebar. Then go to upper-right corner and press spacebar again. Then do lower-left and lower-right corner. Pressing start again will start the autoplayer");
            MessageBox.Show("While the autoplayer is playing, press CTRL-SHIFT-D to stop it. This is important, because it's the only way to stop it while it's playing (CTRL-SHIFT-D will start it again after this)");
            MessageBox.Show("Important: the program was only tested on a game area resolution of 1422x800. Other resolutions may have problems");
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

        }

    }
}
