using MaterialSkin;
using MaterialSkin.Controls;
using MC_Java_Srv_GUI.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MC_Java_Srv_GUI.View
{
    public partial class Console : MaterialForm
    {
        // Globals
        public readonly MaterialSkinManager materialSkinManager;
        public static Console CurrentForm;
        ServerItem parentServer = null;
        // End Globals

        public Console(string ConsoleID, ServerItem server)
        {
            InitializeComponent();
            // Set static form
            CurrentForm = this;
            // Set material colors
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Set ID
            Text = "Console | " + ConsoleID;
            // Set Parent Console
            parentServer = server;
        }

        private void Console_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel= true;
            this.Hide();
        }

        // Send Command
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                parentServer.SendCommand(textBox1.Text);
                consoleBox.Text += $"{Environment.NewLine} {textBox1.Text}";
                textBox1.Text = "";
            }
            e.Handled = true;
        }

        private void consoleBox_TextChanged(object sender, EventArgs e)
        {
            consoleBox.SelectionStart = consoleBox.Text.Length;
            consoleBox.ScrollToCaret();
        }
    }
}
