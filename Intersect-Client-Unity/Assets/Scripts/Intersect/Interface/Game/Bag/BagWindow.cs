using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Bag
{

    public class BagWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private BagItem bagItemPrefab;
        [SerializeField]
        private Transform itemContainer;
        [SerializeField]
        private Transform descTransform;

        private readonly List<BagItem> items = new List<BagItem>();
        private bool reloadItems = true;

        private void Start()
        {
            textTitle.text = Strings.Bags.title;
            buttonClose.onClick.AddListener(Interface.GameUi.NotifyCloseBag);
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            reloadItems = true;
        }

        internal void Draw()
        {
            if (IsHidden)
            {
                return;
            }

            if (reloadItems)
            {
                reloadItems = false;
                InitItemContainer();
            }

            for (int i = 0; i < Globals.Bag.Length; i++)
            {
                items[i].Set(Globals.Bag[i]);
            }
        }

        private void InitItemContainer()
        {
            for (int i = 0; i < Globals.Bag.Length; i++)
            {
                if (items.Count - 1 < i)
                {
                    BagItem item = Instantiate(bagItemPrefab, itemContainer, false);
                    item.Setup(i, descTransform);
                    items.Add(item);
                }
                else
                {
                    items[i].gameObject.SetActive(true);
                }
            }

            for (int i = Globals.Bag.Length; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(false);
            }
        }
    }
}
