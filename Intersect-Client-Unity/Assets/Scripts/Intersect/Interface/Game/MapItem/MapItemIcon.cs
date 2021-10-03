using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Inventory
{
    public class MapItemIcon : UIBaseItem, IPointerClickHandler
    {
        private MapItemInstance MyItem;
        private Guid MapId;
        private int TileIndex;

        private Guid currentItemId;
        private int mCurrentAmt;
        private string mTexLoaded = string.Empty;

        public override void Setup(int index, Transform descTransformDisplay)
        {
            base.Setup(index, descTransformDisplay);
            isDraggable = false;
        }

        internal void SetEmpty()
        {
            MyItem = null;
            MapId = Guid.Empty;
            TileIndex = -1;
            displayer.IconVisible(false);
            displayer.TextBottomVisible(false);
            displayer.TextTopVisible(false);
            currentItemId = Guid.Empty;
        }

        internal void Set(MapItemInstance item, Guid mapId, int tileIndex)
        {
            MyItem = item;
            MapId = mapId;
            TileIndex = tileIndex;

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
                if (mCurrentAmt != item.Quantity)
                {
                    mCurrentAmt = item.Quantity;
                    displayer.SetTextBottom(Strings.FormatQuantityAbbreviated(item.Quantity));
                }
            }

            if (item.ItemId != currentItemId
                || mTexLoaded != itemBase.Icon)
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

                mTexLoaded = itemBase.Icon;
            }
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (MyItem == null || TileIndex < 0 || TileIndex >= Options.MapWidth * Options.MapHeight)
            {
                return;
            }

            Globals.Me.TryPickupItem(MapId, TileIndex, MyItem.UniqueId);
        }

        public override void Drop(PointerEventData eventData)
        {
        }
    }
}
