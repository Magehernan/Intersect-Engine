using UnityEngine;

namespace Intersect.Client.Utils
{
    public static class Extensions
    {

        public static Color32 ToColor32(this Color color)
        {
            return new Color32(color.R, color.G, color.B, color.A);
        }

        public static Color32 ToColor32(this ColorF color)
        {
            return new Color32((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
        }

        public static Vector2 ToVector2(this Framework.GenericClasses.Pointf pointf)
        {
            return new Vector2(pointf.X, pointf.Y);
        }
    }
}
