using System;
using System.IO;
namespace uTikDownloadHelper
{
    public class Settings
    {
        private IniFile ini;
        private String section = "Settings";

        

        public Settings()
        {
            if (!Directory.Exists(Common.SettingsPath)) Directory.CreateDirectory(Common.SettingsPath);

            ini = new IniFile(Path.Combine(Common.SettingsPath, "settings.ini"));
        }

        public String lastSelectedRegion
        {
            get
            {
                return ini.GetString(section, "lastSelectedRegion", "Any");
            }
            set
            {
                ini.WriteString(section, "lastSelectedRegion", value);
            }
        }
        public String ticketWebsite
        {
            get
            {
                return ini.GetString(section, "ticketWebsite", "");
            }
            set
            {
                ini.WriteString(section, "ticketWebsite", value);
            }
        }
        public String lastPath
        {
            get
            {
                return ini.GetString(section, "lastPath", "Any");
            }
            set
            {
                ini.WriteString(section, "lastPath", value);
            }
        }
    }
}
