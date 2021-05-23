using Intersect.Client.General;
using Intersect.Client.UnityGame;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect.Client.Interface.Game.Hotbar
{

    public class HotBarWindow : Window
    {
        [SerializeField]
        private HotbarItem hotbarItemPrefab;

        public List<HotbarItem> Items { get; private set; } = new List<HotbarItem>(Options.MaxHotbar);

        public void Setup()
        {
            for (int i = 0; i < Options.MaxHotbar; i++)
            {
                HotbarItem item = Instantiate(hotbarItemPrefab, MyRectTransform, false);
                Items.Add(item);
                item.Setup(i, null);
            }
        }

        internal void Draw()
        {
            if (Globals.Me == null)
            {
                return;
            }

            Show();
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Draw();
            }
        }
    }

}
