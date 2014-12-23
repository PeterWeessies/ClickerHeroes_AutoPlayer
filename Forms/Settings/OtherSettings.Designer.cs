namespace clickerheroes.autoplayer
{
    partial class OtherSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OtherSettings));
            this.loggingChk = new System.Windows.Forms.CheckBox();
            this.clickingChk = new System.Windows.Forms.CheckBox();
            this.dogcogTxt = new System.Windows.Forms.TextBox();
            this.dogcogLbl = new System.Windows.Forms.Label();
            this.skillChk = new System.Windows.Forms.CheckBox();
            this.tasklistChk = new System.Windows.Forms.CheckBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // loggingChk
            // 
            this.loggingChk.AutoSize = true;
            this.loggingChk.Location = new System.Drawing.Point(12, 81);
            this.loggingChk.Name = "loggingChk";
            this.loggingChk.Size = new System.Drawing.Size(77, 17);
            this.loggingChk.TabIndex = 29;
            this.loggingChk.Text = "Log output";
            this.loggingChk.UseVisualStyleBackColor = true;
            // 
            // clickingChk
            // 
            this.clickingChk.AutoSize = true;
            this.clickingChk.Location = new System.Drawing.Point(12, 58);
            this.clickingChk.Name = "clickingChk";
            this.clickingChk.Size = new System.Drawing.Size(87, 17);
            this.clickingChk.TabIndex = 28;
            this.clickingChk.Text = "Auto clicking";
            this.clickingChk.UseVisualStyleBackColor = true;
            // 
            // dogcogTxt
            // 
            this.dogcogTxt.Location = new System.Drawing.Point(85, 98);
            this.dogcogTxt.Name = "dogcogTxt";
            this.dogcogTxt.Size = new System.Drawing.Size(27, 20);
            this.dogcogTxt.TabIndex = 27;
            this.dogcogTxt.Text = "25";
            // 
            // dogcogLbl
            // 
            this.dogcogLbl.AutoSize = true;
            this.dogcogLbl.Location = new System.Drawing.Point(9, 101);
            this.dogcogLbl.Name = "dogcogLbl";
            this.dogcogLbl.Size = new System.Drawing.Size(70, 13);
            this.dogcogLbl.TabIndex = 26;
            this.dogcogLbl.Text = "Dogcog level";
            // 
            // skillChk
            // 
            this.skillChk.AutoSize = true;
            this.skillChk.Location = new System.Drawing.Point(12, 35);
            this.skillChk.Name = "skillChk";
            this.skillChk.Size = new System.Drawing.Size(100, 17);
            this.skillChk.TabIndex = 30;
            this.skillChk.Text = "Auto skill usage";
            this.skillChk.UseVisualStyleBackColor = true;
            // 
            // tasklistChk
            // 
            this.tasklistChk.AutoSize = true;
            this.tasklistChk.Location = new System.Drawing.Point(12, 12);
            this.tasklistChk.Name = "tasklistChk";
            this.tasklistChk.Size = new System.Drawing.Size(91, 17);
            this.tasklistChk.TabIndex = 31;
            this.tasklistChk.Text = "Follow tasklist";
            this.tasklistChk.UseVisualStyleBackColor = true;
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(37, 124);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 32;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // OtherSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(185, 163);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.tasklistChk);
            this.Controls.Add(this.skillChk);
            this.Controls.Add(this.loggingChk);
            this.Controls.Add(this.clickingChk);
            this.Controls.Add(this.dogcogTxt);
            this.Controls.Add(this.dogcogLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "OtherSettings";
            this.Text = "Other";
            this.Load += new System.EventHandler(this.OtherSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox loggingChk;
        private System.Windows.Forms.CheckBox clickingChk;
        private System.Windows.Forms.TextBox dogcogTxt;
        private System.Windows.Forms.Label dogcogLbl;
        private System.Windows.Forms.CheckBox skillChk;
        private System.Windows.Forms.CheckBox tasklistChk;
        private System.Windows.Forms.Button saveBtn;
    }
}