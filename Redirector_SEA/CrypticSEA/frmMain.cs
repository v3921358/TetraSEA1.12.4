namespace CrypticSEA
{
    using CrypticSEA.Properties;
    using MapleLib.PacketLib;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;

    public class frmMain : Form
    {
        private IContainer components = null;
        public string installpath = "";
        public Dictionary<ushort, Listener> Listeners = new Dictionary<ushort, Listener>();
        private Process Maple;
        private string TempFolder = "";
        private NotifyIcon trayIcon;
        public ContextMenu trayMenu;
        private int tries = 0;

        public frmMain()
        {
            this.InitializeComponent();
            this.TempFolder = Path.GetTempPath();
            this.trayMenu = new ContextMenu();
            this.trayMenu.MenuItems.Add("Launch CrypticSEA", new EventHandler(this.OnStartButton));
            this.trayMenu.MenuItems.Add("Settings", new EventHandler(this.OnSettings));
            this.trayMenu.MenuItems.Add("-");
            if (Settings.Default.launchpath == "...")
            {
                this.trayMenu.MenuItems[0].Enabled = false;
            }
            this.trayMenu.MenuItems.Add("Website", new EventHandler(this.OnSiteButton));
            this.trayMenu.MenuItems.Add("Register", new EventHandler(this.OnRegisterButton));
            this.trayMenu.MenuItems.Add("Vote", new EventHandler(this.OnVoteButton));
            this.trayMenu.MenuItems.Add("-");
            //this.trayMenu.MenuItems.Add("About", new EventHandler(this.OnAbout));
            this.trayMenu.MenuItems.Add("Close", new EventHandler(this.OnExit));
            this.trayIcon = new NotifyIcon();
            this.trayIcon.Text = "CrypticSEA" + (Program.DevMode ? " - DEV" : "");
            this.trayIcon.Icon = new Icon(base.Icon, 40, 40);
            this.trayIcon.ContextMenu = this.trayMenu;
            this.trayIcon.Visible = true;
            base.ShowInTaskbar = false;
            if ((Settings.Default.launchpath == "..."))
            {
                new SettingsForm().Show();
            }
            new Thread(new ThreadStart(this.StartLoading)).Start();
        }

        public bool AdapterChecks()
        {
            NetworkInterface loopBack = GetLoopBack();
            if (loopBack == null)
            {
                if (this.tries >= 5)
                {
                    return false;
                }
                this.CreateNewLoopback();
                this.tries++;
                Thread.Sleep(0x2710);
                return this.AdapterChecks();
            }
            if (loopBack.OperationalStatus != OperationalStatus.Up)
            {
                if (this.tries >= 5)
                {
                    return false;
                }
                Thread.Sleep(500);
                this.tries++;
                this.AdapterChecks();
            }
            List<IPAddress> iPFromAdapter = this.GetIPFromAdapter(loopBack);
            Dictionary<string, bool> iPList = new Dictionary<string, bool>();
            iPList.Add("203.116.196.8", false);
            iPList.Add("203.188.239.82", false);
            foreach (IPAddress address in iPFromAdapter)
            {
                if (iPList.ContainsKey(address.ToString()))
                {
                    iPList.Remove(address.ToString());
                }
            }
            if (iPList.Count != 0)
            {
                this.AddIPToAdapter(loopBack.Name, iPList);
            }
            return true;
        }

        public void AddIPToAdapter(string Name, Dictionary<string, bool> IPList)
        {
            foreach (KeyValuePair<string, bool> pair in IPList)
            {
                if (!pair.Value)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = "int ip add address name=\"" + Name + "\" " + pair.Key + " 255.255.255.0";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                }
            }
            Thread.Sleep(200);
            while (Process.GetProcessesByName(Path.GetFileNameWithoutExtension("netsh")).Length > 0)
            {
                Thread.Sleep(30);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(Program.RegisterURL);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new SettingsForm().ShowDialog();
        }

        public void CallDevCon(string command, string file)
        {
            this.ExtractTools(file);
            this.installpath = "install.bat";
            TextWriter writer = new StreamWriter(this.TempFolder + this.installpath);
            writer.WriteLine("@echo off");
            string str = "@start \"\" \"" + this.TempFolder + file + "\" " + command;
            writer.WriteLine(str);
            writer.WriteLine("@exit");
            writer.Close();
            Process.Start(this.TempFolder + this.installpath);
            Thread.Sleep(0x3e8);
            while (Process.GetProcessesByName(file).Length > 0)
            {
                Thread.Sleep(30);
            }
            Thread.Sleep(0x7d0);
            try
            {
                System.IO.File.Delete(this.TempFolder + this.installpath);
                this.installpath = "";
                System.IO.File.Delete(this.TempFolder + file);
            }
            catch
            {
            }
        }

        public void CreateNewLoopback()
        {
            string command = @"install %WINDIR%\Inf\Netloop.inf *MSLOOP";
            if ((Environment.OSVersion.Version.Major >= 6) && this.is64bit())
            {
                this.CallDevCon(command, "amd64.exe");
            }
            else if (!((Environment.OSVersion.Version.Major < 6) || this.is64bit()))
            {
                this.CallDevCon(command, "i32.exe");
            }
            else if (this.is64bit())
            {
                this.CallDevCon(command, "amd64.exe");
            }
            else
            {
                this.CallDevCon(command, "i32.exe");
            }
        }

        public static void DisableTunnel(string Name)
        {
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            dictionary.Add("203.116.196.8", false);
            dictionary.Add("203.188.239.82", false);
            foreach (KeyValuePair<string, bool> pair in dictionary)
            {
                Process process = new Process();
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = "int ip delete address name=\"" + Name + "\" " + pair.Key + " all";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ExtractTools(string fileName)
        {
            if (!System.IO.File.Exists(this.TempFolder + fileName))
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                int index = -1;
                string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
                for (int i = 0; i < manifestResourceNames.Length; i++)
                {
                    if (manifestResourceNames[i].Contains(fileName))
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                {
                    Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(manifestResourceNames[index]);
                    FileStream stream2 = System.IO.File.Create(this.TempFolder + fileName);
                    byte[] buffer = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(buffer, 0, buffer.Length);
                    stream2.Write(buffer, 0, buffer.Length);
                    stream2.Close();
                    manifestResourceStream.Close();
                }
            }
        }

        public void FixALL()
        {
            this.trayIcon.Visible = false;
            this.trayIcon.Dispose();
            try
            {
                if (this.installpath != "")
                {
                    System.IO.File.Delete(this.TempFolder + this.installpath);
                }
                NetworkInterface loopBack = GetLoopBack();
                if (loopBack != null)
                {
                    DisableTunnel(loopBack.Name);
                    Application.Exit();
                }
            }
            catch
            {
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.FixALL();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
        }

        public List<IPAddress> GetIPFromAdapter(NetworkInterface adapter)
        {
            List<IPAddress> list = new List<IPAddress>();
            IPInterfaceProperties iPProperties = adapter.GetIPProperties();
            foreach (IPAddressInformation information in iPProperties.UnicastAddresses)
            {
                if (!(IPAddress.IsLoopback(information.Address) || (information.Address.AddressFamily == AddressFamily.InterNetworkV6)))
                {
                    list.Add(information.Address);
                }
            }
            return list;
        }

        public static NetworkInterface GetLoopBack()
        {
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            string adapter = Settings.Default.adapter;
            foreach (NetworkInterface interface3 in allNetworkInterfaces)
            {
                string str2 = interface3.Description.ToLower();
                if (((str2.Contains("microsoft") && str2.Contains("loopback")) || str2.Contains("invertido")) || interface3.Description.ToLower().Contains(adapter.ToLower()))
                {
                    return interface3;
                }
            }
            return null;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.SuspendLayout();
            // 
            // frmMain
            // 
            this.ClientSize = new System.Drawing.Size(200, 50);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CrypticSEA";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);

        }

        public bool is64bit()
        {
            return (IntPtr.Size == 8);
        }

        public void LaunchMaple()
        {
            if (!System.IO.File.Exists(Settings.Default.launchpath))
            {
                MessageBox.Show("Could not find MapleStory client.");
            }
            else
            {
                this.Maple = new Process();
                this.Maple.StartInfo.FileName = Settings.Default.launchpath;
                this.Maple.Start();
            }
        }

        private void listener_OnClientConnected(Session session, ushort port)
        {
            Debug.WriteLine("Accepted connection on " + port);
            InterceptedLinkedClient client = new InterceptedLinkedClient(session, Program.toIP, port);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            //(new AboutBox())
        }

        private void OnExit(object sender, EventArgs e)
        {
            try
            {
                if (this.Maple != null)
                {
                    this.Maple.Kill();
                }
            }
            catch
            {
            }
            this.frmMain_FormClosed(null, null);
            Environment.Exit(0);
        }

        private void OnSettings(object sender, EventArgs e)
        {
            new SettingsForm().Show();
        }

        private void OnShow(object sender, EventArgs e)
        {
            base.Show();
            base.Visible = true;
        }

        private void OnSiteButton(object sender, EventArgs e)
        {
            Process.Start(Program.MainWebsite);
        }

        private void OnRegisterButton(object sender, EventArgs e)
        {
            Process.Start(Program.RegisterURL);
        }

        private void OnVoteButton(object sender, EventArgs e)
        {
            Process.Start(Program.VoteLink);
        }

        private void OnStartButton(object sender, EventArgs e)
        {
            this.LaunchMaple();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start(Program.LogoLink);
        }

        private void StartLoading()
        {
            if (!this.AdapterChecks())
            {
                MessageBox.Show("Failed to install the application. Make sure you run this program as Administrator.");
            }
            else if (Program.checkIP(Program.toIP))
            {
                if (!(!(Program.toIP != "127.0.0.1") || Program.DevMode))
                {
                    this.StartTunnels();
                }
                if (Settings.Default.autolaunch)
                {
                    this.LaunchMaple();
                }
            }
        }

        public void StartTunnels()
        {
            try
            {
                ushort lowPort = Program.lowPort;
                ushort highPort = Program.highPort;
                string toIP = Program.toIP;
                if (Program.checkIP(toIP))
                {
                    ushort num4;
                    ushort num3;
                        Listener listener = new Listener();
                        Debug.WriteLine("Listening on 8484");
                        listener.OnClientConnected += new Listener.ClientConnectedHandler(this.listener_OnClientConnected);
                        listener.Listen(0x2124);
                        num3 = (ushort) (highPort - lowPort);
                        for (num4 = 0; num4 <= num3; num4 = (ushort) (num4 + 1))
                        {
                            Listener listener2 = new Listener();
                            listener2.OnClientConnected += new Listener.ClientConnectedHandler(this.listener_OnClientConnected);
                            listener2.Listen((ushort) (lowPort + num4));
                            Debug.WriteLine("Listening on " + ((lowPort + num4)).ToString());
                            this.Listeners.Add((ushort) (lowPort + num4), listener2);
                    }
                }
            }
            catch
            {
            }
        }
    }
}

