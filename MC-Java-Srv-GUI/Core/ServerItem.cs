using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MC_Java_Srv_GUI.Core
{
    public class ServerItem
    {
        //
        // Configs Handler
        //

        // Open Config
        public void OpenConfig()
        {
            Form1.CurrentForm.materialTextBox22.Text = Path2Server + "\\" + Version + "\\" + "server.properties";
            Form1.CurrentForm.materialMultiLineTextBox21.Text = File.ReadAllText(Path2Server + "\\" + Version + "\\" + "server.properties");
            Form1.CurrentForm.materialButton7.Enabled = true;
        }

        //
        // End Configs Handler
        //

        // 
        // Console Handling
        //

        // Global Console Window
        private View.Console console;

        // Send Command via Console
        public void SendCommand(string cmd) =>
            Server.StandardInput.WriteLine(cmd);

        // Reset Console Text
        public void ResetConsole() =>
            console.consoleBox.Text = "";

        // Get Console Text
        public void ConsoleOutput()
        {
            console.Show();
            console.TopMost = true;
            console.TopMost = false;
        }

        // Crossthread Writeup
        public void WriteTextSafe(string text)
        {
            if (console.consoleBox.InvokeRequired)
            {
                Action safeWrite = delegate { WriteTextSafe(text); };
                console.consoleBox.Invoke(safeWrite);
            }
            else
                console.consoleBox.Text += text;
        }

        // Read Output to console
        private void Server_OutputDataReceived(object sender, DataReceivedEventArgs e) =>
            WriteTextSafe($"{Environment.NewLine} {e.Data}");

        // Read Error to console
        private void Server_ErrorDataReceived(object sender, DataReceivedEventArgs e) =>
            WriteTextSafe($"{Environment.NewLine} {e.Data}");

        //
        // End Console Handling
        //


        //
        // Server Handling
        //

        // Create Server Item
        private Process Server = null;

        // Set Server Item
        public ServerItem(string _Path2Server, string Name)
        {
            Path2Server = _Path2Server;
            ServerName = Path.GetDirectoryName(_Path2Server);
            Version = new DirectoryInfo(Directory.GetDirectories(_Path2Server)[0]).Name;
            Port = "Accept EULA";
            if (File.Exists(_Path2Server + "\\" + Version + "\\" + "server.properties"))
                foreach (string line in File.ReadAllLines(_Path2Server + "\\" + Version + "\\" + "server.properties"))
                    if (line.StartsWith("server-port="))
                        Port = line.Replace("server-port=", "");
            Online = false;
            // Set Console Window
            console = new View.Console(Name, this);
        }

        // Start Server
        public void StartServer(bool UseGUI = false)
        {
            // Check if started
            if (Online)
                return;
            // Auto Accept EULA
            if (Port == "Accept EULA")
                AcceptEula();
            // Start Server
            StartRoutine(UseGUI);
        }

        // Get Server Stats
        public void GetStats()
        {
            if (!Online)
                return;
            if (MineStatsHandler.Working)
                return;
            Int32 PORT = Int32.Parse(Port);
            Task.Delay(50).ContinueWith(delegate
            {
                Form1.CheckForIllegalCrossThreadCalls = false;
                List<string> stats = new List<string>();
                MineStatsHandler.Working = true;
                stats = MineStatsHandler.GetStats("localhost", PORT);
                Form1.CurrentForm.materialLabel34.Text = stats[0];
                Form1.CurrentForm.materialLabel35.Text = stats[1];
                Form1.CurrentForm.materialLabel36.Text = stats[2];
                Form1.CurrentForm.materialLabel37.Text = stats[3];
                Form1.CurrentForm.materialLabel38.Text = stats[4];
            });
            Task.Delay(50);
            while (MineStatsHandler.Working) { }
            Form1.CurrentForm.materialTabControl1.SelectedIndex = 4;
        }

        // Start Server Routine
        private void StartRoutine(bool UseGUI)
        {
            ResetConsole();
            string Path2JAR = Path2Server + "\\" + Version + "\\" + "server.jar";
            string _startInfo = $"/k java -jar \"{Path2JAR}\" {(UseGUI ? "" : "--nogui")}";
            // Forge compat
            if (File.Exists(Path2Server + "\\" + Version + "\\" + "run.bat"))
            {
                Path2JAR = Path2Server + "\\" + Version + "\\" + "run.bat";
                _startInfo = $"/k \"{Path2JAR}\"";
            }
            // Continue
            Server = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe", _startInfo)
                {
                    WorkingDirectory = Path2Server + "\\" + Version,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };
            // Set Server Events
            Server.ErrorDataReceived += Server_ErrorDataReceived;
            Server.OutputDataReceived += Server_OutputDataReceived;
            // Set Server Online & Start it
            if (Server.Start())
            {
                Server.BeginErrorReadLine();
                Server.BeginOutputReadLine();
                Online = true;
            }
        }

        // Stop Server
        public void StopServer()
        {
            // Check if closed
            if (!Online)
                return;
            // Graceful stop
            Server.StandardInput.WriteLine("stop");
            // Delayed kill
            Task.Delay(5000).ContinueWith(delegate { Server.Close(); });
            // Set Server Offline
            Online = false;
        }

        // Accept EULA auto
        private void AcceptEula()
        {
            string Path2JAR = Path2Server + "\\" + Version + "\\" + "server.jar";
            string _startinfo = $"/k java -jar \"{Path2JAR}\" --nogui";
            if (Path2JAR.Contains("Forge") && File.Exists(Path2Server + "\\" + Version + "\\" + "installer.jar"))
            {
                Path2JAR = Path2JAR.Replace("server.jar", "installer.jar");
                _startinfo = $"/k java -jar \"{Path2JAR}\" --installServer";
            }
            Server = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe", _startinfo)
                {
                    WorkingDirectory = Path2Server + "\\" + Version,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            // Set Server Events
            Server.ErrorDataReceived += Server_ErrorDataReceived;
            Server.OutputDataReceived += Server_OutputDataReceived;
            // Set Server Online & Start it
            if (Server.Start())
            {
                Server.BeginErrorReadLine();
                Server.BeginOutputReadLine();
                Server.StandardInput.WriteLine("exit");
            }
            while (!Server.HasExited) { }
            if (File.Exists(Path2Server + "\\" + Version + "\\" + "run.bat"))
            {
                Server = new Process()
                {
                    StartInfo = new ProcessStartInfo("cmd.exe", $"/k \"{Path2Server + "\\" + Version + "\\" + "run.bat"}\"")
                    {
                        WorkingDirectory = Path2Server + "\\" + Version,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };
                // Set Server Events
                Server.ErrorDataReceived += Server_ErrorDataReceived;
                Server.OutputDataReceived += Server_OutputDataReceived;
                // Set Server Online & Start it
                if (Server.Start())
                {
                    Server.BeginErrorReadLine();
                    Server.BeginOutputReadLine();
                    Server.StandardInput.WriteLine("");
                    Server.StandardInput.WriteLine("exit");
                    Server.WaitForExit(1000);
                }
            }
            string eula = File.ReadAllText(Path2Server + "\\" + Version + "\\" + "eula.txt");
            eula = eula.Replace("eula=false", "eula=true");
            File.WriteAllText(Path2Server + "\\" + Version + "\\" + "eula.txt", eula);
            // Set default PORT
            Port = "25565";
        }

        //
        // End Server Handling
        //

        // Server Name
        public string ServerName { get; private set; }

        // Server Status (Refreshable)
        public bool Online { get; private set; }

        // Server Version
        public string Version { get; private set; }

        // Server Port
        public string Port { get; private set; }

        // Path to Server
        public string Path2Server { get; private set; }
    }
}
