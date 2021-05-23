using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using Intersect.GameObjects.Crafting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect.Client.Interface.Game.Crafting
{
    public class RecipeDetailsItem : MonoBehaviour
    {
        [SerializeField]
        private ItemSpellDisplayer displayer;
        [SerializeField]
        private IngredientItem ingredientItemPrefab;
        [SerializeField]
        private Transform itemContainer;
        [SerializeField]
        private Vector2 descOffset;
        [SerializeField]
        private Vector2 descPivot = new Vector2(1f, 1f);

        private CraftBase craftBase;

        private Guid currentItemId;
        private string texLoaded = string.Empty;
        private int currentAmt;
        private readonly List<IngredientItem> items = new List<IngredientItem>();

        private Transform descTransform;

        internal void Setup(Transform descTransformDisplay)
        {
            currentAmt = 0;
            descTransform = descTransformDisplay;
            displayer.TextTopVisible(false);
            displayer.TextCoolDownVisible(false);
            displayer.TextBottomVisible(true);
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        internal void Set(CraftBase craftBase)
        {
            this.craftBase = craftBase;
            ItemBase itemBase = ItemBase.Get(craftBase.ItemId);
            displayer.Set(itemBase);

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

            if (craftBase.Ingredients != null)
            {
                //Quickly Look through the inventory and create a catalog of what items we have, and how many
                Dictionary<Guid, int> itemdict = new Dictionary<Guid, int>();
                foreach (Items.Item item in Globals.Me.Inventory)
                {
                    if (item != null)
                    {
                        if (itemdict.ContainsKey(item.ItemId))
                        {
                            itemdict[item.ItemId] += item.Quantity;
                        }
                        else
                        {
                            itemdict.Add(item.ItemId, item.Quantity);
                        }
                    }
                }

                for (int i = 0; i < craftBase.Ingredients.Count; i++)
                {
                    IngredientItem item;
                    if (items.Count - 1 < i)
                    {
                        item = Instantiate(ingredientItemPrefab, itemContainer, false);
                        item.Setup(descTransform);
                        items.Add(item);
                    }
                    else
                    {
                        item = items[i];
                        item.gameObject.SetActive(true);
                    }
                    int onHand = 0;
                    if (itemdict.ContainsKey(craftBase.Ingredients[i].ItemId))
                    {
                        onHand = itemdict[craftBase.Ingredients[i].ItemId];
                    }

                    item.Set(ItemBase.Get(craftBase.Ingredients[i].ItemId), $"{onHand}/{craftBase.Ingredients[i].Quantity}");
                }

                for (int i = craftBase.Ingredients.Count; i < items.Count; i++)
                {
                    items[i].gameObject.SetActive(false);
                }
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


            int quantity = Math.Max(craftBase.Quantity, 1);
            if (currentAmt != quantity)
            {
                if (itemBase == null || !itemBase.IsStackable)
                {
                    quantity = 1;
                }
                currentAmt = quantity;
                displayer.SetTextBottom(Strings.FormatQuantityAbbreviated(quantity));
            }

        }
    }
}
