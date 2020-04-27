using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace ArkServerBot
{
    public class User
    {
        public static List<User> users = new List<User>();

        public string CustomName { get; }
        public UInt64 DiscordUser { get; }
        public Group Group { get; }
        public string SteamID { get; }

        public User(string customName, UInt64 socketUserID, Group group, string steamID)
        {
            CustomName = customName;
            DiscordUser = socketUserID;
            Group = group;
            SteamID = steamID;
        }

        public bool Equals(UInt64 socketUserID)
        {
            if (DiscordUser == socketUserID)
                return true;
            else
                return false;
        }

        public static void PopulateUserList(DiscordSocketClient discordSocketClient)
        {
            users.Add(new User("MrSlimbrowser", 343156949958787075, Group.groups.Find(x => x.Equals("Admin")) , "76561198039729283"));
            users.Add(new User("Billy", 469318430240014338, Group.groups.Find(x => x.Equals("User")), "76561198376251838"));
            users.Add(new User("Brownbear", 171370995863650305, Group.groups.Find(x => x.Equals("Admin")), "76561198091347562"));
            users.Add(new User("Cyanide", 371780776246771713, Group.groups.Find(x => x.Equals("User")), "76561197974929457"));
            users.Add(new User("Rollo", 328561358608269323, Group.groups.Find(x => x.Equals("User")), "76561198089259919"));
            users.Add(new User("RollyRolls", 144581855146934273, Group.groups.Find(x => x.Equals("User")), "76561198168636810"));
            users.Add(new User("Solar", 136285384073019392, Group.groups.Find(x => x.Equals("User")), "76561198006677979"));
            users.Add(new User("Cody", 468961595477983242, Group.groups.Find(x => x.Equals("User")), "76561198126255307"));
            users.Add(new User("ToyMaker", 305866447865905152, Group.groups.Find(x => x.Equals("User")), "76561198104394879"));
            users.Add(new User("Queen", 131266736908402688, Group.groups.Find(x => x.Equals("User")), "76561198027404004"));
            users.Add(new User("XionX", 228754341450874884, Group.groups.Find(x => x.Equals("User")), "76561197980675897"));
            users.Add(new User("Clay", 279109755757395968, Group.groups.Find(x => x.Equals("User")), "76561198128127255"));
        }
    }

    public class Group
    {
        public static List<Group> groups = new List<Group>();

        public string GroupName { get; }
        public bool CanVoteStartServer { get; }
        public bool CanVoteStopServer { get; }
        public bool CanVoteRestartServer { get; }
        public bool CanVoteDelayUpdate { get; }
        public bool CanKickOtherPlayers { get; }

        public Group(string groupName, bool canVoteStartServer = false, bool canVoteStopServer = false
            , bool canVoteRestartServer = false, bool canVoteDelayUpdate = false, bool canKickOtherPlayers = false)
        {
            GroupName = groupName;
            CanVoteStartServer = canVoteStartServer;
            CanVoteStopServer = canVoteStopServer;
            CanVoteRestartServer = canVoteRestartServer;
            CanVoteDelayUpdate = canVoteDelayUpdate;
            CanKickOtherPlayers = canKickOtherPlayers;
        }

        public bool Equals(string groupName)
        {
            if (groupName == null) return false;
            return (this.GroupName.Equals(groupName));
        }

        public static void PopulateGroupList()
        {
            groups.Clear();
            groups.Add(new Group("User"));
            groups.Add(new Group("Admin", true, true, true, true, true));
        }
        
    }
}
