using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using Intersect.Enums;
using Intersect.GameObjects;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Character
{
    public class CharacterWindow : Window
    {
        [Serializable]
        public class SlotEquipment
        {
            public string name;
            public EquipmentItem displayer;
        }
        [Serializable]
        public class SlotStat
        {
            public Stats stat;
            public StatDisplayer displayer;
        }

        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private TextMeshProUGUI textDescription;
        [SerializeField]
        private EntityDisplayer entityDisplayer;
        [SerializeField]
        private EquipmentItem equipmentItemPrefab;
        [SerializeField]
        private TextMeshProUGUI textStats;
        [SerializeField]
        private TextMeshProUGUI textPoints;
        [SerializeField]
        private Transform descTransform;
        [SerializeField]
        private List<SlotEquipment> equipments;
        [SerializeField]
        private List<SlotStat> stats;


        private bool initialized = false;

        private readonly Dictionary<string, EquipmentItem> equipmentDisplayers = new Dictionary<string, EquipmentItem>();
        private readonly Dictionary<Stats, StatDisplayer> statDisplayers = new Dictionary<Stats, StatDisplayer>();

        protected override void Awake()
        {
            foreach (SlotEquipment slot in equipments)
            {
                equipmentDisplayers.Add(slot.name, slot.displayer);
                slot.displayer.Setup(Options.EquipmentSlots.IndexOf(slot.name), descTransform);
            }

            //esto hay que definirlo aca ya que sino se pueden tomar los valores originales de los textos en vez de los cargados desde el archivo
            Dictionary<Stats, (string label, string statName)> statDefinitions = new Dictionary<Stats, (string label, string statName)>
            {
                { Stats.Attack, (Strings.Character.stat0, Strings.Combat.stat0) },
                { Stats.Defense, (Strings.Character.stat2, Strings.Combat.stat2) },
                { Stats.Speed, (Strings.Character.stat4, Strings.Combat.stat4) },
                { Stats.AbilityPower, (Strings.Character.stat1, Strings.Combat.stat1) },
                { Stats.MagicResist, (Strings.Character.stat3, Strings.Combat.stat3) },
            };

            foreach (SlotStat slot in stats)
            {
                statDisplayers.Add(slot.stat, slot.displayer);
                (string label, string statName) = statDefinitions[slot.stat];
                slot.displayer.Setup(label, statName, slot.stat);
            }

            buttonClose.onClick.AddListener(() => Hide());
            textTitle.text = Strings.Character.title;
            textStats.text = Strings.Character.stats;
        }

        internal void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            if (!initialized)
            {
                initialized = true;
                entityDisplayer.Set(Globals.Me);
            }


            textName.text = Globals.Me.Name;
            textDescription.text = Strings.Character.levelandclass.ToString(Globals.Me.Level, ClassBase.GetName(Globals.Me.Class));
            textPoints.text = Strings.Character.points.ToString(Globals.Me.StatPoints);
            entityDisplayer.UpdateImage();



            for (int i = 0; i < Options.EquipmentSlots.Count; i++)
            {
                if (!equipmentDisplayers.TryGetValue(Options.EquipmentSlots[i], out EquipmentItem equipmentItem))
                {
                    continue;
                }

                if (Globals.Me.MyEquipment[i] > -1 && Globals.Me.MyEquipment[i] < Options.MaxInvItems && Globals.Me.Inventory[Globals.Me.MyEquipment[i]].ItemId != Guid.Empty)
                {
                    equipmentItem.Set(Globals.Me.Inventory[Globals.Me.MyEquipment[i]]);
                }
                else
                {
                    equipmentItem.Set(null);
                }
            }

            foreach (StatDisplayer statDisplayer in statDisplayers.Values)
            {
                statDisplayer.UpdateValue(Globals.Me.Stat, Globals.Me.StatPoints);
            }
        }
    }

}
