using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Shop
{
    public class ShopItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private ItemSpellDisplayer displayer;
        [SerializeField]
        private Vector2 descOffset;
        [SerializeField]
        private Vector2 descPivot = new Vector2(1f, 1f);


        //Slot info
        private int mySlot;
        private Guid currentItemId;
        private string texLoaded = string.Empty;
        public bool isDragging;


        internal void Setup(int slot, Transform descTransformDisplay)
        {
            mySlot = slot;
            displayer.TextTopVisible(false);
            displayer.TextCoolDownVisible(false);
            displayer.TextBottomVisible(false);
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        internal void Set(GameObjects.ShopItem item)
        {
            ItemBase itemBase = ItemBase.Get(item.ItemId);
            displayer.Set(itemBase);
            displayer.SetValue(Strings.Shop.costs.ToString(item.CostItemQuantity, itemBase.Name));
            if (itemBase != null)
            {
                Draw(itemBase);
            }
            else
            {
                displayer.IconVisible(false);
                displayer.TextBottomVisible(false);
                displayer.TextTopVisible(false);
                currentItemId = Guid.Empty;
            }
        }

        private void Draw(ItemBase itemBase)
        {
            if (itemBase.Id != currentItemId
                || texLoaded != itemBase.Icon)
            {
                currentItemId = itemBase.Id;
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
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                { }
                break;
                case PointerEventData.InputButton.Right:
                {
                    ItemBase item = ItemBase.Get(currentItemId);
                    if (item != null)
                    {
                        if (item.IsStackable)
                        {
                            Interface.InputBox.Show(
                                Strings.Shop.buyitem, Strings.Shop.buyitemprompt.ToString(item.Name), true,
                                InputBox.InputType.NumericInput, BuyItemInputBoxOkay, null, mySlot
                            );
                        }
                        else
                        {
                            PacketSender.SendBuyItem(mySlot, 1);
                        }
                    }
                }
                break;
                case PointerEventData.InputButton.Middle:
                { }
                break;
            }
        }

        private void BuyItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendBuyItem(mySlot, value);
            }
        }
    }
}
