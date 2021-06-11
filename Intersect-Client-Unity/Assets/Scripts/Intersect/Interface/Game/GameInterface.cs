using Intersect.Client.General;
using Intersect.Client.Interface.Game.Bag;
using Intersect.Client.Interface.Game.Bank;
using Intersect.Client.Interface.Game.Character;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Interface.Game.Crafting;
using Intersect.Client.Interface.Game.EntityPanel;
using Intersect.Client.Interface.Game.Hotbar;
using Intersect.Client.Interface.Game.Inventory;
using Intersect.Client.Interface.Game.Shop;
using Intersect.Client.Interface.Game.Spells;
using Intersect.Client.Interface.Game.Trades;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.Enums;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class GameInterface : Window
    {
        public bool FocusChat { get; set; }
        public bool ChatFocussed => chatbox.HasFocus;

        public EscapeMenu EscapeMenu => escapeMenu;
        public Menu GameMenu => gameMenu;
        public AnnouncementWindow AnnouncementWindow => announcementWindow;


        [SerializeField]
        private GameObject gameObjectOverlay;
        [SerializeField]
        private Image imageOverlayColor;
        [Header("Windows"), SerializeField]
        private EscapeMenu escapeMenu;
        [SerializeField]
        private Menu gameMenu;
        [SerializeField]
        private AnnouncementWindow announcementWindow;
        [SerializeField]
        private AdminWindow adminWindow;
        [SerializeField]
        private BagWindow bagWindow;
        [SerializeField]
        private BankWindow bankWindow;
        [SerializeField]
        private CraftingWindow craftingWindow;
        [SerializeField]
        private DebugMenu debugMenu;
        [SerializeField]
        private Chatbox chatbox;
        [SerializeField]
        private EventWindow eventWindow;
        [SerializeField]
        private PictureWindow pictureWindow;
        [SerializeField]
        private QuestOfferWindow questOfferWindow;
        [SerializeField]
        private TradingWindow tradingWindow;
        [SerializeField]
        private ShopWindow shopWindow;

        public HotBarWindow hotbar;
        public EntityBox playerBox;
        public EntityBox targetBox;
        public DescWindow descWindow;



        private bool mShouldCloseBag;
        private bool mShouldCloseBank;
        private bool mShouldCloseCraftingTable;
        private bool mShouldCloseShop;
        private bool mShouldCloseTrading;
        private bool mShouldOpenAdminWindow;
        private bool mShouldOpenBag;
        private bool mShouldOpenBank;
        private bool mShouldOpenCraftingTable;
        private bool mShouldOpenShop;
        private bool mShouldOpenTrading;
        private bool mShouldUpdateQuestLog = true;
        private bool mShouldUpdateFriendsList;
        private bool mShouldUpdateGuildList;
        private bool mShouldHideGuildWindow;
        private string mTradingTarget;

        protected override void Awake()
        {
            base.Awake();
            playerBox.IsPlayerBox = true;
        }

        private void Start()
        {
            hotbar.Setup();
        }

        internal void Draw()
        {
            Show();
            if (Globals.Me != null && playerBox.MyEntity != Globals.Me)
            {
                playerBox.SetEntity(Globals.Me, EntityTypes.Player);
            }

            chatbox.Draw();
            eventWindow.Draw();
            pictureWindow.Draw();

            if (Globals.Picture != null)
            {
                if (pictureWindow.Picture != Globals.Picture.Picture ||
                     pictureWindow.Size != Globals.Picture.Size ||
                     pictureWindow.Clickable != Globals.Picture.Clickable)
                {
                    pictureWindow.Setup(Globals.Picture.Picture, Globals.Picture.Size, Globals.Picture.Clickable);
                }
            }
            else
            {
                pictureWindow.Hide();
            }

            if (FocusChat)
            {
                chatbox.Focus();
                FocusChat = false;
            }

            hotbar.Draw();
            playerBox.Draw();
            gameMenu.Draw(mShouldUpdateQuestLog);
            mShouldUpdateQuestLog = false;
            debugMenu.Draw();
            AnnouncementWindow.Draw();
            EscapeMenu.Draw();

            if (Globals.QuestOffers.Count > 0)
            {
                QuestBase quest = QuestBase.Get(Globals.QuestOffers[0]);
                questOfferWindow.Draw(quest);
            }
            else
            {
                questOfferWindow.Hide();
            }


            //Admin window update
            if (mShouldOpenAdminWindow)
            {
                ToggleAdminWindow();
            }

            //Shop Update
            if (mShouldOpenShop)
            {
                OpenShop();
            }
            if (mShouldCloseShop)
            {
                CloseShop();
            }
            shopWindow.Draw();

            //Bank Update
            if (mShouldOpenBank)
            {
                OpenBank();
            }
            if (mShouldCloseBank)
            {
                CloseBank();
            }
            bankWindow.Draw();

            //Bag Update
            if (mShouldOpenBag)
            {
                OpenBag();
            }
            if (mShouldCloseBag)
            {
                CloseBag();
            }
            bagWindow.Draw();

            //Crafting station update
            if (mShouldOpenCraftingTable)
            {
                OpenCraftingTable();
            }
            if (mShouldCloseCraftingTable)
            {
                CloseCraftingTable();
            }
            craftingWindow.Draw();

            //Trading update
            if (mShouldOpenTrading)
            {
                OpenTrading();
            }
            if (mShouldCloseTrading)
            {
                CloseTrading();
            }
            tradingWindow.Draw();

            if (mShouldUpdateFriendsList)
            {
                gameMenu.UpdateFriendsList();
                mShouldUpdateFriendsList = false;
            }

            if (mShouldUpdateGuildList)
            {
                gameMenu.UpdateGuildList();
                mShouldUpdateGuildList = false;
            }

            if (mShouldHideGuildWindow)
            {
                gameMenu.HideGuildWindow();
                mShouldHideGuildWindow = false;
            }
        }

        public void ChangeOverlayColor(Color color)
        {
            if (color.A == 0)
            {
                gameObjectOverlay.SetActive(false);
                return;
            }
            imageOverlayColor.color = new Color32(color.R, color.G, color.B, color.A);
            gameObjectOverlay.SetActive(true);
        }

        internal void NotifyUpdateGuildList()
        {
            mShouldUpdateGuildList = true;
        }

        internal void NotifyUpdateFriendsList()
        {
            mShouldUpdateFriendsList = true;
        }

        internal void NotifyQuestsUpdated()
        {
            mShouldUpdateQuestLog = true;
        }

        internal void NotifyOpenTrading(string traderName)
        {
            mShouldOpenTrading = true;
            mTradingTarget = traderName;
        }

        internal void NotifyCloseTrading()
        {
            mShouldCloseTrading = true;
        }

        internal void NotifyCloseBank()
        {
            mShouldCloseBank = true;
        }

        internal void NotifyOpenBank()
        {
            mShouldOpenBank = true;
        }

        internal void NotifyCloseBag()
        {
            mShouldCloseBag = true;
        }

        internal void NotifyOpenBag()
        {
            mShouldOpenBag = true;
        }

        internal void NotifyCloseShop()
        {
            mShouldCloseShop = true;
        }

        internal void NotifyOpenShop()
        {
            mShouldOpenShop = true;
        }

        internal void NotifyCloseCraftingTable()
        {
            mShouldCloseCraftingTable = true;
        }

        internal void NotifyOpenCraftingTable()
        {
            mShouldOpenCraftingTable = true;
        }

        internal void NotifyOpenAdminWindow()
        {
            mShouldOpenAdminWindow = true;
        }

        internal bool AdminWindowOpen()
        {
            return adminWindow.IsVisible;
        }

        internal void AdminWindowSelectName(string name)
        {
            adminWindow.SetName(name);
        }

        public void ToggleAdminWindow()
        {
            if (adminWindow.IsVisible)
            {
                adminWindow.Hide();
            }
            else
            {
                adminWindow.Show();
            }

            mShouldOpenAdminWindow = false;
        }

        public void HideGuildWindow()
        {
            mShouldHideGuildWindow = true;
        }

        public bool CloseAllWindows()
        {
            bool closedWindows = false;
            if (bagWindow.IsVisible)
            {
                NotifyCloseBag();
                closedWindows = true;
            }

            if (tradingWindow.IsVisible)
            {
                NotifyCloseTrading();
                closedWindows = true;
            }

            if (bankWindow.IsVisible)
            {
                NotifyCloseBank();
                closedWindows = true;
            }

            if (craftingWindow.IsVisible)
            {
                NotifyCloseCraftingTable();
                closedWindows = true;
            }

            if (shopWindow.IsVisible)
            {
                NotifyCloseShop();
                closedWindows = true;
            }

            if (gameMenu.HasWindowsOpen())
            {
                gameMenu.CloseAllWindows();
                closedWindows = true;
            }

            return closedWindows;
        }

        public void SetChatboxText(string text)
        {
            chatbox.SetChatboxText(text);
        }

        public void ShowHideDebug()
        {
            if (debugMenu.IsHidden)
            {
                debugMenu.Show();
            }
            else
            {
                debugMenu.Hide();
            }
        }

        private void CloseBank()
        {
            bankWindow.Hide();
            mShouldCloseBank = false;
            Globals.InBank = false;
            PacketSender.SendCloseBank();
        }

        private void OpenBank()
        {
            bankWindow.Show();
            gameMenu.OpenInventory();
            mShouldOpenBank = false;
            Globals.InBank = true;
        }

        private void CloseTrading()
        {
            tradingWindow.Hide();
            mShouldCloseTrading = false;
            Globals.InTrade = false;
            PacketSender.SendDeclineTrade();
        }

        private void OpenTrading()
        {
            tradingWindow.Show(mTradingTarget);
            gameMenu.OpenInventory();
            mShouldOpenTrading = false;
            Globals.InTrade = true;
        }

        private void CloseBag()
        {
            bagWindow.Hide();
            mShouldCloseBag = false;
            Globals.InBag = false;
            PacketSender.SendCloseBag();
        }

        private void OpenBag()
        {
            bagWindow.Show();
            mShouldOpenBag = false;
            Globals.InBag = true;
        }

        private void CloseShop()
        {
            Globals.InShop = false;
            mShouldCloseShop = false;
            shopWindow.Hide();
            PacketSender.SendCloseShop();
        }

        private void OpenShop()
        {
            Globals.InShop = true;
            shopWindow.Show(Globals.GameShop.Name);
            gameMenu.OpenInventory();
            mShouldOpenShop = false;
        }

        private void CloseCraftingTable()
        {
            craftingWindow.Hide();
            mShouldCloseCraftingTable = false;
            Globals.InCraft = false;
            PacketSender.SendCloseCrafting();
        }

        private void OpenCraftingTable()
        {
            craftingWindow.Show();
            gameMenu.OpenInventory();
            mShouldOpenCraftingTable = false;
            Globals.InCraft = true;
        }

    }
}