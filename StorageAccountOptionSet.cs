using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using azb.Properties;

namespace azb
{
    internal class StorageAccountOptionSet
    {
        private string _storageAccount;
        private string _storageAccountKey;

        [Option("a", "account")]
        [Description("The name of the storage account. If not specified, the default will be the last saved storage account.")]
        public string StorageAccount
        {
            get
            {
                if (_storageAccount == null)
                    _storageAccount = Settings.Default.Account;
                return _storageAccount;
            }
            set { _storageAccount = value; }
        }

        [Option("k", "key")]
        [Description("The key for the storage account. If not specified, the default will be the last saved storage account key.")]
        public string StorageAccountKey
        {
            get
            {
                if (_storageAccountKey == null)
                    _storageAccountKey = Settings.Default.Key;
                return _storageAccountKey;
            }
            set { _storageAccountKey = value; }
        }

        [Option("save")]
        [Description("Save the account settings to the settings file as the default.")]
        public bool SaveAccount { get; set; }

        public void SaveToSettings()
        {
            if (SaveAccount)
            {
                Settings.Default.Account = StorageAccount;
                Settings.Default.Key = StorageAccountKey;
            }
        }

        /// <summary>
        /// Check that the account settings are usable.
        /// </summary>
        public bool SettingsInvalid(IConsoleAdapter console = null)
        {
            var settingsInvalid = string.IsNullOrEmpty(StorageAccount) || string.IsNullOrEmpty(StorageAccountKey);

            if (settingsInvalid && console != null)
                console.WrapLine("A valid storage account name and key must have been saved previously, or be specified using the -account and -key options.");

            return settingsInvalid;
        }
    }
}