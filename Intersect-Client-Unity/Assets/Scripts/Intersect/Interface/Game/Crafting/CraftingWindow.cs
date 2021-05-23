using Intersect.Client.General;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UI.Components;
using Intersect.Client.UnityGame;
using Intersect.GameObjects.Crafting;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Crafting
{
    public class CraftingWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI labelTitle;
        [SerializeField]
        private TextMeshProUGUI labelProduct;
        [SerializeField]
        private TextMeshProUGUI labelIngredients;
        [SerializeField]
        private TextMeshProUGUI labelRecipes;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private Button buttonCraft;
        [SerializeField]
        private TextMeshProUGUI labelCraft;
        [SerializeField]
        private RecipeItem recipeItemPrefab;
        [SerializeField]
        private Transform itemContainer;
        [SerializeField]
        private Transform descTransform;
        [SerializeField]
        private RecipeDetailsItem recipeDetails;
        [SerializeField]
        private FillBar fillBar;


        private readonly List<RecipeItem> items = new List<RecipeItem>();
        private bool reloadItems = true;
        private bool initializated = false;
        private bool crafting;
        private CraftBase currentCraft;
        private long mBarTimer;

        private void Start()
        {
            buttonClose.onClick.AddListener(Interface.GameUi.NotifyCloseCraftingTable);
            buttonCraft.onClick.AddListener(OnClickCraft);
            labelProduct.text = Strings.Crafting.product;
            labelIngredients.text = Strings.Crafting.ingredients;
            labelRecipes.text = Strings.Crafting.recipes;
            labelCraft.text = Strings.Crafting.craft;
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            labelTitle.text = Globals.ActiveCraftingTable.Name;
            reloadItems = true;
            currentCraft = null;
            fillBar.Hide();
        }

        internal void Draw()
        {
            if (IsHidden)
            {
                return;
            }

            if (!initializated)
            {
                initializated = true;
                recipeDetails.Setup(descTransform);
                Globals.Me.InventoryUpdatedDelegate = () =>
                {
                    if (IsVisible)
                    {
                        //Refresh crafting window items
                        LoadCraftItems(currentCraft);
                    }
                };
            }

            if (reloadItems)
            {
                reloadItems = false;
                InitItemContainer();
            }

            if (!crafting)
            {
                fillBar.Hide();
                return;
            }

            long delta = Globals.System.GetTimeMs() - mBarTimer;
            if (delta > currentCraft.Time)
            {
                delta = currentCraft.Time;
                crafting = false;
                IsClosable = true;
                fillBar.Hide();
                //LoadCraftItems(mCraftId);
                return;
            }

            fillBar.Show();
            float ratio = currentCraft.Time == 0 ? 0 : (float)delta / currentCraft.Time;
            fillBar.ChangeValue(ratio);

        }

        private void InitItemContainer()
        {
            for (int i = 0; i < Globals.ActiveCraftingTable.Crafts.Count; i++)
            {
                RecipeItem item;
                if (items.Count - 1 < i)
                {
                    item = Instantiate(recipeItemPrefab, itemContainer, false);
                    items.Add(item);
                }
                else
                {
                    item = items[i];
                    item.gameObject.SetActive(true);
                }
                item.Set(i, CraftBase.Get(Globals.ActiveCraftingTable.Crafts[i]), OnCraftSelected);
            }

            for (int i = Globals.ActiveCraftingTable.Crafts.Count; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(false);
            }

            if (Globals.ActiveCraftingTable?.Crafts?.Count > 0)
            {
                LoadCraftItems(CraftBase.Get(Globals.ActiveCraftingTable.Crafts[0]));
            }
        }

        private void OnCraftSelected(CraftBase craftBase)
        {
            if (!crafting)
            {
                LoadCraftItems(craftBase);
            }
        }

        private void LoadCraftItems(CraftBase craftBase)
        {
            currentCraft = craftBase;

            if (!Globals.ActiveCraftingTable.Crafts.Contains(currentCraft.Id))
            {
                return;
            }

            recipeDetails.Set(currentCraft);
        }

        private void OnClickCraft()
        {
            if (crafting)
            {
                return;
            }

            //This shouldn't be client side :(
            //Quickly Look through the inventory and create a catalog of what items we have, and how many
            Dictionary<Guid, int> availableItemQuantities = new Dictionary<Guid, int>();
            foreach (Items.Item item in Globals.Me.Inventory)
            {
                if (item != null)
                {
                    if (availableItemQuantities.ContainsKey(item.ItemId))
                    {
                        availableItemQuantities[item.ItemId] += item.Quantity;
                    }
                    else
                    {
                        availableItemQuantities.Add(item.ItemId, item.Quantity);
                    }
                }
            }


            bool canCraft = currentCraft?.Ingredients != null;

            if (canCraft)
            {
                foreach (CraftIngredient ingredient in currentCraft.Ingredients)
                {
                    if (!availableItemQuantities.TryGetValue(ingredient.ItemId, out int availableQuantity))
                    {
                        canCraft = false;

                        break;
                    }

                    if (availableQuantity < ingredient.Quantity)
                    {
                        canCraft = false;

                        break;
                    }

                    availableItemQuantities[ingredient.ItemId] -= ingredient.Quantity;
                }
            }

            if (!canCraft)
            {
                ChatboxMsg.AddMessage(new ChatboxMsg(Strings.Crafting.incorrectresources, Color.Red, Enums.ChatMessageType.Crafting));
                return;
            }

            fillBar.ChangeValue(0, true);
            crafting = true;
            mBarTimer = Globals.System.GetTimeMs();
            PacketSender.SendCraftItem(currentCraft.Id);
            IsClosable = false;
        }
    }
}
