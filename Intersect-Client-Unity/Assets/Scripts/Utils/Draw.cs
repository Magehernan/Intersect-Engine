using Intersect.Client.Framework.GenericClasses;
using UnityEngine;

namespace Intersect.Client.Utils
{
    public class Draw
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Ellipse(Pointf position, float radiusX, float radiusY, int segments, UnityEngine.Color color, float duration = 0)
        {
            Ellipse(position, Vector3.forward, Vector3.up, radiusX, radiusY, segments, color, duration);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Ellipse(Pointf position, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, UnityEngine.Color color, float duration = 0)
        {
            Vector3 pos = new Vector2(position.X, -position.Y);
            float angle = 0f;
            Quaternion rot = Quaternion.LookRotation(forward, up);
            Vector3 lastPoint = Vector3.zero;
            Vector3 thisPoint = Vector3.zero;

            for (int i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0)
                {
                    Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Rectangle(FloatRect rectangle, Color color)
        {
            Vector3 leftBotR = new Vector3(rectangle.Left, -rectangle.Bottom);
            Vector3 rightBotR = new Vector3(rectangle.Right, -rectangle.Bottom);
            Vector3 leftTopR = new Vector3(rectangle.Left, -rectangle.Top);
            Vector3 rightTopR = new Vector3(rectangle.Right, -rectangle.Top);

            Rectangle(leftBotR, rightBotR, leftTopR, rightTopR, color.ToColor32());
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Rectangle(Vector3 leftBottom, Vector3 rightBottom, Vector3 leftTop, Vector3 rightTop, UnityEngine.Color color)
        {
            Debug.DrawLine(leftBottom, rightBottom, color);
            Debug.DrawLine(leftBottom, leftTop, color);
            Debug.DrawLine(leftTop, rightTop, color);
            Debug.DrawLine(rightBottom, rightTop, color);
        }
    }
}