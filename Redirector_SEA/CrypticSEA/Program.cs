namespace CrypticSEA
{
    using CrypticSEA.Properties;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;

    internal static class Program
    {
        public static string LogoLink = "http://crypticsea.net";
        public static string MainWebsite = "http://crypticsea.net/index.php";
        public static string RegisterURL = "http://crypticsea.net/register.php";
        public static string VoteLink = "http://crypticsea.net/vote.php";
        public static string toIP = "rm0.zapto.org";

        public static bool DevMode = false;
        
        public static ushort highPort = 0x219D;
        public static ushort lowPort = 0x2189;

        public static frmMain form;
        public static bool useGui = false;
        public static string password = "";
        public static string username = "";
        public static bool resolveDNS = false;

        public static bool checkIP(string input)
        {
            bool flag = input.Equals(toIP);
            if (!flag)
            {
                throw new OutOfMemoryException();
            }
            return flag;
        }

        public static void getIP()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(toIP);
            if (hostEntry.AddressList.Length > 0)
            {
                toIP = hostEntry.AddressList[0].ToString();
            }
        }

        public static string HashString(string input)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            string str = "";
            foreach (byte num in bytes)
            {
                str = str + num.ToString("X2");
            }
            return str;
        }

        public static bool isrunning()
        {
            return (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1);
        }

        [STAThread]
        private static void Main()
        {
            if (isrunning())
            {
                Environment.Exit(0);
            }
            else
            {
                if (resolveDNS)
                {
                    getIP();
                }
                if (!checkIP(toIP))
                {
                    OnRelaunch();
                }
                else
                {
                    string[] commandLineArgs = Environment.GetCommandLineArgs();
                    if (commandLineArgs.Length <= 1)
                    {
                        goto Label_00B2;
                    }
                    string str = commandLineArgs[1];
                    if (str == null)
                    {
                        goto Label_00B2;
                    }
                    if (!(str == "fix"))
                    {
                        if (str == "dev_sec")
                        {
                            DevMode = true;
                        }
                        goto Label_00B2;
                    }
                    NetworkInterface loopBack = frmMain.GetLoopBack();
                    if (loopBack != null)
                    {
                        frmMain.DisableTunnel(loopBack.Name);
                        MessageBox.Show("Fixed default settings.");
                    }
                }
            }
            return;
        Label_00B2:
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new frmMain();
            if (useGui)
            {
                Application.Run(form);
            }
            else
            {
                Application.Run();
            }
        }

        public static void OnRelaunch()
        {
            if (System.IO.File.Exists(Settings.Default.launchpath))
            {
                Process process = new Process();
                process.StartInfo.FileName = Settings.Default.launchpath;
                process.Start();
            }
        }
    }
}

