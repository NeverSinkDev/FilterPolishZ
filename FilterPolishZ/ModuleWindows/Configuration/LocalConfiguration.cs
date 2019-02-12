using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Specialized;
using FilterPolishZ.Domain;
using FilterPolishZ.Domain.DataType;

namespace FilterPolishZ.Configuration
{
    public class LocalConfiguration
    {
        private System.Configuration.Configuration config;
        private static LocalConfiguration instance { get; set; }

        public NameValueCollection AppSettings { get; private set; }

        private LocalConfiguration()
        {
            this.config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            this.AppSettings = ConfigurationManager.AppSettings;
        }

        public static LocalConfiguration GetInstance()
        {
            if (instance == null)
            {
                instance = new LocalConfiguration();
            }

            return instance;
        }

        public void Set(string key, string value, bool doAutoSave = true)
        {
            this.config.AppSettings.Settings[key].Value = value;            
            if (doAutoSave) this.Save();
        }

        public void Save()
        {
            this.config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public IEnumerable<ConfigurationData> YieldConfiguration()
        {
            foreach (string key in AppSettings)
            {
                yield return new ConfigurationData() { Key = key, Value = AppSettings[key] };
            }
        }
    }
}
