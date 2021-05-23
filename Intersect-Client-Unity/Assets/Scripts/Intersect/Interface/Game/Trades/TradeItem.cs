using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Trades
{
    public class TradeItem : MonoBehaviour, IPointerClickHandler
    {

        [SerializeField]
        private ItemSpellDisplayer displayer;
        [SerializeField]
        private Vector2 descOffset;
        [SerializeField]
        private Vector2 descPivot = new Vector2(1f, 1f);


        //Slot info
        private int mySlot;
        private int mySide;
        private int currentAmt;
        private Guid currentItemId;
        private string texLoaded = string.Empty;
        public bool isDragging;

        internal void Setup(int slot, int side, Transform descTransformDisplay)
        {
            mySlot = slot;
            mySide = side;
            displayer.TextTopVisible(false);
            displayer.TextCoolDownVisible(false);
            currentAmt = 0;
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        internal void Set(Item item)
        {
            ItemBase itemBase = ItemBase.Get(item.ItemId);
            displayer.Set(itemBase, item);
            if (itemBase != null)
            {
                Draw(item, itemBase);
            }
            else
            {
                displayer.IconVisible(false);
                displayer.TextBottomVisible(false);
                displayer.TextTopVisible(false);
                currentItemId = Guid.Empty;
            }
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
            if (mySide != 0)
            {
                return;
            }

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
                    if (Globals.InTrade)
                    {
                        Globals.Me.TryRevokeItem(mySlot);
                    }
                }
                break;
                case PointerEventData.InputButton.Middle:
                { }
                break;
            }
        }
    }
}
