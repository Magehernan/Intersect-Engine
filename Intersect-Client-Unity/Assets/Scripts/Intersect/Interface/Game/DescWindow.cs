using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using Intersect.Enums;
using Intersect.GameObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Intersect.Client.Utils;
using Intersect.Client.Items;
using System;

namespace Intersect.Client.Interface.Game
{

    public class DescWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textQuantity;
        [SerializeField]
        private TextMeshProUGUI textValue;
        [SerializeField]
        private Image imageIcon;
        [SerializeField]
        private TextMeshProUGUI textType;
        [SerializeField]
        private TextMeshProUGUI textDesc;
        [SerializeField]
        private TextMeshProUGUI textStats;
        [SerializeField]
        private int widthNomal;
        [SerializeField]
        private int widthExtended;


        internal void SetItem(ItemBase itemBase, Item item, Vector2 position, Vector2 pivot, string title = "", string valueLabel = "")
        {
            if (itemBase is null)
            {
                Hide();
                return;
            }
            MyRectTransform.pivot = pivot;

            if (string.IsNullOrWhiteSpace(title))
            {
                title = itemBase.Name;
            }

            textTitle.text = title;
            textValue.text = valueLabel;

            if (itemBase.IsStackable && item != null)
            {
                textQuantity.text = item.Quantity.ToString("N0").Replace(",", Strings.Numbers.comma);
            }
            else
            {
                textQuantity.text = string.Empty;
            }

            GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, itemBase.Icon);
            if (itemTex != null)
            {
                imageIcon.sprite = itemTex.GetSpriteDefault();
            }

            textType.text = Strings.ItemDesc.itemtypes[(int)itemBase.ItemType];

            if (itemBase.ItemType == ItemTypes.Equipment
                && itemBase.EquipmentSlot >= 0
                && itemBase.EquipmentSlot < Options.EquipmentSlots.Count)
            {
                textType.text = Options.EquipmentSlots[itemBase.EquipmentSlot];
                if (itemBase.EquipmentSlot == Options.WeaponIndex && itemBase.TwoHanded)
                {
                    textType.text = $"{textType.text} - {Strings.ItemDesc.twohand}";
                }
            }

            if (itemBase.Rarity > 0)
            {
                textType.text = $"{textType.text} - {Strings.ItemDesc.rarity[itemBase.Rarity]}";
                Color rarity = CustomColors.Items.Rarities.ContainsKey(itemBase.Rarity) ? CustomColors.Items.Rarities[itemBase.Rarity] : Color.White;
                textType.color = rarity.ToColor32();
            }


            string desc = string.Empty;
            if (itemBase.Description.Length > 0)
            {
                desc = Strings.ItemDesc.desc.ToString(itemBase.Description);
            }

            textDesc.text = desc;

            string stats = string.Empty;
            if (itemBase.ItemType == ItemTypes.Equipment)
            {
                stats = $"{Strings.ItemDesc.bonuses}\n";

                if (itemBase.ItemType == ItemTypes.Equipment && itemBase.EquipmentSlot == Options.WeaponIndex)
                {
                    stats = $"{stats}{Strings.ItemDesc.damage.ToString(itemBase.Damage)}\n";
                }

                for (int i = 0; i < (int)Vitals.VitalCount; i++)
                {
                    string bonus = itemBase.VitalsGiven[i].ToString();
                    if (itemBase.PercentageVitalsGiven[i] > 0)
                    {
                        if (itemBase.VitalsGiven[i] > 0)
                        {
                            bonus += " + ";
                        }
                        else
                        {
                            bonus = string.Empty;
                        }

                        bonus += itemBase.PercentageVitalsGiven[i] + "%";
                    }

                    string vitals = Strings.ItemDesc.vitals[i].ToString(bonus);
                    stats = $"{stats}{vitals}\n";
                }

                if (item?.StatBuffs != null)
                {
                    int[] statBuffs = item.StatBuffs;
                    for (int i = 0; i < (int)Stats.StatCount; i++)
                    {
                        int flatStat = itemBase.StatsGiven[i] + statBuffs[i];
                        string bonus = flatStat.ToString();

                        if (itemBase.PercentageStatsGiven[i] > 0)
                        {
                            if (flatStat > 0)
                            {
                                bonus += " + ";
                            }
                            else
                            {
                                bonus = string.Empty;
                            }

                            bonus += $"{itemBase.PercentageStatsGiven[i]}%";
                        }

                        stats = $"{stats}{Strings.ItemDesc.stats[i].ToString(bonus)}\n";
                    }
                }
            }

            if (itemBase.ItemType == ItemTypes.Equipment &&
                itemBase.Effect.Type != EffectType.None &&
                itemBase.Effect.Percentage > 0)
            {
                stats = $"{stats}{Strings.ItemDesc.effect.ToString(itemBase.Effect.Percentage, Strings.ItemDesc.effects[(int)itemBase.Effect.Type - 1])}";
            }
            textStats.text = stats;

            Vector2 sizeDelta = MyRectTransform.sizeDelta;
            if (string.IsNullOrWhiteSpace(stats))
            {
                sizeDelta.x = widthNomal;
            }
            else
            {
                sizeDelta.x = widthExtended;
            }
            SetPosition(position, sizeDelta);
        }

        internal void SetSpell(Guid spellId, Vector2 position, Vector2 pivot)
        {
            SpellBase spell = SpellBase.Get(spellId);
            if (spell is null)
            {
                Hide();
                return;
            }
            MyRectTransform.pivot = pivot;


            textTitle.text = spell.Name;
            textQuantity.text = string.Empty;
            textValue.text = string.Empty;

            GameTexture spellTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Spell, spell.Icon);
            if (spellTex != null)
            {
                imageIcon.sprite = spellTex.GetSpriteDefault();
            }
            textType.text = Strings.SpellDesc.spelltypes[(int)spell.SpellType];
            textType.color = UnityEngine.Color.white;

            string desc = string.Empty;
            if (spell.Description.Length > 0)
            {
                desc = Strings.SpellDesc.desc.ToString(spell.Description);
            }
            textDesc.text = desc;

            if (spell.SpellType == (int)SpellTypes.CombatSpell)
            {
                if (spell.Combat.TargetType == SpellTargetTypes.Projectile)
                {
                    ProjectileBase proj = ProjectileBase.Get(spell.Combat.ProjectileId);
                    textType.text = Strings.SpellDesc.targettypes[(int)spell.Combat.TargetType].ToString(proj?.Range ?? 0, spell.Combat.HitRadius);
                }
                else
                {
                    textType.text = Strings.SpellDesc.targettypes[(int)spell.Combat.TargetType].ToString(spell.Combat.CastRange, spell.Combat.HitRadius);
                }
            }

            string stats = string.Empty;
            if (spell.SpellType == (int)SpellTypes.CombatSpell &&
                (spell.Combat.TargetType == SpellTargetTypes.AoE ||
                 spell.Combat.TargetType == SpellTargetTypes.Single) &&
                spell.Combat.HitRadius > 0)
            {
                stats = $"{Strings.SpellDesc.radius.ToString(spell.Combat.HitRadius)}\n\n";
            }

            if (spell.CastDuration > 0)
            {
                float castDuration = spell.CastDuration / 1000f;
                stats = $"{stats}{Strings.SpellDesc.casttime.ToString(castDuration)}\n";

                if (spell.CooldownDuration <= 0)
                {
                    stats = $"{stats}\n";
                }
            }

            if (spell.CooldownDuration > 0)
            {
                decimal cdr = 1 - Globals.Me.GetCooldownReduction() / 100;
                float cd = (float)(spell.CooldownDuration * cdr) / 1000f;
                stats = $"{stats}{Strings.SpellDesc.cooldowntime.ToString(cd)}\n\n";
            }

            bool requirements = spell.VitalCost[(int)Vitals.Health] > 0 || spell.VitalCost[(int)Vitals.Mana] > 0;

            if (requirements == true)
            {
                stats = $"{stats}{Strings.SpellDesc.prereqs}\n";
                if (spell.VitalCost[(int)Vitals.Health] > 0)
                {
                    stats = $"{stats}{Strings.SpellDesc.vitalcosts[(int)Vitals.Health].ToString(spell.VitalCost[(int)Vitals.Health])}\n";
                }

                if (spell.VitalCost[(int)Vitals.Mana] > 0)
                {
                    stats = $"{stats}{Strings.SpellDesc.vitalcosts[(int)Vitals.Mana].ToString(spell.VitalCost[(int)Vitals.Mana])}\n";
                }

                stats = $"{stats}\n";
            }

            if (spell.SpellType == (int)SpellTypes.CombatSpell)
            {
                stats = $"{stats}{Strings.SpellDesc.effects}\n";
                if (spell.Combat.Effect > 0)
                {
                    stats = $"{stats}{Strings.SpellDesc.effectlist[(int)spell.Combat.Effect]}\n";
                }

                for (int i = 0; i < (int)Vitals.VitalCount; i++)
                {
                    int vitalDiff = spell.Combat.VitalDiff?[i] ?? 0;
                    if (vitalDiff == 0)
                    {
                        continue;
                    }

                    Intersect.Localization.LocalizedString vitalSymbol = vitalDiff < 0 ? Strings.SpellDesc.addsymbol : Strings.SpellDesc.removesymbol;
                    if (spell.Combat.Effect == StatusTypes.Shield)
                    {
                        stats = $"{stats}{Strings.SpellDesc.shield.ToString(Math.Abs(vitalDiff))}\n";
                    }
                    else
                    {
                        stats = $"{stats}{Strings.SpellDesc.vitals[i].ToString(vitalSymbol, Math.Abs(vitalDiff))}\n";
                    }

                }

                if (spell.Combat.Duration > 0)
                {
                    for (int i = 0; i < (int)Stats.StatCount; i++)
                    {
                        if (spell.Combat.StatDiff[i] != 0)
                        {
                            stats = $"{stats}{Strings.SpellDesc.stats[i].ToString((spell.Combat.StatDiff[i] > 0 ? Strings.SpellDesc.addsymbol.ToString() : Strings.SpellDesc.removesymbol.ToString()) + Math.Abs(spell.Combat.StatDiff[i]))}\n";
                        }
                    }

                    float duration = (float)spell.Combat.Duration / 1000f;
                    stats = $"{stats}{Strings.SpellDesc.duration.ToString(duration)}\n";
                }
            }
            textStats.text = stats;

            Vector2 sizeDelta = MyRectTransform.sizeDelta;
            sizeDelta.x = widthExtended;
            SetPosition(position, sizeDelta);
        }

        private void SetPosition(Vector2 position, Vector2 sizeDelta)
        {
            MyRectTransform.sizeDelta = sizeDelta;
            float pivotX = 1f - MyRectTransform.pivot.x;
            if (Screen.width - (position.x + sizeDelta.x * pivotX) < 0)
            {
                position.x = Screen.width - sizeDelta.x * pivotX;
            }

            if ((position.x - sizeDelta.x * MyRectTransform.pivot.x) < 0)
            {
                position.x = sizeDelta.x * MyRectTransform.pivot.x;
            }
            MyRectTransform.position = position;
        }
    }
}
