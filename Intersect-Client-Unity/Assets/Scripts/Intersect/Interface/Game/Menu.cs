using Intersect.Client.General;
using Intersect.Client.Interface.Game.Character;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Interface.Game.Inventory;
using Intersect.Client.Interface.Game.Spells;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class Menu : Window
    {
        [Header("Windows"), SerializeField]
        private CharacterWindow characterWindow;
        [SerializeField]
        private SpellsWindow spellsWindow;
        [SerializeField]
        private FriendsWindow friendsWindow;
        [SerializeField]
        private InventoryWindow inventoryWindow;
        [SerializeField]
        private PartyWindow partyWindow;
        [SerializeField]
        private GuildWindow guildWindow;
        [SerializeField]
        private QuestsWindow questsWindow;
        [SerializeField]
        private MapItemWindow mapItemWindow;

        [Header("Menu Inferior"), SerializeField]
        private Button buttonInventory;
        [SerializeField]
        private Button buttonSpells;
        [SerializeField]
        private Button buttonCharacter;
        [SerializeField]
        private Button buttonQuests;
        [SerializeField]
        private Button buttonFriends;
        [SerializeField]
        private Button buttonGuild;
        [SerializeField]
        private Button buttonParty;
        [SerializeField]
        private Button buttonMenu;

        protected override bool VisibleOnInit => true;

        private void Start()
        {
            buttonInventory.onClick.AddListener(ToggleInventoryWindow);
            buttonSpells.onClick.AddListener(ToggleSpellsWindow);
            buttonCharacter.onClick.AddListener(ToggleCharacterWindow);
            buttonQuests.onClick.AddListener(ToggleQuestsWindow);
            buttonFriends.onClick.AddListener(ToggleFriendsWindow);
            buttonGuild.onClick.AddListener(ToggleGuildWindow);
            buttonParty.onClick.AddListener(TogglePartyWindow);
            buttonMenu.onClick.AddListener(ToggleEscapeMenu);
        }
        public override void Hide(object obj = null)
        {
            base.Hide(obj);
        }

        public void Draw(bool updateQuestLog)
        {
            inventoryWindow.Draw();
            spellsWindow.Draw();
            characterWindow.Draw();
            partyWindow.Draw();
            friendsWindow.Draw();
            guildWindow.Draw();
            questsWindow.Draw(updateQuestLog);
            mapItemWindow.Draw();
        }

        private void HideWindows()
        {
            if (!Globals.Database.HideOthersOnWindowOpen)
            {
                return;
            }

            CloseAllWindows();
        }

        public void CloseAllWindows()
        {
            characterWindow.Hide();
            friendsWindow.Hide();
            inventoryWindow.Hide();
            questsWindow.Hide();
            spellsWindow.Hide();
            partyWindow.Hide();
            guildWindow.Hide();
        }

        public void UpdateFriendsList()
        {
            friendsWindow.UpdateList();
        }

        public void UpdateGuildList()
        {
            guildWindow.UpdateList();
        }

        public void HideGuildWindow()
        {
            guildWindow.Hide();
        }

        internal void ToggleCharacterWindow()
        {
            if (characterWindow.IsVisible)
            {
                characterWindow.Hide();
            }
            else
            {
                HideWindows();
                characterWindow.Show();
            }
        }

        internal void ToggleFriendsWindow()
        {
            if (friendsWindow.IsVisible)
            {
                friendsWindow.Hide();
            }
            else
            {
                HideWindows();
                PacketSender.SendRequestFriends();
                friendsWindow.UpdateList();
                friendsWindow.Show();
            }
        }

        internal void ToggleGuildWindow()
        {
            if (string.IsNullOrEmpty(Globals.Me.Guild))
            {
                ChatboxMsg.AddMessage(new ChatboxMsg(Strings.Guilds.NotInGuild, CustomColors.Alerts.Error, ChatMessageType.Guild));
                return;
            }

            if (guildWindow.IsVisible)
            {
                guildWindow.Hide();
            }
            else
            {
                HideWindows();
                PacketSender.SendRequestGuild();
                guildWindow.UpdateList();
                guildWindow.Show();
            }
        }

        internal void TogglePartyWindow()
        {
            if (partyWindow.IsVisible)
            {
                partyWindow.Hide();
            }
            else
            {
                HideWindows();
                partyWindow.Show();
            }
        }

        internal void ToggleInventoryWindow()
        {
            if (inventoryWindow.IsVisible)
            {
                inventoryWindow.Hide();
            }
            else
            {
                HideWindows();
                inventoryWindow.Show();
            }
        }

        internal void ToggleQuestsWindow()
        {
            if (questsWindow.IsVisible)
            {
                questsWindow.Hide();
            }
            else
            {
                HideWindows();
                questsWindow.Show();
            }
        }

        internal void ToggleSpellsWindow()
        {
            if (spellsWindow.IsVisible)
            {
                spellsWindow.Hide();
            }
            else
            {
                HideWindows();
                spellsWindow.Show();
            }
        }

        public void OpenInventory()
        {
            inventoryWindow.Show();
        }

        public bool HasWindowsOpen()
        {
            return characterWindow.IsVisible
                || friendsWindow.IsVisible
                || inventoryWindow.IsVisible
                || questsWindow.IsVisible
                || spellsWindow.IsVisible
                || partyWindow.IsVisible
                || guildWindow.IsVisible;
        }

        private void ToggleEscapeMenu()
        {
            Interface.GameUi.EscapeMenu.ToggleHidden();
        }
    }
}