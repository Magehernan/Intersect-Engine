using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Trades
{

    public class TradingWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI labelTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private TradeSegment yourTrade;
        [SerializeField]
        private TradeSegment theirTrade;
        [SerializeField]
        private Button buttonAccept;
        [SerializeField]
        private TextMeshProUGUI labelAccept;
        [SerializeField]
        private Transform descTransform;

        private void Start()
        {
            buttonClose.onClick.AddListener(Interface.GameUi.NotifyCloseTrading);
            buttonAccept.onClick.AddListener(AcceptTrade);

            yourTrade.Init(Strings.Trading.youroffer, 0, descTransform);
            theirTrade.Init(Strings.Trading.theiroffer, 1, descTransform);
        }

        public void Show(string tradingName)
        {
            labelTitle.text = Strings.Trading.title.ToString(tradingName);
            labelAccept.text = Strings.Trading.accept;
            Show();
        }

        internal void Draw()
        {

            if (IsHidden)
            {
                return;
            }

            yourTrade.Draw();
            theirTrade.Draw();

        }

        private void AcceptTrade()
        {
            labelAccept.text = Strings.Trading.pending;
            PacketSender.SendAcceptTrade();
        }
    }
}
