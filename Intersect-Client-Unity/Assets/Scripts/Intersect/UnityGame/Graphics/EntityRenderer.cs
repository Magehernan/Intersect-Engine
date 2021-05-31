using Intersect.Client.UI.Components;
using Intersect.Client.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{

    public class EntityRenderer : MonoBehaviour
    {
        [Header("Entity")]
        [SerializeField]
        private Transform myTranform;
        [SerializeField]
        private SpriteRenderer spriteRendererPrefab;
        [SerializeField]
        private Transform spriteRendererContainer;
        [SerializeField]
        private RectTransform canvasTransform;
        [Header("Name"), SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private GameObject gameObjectName;
        [Header("Bars"), SerializeField]
        private FillBar hpBar;
        [SerializeField]
        private GameObject gameObjectHPBar;
        [SerializeField]
        private FillBar castBar;
        [SerializeField]
        private GameObject gameObjectCastBar;
        [Header("Target"), SerializeField]
        private SpriteRenderer targetSpriteRenderer;
        [SerializeField]
        private GameObject gameObjectTarget;

        [Header("Chat"), SerializeField]
        private Transform chatBubbleParent;

        public Transform ChatBubbleParent => chatBubbleParent;

        protected float height = 0f;

        private readonly List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

        protected string entityName = string.Empty;

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetPosition(float x, float y)
        {
            myTranform.position = new Vector2(x + .5f, -y);
        }

        public void SetHeight(int spriteHeight)
        {
            Vector2 sizeDelta = canvasTransform.sizeDelta;
            sizeDelta.y = spriteHeight;
            canvasTransform.sizeDelta = sizeDelta;
        }

        public void Draw(int z, Sprite sprite, byte alpha)
        {
            SpriteRenderer spriteRenderer = GetSpriteRenderer(z);
            if (spriteRenderer == null)
            {
                spriteRenderer = Instantiate(spriteRendererPrefab, spriteRendererContainer, false);
                spriteRenderers[z] = spriteRenderer;
                spriteRenderer.transform.SetSiblingIndex(z);
            }


            spriteRenderer.sprite = sprite;
            spriteRenderer.color = new Color32(255, 255, 255, alpha);
            spriteRenderer.enabled = true;
            if (z == 0)
            {
                //seteamos el height que se usa en otros lados
                height = sprite.rect.height;
                spriteRendererContainer.localPosition = new Vector2(0, height / sprite.pixelsPerUnit * .5f); 
                return;
            }
        }

        public void Hide(int z)
        {
            SpriteRenderer spriteRender = GetSpriteRenderer(z);
            if (spriteRender != null)
            {
                spriteRender.enabled = false;
            }
        }

        internal void HideAll()
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }

            HideHp();
            HideName();
            HideCastBar();
            HideTarget();
        }

        private SpriteRenderer GetSpriteRenderer(int index)
        {
            for (int i = spriteRenderers.Count - 1; i < index; i++)
            {
                spriteRenderers.Add(null);
            }
            return spriteRenderers[index];
        }

        #region Name
        public void DrawName(string name, Color color)
        {
            entityName = name;
            gameObjectName.SetActive(true);
            textName.text = name;
            textName.color = color.ToColor32();
        }

        public void HideName()
        {
            gameObjectName.SetActive(false);
        }
        #endregion

        #region HP
        public void ChangeHp(float value)
        {
            hpBar.ChangeValue(value);
            gameObjectHPBar.SetActive(true);
        }

        public void HideHp()
        {
            gameObjectHPBar.SetActive(false);
        }
        #endregion

        #region Cast
        public void ChangeCast(float value)
        {
            castBar.ChangeValue(value, true);
            gameObjectCastBar.SetActive(true);
        }

        public void HideCastBar()
        {
            gameObjectCastBar.SetActive(false);
        }
        #endregion

        #region Target
        public void DrawTarget(Sprite sprite)
        {
            targetSpriteRenderer.sprite = sprite;
            gameObjectTarget.SetActive(true);
        }

        public void HideTarget()
        {
            gameObjectTarget.SetActive(false);
        }
        #endregion
    }
}