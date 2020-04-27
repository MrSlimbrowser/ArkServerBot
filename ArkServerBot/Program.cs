using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Diagnostics;

namespace ArkServerBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commandservice = new CommandService();
        private CommandHandler _commandHandler;
        public const bool isTestAssembly = true;

    //public static string GetPlayerSteamID(SocketUser user)
    //{
    //    switch (user.Id)
    //        {
    //            case 343156949958787075: // MrSlimbrowser
    //                return "76561198039729283";
    //            case 469318430240014338: // billy
    //                return "76561198376251838";
    //            case 171370995863650305: // Brownbear
    //                return "76561198091347562";
    //            case 371780776246771713: // Cyanide
    //                return "76561197974929457";
    //            case 328561358608269323: // Rollo
    //                return "76561198089259919";
    //            case 144581855146934273: // rollyrolls
    //                return "76561198168636810";
    //            case 136285384073019392: // Solar
    //                return "76561198006677979";
    //            case 468961595477983242: // Cody
    //                return "76561198126255307";
    //            case 305866447865905152: // Toy
    //                return "76561198104394879";
    //            case 131266736908402688: // Queen
    //                return "76561198027404004";
    //            case 228754341450874884: // XionX
    //                return "76561197980675897";
    //            case 279109755757395968: // clay
    //                return "76561198128127255";
    //            default:
    //                return "0";
    //        }
    //}

    public static void Main(string[] args)
    => new Program().MainAsync().GetAwaiter().GetResult();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

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
            await _client.LoginAsync(TokenType.Bot, "NjY2NzY3MzE1NTkzMDY4NTY2.Xh5CVQ.3CIWCR5oqlvhePamhwdswEK5bkY");
            await _client.StartAsync();

            Group.PopulateGroupList();
            User.PopulateUserList(_client);

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
                    "despairingly trying to improve me... but I'll be there for you again once he is done :)");
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
            => ReplyAsync("Hi <@" + Context.Message.Author.Id + ">, I'm here to assist you :)\r\n" +
                "You can message me a command to make me do something. Notice, that all commands start with an exclamation mark.\r\n" +
                "Commands will work on the ark-control channel or on a private chat with me.\r\n" +
                "Following commands are available:\r\n" +
                "  !help\r\n" +
                "         Shows this help\r\n" +
                "  !kick\r\n" +
                "         Kicks your ark character from any server of the cluster\r\n" +
                "  !id\r\n" +
                "         Shows your personal unique discord user ID" +
                "");

        [Command("ShowID")]
        [Summary("Shows discord ID of the mentioned user")]
        [Alias("ID")]
        public Task ShowID(SocketUser user = null)
        {
            var userInfo = user ?? Context.Message.Author;
            return ReplyAsync("User: " + userInfo.Username + " | ID: " + userInfo.Id.ToString());
        }

        [Command("kick")]
        [Summary("Kicks player from all ark servers")]
        public Task KickArkPlayer(SocketUser socketUser = null)
        {
            var userArg = socketUser ?? Context.Message.Author;
            var user = User.users.Find(x => x.Equals(userArg.Id)) ?? null;

            if (Context.Message.Author.Equals(userArg))


            if (user == null)
            {
                Console.WriteLine("Kick player failed because there is no ark player for " + userArg.Username + "#" + userArg.Discriminator);
                return ReplyAsync("<@" + Context.Message.Author.Id + "> This discord user does not have an ark character or has not been unlocked yet.");
            }




            //if (user != null && ((Context.Message.Author.Id != 343156949958787075) && !Context.Message.Author.Equals(user)))
            //{
            //    Console.WriteLine("Refused kick command from user " + Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator);
            //    return ReplyAsync("<@" + Context.Message.Author.Id + "> You have no permission to kick other players.");
            //}

            //string arkPlayerSteamID = Program.GetPlayerSteamID(userInfo);
            //if (arkPlayerSteamID == "0")
            //{
            //    Console.WriteLine("Kick player failed because there is no ark player for " + userInfo.Username + "#" + userInfo.Discriminator);
            //    return ReplyAsync("<@" + Context.Message.Author.Id + "> This discord user does not have an ark character or has not been unlocked yet.");
            //}

            //Process proc = new Process();
            //ProcessStartInfo procStartInfo = new ProcessStartInfo("arkmanager", "rconcmd \"kickplayer " + arkPlayerSteamID + "\" @all");
            //proc.StartInfo = procStartInfo;
            //proc.Start();
            //proc.WaitForExit();

            //if (proc.ExitCode == 0)
            //{
            //    Console.WriteLine("Kicked player " + userInfo.Username + "#" + userInfo.Discriminator + " from ark servers");
            //    return ReplyAsync("<@" + userInfo.Id + "> Your character has been kicked from all ark servers of the cluster.");
            //}
            //else
            //{
            //    Console.WriteLine("Error while trying to kick player " + userInfo.Username + "#" + userInfo.Discriminator);
            //    return ReplyAsync("<@" + userInfo.Id + "> An unknown error occured... :(");
            //}
        }
    }
}
