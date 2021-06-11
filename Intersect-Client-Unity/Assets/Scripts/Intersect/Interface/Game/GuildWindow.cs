using Intersect.Client.Entities;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.Client.Utils;
using Intersect.Config.Guilds;
using Intersect.Network.Packets.Server;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class GuildWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textGuildName;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private GameObject gameobjectClose;
        [SerializeField]
        private CharacterDisplayer characterDisplayerPrefab;
        [SerializeField]
        private Transform characterContainer;
        [SerializeField]
        private Button buttonInvite;
        [SerializeField]
        private TextMeshProUGUI labelInvite;
        [SerializeField]
        private Button buttonLeave;
        [SerializeField]
        private TextMeshProUGUI labelLeave;

        [SerializeField]
        private Transform contextMenuTransform;
        [SerializeField]
        private Button buttonTextPrefab;
        [SerializeField]
        private AutoHideClickOutside autoHideContextMenu;

        private readonly Dictionary<string, CharacterDisplayer> members = new Dictionary<string, CharacterDisplayer>();

        private GameObject contextMenuGameObject;

        private GameObject mPmOption;

        private GameObject[] mPromoteOptions;

        private GameObject[] mDemoteOptions;

        private GameObject mKickOption;

        private GameObject mTransferOption;

        private GuildMember mSelectedMember;

        private void Start()
        {
            labelInvite.text = Strings.Guilds.Invite;
            labelLeave.text = Strings.Guilds.Leave;
            buttonClose.onClick.AddListener(() => Hide());
            buttonInvite.onClick.AddListener(OnClickInvite);
            buttonLeave.onClick.AddListener(OnClickLeave);

            contextMenuGameObject = contextMenuTransform.gameObject;
            contextMenuGameObject.SetActive(false);
            Button buttonPM = Instantiate(buttonTextPrefab, contextMenuTransform, false);
            mPmOption = buttonPM.gameObject;
            buttonPM.onClick.AddListener(OnClickPM);
            buttonPM.GetComponentInChildren<TextMeshProUGUI>().text = Strings.Guilds.PM;

            mPromoteOptions = new GameObject[Options.Instance.Guild.Ranks.Length - 2];
            for (int i = 1; i < Options.Instance.Guild.Ranks.Length - 1; i++)
            {
                Button button = Instantiate(buttonTextPrefab, contextMenuTransform, false);
                mPromoteOptions[i - 1] = button.gameObject;
                button.GetComponentInChildren<TextMeshProUGUI>().text = Strings.Guilds.Promote.ToString(Options.Instance.Guild.Ranks[i].Title);
                int rank = i;
                button.onClick.AddListener(() => OnClickPromote(rank));
            }

            mDemoteOptions = new GameObject[Options.Instance.Guild.Ranks.Length - 2];
            for (int i = 2; i < Options.Instance.Guild.Ranks.Length; i++)
            {
                Button button = Instantiate(buttonTextPrefab, contextMenuTransform, false);
                mDemoteOptions[i - 2] = button.gameObject;
                button.GetComponentInChildren<TextMeshProUGUI>().text = Strings.Guilds.Demote.ToString(Options.Instance.Guild.Ranks[i].Title);
                int rank = i;
                button.onClick.AddListener(() => OnClickDemote(rank));
            }

            Button buttonKick = Instantiate(buttonTextPrefab, contextMenuTransform, false);
            mKickOption = buttonKick.gameObject;
            buttonKick.GetComponentInChildren<TextMeshProUGUI>().text = Strings.Guilds.Kick.ToString();
            buttonKick.onClick.AddListener(OnClickKick);

            Button buttonTransfer = Instantiate(buttonTextPrefab, contextMenuTransform, false);
            mTransferOption = buttonTransfer.gameObject;
            buttonTransfer.GetComponentInChildren<TextMeshProUGUI>().text = Strings.Guilds.Transfer.ToString();
            buttonTransfer.onClick.AddListener(OnClickTransfer);
            autoHideContextMenu.Init(() => contextMenuGameObject.SetActive(false));
        }

        internal void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            // Force our window title to co-operate, might be empty after creating/joining a guild.
            if (string.IsNullOrEmpty(textGuildName.text) || !textGuildName.text.Equals(Globals.Me.Guild, StringComparison.Ordinal))
            {
                textGuildName.text = Globals.Me.Guild;
            }
        }

        private void OnClickPM()
        {
            contextMenuGameObject.SetActive(false);
            OnClickLeft(mSelectedMember);
        }

        #region Member List
        internal void UpdateList()
        {
            foreach (GuildMember member in Globals.Me.GuildMembers)
            {
                if (!members.TryGetValue(member.Name, out CharacterDisplayer displayer))
                {
                    displayer = Instantiate(characterDisplayerPrefab, characterContainer, false);
                    members.Add(member.Name, displayer);
                }

                displayer.UpdateGuildMember(member, OnClickLeft, OnClickRight);
            }

            if (Globals.Me.GuildMembers.Length != members.Count)
            {
                List<string> toDelete = new List<string>();
                foreach (KeyValuePair<string, CharacterDisplayer> member in members)
                {
                    bool found = false;
                    foreach (GuildMember guildMember in Globals.Me.GuildMembers)
                    {
                        if (guildMember.Name.Equals(member.Key, StringComparison.Ordinal))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        toDelete.Add(member.Key);
                    }
                }

                foreach (string name in toDelete)
                {
                    members[name].Destroy();
                    members.Remove(name);
                }
            }

            buttonLeave.gameObject.SetActive(Globals.Me != null && Globals.Me.Rank > 0);
        }

        private void OnClickLeft(object value)
        {
            GuildMember member = value as GuildMember;
            if (Globals.Me.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (GuildMember currentMember in Globals.Me.GuildMembers)
            {
                if (currentMember.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (currentMember.Online)
                    {
                        Interface.GameUi.SetChatboxText($"/pm {currentMember.Name} ");
                    }
                    return;
                }
            }
        }

        private void OnClickRight(object value)
        {
            if (!(value is GuildMember member) || member.Id == Globals.Me?.Id)
            {
                return;
            }

            mSelectedMember = member;

            GuildRank rank = Globals.Me?.GuildRank ?? null;

            if (rank == null)
            {
                return;
            }

            int rankIndex = Globals.Me.Rank;
            bool isOwner = rankIndex == 0;

            bool isOnline = mSelectedMember?.Online ?? false;
            bool show = isOnline;
            //Only pm online players
            mPmOption.SetActive(isOnline);

            //Promote Options
            for (int i = 0; i < mPromoteOptions.Length; i++)
            {
                int currentRank = i + 1;
                bool canPromote = (isOwner || rank.Permissions.Promote) && currentRank > rankIndex && currentRank < member.Rank && member.Rank > rankIndex;
                show |= canPromote;
                mPromoteOptions[i].SetActive(canPromote);
            }

            //Demote Options
            for (int i = 0; i < mDemoteOptions.Length; i++)
            {
                int currentRank = i + 2;
                bool canDemote = (isOwner || rank.Permissions.Demote) && currentRank > rankIndex && currentRank > member.Rank && member.Rank > rankIndex;
                show |= canDemote;
                mDemoteOptions[i].SetActive(canDemote);
            }
            bool canKick = (rank.Permissions.Kick || isOwner) && member.Rank > rankIndex;
            mKickOption.SetActive(canKick);
            show |= canKick;

            mTransferOption.SetActive(isOwner);
            show |= isOwner;
            if (!show)
            {
                return;
            }

            contextMenuTransform.position = Input.mousePosition;
            contextMenuGameObject.SetActive(true);
        }
        #endregion

        #region Adding/Leaving
        private void OnClickInvite()
        {
            Interface.InputBox.Show(Strings.Guilds.InviteMemberTitle, Strings.Guilds.InviteMemberPrompt.ToString(Globals.Me.Guild), true, InputBox.InputType.TextInput, AddMember, null, 0);
        }

        private void AddMember(object sender, EventArgs e)
        {
            InputBox ibox = (InputBox)sender;
            if (ibox.TextValue.Trim().Length >= 3) //Don't bother sending a packet less than the char limit
            {
                PacketSender.SendInviteGuild(ibox.TextValue);
            }
        }

        private void OnClickLeave()
        {
            Interface.InputBox.Show(Strings.Guilds.LeaveTitle, Strings.Guilds.LeavePrompt, true, InputBox.InputType.YesNo, LeaveGuild, null, 0);
        }

        private void LeaveGuild(object sender, EventArgs e)
        {
            PacketSender.SendLeaveGuild();
        }
        #endregion

        #region Kicking
        private void OnClickKick()
        {
            contextMenuGameObject.SetActive(false);
            GuildRank rank = Globals.Me?.GuildRank ?? null;
            int? rankIndex = Globals.Me?.Rank;
            bool isOwner = rankIndex == 0;
            if (mSelectedMember != null && (rank.Permissions.Kick || isOwner) && mSelectedMember.Rank > rankIndex)
            {
                Interface.InputBox.Show(
                    Strings.Guilds.KickTitle, Strings.Guilds.KickPrompt.ToString(mSelectedMember?.Name), true, InputBox.InputType.YesNo,
                    KickMember, null, mSelectedMember
                );
            }
        }

        private void KickMember(object sender, EventArgs e)
        {
            InputBox input = (InputBox)sender;
            GuildMember member = (GuildMember)input.UserData;
            PacketSender.SendKickGuildMember(member.Id);
        }

        #endregion

        #region Transferring
        private void OnClickTransfer()
        {
            contextMenuGameObject.SetActive(false);
            GuildRank rank = Globals.Me?.GuildRank ?? null;
            int? rankIndex = Globals.Me?.Rank;
            bool isOwner = rankIndex == 0;
            if (mSelectedMember != null && (rank.Permissions.Kick || isOwner) && mSelectedMember.Rank > rankIndex)
            {
                Interface.InputBox.Show(
                    Strings.Guilds.TransferTitle, Strings.Guilds.TransferPrompt.ToString(mSelectedMember?.Name, rank.Title, Globals.Me?.Guild), true, InputBox.InputType.TextInput,
                    TransferGuild, null, mSelectedMember
                );
            }
        }

        private void TransferGuild(object sender, EventArgs e)
        {
            InputBox input = (InputBox)sender;
            GuildMember member = (GuildMember)input.UserData;
            string text = input.TextValue;
            if (text == Globals.Me?.Guild)
            {
                PacketSender.SendTransferGuild(member.Id);
            }
        }
        #endregion

        #region Promoting
        private void OnClickPromote(int newRank)
        {
            contextMenuGameObject.SetActive(false);
            GuildRank rank = Globals.Me?.GuildRank ?? null;
            int? rankIndex = Globals.Me?.Rank;
            bool isOwner = rankIndex == 0;
            if (mSelectedMember != null && (rank.Permissions.Kick || isOwner) && mSelectedMember.Rank > rankIndex)
            {
                Interface.InputBox.Show(
                    Strings.Guilds.PromoteTitle, Strings.Guilds.PromotePrompt.ToString(mSelectedMember?.Name, Options.Instance.Guild.Ranks[newRank].Title), true, InputBox.InputType.YesNo,
                    PromoteMember, null, new KeyValuePair<GuildMember, int>(mSelectedMember, newRank)
                );
            }
        }

        private void PromoteMember(object sender, EventArgs e)
        {
            InputBox input = (InputBox)sender;
            KeyValuePair<GuildMember, int> memberRankPair = (KeyValuePair<GuildMember, int>)input.UserData;
            PacketSender.SendPromoteGuildMember(memberRankPair.Key?.Id ?? Guid.Empty, memberRankPair.Value);
        }

        #endregion

        #region Demoting
        private void OnClickDemote(int newRank)
        {
            contextMenuGameObject.SetActive(false);
            GuildRank rank = Globals.Me?.GuildRank ?? null;
            int? rankIndex = Globals.Me?.Rank;
            bool isOwner = rankIndex == 0;
            if (mSelectedMember != null && (rank.Permissions.Kick || isOwner) && mSelectedMember.Rank > rankIndex)
            {
                Interface.InputBox.Show(
                    Strings.Guilds.DemoteTitle, Strings.Guilds.DemotePrompt.ToString(mSelectedMember?.Name, Options.Instance.Guild.Ranks[newRank].Title), true, InputBox.InputType.YesNo,
                    DemoteMember, null, new KeyValuePair<GuildMember, int>(mSelectedMember, newRank)
                );
            }
        }

        private void DemoteMember(object sender, EventArgs e)
        {
            InputBox input = (InputBox)sender;
            KeyValuePair<GuildMember, int> memberRankPair = (KeyValuePair<GuildMember, int>)input.UserData;
            PacketSender.SendDemoteGuildMember(memberRankPair.Key?.Id ?? Guid.Empty, memberRankPair.Value);
        }

        #endregion
    }

}
