using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows;

namespace DataLakeFileAccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ResourceManager rm = new ResourceManager("DataLakeFileAccess.Properties.Resources", Assembly.GetExecutingAssembly());
        private static DataLakeStoreAccountManagementClient adlsClient = null;
        private static DataLakeStoreFileSystemManagementClient adlsFileSystemClient = null;

        private static string acountName = String.Empty;
        private static string resourceGroupName = String.Empty;
        private static string location = String.Empty;
        private static string subscription = String.Empty;
        private static string tenant = String.Empty;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = rm.GetString("title");
            this.adlsAccountName.Text = rm.GetString("adlsAccountName");
            this.tenantID.Text = rm.GetString("tenantID");
            this.localFolder.Text = rm.GetString("localFolder");
            this.localFile.Text = rm.GetString("localFile");
            this.remoteFile.Text = rm.GetString("remoteFile");
            this.txtAdlsAccountName.Text = rm.GetString("txtAdlsAccountName");
            this.txtTenantID.Text = rm.GetString("txtTenantID");
            this.txtLocalFolder.Text = rm.GetString("txtLocalFolder");
            this.txtLocalFile.Text = rm.GetString("txtLocalFile");
            this.txtRemoteFile.Text = rm.GetString("txtRemoteFile");
            this.fileMenuItem.Header = rm.GetString("txtFileMenuItem");
            this.uploadMenuItem.Header = rm.GetString("txtUploadMenuItem");
            this.downloadMenuItem.Header = rm.GetString("txtDownloadMenuItem");
            this.listFilesMenuItem.Header = rm.GetString("txtListFilesMenuItem");
            this.getFileInfoMenuItem.Header = rm.GetString("txtGetFileInfoMenuItem");
            this.exitMenuItem.Header = rm.GetString("txtExitMenuItem");
            this.securityMenuItem.Header = rm.GetString("txtSecurityMenuItem");
            this.connectMenuItem.Header = rm.GetString("txtConnectMenuItem");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // User login via interactive popup
                // Authenticate using an an Azure AD domain and client ID that is available by default for all Azure subscriptions
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                tenant = this.txtTenantID.Text;
                var nativeClientApp_clientId = "1950a258-227b-4e31-a9cf-717495945fc2";
                var activeDirectoryClientSettings = ActiveDirectoryClientSettings.UsePromptOnly(nativeClientApp_clientId, new Uri("urn:ietf:wg:oauth:2.0:oob"));
                var creds = UserTokenProvider.LoginWithPromptAsync(tenant, activeDirectoryClientSettings).Result;
                this.uploadMenuItem.IsEnabled = true;
                this.downloadMenuItem.IsEnabled = true;
                this.listFilesMenuItem.IsEnabled = true;
                this.getFileInfoMenuItem.IsEnabled = true;

                // Create client objects and set the subscription ID
                adlsClient = new DataLakeStoreAccountManagementClient(creds);
                adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(creds);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}", "Connect Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UploadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Upload a file
            try
            {
                string srcFilePath = $@"{this.txtLocalFolder.Text}\{this.txtLocalFile.Text}";
                adlsFileSystemClient.FileSystem.UploadFile(this.txtAdlsAccountName.Text, srcFilePath, this.txtRemoteFile.Text, overwrite: true);
                MessageBox.Show("File Uploaded", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}", "Upload Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Download file
                string destFilePath = $@"{this.txtLocalFolder.Text}\{this.txtLocalFile.Text}";
                adlsFileSystemClient.FileSystem.DownloadFile(txtAdlsAccountName.Text, txtRemoteFile.Text, $"{destFilePath}");
                MessageBox.Show("File Downloaded", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}", "Download Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // List files and directories
            try
            {
                var listFileStatuses = adlsFileSystemClient.FileSystem.ListFileStatus(txtAdlsAccountName.Text, txtRemoteFile.Text).FileStatuses.FileStatus.ToList();
                StringBuilder statusesStringBuilder = new StringBuilder();
                foreach (var listFileStatus in listFileStatuses)
                {
                    statusesStringBuilder.Append($"Name: {listFileStatus.PathSuffix}{Environment.NewLine}");
                    statusesStringBuilder.Append($"Owner: {listFileStatus.Owner}{Environment.NewLine}");
                    statusesStringBuilder.Append($"Permissions: {listFileStatus.Permission}{Environment.NewLine}");
                    statusesStringBuilder.Append($"Length: {listFileStatus.Length}{Environment.NewLine}");
                    statusesStringBuilder.Append($"Type: {listFileStatus.Type.ToString()}{Environment.NewLine}{Environment.NewLine}");
                }
                MessageBox.Show($"{statusesStringBuilder.ToString()}", $"Files in Folder {txtRemoteFile.Text}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}", "List Files Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GetFileInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Get file or directory info
            try
            {
                var statusInfo = await adlsFileSystemClient.FileSystem.GetFileStatusAsync(txtAdlsAccountName.Text, txtRemoteFile.Text);
                var info = statusInfo.FileStatus;
                string statusString = $"Access Time: {info.AccessTime}{Environment.NewLine}" +
                                      $"ACLs enabled: {info.AclBit}{Environment.NewLine}" +
                                      $"Block Size: {info.BlockSize}{Environment.NewLine}" +
                                      $"Expiration Time: {info.ExpirationTime}{Environment.NewLine}" +
                                      $"Group: {info.Group}{Environment.NewLine}" +
                                      $"Length: {info.Length}{Environment.NewLine}" +
                                      $"Modification Time: {info.ModificationTime}{Environment.NewLine}" +
                                      $"Owner: {info.Owner}{Environment.NewLine}" +
                                      $"Permissions: {info.Permission}{Environment.NewLine}" +
                                      $"Type: {info.Type.ToString()}{Environment.NewLine}";
                MessageBox.Show($"{statusString}", $"File or Folder Information for {txtRemoteFile.Text}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}", "Get File Info Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
