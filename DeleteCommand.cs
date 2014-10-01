using System;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace azb
{
    [Command]
    [Description("Delete a file.")]
    class DeleteCommand
    {
        [Positional]
        [Description("The name of the container for the blob.")]
        public string Container { get; set; }

        [Positional]
        [Description("The name of the blob to delete.")]
        public string BlobName { get; set; }

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

            try
            {
                var blobRef = container.GetBlockBlobReference(BlobName);
                blobRef.Delete();
            }
            catch (Exception e)
            {
                console.WrapLine("Unable to retrieve blob details.");
                console.WrapLine(e.Message);
                Environment.ExitCode = 100;
                return;
            }

            console.WrapLine("{0} Deleted from {1}.", BlobName.White(), Container.White());

            AccountOptions.SaveToSettings();
        }
    }
}
