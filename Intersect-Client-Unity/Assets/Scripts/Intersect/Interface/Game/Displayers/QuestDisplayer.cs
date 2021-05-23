using Intersect.Client.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Displayers
{
    public class QuestDisplayer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private Button button;

        private Guid questId;

        private Action<Guid> onClick;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        internal void UpdateQuest(string name, Guid questId, Color color, Action<Guid> onClick)
        {
            this.questId = questId;
            this.onClick = onClick;
            textName.text = name;
            textName.color = color.ToColor32();
        }

        private void OnClick()
        {
            onClick?.Invoke(questId);
        }

        internal void Destroy()
        {
            Destroy(gameObject);
        }
    }
}