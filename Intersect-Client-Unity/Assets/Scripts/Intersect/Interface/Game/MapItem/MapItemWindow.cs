using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Maps;
using Intersect.Client.UnityGame;
using Intersect.GameObjects;
using Intersect.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Inventory
{
    public class MapItemWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Transform itemContainer;
        [SerializeField]
        private MapItemIcon mapItemIconPrefab;
        [SerializeField]
        private Button buttonLootAll;
        [SerializeField]
        private TextMeshProUGUI textLootAll;
        [SerializeField]
        private Transform descTransform;

        //Item List
        private readonly List<MapItemIcon> items = new List<MapItemIcon>();

        private bool mFoundItems;
        private const int mScanTimer = 250;   // How often do we scan for items in Milliseconds?
        private long mLastItemScan;

        private bool initialized = false;

        private void Start()
        {
            buttonLootAll.onClick.AddListener(OnClickLootAll);
            textTitle.text = Strings.MapItemWindow.Title;
            textLootAll.text = Strings.MapItemWindow.LootButton;
        }

        private void CreateItemContainer()
        {
            for (int i = 0; i < Options.Loot.MaximumLootWindowItems; i++)
            {
                MapItemIcon mapItemIcon = Instantiate(mapItemIconPrefab, itemContainer, false);
                items.Add(mapItemIcon);

                mapItemIcon.Setup(i, descTransform);
            }
        }

        internal void Draw()
        {
            // Is this disabled from the server config? If so, skip doing anything.
            if (!Options.Loot.EnableLootWindow)
            {
                Hide();
                return;
            }

            if (!initialized)
            {
                initialized = true;
                CreateItemContainer();
            }

            // Are we allowed to scan for items?
            if (mLastItemScan < Timing.Global.Milliseconds)
            {
                // Reset if we've found items
                mFoundItems = false;

                // Find all valid locations near our location and iterate through them to find items we can display.
                int itemSlot = 0;
                foreach (KeyValuePair<MapInstance, List<int>> map in FindSurroundingTiles(Globals.Me.X, Globals.Me.Y, Options.Loot.MaximumLootWindowDistance))
                {
                    Dictionary<int, List<Items.MapItemInstance>> mapItems = map.Key.MapItems;
                    List<int> tiles = map.Value;

                    // iterate through all locations on this map to see if we've got items there.
                    foreach (int tileIndex in tiles)
                    {
                        // When no items have ever been on this location, just skip straight away.
                        if (!mapItems.ContainsKey(tileIndex))
                        {
                            continue;
                        }

                        // Go through each item up to our display limit and add them to our window.
                        foreach (Items.MapItemInstance mapItem in mapItems[tileIndex])
                        {
                            // Skip rendering this item if we're already past the cap we are allowed to display.
                            if (itemSlot > Options.Loot.MaximumLootWindowItems - 1)
                            {
                                continue;
                            }

                            ItemBase finalItem = mapItem.Base;
                            if (finalItem != null)
                            {
                                items[itemSlot].Set(mapItem, map.Key.Id, tileIndex);
                                mFoundItems = true;
                                itemSlot++;
                            }
                            else
                            {
                                items[itemSlot].SetEmpty();
                            }
                        }
                    }
                }

                // Update our UI and hide our unused icons.
                for (int slot = itemSlot; slot < Options.Loot.MaximumLootWindowItems; slot++)
                {
                    items[slot].SetEmpty();
                }

                // Set up our timer
                mLastItemScan = Timing.Global.Milliseconds + mScanTimer;
            }

            // Do we display our window?
            if (mFoundItems)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnClickLootAll()
        {
            if (Globals.Me.MapInstance == null)
            {
                return;
            }

            // Try and pick up everything on our location.
            Globals.Me.TryPickupItem(Globals.Me.MapInstance.Id, Globals.Me.Y * Options.MapWidth + Globals.Me.X);
        }

        private Dictionary<MapInstance, List<int>> FindSurroundingTiles(int myX, int myY, int distance)
        {
            // Loop through all locations surrounding us to get valid tiles.
            Dictionary<MapInstance, List<int>> locations = new Dictionary<MapInstance, List<int>>();
            for (int x = 0 - distance; x <= distance; x++)
            {
                for (int y = 0 - distance; y <= distance; y++)
                {
                    // Use these to keep track of our translation.
                    MapInstance currentMap = Globals.Me.MapInstance;
                    int currentX = myX + x;
                    int currentY = myY + y;

                    // Are we on a valid map at all?
                    if (currentMap == null)
                    {
                        break;
                    }

                    // Are we going to the map on our left?
                    if (currentX < 0)
                    {
                        MapInstance oldMap = currentMap;
                        if (currentMap.Left != Guid.Empty)
                        {
                            currentMap = MapInstance.Get(currentMap.Left);
                            if (currentMap == null)
                            {
                                currentMap = oldMap;
                                continue;
                            }

                            currentX = (Options.MapWidth + 1) + x;
                        }
                    }

                    // Are we going to the map on our right?
                    if (currentX >= Options.MapWidth)
                    {
                        MapInstance oldMap = currentMap;
                        if (currentMap.Right != Guid.Empty)
                        {
                            currentMap = MapInstance.Get(currentMap.Right);
                            if (currentMap == null)
                            {
                                currentMap = oldMap;
                                continue;
                            }

                            currentX = -1 + x;
                        }
                    }

                    // Are we going to the map up from us?
                    if (currentY < 0)
                    {
                        MapInstance oldMap = currentMap;
                        if (currentMap.Up != Guid.Empty)
                        {
                            currentMap = MapInstance.Get(currentMap.Up);
                            if (currentMap == null)
                            {
                                currentMap = oldMap;
                                continue;
                            }

                            currentY = (Options.MapHeight + 1) + y;
                        }
                    }

                    // Are we going to the map down from us?
                    if (currentY >= Options.MapHeight)
                    {
                        MapInstance oldMap = currentMap;
                        if (currentMap.Down != Guid.Empty)
                        {
                            currentMap = MapInstance.Get(currentMap.Down);
                            if (currentMap == null)
                            {
                                currentMap = oldMap;
                                continue;
                            }

                            currentY = -1 + y;
                        }
                    }

                    if (!locations.ContainsKey(currentMap))
                    {
                        locations.Add(currentMap, new List<int>());
                    }
                    locations[currentMap].Add(currentY * Options.MapWidth + currentX);
                }
            }

            return locations;
        }

    }
}
