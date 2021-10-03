using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Spells;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Spells
{

    public class SpellItem : UIBaseItem, IPointerClickHandler
    {
        private Guid mCurrentSpellId;
        private string mTexLoaded = string.Empty;
        private bool mIconCd;

        public override void Setup(int index, Transform descTransformDisplay)
        {
            base.Setup(index, descTransformDisplay);
            displayer.TextTopVisible(false);
            displayer.TextBottomVisible(false);
            displayer.TextCoolDownVisible(false);
        }

        internal void Set(Spell spell)
        {
            if (spell is null)
            {
                HideInfo();
                return;
            }

            SpellBase spellBase = SpellBase.Get(spell.SpellId);
            if (spellBase is null)
            {
                HideInfo();
                return;
            }

            displayer.Set(spellBase: spellBase);
            Draw(spell, spellBase);
        }

        private void HideInfo()
        {
            mCurrentSpellId = Guid.Empty;
            displayer.IconVisible(false);
        }

        private void Draw(Spell spell, SpellBase spellBase)
        {
            if (spell.SpellId != mCurrentSpellId
                || mTexLoaded != spellBase.Icon
                || mIconCd != Globals.Me.GetSpellCooldown(spell.SpellId) > Globals.System.GetTimeMs()
                || Globals.Me.GetSpellCooldown(spell.SpellId) > Globals.System.GetTimeMs())
            {
                mCurrentSpellId = spell.SpellId;
                spellBase = SpellBase.Get(mCurrentSpellId);
                displayer.TextCoolDownVisible(false);

                GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Spell, spellBase.Icon);
                mIconCd = Globals.Me.GetSpellCooldown(spell.SpellId) > Globals.System.GetTimeMs();
                if (itemTex != null)
                {
                    displayer.IconSprite(itemTex.GetSpriteDefault());
                    displayer.IconVisible(true);
                    if (mIconCd)
                    {
                        displayer.IconColor(new Color32(255, 255, 255, 100));
                    }
                    else
                    {
                        displayer.IconColor(UnityEngine.Color.white);
                    }
                }
                else
                {
                    displayer.IconVisible(false);
                }

                mTexLoaded = spellBase.Icon;
                if (mIconCd)
                {
                    displayer.TextCoolDownVisible(true);
                    float secondsRemaining = (Globals.Me.GetSpellCooldown(mCurrentSpellId) - Globals.System.GetTimeMs()) / 1000f;
                    if (secondsRemaining > 10f)
                    {
                        displayer.SetTextCoolDown(Strings.Spells.cooldown.ToString(secondsRemaining.ToString("N0")));
                    }
                    else
                    {
                        displayer.SetTextCoolDown(Strings.Spells.cooldown.ToString(secondsRemaining.ToString("N1").Replace(".", Strings.Numbers.dec)));
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Guid.Empty.Equals(mCurrentSpellId))
            {
                return;
            }


            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                {
                    Globals.Me.TryUseSpell(Index);
                }
                break;
                case PointerEventData.InputButton.Right:
                {
                    Globals.Me.TryForgetSpell(Index);
                }
                break;
                case PointerEventData.InputButton.Middle:
                {
                }
                break;
            }
        }

        public override void Drop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            SpellItem draggedItem = eventData.pointerDrag.GetComponent<SpellItem>();
            if (draggedItem != null)
            {
                //Try to swap....
                Globals.Me.SwapSpells(draggedItem.Index, Index);
                return;
            }
        }
    }
}
