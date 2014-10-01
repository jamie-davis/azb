using System.IO;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace azb
{
    [Command]
    [Description("Upload a file.")]
    class UploadCommand
    {
        [Positional]
        [Description("The file to upload.")]
        public string File { get; set; }

        [Positional]
        [Description("The name of the container for the blob.")]
        public string Container { get; set; }

        [Positional]
        [Description("The name to give the blob.")]
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

            using (var file = new FileStream(File, FileMode.Open, FileAccess.Read))
            {
                var blobRef = container.GetBlockBlobReference(BlobName);
                blobRef.UploadFromStream(file);
            }

            console.WrapLine("{0} Uploaded to {1} as {2}.", File.White(), Container.White(), BlobName.White());

            AccountOptions.SaveToSettings();
        }
    }
    [Command]
    [Description("Download a file.")]
    class DownloadCommand
    {
        [Positional]
        [Description("The name of the container for the blob.")]
        public string Container { get; set; }

        [Positional]
        [Description("The name of the blob you wish to download.")]
        public string BlobName { get; set; }

        [Positional]
        [Description("The local file to create.")]
        public string File { get; set; }


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

            var blobRef = container.GetBlockBlobReference(BlobName);
            blobRef.DownloadToFile(File, FileMode.Create);

            console.WrapLine("{0} Downloaded from {1} to {2}.", Container.White(), BlobName.White(), File.White());

            AccountOptions.SaveToSettings();
        }
    }
}
