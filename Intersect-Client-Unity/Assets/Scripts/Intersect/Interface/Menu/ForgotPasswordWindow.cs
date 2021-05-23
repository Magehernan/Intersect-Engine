using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Menu
{
    public class ForgotPasswordWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textWindow;
        [SerializeField]
        private Button buttonSubmit;
        [SerializeField]
        private TextMeshProUGUI textSubmit;
        [SerializeField]
        private Button buttonBack;
        [SerializeField]
        private TextMeshProUGUI textBack;
        [SerializeField]
        private TextMeshProUGUI textUser;
        [SerializeField]
        private TMP_InputField inputUser;
        [SerializeField]
        private TextMeshProUGUI textHint;

        private void Start()
        {
            textWindow.text = Strings.ForgotPass.title;

            textUser.text = Strings.ForgotPass.label;
            inputUser.onSubmit.AddListener(OnSubmit);
            textHint.text = Strings.ForgotPass.hint;

            textSubmit.text = Strings.ForgotPass.submit;
            buttonSubmit.onClick.AddListener(OnClickSubmit);

            textBack.text = Strings.ForgotPass.back;
            buttonBack.onClick.AddListener(OnClickBack);
        }

        public override void Show(object obj = null)
        {
            inputUser.text = string.Empty;
            base.Show(obj);
        }

        public void Draw()
        {
            if (!Networking.Network.Connected)
            {
                Hide();
                Interface.MenuUi.MainMenu.Show();
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.lostconnection));
            }

            // Re-Enable our buttons if we're not waiting for the server anymore with it disabled.
            if (!Globals.WaitingOnServer && !buttonSubmit.interactable)
            {
                buttonSubmit.interactable = true;
            }
        }

        private void OnSubmit(string arg0)
        {
            OnClickSubmit();
        }

        private void OnClickBack()
        {
            Hide();
            Interface.MenuUi.MainMenu.NotifyOpenLogin();
        }

        private void OnClickSubmit()
        {
            if (Globals.WaitingOnServer)
            {
                return;
            }

            TrySendCode();

            buttonSubmit.interactable = false;
        }

        public void TrySendCode()
        {
            if (!Networking.Network.Connected)
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.notconnected));

                return;
            }

            if (!FieldChecking.IsValidUsername(inputUser.text, Strings.Regex.username) &&
                !FieldChecking.IsWellformedEmailAddress(inputUser.text, Strings.Regex.email))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.usernameinvalid));

                return;
            }

            Interface.MenuUi.MainMenu.OpenResetPassword(inputUser.text);
            PacketSender.SendRequestPasswordReset(inputUser.text);
        }
    }
}
