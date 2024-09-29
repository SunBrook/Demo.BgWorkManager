using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisposeHub.Con
{
    public class IniConfig
    {
        private static IniConfig cfg;

        public static IniConfig Instance => GetInstance();

        public static IniConfig GetInstance()
        {
            if (cfg == null)
            {
                cfg = new IniConfig();
            }
            return cfg;
        }


        private string _iniFilePath;

        public IniConfig()
        {
            _iniFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cfg.ini");
        }

        public void Write(string section, string key, string value)
        {
            var iniFileMap = new ExeConfigurationFileMap { ExeConfigFilename = _iniFilePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(iniFileMap, ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public string Read(string section, string key)
        {
            var iniFile = new ExeConfigurationFileMap { ExeConfigFilename = _iniFilePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(iniFile, ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] != null)
            {
                return config.AppSettings.Settings[key].Value;
            }

            return null;
        }
    }
}
