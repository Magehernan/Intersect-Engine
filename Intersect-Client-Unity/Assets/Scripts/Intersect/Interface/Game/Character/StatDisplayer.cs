using Intersect.Client.Networking;
using Intersect.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Character
{
    public class StatDisplayer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textStat;
        [SerializeField]
        private Button buttonAdd;
        [SerializeField]
        private GameObject gameObjectAdd;

        private string label;
        private string statName;
        private Stats stat;

        private void Awake()
        {
            buttonAdd.onClick.AddListener(OnClickAdd);
        }

        internal void Setup(string label, string statName, Stats stat)
        {
            this.label = label;
            this.statName = statName;
            this.stat = stat;
        }

        internal void UpdateValue(int[] statValues, int statPoints)
        {
            int value = statValues[(int)stat];
            textStat.text = string.Format(label, statName, value);
            gameObjectAdd.SetActive(statPoints > 0 && value < Options.MaxStatValue);
        }

        private void OnClickAdd()
        {
            PacketSender.SendUpgradeStat((byte)stat);
        }
    }
}
