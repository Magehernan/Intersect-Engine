using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Bank
{

    public class BankWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private BankItem bankItemPrefab;
        [SerializeField]
        private Transform itemContainer;
        [SerializeField]
        private Transform descTransform;


        private readonly List<BankItem> items = new List<BankItem>();

        private void Start()
        {
            buttonClose.onClick.AddListener(Interface.GameUi.NotifyCloseBank);
        }

        public override void Show(object obj = null)
        {
            textTitle.text = Globals.GuildBank ? Strings.Guilds.Bank.ToString(Globals.Me?.Guild) : Strings.Bank.title.ToString();

            InitItemContainer();
            base.Show(obj);
        }

        internal void Draw()
        {
            if (IsHidden)
            {
                return;
            }

            for (int i = 0; i < Globals.BankSlots; i++)
            {
                items[i].Set(Globals.Bank[i]);
            }
        }

        private void InitItemContainer()
        {
            int slots = Math.Max(items.Count, Globals.BankSlots);

            for (int i = 0; i < slots; i++)
            {
                if (items.Count < Globals.BankSlots)
                {
                    BankItem item = Instantiate(bankItemPrefab, itemContainer, false);
                    item.Setup(i, descTransform);
                    items.Add(item);
                    continue;
                }

                items[i].gameObject.SetActive(i < Globals.BankSlots);
            }
        }
    }
}
