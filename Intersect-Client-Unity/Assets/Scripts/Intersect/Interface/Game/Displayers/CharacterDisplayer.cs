using Intersect.Client.Entities;
using Intersect.Client.Localization;
using Intersect.Client.Utils;
using Intersect.Localization;
using Intersect.Network.Packets.Server;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Displayers
{
    public class CharacterDisplayer : MonoBehaviour, IPointerClickHandler 
    {
        [SerializeField]
        private TextMeshProUGUI textName;

        private Action<object> leftClick;
        private Action<object> rightClick;
        private object value;

        internal void UpdateFriend(FriendInstance friend, Action<object> leftClick, Action<object> rightClick) 
        {
            this.leftClick = leftClick;
            this.rightClick = rightClick;
            value = friend;

            textName.text = friend.Online ? $"{friend.Name} - {friend.Map}" : friend.Name;
            //Row Render color (red = offline, green = online)
            if (friend.Online == true)
            {
                textName.color = Color.Green.ToColor32();
            }
            else
            {
                textName.color = Color.Red.ToColor32();
            }
        }

        internal void UpdateGuildMember(GuildMember member, Action<object> leftClick, Action<object> rightClick)
        {
            this.leftClick = leftClick;
            this.rightClick = rightClick;
            value = member;

            LocalizedString str = member.Online ? Strings.Guilds.OnlineListEntry : Strings.Guilds.OfflineListEntry;
            textName.text = str.ToString(Options.Instance.Guild.Ranks[member.Rank].Title, member.Name, member.Map);
            //Row Render color (red = offline, green = online)
            if (member.Online)
            {
                textName.color = Color.Green.ToColor32();
            }
            else
            {
                textName.color = Color.Red.ToColor32();
            }
        }

        internal void Destroy()
        {
            Destroy(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                {
                    leftClick?.Invoke(value);
                }
                break;
                case PointerEventData.InputButton.Right:
                {
                    rightClick?.Invoke(value);
                }
                break;
                case PointerEventData.InputButton.Middle:
                    break;
            }
        }
    }
}
