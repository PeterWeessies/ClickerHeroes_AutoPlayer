using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clickerheroes.autoplayer
{
    public partial class ClickerHeroesPosition : Form
    {
        public ClickerHeroesPosition()
        {
            InitializeComponent();
        }

        private int stage = 0;
        private int top, bot, left, right;

        private void setBtn_Click(object sender, EventArgs e)
        {
            Imports.POINT lpPoint;
            bool b = Imports.GetCursorPos(out lpPoint);
            if (!b)
            {
                return;
            }

            switch (stage)
            {
                case 0:
                    topLeftLbl.Text = "Top Left";
                    TopRightLbl.Text = "Top Right";
                    BotLeftLbl.Text = "Bottom Left";
                    BotRightLbl.Text = "Bottom Right";
                    clickAreaLbl.Text = "Clicking Area";
                    topLeftLbl.ForeColor = Color.Red;
                    stopSetBtn.Visible = true;
                    stage++;
                    break;
                case 1:
                    topLeftLbl.Text = string.Format("{0}, {1}", lpPoint.X, lpPoint.Y);
                    left = lpPoint.X;
                    top = lpPoint.Y;
                    topLeftLbl.ForeColor = Color.Black;
                    TopRightLbl.ForeColor = Color.Red;
                    stage++;
                    break;
                case 2:
                    TopRightLbl.Text = string.Format("{0}, {1}", lpPoint.X, lpPoint.Y);

                    if (left > lpPoint.X)
                    {
                        MessageBox.Show("Top right needs to be on the right of top left -- try again!");
                        TopRightLbl.Text = "Top Right";
                        break;
                    }
                    if (Math.Abs(top - lpPoint.Y) > 10)
                    {
                        MessageBox.Show("Y value is too different from top left -- try again!");
                        TopRightLbl.Text = "Top Right";
                        break;
                    }

                    if (Math.Abs(lpPoint.X - left) < 950)
                    {
                        MessageBox.Show("The width of the the game is to small -- try again!");
                        TopRightLbl.Text = "Top Right";
                        break;
                    }

                    top = (top + lpPoint.Y) / 2;
                    right = lpPoint.X;
                    TopRightLbl.ForeColor = Color.Black;
                    BotLeftLbl.ForeColor = Color.Red;
                    stage++;
                    break;
                case 3:
                    BotLeftLbl.Text = string.Format("{0}, {1}", lpPoint.X, lpPoint.Y);

                    if (top > lpPoint.Y)
                    {
                        MessageBox.Show("The bottom needs to be below the top -- try again!");
                        BotLeftLbl.Text = "Bottom Left";
                        break;
                    }
                    if (Math.Abs(left - lpPoint.X) > 10)
                    {
                        MessageBox.Show("X Value too different from top left -- try again!");
                        BotLeftLbl.Text = "Bottom Left";
                        break;
                    }
                    if (Math.Abs(lpPoint.Y - top) < 500)
                    {
                        MessageBox.Show("The height of the game is too small -- try again!");
                        BotLeftLbl.Text = "Bottom Left";
                        break;
                    }

                    left = (left + lpPoint.X) / 2;
                    bot = lpPoint.Y;
                    BotLeftLbl.ForeColor = Color.Black;
                    BotRightLbl.ForeColor = Color.Red;
                    stage++;
                    break;
                case 4:
                    BotRightLbl.Text = string.Format("{0}, {1}", lpPoint.X, lpPoint.Y);

                    if (Math.Abs(right - lpPoint.X) > 10)
                    {
                        MessageBox.Show("X value is too different from bottom left  -- try again!");
                        BotRightLbl.Text = "Bottom Right";
                        break;
                    }
                    if (Math.Abs(bot - lpPoint.Y) > 10)
                    {
                        MessageBox.Show("Y value is too different from bottom left  -- try again!");
                        BotRightLbl.Text = "Bottom Right";
                        break;
                    }

                    right = (right + lpPoint.X) / 2;
                    bot = (bot + lpPoint.Y) / 2;
                    BotRightLbl.ForeColor = Color.Black;

                    GameEngine.SetPlayableArea(new Rectangle(left, top, right - left, bot - top));

                    Point clickPoint = GameEngine.GetClickArea();
                    clickAreaLbl.Text = string.Format("{0}, {1}", clickPoint.X, clickPoint.Y);

                    if (GameEngine.ValidatePlayableArea())
                    {
                        Properties.Settings.Default.top = top;
                        Properties.Settings.Default.bot = bot;
                        Properties.Settings.Default.left = left;
                        Properties.Settings.Default.right = right;
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Settings saved!");
                    }
                    else
                    {
                        getSettings();
                        MessageBox.Show("Can't find game, please try again");
                    }

                    stopSetBtn.Visible = false;
                    stage = 0;
                    break;
            }

        }

        private void stopSetBtn_Click(object sender, EventArgs e)
        {
            stage = 0;

            getSettings();
            stopSetBtn.Visible = false;
        }

        private void mouseTimer_Tick_1(object sender, EventArgs e)
        {
            Imports.POINT lpPoint;
            bool b = Imports.GetCursorPos(out lpPoint);
            if (b)
            {
                curPosLbl.Text = string.Format("{0}, {1}", lpPoint.X, lpPoint.Y);
            }
        }

        private void ClickerHeroesPosition_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stage != 0)
            {
                e.Cancel = true;
            }
        }

        private void ClickerHeroesPosition_Load(object sender, EventArgs e)
        {
            getSettings();
        }

        private void getSettings()
        {
            top = Properties.Settings.Default.top;
            bot = Properties.Settings.Default.bot;
            left = Properties.Settings.Default.left;
            right = Properties.Settings.Default.right;

            topLeftLbl.Text = string.Format("{0}, {1}", left, top);
            TopRightLbl.Text = string.Format("{0}, {1}", right, top);
            BotLeftLbl.Text = string.Format("{0}, {1}", left, bot);
            BotRightLbl.Text = string.Format("{0}, {1}", right, bot);

            GameEngine.SetPlayableArea(new Rectangle(left, top, right - left, bot - top));

            Point clickPoint = GameEngine.GetClickArea();

            clickAreaLbl.Text = string.Format("{0}, {1}", clickPoint.X, clickPoint.Y);

            topLeftLbl.ForeColor = Color.Black;
            TopRightLbl.ForeColor = Color.Black;
            BotLeftLbl.ForeColor = Color.Black;
            BotRightLbl.ForeColor = Color.Black;

            checkBox1.Checked = Properties.Settings.Default.backgroundWindow;
        }

        /// <summary>
        /// Look for a Window named 'Clicker Heroes' and grab its rectangle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void detectBtn_Click(object sender, EventArgs e)
        {
            // Find the Window
            var hwnd = Imports.FindWindow(null, "Clicker Heroes");

            // Get the RECT
            var rect = new Imports.RECT();
            var b = Imports.GetClientRect(hwnd, out rect);
            if (b)
            {
                var point = new Imports.POINT();
                b = Imports.ClientToScreen(hwnd, ref point);
                if (b)
                {
                    // Set the playable area and update labels
                    GameEngine.SetPlayableArea(new Rectangle(point.X, point.Y, rect.Right - rect.Left, rect.Bottom - rect.Top));

                    Point clickPoint = GameEngine.GetClickArea();
                    clickAreaLbl.Text = string.Format("{0}, {1}", clickPoint.X, clickPoint.Y);

                    if (GameEngine.ValidatePlayableArea())
                    {
                        Properties.Settings.Default.top = point.X;
                        Properties.Settings.Default.bot = point.Y;
                        Properties.Settings.Default.left = rect.Left;
                        Properties.Settings.Default.right = rect.Right;
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Settings saved!");
                        return;
                    }
                }
            }
            getSettings();
            MessageBox.Show("Can't find game, please try again");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.backgroundWindow = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

    }
}
