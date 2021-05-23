using Intersect.Client.Framework.Graphics;
using Intersect.Client.UnityGame.Graphics.Maps;
using Intersect.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Intersect.Client.Framework.File_Management.GameContentManager;
using static Intersect.Client.UnityGame.Graphics.Maps.AutoTile;

namespace Intersect.Client.UnityGame.Graphics
{
    [Serializable]
    public class UnityTexture : GameTexture
    {
        [SerializeField]
        private string name = string.Empty;
        [SerializeField]
        private Texture2D texture = default;
        [SerializeField]
        private List<Sprite> sprites = default;
        [SerializeField]
        private int spriteColumns;
        [SerializeField]
        private int width = 0;
        [SerializeField]
        private int height = 0;
        [SerializeField]
        private int spriteWidth = 0;
        [SerializeField]
        private int spriteHeight = 0;
        [SerializeField]
        private bool hasTexture;

        private readonly Dictionary<TileTypes, Dictionary<int, TileBase>> autoTiles = new Dictionary<TileTypes, Dictionary<int, TileBase>>();

#if UNITY_EDITOR
        public UnityTexture() { }

        public UnityTexture(string name, Texture2D texture, List<Sprite> sprites, TextureType textureType)
        {
            this.name = name;
            this.texture = texture;
            this.sprites = sprites;
            switch (textureType)
            {
                case TextureType.Entity:
                case TextureType.Paperdoll:
                    spriteColumns = 4;
                    break;
                case TextureType.Face:
                case TextureType.Fog:
                case TextureType.Image:
                case TextureType.Item:
                case TextureType.Spell:
                case TextureType.Misc:
                case TextureType.Resource:
                    spriteColumns = 1;
                    break;
                case TextureType.Tileset:
                    spriteColumns = texture.width / 32;
                    break;

                case TextureType.Animation:
                    spriteColumns = -1;
                    break;
                default:
                    throw new NotImplementedException($"Not Implemented: {textureType} name: {name}");
            }
            hasTexture = texture != null;
            if (hasTexture)
            {
                width = texture.width;
                height = texture.height;
            }
            if (sprites?.Count > 0)
            {
                spriteWidth = (int)sprites[0].rect.size.x;
                spriteHeight = (int)sprites[0].rect.size.y;
            }
        }

        public (int x, int y) GetCoord(int index)
        {
            return (index % spriteColumns, index / spriteColumns);
        }
#endif


        public override string Name => name;

        public override int Width => width;

        public override int Height => height;

        public override int SpriteWidth => spriteWidth;

        public override int SpriteHeight => spriteHeight;

        public override float PixelPerUnits => GetSpriteDefault().pixelsPerUnit;

        public override Texture2D Texture => texture;

        public override Color GetPixel(int x1, int y1)
        {
            if (!hasTexture)
            {
                return Color.White;
            }

            Texture2D tex = texture;
            UnityEngine.Color pixel = tex.GetPixel(x1, y1);

            return new Color((int)(pixel.a * 255), (int)(pixel.r * 255), (int)(pixel.g * 255), (int)(pixel.b * 255));
        }


        public override Sprite GetSpriteAnimation(int index)
        {
            if (hasTexture && index < sprites.Count)
            {
                return sprites[index];
            }
            return default;
        }


        public override Sprite GetSprite(int x, int y)
        {
            int index = x + y * spriteColumns;
            if (hasTexture && index < sprites.Count)
            {
                return sprites[index];
            }
            return default;
        }

        public override Sprite GetSpriteDefault()
        {
            return sprites[0];
        }

        public override TileBase GetTile(int x, int y, byte tileTypeNumber)
        {
            int index = x + y * spriteColumns;

            TileTypes tileType = (TileTypes)tileTypeNumber;
            if (!autoTiles.TryGetValue(tileType, out Dictionary<int, TileBase> autoTileType))
            {
                autoTileType = new Dictionary<int, TileBase>();
                autoTiles.Add(tileType, autoTileType);
            }

            if (!autoTileType.TryGetValue(index, out TileBase tile))
            {
                switch (tileType)
                {
                    case TileTypes.Normal:
                    {
                        Tile newTile = ScriptableObject.CreateInstance<Tile>();
                        newTile.sprite = sprites[index];
                        tile = newTile;
                    }
                    break;
                    case TileTypes.AutotileXP:
                    case TileTypes.AnimatedXP:
                    case TileTypes.Animated:
                    case TileTypes.Autotile:
                    {
                        AutoTile newTile = ScriptableObject.CreateInstance<AutoTile>();
                        newTile.GenerateTile(sprites[index], this, x, y, tileType);
                        tile = newTile;
                    }
                    break;
                    default:
                    {
                        string error = $"Falta implementar tipo de tile: {tileType}";
                        Debug.Log(error);
                        Log.Error(error);
                        return default;
                    }
                }
                autoTileType.Add(index, tile);
            }

            return tile;
        }
    }
}
