using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Intersect.Client.UnityGame.Graphics
{
    public class LightRenderer : MonoBehaviour
    {
        [SerializeField]
        private Light2D light2d;

        [SerializeField]
        private Transform myTransform;

        internal void UpdateLight(float size, float intensity, float expand, UnityEngine.Color color, int order = 1)
        {
            //proceso la luz para que quede el maximo entre la luz el zol y la luz actual
            Color32 sunLightColor = Core.Graphics.SunLightColor;
            Color32 light = Color32.LerpUnclamped(sunLightColor, color, intensity / 255f);
            light.r = Math.Max(light.r, sunLightColor.r);
            light.g = Math.Max(light.g, sunLightColor.g);
            light.b = Math.Max(light.b, sunLightColor.b);

            //se divide por el tamaño del tile
            float externalRadio = size / 32f;
            light2d.pointLightOuterRadius = externalRadio;
            //se calcula en base a un valor de 100... 100 es totalmente expanido y 0 es sin expandir
            light2d.pointLightInnerRadius = externalRadio * expand / 100f;
            light2d.intensity = 1f;
            light2d.color = light;
            light2d.lightOrder = order;
        }

        internal void Destroy()
        {
            Destroy(gameObject);
        }

        internal void SetPosition(float x, float y)
        {
            //se suma la mitad de una celda para que quede centrado
            myTransform.position = new Vector2(x, -y);
        }
    }
}