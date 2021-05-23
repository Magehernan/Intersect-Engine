using Intersect.Client.Entities;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Input;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.GameObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.EntityPanel
{

    public class SpellStatus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TextMeshProUGUI textDuration;
        [SerializeField]
        private Image imageIcon;
        [SerializeField]
        private Vector3 descOffset;

        private Vector2 descPivot = new Vector2(0f, 1f);

        private Transform descTransformDisplay;

        private Status status;
        private SpellBase spell;
        private string mTexLoaded;
        private bool mouseOver = false;

        internal void Draw()
        {
            if (status != null)
            {
                long remaining = status.RemainingMs();
                float secondsRemaining = remaining / 1000f;
                if (secondsRemaining > 10f)
                {
                    textDuration.text = Strings.EntityBox.cooldown.ToString((remaining / 1000f).ToString("N0"));
                }
                else
                {
                    textDuration.text = Strings.EntityBox.cooldown.ToString((remaining / 1000f).ToString("N1").Replace(".", Strings.Numbers.dec));
                }

                if ((mTexLoaded != string.Empty && spell == null
                    || spell != null && mTexLoaded != spell.Icon
                    || spell.Id != status.SpellId)
                    && remaining > 0)
                {
                    if (spell != null)
                    {
                        GameTexture spellTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Spell, spell.Icon);

                        if (spellTex != null)
                        {
                            imageIcon.sprite = spellTex.GetSpriteDefault();
                            imageIcon.enabled = true;
                        }
                        else
                        {
                            imageIcon.enabled = false;

                        }

                        mTexLoaded = spell.Icon;
                    }
                    else
                    {
                        mTexLoaded = string.Empty;
                    }
                }
                else if (remaining <= 0)
                {
                    mTexLoaded = string.Empty;
                }
            }
        }

        internal void UpdateStatus(Status status, Transform descTransformDisplay)
        {
            this.status = status;
            spell = SpellBase.Get(status.SpellId);
            this.descTransformDisplay = descTransformDisplay;
        }

        private void OnDisable()
        {
            if (mouseOver)
            {
                mouseOver = false;
                Interface.GameUi.descWindow.Hide();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DescWindow descWindow = Interface.GameUi.descWindow;
            mouseOver = true;
            if (spell != null)
            {
                descWindow.SetSpell(spell.Id, descTransformDisplay.position + descOffset, descPivot);
                descWindow.Show();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Interface.GameUi.descWindow.Hide();
            mouseOver = false;
        }
    }

}
