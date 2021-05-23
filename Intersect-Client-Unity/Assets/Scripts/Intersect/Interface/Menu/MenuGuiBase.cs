using Intersect.Client.Localization;
using Intersect.Client.MessageSystem;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.Network;
using System;
using TMPro;
using UnityEngine;

namespace Intersect.Client.Interface.Menu
{
    public class MenuGuiBase : Window
    {

        public MainMenu MainMenu;

        [SerializeField]
        private TextMeshProUGUI textNetworkStatus = default;

        private bool mShouldReset;

        protected override void Awake()
        {
            base.Awake();
            OnNetworkStatus(NetworkStatus.Connecting);
            MessageManager.AttachListener(MessageTypes.NetworkStatus, OnNetworkStatus);
        }

        private void OnDestroy()
        {
            MessageManager.DetachListener(MessageTypes.NetworkStatus, OnNetworkStatus);
        }


        internal void ResetInterface()
        {
            mShouldReset = true;
        }

        public void Draw()
        {
            Show();
            if (mShouldReset)
            {
                MainMenu.ResetInternal();
                mShouldReset = false;
            }

            MainMenu.Draw();
        }

        private void OnNetworkStatus(object obj)
        {
            NetworkStatus networkStatus = (NetworkStatus)obj;

            textNetworkStatus.text = Strings.Server.StatusLabel.ToString(networkStatus.ToLocalizedString());
        }
    }
}