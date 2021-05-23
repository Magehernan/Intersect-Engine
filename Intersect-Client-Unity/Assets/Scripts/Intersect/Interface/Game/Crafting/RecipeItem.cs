using Intersect.GameObjects;
using Intersect.GameObjects.Crafting;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Crafting
{

    public class RecipeItem : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI labelName;
        [SerializeField]
        private Button button;

        private Action<CraftBase> onClick;
        private CraftBase craftBase;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            onClick?.Invoke(craftBase);
        }

        internal void Set(int index, CraftBase craftBase, Action<CraftBase> onClick)
        {
            this.craftBase = craftBase;
            this.onClick = onClick;
            labelName.text = $"{index + 1}) {ItemBase.GetName(craftBase.ItemId)}";
        }
    }
}
