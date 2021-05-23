using Intersect.Client.Entities;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class PartyWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private GameObject gameobjectClose;
        [SerializeField]
        private PartyMemberDisplayer partyMemberDisplayerPrefab;
        [SerializeField]
        private Transform partyMemberContainer;
        [SerializeField]
        private Button buttonLeave;
        [SerializeField]
        private GameObject leaveGameObject;
        [SerializeField]
        private TextMeshProUGUI labelLeave;

        private readonly List<PartyMemberDisplayer> memberDisplayers = new List<PartyMemberDisplayer>();


        private void Start()
        {
            textTitle.text = Strings.Parties.title;
            labelLeave.text = Strings.Parties.leave;
            buttonClose.onClick.AddListener(() => Hide());
            buttonLeave.onClick.AddListener(OnClickLeave);
        }

        internal void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            if (!Globals.Me.IsInParty())
            {
                leaveGameObject.SetActive(false);
                for (int i = 0; i < memberDisplayers.Count; i++)
                {
                    memberDisplayers[i].Hide();
                }
                return;
            }

            leaveGameObject.SetActive(true);

            bool iamLeader = Globals.Me.Party[0].Id.Equals(Globals.Me.Id);

            for (int i = 0; i < Globals.Me.Party.Count; i++)
            {
                PartyMember member = Globals.Me.Party[i];
                if (memberDisplayers.Count <= i)
                {
                    memberDisplayers.Add(Instantiate(partyMemberDisplayerPrefab, partyMemberContainer, false));
                }

                memberDisplayers[i].UpdateMember(member, i == 0, iamLeader);
            }

            for (int i = Globals.Me.Party.Count; i < memberDisplayers.Count; i++)
            {
                memberDisplayers[i].Hide();
            }
        }


        private void OnClickLeave()
        {
            if (Globals.Me.IsInParty())
            {
                PacketSender.SendPartyLeave();
            }
        }
    }
}
