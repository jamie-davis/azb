using System.Linq;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace azb
{
    [Command]
    [Description("List all of the containers in a storage account.")]
    class ContainersCommand
    {
        [OptionSet]
        public StorageAccountOptionSet AccountOptions { get; set; }

        [CommandHandler]
        public void Handle(IConsoleAdapter console)
        {
            if (AccountOptions.SettingsInvalid(console)) return;

            var credentials = new StorageCredentials(AccountOptions.StorageAccount, AccountOptions.StorageAccountKey);
            var account = new CloudStorageAccount(credentials, true);
            var blobClient = account.CreateCloudBlobClient();
            var containers = blobClient.ListContainers().ToList();

            var report = containers.Select(c => FetchContainerDetails(c))
                                   .AsReport(p => p.AddColumn(c => c.Name, col => col.Heading("Container"))
                                                   .AddColumn(b => b.Properties.ETag, col => col.Heading("ETag"))
                                                   .AddColumn(c => string.Join(", ", c.Metadata.Select(m => string.Format("{0} = {1}", m.Key, m.Value))), col => col.Heading("Metadata"))
                                                   .AddColumn(c => c.Properties.LastModified, col => col.Heading("Modified"))
                );
            console.FormatTable(report);

            AccountOptions.SaveToSettings();
        }

        private CloudBlobContainer FetchContainerDetails(CloudBlobContainer cloudBlobContainer)
        {
            cloudBlobContainer.FetchAttributes();
            return cloudBlobContainer;
        }
    }
}