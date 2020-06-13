using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace ArkServerBot
{
    class Server
    {
        public static List<Server> servers = new List<Server>();

        public string CustomName { get; }
        public string ConfigName { get; } //without file extension

        public Server(string customName, string configName)
        {
            CustomName = customName;
            ConfigName = configName;
        }

        public static void PopulateServerList()
        {
            servers.Add(new Server("Abberation", "abberation"));
            servers.Add(new Server("Extinction", "extinction"));
            servers.Add(new Server("Genesis", "genesis"));
            servers.Add(new Server("Ragnarok", "ragnarok"));
            servers.Add(new Server("ScorchedEarth", "scorchedearth"));
            servers.Add(new Server("TheIsland", "theisland"));
            servers.Add(new Server("Valguero", "valguero"));
        }

        private int GetPlayerCount(Server server)
        {
            int playercount = 1;

            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("arkmanager", "rconcmd \"listplayers\" @" + server.ConfigName);
            int exitCode;
#if DEBUG
            exitCode = 0;
#else
                        proc.Start();
                        proc.WaitForExit();
                        exitCode = prox.ExitCode;
#endif
            if (exitCode == 0)
            {
                Console.WriteLine("Kicked player " + userArg.Username + "#" + userArg.Discriminator + " from ark servers");
                return ReplyAsync("<@" + userArg.Id + "> Your character has been kicked from all ark servers of the cluster.");
            }
            else
            {
                Console.WriteLine("Error while trying to kick player " + userArg.Username + "#" + userArg.Discriminator);
                return ReplyAsync("<@" + userArg.Id + "> An error occured... maybe one of the servers is down? :(");
            }

            return playercount;
        }




//        private int StopServer(Server server)
//        {
//            /*
//             * STATUS-CODES:
//             * 0    success
//             * 1    general error
//             * 2    server not empty
//             */

//            Process proc = new Process();
//            proc.StartInfo = new ProcessStartInfo("arkmanager", "rconcmd \"kickplayer " + userArgInt.SteamID + "\" @all");
//            int exitCode;
//#if DEBUG
//            exitCode = 0;
//#else
//            proc.Start();
//            proc.WaitForExit();
//            exitCode = prox.ExitCode;
//#endif
//            if (exitCode == 0)
//            {
//                Console.WriteLine("Kicked player " + userArg.Username + "#" + userArg.Discriminator + " from ark servers");
//                return ReplyAsync("<@" + userArg.Id + "> Your character has been kicked from all ark servers of the cluster.");
//            }
//            else
//            {
//                Console.WriteLine("Error while trying to kick player " + userArg.Username + "#" + userArg.Discriminator);
//                return ReplyAsync("<@" + userArg.Id + "> An error occured... maybe one of the servers is down? :(");
//            }

//            return 0;
//        }
    }
}
