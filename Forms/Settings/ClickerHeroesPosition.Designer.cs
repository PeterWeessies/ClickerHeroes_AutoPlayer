namespace clickerheroes.autoplayer
{
    partial class ClickerHeroesPosition
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClickerHeroesPosition));
            this.BotRightLbl = new System.Windows.Forms.Label();
            this.TopRightLbl = new System.Windows.Forms.Label();
            this.topLeftLbl = new System.Windows.Forms.Label();
            this.clickerHeroes = new System.Windows.Forms.GroupBox();
            this.clickAreaLbl = new System.Windows.Forms.Label();
            this.setBtn = new System.Windows.Forms.Button();
            this.CurPosDesLbl = new System.Windows.Forms.Label();
            this.curPosLbl = new System.Windows.Forms.Label();
            this.mouseTimer = new System.Windows.Forms.Timer(this.components);
            this.stopSetBtn = new System.Windows.Forms.Button();
            this.BotLeftLbl = new System.Windows.Forms.Label();
            this.clickerHeroes.SuspendLayout();
            this.SuspendLayout();
            // 
            // BotRightLbl
            // 
            this.BotRightLbl.Location = new System.Drawing.Point(279, 167);
            this.BotRightLbl.Name = "BotRightLbl";
            this.BotRightLbl.Size = new System.Drawing.Size(75, 13);
            this.BotRightLbl.TabIndex = 11;
            this.BotRightLbl.Text = "Bottom Right";
            // 
            // TopRightLbl
            // 
            this.TopRightLbl.Location = new System.Drawing.Point(279, 9);
            this.TopRightLbl.Name = "TopRightLbl";
            this.TopRightLbl.Size = new System.Drawing.Size(75, 13);
            this.TopRightLbl.TabIndex = 9;
            this.TopRightLbl.Text = "Top Right";
            this.TopRightLbl.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // topLeftLbl
            // 
            this.topLeftLbl.Location = new System.Drawing.Point(-3, 9);
            this.topLeftLbl.Name = "topLeftLbl";
            this.topLeftLbl.Size = new System.Drawing.Size(75, 13);
            this.topLeftLbl.TabIndex = 8;
            this.topLeftLbl.Text = "Top Left";
            this.topLeftLbl.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // clickerHeroes
            // 
            this.clickerHeroes.Controls.Add(this.clickAreaLbl);
            this.clickerHeroes.Location = new System.Drawing.Point(58, 25);
            this.clickerHeroes.Name = "clickerHeroes";
            this.clickerHeroes.Size = new System.Drawing.Size(236, 139);
            this.clickerHeroes.TabIndex = 7;
            this.clickerHeroes.TabStop = false;
            // 
            // clickAreaLbl
            // 
            this.clickAreaLbl.Location = new System.Drawing.Point(144, 69);
            this.clickAreaLbl.Name = "clickAreaLbl";
            this.clickAreaLbl.Size = new System.Drawing.Size(75, 13);
            this.clickAreaLbl.TabIndex = 7;
            this.clickAreaLbl.Text = "Clicking Area";
            this.clickAreaLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // setBtn
            // 
            this.setBtn.Location = new System.Drawing.Point(15, 204);
            this.setBtn.Name = "setBtn";
            this.setBtn.Size = new System.Drawing.Size(75, 23);
            this.setBtn.TabIndex = 12;
            this.setBtn.Text = "Set";
            this.setBtn.UseVisualStyleBackColor = true;
            this.setBtn.Click += new System.EventHandler(this.setBtn_Click);
            // 
            // CurPosDesLbl
            // 
            this.CurPosDesLbl.AutoSize = true;
            this.CurPosDesLbl.Location = new System.Drawing.Point(12, 188);
            this.CurPosDesLbl.Name = "CurPosDesLbl";
            this.CurPosDesLbl.Size = new System.Drawing.Size(76, 13);
            this.CurPosDesLbl.TabIndex = 17;
            this.CurPosDesLbl.Text = "Cursor position";
            // 
            // curPosLbl
            // 
            this.curPosLbl.AutoSize = true;
            this.curPosLbl.Location = new System.Drawing.Point(88, 188);
            this.curPosLbl.Name = "curPosLbl";
            this.curPosLbl.Size = new System.Drawing.Size(24, 13);
            this.curPosLbl.TabIndex = 16;
            this.curPosLbl.Text = "n/a";
            // 
            // mouseTimer
            // 
            this.mouseTimer.Enabled = true;
            this.mouseTimer.Tick += new System.EventHandler(this.mouseTimer_Tick_1);
            // 
            // stopSetBtn
            // 
            this.stopSetBtn.Location = new System.Drawing.Point(96, 204);
            this.stopSetBtn.Name = "stopSetBtn";
            this.stopSetBtn.Size = new System.Drawing.Size(75, 23);
            this.stopSetBtn.TabIndex = 18;
            this.stopSetBtn.Text = "Stop Setting";
            this.stopSetBtn.UseVisualStyleBackColor = true;
            this.stopSetBtn.Visible = false;
            this.stopSetBtn.Click += new System.EventHandler(this.stopSetBtn_Click);
            // 
            // BotLeftLbl
            // 
            this.BotLeftLbl.Location = new System.Drawing.Point(-3, 167);
            this.BotLeftLbl.Name = "BotLeftLbl";
            this.BotLeftLbl.Size = new System.Drawing.Size(75, 13);
            this.BotLeftLbl.TabIndex = 19;
            this.BotLeftLbl.Text = "Bottom Left";
            this.BotLeftLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ClickerHeroesPosition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 238);
            this.Controls.Add(this.BotLeftLbl);
            this.Controls.Add(this.stopSetBtn);
            this.Controls.Add(this.CurPosDesLbl);
            this.Controls.Add(this.curPosLbl);
            this.Controls.Add(this.setBtn);
            this.Controls.Add(this.BotRightLbl);
            this.Controls.Add(this.TopRightLbl);
            this.Controls.Add(this.topLeftLbl);
            this.Controls.Add(this.clickerHeroes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ClickerHeroesPosition";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ClickerHeroes Position";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClickerHeroesPosition_FormClosing);
            this.Load += new System.EventHandler(this.ClickerHeroesPosition_Load);
            this.clickerHeroes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BotRightLbl;
        private System.Windows.Forms.Label TopRightLbl;
        private System.Windows.Forms.Label topLeftLbl;
        private System.Windows.Forms.GroupBox clickerHeroes;
        private System.Windows.Forms.Label clickAreaLbl;
        private System.Windows.Forms.Button setBtn;
        private System.Windows.Forms.Label CurPosDesLbl;
        private System.Windows.Forms.Label curPosLbl;
        private System.Windows.Forms.Timer mouseTimer;
        private System.Windows.Forms.Button stopSetBtn;
        private System.Windows.Forms.Label BotLeftLbl;
        
    }
}