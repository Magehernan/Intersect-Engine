using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using System;
using UnityEngine;

namespace Intersect.Client.Interface.Game.Crafting
{
    public class IngredientItem : MonoBehaviour
    {
        [SerializeField]
        private ItemSpellDisplayer displayer;
        [SerializeField]
        private Vector2 descOffset;
        [SerializeField]
        private Vector2 descPivot = new Vector2(1f, 1f);


        private Guid currentItemId;
        private string texLoaded = string.Empty;

        internal void Setup(Transform descTransformDisplay)
        {
            displayer.TextTopVisible(false);
            displayer.TextCoolDownVisible(false);
            displayer.TextBottomVisible(true);
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        internal void Set(ItemBase itemBase, string quantity)
        {
            displayer.Set(itemBase);
            if (itemBase != null)
            {
                Draw(itemBase, quantity);
            }
            else
            {
                displayer.IconVisible(false);
                displayer.TextBottomVisible(false);
                displayer.TextTopVisible(false);
                currentItemId = Guid.Empty;
            }
        }

        private void Draw(ItemBase itemBase, string quantity)
        {
            displayer.SetTextBottom(quantity);

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
    }
}
