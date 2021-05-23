using Intersect.Client.General;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.MessageSystem;
using Intersect.Client.UnityGame;
using Intersect.Network;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Menu
{

    public class MainMenu : Window
    {
        [Header("Windows")]
        [SerializeField]
        private LoginWindow mLoginWindow = default;
        [SerializeField]
        private RegisterWindow mRegisterWindow = default;
        [SerializeField]
        private CreateCharacterWindow mCreateCharacterWindow = default;
        [SerializeField]
        private SelectCharacterWindow mSelectCharacterWindow = default;
        [SerializeField]
        private ResetPasswordWindow mResetPasswordWindow = default;
        [SerializeField]
        private ForgotPasswordWindow mForgotPasswordWindow = default;
        [SerializeField]
        private OptionsWindow mOptionsWindow = default;
        [SerializeField]
        private CreditsWindow mCreditsWindow = default;
        [Header("Main Buttons")]
        [SerializeField]
        private TextMeshProUGUI textLogin;
        [SerializeField]
        private Button mLoginButton = default;
        [SerializeField]
        private TextMeshProUGUI textRegister;
        [SerializeField]
        private Button mRegisterButton = default;
        [SerializeField]
        private TextMeshProUGUI textOptions;
        [SerializeField]
        private Button mOptionsButton = default;
        [SerializeField]
        private TextMeshProUGUI textCredits;
        [SerializeField]
        private Button mCreditsButton = default;
        [SerializeField]
        private TextMeshProUGUI textExit;
        [SerializeField]
        private Button mExitButton = default;

        private bool mShouldOpenCharacterSelection;
        private bool mShouldOpenCharacterCreation;
        protected override bool VisibleOnInit => true;

        protected override void Awake()
        {
            base.Awake();

            mLoginButton.onClick.AddListener(ClickLogin);
            mRegisterButton.onClick.AddListener(ClickRegister);
            mCreditsButton.onClick.AddListener(ClickCredits);
            mExitButton.onClick.AddListener(ClickExit);
            mOptionsButton.onClick.AddListener(ClickOptions);

            textLogin.text = Strings.MainMenu.login;
            textRegister.text = Strings.MainMenu.register;
            textOptions.text = Strings.MainMenu.options;
            textCredits.text = Strings.MainMenu.credits;
            textExit.text = Strings.MainMenu.exit;

            OnNetworkStatus(NetworkStatus.Connecting);

            MessageManager.AttachListener(MessageTypes.NetworkStatus, OnNetworkStatus);
        }

        protected void OnDestroy()
        {
            MessageManager.DetachListener(MessageTypes.NetworkStatus, OnNetworkStatus);
        }

        internal void Draw()
        {
            if (mShouldOpenCharacterSelection)
            {
                CreateCharacterSelection();
            }

            if (mShouldOpenCharacterCreation)
            {
                CreateCharacterCreation();
            }

            if (!mLoginWindow.IsHidden)
            {
                mLoginWindow.Draw();
            }

            if (!mForgotPasswordWindow.IsHidden)
            {
                mForgotPasswordWindow.Draw();
            }

            if (!mCreateCharacterWindow.IsHidden)
            {
                mCreateCharacterWindow.Draw();
            }

            if (!mRegisterWindow.IsHidden)
            {
                mRegisterWindow.Draw();
            }

            if (!mSelectCharacterWindow.IsHidden)
            {
                mSelectCharacterWindow.Draw();
            }
        }

        internal void ResetInternal()
        {
            mLoginWindow.Hide();
            mRegisterWindow.Hide();
            mOptionsWindow.Hide();
            mCreditsWindow.Hide();
            mForgotPasswordWindow.Hide();
            mResetPasswordWindow.Hide();
            mCreateCharacterWindow.Hide();
            mSelectCharacterWindow.Hide();

            Show();
        }

        internal void NotifyOpenCharacterSelection(List<Character> characters)
        {
            mShouldOpenCharacterSelection = true;
            mSelectCharacterWindow.Characters = characters;
        }

        internal void ShowCharacterSelection()
        {
            mSelectCharacterWindow.Show();
        }

        internal void NotifyOpenForgotPassword()
        {
            ResetInternal();
            Hide();
            mForgotPasswordWindow.Show();
        }

        internal void NotifyOpenLogin()
        {
            ResetInternal();
            Hide();
            mLoginWindow.Show();
        }

        public void OpenResetPassword(string nameEmail)
        {
            ResetInternal();
            Hide();
            mResetPasswordWindow.Target = nameEmail;
            mResetPasswordWindow.Show();
        }

        private void CreateCharacterSelection()
        {
            Hide();
            mLoginWindow.Hide();
            mRegisterWindow.Hide();
            mOptionsWindow.Hide();
            mCreateCharacterWindow.Hide();
            mSelectCharacterWindow.Show();
            mShouldOpenCharacterSelection = false;
        }

        internal void NotifyOpenCharacterCreation()
        {
            mShouldOpenCharacterCreation = true;
        }

        public void CreateCharacterCreation()
        {
            Hide();
            mLoginWindow.Hide();
            mRegisterWindow.Hide();
            mOptionsWindow.Hide();
            mSelectCharacterWindow.Hide();
            mCreateCharacterWindow.Show();
            mCreateCharacterWindow.Init();
            mShouldOpenCharacterCreation = false;
        }

        private void ClickLogin()
        {
            Hide();
            mLoginWindow.Show();
        }

        private void ClickRegister()
        {
            Hide();
            mRegisterWindow.Show();
        }

        private void ClickOptions()
        {
            Hide();
            mOptionsWindow.Show();
        }

        private void ClickCredits()
        {
            Hide();
            mCreditsWindow.Show();
        }

        private void ClickExit()
        {
            Globals.IsRunning = false;
        }

        private void OnNetworkStatus(object obj)
        {
            NetworkStatus networkStatus = (NetworkStatus)obj;
            mLoginButton.interactable = networkStatus == NetworkStatus.Online;
            mRegisterButton.interactable = networkStatus == NetworkStatus.Online && Options.Loaded && !Options.BlockClientRegistrations;
        }
    }
}
