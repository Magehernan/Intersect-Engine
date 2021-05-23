using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Shop
{

    public class ShopWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private ShopItem shopItemPrefab;
        [SerializeField]
        private Transform itemContainer;
        [SerializeField]
        private Transform descTransform;


        private readonly List<ShopItem> items = new List<ShopItem>();
        private bool reloadShop = true;

        private void Start()
        {
            buttonClose.onClick.AddListener(Interface.GameUi.NotifyCloseShop);
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            textTitle.text = Globals.GameShop.Name;
            reloadShop = true;
        }

        internal void Draw()
        {
            if (IsHidden)
            {
                return;
            }

            if (reloadShop)
            {
                reloadShop = false;
                InitItemContainer();
            }

            for (int i = 0; i < Globals.GameShop.SellingItems.Count; i++)
            {
                items[i].Set(Globals.GameShop.SellingItems[i]);
            }
        }

        private void InitItemContainer()
        {
            for (int i = 0; i < Globals.GameShop.SellingItems.Count; i++)
            {
                if (items.Count - 1 < i)
                {
                    ShopItem item = Instantiate(shopItemPrefab, itemContainer, false);
                    item.Setup(i, descTransform);
                    items.Add(item);
                }
                else
                {
                    items[i].gameObject.SetActive(true);
                }
            }

            for (int i = Globals.GameShop.SellingItems.Count; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(false);
            }
        }
    }
}
