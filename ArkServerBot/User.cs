using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace ArkServerBot
{
    public class User(string customName, UInt64 socketUserID, Group group, string steamID)
    {
        readonly private static Group groupAdmin = Group.groups.Find(x => x.Equals("Admin"));
        readonly private static Group groupUser = Group.groups.Find(x => x.Equals("User"));

        readonly public static List<User> users = [ 
            new User("MrSlimbrowser", 343156949958787075, groupAdmin , "76561198039729283"),
            //new User("Billy", 469318430240014338, groupUser, "76561198376251838"),
            new User("Brownbear", 171370995863650305, groupUser, "76561198091347562"),
            //new User("Cyanide", 371780776246771713, groupUser, "76561197974929457"),
            //new User("Pischer", 328561358608269323, groupUser, "76561198089259919"),
            //new User("RollyRolls", 144581855146934273, groupUser, "76561198168636810"),
            //new User("Solar", 136285384073019392, groupUser, "76561198006677979"),
            //new User("Cody", 468961595477983242, groupUser, "76561198126255307"),
            //new User("ToyMaker", 305866447865905152, groupUser, "76561198104394879"),
            new User("uniqueQueen", 131266736908402688, groupUser, "76561198027404004"),
            //new User("XionX", 228754341450874884, groupUser, "76561197980675897"),
            //new User("Clay", 279109755757395968, groupUser, "76561198128127255"),
            //new User("BarleyLightning", 123453702546653185, groupUser, "76561198020174519"),
            //new User("Uncanny", 126476526484062210, groupUser, "")
        ];

        public string CustomName { get; } = customName;
        public UInt64 DiscordUser { get; } = socketUserID;
        public Group Group { get; } = group;
        public string SteamID { get; } = steamID;

        public bool Equals(UInt64 socketUserID)
        {
            if (DiscordUser == socketUserID)
                return true;
            else
                return false;
        }
    }

    public class Group(string groupName, bool canVoteStartServer = false, bool canVoteStopServer = false
            , bool canVoteRestartServer = false, bool canVoteDelayUpdate = false, bool canKickOtherPlayers = false)
    {
        readonly public static List<Group> groups = [
            new Group("User",true,true,false,false,false),
            new Group("Admin", true, true, true, true, true)
            ];

        public string GroupName { get; } = groupName;
        public bool CanVoteStartServer { get; } = canVoteStartServer;
        public bool CanVoteStopServer { get; } = canVoteStopServer;
        public bool CanVoteRestartServer { get; } = canVoteRestartServer;
        public bool CanVoteDelayUpdate { get; } = canVoteDelayUpdate;
        public bool CanKickOtherPlayers { get; } = canKickOtherPlayers;

        public bool Equals(string groupName)
        {
            if (groupName == null) return false;
            return (this.GroupName.Equals(groupName));
        }       
    }
}
