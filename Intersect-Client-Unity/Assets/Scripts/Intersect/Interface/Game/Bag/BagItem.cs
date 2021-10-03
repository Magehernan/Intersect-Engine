﻿using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Interface.Game.Inventory;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Bag
{

    public class BagItem : UIBaseItem, IPointerClickHandler
    {
        //Slot info
        private int currentAmt;
        private Guid currentItemId;
        private string texLoaded = string.Empty;
        public bool isDragging;

        public override void Setup(int slot, Transform descTransformDisplay)
        {
            base.Setup(slot, descTransformDisplay);
            displayer.TextTopVisible(false);
            displayer.TextCoolDownVisible(false);
            currentAmt = 0;
        }

        internal void Set(Item item)
        {
            if (item is null)
            {
                HideInfo();
                return;
            }

            ItemBase itemBase = ItemBase.Get(item.ItemId);
            if (itemBase is null)
            {
                HideInfo();
                return;
            }

            displayer.Set(itemBase, item);
            Draw(item, itemBase);
        }

        private void HideInfo()
        {
            displayer.IconVisible(false);
            displayer.TextBottomVisible(false);
            displayer.TextTopVisible(false);
            currentItemId = Guid.Empty;
        }

        private void Draw(Item item, ItemBase itemBase)
        {
            displayer.TextBottomVisible(itemBase.IsStackable);
            if (itemBase.IsStackable)
            {
                if (currentAmt != item.Quantity)
                {
                    currentAmt = item.Quantity;
                    displayer.SetTextBottom(Strings.FormatQuantityAbbreviated(item.Quantity));
                }
            }

            if (item.ItemId != currentItemId
                || texLoaded != itemBase.Icon)
            {
                currentItemId = item.ItemId;
                GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, itemBase.Icon);
                if (itemTex != null)
                {
                    displayer.IconSprite(itemTex.GetSpriteDefault());
                    displayer.IconVisible(true);
                }
                else
                {
                    displayer.IconVisible(false);
                }

                texLoaded = itemBase.Icon;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Guid.Empty.Equals(currentItemId))
            {
                return;
            }

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                { }
                break;
                case PointerEventData.InputButton.Right:
                {
                    if (Globals.InBag)
                    {
                        Globals.Me.TryRetreiveBagItem(Index, -1);
                    }
                }
                break;
                case PointerEventData.InputButton.Middle:
                { }
                break;
            }
        }

        public override void Drop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            BagItem bagItem = eventData.pointerDrag.GetComponent<BagItem>();
            if (bagItem != null)
            {
                //Try to swap....
                Globals.Me.SwapBagItems(bagItem.Index, Index);
                return;
            }

            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                //Try to swap....
                Globals.Me.TryStoreBagItem(inventoryItem.Index, Index);
                return;
            }
        }
    }
}