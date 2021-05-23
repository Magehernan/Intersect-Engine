using Intersect.Client.Utils;
using TMPro;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{
    public class PlayerEntityRenderer : EntityRenderer
    {
        [Header("Player")]
        [SerializeField]
        private TextMeshProUGUI textGuild;
        [SerializeField]
        private GameObject gameObjectGuildLabel;
        [SerializeField]
        private Transform tranformGuildLabel;
        [SerializeField]
        private LightRenderer lightRendererPrefab;

        private LightRenderer lightRenderer;

        public void DrawGuildName(string guild, Color color)
        {
            gameObjectGuildLabel.SetActive(true);
            textGuild.text = guild;
            textGuild.color = color.ToColor32();
        }

        internal void HideGuild()
        {
            gameObjectGuildLabel.SetActive(false);
        }

        internal void SetGuildLabelPosition(int x, float y)
        {
            tranformGuildLabel.localPosition = new Vector2(x, y);
        }

        internal void UpdateLight(float size, float intensity, float expand, UnityEngine.Color color)
        {
            if (lightRenderer == null)
            {
                lightRenderer = UnityFactory.GetLightRender(entityName, transform);
            }

            lightRenderer.UpdateLight(size, intensity, expand, color, 2);
        }
    }
}