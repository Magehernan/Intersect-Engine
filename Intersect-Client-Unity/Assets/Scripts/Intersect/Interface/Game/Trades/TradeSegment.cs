using Intersect.Client.General;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using Intersect.Localization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Intersect.Client.Interface.Game.Trades
{
    public class TradeSegment : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI labelTitle;
        [SerializeField]
        private TextMeshProUGUI labelValue;
        [SerializeField]
        private TradeItem tradeItemPrefab;
        [SerializeField]
        private Transform itemContainer;


        private bool initialized = false;
        private int mySide;

        private readonly List<TradeItem> items = new List<TradeItem>();

        internal void Init(LocalizedString youroffer, int side, Transform descTransform)
        {
            mySide = side;
            labelTitle.text = youroffer;

            for (int i = 0; i < Options.MaxInvItems; i++)
            {
                TradeItem item = Instantiate(tradeItemPrefab, itemContainer, false);
                item.Setup(i, mySide, descTransform);
                items.Add(item);
            }
            initialized = true;
        }

        internal void Draw()
        {
            if (!initialized)
            {
                return;
            }

            int g = 0;
            for (int i = 0; i < Options.MaxInvItems; i++)
            {
                Item item = Globals.Trade[mySide, i];
                if (item?.ItemId != Guid.Empty)
                {
                    ItemBase itemBase = ItemBase.Get(item.ItemId);
                    if (itemBase != null)
                    {
                        g += itemBase.Price * item.Quantity;
                    }
                }
                items[i].Set(item);
            }

            labelValue.text = Strings.Trading.value.ToString(g);
        }
    }
}
