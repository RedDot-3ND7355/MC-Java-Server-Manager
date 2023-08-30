using MaterialSkin;
using MaterialSkin.Controls;
using MC_Java_Srv_GUI.Curse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MC_Java_Srv_GUI.View
{
    public partial class CurseModManager : MaterialForm
    {
        // Globals
        public readonly MaterialSkinManager materialSkinManager;
        public static CurseModManager CurrentForm;
        public Dictionary<string, int> mod_ids = new Dictionary<string, int>(); // Name as Key, ID as Val
        // End Globals

        public CurseModManager()
        {
            InitializeComponent();
            // Set static form
            CurrentForm = this;
            // Set material colors
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Populate Local Servers
            PopulateServers();
            // Populate Versions
            PopulateVersions();
        }

        // Populate Versions
        private void PopulateVersions()
        {
            foreach (var Version in ForgeAPI.MinecraftVersions.Data)
                materialComboBox2.Items.Add(Version.VersionString);
        }

        // Populate Servers
        private void PopulateServers()
        {
            foreach (var Item in Form1.CurrentForm.materialComboBox1.Items)
                materialComboBox1.Items.Add(Item);
        }

        // Select Servers and show mods
        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e) =>
            RefreshLocalMods();

        // Select Local Mod
        private void materialListBox1_SelectedIndexChanged(object sender, MaterialListBoxItem selectedItem)
        {
            materialButton1.Enabled = true;
            materialButton2.Enabled = true;
        }

        // Delete Selected Mod
        private void materialButton2_Click(object sender, EventArgs e)
        {
            ForgeAPI.DeleteSelectedMod(materialListBox1.SelectedItem.Text);
            materialListBox1.RemoveItem(materialListBox1.SelectedItem);
        }

        // Version Select
        private void materialComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset
            if (materialListBox2.Items.Count > 0)
                materialListBox2.Items.Clear();
            mod_ids = new Dictionary<string, int>();
            // Continue
            materialTextBox21.Enabled = true;
            ForgeAPI.SearchMods(materialComboBox2.SelectedItem.ToString(), CurseForge.APIClient.Models.Mods.ModsSearchSortField.Featured);
        }

        // Search Function
        private void materialTextBox21_TextChanged(object sender, EventArgs e)
        {
            if (materialTextBox21.Text.Length > 0)
                materialButton4.Enabled = true;
        }

        // Search Button
        private void materialButton4_Click(object sender, EventArgs e) =>
            startSearchWithFilter();

        // Search Redirect
        private void materialTextBox21_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                startSearchWithFilter();
        }

        // Search Function
        private void startSearchWithFilter()
        {
            // Reset
            if (materialListBox2.Items.Count > 0)
                materialListBox2.Items.Clear();
            mod_ids = new Dictionary<string, int>();
            // Continue
            ForgeAPI.SearchMods(materialComboBox2.SelectedItem.ToString(), CurseForge.APIClient.Models.Mods.ModsSearchSortField.Popularity, materialTextBox21.Text);
        }

        // Select Mod
        private void materialListBox2_SelectedIndexChanged(object sender, MaterialListBoxItem selectedItem)
        {
            if (materialListBox2.SelectedIndex >= 0)
                materialButton3.Enabled = true;
            else
                materialButton3.Enabled = false;
        }

        // Install Mod
        private void materialButton3_Click(object sender, EventArgs e)
        {
            materialButton3.Enabled = false;
            if (materialComboBox1.SelectedItem != null)
                ForgeAPI.DownloadSelectedMod(mod_ids[materialListBox2.SelectedItem.Text]);
            else
                MaterialMessageBox.Show("Please Select Server to install to!", false);
        }

        // Refresh Mods
        public void RefreshLocalMods()
        {
            IEnumerable<string> files = ForgeAPI.SelectServer2Mods(materialComboBox1.SelectedItem.ToString());
            if (materialListBox1.Items.Count > 0)
                materialListBox1.Items.Clear();
            if (files != null)
                foreach (string file in files)
                    materialListBox1.Items.Add(new MaterialListBoxItem(Path.GetFileName(file)));
        }

        // Update Selected Mod
        private void materialButton1_Click(object sender, EventArgs e) =>
            ForgeAPI.UpdateSelectedMod(materialListBox1.SelectedItem.Text);

        // Remove selected Mod in local mods
        public void removeMod() =>
            materialListBox1.RemoveItem(materialListBox1.SelectedItem);
    }
}
