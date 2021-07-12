using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Bag;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Inventory
{
    public class InventoryItem : UIBaseItem, IPointerClickHandler
    {
        private bool mIconCd;
        private int mCurrentAmt;
        private Guid currentItemId;
        private bool mIsEquipped;
        private string mTexLoaded = string.Empty;

        public override void Setup(int index, Transform descTransformDisplay)
        {
            base.Setup(index, descTransformDisplay);
            displayer.TextTopVisible(false);
            displayer.SetTextTop(Strings.Inventory.equippedicon);
            displayer.TextCoolDownVisible(false);
            mCurrentAmt = 0;
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
                if (mCurrentAmt != item.Quantity)
                {
                    mCurrentAmt = item.Quantity;
                    displayer.SetTextBottom(Strings.FormatQuantityAbbreviated(item.Quantity));
                }
            }

            bool equipped = false;
            for (int i = 0; i < Options.EquipmentSlots.Count; i++)
            {
                if (Globals.Me.MyEquipment[i] == Index)
                {
                    equipped = true;

                    break;
                }
            }

            if (equipped != mIsEquipped)
            {
                mIsEquipped = equipped;
                displayer.TextTopVisible(mIsEquipped);
            }


            bool itemCd = Globals.Me.ItemOnCd(Index);
            if (item.ItemId != currentItemId
                || mTexLoaded != itemBase.Icon
                || mIconCd != itemCd)
            {
                currentItemId = item.ItemId;

                mIconCd = itemCd;
                GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, itemBase.Icon);
                if (itemTex != null)
                {
                    displayer.IconSprite(itemTex.GetSpriteDefault());
                    displayer.IconVisible(true);
                    if (itemCd)
                    {
                        displayer.IconColor(new Color32(255, 255, 255, 100));
                    }
                    else
                    {
                        displayer.IconColor(UnityEngine.Color.white);
                    }
                }
                else
                {
                    displayer.IconVisible(false);
                }

                mTexLoaded = itemBase.Icon;
            }

            displayer.TextCoolDownVisible(mIconCd);
            if (mIconCd)
            {
                float secondsRemaining = Globals.Me.ItemCdRemainder(Index) / 1000f;
                if (secondsRemaining > 10f)
                {
                    displayer.SetTextCoolDown(Strings.Inventory.cooldown.ToString(secondsRemaining.ToString("N0")));
                }
                else
                {
                    displayer.SetTextCoolDown(Strings.Inventory.cooldown.ToString(secondsRemaining.ToString("N1").Replace(".", Strings.Numbers.dec)));
                }
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
                {
                    Globals.Me.TryUseItem(Index);
                }
                break;
                case PointerEventData.InputButton.Right:
                {
                    if (Globals.InShop)
                    {
                        Globals.Me.TrySellItem(Index);
                    }
                    else if (Globals.InBank)
                    {
                        Globals.Me.TryDepositItem(Index);
                    }
                    else if (Globals.InBag)
                    {
                        Globals.Me.TryStoreBagItem(Index, -1);
                    }
                    else if (Globals.InTrade)
                    {
                        Globals.Me.TryTradeItem(Index);
                    }
                    else
                    {
                        Globals.Me.TryDropItem(Index);
                    }
                }
                break;
                case PointerEventData.InputButton.Middle:
                {
                }
                break;
            }
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                //Try to swap....
                Globals.Me.SwapInventoryItems(inventoryItem.Index, Index);
                return;
            }

            BagItem bagItem = eventData.pointerDrag.GetComponent<BagItem>();
            if (bagItem != null)
            {
                //Try to swap....
                Globals.Me.TryRetreiveBagItem(bagItem.Index, Index);
                return;
            }
        }
    }
}
