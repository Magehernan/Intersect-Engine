using Intersect.Client.Framework.Input;
using Intersect.Client.General;
using Intersect.Client.Items;
using Intersect.GameObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Displayers
{
    public class ItemSpellDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private Image imageIcon;
        [SerializeField]
        private TextMeshProUGUI textTop;
        [SerializeField]
        private GameObject containerTop;
        [SerializeField]
        private TextMeshProUGUI textBottom;
        [SerializeField]
        private GameObject containerBottom;
        [SerializeField]
        private TextMeshProUGUI textCooldown;

        private ItemBase itemBase;
        private Item item;
        private SpellBase spellBase;

        private Transform displayTransform;
        private Vector3 offset;
        private Vector2 pivot;
        private bool mouseOver = false;
        private string valueText = string.Empty;

        private Transform myTransform;

        private void Awake()
        {
            myTransform = transform;
        }

        private void OnDisable()
        {
            if (mouseOver)
            {
                mouseOver = false;
                Interface.GameUi.descWindow.Hide();
            }
        }

        internal void SetDescriptionPosition(Transform displayTransform, Vector3 offset, Vector2 pivot)
        {
            this.displayTransform = displayTransform;
            this.offset = offset;
            this.pivot = pivot;
        }

        internal void Set(ItemBase itemBase = null, Item item = null, SpellBase spellBase = null)
        {
            this.itemBase = itemBase;
            this.item = item;
            this.spellBase = spellBase;
            valueText = string.Empty;
        }

        internal void SetValue(string value)
        {
            valueText = value;
        }

        internal void SetTextTop(string text)
        {
            textTop.text = text;
        }

        internal void TextTopVisible(bool visible)
        {
            containerTop.SetActive(visible);
        }

        internal void SetTextBottom(string text)
        {
            textBottom.text = text;
        }

        internal void TextBottomVisible(bool visible)
        {
            containerBottom.SetActive(visible);

        }

        internal void SetTextCoolDown(string text)
        {
            textCooldown.text = text;
        }

        internal void TextCoolDownVisible(bool visible)
        {
            textCooldown.enabled = visible;
        }

        internal void IconVisible(bool visible)
        {
            imageIcon.enabled = visible;
        }

        internal void IconSprite(Sprite sprite)
        {
            imageIcon.sprite = sprite;
        }

        internal void IconColor(Color32 color)
        {
            imageIcon.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DescWindow descWindow = Interface.GameUi.descWindow;
            if (Globals.InputManager.KeyDown(KeyCode.Mouse0))
            {
                descWindow.Hide();
                return;
            }

            mouseOver = true;
            if (itemBase != null)
            {
                descWindow.SetItem(itemBase, item, displayTransform.position + offset, pivot, valueLabel: valueText);
                descWindow.Show();
            }
            else if (spellBase != null)
            {
                descWindow.SetSpell(spellBase.Id, displayTransform.position + offset, pivot);
                descWindow.Show();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Interface.GameUi.descWindow.Hide();
            mouseOver = false;
        }

        public void SetPosition(Vector2 position)
        {
            myTransform.position = position;
        }

        internal void Drag(Transform parent, bool startDragging)
        {
            myTransform.SetParent(parent);
            canvasGroup.blocksRaycasts = !startDragging;
            if (!startDragging)
            {
                myTransform.localPosition = Vector3.zero;
            }
        }
    }
}