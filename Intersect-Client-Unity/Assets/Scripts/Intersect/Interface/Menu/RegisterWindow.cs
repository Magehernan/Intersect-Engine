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
    public class RegisterWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonRegister;
        [SerializeField]
        private TextMeshProUGUI textRegister;
        [SerializeField]
        private Button buttonBack;
        [SerializeField]
        private TextMeshProUGUI textBack;
        [SerializeField]
        private TextMeshProUGUI textUser;
        [SerializeField]
        private TMP_InputField inputUser;
        [SerializeField]
        private TextMeshProUGUI textEmail;
        [SerializeField]
        private TMP_InputField inputEmail;
        [SerializeField]
        private TextMeshProUGUI textPassword;
        [SerializeField]
        private TMP_InputField inputPassword;
        [SerializeField]
        private TextMeshProUGUI textConfirmPassword;
        [SerializeField]
        private TMP_InputField inputConfirmPassword;

        private void Start()
        {
            textTitle.text = Strings.Registration.title;
            textUser.text = Strings.Registration.username;
            textEmail.text = Strings.Registration.email;
            textPassword.text = Strings.Registration.password;
            textConfirmPassword.text = Strings.Registration.confirmpass;
            inputUser.onSubmit.AddListener(OnSubmitConfirmPassword);
            inputEmail.onSubmit.AddListener(OnSubmitConfirmPassword);
            inputPassword.onSubmit.AddListener(OnSubmitConfirmPassword);
            inputConfirmPassword.onSubmit.AddListener(OnSubmitConfirmPassword);


            textRegister.text = Strings.Registration.register;
            buttonRegister.onClick.AddListener(OnClickRegister);

            textBack.text = Strings.Registration.back;
            buttonBack.onClick.AddListener(OnClickBack);
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            inputUser.text = string.Empty;
            inputEmail.text = string.Empty;
            inputPassword.text = string.Empty;
            inputConfirmPassword.text = string.Empty;
            inputUser.Select();
        }

        internal void Draw()
        {
            if (!Networking.Network.Connected)
            {
                Hide();
                Interface.MenuUi.MainMenu.Show();
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.lostconnection));
            }

            // Re-Enable our buttons if we're not waiting for the server anymore with it disabled.
            if (!Globals.WaitingOnServer && !buttonRegister.interactable)
            {
                buttonRegister.interactable = true;
            }
        }

        private void OnClickBack()
        {
            Hide();
            Interface.MenuUi.MainMenu.Show();
        }

        private void OnClickRegister()
        {
            TryRegister();
        }

        private void OnSubmitConfirmPassword(string arg0)
        {
            TryRegister();
        }

        private void TryRegister()
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

            if (!FieldChecking.IsValidUsername(inputUser.text, Strings.Regex.username))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.usernameinvalid));
                return;
            }

            if (!inputPassword.text.Equals(inputConfirmPassword.text))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Registration.passwordmatch));
                return;
            }

            if (!FieldChecking.IsValidPassword(inputPassword.text, Strings.Regex.password))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.passwordinvalid));
                return;
            }

            if (!FieldChecking.IsWellformedEmailAddress(inputEmail.text, Strings.Regex.email))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Registration.emailinvalid));
                return;
            }

            Hide();

            //Hash Password
            using (SHA256Managed sha = new SHA256Managed())
            {
                string hashedPass = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(inputPassword.text.Trim()))).Replace("-", "");

                PacketSender.SendCreateAccount(inputUser.text, hashedPass, inputEmail.text);
            }

            Globals.WaitingOnServer = true;
            buttonRegister.interactable = false;
            ChatboxMsg.ClearMessages();
        }
    }
}
