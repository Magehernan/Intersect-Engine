using Intersect.Client.UI.Components;
using Intersect.Client.Utils;
using System;
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
        [Header("Name"), SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private GameObject gameObjectName;
        [SerializeField]
        private Transform tranformName;
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
        private Transform tranformTarget;
        [SerializeField]
        private GameObject gameObjectTarget;

        [Header("Chat"), SerializeField]
        private Transform chatBubbleParent;

        public Transform ChatBubbleParent => chatBubbleParent;

        protected float height = 0f;

        private readonly List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        private readonly List<Transform> spriteRendererTransforms = new List<Transform>();

        protected string entityName = string.Empty;

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetPosition(float x, float y)
        {
            myTranform.position = new Vector2(x + .5f, -y);
        }

        public void Draw(int z, Sprite sprite, byte alpha)
        {
            SpriteRenderer spriteRenderer = GetSpriteRenderer(z);
            if (spriteRenderer == null)
            {
                spriteRenderer = Instantiate(spriteRendererPrefab, spriteRendererContainer, false);
                spriteRenderers[z] = spriteRenderer;
                spriteRendererTransforms[z] = spriteRenderer.transform;
                spriteRendererTransforms[z].SetSiblingIndex(z);
            }

            Transform transform = spriteRendererTransforms[z];

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = new Color32(255, 255, 255, alpha);
            spriteRenderer.enabled = true;
            if (z == 0)
            {
                //seteamos el height que se usa en otros lados
                height = sprite.rect.height;
            }
            else
            {
                if (height != sprite.rect.height)
                {
                    transform.localPosition = new Vector2(0, (height - sprite.rect.height) / sprite.pixelsPerUnit / 2f);
                }
                else
                {
                    transform.localPosition = Vector2.zero;
                }
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
        }

        private SpriteRenderer GetSpriteRenderer(int index)
        {
            for (int i = spriteRenderers.Count - 1; i < index; i++)
            {
                spriteRenderers.Add(null);
                spriteRendererTransforms.Add(null);
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

        public void SetNamePosition(float x, float y)
        {
            tranformName.localPosition = new Vector2(x, y);
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
            if (height != sprite.rect.height)
            {
                tranformTarget.localPosition = new Vector2(0, (height - sprite.rect.height) / sprite.pixelsPerUnit / 2f);
            }
            else
            {
                tranformTarget.localPosition = Vector2.zero;
            }
            gameObjectTarget.SetActive(true);
        }

        public void HideTarget()
        {
            gameObjectTarget.SetActive(false);
        }

        #endregion
    }
}