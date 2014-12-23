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
    public partial class OtherSettings : Form
    {
        public OtherSettings()
        {
            InitializeComponent();
        }

        private void OtherSettings_Load(object sender, EventArgs e)
        {
            tasklistChk.Checked = Properties.Settings.Default.useTaskList;
            skillChk.Checked = Properties.Settings.Default.autoSkill;
            clickingChk.Checked = Properties.Settings.Default.autoClicking;
            loggingChk.Checked = Properties.Settings.Default.logging;
            dogcogTxt.Text = Properties.Settings.Default.dogcog.ToString();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            int dogcogLvl;

            if (int.TryParse(dogcogTxt.Text, out dogcogLvl) && dogcogLvl >= 0 && dogcogLvl <= 25)
            {
                Properties.Settings.Default.dogcog = dogcogLvl;
                GameEngine.SetHeroDiscount(1.0 - 0.02 * dogcogLvl);
            }
            else
            {
                MessageBox.Show("Dogcog level needs to be a number between 0 and 25");
                return;
            }

            Properties.Settings.Default.useTaskList = tasklistChk.Checked;
            Properties.Settings.Default.autoSkill = skillChk.Checked;
            Properties.Settings.Default.autoClicking = clickingChk.Checked;
            Properties.Settings.Default.logging = loggingChk.Checked;
            Properties.Settings.Default.Save();
        }

    }
}
