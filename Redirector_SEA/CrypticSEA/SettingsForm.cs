namespace CrypticSEA
{
    using CrypticSEA.Properties;
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class SettingsForm : Form
    {
        private Button button1 = new Button();
        private Button button2;
        private CheckBox chkLaunch;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private TextBox txtAdapter;
        private PictureBox pictureBox1;
        private TextBox txtFilePath;

        public SettingsForm()
        {
            try
            {
                this.InitializeComponent();
                if (Settings.Default.launchpath == "...")
                {
                    this.txtFilePath.Text = this.GetMaplePath() + @"\MapleStory.exe";
                }
                else
                {
                    this.txtFilePath.Text = Settings.Default.launchpath;
                }
                this.chkLaunch.Checked = Settings.Default.autolaunch;
                this.txtAdapter.Text = Settings.Default.adapter;
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.Default.autolaunch = this.chkLaunch.Checked;
                Settings.Default.launchpath = this.txtFilePath.Text;
                Settings.Default.adapter = this.txtAdapter.Text;
                Settings.Default.Save();
                Program.form.trayMenu.MenuItems[0].Enabled = true;
                base.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "MapleStory Exe (*.exe)|*.exe";
                dialog.Title = "Select MapleStory";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtFilePath.Text = dialog.FileName;
                }
            }
            catch
            {
            }
        }

        private void chkLaunch_CheckedChanged(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public string GetMaplePath()
        {
            try
            {
                if (this.is64bit())
                {
                    return (string) Registry.GetValue(Registry.LocalMachine + @"\SOFTWARE\Wow6432Node\Wizet\MapleStory", "ExecPath", "...");
                }
                return (string) Registry.GetValue(Registry.LocalMachine + @"\SOFTWARE\Wizet\MapleStory", "ExecPath", "...");
            }
            catch
            {
                return @"C:\Program Files\Wizet\MapleStory\MapleStory.exe";
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.button1 = new System.Windows.Forms.Button();
            this.chkLaunch = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAdapter = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(256, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 24);
            this.button1.TabIndex = 6;
            this.button1.Text = "Save Changes";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkLaunch
            // 
            this.chkLaunch.AutoSize = true;
            this.chkLaunch.Location = new System.Drawing.Point(12, 12);
            this.chkLaunch.Name = "chkLaunch";
            this.chkLaunch.Size = new System.Drawing.Size(182, 17);
            this.chkLaunch.TabIndex = 8;
            this.chkLaunch.Text = "Launch MapleStory automatically";
            this.chkLaunch.UseVisualStyleBackColor = true;
            this.chkLaunch.CheckedChanged += new System.EventHandler(this.chkLaunch_CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(306, 41);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(50, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(12, 42);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(288, 20);
            this.txtFilePath.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "For those having trouble to connect:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Adapter Description:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // txtAdapter
            // 
            this.txtAdapter.Location = new System.Drawing.Point(157, 98);
            this.txtAdapter.Name = "txtAdapter";
            this.txtAdapter.Size = new System.Drawing.Size(199, 20);
            this.txtAdapter.TabIndex = 14;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::CrypticSEA.Properties.Resources.votelogo;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(362, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(167, 108);
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(541, 133);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtAdapter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.chkLaunch);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CrypticSEA Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool is64bit()
        {
            return (IntPtr.Size == 8);
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
        }
    }
}

