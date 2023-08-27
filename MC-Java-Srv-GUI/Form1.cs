using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using MaterialSkin;
using MaterialSkin.Controls;
using MC_Java_Srv_GUI.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MC_Java_Srv_GUI
{
    public partial class Form1 : MaterialForm
    {
        // Globals
        public readonly MaterialSkinManager materialSkinManager;
        private string ServersFolder = "";
        public static Form1 CurrentForm;
        // End Globals

        public Form1()
        {
            InitializeComponent();
            // Set static form
            CurrentForm = this;
            // Set material colors
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Load Version List for downloader
            DownloaderHandler.Ini();
            PopulateDLVersions();
            // Prepare for downloads
            CreateFolders();
            // Ini & Read Config
            ConfigHandler.ReadConfig();
            // Show detected servers
            PopulateServers();
            // Populate Local Servers
            PopulateLocalServers();
            // Set App Version
            AppVer();
        }

        // ====================
        //  Page: Welcome Page
        // ====================

        // LETS GO Button
        private void materialButton3_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedIndex = 1;
        }

        // VerStr
        private void AppVer()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            materialLabel2.Text = materialLabel2.Text.Replace("VerStr", version);
        }

        // ====================
        //   End Welcome Page
        // ====================

        // ====================
        //    Page: DLs Page
        // ====================

        private void CreateFolders()
        {
            if (!Directory.Exists(Application.StartupPath + "\\Servers"))
                Directory.CreateDirectory(Application.StartupPath + "\\Servers");
            ServersFolder = Application.StartupPath + "\\Servers\\";
        }

        private void PopulateDLVersions()
        {
            materialComboBox2.Items.AddRange(DownloaderHandler.VersionList.Keys.ToArray());
            materialComboBox2.SelectedIndex = 0;
            materialLabel39.Text += DownloaderHandler.LatestVersion;
        }

        // Textbox for naming your server
        private void materialTextBox21_TextChanged(object sender, EventArgs e)
        {
            if (materialTextBox21.Text.Length > 0 && !Directory.Exists(ServersFolder + materialTextBox21.Text))
            {
                pictureBox2.Image = imageList2.Images[1];
                materialButton8.Enabled = true;
            }
            else
            {
                pictureBox2.Image = imageList2.Images[0];
                materialButton8.Enabled = false;
            }
        }

        // Download Button
        private void materialButton8_Click(object sender, EventArgs e)
        {
            materialButton8.Enabled = false;
            // Check Limit
            int curr = 0;
            if (materialExpansionPanel1.Visible)
                curr++;
            if (materialExpansionPanel2.Visible)
                curr++;
            if (materialExpansionPanel3.Visible)
                curr++;
            if (materialExpansionPanel4.Visible)
                curr++;
            if (materialExpansionPanel5.Visible)
                curr++;
            if (materialExpansionPanel6.Visible)
                curr++;
            if (materialExpansionPanel7.Visible)
                curr++;
            if (materialExpansionPanel8.Visible)
                curr++;
            if (materialExpansionPanel9.Visible)
                curr++;
            if (materialExpansionPanel10.Visible)
                curr++;
            if (curr == 10)
            {
                MaterialMessageBox.Show("Limit of Managed Servers Reached! :C", false);
                materialButton8.Enabled = true;
                return;
            }
            // Check if any server is online
            if (CheckActiveServers())
            {
                MaterialMessageBox.Show("Please stop any running servers before proceeding! :)", false);
                materialButton8.Enabled = true;
                return;
            }
            // Start/Proceed
            startDownload(DownloaderHandler.VersionList[materialComboBox2.SelectedItem.ToString()], materialComboBox2.SelectedItem.ToString(), materialTextBox21.Text);
        }

        // Check Active Servers
        private bool CheckActiveServers()
        {
            if ((ConfigHandler.Server1 != null && ConfigHandler.Server1.Online) ||
                (ConfigHandler.Server2 != null && ConfigHandler.Server2.Online) ||
                (ConfigHandler.Server3 != null && ConfigHandler.Server3.Online) ||
                (ConfigHandler.Server4 != null && ConfigHandler.Server4.Online) ||
                (ConfigHandler.Server5 != null && ConfigHandler.Server5.Online) ||
                (ConfigHandler.Server6 != null && ConfigHandler.Server6.Online) ||
                (ConfigHandler.Server7 != null && ConfigHandler.Server7.Online) ||
                (ConfigHandler.Server8 != null && ConfigHandler.Server8.Online) ||
                (ConfigHandler.Server9 != null && ConfigHandler.Server9.Online) ||
                (ConfigHandler.Server10 != null && ConfigHandler.Server10.Online))
                return true;
            else 
                return false;
        }

        public void startDownload(string url, string version, string naming)
        {
            // Global the values
            tmp_name = naming;
            tmp_path = ServersFolder + naming;
            // Create needed folder for file drop
            Directory.CreateDirectory(ServersFolder + naming + "\\" + version);
            // Download file to folder...
            Thread thread = new Thread(() => {
                CheckForIllegalCrossThreadCalls = false;
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(url), ServersFolder + naming + "\\" + version + "\\server.jar");
            });
            thread.Start();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                CheckForIllegalCrossThreadCalls = false;
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                materialButton8.Text = Math.Truncate(percentage).ToString();
                materialProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }

        string tmp_name = "";
        string tmp_path = "";
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                CheckForIllegalCrossThreadCalls = false;
                materialButton8.Text = "Download";
                materialButton8.Enabled = true;
                materialTextBox21.Text = "";
                materialProgressBar1.Value = 0;
                // Add to config
                ConfigHandler.WriteConfig(tmp_name, tmp_path);
                // Reset Manager
                ResetServers();
                // Reset Local Servers
                ResetLocalServers();
            });
        }

        // ====================
        //     End DLs Page
        // ====================

        // ====================
        // Page: Server Manager
        // ====================

        // Delete Selected Server from Downloader
        private void materialButton54_Click(object sender, EventArgs e)
        {
            if (materialComboBox1.SelectedItem != null && !CheckActiveServers())
                ConfigHandler.DeleteServer();
        }

        // Reset Local Servers in Downloader
        public void ResetLocalServers()
        {
            materialComboBox1.Items.Clear();
            PopulateLocalServers();
        }

        // Populate Local Servers in Downloader
        private void PopulateLocalServers()
        {
            foreach (string name in ConfigHandler.serverfolders.Keys)
                materialComboBox1.Items.Add(name);
        }

        // Reset to Repopulate detected servers
        public void ResetServers()
        {
            materialExpansionPanel1.Visible = false;
            ConfigHandler.Spot1 = false;
            materialExpansionPanel2.Visible = false;
            ConfigHandler.Spot2 = false;
            materialExpansionPanel3.Visible = false;
            ConfigHandler.Spot3 = false;
            materialExpansionPanel4.Visible = false;
            ConfigHandler.Spot4 = false;
            materialExpansionPanel5.Visible = false;
            ConfigHandler.Spot5 = false;
            materialExpansionPanel6.Visible = false;
            ConfigHandler.Spot6 = false;
            materialExpansionPanel7.Visible = false;
            ConfigHandler.Spot7 = false;
            materialExpansionPanel8.Visible = false;
            ConfigHandler.Spot8 = false;
            materialExpansionPanel9.Visible = false;
            ConfigHandler.Spot9 = false;
            materialExpansionPanel10.Visible = false;
            ConfigHandler.Spot10 = false;
            PopulateServers();
        }

        // Populate detected servers into bubbles
        private void PopulateServers()
        {
            // Read Config 
            foreach (string servername in ConfigHandler.serverfolders.Keys)
            {
                string name = servername;
                // Set Read Config
                if (!materialExpansionPanel1.Visible && !ConfigHandler.Spot1)
                {
                    ConfigHandler.Server1 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot1 = true;
                    materialExpansionPanel1.Visible = true;
                    materialExpansionPanel1.Title = name;
                    materialLabel4.Text = "Managed: Yes";
                    materialLabel1.Text = $"Version: {ConfigHandler.Server1.Version}";
                    materialLabel5.Text = $"Port: {ConfigHandler.Server1.Port}";
                }
                else if (!materialExpansionPanel2.Visible && !ConfigHandler.Spot2)
                {
                    ConfigHandler.Server2 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot2 = true;
                    materialExpansionPanel2.Visible = true;
                    materialExpansionPanel2.Title = name;
                    materialLabel6.Text = "Managed: Yes";
                    materialLabel7.Text = $"Version: {ConfigHandler.Server2.Version}";
                    materialLabel8.Text = $"Port: {ConfigHandler.Server2.Port}";
                }
                else if (!materialExpansionPanel3.Visible && !ConfigHandler.Spot3)
                {
                    ConfigHandler.Server3 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot3 = true;
                    materialExpansionPanel3.Visible = true;
                    materialExpansionPanel3.Title = name;
                    materialLabel9.Text = "Managed: Yes";
                    materialLabel10.Text = $"Version: {ConfigHandler.Server3.Version}";
                    materialLabel11.Text = $"Port: {ConfigHandler.Server3.Port}";
                }
                else if (!materialExpansionPanel4.Visible && !ConfigHandler.Spot4)
                {
                    ConfigHandler.Server4 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot4 = true;
                    materialExpansionPanel4.Visible = true;
                    materialExpansionPanel4.Title = name;
                    materialLabel12.Text = "Managed: Yes";
                    materialLabel13.Text = $"Version: {ConfigHandler.Server4.Version}";
                    materialLabel14.Text = $"Port: {ConfigHandler.Server4.Port}";
                }
                else if (!materialExpansionPanel5.Visible && !ConfigHandler.Spot5)
                {
                    ConfigHandler.Server5 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot5 = true;
                    materialExpansionPanel5.Visible = true;
                    materialExpansionPanel5.Title = name;
                    materialLabel15.Text = "Managed: Yes";
                    materialLabel16.Text = $"Version: {ConfigHandler.Server5.Version}";
                    materialLabel17.Text = $"Port: {ConfigHandler.Server5.Port}";
                }
                else if (!materialExpansionPanel6.Visible && !ConfigHandler.Spot6)
                {
                    ConfigHandler.Server6 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot6 = true;
                    materialExpansionPanel6.Visible = true;
                    materialExpansionPanel6.Title = name;
                    materialLabel18.Text = "Managed: Yes";
                    materialLabel19.Text = $"Version: {ConfigHandler.Server6.Version}";
                    materialLabel20.Text = $"Port: {ConfigHandler.Server6.Port}";
                }
                else if (!materialExpansionPanel7.Visible && !ConfigHandler.Spot7)
                {
                    ConfigHandler.Server7 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot7 = true;
                    materialExpansionPanel7.Visible = true;
                    materialExpansionPanel7.Title = name;
                    materialLabel21.Text = "Managed: Yes";
                    materialLabel22.Text = $"Version: {ConfigHandler.Server7.Version}";
                    materialLabel23.Text = $"Port: {ConfigHandler.Server7.Port}";
                }
                else if (!materialExpansionPanel8.Visible && !ConfigHandler.Spot8)
                {
                    ConfigHandler.Server8 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot8 = true;
                    materialExpansionPanel8.Visible = true;
                    materialExpansionPanel8.Title = name;
                    materialLabel24.Text = "Managed: Yes";
                    materialLabel25.Text = $"Version: {ConfigHandler.Server8.Version}";
                    materialLabel26.Text = $"Port: {ConfigHandler.Server8.Port}";
                }
                else if (!materialExpansionPanel9.Visible && !ConfigHandler.Spot9)
                {
                    ConfigHandler.Server9 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot9 = true;
                    materialExpansionPanel9.Visible = true;
                    materialExpansionPanel9.Title = name;
                    materialLabel27.Text = "Managed: Yes";
                    materialLabel28.Text = $"Version: {ConfigHandler.Server9.Version}";
                    materialLabel29.Text = $"Port: {ConfigHandler.Server9.Port}";
                }
                else if (!materialExpansionPanel10.Visible && !ConfigHandler.Spot10)
                {
                    ConfigHandler.Server10 = new ServerItem(ConfigHandler.serverfolders[name], name);
                    ConfigHandler.Spot10 = true;
                    materialExpansionPanel10.Visible = true;
                    materialExpansionPanel10.Title = name;
                    materialLabel30.Text = "Managed: Yes";
                    materialLabel31.Text = $"Version: {ConfigHandler.Server10.Version}";
                    materialLabel32.Text = $"Port: {ConfigHandler.Server10.Port}";
                }
            }
        }

        // Expand Action
        private void materialExpansionPanel1_PanelExpand(object sender, EventArgs e)
        {
            // Bring to Front
            var obj = sender as MaterialExpansionPanel;
            obj.BringToFront();
            // Collapse Others
            if (obj.Title != materialExpansionPanel1.Title)
                materialExpansionPanel1.Collapse = true;
            if (obj.Title != materialExpansionPanel2.Title)
                materialExpansionPanel2.Collapse = true;
            if (obj.Title != materialExpansionPanel3.Title)
                materialExpansionPanel3.Collapse = true;
            if (obj.Title != materialExpansionPanel4.Title)
                materialExpansionPanel4.Collapse = true;
            if (obj.Title != materialExpansionPanel5.Title)
                materialExpansionPanel5.Collapse = true;
            if (obj.Title != materialExpansionPanel6.Title)
                materialExpansionPanel6.Collapse = true;
            if (obj.Title != materialExpansionPanel7.Title)
                materialExpansionPanel7.Collapse = true;
            if (obj.Title != materialExpansionPanel8.Title)
                materialExpansionPanel8.Collapse = true;
            if (obj.Title != materialExpansionPanel9.Title)
                materialExpansionPanel9.Collapse = true;
            if (obj.Title != materialExpansionPanel10.Title)
                materialExpansionPanel10.Collapse = true;
        }

        // View server stats
        private void materialButton4_Click(object sender, EventArgs e)
        {
            var obj = sender as MaterialButton;
            switch (obj.Name)
            {
                // Server 1
                case "materialButton4":
                    ConfigHandler.Server1.GetStats();
                    break;
                // Server 2
                case "materialButton12":
                    ConfigHandler.Server2.GetStats();
                    break;
                // Server 3
                case "materialButton17":
                    ConfigHandler.Server3.GetStats();
                    break;
                // Server 4
                case "materialButton22":
                    ConfigHandler.Server4.GetStats();
                    break;
                // Server 5
                case "materialButton27":
                    ConfigHandler.Server5.GetStats();
                    break;
                // Server 6
                case "materialButton32":
                    ConfigHandler.Server6.GetStats();
                    break;
                // Server 7
                case "materialButton37":
                    ConfigHandler.Server7.GetStats();
                    break;
                // Server 8
                case "materialButton42":
                    ConfigHandler.Server8.GetStats();
                    break;
                // Server 9
                case "materialButton47":
                    ConfigHandler.Server9.GetStats();
                    break;
                // Server 10
                case "materialButton52":
                    ConfigHandler.Server10.GetStats();
                    break;
            }
        }

        // Start Server Button
        private void materialButton2_Click(object sender, EventArgs e)
        {
            var obj = sender as MaterialButton;
            switch (obj.Name)
            {
                // Server 1
                case "materialButton2":
                    ConfigHandler.Server1.StartServer();
                    if (ConfigHandler.Server1.Online)
                    {
                        materialButton4.Enabled = true;
                        materialButton6.Enabled = true;
                        materialButton2.Enabled = false;
                        materialButton1.Enabled = true;
                        materialExpansionPanel1.Description = "Online";
                        if (materialLabel5.Text.Contains("EULA"))
                            materialLabel5.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 2
                case "materialButton9":
                    ConfigHandler.Server2.StartServer();
                    if (ConfigHandler.Server2.Online)
                    {
                        materialButton11.Enabled = true;
                        materialButton12.Enabled = true;
                        materialButton9.Enabled = false;
                        materialButton10.Enabled = true;
                        materialExpansionPanel2.Description = "Online";
                        if (materialLabel8.Text.Contains("EULA"))
                            materialLabel8.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 3
                case "materialButton14":
                    ConfigHandler.Server3.StartServer();
                    if (ConfigHandler.Server3.Online)
                    {
                        materialButton16.Enabled = true;
                        materialButton17.Enabled = true;
                        materialButton14.Enabled = false;
                        materialButton15.Enabled = true;
                        materialExpansionPanel3.Description = "Online";
                        if (materialLabel11.Text.Contains("EULA"))
                            materialLabel11.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 4
                case "materialButton19":
                    ConfigHandler.Server4.StartServer();
                    if (ConfigHandler.Server4.Online)
                    {
                        materialButton21.Enabled = true;
                        materialButton22.Enabled = true;
                        materialButton19.Enabled = false;
                        materialButton20.Enabled = true;
                        materialExpansionPanel4.Description = "Online";
                        if (materialLabel14.Text.Contains("EULA"))
                            materialLabel14.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 5
                case "materialButton24":
                    ConfigHandler.Server5.StartServer();
                    if (ConfigHandler.Server5.Online)
                    {
                        materialButton26.Enabled = true;
                        materialButton27.Enabled = true;
                        materialButton24.Enabled = false;
                        materialButton25.Enabled = true;
                        materialExpansionPanel5.Description = "Online";
                        if (materialLabel17.Text.Contains("EULA"))
                            materialLabel17.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 6
                case "materialButton29":
                    ConfigHandler.Server6.StartServer();
                    if (ConfigHandler.Server6.Online)
                    {
                        materialButton31.Enabled = true;
                        materialButton32.Enabled = true;
                        materialButton29.Enabled = false;
                        materialButton30.Enabled = true;
                        materialExpansionPanel6.Description = "Online";
                        if (materialLabel20.Text.Contains("EULA"))
                            materialLabel20.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 7
                case "materialButton34":
                    ConfigHandler.Server7.StartServer();
                    if (ConfigHandler.Server7.Online)
                    {
                        materialButton36.Enabled = true;
                        materialButton37.Enabled = true;
                        materialButton34.Enabled = false;
                        materialButton35.Enabled = true;
                        materialExpansionPanel7.Description = "Online";
                        if (materialLabel23.Text.Contains("EULA"))
                            materialLabel23.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 8
                case "materialButton39":
                    ConfigHandler.Server8.StartServer();
                    if (ConfigHandler.Server8.Online)
                    {
                        materialButton41.Enabled = true;
                        materialButton42.Enabled = true;
                        materialButton39.Enabled = false;
                        materialButton40.Enabled = true;
                        materialExpansionPanel8.Description = "Online";
                        if (materialLabel26.Text.Contains("EULA"))
                            materialLabel26.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 9
                case "materialButton44":
                    ConfigHandler.Server9.StartServer();
                    if (ConfigHandler.Server9.Online)
                    {
                        materialButton46.Enabled = true;
                        materialButton47.Enabled = true;
                        materialButton44.Enabled = false;
                        materialButton45.Enabled = true;
                        materialExpansionPanel9.Description = "Online";
                        if (materialLabel29.Text.Contains("EULA"))
                            materialLabel29.Text = "Port: 25565";
                    }
                    break;
                //
                // Server 10
                case "materialButton49":
                    ConfigHandler.Server10.StartServer();
                    if (ConfigHandler.Server10.Online)
                    {
                        materialButton51.Enabled = true;
                        materialButton52.Enabled = true;
                        materialButton49.Enabled = false;
                        materialButton50.Enabled = true;
                        materialExpansionPanel10.Description = "Online";
                        if (materialLabel32.Text.Contains("EULA"))
                            materialLabel32.Text = "Port: 25565";
                    }
                    break;
                //
            }
        }

        // Stop Server Button
        private void materialButton1_Click(object sender, EventArgs e)
        {
            var obj = sender as MaterialButton;
            switch (obj.Name)
            {
                // Server 1
                case "materialButton1":
                    ConfigHandler.Server1.StopServer();
                    if (!ConfigHandler.Server1.Online)
                    {
                        ConfigHandler.Server1.ResetConsole();
                        materialButton4.Enabled = false;
                        materialButton6.Enabled = false;
                        materialButton2.Enabled = true;
                        materialButton1.Enabled = false;
                        materialExpansionPanel1.Description = "Offline";
                        materialExpansionPanel1.Refresh();
                    }
                    break;
                //
                // Server 2
                case "materialButton10":
                    ConfigHandler.Server2.StopServer();
                    if (!ConfigHandler.Server2.Online)
                    {
                        ConfigHandler.Server2.ResetConsole();
                        materialButton11.Enabled = false;
                        materialButton12.Enabled = false;
                        materialButton9.Enabled = true;
                        materialButton10.Enabled = false;
                        materialExpansionPanel2.Description = "Offline";
                        materialExpansionPanel2.Refresh();
                    }
                    break;
                //
                // Server 3
                case "materialButton15":
                    ConfigHandler.Server3.StopServer();
                    if (!ConfigHandler.Server3.Online)
                    {
                        ConfigHandler.Server3.ResetConsole();
                        materialButton16.Enabled = false;
                        materialButton17.Enabled = false;
                        materialButton14.Enabled = true;
                        materialButton15.Enabled = false;
                        materialExpansionPanel3.Description = "Offline";
                        materialExpansionPanel3.Refresh();
                    }
                    break;
                //
                // Server 4
                case "materialButton20":
                    ConfigHandler.Server4.StopServer();
                    if (!ConfigHandler.Server4.Online)
                    {
                        ConfigHandler.Server4.ResetConsole();
                        materialButton21.Enabled = false;
                        materialButton22.Enabled = false;
                        materialButton19.Enabled = true;
                        materialButton20.Enabled = false;
                        materialExpansionPanel4.Description = "Offline";
                        materialExpansionPanel4.Refresh();
                    }
                    break;
                //
                // Server 5
                case "materialButton25":
                    ConfigHandler.Server5.StopServer();
                    if (!ConfigHandler.Server5.Online)
                    {
                        ConfigHandler.Server5.ResetConsole();
                        materialButton26.Enabled = false;
                        materialButton27.Enabled = false;
                        materialButton24.Enabled = true;
                        materialButton25.Enabled = false;
                        materialExpansionPanel5.Description = "Offline";
                        materialExpansionPanel5.Refresh();
                    }
                    break;
                //
                // Server 6
                case "materialButton30":
                    ConfigHandler.Server6.StopServer();
                    if (!ConfigHandler.Server6.Online)
                    {
                        ConfigHandler.Server6.ResetConsole();
                        materialButton31.Enabled = false;
                        materialButton32.Enabled = false;
                        materialButton29.Enabled = true;
                        materialButton30.Enabled = false;
                        materialExpansionPanel6.Description = "Offline";
                        materialExpansionPanel6.Refresh();
                    }
                    break;
                //
                // Server 7
                case "materialButton35":
                    ConfigHandler.Server7.StopServer();
                    if (!ConfigHandler.Server7.Online)
                    {
                        ConfigHandler.Server7.ResetConsole();
                        materialButton36.Enabled = false;
                        materialButton37.Enabled = false;
                        materialButton34.Enabled = true;
                        materialButton35.Enabled = false;
                        materialExpansionPanel7.Description = "Offline";
                        materialExpansionPanel7.Refresh();
                    }
                    break;
                //
                // Server 8
                case "materialButton40":
                    ConfigHandler.Server8.StopServer();
                    if (!ConfigHandler.Server8.Online)
                    {
                        ConfigHandler.Server8.ResetConsole();
                        materialButton41.Enabled = false;
                        materialButton42.Enabled = false;
                        materialButton39.Enabled = true;
                        materialButton40.Enabled = false;
                        materialExpansionPanel8.Description = "Offline";
                        materialExpansionPanel8.Refresh();
                    }
                    break;
                //
                // Server 9
                case "materialButton45":
                    ConfigHandler.Server9.StopServer();
                    if (!ConfigHandler.Server9.Online)
                    {
                        ConfigHandler.Server9.ResetConsole();
                        materialButton46.Enabled = false;
                        materialButton47.Enabled = false;
                        materialButton44.Enabled = true;
                        materialButton45.Enabled = false;
                        materialExpansionPanel9.Description = "Offline";
                        materialExpansionPanel9.Refresh();
                    }
                    break;
                //
                // Server 10
                case "materialButton50":
                    ConfigHandler.Server10.StopServer();
                    if (!ConfigHandler.Server10.Online)
                    {
                        ConfigHandler.Server10.ResetConsole();
                        materialButton51.Enabled = false;
                        materialButton52.Enabled = false;
                        materialButton49.Enabled = true;
                        materialButton50.Enabled = false;
                        materialExpansionPanel10.Description = "Offline";
                        materialExpansionPanel10.Refresh();
                    }
                    break;
                //
            }
        }

        // View Server Console
        private void materialButton6_Click(object sender, EventArgs e)
        {
            var obj = sender as MaterialButton;
            switch (obj.Name)
            {
                // Server 1
                case "materialButton6":
                    ConfigHandler.Server1.ConsoleOutput();
                    break;
                // Server 2
                case "materialButton11":
                    ConfigHandler.Server2.ConsoleOutput();
                    break;
                // Server 3
                case "materialButton16":
                    ConfigHandler.Server3.ConsoleOutput();
                    break;
                // Server 4
                case "materialButton21":
                    ConfigHandler.Server4.ConsoleOutput();
                    break;
                // Server 5
                case "materialButton26":
                    ConfigHandler.Server5.ConsoleOutput();
                    break;
                // Server 6
                case "materialButton31":
                    ConfigHandler.Server6.ConsoleOutput();
                    break;
                // Server 7
                case "materialButton36":
                    ConfigHandler.Server7.ConsoleOutput();
                    break;
                // Server 8
                case "materialButton41":
                    ConfigHandler.Server8.ConsoleOutput();
                    break;
                // Server 9
                case "materialButton46":
                    ConfigHandler.Server9.ConsoleOutput();
                    break;
                // Server 10
                case "materialButton51":
                    ConfigHandler.Server10.ConsoleOutput();
                    break;
            }
        }

        // ====================
        //  End Server Manager
        // ====================

        // ====================
        // Configs Handler
        // ====================

        // Save Config
        private void materialButton7_Click(object sender, EventArgs e) =>
            File.WriteAllText(materialTextBox22.Text, materialMultiLineTextBox21.Text);

        // View Server Console
        private void materialButton5_Click(object sender, EventArgs e)
        {
            var obj = sender as MaterialButton;
            switch (obj.Name)
            {
                // Server 1
                case "materialButton5":
                    ConfigHandler.Server1.OpenConfig();
                    break;
                // Server 2
                case "materialButton13":
                    ConfigHandler.Server2.OpenConfig();
                    break;
                // Server 3
                case "materialButton18":
                    ConfigHandler.Server3.OpenConfig();
                    break;
                // Server 4
                case "materialButton23":
                    ConfigHandler.Server4.OpenConfig();
                    break;
                // Server 5
                case "materialButton28":
                    ConfigHandler.Server5.OpenConfig();
                    break;
                // Server 6
                case "materialButton33":
                    ConfigHandler.Server6.OpenConfig();
                    break;
                // Server 7
                case "materialButton38":
                    ConfigHandler.Server7.OpenConfig();
                    break;
                // Server 8
                case "materialButton43":
                    ConfigHandler.Server8.OpenConfig();
                    break;
                // Server 9
                case "materialButton48":
                    ConfigHandler.Server9.OpenConfig();
                    break;
                // Server 10
                case "materialButton53":
                    ConfigHandler.Server10.OpenConfig();
                    break;
            }
            materialTabControl1.SelectedIndex = 3;
        }

        // ====================
        // End Configs Handler
        // ====================
    }
}
