using Intersect.Client.General;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.Utilities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Menu
{

    public class LoginWindow : Window
    {
        private const string USER_KEY = "Username";
        private const string PASS_KEY = "Password";
        private const string PASS_ISSAVED = "lapasswordestaguarda";
        [SerializeField]
        private TMP_InputField textUser = default;
        [SerializeField]
        private TMP_InputField textPassword = default;
        [SerializeField]
        private Toggle toogleSavePassword = default;
        [SerializeField]
        private Button mLoginBtn = default;
        [SerializeField]
        private Button mBackBtn = default;
        [SerializeField]
        private Button mForgotPassswordButton;

        private string savedPass = string.Empty;
        private bool mUseSavedPass;


        //protected override MessageTypes ShowMessage => MessageTypes.ShowLogin;

        private void Start()
        {
            LoadCredentials();
            mLoginBtn.onClick.AddListener(TryLogin);
            mBackBtn.onClick.AddListener(ClickBack);
            mForgotPassswordButton.onClick.AddListener(ClickForgot);
            textPassword.onValueChanged.AddListener(PasswordChanged);
            textUser.onSubmit.AddListener(OnSubmit);
            textPassword.onSubmit.AddListener(OnSubmit);
        }

        public void Draw()
        {
            if (Networking.Network.Connected)
            {
                return;
            }

            Hide();
            Interface.MenuUi.MainMenu.Show();
            Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.lostconnection));
        }


        public override void Show(object obj = null)
        {
            base.Show(obj);

            mForgotPassswordButton.gameObject.SetActive(Options.Instance.SmtpValid);
            textUser.Select();
        }

        private void OnSubmit(string arg0)
        {
            TryLogin();
        }

        private void ClickForgot()
        {
            Interface.MenuUi.MainMenu.NotifyOpenForgotPassword();
        }

        private void PasswordChanged(string arg0)
        {
            mUseSavedPass = false;
        }

        private void ClickBack()
        {
            Hide();
            Interface.MenuUi.MainMenu.Show();
        }

        private void TryLogin()
        {
            if (Globals.WaitingOnServer)
            {
                return;
            }

            if (!Networking.Network.Connected)
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.notconnected));
                return;
            }

            if (!FieldChecking.IsValidUsername(textUser.text, Strings.Regex.username))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.usernameinvalid));
                return;
            }

            if (!FieldChecking.IsValidPassword(textPassword.text, Strings.Regex.password))
            {
                if (!mUseSavedPass)
                {
                    Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.passwordinvalid));
                    return;
                }
            }

            string password = savedPass;
            if (!mUseSavedPass)
            {
                password = ComputePasswordHash(textPassword.text.Trim());
            }

            PacketSender.SendLogin(textUser.text, password);
            SaveCredentials();
            Globals.WaitingOnServer = true;
            ChatboxMsg.ClearMessages();
        }

        private void LoadCredentials()
        {
            string name = Globals.Database.LoadPreference(USER_KEY);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            textUser.text = name;
            string pass = Globals.Database.LoadPreference(PASS_KEY);
            if (string.IsNullOrEmpty(pass))
            {
                return;
            }

            textPassword.text = PASS_ISSAVED;
            savedPass = pass;
            mUseSavedPass = true;
            toogleSavePassword.isOn = true;
        }

        private void SaveCredentials()
        {
            string username = string.Empty;
            string password = string.Empty;

            if (toogleSavePassword.isOn)
            {
                username = textUser.text.Trim();
                password = mUseSavedPass ? savedPass : ComputePasswordHash(textPassword.text.Trim());
            }

            Globals.Database.SavePreference(USER_KEY, username);
            Globals.Database.SavePreference(PASS_KEY, password);
        }

        private string ComputePasswordHash(string password)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty))).Replace("-", string.Empty);
            }
        }
    }

}
