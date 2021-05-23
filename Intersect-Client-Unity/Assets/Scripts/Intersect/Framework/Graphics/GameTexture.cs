using Intersect.Client.Framework.Content;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Intersect.Client.Framework.Graphics {

	public abstract class GameTexture : IAsset {
		public abstract string Name { get; }
		public abstract int Width { get; }
		public abstract int Height { get; }
		public abstract int SpriteWidth { get; }
		public abstract int SpriteHeight { get; }
		public abstract float PixelPerUnits { get; }
		public abstract Texture2D Texture { get; }
		public abstract Color GetPixel(int x, int y);
		public abstract Sprite GetSprite(int x, int y);
		public abstract Sprite GetSpriteAnimation(int index);
		public abstract Sprite GetSpriteDefault();
		public abstract TileBase GetTile(int x, int y, byte autotile);
	}

}
