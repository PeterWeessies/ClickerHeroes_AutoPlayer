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
    public partial class TaskList : Form
    {
        public TaskList()
        {
            InitializeComponent();
        }

        private void reloadBtn_Click(object sender, EventArgs e)
        {
            TaskBox.Text = Properties.Settings.Default.taskList;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string ret = PlayerEngine.ParseTasklist(TaskBox.Text);
            if (ret != null)
            {
                MessageBox.Show(string.Format("Error parsing task list: {0}", ret));
            }
            else
            {
                Properties.Settings.Default.taskList = TaskBox.Text;
                Properties.Settings.Default.Save();

                MessageBox.Show("Tasklist Saved!");
            }
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            TaskBox.Text = Properties.Settings.Default.taskList;
        }

    }
}
