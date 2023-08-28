using CurseForge.APIClient;
using CurseForge.APIClient.Models;
using CurseForge.APIClient.Models.Minecraft;
using MaterialSkin;
using MaterialSkin.Controls;
using MC_Java_Srv_GUI.Core;
using MC_Java_Srv_GUI.View;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace MC_Java_Srv_GUI.Curse
{
    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }

    public static class ForgeAPI
    {
        // Minecraft ID = 432

        // Global
        private static ApiClient cfApiClient = null;
        private static GenericListResponse<MinecraftModloaderInfoListItem> ForgeVersions = null;
        public static GenericListResponse<MinecraftVersionInfo> MinecraftVersions = null;
        public static GenericResponse<MinecraftModloaderInfo> selectedForge = null;

        // =============
        // Curse Server
        // =============

        // Init Curse and Populate
        public static async void InitCurse(string apiKey, bool bypasssave = false)
        {
            try
            {
                Form1.CurrentForm.materialButton55.Enabled = false;
                cfApiClient = new ApiClient(apiKey);
                MinecraftVersions = await cfApiClient.GetMinecraftVersions(true);
                foreach (var Version in MinecraftVersions.Data)
                    Form1.CurrentForm.materialComboBox3.Items.Add(Version.VersionString);
                Form1.CurrentForm.materialTextBox23.Enabled = false;
                Form1.CurrentForm.materialComboBox3.Enabled = true;
                Form1.CurrentForm.materialButton58.Enabled = true;
                if (!bypasssave)
                    ConfigHandler.SaveApiKey(apiKey);
            }
            catch
            {
                Form1.CurrentForm.materialTextBox23.Enabled = true;
                Form1.CurrentForm.materialTextBox23.Text = "";
                Form1.CurrentForm.materialButton55.Enabled = true;
                MaterialMessageBox.Show("Wrong API Key Used.", false);
            }
        }

        // Populate Forge Versions
        public static async void SpawnForges(string gameVersion)
        {
            Form1.CurrentForm.materialComboBox4.Enabled = true;
            if (Form1.CurrentForm.materialComboBox4.Items.Count > 0)
                Form1.CurrentForm.materialComboBox4.Items.Clear();
            ForgeVersions = await cfApiClient.GetMinecraftModloaders(gameVersion, true);
            foreach (var Version in ForgeVersions.Data)
            {
                if (Version.Type == CurseForge.APIClient.Models.Enums.CoreModloaderType.Forge)
                    Form1.CurrentForm.materialComboBox4.Items.Add(Version.Name);
            }
        }

        // Select Forge version
        public static async void FetchForge(string forgeVersion)
        {
            selectedForge = await cfApiClient.GetSpecificMinecraftModloaderInfo(forgeVersion);
            Form1.CurrentForm.materialTextBox24.Enabled = true;
        }

        // =================
        // End Curse Server
        // =================

        // ===========
        // Curse Mods
        // ===========

        // Globals
        private static string SelectedPath = "";
        public static GenericListResponse<CurseForge.APIClient.Models.Mods.Mod> foundMods;

        // Selected Server -> Show Installed Mods
        public static IEnumerable<string> SelectServer2Mods(string servername)
        {
            string ServerPath = ConfigHandler.serverfolders[servername];
            string Dir = Directory.GetDirectories(ServerPath)[0];
            string ModPath = Dir + "\\" + "mods";
            SelectedPath = ModPath;
            if (Directory.Exists(ModPath))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(ModPath, "*.jar");
                return files;
            }
            else
            {
                MaterialMessageBox.Show("Please Select a Forge Server", false);
                return null;
            }
        }

        // Delete Selected Local Mod
        public static void DeleteSelectedMod(string modname)
        {
            if (File.Exists(SelectedPath + "\\" + modname))
                File.Delete(SelectedPath + "\\" + modname);
        }

        // Search Mod via Filter (GameID && MC Version && Name)
        public static async void SearchMods(string version, CurseForge.APIClient.Models.Mods.ModsSearchSortField sort, string filter = null)
        {
            foundMods = await cfApiClient.SearchModsAsync(432, null, null, version, filter, sort, CurseForge.APIClient.Models.Mods.ModsSearchSortOrder.Descending, CurseForge.APIClient.Models.Mods.ModLoaderType.Forge, null, null, null, 25);
            foreach (var mod in foundMods.Data)
            {
                CurseModManager.CurrentForm.mod_ids.Add(mod.Name, mod.Id);
                CurseModManager.CurrentForm.materialListBox2.Items.Add(new MaterialListBoxItem(mod.Name));
            }
        }

        // Download Mod
        public static async void DownloadSelectedMod(int modID)
        {
            var mod = await cfApiClient.GetModAsync(modID);
            WebClient client = new WebClient();
            client.DownloadFile(mod.Data.LatestFiles[0].DownloadUrl, SelectedPath + "\\" + mod.Data.LatestFiles[0].DisplayName);
            // Extract Mod Pack
            if (mod.Data.LatestFiles[0].DownloadUrl.EndsWith(".zip"))
            {
                var zipFile = ZipFile.Open(SelectedPath + "\\" + mod.Data.LatestFiles[0].DisplayName, ZipArchiveMode.Read);
                zipFile.ExtractToDirectory(SelectedPath, true);
                zipFile.Dispose();
                File.Delete(SelectedPath + "\\" + mod.Data.LatestFiles[0].DisplayName);
            }
            // Fix File name
            else if (!mod.Data.LatestFiles[0].DisplayName.EndsWith(".jar"))
                File.Move(SelectedPath + "\\" + mod.Data.LatestFiles[0].DisplayName, SelectedPath + "\\" + mod.Data.LatestFiles[0].DisplayName + ".jar");
            CurseModManager.CurrentForm.RefreshLocalMods();
            CurseModManager.CurrentForm.materialButton3.Enabled = true;
        }

        // ===============
        // End Curse Mods
        // ===============
    }
}
