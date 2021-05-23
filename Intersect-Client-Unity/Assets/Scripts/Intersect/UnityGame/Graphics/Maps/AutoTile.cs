using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using Intersect.Logging;
using Intersect.Client.General;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Intersect.Client.UnityGame.Graphics.Maps
{
    //#if UNITY_EDITOR
    //	[CreateAssetMenu(fileName = "New FangAuto Tile", order = 1)]
    //#endif
    public class AutoTile : TileBase
    {
        public enum TileTypes
        {
            Normal = 0,
            Autotile = 1,
            Fake = 2,
            Animated = 3,
            Cliff = 4,
            Waterfall = 5,
            AutotileXP = 6,
            AnimatedXP = 7
        }

        public TileTypes TileType { get; set; }
        public Texture2D Texture { get; private set; }

        private const float animationSpeed = 1.7f;
        private const float animationStartTime = 0f;
        private const bool enablePadding = true;
        private const int pixelPerUnit = 32;
        private const Tile.ColliderType colliderType = Tile.ColliderType.None;

        private List<AutoTilePattern> patterns;

        private int currentIndex = 0;

        public static int MapGridX { get; set; }
        public static int MapGridY { get; set; }

        private bool CheckForBeingPrepared()
        {
            if (patterns is null || patterns.Count == 0)
            {
                return false;
            }
            return true;
        }

        public override void RefreshTile(Vector3Int location, ITilemap tileMap)
        {
            for (int yd = -1; yd <= 1; yd++)
            {
                for (int xd = -1; xd <= 1; xd++)
                {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);


                    if (TileValue(tileMap, position))
                    {
                        tileMap.RefreshTile(position);
                    }
                }
            }
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            if (!CheckForBeingPrepared())
            {
                return false;
            }

            if (patterns[currentIndex].Frames.Length == 1)
            {
                return false;
            }
            else
            {
                tileAnimationData.animatedSprites = patterns[currentIndex].Frames;
                tileAnimationData.animationSpeed = animationSpeed;
                tileAnimationData.animationStartTime = animationStartTime;
                return true;
            }
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            if (!CheckForBeingPrepared())
            {
                return;
            }

            tileData.transform = Matrix4x4.identity;
            tileData.color = UnityEngine.Color.white;

            int mask = TileValue(tileMap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(1, 1, 0)) ? 2 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(1, 0, 0)) ? 4 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(1, -1, 0)) ? 8 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(0, -1, 0)) ? 16 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0;
            mask += TileValue(tileMap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0;

            int index = 0;
            for (int i = patterns.Count - 1; i >= 0; i--)
            {
                int masked = mask & (patterns[i].Mask);
                if (masked == patterns[i].Combination)
                {
                    //this is!
                    index = i;
                    break;
                }
            }
            if (TileValue(tileMap, location))
            {
                tileData.sprite = patterns[index].Frames[0];
                //dentro del mapa
                int mapGridX = location.x / Options.MapWidth;
                int mapGridY = location.y / -Options.MapHeight;

                if (mapGridX == MapGridX && mapGridY == MapGridY)
                {
                    tileData.color = UnityEngine.Color.white;
                }
                else
                {
                    //todos los que estan afuera tiene que ser invisibles
                    tileData.color = UnityEngine.Color.clear;
                }
                tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
                tileData.colliderType = colliderType;
            }
            currentIndex = index;
        }


        private bool TileValue(ITilemap tileMap, Vector3Int position)
        {
            TileBase tile = tileMap.GetTile(position);

            if (tile == null)
            {
                if (position.x < 0 || position.y > 0)
                {
                    return true;
                }

                int mapGridX = position.x / Options.MapWidth;
                int mapGridY = position.y / -Options.MapHeight;
                if (mapGridX < 0 || mapGridY < 0 || Globals.MapGrid.GetLength(0) <= mapGridX || Globals.MapGrid.GetLength(1) <= mapGridY)
                {
                    return true;
                }

                if (Guid.Empty.Equals(Globals.MapGrid[mapGridX, mapGridY]))
                {
                    return true;
                }

                return false;
            }

            return tile == this;
        }

        //private static bool IsInsideMap(Vector3Int location) {
        //	return location.x >= 0 && location.x < Options.MapWidth && location.y >= 0 && location.y < Options.MapHeight;
        //}

        #region Generation
        public void GenerateTile(Sprite sprite, UnityTexture unityTexture, int x, int y, TileTypes tileType)
        {
            try
            {
                int tileSize = unityTexture.SpriteWidth;
                CheckCorrection(tileType, sprite, tileSize, out int frameCount, out int animatedOffsetX);
                Sprite[] spriteParts = GetSpriteParts(unityTexture, x, y, tileType);

                UnityEngine.Color[][][][] Parts = GenerateParts(spriteParts, tileSize, frameCount, animatedOffsetX);
                patterns = EnumeratePatterns();

                int wholeTileSpriteCount = patterns.Count * frameCount;
                int texSize = CalcTexSize(tileSize, wholeTileSpriteCount);
                CreateTextureAndSprites(this, Parts, texSize, tileSize, frameCount);
            }
            catch (Exception e)
            {
                string error = $"Error generando Tile sprite: {sprite.name}\n{e}";
                Debug.LogError(error);
                Log.Error(error);
            }
        }

        private static Sprite[] GetSpriteParts(UnityTexture unityTexture, int x, int y, TileTypes tileType)
        {
            switch (tileType)
            {
                case TileTypes.Autotile:
                case TileTypes.Animated:
                    return new Sprite[] {
                        unityTexture.GetSprite(x + 1, y),
                        unityTexture.GetSprite(x, y + 1),
                        unityTexture.GetSprite(x + 1, y + 1),
                        unityTexture.GetSprite(x, y + 2),
                        unityTexture.GetSprite(x + 1, y + 2),
                    };
                case TileTypes.AutotileXP:
                case TileTypes.AnimatedXP:
                    return new Sprite[] {
                        unityTexture.GetSprite(x + 2, y),
                        unityTexture.GetSprite(x, y + 1),
                        unityTexture.GetSprite(x + 2, y + 1),
                        unityTexture.GetSprite(x, y + 3),
                        unityTexture.GetSprite(x + 2, y + 3),
                    };
                default:
                    throw new NotImplementedException($"tileTypes is not implemented: {tileType}");
            }
        }

        private enum QuadTypes
        {
            TL,
            TR,
            BL,
            BR
        }

        public static void CheckCorrection(TileTypes tileType, Sprite sprite, int tileSize, out int frameCount, out int animatedOffsetX)
        {
            //chequeo que la textura este en modo lectura y escritura
            if (!sprite.texture.isReadable)
            {
                throw new InvalidOperationException($"Texture isn't readeable name: {sprite.texture.name}");
            }

            switch (tileType)
            {
                case TileTypes.Autotile:
                case TileTypes.AutotileXP:
                    frameCount = 1;
                    animatedOffsetX = 0;
                    break;
                case TileTypes.Animated:
                    frameCount = 3;
                    animatedOffsetX = tileSize * 2;
                    break;
                case TileTypes.AnimatedXP:
                    frameCount = 3;
                    animatedOffsetX = tileSize * 3;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Tile Type: {tileType}");
            }
        }

        public static UnityEngine.Color[][][][] GenerateParts(Sprite[] spriteParts, int tileSize, int frameCount, int animatedOffsetX)
        {
            int partSize = tileSize / 2;
            int partColors = partSize * partSize;
            UnityEngine.Color[][][][] parts = new UnityEngine.Color[5][][][];
            //Type Iteration
            for (int tileType = 0; tileType < 5; tileType++)
            {
                Sprite sprite = spriteParts[tileType];
                Texture2D texture = null;
                int tx = 0;
                int ty = 0;
                if (sprite != null)
                {
                    texture = sprite.texture;
                    tx = (int)sprite.rect.x;
                    ty = (int)sprite.rect.y;
                }
                parts[tileType] = new UnityEngine.Color[frameCount][][];
                //Frame Iteration
                for (int frame = 0; frame < frameCount; frame++)
                {
                    parts[tileType][frame] = new UnityEngine.Color[4][];
                    //calc top-left of tile
                    int TLX = tx + frame * animatedOffsetX;
                    int TLY = ty;
                    //Part Iteration
                    for (int part = 0; part < 4; part++)
                    {
                        int tlxOffsetted = TLX + ((part == 1 || part == 3) ? partSize : 0);
                        int tlyOffsetted = TLY + ((part == 0 || part == 1) ? partSize : 0);
                        if (texture != null
                            && tlxOffsetted + partSize <= texture.width
                            && tlyOffsetted + partSize <= texture.height)
                        {
                            parts[tileType][frame][part] = texture.GetPixels(tlxOffsetted, tlyOffsetted, partSize, partSize);
                        }
                        else
                        {
                            parts[tileType][frame][part] = new UnityEngine.Color[partColors];
                        }

                    }
                }
            }
            return parts;
        }

        private static bool IsHorizontalEdgeClosed(int type, QuadTypes quad)
        {
            switch (type)
            {
                case 0:
                    return false;
                case 1:
                    switch (quad)
                    {
                        case QuadTypes.TL:
                        case QuadTypes.TR:
                            return true;
                        case QuadTypes.BL:
                        case QuadTypes.BR:
                            return false;
                    }
                    break;
                case 2:
                    switch (quad)
                    {
                        case QuadTypes.TL:
                        case QuadTypes.TR:
                            return true;
                        case QuadTypes.BL:
                        case QuadTypes.BR:
                            return false;
                    }
                    break;
                case 3:
                    switch (quad)
                    {
                        case QuadTypes.BL:
                        case QuadTypes.BR:
                            return true;
                        case QuadTypes.TL:
                        case QuadTypes.TR:
                            return false;
                    }
                    break;
                case 4:
                    switch (quad)
                    {
                        case QuadTypes.BL:
                        case QuadTypes.BR:
                            return true;
                        case QuadTypes.TL:
                        case QuadTypes.TR:
                            return false;
                    }
                    break;
            }
            throw new IndexOutOfRangeException("Tile type id must be in range from 0 to 4");
        }

        private static bool IsVerticalEdgeClosed(int type, QuadTypes quad)
        {
            switch (type)
            {
                case 0:
                    return false;
                case 1:
                    switch (quad)
                    {
                        case QuadTypes.TL:
                        case QuadTypes.BL:
                            return true;
                        case QuadTypes.TR:
                        case QuadTypes.BR:
                            return false;
                    }
                    break;
                case 2:
                    switch (quad)
                    {
                        case QuadTypes.TR:
                        case QuadTypes.BR:
                            return true;
                        case QuadTypes.TL:
                        case QuadTypes.BL:
                            return false;
                    }
                    break;
                case 3:
                    switch (quad)
                    {
                        case QuadTypes.TL:
                        case QuadTypes.BL:
                            return true;
                        case QuadTypes.TR:
                        case QuadTypes.BR:
                            return false;
                    }
                    break;
                case 4:
                    switch (quad)
                    {
                        case QuadTypes.TR:
                        case QuadTypes.BR:
                            return true;
                        case QuadTypes.TL:
                        case QuadTypes.BL:
                            return false;
                    }
                    break;
            }
            throw new IndexOutOfRangeException("Tile type id must be in range from 0 to 4");
        }

        private static int GetMaskPattern(int[] parts)
        {
            if (parts.Length != 4)
            {
                throw new ArgumentException("An Error occured when trying to get MASK pattern");
            }

            int[] primitiveMaskPatterns = {
                0b01010101,
                0b01011101,
                0b01110101,
                0b01010111,
                0b11010101
            };
            int[] directionMaskWindow = {
                0b11000001,
                0b00000111,
                0b01110000,
                0b00011100
            };
            int mask = 0;
            for (int i = 0; i < 4; i++)
            {
                mask |= primitiveMaskPatterns[parts[i]] & directionMaskWindow[i];
            }
            return mask;
        }

        private static int GetCombinationPattern(int[] parts)
        {
            if (parts.Length != 4)
            {
                throw new ArgumentException("An Error occured when trying to get COMBINATION pattern");
            }

            int[] primitiveCombinationPatterns = {
                0b01010101,
                0b00011100,
                0b01110000,
                0b00000111,
                0b11000001
            };
            int[] directionMaskWindow = {
                0b11000001,
                0b00000111,
                0b01110000,
                0b00011100
            };
            int mask = 0;
            for (int i = 0; i < 4; i++)
            {
                mask |= primitiveCombinationPatterns[parts[i]] & directionMaskWindow[i];
            }
            return mask;
        }

        //private static void RemoveAllSpriteAssets(AutoTile tile) {
        //	//Delete All Sprites
        //	if (tile.Patterns != null) {
        //		foreach (AutoTilePattern pattern in tile.Patterns) {
        //			if (pattern.Frames != null) {
        //				foreach (Sprite s in pattern.Frames) {
        //					if (s) {
        //						DestroyImmediate(s, true);
        //					}
        //				}
        //			}
        //		}
        //	}
        //}

        //private static void RemoveAllPatterns(AutoTile tile) {
        //	RemoveAllSpriteAssets(tile);
        //	if (tile.Patterns != null) {
        //		//foreach (AutoTilePattern pattern in tile.Patterns) {
        //		//	DestroyImmediate(pattern, true);
        //		//}
        //		tile.Patterns.Clear();
        //	} else {
        //		tile.Patterns = new List<AutoTilePattern>();
        //	}
        //}

        public static List<AutoTilePattern> EnumeratePatterns()
        {
            List<AutoTilePattern> patterns = new List<AutoTilePattern>();
            for (int TL = 0; TL < 5; TL++)
            {
                for (int TR = 0; TR < 5; TR++)
                {
                    if (IsHorizontalEdgeClosed(TL, QuadTypes.TL) == IsHorizontalEdgeClosed(TR, QuadTypes.TR))
                    {
                        //Correct
                        for (int BL = 0; BL < 5; BL++)
                        {
                            if (IsVerticalEdgeClosed(TL, QuadTypes.TL) == IsVerticalEdgeClosed(BL, QuadTypes.BL))
                            {
                                //Correct
                                for (int BR = 0; BR < 5; BR++)
                                {
                                    if (IsHorizontalEdgeClosed(BL, QuadTypes.BL) == IsHorizontalEdgeClosed(BR, QuadTypes.BR) && IsVerticalEdgeClosed(TR, QuadTypes.TR) == IsVerticalEdgeClosed(BR, QuadTypes.BR))
                                    {
                                        //Correct
                                        int[] parts = new int[4] { TL, TR, BL, BR };
                                        AutoTilePattern pattern = new AutoTilePattern
                                        {
                                            Mask = GetMaskPattern(parts),
                                            Combination = GetCombinationPattern(parts),
                                            Pattern = parts,
                                            name = $"Pattern {TL}{TR}{BL}{BR}",
                                        };
                                        patterns.Add(pattern);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return patterns;
        }

        public static int CalcTexSize(int tileSize, int tileCount)
        {
            //Approach from square
            int paddedTileSize = tileSize;
            if (enablePadding)
            {
                paddedTileSize += 2;
            }

            int s = paddedTileSize * paddedTileSize * tileCount;
            int e = (int)Mathf.Sqrt(s);
            int minW = 0;
            for (int i = 2; i <= 4096; i *= 2)
            {
                if (e < i)
                {
                    minW = i;
                    break;
                }
            }
            if (minW == 0)
            {
                throw new InvalidOperationException("Your tiles are too big or have too many frames!");
            }

            while (true)
            {
                int c = minW / paddedTileSize;
                if (c * c >= tileCount)
                {
                    //Enough
                    break;
                }
                else
                {
                    //Falls short
                    if (minW >= 2048)
                    {
                        throw new InvalidOperationException("Your tiles are too big or have too many frames!");

                    }
                    minW *= 2;

                    continue;
                }
            }
            return minW;
        }

        public static void CreateTextureAndSprites(AutoTile tile, UnityEngine.Color[][][][] parts, int texSize, int tileSize, int frameCount)
        {
            int paddedTileSize = tileSize;
            if (enablePadding)
            {
                paddedTileSize += 2;
            }

            int c = texSize / paddedTileSize;
            int column = 0;
            int row = 0;
            UnityEngine.Color[] mainTexArray = new UnityEngine.Color[texSize * texSize];
            Texture2D mainTex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);

            int partSize = tileSize / 2;

            for (int i = 0; i < tile.patterns.Count; i++)
            {
                tile.patterns[i].Frames = new Sprite[frameCount];
                for (int frame = 0; frame < frameCount; frame++)
                {
                    int tlx = column * paddedTileSize;
                    int tly = row * paddedTileSize;
                    for (int part = 0; part < 4; part++)
                    {
                        int tlxOffsetted = tlx + (part % 2 == 1 ? partSize : 0);
                        int tlyOffsetted = tly + (part < 2 ? partSize : 0);
                        for (int line = 0; line < partSize; line++)
                        {
                            if (!enablePadding)
                            {
                                //No Padding
                                Array.Copy(parts[tile.patterns[i].Pattern[part]][frame][part], line * partSize, mainTexArray, tlyOffsetted * texSize + tlxOffsetted, partSize);
                            }
                            else
                            {
                                //padding enabled
                                Array.Copy(parts[tile.patterns[i].Pattern[part]][frame][part], line * partSize, mainTexArray, (tlyOffsetted + 1) * texSize + (tlxOffsetted + 1), partSize);
                                //Clamp x
                                if (part == 0 || part == 2)
                                {
                                    mainTexArray[(tlyOffsetted + 1) * texSize + tlxOffsetted] = parts[tile.patterns[i].Pattern[part]][frame][part][line * partSize];
                                }

                                if (part == 1 || part == 3)
                                {
                                    mainTexArray[(tlyOffsetted + 1) * texSize + tlxOffsetted + partSize + 1] = parts[tile.patterns[i].Pattern[part]][frame][part][(line + 1) * partSize - 1];
                                }
                            }
                            tlyOffsetted++;
                        }
                    }
                    if (enablePadding)
                    {
                        //Clamp y
                        Array.Copy(mainTexArray, (tly + 1) * texSize + tlx, mainTexArray, tly * texSize + tlx, paddedTileSize);
                        Array.Copy(mainTexArray, (tly + tileSize + 0) * texSize + tlx, mainTexArray, (tly + tileSize + 1) * texSize + tlx, paddedTileSize);
                        tlx += 1;
                        tly += 1;
                    }

                    Sprite s = Sprite.Create(mainTex, new Rect(tlx, tly, tileSize, tileSize), new Vector2(0.5f, 0.5f), pixelPerUnit);
                    tile.patterns[i].Frames[frame] = s;
                    column++;
                    if (column >= c)
                    {
                        row++;
                        column = 0;
                    }
                }
            }
            mainTex.filterMode = FilterMode.Point;
            mainTex.wrapMode = TextureWrapMode.Clamp;
            mainTex.SetPixels(mainTexArray);
            mainTex.Apply(false, true);
            mainTex.name = "MainTexture";
            tile.Texture = mainTex;
        }
        #endregion
    }

    #region Editor
#if UNITY_EDITOR
    //[CustomEditor(typeof(AutoTile))]
    //public class FangAutoTileEditor : Editor {
    //	private AutoTile Tile => target as AutoTile;

    //	private AssetReferences assetReferences;

    //	public override void OnInspectorGUI() {
    //		if (assetReferences == null) {
    //			assetReferences = AssetDatabase.LoadAssetAtPath<AssetReferences>("Assets/IntersectResources/AssetReferences.asset");
    //		}

    //		serializedObject.Update();

    //		EditorGUI.BeginChangeCheck();
    //		EditorGUILayout.LabelField("");
    //		if (!TypesEnabled(Tile.TileType)) {
    //			Tile.TileType = TileTypes.Autotile;
    //		}

    //		int tileAmount;
    //		switch (Tile.TileType) {
    //			case TileTypes.Autotile:
    //			case TileTypes.Animated:
    //				tileAmount = 5;
    //				break;
    //			case TileTypes.AutotileXP:
    //			case TileTypes.AnimatedXP:
    //				tileAmount = 5;
    //				break;
    //			default:
    //				throw new FangAutoTileIncorrectException("Invalid Tile Type");
    //		}

    //		if (Tile.SpriteParts is null) {
    //			Tile.SpriteParts = new Sprite[tileAmount];
    //		}

    //		if (Tile.SpriteParts.Length != tileAmount) {
    //			Array.Resize(ref Tile.SpriteParts, tileAmount);
    //		}

    //		Tile.TileType = (TileTypes)EditorGUILayout.EnumPopup(new GUIContent("Tile Type"), Tile.TileType, TypesEnabled, false);
    //		Tile.mainSprite = (Sprite)EditorGUILayout.ObjectField($"Main Sprite", Tile.mainSprite, typeof(Sprite), false, null);

    //		for (int i = 0; i < Tile.SpriteParts.Length; i++) {
    //			Tile.SpriteParts[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite Part {i}", Tile.SpriteParts[i], typeof(Sprite), false, null);
    //		}

    //		if (GUILayout.Button("Fetch Tiles")) {
    //			if (Tile.mainSprite != null) {
    //				string textureName = $"{Tile.mainSprite.texture.name}.png";
    //				UnityTexture textureAsset = default;
    //				foreach (UnityTexture asset in assetReferences.tilesets) {
    //					if (asset.Name.Equals(textureName, StringComparison.OrdinalIgnoreCase)) {
    //						textureAsset = asset;
    //						break;
    //					}
    //				}

    //				if (textureAsset != null) {
    //					string[] split = Tile.mainSprite.name.Split('_');
    //					int currentIndex = int.Parse(split[split.Length - 1]);

    //					(int x, int y) = textureAsset.GetCoord(currentIndex);
    //					Tile.SpriteParts[0] = textureAsset.GetSprite(x + 1, y);
    //					Tile.SpriteParts[1] = textureAsset.GetSprite(x, y + 1);
    //					Tile.SpriteParts[2] = textureAsset.GetSprite(x + 1, y + 1);
    //					Tile.SpriteParts[3] = textureAsset.GetSprite(x, y + 2);
    //					Tile.SpriteParts[4] = textureAsset.GetSprite(x + 1, y + 2);
    //				}
    //			}
    //		}


    //		Tile.enablePadding = EditorGUILayout.ToggleLeft("Enable Padding", Tile.enablePadding, GUIStyle.none, null);
    //		Tile.forceRelayout = EditorGUILayout.ToggleLeft("Force Re-layout", Tile.forceRelayout, GUIStyle.none, null);
    //		Tile.oneTilePerUnit = EditorGUILayout.ToggleLeft("1 Tile Per Unit", Tile.oneTilePerUnit, GUIStyle.none, null);
    //		if (!Tile.oneTilePerUnit) {
    //			Tile.pixelPerUnit = EditorGUILayout.IntField("Pixel Per Unit", Tile.pixelPerUnit);
    //		}
    //		Tile.hideChildAssets = EditorGUILayout.ToggleLeft("Hide Sprite Assets", Tile.hideChildAssets, GUIStyle.none, null);
    //		if (GUILayout.Button("GENERATE!")) {
    //			SetupTiles(Tile.forceRelayout);
    //		}
    //		if (!string.IsNullOrEmpty(Tile.tileInfoMessage)) {
    //			EditorGUILayout.HelpBox(Tile.tileInfoMessage, MessageType.Info);
    //		}

    //		EditorGUILayout.LabelField("-");
    //		Tile.colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Collider Type", Tile.colliderType);
    //		float minSpeed = EditorGUILayout.FloatField("Minimum Speed", Tile.m_MinSpeed);
    //		float maxSpeed = EditorGUILayout.FloatField("Maximum Speed", Tile.m_MaxSpeed);
    //		if (minSpeed < 0.0f) {
    //			minSpeed = 0.0f;
    //		}

    //		if (maxSpeed < 0.0f) {
    //			maxSpeed = 0.0f;
    //		}

    //		if (maxSpeed < minSpeed) {
    //			maxSpeed = minSpeed;
    //		}

    //		Tile.m_MinSpeed = minSpeed;
    //		Tile.m_MaxSpeed = maxSpeed;
    //		Tile.m_AnimationStartTime = EditorGUILayout.FloatField("Animation Start Time", Tile.m_AnimationStartTime);

    //		if (GUILayout.Button("Export Extracted Texture")) {
    //			string path = EditorUtility.SaveFilePanel("Export Extracted Texture", "", name + ".png", "png");
    //			if (path.Length != 0) {
    //				byte[] pngData = Tile.Texture.EncodeToPNG();
    //				if (pngData != null) {
    //					System.IO.File.WriteAllBytes(path, pngData);
    //				}
    //			}
    //		}
    //		EditorGUILayout.Space();
    //		if (GUILayout.Button("Regenerate Patterns")) {
    //			EnumeratePatterns(Tile);
    //		}


    //		if (EditorGUI.EndChangeCheck()) {
    //			EditorUtility.SetDirty(Tile);
    //		}
    //		serializedObject.ApplyModifiedProperties();
    //	}

    //	private bool TypesEnabled(Enum arg) {
    //		TileTypes value = (TileTypes)arg;
    //		return value == TileTypes.Animated
    //			|| value == TileTypes.AnimatedXP
    //			|| value == TileTypes.Autotile
    //			|| value == TileTypes.AutotileXP;
    //	}

    //	public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height) {
    //		if (Tile.Patterns != null && Tile.Patterns.Count != 0 && Tile.Patterns[0].Frames != null && Tile.Patterns[0].Frames.Length != 0) {
    //			Texture2D p = AssetPreview.GetAssetPreview(Tile.Patterns[0].Frames[0]);
    //			Texture2D f = new Texture2D(width, height);
    //			EditorUtility.CopySerialized(p, f);
    //			return f;
    //		}
    //		return base.RenderStaticPreview(assetPath, subAssets, width, height);
    //	}

    //	private void GenerateTileInfoMessage(int tileSize, int frameCount, int texSize) {
    //		StringBuilder sb = new StringBuilder();
    //		sb.Append("Generated data:\n");
    //		sb.AppendFormat("  Tile Size: {0}\n", tileSize.ToString());
    //		sb.AppendFormat("  Frame Count: {0}\n", frameCount.ToString());
    //		sb.AppendFormat("  Sprite Count: {0}\n", (Tile.Patterns.Count * frameCount).ToString());
    //		sb.AppendFormat("  Texture Size: {0}", texSize.ToString());
    //		Tile.tileInfoMessage = sb.ToString();
    //	}

    //	private void SetupTiles(bool forceRelayout) {
    //		try {
    //			CheckCorrection(Tile.TileType, Tile.mainSprite, out int tileSize, out int frameCount, out int animatedOffsetX);
    //			UnityEngine.Color[][][][] Parts = GenerateParts(Tile.SpriteParts, tileSize, frameCount, animatedOffsetX);
    //			if (Tile.Patterns == null || Tile.Patterns.Count == 0) {
    //				EnumeratePatterns(Tile);
    //			}

    //			int wholeTileSpriteCount = Tile.Patterns.Count * frameCount;
    //			int texSize = CalcTexSize(Tile, tileSize, wholeTileSpriteCount);
    //			CreateTextureAndSprites(Tile, Parts, texSize, tileSize, frameCount, forceRelayout);
    //			GenerateTileInfoMessage(tileSize, frameCount, texSize);
    //		} catch (Exception e) {
    //			EditorUtility.DisplayDialog("Error", e.Message, "OK");
    //			throw e;
    //		}
    //		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Tile));
    //	}


    //}

    //[CustomPreview(typeof(AutoTile))]
    //public class FangAutoTilePreview : ObjectPreview {
    //	private readonly GUIContent previewTitle = new GUIContent("Tiles");
    //	private AutoTile Tile => target as AutoTile;
    //	public override bool HasPreviewGUI() {
    //		if (!target) {
    //			return false;
    //		}

    //		return m_Targets.Length > 1;
    //	}

    //	public override GUIContent GetPreviewTitle() {
    //		return previewTitle;
    //	}
    //	public override void Initialize(UnityEngine.Object[] targets) {
    //		base.Initialize(targets);

    //		UnityEngine.Object[] sprites = new UnityEngine.Object[0];
    //		if (Tile.Patterns != null) {
    //			foreach (AutoTilePattern pattern in Tile.Patterns) {
    //				if (pattern.Frames != null) {
    //					ArrayUtility.AddRange(ref sprites, pattern.Frames);
    //				}
    //			}
    //		}

    //		if (sprites.Length != 0) {
    //			m_Targets = sprites;
    //		}
    //	}
    //	public override void OnPreviewGUI(Rect r, GUIStyle background) {
    //		Texture2D previewTexture = AssetPreview.GetAssetPreview(target);
    //		if (previewTexture) {
    //			EditorGUI.DrawTextureTransparent(r, previewTexture);
    //		}
    //	}
    //}

    //[CustomPreview(typeof(AutoTile))]
    //public class FangAutoTileTexturePreview : ObjectPreview {
    //	private readonly GUIContent previewTitle = new GUIContent("Texture");
    //	private AutoTile Tile => target as AutoTile;
    //	public override bool HasPreviewGUI() {
    //		if (!target) {
    //			return false;
    //		}

    //		return Tile.Texture;
    //	}

    //	public override GUIContent GetPreviewTitle() {
    //		return previewTitle;
    //	}
    //	public override void OnPreviewGUI(Rect r, GUIStyle background) {
    //		if (Tile.Texture) {
    //			//    var previewTexture = AssetPreview.GetAssetPreview(tile.Texture);
    //			//    EditorGUI.DrawTextureTransparent(r, previewTexture);
    //			Editor e = Editor.CreateEditor(Tile.Texture);
    //			e.OnPreviewGUI(r, background);
    //			//GUI.SelectionGrid(r, -1, new Texture2D[] { tile.Texture }, 1, EditorStyles.whiteBoldLabel);
    //			//EditorGUI.DrawTextureTransparent(r, tile.Texture);
    //		}
    //	}
    //}

    //public class FangAutoTileIncorrectException : Exception {
    //	public FangAutoTileIncorrectException(string message) : base(message) {
    //	}
    //}
#endif
    #endregion
}