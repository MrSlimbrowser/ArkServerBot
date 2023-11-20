using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace ArkServerBot
{
    class Program
    {
        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandService _commandservice = new CommandService();
        private CommandHandler _commandHandler;
#if DEBUG
        public const bool isTestAssembly = true;
#else
        public const bool isTestAssembly = false;
#endif

        public static void Main(string[] args)
    => new Program().MainAsync().GetAwaiter().GetResult();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public async Task MainAsync()
        {
            _config = new DiscordSocketConfig();
            _config.GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged;

            _client = new DiscordSocketClient(_config);
            _commandHandler = new CommandHandler(_client, _commandservice);

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            await _commandHandler.InstallCommandsAsync();

            // Remember to keep token private or to read it from an 
            // external source! In this case, we are reading the token 
            // from an environment variable. If you do not know how to set-up
            // environment variables, you may find more information on the 
            // Internet or by using other methods such as reading from 
            // a configuration.
#if DEBUG
            await _client.LoginAsync(TokenType.Bot, File.ReadAllText(".secrets\\discord_bot_token"));
#else
            await _client.LoginAsync(TokenType.Bot, File.ReadAllText(".secrets/discord_bot_token"));
#endif
            await _client.StartAsync();

            Group.PopulateGroupList();
            Server.PopulateServerList();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot)
                return;
            await Log(new LogMessage(LogSeverity.Info, "", "Message Received. User: " + message.Author + "   Message: " + message.Content));
        }
    }

    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Don't process the command if it's not on ark-control channel or in a private message
            // and only process commands by MrSlimbrowser if isTestAssembly is true
            if (Program.isTestAssembly && message.Author.Id != 343156949958787075)
            {
                await message.Channel.SendMessageAsync("Sorry, I am currently in testmode because Slim is " +
                    "hopelessly trying to improve me... but I'll be there for you again once he is done :)");
                return;
            }
            else if (message.Channel.GetType().ToString() != "Discord.WebSocket.SocketDMChannel"
                && message.Channel.Name != "ark-control")
            {
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);

            // Optionally, we may inform the user if the command fails
            // to be executed; however, this may not always be desired,
            // as it may clog up the request queue should a user spam a
            // command.
            // if (!result.IsSuccess)
            // await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }

    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private bool isServerCommandRunning = false;

        //// ~say hello world -> hello world
        //[Command("say")]
        //[Summary("Echoes a message.")]
        //public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        //    => ReplyAsync(echo);

        // ReplyAsync is a method on ModuleBase 

        // ping! pong!
        [Command("ping")]
        [Summary("Responds Pong!.")]
        public Task PingAsync()
            => ReplyAsync("Pong!");

        [Command("help")]
        [Summary("Returns information about supported commands")]
        public Task Help()
            => ReplyAsync(
                "Following commands are available:\r\n" +
                "  !help\r\n" +
                "         Shows this help\r\n" +
                "  !kick\r\n" +
                "         Kicks your ark character from any server of the cluster\r\n" +
                "  !start mapname\r\n" +
                "         Starts the specified server\r\n" +
                "  !stop mapname\r\n" +
                "         Stops the specified server\r\n" +
                "  !id\r\n" +
                "         Shows your personal unique discord user ID\r\n" +
                "  !listplayers mapname\r\n" +
                "         Lists all players connected to the specified server\r\n" +
                "  !status\r\n" +
                "         Lists currently enabled/disabled servers" +
                "\r\n\r\nCommands will work on the ark-control channel or on a private chat with me.\r\n" +
                "Some commands require a mapname. Mapnames don't need to be written-out as long as the partial mapname is unmistakable.\r\n" + 
                "");

        [Command("ShowID")]
        [Summary("Shows discord ID of the mentioned user")]
        [Alias("ID")]
        public Task ShowID(SocketUser socketUser = null)
        {
            var userArg = socketUser ?? Context.Message.Author;
            return ReplyAsync("User: " + userArg.Username + " | ID: " + userArg.Id.ToString());
        }

        [Command("kick")]
        [Summary("Kicks player from all ark servers")]
        public Task KickArkPlayer(SocketUser socketUser = null)
        {
            // userArg is either the sender or a user specified by an argument
            // userArgInt is that same user but as an object of the ArkServerBot-Class "User"
            // sender is the message author just to shorten things from this point on
            // senderInt is that same user but as an object of the ArkServerBot-Class "User"
            var userArg = socketUser ?? Context.Message.Author;
            var userArgInt = User.users.Find(x => x.Equals(userArg.Id)) ?? null;
            var sender = Context.Message.Author;
            var senderInt = User.users.Find(x => x.Equals(sender.Id)) ?? null;

            if (senderInt == null)
            {
                Console.WriteLine("Kick player failed because user " + sender.Username + "#" + sender.Discriminator + " has not been unlocked for commands yet.");
                return ReplyAsync("<@" + sender.Id + "> This discord user has not been unlocked for commands yet.");
            }

            if (!userArg.Equals(sender) && (senderInt.Group.CanKickOtherPlayers != true))
            {
                Console.WriteLine("Refused kick command from user " + sender.Username + "#" + sender.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> You have no permission to kick other players.");
            }

            if (userArgInt == null || userArgInt.SteamID.Length == 0)
            {
                Console.WriteLine("Kick player failed because there is no ark player for " + userArg.Username + "#" + userArg.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> That user does not have an ark character.");
            }

            int exitCode;
#if DEBUG
            exitCode = 0;
#else
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("arkmanager", "rconcmd \"kickplayer " + userArgInt.SteamID + "\" @all");
            proc.Start();
            proc.WaitForExit();
            exitCode = proc.ExitCode;
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
        }

        [Command("start")]
        [Summary("enables/starts selected server")]
        public Task EnableServer(String servername = null)
        {
            // sender is the message author just to shorten things from this point on
            // senderInt is that same user but as an object of the ArkServerBot-Class "User"
            var sender = Context.Message.Author;
            var senderInt = User.users.Find(x => x.Equals(sender.Id)) ?? null;

            if (senderInt == null)
            {
                Console.WriteLine("Start server failed because user " + sender.Username + "#" + sender.Discriminator + " has not been unlocked for commands yet.");
                return ReplyAsync("<@" + sender.Id + "> This discord user has not been unlocked for commands yet.");
            }

            if (Context.Message.Channel.Name != "ark-control")
            {
                Console.WriteLine("Blocked start command by user " + sender.Username + "#" + sender.Discriminator + " because message was a DM.");
                return ReplyAsync("<@" + sender.Id + "> This command can only be used in the ark-control channel.");
            }

            if (!senderInt.Group.CanVoteStartServer)
            {
                Console.WriteLine("Start server failed because user " + sender.Username + "#" + sender.Discriminator + " has no permission to start servers.");
                return ReplyAsync("<@" + sender.Id + "> You have no permission to start servers.");
            }

            Server server = Server.FindServer(servername);
            if (server == null)
            {
                Console.WriteLine("Start server failed because user " + sender.Username + "#" + sender.Discriminator + " has not given clear servername");
                return ReplyAsync("<@" + sender.Id + "> A server with this name does not exist or given name is mistakable.");
            }

            if (isServerCommandRunning)
            {
                Console.WriteLine("Blocked command by user " + sender.Username + "#" + sender.Discriminator + " because another server command is running already.");
                return ReplyAsync("<@" + sender.Id + "> Another request to start/stop a server is currently being processed. Please wait for it to finish and try again.");
            }

            isServerCommandRunning = true;
            int exitcode = server.EnableServer();
            isServerCommandRunning = false;

            if (exitcode == 0)
            {
                Console.WriteLine("Server " + server.CustomName + " has been started by " + sender.Username + "#" + sender.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> Server " + server.CustomName + " is starting and should be up in a few minutes.");
            }
            else if (exitcode == 2)
            {
                Console.WriteLine(sender.Username + "#" + sender.Discriminator + " requested to start a server, but the server limit has been reached.");
                return ReplyAsync("<@" + sender.Id + "> The maximum amount of servers allowed to run has been reached. Please stop any other server first before trying to start another one.");
            }
            else if (exitcode == 4)
            {
                Console.WriteLine("Error while starting server (instance config file couldn't be moved) " + server.CustomName + " , Requester: " + sender.Username + "#" + sender.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> Server " + server.CustomName + " couldn't be started, it's configuration file is missing or broken. Notifying <@343156949958787075>.");
            }
            else
            {
                Console.WriteLine("Error while starting server " + server.CustomName + " , Requester: " + sender.Username + "#" + sender.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> Server " + server.CustomName + " failed to start :(");
            }
        }

        [Command("stop")]
        [Summary("disables/stops selected server")]
        public Task DisableServer(String servername = null)
        {
            // sender is the message author just to shorten things from this point on
            // senderInt is that same user but as an object of the ArkServerBot-Class "User"
            var sender = Context.Message.Author;
            var senderInt = User.users.Find(x => x.Equals(sender.Id)) ?? null;

            if (senderInt == null)
            {
                Console.WriteLine("Stop server failed because user " + sender.Username + "#" + sender.Discriminator + " has not been unlocked for commands yet.");
                return ReplyAsync("<@" + sender.Id + "> This discord user has not been unlocked for commands yet.");
            }

            if (Context.Message.Channel.Name != "ark-control")
            {
                Console.WriteLine("Blocked stop command by user " + sender.Username + "#" + sender.Discriminator + " because message was a DM.");
                return ReplyAsync("<@" + sender.Id + "> This command can only be used in the ark-control channel.");
            }

            if (!senderInt.Group.CanVoteStopServer)
            {
                Console.WriteLine("Stop server failed because user " + sender.Username + "#" + sender.Discriminator + " has no permission to start servers.");
                return ReplyAsync("<@" + sender.Id + "> You have no permission to stop servers.");
            }

            Server server = Server.FindServer(servername);
            if (server == null)
            {
                Console.WriteLine("Stop server failed because user " + sender.Username + "#" + sender.Discriminator + " has not given clear servername");
                return ReplyAsync("<@" + sender.Id + "> A server with this name does not exist.");
            }

            if (isServerCommandRunning)
            {
                Console.WriteLine("Blocked command by user " + sender.Username + "#" + sender.Discriminator + " because another server command is running already.");
                return ReplyAsync("<@" + sender.Id + "> Another request to start/stop a server is currently being processed. Please wait for it to finish and try again.");
            }

            isServerCommandRunning = true;
            int exitcode = server.DisableServer();
            isServerCommandRunning = false;

            if (exitcode == 0)
            {
                Console.WriteLine("Server " + server.CustomName + " has been stopped by " + sender.Username + "#" + sender.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> Server " + server.CustomName + " has been stopped.");
            }
            else if (exitcode == 2)
            {
                Console.WriteLine("Blocked command by user " + sender.Username + "#" + sender.Discriminator + " because server is not empty");
                return ReplyAsync("<@" + sender.Id + "> Server " + server.CustomName + " is not empty. Ask all players to leave and try again.");
            }
            else
            {
                Console.WriteLine("Error while stopping server " + server.CustomName + " , Requester: " + sender.Username + "#" + sender.Discriminator);
                return ReplyAsync("<@" + sender.Id + "> An error occured while stopping the server " + server.CustomName + " :(");
            }
        }

        [Command("listplayers")]
        [Summary("lists players on server")]
        public Task ListPlayers(String servername = null)
        {
            var sender = Context.Message.Author;

            if (servername == null)
            {
                Console.WriteLine("Listing players failed because user " + sender.Username + "#" + sender.Discriminator + " has not given a servername");
                return ReplyAsync("<@" + sender.Id + "> You must specify a servername.");
            }

            Server server = Server.FindServer(servername);
            if (server == null)
            {
                Console.WriteLine("Listing players failed because user " + sender.Username + "#" + sender.Discriminator + " has not given clear servername");
                return ReplyAsync("<@" + sender.Id + "> A server with this name does not exist.");
            }
            else
            {
                server.GetPlayers();
                string output = String.Empty;
                string playerName = String.Empty;

                foreach (string playerID in server.PlayerIDs)
                {
                    // find player in List
                    foreach (User user in User.users)
                    {
                        if (user.SteamID.Equals(playerID))
                        {
                            playerName = user.CustomName;
                            break;
                        }
                        else
                        {
                            playerName = "unknown player";
                        }
                    }

                    if (output == String.Empty)
                    {
                        output = playerID + " (" + playerName + ")";
                    }
                    else
                    {
                        output = ", " + playerID + " (" + playerName + ")";
                    }
                }
                if (output == String.Empty)
                    return ReplyAsync("<@" + sender.Id + "> There are no players connected to this server.");
                else
                    return ReplyAsync("<@" + sender.Id + "> Connected players: " + output);
            }
        }

        [Command("status")]
        [Summary("lists enabled and abailable servers")]
        public Task ListServers()
        {
            var sender = Context.Message.Author;
            string enabledServers = String.Empty;
            string disabledServers = String.Empty;
            
            foreach (Server s in Server.servers)
            {
                if (s.IsEnabled)
                {
                    if (enabledServers == String.Empty)
                        enabledServers = s.CustomName;
                    else
                        enabledServers = enabledServers + ", " + s.CustomName;
                }
                else
                {
                    if (disabledServers == String.Empty)
                        disabledServers = s.CustomName;
                    else
                        disabledServers = disabledServers + ", " + s.CustomName;
                }
            }

            return ReplyAsync(
                "<@" + sender.Id + "> Currently, " + Server.maxServersRunning.ToString() + " servers are allowed to run at the same time.\r\n" +
                "Enabled servers: " + enabledServers + " \r\n" +
                "Disabled servers: " + disabledServers
                );
        }
    }
}
