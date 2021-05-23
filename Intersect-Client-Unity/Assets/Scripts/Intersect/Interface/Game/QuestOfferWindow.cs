using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.GameObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class QuestOfferWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private TextMeshProUGUI textDescription;
        [SerializeField]
        private Button buttonAccept;
        [SerializeField]
        private TextMeshProUGUI labelAccept;
        [SerializeField]
        private Button buttonDecline;
        [SerializeField]
        private TextMeshProUGUI labelDecline;

        private void Start()
        {
            textTitle.text = Strings.QuestOffer.title;
            buttonAccept.onClick.AddListener(OnClickAccept);
            labelAccept.text = Strings.QuestOffer.accept;
            buttonDecline.onClick.AddListener(OnClickDecline);
            labelDecline.text = Strings.QuestOffer.decline;
        }

        internal void Draw(QuestBase quest)
        {
            if (quest == null)
            {
                Hide();
            }
            else
            {
                Show();
                textName.text = quest.Name;
                textDescription.text = quest.StartDescription;
            }

        }

        private void OnClickAccept()
        {
            if (Globals.QuestOffers.Count > 0)
            {
                PacketSender.SendAcceptQuest(Globals.QuestOffers[0]);
                Globals.QuestOffers.RemoveAt(0);
            }
        }

        private void OnClickDecline()
        {
            if (Globals.QuestOffers.Count > 0)
            {
                PacketSender.SendDeclineQuest(Globals.QuestOffers[0]);
                Globals.QuestOffers.RemoveAt(0);
            }
        }
    }

}
