using Intersect.Client.Entities;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UI.Components;
using Intersect.Enums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Displayers
{
    public class PartyMemberDisplayer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private TextMeshProUGUI labelAction;
        [SerializeField]
        private Button buttonAction;
        [SerializeField]
        private TextMeshProUGUI labelHP;
        [SerializeField]
        private TextMeshProUGUI textHP;
        [SerializeField]
        private FillBar fillHP;
        [SerializeField]
        private TextMeshProUGUI labelMP;
        [SerializeField]
        private TextMeshProUGUI textMP;
        [SerializeField]
        private FillBar fillMP;

        private GameObject myGameObject;
        private Guid id;

        private void Awake()
        {
            myGameObject = gameObject;
            labelHP.text = Strings.Parties.vital0;
            labelMP.text = Strings.Parties.vital1;
            buttonAction.onClick.AddListener(OnClickAction);
        }

        private void OnClickAction()
        {
            PacketSender.SendPartyKick(id);
        }

        internal void UpdateMember(PartyMember member, bool isLeader, bool iamLeader)
        {
            id = member.Id;
            if (!myGameObject.activeSelf)
            {
                myGameObject.SetActive(true);
            }

            textName.text = Strings.Parties.name.ToString(member.Name, member.Level);

            int currentHp = member.Vital[(int)Vitals.Health];
            int maxHp = member.MaxVital[(int)Vitals.Health];
            fillHP.ChangeValue((float)currentHp / maxHp);
            textHP.text = Strings.Parties.vital0val.ToString(currentHp, maxHp);

            int currentMp = member.Vital[(int)Vitals.Mana];
            int maxMp = member.MaxVital[(int)Vitals.Mana];
            fillMP.ChangeValue((float)currentMp / maxMp);
            textMP.text = Strings.Parties.vital1val.ToString(currentMp, maxMp);

            labelAction.enabled = isLeader || iamLeader;
            buttonAction.interactable = !isLeader && iamLeader;
            if (isLeader)
            {
                labelAction.text = Strings.Parties.leader;
            }
            else
            {
                labelAction.text = Strings.Parties.kicklbl;
            }
        }

        internal void Hide()
        {
            if (myGameObject.activeSelf)
            {
                myGameObject.SetActive(false);
            }
        }
    }
}
