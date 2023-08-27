﻿using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MC_Java_Srv_GUI.Core
{
    public static class ConfigHandler
    {

        //////////////////////////////////////////////////////
        // Dictionary Guide:                                //
        //     String (Server Name) -> String (Server Path) //
        //////////////////////////////////////////////////////

        // Globals
        private static string defaultsettings = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<servers>\r\n</servers>";
        public static Dictionary<string,string> serverfolders = new Dictionary<string, string>();
        private static string Path2Config = Application.StartupPath + "\\Config.xml";
        private static XmlDocument ConfigXML = new XmlDocument();
        // End Globals

        // Bool Check for nested loop
        public static bool Spot1 = false;
        public static bool Spot2 = false;
        public static bool Spot3 = false;
        public static bool Spot4 = false;
        public static bool Spot5 = false;
        public static bool Spot6 = false;
        public static bool Spot7 = false;
        public static bool Spot8 = false;
        public static bool Spot9 = false;
        public static bool Spot10 = false;
        // End Bool Check

        // Set ServerItems for manager
        public static ServerItem Server1 = null;
        public static ServerItem Server2 = null;
        public static ServerItem Server3 = null;
        public static ServerItem Server4 = null;
        public static ServerItem Server5 = null;
        public static ServerItem Server6 = null;
        public static ServerItem Server7 = null;
        public static ServerItem Server8 = null;
        public static ServerItem Server9 = null;
        public static ServerItem Server10 = null;
        // End ServerItems

        // Delete Selected Server
        public static void DeleteServer()
        {
            string selected = Form1.CurrentForm.materialComboBox1.SelectedItem.ToString();
            string path2del = serverfolders[selected];
            serverfolders.Remove(selected);
            Directory.Delete(path2del, true);
            SaveConfig(selected);
            Form1.CurrentForm.ResetServers();
            Form1.CurrentForm.ResetLocalServers();
        }

        // Save new Config.ini
        private static void SaveConfig(string removed)
        {
            // Read Config.ini
            ConfigXML.Load(Path2Config);
            // Edit Config
            var child = ConfigXML.DocumentElement.SelectSingleNode($"server[@name='{removed}']");
            child.ParentNode.RemoveChild(child);
            // Save 2 Config.ini
            ConfigXML.Save(Path2Config);
        }

        // Read Config.ini
        public static void ReadConfig()
        {
            // Create if missing
            if (!File.Exists(Path2Config))
                CreateConfig();
            // Read Config
            ConfigXML.Load(Path2Config);
            var servers = ConfigXML.DocumentElement.SelectNodes("server");
            foreach (XmlNode node in servers)
                serverfolders.Add(node.Attributes["name"].Value, node.InnerText);
        }

        // Write Config.ini
        public static void WriteConfig(string servername, string serverpath) 
        {
            // Add server to config
            XmlElement newChild = ConfigXML.CreateElement("server");
            newChild.InnerText = serverpath;
            newChild.SetAttribute("name", servername);
            ConfigXML.DocumentElement.AppendChild(newChild);
            ConfigXML.Save(Path2Config);
            // Reload config
            if (serverfolders.Count > 0)
                serverfolders.Clear();
            ReadConfig();
        }

        private static void CreateConfig() =>
            File.WriteAllText(Path2Config, defaultsettings);
    }
}
