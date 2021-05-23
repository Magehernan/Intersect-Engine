using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Spells
{
    public class SpellsWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private SpellItem spellItemPrefab;
        [SerializeField]
        private Transform spellContainer;
        [SerializeField]
        private Transform descTransform;

        private readonly List<SpellItem> items = new List<SpellItem>();
        private bool mInitializedSpells = false;

        private void Start()
        {
            buttonClose.onClick.AddListener(() => Hide());
            textTitle.text = Strings.Spells.title;
        }

        internal void Draw()
        {
            if (!mInitializedSpells)
            {
                InitItemContainer();
                mInitializedSpells = true;
            }

            if (!IsVisible)
            {
                return;
            }

            for (int i = 0; i < Options.MaxPlayerSkills; i++)
            {
                items[i].Set(Globals.Me.Spells[i]);
            }
        }

        private void InitItemContainer()
        {
            for (int i = 0; i < Options.MaxPlayerSkills; i++)
            {
                SpellItem spellItem = Instantiate(spellItemPrefab, spellContainer, false);
                spellItem.Setup(i, descTransform);
                items.Add(spellItem);
            }
        }
    }
}
