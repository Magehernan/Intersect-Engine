using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
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
        private bool mInitializedItems = false;

        private void Start()
        {
            textTitle.text = Strings.Bank.title;
            buttonClose.onClick.AddListener(Interface.GameUi.NotifyCloseBank);
        }

        internal void Draw()
        {
            if (!mInitializedItems)
            {
                mInitializedItems = true;
                InitItemContainer();
            }

            if (IsHidden)
            {
                return;
            }

            for (int i = 0; i < Options.MaxBankSlots; i++)
            {
                items[i].Set(Globals.Bank[i]);
            }
        }

        private void InitItemContainer()
        {
            for (int i = 0; i < Options.MaxBankSlots; i++)
            {
                BankItem item = Instantiate(bankItemPrefab, itemContainer, false);
                item.Setup(i, descTransform);
                items.Add(item);
            }
        }
    }
}
