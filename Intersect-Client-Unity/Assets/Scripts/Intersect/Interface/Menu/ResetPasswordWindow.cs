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

    public class ResetPasswordWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonSubmit;
        [SerializeField]
        private TextMeshProUGUI textSubmit;
        [SerializeField]
        private Button buttonBack;
        [SerializeField]
        private TextMeshProUGUI textBack;
        [SerializeField]
        private TextMeshProUGUI textCode;
        [SerializeField]
        private TMP_InputField inputCode;
        [SerializeField]
        private TextMeshProUGUI textPassword;
        [SerializeField]
        private TMP_InputField inputPassword;
        [SerializeField]
        private TextMeshProUGUI textConfirmPassword;
        [SerializeField]
        private TMP_InputField inputConfirmPassword;
        public string Target { set; get; } = string.Empty;


        private void Start()
        {
            textTitle.text = Strings.ResetPass.title;
            textCode.text = Strings.ResetPass.code;
            textPassword.text = Strings.ResetPass.password;
            textConfirmPassword.text = Strings.ResetPass.password2;
            inputCode.onSubmit.AddListener(OnSubmitConfirmPassword);
            inputPassword.onSubmit.AddListener(OnSubmitConfirmPassword);
            inputConfirmPassword.onSubmit.AddListener(OnSubmitConfirmPassword);


            textSubmit.text = Strings.ForgotPass.submit;
            buttonSubmit.onClick.AddListener(OnClickSubmit);

            textBack.text = Strings.ForgotPass.back;
            buttonBack.onClick.AddListener(OnClickBack);
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            inputCode.text = string.Empty;
            inputPassword.text = string.Empty;
            inputConfirmPassword.text = string.Empty;
            inputCode.Select();
        }

        public void Draw()
        {
            if (!Networking.Network.Connected)
            {
                Hide();
                Interface.MenuUi.MainMenu.Show();
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.lostconnection));
            }
        }

        private void OnClickBack()
        {
            Hide();
            Interface.MenuUi.MainMenu.NotifyOpenLogin();
        }

        private void OnSubmitConfirmPassword(string arg0)
        {
            TrySendCode();
        }

        private void OnClickSubmit()
        {
            TrySendCode();
        }

        private void TrySendCode()
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

            if (string.IsNullOrEmpty(inputCode.text))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.ResetPass.inputcode));

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

            using (SHA256Managed sha = new SHA256Managed())
            {
                PacketSender.SendResetPassword(Target, inputCode.text,
                    BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(inputPassword.text.Trim()))).Replace("-", "")
                );
            }

            Globals.WaitingOnServer = true;
            ChatboxMsg.ClearMessages();
        }

    }

}
