namespace CrypticSEA.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0"), CompilerGenerated]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings) SettingsBase.Synchronized(new Settings()));

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
        }

        [DebuggerNonUserCode, UserScopedSetting, DefaultSettingValue("Don't touch if you don't need to.")]
        public string adapter
        {
            get
            {
                return (string) this["adapter"];
            }
            set
            {
                this["adapter"] = value;
            }
        }

        [DefaultSettingValue("False"), DebuggerNonUserCode, UserScopedSetting]
        public bool autolaunch
        {
            get
            {
                return (bool) this["autolaunch"];
            }
            set
            {
                this["autolaunch"] = value;
            }
        }

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting, DefaultSettingValue("..."), DebuggerNonUserCode]
        public string launchpath
        {
            get
            {
                return (string) this["launchpath"];
            }
            set
            {
                this["launchpath"] = value;
            }
        }

        [DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
        public bool remUsername
        {
            get
            {
                return (bool) this["remUsername"];
            }
            set
            {
                this["remUsername"] = value;
            }
        }

        [UserScopedSetting, DebuggerNonUserCode, DefaultSettingValue("")]
        public string username
        {
            get
            {
                return (string) this["username"];
            }
            set
            {
                this["username"] = value;
            }
        }
    }
}

