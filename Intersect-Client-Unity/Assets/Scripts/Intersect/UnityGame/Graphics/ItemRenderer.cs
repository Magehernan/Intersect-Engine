using Intersect.Client.Utils;
using TMPro;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{
    public class ItemRenderer : MonoBehaviour
    {
        [SerializeField]
        private Transform myTranform;
        [SerializeField]
        private SpriteRenderer bodySpriteRenderer;
        [SerializeField]
        private TextMeshPro textName;
        [SerializeField]
        private Transform transformName;
        [SerializeField]
        private float textSeparationModifier;

        public void Draw(Sprite sprite, float x, float y)
        {
            Utils.Draw.Rectangle(new Framework.GenericClasses.FloatRect(x, y - 1, 1, 1), Color.White);
            bodySpriteRenderer.sprite = sprite;
            myTranform.position = new Vector2(x + .5f, -y + .5f);
        }

        public void DrawName(string name, LabelColor color, int order)
        {
            textName.text = name;
            textName.enabled = true;
            textName.color = color.Name.ToColor32();
            transformName.localPosition = Vector3.up * order * textSeparationModifier;
        }

        public void HideName()
        {
            textName.enabled = false;
        }

        internal void Destroy()
        {
            Destroy(gameObject);
        }
    }
}