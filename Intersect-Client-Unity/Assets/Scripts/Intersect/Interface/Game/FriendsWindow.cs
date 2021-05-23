using Intersect.Client.Entities;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{
    public class FriendsWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private GameObject gameobjectClose;
        [SerializeField]
        private CharacterDisplayer characterDisplayerPrefab;
        [SerializeField]
        private Transform characterContainer;
        [SerializeField]
        private Button buttonAddFriend;
        [SerializeField]
        private TextMeshProUGUI labelAddFriend;

        private readonly Dictionary<string, CharacterDisplayer> friends = new Dictionary<string, CharacterDisplayer>();

        private void Start()
        {
            textTitle.text = Strings.Friends.title;
            labelAddFriend.text = Strings.Friends.addfriend;
            buttonClose.onClick.AddListener(() => Hide());
            buttonAddFriend.onClick.AddListener(OnClickAddFriend);
        }

        internal void UpdateList()
        {
            foreach (FriendInstance friend in Globals.Me.Friends)
            {
                if (!friends.TryGetValue(friend.Name, out CharacterDisplayer displayer))
                {
                    displayer = Instantiate(characterDisplayerPrefab, characterContainer, false);
                    friends.Add(friend.Name, displayer);
                }

                displayer.UpdateFriend(friend, ClickLeft, ClickRight);

                if (friend.Online)
                {
                    displayer.transform.SetAsFirstSibling();
                }
            }

            if (Globals.Me.Friends.Count != friends.Count)
            {
                List<string> toDelete = new List<string>();
                foreach (KeyValuePair<string, CharacterDisplayer> friend in friends)
                {
                    bool found = false;
                    foreach (FriendInstance f in Globals.Me.Friends)
                    {
                        if (f.Name.Equals(friend.Key, StringComparison.Ordinal))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        toDelete.Add(friend.Key);
                    }
                }

                foreach (string name in toDelete)
                {
                    friends[name].Destroy();
                    friends.Remove(name);
                }
            }
        }

        private void OnClickAddFriend()
        {
            Interface.InputBox.Show(
                Strings.Friends.addfriend, Strings.Friends.addfriendprompt, true, InputBox.InputType.TextInput,
                AddFriend, null, 0);
        }

        private void AddFriend(object sender, EventArgs e)
        {
            InputBox ibox = (InputBox)sender;
            if (ibox.TextValue.Trim().Length >= 3) //Don't bother sending a packet less than the char limit
            {
                PacketSender.SendAddFriend(ibox.TextValue);
            }
        }

        internal void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            UpdateList();
        }


        private void ClickRight(object value)
        {
            FriendInstance friend = value as FriendInstance;
            Interface.InputBox.Show(
                Strings.Friends.removefriend, Strings.Friends.removefriendprompt.ToString(friend.Name), true,
                InputBox.InputType.YesNo, RemoveFriend, null, 0
            );
        }
        private void ClickLeft(object value)
        {
            FriendInstance friend = value as FriendInstance;

            //Only pm online players
            foreach (FriendInstance currentFriend in Globals.Me.Friends)
            {
                if (friend.Name.Equals(currentFriend.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (currentFriend.Online)
                    {
                        Interface.GameUi.SetChatboxText($"/pm {friend.Name} ");
                    }
                    return;
                }
            }
        }

        private void RemoveFriend(object sender, EventArgs e)
        {
            PacketSender.SendRemoveFriend(((InputBox)sender).TextValue);
        }
    }
}
