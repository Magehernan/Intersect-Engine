using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Inventory
{
    public class InventoryWindow : Window
    {

        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private GameObject gameobjectClose;
        [SerializeField]
        private InventoryItem inventoryItemPrefab;
        [SerializeField]
        private Transform inventoryContainer;
        [SerializeField]
        private Transform descTransform;

        //Item List
        private readonly List<InventoryItem> items = new List<InventoryItem>();
        private bool mInitializedItems = false;

        private void Start()
        {
            buttonClose.onClick.AddListener(() => Hide());
            textTitle.text = Strings.Inventory.title;
        }

        internal void Draw()
        {
            if (!mInitializedItems)
            {
                mInitializedItems = true;
                InitItemContainer();
            }

            if (!IsVisible)
            {
                return;
            }

            gameobjectClose.SetActive(Globals.CanCloseInventory);

            for (int i = 0; i < Options.MaxInvItems; i++)
            {
                items[i].Set(Globals.Me.Inventory[i]);
            }
        }


        private void InitItemContainer()
        {
            for (int i = 0; i < Options.MaxInvItems; i++)
            {
                InventoryItem inventoryItem = Instantiate(inventoryItemPrefab, inventoryContainer, false);
                inventoryItem.name = $"{nameof(InventoryItem)} ({i})";
                inventoryItem.Setup(i, descTransform);
                items.Add(inventoryItem);
            }
        }
    }
}
