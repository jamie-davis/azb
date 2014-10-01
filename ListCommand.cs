using System.IO;
using System.Linq;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace azb
{
    [Command]
    [Description("List container contents.")]
    class ListCommand
    {
        [Positional]
        [Description("The name of the container to display.")]
        public string Container { get; set; }

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

            var listBlobItems = container.ListBlobs().ToList();
            var report = listBlobItems.Select(b => FetchBlockDetails(container, b))
                                      .AsReport(p => p.AddColumn(b => b.Name, col => col.Heading("Name"))
                                                      .AddColumn(b => b.Properties.ETag, col => col.Heading("ETag"))
                                                      .AddColumn(b => string.Join(", ", b.Metadata.Select(m => string.Format("{0} = {1}", m.Key, m.Value))), col => col.Heading("Metadata"))
                                                      .AddColumn(b => b.Properties.Length, col => col.Heading("Length"))
                                                      .AddColumn(b => b.Properties.LastModified, col => col.Heading("Modified"))
                );
            console.FormatTable(report);

            AccountOptions.SaveToSettings();
        }

        private static CloudBlockBlob FetchBlockDetails(CloudBlobContainer container, IListBlobItem listBlobItem)
        {
            var block = container.GetBlockBlobReference(Path.GetFileName(listBlobItem.StorageUri.PrimaryUri.LocalPath));
            block.FetchAttributes();
            return block;
        }
    }
}