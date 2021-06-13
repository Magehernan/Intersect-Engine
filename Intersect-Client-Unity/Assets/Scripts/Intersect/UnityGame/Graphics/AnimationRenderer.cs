using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{
    public class AnimationRenderer : MonoBehaviour
    {
        [SerializeField]
        protected Transform myTranform = default;
        [SerializeField]
        protected SpriteRenderer lowerSpriteRenderer = default;
        [SerializeField]
        protected Transform lowerTranform = default;
        [SerializeField]
        protected SpriteRenderer upperSpriteRenderer = default;
        [SerializeField]
        protected Transform upperTranform = default;

        private GameObject myGameObject;
        private void Awake()
        {
            myGameObject = gameObject;
        }

        public void Draw(bool show)
        {
            myGameObject.SetActive(show);
        }

        public void SetPosition(float x, float y)
        {
            myTranform.position = new Vector2(x + .5f, -y + .5f);
        }

        public void DrawLower(Sprite sprite, float zRotation, bool alternateRenderLayer)
        {
            lowerSpriteRenderer.sprite = sprite;
            upperSpriteRenderer.sortingOrder = Core.Graphics.MIDDLE_LAYERS + (alternateRenderLayer ? 1 : -1);
            lowerTranform.eulerAngles = new Vector3(0, 0, zRotation);
        }

        public void DrawUpper(Sprite sprite, float zRotation, bool alternateRenderLayer)
        {
            upperSpriteRenderer.sprite = sprite;
            upperSpriteRenderer.sortingOrder = alternateRenderLayer ? Core.Graphics.MIDDLE_LAYERS : Core.Graphics.UPPER_LAYERS;
            upperTranform.eulerAngles = new Vector3(0, 0, zRotation);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}