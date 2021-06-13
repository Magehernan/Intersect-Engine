using Intersect.Enums;
using Intersect.Localization;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Intersect.Client.Localization
{

    public static class Strings
    {

        private static readonly char[] mQuantityTrimChars = new char[] { '.', '0' };

        public static string FormatQuantityAbbreviated(long value)
        {
            if (value == 0)
            {
                return string.Empty;
            }
            else
            {
                double returnVal;
                string postfix = string.Empty;

                // hundreds
                if (value <= 999)
                {
                    returnVal = value;
                }

                // thousands
                else if (value >= 1000 && value <= 999999)
                {
                    returnVal = value / 1000.0;
                    postfix = Numbers.thousands;
                }

                // millions
                else if (value >= 1000000 && value <= 999999999)
                {
                    returnVal = value / 1000000.0;
                    postfix = Numbers.millions;
                }

                // billions
                else if (value >= 1000000000 && value <= 999999999999)
                {
                    returnVal = value / 1000000000.0;
                    postfix = Numbers.billions;
                }
                else
                {
                    return "OOB";
                }

                if (returnVal >= 10)
                {
                    returnVal = Math.Floor(returnVal);

                    return returnVal.ToString() + postfix;
                }
                else
                {
                    returnVal = Math.Floor(returnVal * 10) / 10.0;

                    return returnVal.ToString("F1")
                               .TrimEnd(mQuantityTrimChars)
                               .Replace(".", Numbers.dec) +
                           postfix;
                }
            }
        }

        public static void Load(string json)
        {
            JSONNode node = JSON.Parse(json);

            Type type = typeof(Strings);

            List<Type> fields = new List<Type>();
            fields.AddRange(type.GetNestedTypes(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));

            foreach (Type p in fields)
            {
                if (!node.HasKey(p.Name))
                {
                    continue;
                }

                JSONNode inner = node[p.Name];
                foreach (System.Reflection.FieldInfo fieldInfo in p.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                {
                    object fieldValue = fieldInfo.GetValue(null);
                    if (!inner.HasKey(fieldInfo.Name.ToLower()))
                    {
                        continue;
                    }

                    switch (fieldValue)
                    {
                        case LocalizedString value:
                        {
                            fieldInfo.SetValue(null, new LocalizedString(inner[fieldInfo.Name.ToLower()]));
                        }
                        break;
                        case Dictionary<int, LocalizedString> existingDict:
                        {
                            foreach (KeyValuePair<string, JSONNode> item in inner[fieldInfo.Name])
                            {
                                existingDict[int.Parse(item.Key)] = item.Value.Value;
                            }
                        }
                        break;
                        case Dictionary<string, LocalizedString> existingDict:
                        {
                            JSONNode inner2 = inner[fieldInfo.Name];

                            foreach (KeyValuePair<string, JSONNode> pair in inner2)
                            {
                                if (pair.Key == null)
                                {
                                    continue;
                                }

                                existingDict[pair.Key.ToLower()] = pair.Value.Value;
                            }
                        }
                        break;
                        case Dictionary<KeyCode, LocalizedString> existingDict:
                        {
                            JSONNode inner2 = inner[fieldInfo.Name];

                            foreach (KeyValuePair<string, JSONNode> pair in inner2)
                            {
                                if (pair.Key == null || Enum.TryParse(pair.Key, true, out KeyCode key))
                                {
                                    continue;
                                }

                                existingDict[key] = pair.Value.Value;
                            }
                        }
                        break;
                    }
                }
            }
            Save();
        }
        [Conditional("UNITY_EDITOR")]
        public static void Save()
        {
            Dictionary<string, Dictionary<string, object>> strings = new Dictionary<string, Dictionary<string, object>>();
            Type type = typeof(Strings);
            Type[] fields = type.GetNestedTypes(
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
            );

            foreach (Type p in fields)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (System.Reflection.FieldInfo p1 in p.GetFields(
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
                ))
                {
                    if (p1.GetValue(null).GetType() == typeof(LocalizedString))
                    {
                        dict.Add(p1.Name.ToLower(), ((LocalizedString)p1.GetValue(null)).ToString());
                    }
                    else if (p1.GetValue(null).GetType() == typeof(Dictionary<int, LocalizedString>))
                    {
                        Dictionary<int, string> dic = new Dictionary<int, string>();
                        foreach (KeyValuePair<int, LocalizedString> val in (Dictionary<int, LocalizedString>)p1.GetValue(null))
                        {
                            dic.Add(val.Key, val.Value.ToString());
                        }

                        dict.Add(p1.Name, dic);
                    }
                    else if (p1.GetValue(null).GetType() == typeof(Dictionary<string, LocalizedString>))
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, LocalizedString> val in (Dictionary<string, LocalizedString>)p1.GetValue(null))
                        {
                            dic.Add(val.Key.ToLower(), val.Value.ToString());
                        }

                        dict.Add(p1.Name, dic);
                    }
                    else if (p1.GetValue(null).GetType() == typeof(Dictionary<KeyCode, LocalizedString>))
                    {
                        Dictionary<KeyCode, string> dic = new Dictionary<KeyCode, string>();
                        foreach (KeyValuePair<KeyCode, LocalizedString> val in (Dictionary<KeyCode, LocalizedString>)p1.GetValue(null))
                        {
                            dic.Add(val.Key, val.Value.ToString());
                        }

                        dict.Add(p1.Name, dic);
                    }
                }

                strings.Add(p.Name, dict);
            }

            string languageDirectory = Path.Combine("Assets/IntersectResources");
            if (Directory.Exists(languageDirectory))
            {
                File.WriteAllText(
                    Path.Combine(languageDirectory, "client_strings.txt"),
                    JsonConvert.SerializeObject(strings, Formatting.Indented)
                );

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }

        public struct Admin
        {

            public static LocalizedString access = @"Access:";

            public static LocalizedString access0 = @"None";

            public static LocalizedString access1 = @"Moderator";

            public static LocalizedString access2 = @"Admin";

            public static LocalizedString ban = @"Ban";

            public static LocalizedString bancaption = @"Ban {00}";

            public static LocalizedString banprompt = @"Banning {00} will not allow them to access this game for the duration you set!";

            public static LocalizedString chronological = @"123...";

            public static LocalizedString chronologicaltip = @"Order maps chronologically.";

            public static LocalizedString face = @"Face:";

            public static LocalizedString kick = @"Kick";

            public static LocalizedString kill = @"Kill";

            public static LocalizedString maplist = @"Map List:";

            public static LocalizedString mute = @"Mute";

            public static LocalizedString mutecaption = @"Mute {00}";

            public static LocalizedString muteprompt = @"Muting {00} will not allow them to chat in game for the duration you set!";

            public static LocalizedString name = @"Name:";

            public static LocalizedString noclip = @"No Clip:";

            public static LocalizedString nocliptip = @"Check to walk through obstacles.";

            public static LocalizedString none = @"None";

            public static LocalizedString setface = @"Set Face";

            public static LocalizedString setpower = @"Set Power";

            public static LocalizedString setsprite = @"Set Sprite";

            public static LocalizedString sprite = @"Sprite:";

            public static LocalizedString title = @"Administration";

            public static LocalizedString unban = @"Unban";

            public static LocalizedString unbancaption = @"Unban {00}";

            public static LocalizedString unbanprompt = @"Are you sure that you want to unban {00}?";

            public static LocalizedString unmute = @"Unmute";

            public static LocalizedString unmutecaption = @"Unute {00}";

            public static LocalizedString unmuteprompt = @"Are you sure that you want to unmute {00}?";

            public static LocalizedString warp2me = @"Warp To Me";

            public static LocalizedString warpme2 = @"Warp Me To";

        }

        public struct Bags
        {

            public static LocalizedString retreiveitem = @"Retreive Item";

            public static LocalizedString retreiveitemprompt = @"How many/much {00} would you like to retreive?";

            public static LocalizedString storeitem = @"Store Item";

            public static LocalizedString storeitemprompt = @"How many/much {00} would you like to store?";

            public static LocalizedString title = @"Bag";

        }

        public struct Bank
        {

            public static LocalizedString deposititem = @"Deposit Item";

            public static LocalizedString deposititemprompt = @"How many/much {00} would you like to deposit?";

            public static LocalizedString title = @"Bank";

            public static LocalizedString withdrawitem = @"Withdraw Item";

            public static LocalizedString withdrawitemprompt = @"How many/much {00} would you like to withdraw?";

        }

        public struct BanMute
        {

            public static LocalizedString oneday = @"1 day";

            public static LocalizedString onemonth = @"1 month";

            public static LocalizedString oneweek = @"1 week";

            public static LocalizedString oneyear = @"1 year";

            public static LocalizedString twodays = @"2 days";

            public static LocalizedString twomonths = @"2 months";

            public static LocalizedString twoweeks = @"2 weeks";

            public static LocalizedString threedays = @"3 days";

            public static LocalizedString fourdays = @"4 days";

            public static LocalizedString fivedays = @"5 days";

            public static LocalizedString sixmonths = @"6 months";

            public static LocalizedString cancel = @"Cancel:";

            public static LocalizedString duration = @"Duration:";

            public static LocalizedString forever = @"Indefinitely";

            public static LocalizedString ip = @"Include IP:";

            public static LocalizedString ok = @"Okay:";

            public static LocalizedString reason = @"Reason:";

        }

        public struct Character
        {

            public static LocalizedString equipment = @"Equipment:";

            public static LocalizedString costume = @"Costume";

            public static LocalizedString levelandclass = @"Level: {00} {01}";

            public static LocalizedString name = @"{00}";

            public static LocalizedString points = @"Points: {00}";

            public static LocalizedString stat0 = @"{00}: {01}";

            public static LocalizedString stat1 = @"{00}: {01}";

            public static LocalizedString stat2 = @"{00}: {01}";

            public static LocalizedString stat3 = @"{00}: {01}";

            public static LocalizedString stat4 = @"{00}: {01}";

            public static LocalizedString stat5 = @"{00}: {01}";

            public static LocalizedString stats = @"Stats:";

            public static LocalizedString title = @"Character";

        }

        public struct CharacterCreation
        {

            public static LocalizedString back = @"Back";

            public static LocalizedString Class = @"Class:";

            public static LocalizedString create = @"Create";

            public static LocalizedString female = @"Female";

            public static LocalizedString gender = @"Gender:";

            public static LocalizedString hint = @"Customize";

            public static LocalizedString hint2 = @"Your Character";

            public static LocalizedString invalidname =
                @"Character name is invalid. Please use alphanumeric characters with a length between 2 and 20.";

            public static LocalizedString male = @"Male";

            public static LocalizedString name = @"Char Name:";

            public static LocalizedString title = @"Create Character";

        }

        public struct CharacterSelection
        {

            public static LocalizedString delete = @"Delete";

            public static LocalizedString deleteprompt =
                @"Are you sure you want to delete {00}? This action is irreversible!";

            public static LocalizedString deletetitle = @"Delete {00}";

            public static LocalizedString empty = @"Empty Character Slot";

            public static LocalizedString info = @"Level {00} {01}";

            public static LocalizedString logout = @"Logout";

            public static LocalizedString name = @"{00}";

            public static LocalizedString New = @"New";

            public static LocalizedString play = @"Use";

            public static LocalizedString title = @"Select a Character";

        }

        public struct Chatbox
        {

            public static LocalizedString channel = @"Channel:";

            public static Dictionary<int, LocalizedString> channels = new Dictionary<int, LocalizedString>
            {
                {0, @"local"},
                {1, @"global"},
                {2, @"party"},
                {3, @"guild"}
            };

            public static LocalizedString channeladmin = @"admin";

            public static LocalizedString enterchat = @"Click here to chat.";

            public static LocalizedString enterchat1 = @"Press {00} to chat.";

            public static LocalizedString enterchat2 = @"Press {00} or {01} to chat.";

            public static LocalizedString send = @"Send";

            public static LocalizedString title = @"Chat";

            public static LocalizedString toofast = @"You are chatting too fast!";

            public static Dictionary<ChatboxTab, LocalizedString> ChatTabButtons = new Dictionary<ChatboxTab, LocalizedString>() {
                { ChatboxTab.All, @"All" },
                { ChatboxTab.Local, @"Local" },
                { ChatboxTab.Party, @"Party" },
                { ChatboxTab.Global, @"Global" },
                { ChatboxTab.Guild, @"Guild" },
                { ChatboxTab.System, @"System" },
            };

            public static LocalizedString UnableToCopy = @"It appears you are not able to copy/paste on this platform. Please make sure you have either the 'xclip' or 'wl-clipboard' packages installed if you are running Linux.";
        }

        public struct Colors
        {

            public static Dictionary<int, LocalizedString> presets = new Dictionary<int, LocalizedString>
            {
                {0, @"Black"},
                {1, @"White"},
                {2, @"Pink"},
                {3, @"Blue"},
                {4, @"Red"},
                {5, @"Green"},
                {6, @"Yellow"},
                {7, @"Orange"},
                {8, @"Purple"},
                {9, @"Gray"},
                {10, @"Cyan"}
            };
        }

        public struct Combat
        {

            public static LocalizedString exp = @"Experience";

            public static LocalizedString stat0 = @"Str";

            public static LocalizedString stat1 = @"Int";

            public static LocalizedString stat2 = @"Def";

            public static LocalizedString stat3 = @"M.Def";

            public static LocalizedString stat4 = @"Mov";

            public static LocalizedString stat5 = @"Agi";

            public static LocalizedString targetoutsiderange = @"Target too far away!";

            public static LocalizedString vital0 = @"Health";

            public static LocalizedString vital1 = @"Mana";

            public static LocalizedString warningtitle = @"Combat Warning!";

            public static LocalizedString warningforceclose =
                @"Game was closed while in combat! Your character will remain logged in until combat has concluded!";

            public static LocalizedString warninglogout =
                @"You are about to logout while in combat! Your character will remain in-game until combat has ended! Are you sure you want to logout now?";

            public static LocalizedString warningcharacterselect =
                @"You are about to logout while in combat! Your character will remain in-game until combat has ended! Are you sure you want to logout now?";

            public static LocalizedString warningexitdesktop =
                @"You are about to exit while in combat! Your character will remain in-game until combat has ended! Are you sure you want to quit now?";

        }

        public struct Controls
        {

            public static Dictionary<string, LocalizedString> controldict = new Dictionary<string, LocalizedString>
            {
                {"attackinteract", @"Attack/Interact:"},
                {"block", @"Block:"},
                {"autotarget", @"Auto Target:"},
                {"enter", @"Enter:"},
                {"hotkey0", @"Hot Key 0:"},
                {"hotkey1", @"Hot Key 1:"},
                {"hotkey2", @"Hot Key 2:"},
                {"hotkey3", @"Hot Key 3:"},
                {"hotkey4", @"Hot Key 4:"},
                {"hotkey5", @"Hot Key 5:"},
                {"hotkey6", @"Hot Key 6:"},
                {"hotkey7", @"Hot Key 7:"},
                {"hotkey8", @"Hot Key 8:"},
                {"hotkey9", @"Hot Key 9:"},
                {"movedown", @"Down:"},
                {"moveleft", @"Left:"},
                {"moveright", @"Right:"},
                {"moveup", @"Up:"},
                {"pickup", @"Pick Up:"},
                {"screenshot", @"Screenshot:"},
                {"openmenu", @"Open Menu:"},
                {"openinventory", @"Open Inventory:"},
                {"openquests", @"Open Quests:"},
                {"opencharacterinfo", @"Open Character Info:"},
                {"openparties", @"Open Parties:"},
                {"openspells", @"Open Spells:"},
                {"openfriends", @"Open Friends:"},
                {"openguild", @"Open Guild:"},
                {"opensettings", @"Open Settings:"},
                {"opendebugger", @"Open Debugger:"},
                {"openadminpanel", @"Open Admin Panel:"},
                {"togglegui", @"Toggle Interface:"},
            };

            public static LocalizedString edit = @"Edit Controls";

            public static LocalizedString listening = @"Listening";

            public static LocalizedString title = @"Controls";

        }

        public struct Crafting
        {

            public static LocalizedString craft = @"Craft";

            public static LocalizedString incorrectresources =
                @"You do not have the correct resources to craft this item.";

            public static LocalizedString ingredients = @"Requires:";

            public static LocalizedString product = @"Crafting:";

            public static LocalizedString recipes = @"Recipes:";

            public static LocalizedString title = @"Crafting Table";

        }

        public struct Credits
        {

            public static LocalizedString back = @"Main Menu";

            public static LocalizedString title = @"Credits";

        }

        public struct Debug
        {

            public static LocalizedString draws = @"Draws: {00}";

            public static LocalizedString entitiesdrawn = @"Entities Drawn: {00}";

            public static LocalizedString fps = @"FPS: {00}";

            public static LocalizedString knownentities = @"Known Entities: {00}";

            public static LocalizedString knownmaps = @"Known Maps: {00}";

            public static LocalizedString lightsdrawn = @"Lights Drawn: {00}";

            public static LocalizedString map = @"Map: {00}";

            public static LocalizedString mapsdrawn = @"Maps Drawn: {00}";

            public static LocalizedString ping = @"Ping: {00}";

            public static LocalizedString time = @"Time: {00}";

            public static LocalizedString title = @"Debug";

            public static LocalizedString x = @"X: {00}";

            public static LocalizedString y = @"Y: {00}";

            public static LocalizedString z = @"Z: {00}";

            public static LocalizedString interfaceobjects = @"Interface Objects: {00}";

        }

        public struct EntityBox
        {

            public static LocalizedString NameAndLevel = @"{00}    {01}";

            public static LocalizedString cooldown = "{00}s";

            public static LocalizedString exp = @"EXP:";

            public static LocalizedString expval = @"{00} / {01}";

            public static LocalizedString friend = "Befriend";

            public static LocalizedString friendtip = "Send {00} a friend request.";

            public static LocalizedString level = @"Lv. {00}";

            public static LocalizedString map = @"{00}";

            public static LocalizedString maxlevel = @"Max Level";

            public static LocalizedString party = @"Party";

            public static LocalizedString partytip = @"Invite {00} to your party.";

            public static LocalizedString trade = @"Trade";

            public static LocalizedString tradetip = @"Request to trade with {00}.";

            public static LocalizedString vital0 = @"HP:";

            public static LocalizedString vital0val = @"{00} / {01}";

            public static LocalizedString vital1 = @"MP:";

            public static LocalizedString vital1val = @"{00} / {01}";

        }

        public struct Errors
        {

            public static LocalizedString displaynotsupported = @"Invalid Display Configuration!";

            public static LocalizedString displaynotsupportederror =
                @"Fullscreen {00} resolution is not supported on this device!";

            public static LocalizedString errorencountered =
                @"The Intersect Client has encountered an error and must close. Error information can be found in logs/errors.log";

            public static LocalizedString notconnected = @"Not connected to the game server. Is it online?";

            public static LocalizedString notsupported = @"Not Supported!";

            public static LocalizedString openallink = @"https://goo.gl/Nbx6hx";

            public static LocalizedString opengllink = @"https://goo.gl/RSP3ts";

            public static LocalizedString passwordinvalid =
                @"Password is invalid. Please use alphanumeric characters with a length between 4 and 20.";

            public static LocalizedString resourcesnotfound =
                @"The resources directory could not be found! Intersect will now close.";

            public static LocalizedString title = @"Error!";

            public static LocalizedString usernameinvalid =
                @"Username is invalid. Please use alphanumeric characters with a length between 2 and 20.";

            public static LocalizedString LoadFile =
                @"Failed to load a {00}. Please send the game administrator a copy of your errors log file in the logs directory.";

            public static LocalizedString lostconnection =
                @"Lost connection to the game server. Please make sure you're connected to the internet and try again!";

        }

        public struct Words
        {

            public static LocalizedString lcase_sound = @"sound";

            public static LocalizedString lcase_music = @"soundtrack";

            public static LocalizedString lcase_sprite = @"sprite";

            public static LocalizedString lcase_animation = @"animation";

        }

        public struct EventWindow
        {

            public static LocalizedString Continue = @"Continue";

        }

        public struct ForgotPass
        {

            public static LocalizedString back = @"Back";

            public static LocalizedString hint =
                @"If your account exists we will send you a temporary password reset code.";

            public static LocalizedString label = @"Enter your username or email below:";

            public static LocalizedString submit = @"Submit";

            public static LocalizedString title = @"Password Reset";

        }

        public struct Friends
        {

            public static LocalizedString addfriend = @"Add Friend";

            public static LocalizedString addfriendtitle = @"Add Friend";

            public static LocalizedString addfriendprompt = @"Who would you like to add as a friend?";

            public static LocalizedString infight = @"You are currently fighting!";

            public static LocalizedString removefriend = @"Remove Friend";

            public static LocalizedString removefriendprompt = @"Do you wish to remove {00} from your friends list?";

            public static LocalizedString request = @"Friend Request";

            public static LocalizedString requestprompt = @"{00} has sent you a friend request. Do you accept?";

            public static LocalizedString title = @"Friends";

        }

        public struct GameMenu
        {

            public static LocalizedString character = @"Character Info";

            public static LocalizedString Menu = @"Open Menu";

            public static LocalizedString friends = @"Friends";

            public static LocalizedString Guild = "Guild";

            public static LocalizedString items = @"Inventory";

            public static LocalizedString party = @"Party";

            public static LocalizedString quest = @"Quest Log";

            public static LocalizedString spells = @"Spell Book";

        }

        public struct General
        {

            public static LocalizedString none = @"None";

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public static LocalizedString MapItemStackable = @"{01} {00}";

        }

        public struct Guilds
        {
            public static LocalizedString Guild = @"Guild";

            public static LocalizedString guildtip = "Send {00} an invite to your guild.";

            public static LocalizedString Invite = @"Invite";

            public static LocalizedString NotInGuild = @"You are not currently in a guild!";

            public static LocalizedString InviteMemberTitle = @"Invite Player";

            public static LocalizedString InviteMemberPrompt = @"Who would you like to invite to {00}?";

            public static LocalizedString InviteRequestTitle = @"Guild Invite";

            public static LocalizedString InviteRequestPrompt = @"{00} would like to invite you to join their guild, {01}. Do you accept?";

            public static LocalizedString Leave = "Leave";

            public static LocalizedString LeaveTitle = @"Leave Guild";

            public static LocalizedString LeavePrompt = @"Are you sure you would like to leave your guild?";

            public static LocalizedString Promote = @"Promote to {00}";

            public static LocalizedString Demote = @"Demote to {00}";

            public static LocalizedString Kick = @"Kick";

            public static LocalizedString PM = @"PM";

            public static LocalizedString Transfer = @"Transfer";

            public static LocalizedString OnlineListEntry = @"[{00}] {01} - {02}";

            public static LocalizedString OfflineListEntry = @"[{00}] {01} - {02}";

            public static LocalizedString Tooltip = @"Lv. {00} {01}";

            public static LocalizedString KickTitle = @"Kick Guild Member";

            public static LocalizedString KickPrompt = @"Are you sure you would like to kick {00}?";

            public static LocalizedString PromoteTitle = @"Promote Guild Member";

            public static LocalizedString PromotePrompt = @"Are you sure you would like promote {00} to rank {01}?";

            public static LocalizedString DemoteTitle = @"Demote Guild Member";

            public static LocalizedString DemotePrompt = @"Are you sure you would like to demote {00} to rank {01}?";

            public static LocalizedString TransferTitle = @"Transfer Guild";

            public static LocalizedString TransferPrompt = @"This action will completely transfer all ownership of your guild to {00} and you will lose your rank of {01}. If you are sure you want to hand over your guild enter '{02}' below.";

            public static LocalizedString Bank = @"{00} Guild Bank";

            public static LocalizedString NotAllowedWithdraw = @"You do not have permission to withdraw from {00}'s guild bank!";

            public static LocalizedString NotAllowedDeposit = @"You do not have permission to deposit items into {00}'s guild bank!";

            public static LocalizedString NotAllowedSwap = @"You do not have permission to swap items around within {00}'s guild bank!";

            public static LocalizedString InviteAlreadyInGuild = @"The player you're trying to invite is already in a guild or has a pending invite.";
        }

        public struct InputBox
        {

            public static LocalizedString cancel = @"Cancel";

            public static LocalizedString no = @"No";

            public static LocalizedString okay = @"Okay";

            public static LocalizedString yes = @"Yes";

        }

        public struct MapItemWindow
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public static LocalizedString Title = @"Loot";

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public static LocalizedString LootButton = @"Loot All";

        }

        public struct Inventory
        {

            public static LocalizedString cooldown = "{00}s";

            public static LocalizedString dropitem = @"Drop Item";

            public static LocalizedString dropitemprompt = @"How many/much {00} do you want to drop?";

            public static LocalizedString dropprompt = @"Do you wish to drop the item: {00}?";

            public static LocalizedString equippedicon = "E";

            public static LocalizedString title = @"Inventory";

        }

        public struct ItemDesc
        {

            public static LocalizedString bonuses = @"Stat Bonuses:";

            public static LocalizedString damage = @"Base Damage: {00}";

            public static LocalizedString desc = @"{00}";

            public static LocalizedString effect = @"Bonus Effect: {00}% {01}";

            public static Dictionary<int, LocalizedString> effects = new Dictionary<int, LocalizedString>
            {
                {0, @"Cooldown Reduction"},
                {1, @"Lifesteal"},
                {2, @"Tenacity"},
                {3, @"Luck"},
                {4, @"Exp Increase"},
            };

            public static Dictionary<int, LocalizedString> itemtypes = new Dictionary<int, LocalizedString>
            {
                {0, @"None"},
                {1, @"Equipment"},
                {2, @"Consumable"},
                {3, @"Currency"},
                {4, @"Spell"},
                {5, @"Special"},
                {6, @"Bag"},
            };

            public static LocalizedString prereq = @"Prerequisites:";

            public static Dictionary<int, LocalizedString> stats = new Dictionary<int, LocalizedString>
            {
                {0, @"Str: {00}"},
                {1, @"Int: {00}"},
                {2, @"Def: {00}"},
                {3, @"M.Def: {00}"},
                {4, @"Mov: {00}"},
                {5, @"Agi: {00}"}
            };

            public static LocalizedString twohand = @"2H";

            public static Dictionary<int, LocalizedString> rarity = new Dictionary<int, LocalizedString>
            {
                {0, @"None"},
                {1, @"Common"},
                {2, @"Uncommon"},
                {3, @"Rare"},
                {4, @"Epic"},
                {5, @"Legendary"},
            };

            public static Dictionary<int, LocalizedString> vitals = new Dictionary<int, LocalizedString>
            {
                {0, @"HP: {00}"},
                {1, @"MP: {00}"}
            };

        }

        public struct Keys
        {

            public static Dictionary<KeyCode, LocalizedString> keydict = new Dictionary<KeyCode, LocalizedString>()
            {
                { KeyCode.None, "NONE" },
                { KeyCode.Backspace, "Backspace" },
                { KeyCode.Tab, "Tab" },
                { KeyCode.Clear, "Clear" },
                { KeyCode.Return, "Return" },
                { KeyCode.Pause, "Pause" },
                { KeyCode.Escape, "Escape" },
                { KeyCode.Space, "Space" },
                { KeyCode.Exclaim, "!" },
                { KeyCode.DoubleQuote, "\"" },
                { KeyCode.Hash, "#" },
                { KeyCode.Dollar, "$" },
                { KeyCode.Percent, "%" },
                { KeyCode.Ampersand, "&" },
                { KeyCode.Quote, "'" },
                { KeyCode.LeftParen, "(" },
                { KeyCode.RightParen, ")" },
                { KeyCode.Asterisk, "*" },
                { KeyCode.Plus, "+" },
                { KeyCode.Comma, "," },
                { KeyCode.Minus, "-" },
                { KeyCode.Period, "." },
                { KeyCode.Slash, "/" },
                { KeyCode.Alpha0, "0" },
                { KeyCode.Alpha1, "1" },
                { KeyCode.Alpha2, "2" },
                { KeyCode.Alpha3, "3" },
                { KeyCode.Alpha4, "4" },
                { KeyCode.Alpha5, "5" },
                { KeyCode.Alpha6, "6" },
                { KeyCode.Alpha7, "7" },
                { KeyCode.Alpha8, "8" },
                { KeyCode.Alpha9, "9" },
                { KeyCode.Colon, ":" },
                { KeyCode.Semicolon, ";" },
                { KeyCode.Less, "<" },
                { KeyCode.Equals, "=" },
                { KeyCode.Greater, ">" },
                { KeyCode.Question, "?" },
                { KeyCode.At, "@" },
                { KeyCode.LeftBracket, "[" },
                { KeyCode.Backslash, "\\" },
                { KeyCode.RightBracket, "]" },
                { KeyCode.Caret, "^" },
                { KeyCode.Underscore, "_" },
                { KeyCode.BackQuote, "`" },
                { KeyCode.A, "A" },
                { KeyCode.B, "B" },
                { KeyCode.C, "C" },
                { KeyCode.D, "D" },
                { KeyCode.E, "E" },
                { KeyCode.F, "F" },
                { KeyCode.G, "G" },
                { KeyCode.H, "H" },
                { KeyCode.I, "I" },
                { KeyCode.J, "J" },
                { KeyCode.K, "K" },
                { KeyCode.L, "L" },
                { KeyCode.M, "M" },
                { KeyCode.N, "N" },
                { KeyCode.O, "O" },
                { KeyCode.P, "P" },
                { KeyCode.Q, "Q" },
                { KeyCode.R, "R" },
                { KeyCode.S, "S" },
                { KeyCode.T, "T" },
                { KeyCode.U, "U" },
                { KeyCode.V, "V" },
                { KeyCode.W, "W" },
                { KeyCode.X, "X" },
                { KeyCode.Y, "Y" },
                { KeyCode.Z, "Z" },
                { KeyCode.LeftCurlyBracket, "{" },
                { KeyCode.Pipe, "|" },
                { KeyCode.RightCurlyBracket, "}" },
                { KeyCode.Tilde, "~" },
                { KeyCode.Delete, "Delete" },
                { KeyCode.Keypad0, "Num 0" },
                { KeyCode.Keypad1, "Num 1" },
                { KeyCode.Keypad2, "Num 2" },
                { KeyCode.Keypad3, "Num 3" },
                { KeyCode.Keypad4, "Num 4" },
                { KeyCode.Keypad5, "Num 5" },
                { KeyCode.Keypad6, "Num 6" },
                { KeyCode.Keypad7, "Num 7" },
                { KeyCode.Keypad8, "Num 8" },
                { KeyCode.Keypad9, "Num 9" },
                { KeyCode.KeypadPeriod, "Num ." },
                { KeyCode.KeypadDivide, "Num /" },
                { KeyCode.KeypadMultiply, "Num *" },
                { KeyCode.KeypadMinus, "Num -" },
                { KeyCode.KeypadPlus, "Num +" },
                { KeyCode.KeypadEnter, "Num Enter" },
                { KeyCode.KeypadEquals, "Num =" },
                { KeyCode.UpArrow, "Up" },
                { KeyCode.DownArrow, "Down" },
                { KeyCode.RightArrow, "Right" },
                { KeyCode.LeftArrow, "Left" },
                { KeyCode.Insert, "Insert" },
                { KeyCode.Home, "Home" },
                { KeyCode.End, "End" },
                { KeyCode.PageUp, "PageUp" },
                { KeyCode.PageDown, "PageDown" },
                { KeyCode.F1, "F1" },
                { KeyCode.F2, "F2" },
                { KeyCode.F3, "F3" },
                { KeyCode.F4, "F4" },
                { KeyCode.F5, "F5" },
                { KeyCode.F6, "F6" },
                { KeyCode.F7, "F7" },
                { KeyCode.F8, "F8" },
                { KeyCode.F9, "F9" },
                { KeyCode.F10, "F10" },
                { KeyCode.F11, "F11" },
                { KeyCode.F12, "F12" },
                { KeyCode.F13, "F13" },
                { KeyCode.F14, "F14" },
                { KeyCode.F15, "F15" },
                { KeyCode.Numlock, "Numlock" },
                { KeyCode.CapsLock, "Caps Lock" },
                { KeyCode.ScrollLock, "Scroll Lock" },
                { KeyCode.RightShift, "Right Shift" },
                { KeyCode.LeftShift, "Left Shift" },
                { KeyCode.RightControl, "Right Control" },
                { KeyCode.LeftControl, "Left Control" },
                { KeyCode.RightAlt, "Right Alt" },
                { KeyCode.LeftAlt, "Left Alt" },
                { KeyCode.RightCommand, "Command" },
                { KeyCode.LeftCommand, "Command" },
                { KeyCode.LeftWindows, "Left Windows" },
                { KeyCode.RightWindows, "Right Windows" },
                { KeyCode.AltGr, "Alt Gr" },
                { KeyCode.Help, "Help" },
                { KeyCode.Print, "Print" },
                { KeyCode.SysReq, "Sys Req" },
                { KeyCode.Break, "Break" },
                { KeyCode.Menu, "Menu" },
                { KeyCode.Mouse0, "Left Mouse" },
                { KeyCode.Mouse1, "Right Mouse" },
                { KeyCode.Mouse2, "Middle Mouse" },
                { KeyCode.Mouse3, "Mouse 3" },
                { KeyCode.Mouse4, "Mouse 4" },
                { KeyCode.Mouse5, "Mouse 5" },
                { KeyCode.Mouse6, "Mouse 6" },
                { KeyCode.JoystickButton0, "Joystick Button 0" },
                { KeyCode.JoystickButton1, "Joystick Button 1" },
                { KeyCode.JoystickButton2, "Joystick Button 2" },
                { KeyCode.JoystickButton3, "Joystick Button 3" },
                { KeyCode.JoystickButton4, "Joystick Button 4" },
                { KeyCode.JoystickButton5, "Joystick Button 5" },
                { KeyCode.JoystickButton6, "Joystick Button 6" },
                { KeyCode.JoystickButton7, "Joystick Button 7" },
                { KeyCode.JoystickButton8, "Joystick Button 8" },
                { KeyCode.JoystickButton9, "Joystick Button 9" },
                { KeyCode.JoystickButton10, "Joystick Button 10" },
                { KeyCode.JoystickButton11, "Joystick Button 11" },
                { KeyCode.JoystickButton12, "Joystick Button 12" },
                { KeyCode.JoystickButton13, "Joystick Button 13" },
                { KeyCode.JoystickButton14, "Joystick Button 14" },
                { KeyCode.JoystickButton15, "Joystick Button 15" },
                { KeyCode.JoystickButton16, "Joystick Button 16" },
                { KeyCode.JoystickButton17, "Joystick Button 17" },
                { KeyCode.JoystickButton18, "Joystick Button 18" },
                { KeyCode.JoystickButton19, "Joystick Button 19" },
                { KeyCode.Joystick1Button0, "Joystick 1 Button 0" },
                { KeyCode.Joystick1Button1, "Joystick 1 Button 1" },
                { KeyCode.Joystick1Button2, "Joystick 1 Button 2" },
                { KeyCode.Joystick1Button3, "Joystick 1 Button 3" },
                { KeyCode.Joystick1Button4, "Joystick 1 Button 4" },
                { KeyCode.Joystick1Button5, "Joystick 1 Button 5" },
                { KeyCode.Joystick1Button6, "Joystick 1 Button 6" },
                { KeyCode.Joystick1Button7, "Joystick 1 Button 7" },
                { KeyCode.Joystick1Button8, "Joystick 1 Button 8" },
                { KeyCode.Joystick1Button9, "Joystick 1 Button 9" },
                { KeyCode.Joystick1Button10, "Joystick 1 Button 10" },
                { KeyCode.Joystick1Button11, "Joystick 1 Button 11" },
                { KeyCode.Joystick1Button12, "Joystick 1 Button 12" },
                { KeyCode.Joystick1Button13, "Joystick 1 Button 13" },
                { KeyCode.Joystick1Button14, "Joystick 1 Button 14" },
                { KeyCode.Joystick1Button15, "Joystick 1 Button 15" },
                { KeyCode.Joystick1Button16, "Joystick 1 Button 16" },
                { KeyCode.Joystick1Button17, "Joystick 1 Button 17" },
                { KeyCode.Joystick1Button18, "Joystick 1 Button 18" },
                { KeyCode.Joystick1Button19, "Joystick 1 Button 19" },
                { KeyCode.Joystick2Button0,  "Joystick 2 Button 0" },
                { KeyCode.Joystick2Button1,  "Joystick 2 Button 1" },
                { KeyCode.Joystick2Button2,  "Joystick 2 Button 2" },
                { KeyCode.Joystick2Button3,  "Joystick 2 Button 3" },
                { KeyCode.Joystick2Button4,  "Joystick 2 Button 4" },
                { KeyCode.Joystick2Button5,  "Joystick 2 Button 5" },
                { KeyCode.Joystick2Button6,  "Joystick 2 Button 6" },
                { KeyCode.Joystick2Button7,  "Joystick 2 Button 7" },
                { KeyCode.Joystick2Button8,  "Joystick 2 Button 8" },
                { KeyCode.Joystick2Button9,  "Joystick 2 Button 9" },
                { KeyCode.Joystick2Button10, "Joystick 2 Button 10" },
                { KeyCode.Joystick2Button11, "Joystick 2 Button 11" },
                { KeyCode.Joystick2Button12, "Joystick 2 Button 12" },
                { KeyCode.Joystick2Button13, "Joystick 2 Button 13" },
                { KeyCode.Joystick2Button14, "Joystick 2 Button 14" },
                { KeyCode.Joystick2Button15, "Joystick 2 Button 15" },
                { KeyCode.Joystick2Button16, "Joystick 2 Button 16" },
                { KeyCode.Joystick2Button17, "Joystick 2 Button 17" },
                { KeyCode.Joystick2Button18, "Joystick 2 Button 18" },
                { KeyCode.Joystick2Button19, "Joystick 2 Button 19" },
                { KeyCode.Joystick3Button0,  "Joystick 3 Button 0" },
                { KeyCode.Joystick3Button1,  "Joystick 3 Button 1" },
                { KeyCode.Joystick3Button2,  "Joystick 3 Button 2" },
                { KeyCode.Joystick3Button3,  "Joystick 3 Button 3" },
                { KeyCode.Joystick3Button4,  "Joystick 3 Button 4" },
                { KeyCode.Joystick3Button5,  "Joystick 3 Button 5" },
                { KeyCode.Joystick3Button6,  "Joystick 3 Button 6" },
                { KeyCode.Joystick3Button7,  "Joystick 3 Button 7" },
                { KeyCode.Joystick3Button8,  "Joystick 3 Button 8" },
                { KeyCode.Joystick3Button9,  "Joystick 3 Button 9" },
                { KeyCode.Joystick3Button10, "Joystick 3 Button 10" },
                { KeyCode.Joystick3Button11, "Joystick 3 Button 11" },
                { KeyCode.Joystick3Button12, "Joystick 3 Button 12" },
                { KeyCode.Joystick3Button13, "Joystick 3 Button 13" },
                { KeyCode.Joystick3Button14, "Joystick 3 Button 14" },
                { KeyCode.Joystick3Button15, "Joystick 3 Button 15" },
                { KeyCode.Joystick3Button16, "Joystick 3 Button 16" },
                { KeyCode.Joystick3Button17, "Joystick 3 Button 17" },
                { KeyCode.Joystick3Button18, "Joystick 3 Button 18" },
                { KeyCode.Joystick3Button19, "Joystick 3 Button 19" },
                { KeyCode.Joystick4Button0,  "Joystick 4 Button 0" },
                { KeyCode.Joystick4Button1,  "Joystick 4 Button 1" },
                { KeyCode.Joystick4Button2,  "Joystick 4 Button 2" },
                { KeyCode.Joystick4Button3,  "Joystick 4 Button 3" },
                { KeyCode.Joystick4Button4,  "Joystick 4 Button 4" },
                { KeyCode.Joystick4Button5,  "Joystick 4 Button 5" },
                { KeyCode.Joystick4Button6,  "Joystick 4 Button 6" },
                { KeyCode.Joystick4Button7,  "Joystick 4 Button 7" },
                { KeyCode.Joystick4Button8,  "Joystick 4 Button 8" },
                { KeyCode.Joystick4Button9,  "Joystick 4 Button 9" },
                { KeyCode.Joystick4Button10, "Joystick 4 Button 10" },
                { KeyCode.Joystick4Button11, "Joystick 4 Button 11" },
                { KeyCode.Joystick4Button12, "Joystick 4 Button 12" },
                { KeyCode.Joystick4Button13, "Joystick 4 Button 13" },
                { KeyCode.Joystick4Button14, "Joystick 4 Button 14" },
                { KeyCode.Joystick4Button15, "Joystick 4 Button 15" },
                { KeyCode.Joystick4Button16, "Joystick 4 Button 16" },
                { KeyCode.Joystick4Button17, "Joystick 4 Button 17" },
                { KeyCode.Joystick4Button18, "Joystick 4 Button 18" },
                { KeyCode.Joystick4Button19, "Joystick 4 Button 19" },
                { KeyCode.Joystick5Button0,  "Joystick 5 Button 0" },
                { KeyCode.Joystick5Button1,  "Joystick 5 Button 1" },
                { KeyCode.Joystick5Button2,  "Joystick 5 Button 2" },
                { KeyCode.Joystick5Button3,  "Joystick 5 Button 3" },
                { KeyCode.Joystick5Button4,  "Joystick 5 Button 4" },
                { KeyCode.Joystick5Button5,  "Joystick 5 Button 5" },
                { KeyCode.Joystick5Button6,  "Joystick 5 Button 6" },
                { KeyCode.Joystick5Button7,  "Joystick 5 Button 7" },
                { KeyCode.Joystick5Button8,  "Joystick 5 Button 8" },
                { KeyCode.Joystick5Button9,  "Joystick 5 Button 9" },
                { KeyCode.Joystick5Button10, "Joystick 5 Button 10" },
                { KeyCode.Joystick5Button11, "Joystick 5 Button 11" },
                { KeyCode.Joystick5Button12, "Joystick 5 Button 12" },
                { KeyCode.Joystick5Button13, "Joystick 5 Button 13" },
                { KeyCode.Joystick5Button14, "Joystick 5 Button 14" },
                { KeyCode.Joystick5Button15, "Joystick 5 Button 15" },
                { KeyCode.Joystick5Button16, "Joystick 5 Button 16" },
                { KeyCode.Joystick5Button17, "Joystick 5 Button 17" },
                { KeyCode.Joystick5Button18, "Joystick 5 Button 18" },
                { KeyCode.Joystick5Button19, "Joystick 5 Button 19" },
                { KeyCode.Joystick6Button0,  "Joystick 6 Button 0" },
                { KeyCode.Joystick6Button1,  "Joystick 6 Button 1" },
                { KeyCode.Joystick6Button2,  "Joystick 6 Button 2" },
                { KeyCode.Joystick6Button3,  "Joystick 6 Button 3" },
                { KeyCode.Joystick6Button4,  "Joystick 6 Button 4" },
                { KeyCode.Joystick6Button5,  "Joystick 6 Button 5" },
                { KeyCode.Joystick6Button6,  "Joystick 6 Button 6" },
                { KeyCode.Joystick6Button7,  "Joystick 6 Button 7" },
                { KeyCode.Joystick6Button8,  "Joystick 6 Button 8" },
                { KeyCode.Joystick6Button9,  "Joystick 6 Button 9" },
                { KeyCode.Joystick6Button10, "Joystick 6 Button 10" },
                { KeyCode.Joystick6Button11, "Joystick 6 Button 11" },
                { KeyCode.Joystick6Button12, "Joystick 6 Button 12" },
                { KeyCode.Joystick6Button13, "Joystick 6 Button 13" },
                { KeyCode.Joystick6Button14, "Joystick 6 Button 14" },
                { KeyCode.Joystick6Button15, "Joystick 6 Button 15" },
                { KeyCode.Joystick6Button16, "Joystick 6 Button 16" },
                { KeyCode.Joystick6Button17, "Joystick 6 Button 17" },
                { KeyCode.Joystick6Button18, "Joystick 6 Button 18" },
                { KeyCode.Joystick6Button19, "Joystick 6 Button 19" },
                { KeyCode.Joystick7Button0,  "Joystick 7 Button 0" },
                { KeyCode.Joystick7Button1,  "Joystick 7 Button 1" },
                { KeyCode.Joystick7Button2,  "Joystick 7 Button 2" },
                { KeyCode.Joystick7Button3,  "Joystick 7 Button 3" },
                { KeyCode.Joystick7Button4,  "Joystick 7 Button 4" },
                { KeyCode.Joystick7Button5,  "Joystick 7 Button 5" },
                { KeyCode.Joystick7Button6,  "Joystick 7 Button 6" },
                { KeyCode.Joystick7Button7,  "Joystick 7 Button 7" },
                { KeyCode.Joystick7Button8,  "Joystick 7 Button 8" },
                { KeyCode.Joystick7Button9,  "Joystick 7 Button 9" },
                { KeyCode.Joystick7Button10, "Joystick 7 Button 10" },
                { KeyCode.Joystick7Button11, "Joystick 7 Button 11" },
                { KeyCode.Joystick7Button12, "Joystick 7 Button 12" },
                { KeyCode.Joystick7Button13, "Joystick 7 Button 13" },
                { KeyCode.Joystick7Button14, "Joystick 7 Button 14" },
                { KeyCode.Joystick7Button15, "Joystick 7 Button 15" },
                { KeyCode.Joystick7Button16, "Joystick 7 Button 16" },
                { KeyCode.Joystick7Button17, "Joystick 7 Button 17" },
                { KeyCode.Joystick7Button18, "Joystick 7 Button 18" },
                { KeyCode.Joystick7Button19, "Joystick 7 Button 19" },
                { KeyCode.Joystick8Button0,  "Joystick 8 Button 0" },
                { KeyCode.Joystick8Button1,  "Joystick 8 Button 1" },
                { KeyCode.Joystick8Button2,  "Joystick 8 Button 2" },
                { KeyCode.Joystick8Button3,  "Joystick 8 Button 3" },
                { KeyCode.Joystick8Button4,  "Joystick 8 Button 4" },
                { KeyCode.Joystick8Button5,  "Joystick 8 Button 5" },
                { KeyCode.Joystick8Button6,  "Joystick 8 Button 6" },
                { KeyCode.Joystick8Button7,  "Joystick 8 Button 7" },
                { KeyCode.Joystick8Button8,  "Joystick 8 Button 8" },
                { KeyCode.Joystick8Button9,  "Joystick 8 Button 9" },
                { KeyCode.Joystick8Button10, "Joystick 8 Button 10" },
                { KeyCode.Joystick8Button11, "Joystick 8 Button 11" },
                { KeyCode.Joystick8Button12, "Joystick 8 Button 12" },
                { KeyCode.Joystick8Button13, "Joystick 8 Button 13" },
                { KeyCode.Joystick8Button14, "Joystick 8 Button 14" },
                { KeyCode.Joystick8Button15, "Joystick 8 Button 15" },
                { KeyCode.Joystick8Button16, "Joystick 8 Button 16" },
                { KeyCode.Joystick8Button17, "Joystick 8 Button 17" },
                { KeyCode.Joystick8Button18, "Joystick 8 Button 18" },
                { KeyCode.Joystick8Button19, "Joystick 8 Button 19" },
            };
        }

        public struct Login
        {

            public static LocalizedString back = @"Back";

            public static LocalizedString forgot = @"Forgot Password?";

            public static LocalizedString login = @"Login";

            public static LocalizedString password = @"Password:";

            public static LocalizedString savepass = @"Save Password";

            public static LocalizedString title = @"Login";

            public static LocalizedString username = @"Username:";

        }

        public struct Main
        {

            public static LocalizedString gamename = @"Intersect Client";

        }

        public struct MainMenu
        {

            public static LocalizedString credits = @"Credits";

            public static LocalizedString exit = @"Exit";

            public static LocalizedString login = @"Login";

            public static LocalizedString options = @"Settings";

            public static LocalizedString optionstooltip = @"";

            public static LocalizedString register = @"Register";

            public static LocalizedString title = @"Main Menu";

        }

        public struct Options
        {

            public static LocalizedString fps30 = @"30";

            public static LocalizedString fps60 = @"60";

            public static LocalizedString fps90 = @"90";

            public static LocalizedString fps120 = @"120";

            public static LocalizedString apply = @"Apply";

            public static LocalizedString back = @"Back";

            public static LocalizedString cancel = @"Cancel";

            public static LocalizedString fullscreen = @"Fullscreen";

            public static LocalizedString AutocloseWindows = @"Auto-close Windows";

            public static LocalizedString musicvolume = @"Music Volume: {00}%";

            public static LocalizedString resolution = @"Resolution:";

            public static LocalizedString ResolutionCustom = @"Custom Resolution";

            public static LocalizedString restore = @"Restore Defaults";

            public static LocalizedString soundvolume = @"Sound Volume: {00}%";

            public static LocalizedString targetfps = @"Target FPS:";

            public static LocalizedString title = @"Options";

            public static LocalizedString unlimitedfps = @"No Limit";

            public static LocalizedString vsync = @"V-Sync";

        }

        public struct Parties
        {
            public static LocalizedString infight = @"You are currently fighting!";

            public static LocalizedString inviteprompt = @"{00} has invited you to their party. Do you accept?";

            public static LocalizedString kick = @"Kick {00}";

            public static LocalizedString kicklbl = @"Kick";

            public static LocalizedString leader = @"Leader";

            public static LocalizedString leadertip = @"Party Leader";

            public static LocalizedString leave = @"Leave Party";

            public static LocalizedString leavetip = @"Leave Tip";

            public static LocalizedString name = @"{00} - Lv. {01}";

            public static LocalizedString partyinvite = @"Party Invite";

            public static LocalizedString title = @"Party";

            public static LocalizedString vital0 = @"HP:";

            public static LocalizedString vital0val = @"{00} / {01}";

            public static LocalizedString vital1 = @"MP:";

            public static LocalizedString vital1val = @"{00} / {01}";

        }

        public struct QuestLog
        {

            public static LocalizedString abandon = @"Abandon";

            public static LocalizedString abandonprompt = @"Are you sure that you want to quit the quest ""{00}""?";

            public static LocalizedString abandontitle = @"Abandon Quest: {00}";

            public static LocalizedString back = @"Back";

            public static LocalizedString completed = @"Quest Completed";

            public static LocalizedString currenttask = @"Current Task:";

            public static LocalizedString inprogress = @"Quest In Progress";

            public static LocalizedString notstarted = @"Quest Not Started";

            public static LocalizedString taskitem = @"{00}/{01} {02}(s) gathered.";

            public static LocalizedString tasknpc = @"{00}/{01} {02}(s) slain.";

            public static LocalizedString title = @"Quest Log";

        }

        public struct QuestOffer
        {

            public static LocalizedString accept = @"Accept";

            public static LocalizedString decline = @"Decline";

            public static LocalizedString title = @"Quest Offer";

        }

        public struct Regex
        {

            public static LocalizedString email =
                @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

            public static LocalizedString password = @"^[-_=\+`~!@#\$%\^&\*()\[\]{}\\|;\:'"",<\.>/\?a-zA-Z0-9]{4,64}$";

            public static LocalizedString username = @"^[a-zA-Z0-9]{2,20}$";

        }

        public struct Registration
        {

            public static LocalizedString back = @"Back";

            public static LocalizedString confirmpass = @"Confirm Password:";

            public static LocalizedString email = @"Email:";

            public static LocalizedString emailinvalid = @"Email is invalid.";

            public static LocalizedString password = @"Password:";

            public static LocalizedString passwordmatch = @"Passwords didn't match!";

            public static LocalizedString register = @"Register";

            public static LocalizedString title = @"Register";

            public static LocalizedString username = @"Username:";

        }

        public struct ResetPass
        {

            public static LocalizedString back = @"Cancel";

            public static LocalizedString code = @"Enter the reset code that was sent to you:";

            public static LocalizedString fail = @"Error!";

            public static LocalizedString failmsg =
                @"The reset code was not valid, has expired, or the account does not exist!";

            public static LocalizedString inputcode = @"Please enter your password reset code.";

            public static LocalizedString password = @"New Password:";

            public static LocalizedString password2 = @"Confirm Password:";

            public static LocalizedString submit = @"Submit";

            public static LocalizedString success = @"Success!";

            public static LocalizedString successmsg = @"Your password has been reset!";

            public static LocalizedString title = @"Password Reset";

        }

        public struct Resources
        {

            public static LocalizedString cancelled = @"Download was Cancelled!";

            public static LocalizedString failedtoload = @"Failed to load Resources!";

            public static LocalizedString resourceexception =
                @"Failed to download client resources.\n\nException Info: {00}\n\nWould you like to try again?";

            public static LocalizedString resourcesfatal =
                @"Failed to load resources from client directory and Ascension Game Dev server. Cannot launch game!";

        }

        public struct Server
        {

            public static LocalizedString StatusLabel = @"Server Status: {00}";

            public static LocalizedString Online = @"Online";

            public static LocalizedString Offline = @"Offline";

            public static LocalizedString Failed = @"Network Error";

            public static LocalizedString Connecting = @"Connecting...";

            public static LocalizedString Unknown = @"Unknown";

            public static LocalizedString VersionMismatch = @"Bad Version";

            public static LocalizedString ServerFull = @"Full";

            public static LocalizedString HandshakeFailure = @"Handshake Error";

        }

        public struct Shop
        {

            public static LocalizedString buyitem = @"Buy Item";

            public static LocalizedString buyitemprompt = @"How many/much {00} would you like to buy?";

            public static LocalizedString cannotsell = @"This shop does not accept that item!";

            public static LocalizedString costs = @"Costs {00} {01}(s)";

            public static LocalizedString sellitem = @"Sell Item";

            public static LocalizedString sellitemprompt = @"How many/much {00} would you like to sell?";

            public static LocalizedString sellprompt = @"Do you wish to sell the item: {00}?";

            public static LocalizedString sellsfor = @"Sells for {00} {01}(s)";

            public static LocalizedString wontbuy = @"Shop Will Not Buy This Item";

        }

        public struct SpellDesc
        {

            public static LocalizedString addsymbol = @"+";

            public static LocalizedString casttime = @"Cast Time: {00} Seconds";

            public static LocalizedString cooldowntime = @"Cooldown: {00} Seconds";

            public static LocalizedString desc = @"{00}";

            public static LocalizedString duration = @"Duration: {00}s";

            public static Dictionary<int, LocalizedString> effectlist = new Dictionary<int, LocalizedString>
            {
                {0, @""},
                {1, @"Silences Target"},
                {2, @"Stuns Target"},
                {3, @"Snares Target"},
                {4, @"Blinds Target"},
                {5, @"Stealths Target"},
                {6, @"Transforms Target"},
                {7, @"Cleanses Target"},
                {8, @"Target becomes Invulnerable"},
                {9, @"Shields Target"},
                {10, @"Makes the target fall asleep"},
                {11, @"Applies an On Hit effect to the target"},
                {12, @"Taunts Target"},
            };

            public static LocalizedString effects = @"Effects:";

            public static LocalizedString prereqs = @"Prerequisites:";

            public static LocalizedString shield = @"Shielding: {00}";

            public static LocalizedString radius = @"Hit Radius: {00}";

            public static LocalizedString removesymbol = @"-";

            public static Dictionary<int, LocalizedString> spelltypes = new Dictionary<int, LocalizedString>
            {
                {0, @"Combat Spell"},
                {1, @"Warp to Map"},
                {2, @"Warp to Target"},
                {3, @"Dash"},
                {4, @"Special"},
            };

            public static Dictionary<int, LocalizedString> stats = new Dictionary<int, LocalizedString>
            {
                {0, @"Str: {00}"},
                {1, @"Int: {00}"},
                {2, @"Def: {00}"},
                {3, @"M.Def: {00}"},
                {4, @"Mov: {00}"},
                {5, @"Agi: {00}"}
            };

            public static Dictionary<int, LocalizedString> targettypes = new Dictionary<int, LocalizedString>
            {
                {0, @"Self Cast"},
                {1, @"Targetted - Range: {00} Tiles"},
                {2, @"AOE"},
                {3, @"Projectile - Range: {00} Tiles"},
                {4, @"On Hit"},
                {5, @"Trap"},
            };

            public static Dictionary<int, LocalizedString> vitals = new Dictionary<int, LocalizedString>
            {
                {0, @"HP: {00}{01}"},
                {1, @"MP: {00}{01}"},
            };

            public static Dictionary<int, LocalizedString> vitalcosts = new Dictionary<int, LocalizedString>
            {
                {0, @"HP Cost: {00}"},
                {1, @"MP Cost: {00}"},
            };

        }

        public struct Spells
        {

            public static LocalizedString cooldown = "{00}s";

            public static LocalizedString forgetspell = @"Forget Spell";

            public static LocalizedString forgetspellprompt = @"Are you sure you want to forget {00}?";

            public static LocalizedString title = @"Spells";

        }

        public struct Trading
        {

            public static LocalizedString accept = @"Accept";

            public static LocalizedString infight = @"You are currently fighting!";

            public static LocalizedString offeritem = @"Offer Item";

            public static LocalizedString offeritemprompt = @"How many/much {00} would you like to offer?";

            public static LocalizedString pending = @"Pending";

            public static LocalizedString requestprompt =
                @"{00} has invited you to trade items with them. Do you accept?";

            public static LocalizedString revokeitem = @"Revoke Item";

            public static LocalizedString revokeitemprompt = @"How many/much {00} would you like to revoke?";

            public static LocalizedString theiroffer = @"Their Offer:";

            public static LocalizedString title = @"Trading with {00}";

            public static LocalizedString traderequest = @"Trading Invite";

            public static LocalizedString value = @"Value: {00}";

            public static LocalizedString youroffer = @"Your Offer:";

        }

        public struct EscapeMenu
        {

            public static LocalizedString Title = @"Menu";

            public static LocalizedString Options = @"Options";

            public static LocalizedString CharacterSelect = @"Characters";

            public static LocalizedString Logout = @"Logout";

            public static LocalizedString ExitToDesktop = @"Desktop";

            public static LocalizedString Close = @"Close";

        }

        public struct Numbers
        {

            public static LocalizedString thousands = "k";

            public static LocalizedString millions = "m";

            public static LocalizedString billions = "b";

            public static LocalizedString dec = ".";

            public static LocalizedString comma = ",";

        }

        public struct Update
        {

            public static LocalizedString Checking = @"Checking for updates, please wait!";

            public static LocalizedString Updating = @"Downloading updates, please wait!";

            public static LocalizedString Restart = @"Update complete! Relaunch {00} to play!";

            public static LocalizedString Done = @"Update complete! Launching game!";

            public static LocalizedString Error = @"Update Error! Check logs for more info!";

            public static LocalizedString Files = @"{00} Files Remaining";

            public static LocalizedString Size = @"{00} Left";

            public static LocalizedString Percent = @"{00}%";

        }

        public struct GameWindow
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public static LocalizedString EntityNameAndLevel = @"{00} [Lv. {01}]";
        }

    }

}
