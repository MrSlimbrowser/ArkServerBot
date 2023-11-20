using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ArkServerBot
{
    class Server
    {
        public static List<Server> servers = new List<Server>();
        // always set when server is being started/stopped/... and check it first to avoid two running commands that would interfere
        private static string configPath = "/etc/arkmanager/instances/";
        public static int maxServersRunning = 3;

        public string CustomName { get; }
        public string ConfigName { get; } //without file extension
        public List<String> PlayerIDs = new List<string>();
        public bool IsEnabled
        {
            get
            {
                    if (File.Exists(configPath + this.ConfigName + ".cfg"))
                        return true;
                    else
                        return false;
            }
        }

        public Server(string customName, string configName)
        {
            CustomName = customName;
            ConfigName = configName;
        }

        public static void PopulateServerList()
        {
            /*
            servers.Add(new Server("Abberation", "aberration"));
            servers.Add(new Server("CrystalIsles", "crystalisles"));
            servers.Add(new Server("Extinction", "extinction"));
            servers.Add(new Server("Genesis", "genesis"));
            servers.Add(new Server("ScorchedEarth", "scorchedearth"));
            servers.Add(new Server("TheIsland", "theisland"));
            servers.Add(new Server("Valguero", "valguero"));
            */
            servers.Add(new Server("Fjordur", "fjordur"));
        }

        public static Server FindServer(string searchstring)
        {
            if (searchstring == null)
                return null;

            searchstring = searchstring.ToLower();
            Server found = null;
            int findings = 0;
            foreach (Server server in servers)
            {
                if (server.CustomName.Contains(searchstring) || server.ConfigName.Contains(searchstring))
                {
                    found = server;
                    findings++;
                }
            }
            if (findings == 1)
                return found;
            else
                return null;
        }

        public bool GetPlayers()
        {
#if DEBUG
            string output = "x 12345678911234567 0123456789112345678";
#else
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("arkmanager", "rconcmd \"listplayers\" @" + this.ConfigName);
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                return false;
            string output = proc.StandardOutput.ReadToEnd();
#endif

            // walk trough output char by char and search for numeral strings of length 17 (steamID-Length)
            this.PlayerIDs.Clear();
            string findings = String.Empty;
            for (int i = 0; i < output.Length; i++)
            {
                int tryParseDummy;
                long foundID;

                if (findings.Length == 17)
                {
                    if (Int64.TryParse(findings, out foundID))
                    {
                        this.PlayerIDs.Add(findings);
                    }
                    else
                        findings = String.Empty;
                }

                if (int.TryParse(output[i].ToString(), out tryParseDummy))
                {
                    findings = findings + output[i];
                }
                else
                {
                    findings = String.Empty;
                }
            }
            return true;
        }

        private void SaveWorld()
        {
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("arkmanager", "saveworld @" + this.ConfigName);
#if DEBUG

#else
            proc.Start();
            proc.WaitForExit();
#endif
        }

        private int StopServer()
        {
            /*
             * STATUS-CODES:
             * 0    success
             * 1    general error
             * 2    server not empty
             */

            // check if server is empty
            this.GetPlayers();
            if (this.PlayerIDs.Count > 0)
            {
                return 2;
            }

            // Stop server
            this.SaveWorld();
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("arkmanager", "stop @" + this.ConfigName);
            int exitCode;
#if DEBUG
            exitCode = 0;
#else
            proc.Start();
            proc.WaitForExit();
            exitCode = proc.ExitCode;
#endif
            if (exitCode == 0)
                return 0;
            else
                return 1;
        }

        private int StartServer()
        {
            /*
             * STATUS-CODES:
             * 0    success
             * 1    general error
            */

            // Start server
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("sudo", "systemctl start arkmanager@" + this.ConfigName);
            int exitCode;
#if DEBUG
            exitCode = 0;
#else
            proc.Start();
            proc.WaitForExit();
            exitCode = proc.ExitCode;
#endif
            if (exitCode == 0)
                return 0;
            else
                return 1;
        }

        public int DisableServer()
        {
            /*
             * STATUS-CODES:
             * 0    success
             * 1    general error
             * 2    Server not empty
             * 4    Server configurationfile missing or broken
            */

            if (!IsEnabled)
                return 0;

            int tempResult = StopServer();
            if (tempResult != 0)
                return tempResult;

            try
            {
                File.Move(configPath + this.ConfigName + ".cfg", configPath + this.ConfigName + ".cfg.disabled");
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public int EnableServer()
        {
            /*
             * STATUS-CODES:
             * 0    success
             * 1    general error (from StartServer-Method)
             * 2    more than n servers are up already
            */

            if (IsEnabled)
                return StartServer();

            // check if max server limit has been reached
            int tempServersEnabled = 0;
            foreach (Server server in servers)
            {
                if (server.IsEnabled)
                    tempServersEnabled++;
            }
            if (tempServersEnabled >= maxServersRunning)
                return 2;

            try
            {
                File.Move(configPath + this.ConfigName + ".cfg.disabled", configPath + this.ConfigName + ".cfg");
                return StartServer();
            }
            catch
            {
                return 4;
            }
        }
    }
}
