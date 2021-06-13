using Intersect.Client.Core.Controls;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.MessageSystem;
using Intersect.Client.Networking;
using Intersect.Client.UI.Components;
using Intersect.Client.UnityGame;
using Intersect.Client.Utils;
using Intersect.Enums;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Chat
{

    public class Chatbox : Window
    {
        [SerializeField]
        private DropdownUnselectable mChannelCombobox = default;

        //[SerializeField]
        //private TextMeshProUGUI mChannelLabel = default;

        [SerializeField]
        private TMP_InputField mChatboxInput = default;

        [SerializeField]
        private TextMeshProUGUI mChatboxMessages = default;

        //private ScrollBar mChatboxScrollBar;

        [SerializeField]
        private Button mChatboxSendButton = default;

        //[SerializeField]
        //private TextMeshProUGUI mChatboxText = default;

        [SerializeField]
        private TextMeshProUGUI mChatboxTitle = default;

        [SerializeField]
        private Button mBtnAllTab = default;

        [SerializeField]
        private Button mBtnLocalTab = default;

        [SerializeField]
        private Button mBtnPartyTab = default;

        [SerializeField]
        private Button mBtnGlobalTab = default;

        [SerializeField]
        private Button mBtnGuildTab = default;

        [SerializeField]
        private Button mBtnSystemTab = default;

        //Window Controls
        //private Image mChatboxWindow;

        private long mLastChatTime = -1;

        private int mMessageIndex;

        private bool mReceivedMessage;


        /// <summary>
        /// Defines which chat tab we are currently looking at.
        /// </summary>
        private ChatboxTab mCurrentTab = ChatboxTab.All;

        /// <summary>
        /// The last tab that was looked at before switching around, if a switch was made at all.
        /// </summary>
        private ChatboxTab mLastTab = ChatboxTab.All;

        /// <summary>
        /// Keep track of what chat channel we were chatting in on certain tabs so we can remember this when switching back to them.
        /// </summary>
        private readonly Dictionary<ChatboxTab, int> mLastChatChannel = new Dictionary<ChatboxTab, int>() {
            { ChatboxTab.All, 0 },
            { ChatboxTab.System, 0 },
        };

        protected override bool VisibleOnInit => true;

        public bool HasFocus => mChatboxInput.isFocused;

        protected override void OnInit()
        {
            base.OnInit();
            MessageManager.AttachListener(MessageTypes.JoinGamePacket, OnJoinGame);
        }

        private void OnDestroy()
        {
            MessageManager.DetachListener(MessageTypes.JoinGamePacket, OnJoinGame);
        }

        //Init
        private void Start()
        {
            if (mChatboxTitle != null)
            {
                mChatboxTitle.text = Strings.Chatbox.title;
            }

            mBtnAllTab.onClick.AddListener(() => ChangeTab(ChatboxTab.All));
            mBtnLocalTab.onClick.AddListener(() => ChangeTab(ChatboxTab.Local));
            mBtnPartyTab.onClick.AddListener(() => ChangeTab(ChatboxTab.Party));
            mBtnGlobalTab.onClick.AddListener(() => ChangeTab(ChatboxTab.Global));
            mBtnGuildTab.onClick.AddListener(() => ChangeTab(ChatboxTab.Guild));
            mBtnSystemTab.onClick.AddListener(() => ChangeTab(ChatboxTab.System));

            mChatboxInput.onSubmit.AddListener(ChatBoxInput_SubmitPressed);
            (mChatboxInput.placeholder as TextMeshProUGUI).text = GetDefaultInputText();
            mChatboxInput.characterLimit = Options.MaxChatLength;
            mChatboxSendButton.onClick.AddListener(SubmitText);

            mChannelCombobox.onValueChanged.AddListener(OnChangeChannel);

            // Disable this by default, since this is the default tab.
            mBtnAllTab.interactable = false;
        }

        private void OnJoinGame(object obj)
        {
            mChannelCombobox.ClearOptions();
            for (int i = 0; i < 4; i++)
            {
                mChannelCombobox.options.Add(new TMP_Dropdown.OptionData(Strings.Chatbox.channels[i]));
            }

            //Add admin channel only if power > 0.
            if (Globals.Me.Type > 0)
            {
                mChannelCombobox.options.Add(new TMP_Dropdown.OptionData(Strings.Chatbox.channeladmin));
            }
            mChannelCombobox.value = 0;
            mChannelCombobox.RefreshShownValue();

            mLastChatChannel[ChatboxTab.All] = 0;
            mLastChatChannel[ChatboxTab.System] = 0;
            ChangeTab(ChatboxTab.All);
        }

        public void Draw()
        {
            // Did the tab change recently? If so, we need to reset a few things to make it work...
            if (mLastTab != mCurrentTab)
            {
                mChatboxMessages.text = string.Empty;
                mMessageIndex = 0;
                mReceivedMessage = true;

                mLastTab = mCurrentTab;
            }

            if (mReceivedMessage)
            {
                //mChatboxMessages.ScrollToBottom();
                mReceivedMessage = false;
            }

            List<ChatboxMsg> msgs = ChatboxMsg.GetMessages(mCurrentTab);
            for (int i = mMessageIndex; i < msgs.Count; i++)
            {
                ChatboxMsg msg = msgs[i];

                mChatboxMessages.text += $"\n<color=#{ColorUtility.ToHtmlStringRGBA(msg.Color.ToColor32())}>{msg.Message}</color>";
                mReceivedMessage = true;
                mMessageIndex++;
            }
        }

        private void ChangeTab(ChatboxTab tab)
        {
            // Enable all buttons again!
            mBtnAllTab.interactable = tab != ChatboxTab.All;
            mBtnGlobalTab.interactable = tab != ChatboxTab.Global;
            mBtnLocalTab.interactable = tab != ChatboxTab.Local;
            mBtnPartyTab.interactable = tab != ChatboxTab.Party;
            mBtnSystemTab.interactable = tab != ChatboxTab.System;
            mBtnGuildTab.interactable = tab != ChatboxTab.Guild;

            mLastTab = ChatboxTab.Count;
            mCurrentTab = tab;
            // Change the default channel we're trying to chat in based on the tab we've just selected.
            SetChannelToTab(tab);
        }
        private void OnChangeChannel(int channel)
        {
            // If we're on the two generic tabs, remember which channel we're trying to type in so we can switch back to this channel when we decide to swap between tabs.
            if ((mCurrentTab == ChatboxTab.All || mCurrentTab == ChatboxTab.System))
            {
                mLastChatChannel[mCurrentTab] = channel;
            }
        }

        /// <summary>
        /// Sets the selected chat channel to type in by default to the channel corresponding to the provided tab.
        /// </summary>
        /// <param name="tab">The tab to use for reference as to which channel we want to speak in.</param>
        private void SetChannelToTab(ChatboxTab tab)
        {
            switch (tab)
            {
                case ChatboxTab.System:
                case ChatboxTab.All:
                    mChannelCombobox.value = mLastChatChannel[tab];
                    break;

                case ChatboxTab.Local:
                    mChannelCombobox.value = 0;
                    break;

                case ChatboxTab.Global:
                    mChannelCombobox.value = 1;
                    break;

                case ChatboxTab.Party:
                    mChannelCombobox.value = 2;
                    break;

                case ChatboxTab.Guild:
                    mChannelCombobox.value = 3;
                    break;

                default:
                    // remain unchanged.
                    return;
            }
            mChannelCombobox.RefreshShownValue();
        }

        public void SetChatboxText(string msg)
        {
            mChatboxInput.text = msg;
            mChatboxInput.caretPosition = msg.Length;
            Focus();
        }

        //private void ChatboxRow_Clicked(Base sender, ClickedEventArgs arguments) {
        //	var rw = (ListBoxRow)sender;
        //	var target = (string)rw.UserData;
        //	if (target != "") {
        //		if (mGameUi.AdminWindowOpen()) {
        //			mGameUi.AdminWindowSelectName(target);
        //		}
        //	}
        //}

        //Extra Methods
        public void Focus()
        {
            if (!mChatboxInput.isFocused)
            {
                mChatboxInput.Select();
            }
        }

        private void ChatBoxInput_SubmitPressed(string arg0)
        {
            SubmitText();
        }

        private void SubmitText()
        {
            TrySendMessage();
            mChatboxInput.text = string.Empty;
            Interface.ResetInputFocus();
        }

        private void TrySendMessage()
        {
            if (string.IsNullOrWhiteSpace(mChatboxInput.text))
            {
                mChatboxInput.text = string.Empty;
                return;
            }

            if (mLastChatTime > Globals.System.GetTimeMs())
            {
                ChatboxMsg.AddMessage(new ChatboxMsg(Strings.Chatbox.toofast, Color.Red, ChatMessageType.Error));
                mLastChatTime = Globals.System.GetTimeMs() + Options.MinChatInterval;

                return;
            }

            mLastChatTime = Globals.System.GetTimeMs() + Options.MinChatInterval;


            PacketSender.SendChatMsg(mChatboxInput.text.Trim(), (byte)mChannelCombobox.value);
        }

        private string GetDefaultInputText()
        {
            KeyCode key1 = Controls.ActiveControls.ControlMapping[Control.Enter].Key1;
            KeyCode key2 = Controls.ActiveControls.ControlMapping[Control.Enter].Key2;
            if (key1 == KeyCode.None && key2 != KeyCode.None)
            {
                return Strings.Chatbox.enterchat1.ToString(
                    Strings.Keys.keydict[key2]
                );
            }
            else if (key1 != KeyCode.None && key2 == KeyCode.None)
            {
                return Strings.Chatbox.enterchat1.ToString(
                    Strings.Keys.keydict[key1]
                );
            }
            else if (key1 != KeyCode.None && key2 != KeyCode.None)
            {
                return Strings.Chatbox.enterchat1.ToString(
                    Strings.Keys.keydict[key1],
                    Strings.Keys.keydict[key2]
                );
            }

            return Strings.Chatbox.enterchat;
        }
    }
}
