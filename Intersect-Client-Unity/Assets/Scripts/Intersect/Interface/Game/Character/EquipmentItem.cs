using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Items;
using Intersect.Client.Networking;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Character
{
    public class EquipmentItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private ItemSpellDisplayer displayer;
        [SerializeField]
        private Vector2 descOffset;

        private Vector2 descPivot = new Vector2(1f, 1f);
        private int index;
        private Guid currentItemId;

        internal void Setup(int i, Transform descTransformDisplay)
        {
            index = i;
            displayer.TextTopVisible(false);
            displayer.TextBottomVisible(false);
            displayer.TextCoolDownVisible(false);
            displayer.IconVisible(false);
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        public void Set(Item item)
        {
            if (item?.ItemId == currentItemId)
            {
                return;
            }

            currentItemId = item?.ItemId ?? Guid.Empty;

            ItemBase itembBase = ItemBase.Get(currentItemId);
            displayer.Set(itembBase, item);
            if (itembBase == null)
            {
                displayer.IconVisible(false);
                return;
            }

            GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, itembBase.Icon);
            if (itemTex == null)
            {
                displayer.IconVisible(false);
                return;
            }

            displayer.IconSprite(itemTex.GetSpriteDefault());
            displayer.IconVisible(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                {
                }
                break;
                case PointerEventData.InputButton.Right:
                {
                    PacketSender.SendUnequipItem(index);
                }
                break;
                case PointerEventData.InputButton.Middle:
                {
                }
                break;
            }
        }
    }
}
