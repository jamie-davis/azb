using System;
using System.Collections.Generic;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace azb
{
    [Command]
    [Description("Set meta data on a blob or container.")]
    class MetaCommand
    {
        [Positional]
        [Description("The name of the container.")]
        public string Container { get; set; }

        [Positional]
        [Description("The meta data key.")]
        public string Key { get; set; }

        [Positional(DefaultValue = null)]
        [Description("The meta data value. Omit this value or specify an empty string to delete the value.")]
        public string Value { get; set; }

        [Option("b", "blob")]
        [Description("The name of the blob if the meta data is to be applied to a blob.")]
        public string BlobName { get; set; }

        public bool Delete { get; set; }

        [OptionSet]
        public StorageAccountOptionSet AccountOptions { get; set; }

        [CommandHandler]
        public void Handle(IConsoleAdapter console)
        {
            if (AccountOptions.SettingsInvalid(console)) return;

            var credentials = new StorageCredentials(AccountOptions.StorageAccount, AccountOptions.StorageAccountKey);
            var account = new CloudStorageAccount(credentials, true);
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(Container);
            container.CreateIfNotExists();

            if (string.IsNullOrEmpty(Value))
                Delete = true;

            if (BlobName != null)
                SetBlobMetaData(console, container);
            else
                SetContainerMetaData(console, container);

            if (Environment.ExitCode == 0)
               AccountOptions.SaveToSettings();
        }

        private void SetBlobMetaData(IConsoleAdapter console, CloudBlobContainer container)
        {
            try
            {
                var blobRef = container.GetBlockBlobReference(BlobName);
                blobRef.FetchAttributes();

                AlterMetaData(blobRef.Metadata);
                blobRef.SetMetadata();

                if (Delete)
                    console.WrapLine("Deleted {0} from {1}.", Key.White(), BlobName.White());
                else
                    console.WrapLine("Set {0} attribute on {1} to {2}.", Key.White(), BlobName.White(), Value.White());
            }
            catch (Exception e)
            {
                console.WrapLine("Unable to retrieve blob details.");
                console.WrapLine(e.Message);
                Environment.ExitCode = 100;
            }
        }

        private void SetContainerMetaData(IConsoleAdapter console, CloudBlobContainer container)
        {
            try
            {
                container.FetchAttributes();
                AlterMetaData(container.Metadata);

                container.SetMetadata();

                if (Delete)
                    console.WrapLine("Deleted {0} from {1}.", Key.White(), Container.White(), Value.White());
                else
                    console.WrapLine("Set {0} metadata value on {1} to {2}.", Key.White(), Container.White(), Value.White());
            }
            catch (Exception e)
            {
                console.WrapLine("Unable to retrieve container details.");
                console.WrapLine(e.Message);
                Environment.ExitCode = 100;
            }
        }

        private void AlterMetaData(IDictionary<string, string> metadata)
        {
            if (Delete)
                metadata.Remove(Key);
            else
                metadata[Key] = Value;
        }
    }
}