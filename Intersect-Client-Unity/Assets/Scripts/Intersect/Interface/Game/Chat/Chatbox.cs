using Intersect.Client.Core.Controls;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UI.Components;
using Intersect.Client.UnityGame;
using Intersect.Client.Utils;
using Intersect.Enums;
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

        //[SerializeField]
        //private TextMeshProUGUI mChatboxTitle = default;

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

        protected override bool VisibleOnInit => true;

        public bool HasFocus => mChatboxInput.isFocused;

        //Init
        private void Start()
        {
            //mChatboxTitle.text = Strings.Chatbox.title;
            //mChatboxTitle.gameObject.SetActive(false);

            mBtnAllTab.onClick.AddListener(() => TabButtonClicked(ChatboxTab.All));
            mBtnLocalTab.onClick.AddListener(() => TabButtonClicked(ChatboxTab.Local));
            mBtnPartyTab.onClick.AddListener(() => TabButtonClicked(ChatboxTab.Party));
            mBtnGlobalTab.onClick.AddListener(() => TabButtonClicked(ChatboxTab.Global));
            mBtnGuildTab.onClick.AddListener(() => TabButtonClicked(ChatboxTab.Guild));
            mBtnSystemTab.onClick.AddListener(() => TabButtonClicked(ChatboxTab.System));

            //mChatbar.IsHidden = true;

            mChatboxInput.onSubmit.AddListener(ChatBoxInput_SubmitPressed);
            (mChatboxInput.placeholder as TextMeshProUGUI).text = GetDefaultInputText();
            mChatboxInput.characterLimit = Options.MaxChatLength;

            //mChannelCombobox = new ComboBox(mChatboxWindow, "ChatChannelCombobox");
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
            //mChannelCombobox.onValueChanged.AddListener(OnChangeChannelCombo);

            //mChatboxText.text = "ChatboxText";
            //mChatboxText.Font = mChatboxWindow.Parent.Skin.DefaultFont;
            //mChatboxText.IsHidden = true;

            mChatboxSendButton.onClick.AddListener(SubmitText);

            //mChatboxWindow.LoadJsonUi(GameContentManager.UI.InGame, Graphics.Renderer.GetResolutionString());


            // Disable this by default, since this is the default tab.
            mBtnAllTab.interactable = false;
        }

        private void TabButtonClicked(ChatboxTab tab)
        {
            // Enable all buttons again!
            mBtnAllTab.interactable = tab != ChatboxTab.All;
            mBtnGlobalTab.interactable = tab != ChatboxTab.Global;
            mBtnLocalTab.interactable = tab != ChatboxTab.Local;
            mBtnPartyTab.interactable = tab != ChatboxTab.Party;
            mBtnSystemTab.interactable = tab != ChatboxTab.System;
            mBtnGuildTab.interactable = tab != ChatboxTab.Guild;

            mCurrentTab = tab;
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

            System.Collections.Generic.List<ChatboxMsg> msgs = ChatboxMsg.GetMessages(mCurrentTab);
            for (int i = mMessageIndex; i < msgs.Count; i++)
            {
                ChatboxMsg msg = msgs[i];

                mChatboxMessages.text += $"\n<color=#{ColorUtility.ToHtmlStringRGBA(msg.Color.ToColor32())}>{msg.Message}</color>";
                mReceivedMessage = true;
                mMessageIndex++;
            }
        }

        public void SetChatboxText(string msg)
        {
            mChatboxInput.text = msg;
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
