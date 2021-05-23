using Intersect.Client.Framework.Graphics;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Intersect.Client.UnityGame.Graphics.Maps
{

    public class MapRenderer : MonoBehaviour
    {
        [SerializeField]
        private Transform myTransform;

        private readonly Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();

        public FogRenderer fogRenderer = default;

        public OverlayRenderer overlayRenderer = default;

        private int mapX;
        private int mapY;

        internal void Render(Dictionary<string, GameObjects.Maps.Tile[,]> layers, MapRenderer[,] mapGrid)
        {
            Debug.Log($"{nameof(Render)} {name}");
            foreach (string l in Options.Map.Layers.All)
            {
                GameObjects.Maps.Tile[,] layer = layers[l];
                Tilemap tilemap = GetTilemap(l);
                int width = layer.GetLength(0);
                int height = layer.GetLength(1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int tileX = layer[x, y].X;
                        int tileY = layer[x, y].Y;
                        if (tileX < 0 || tileY < 0)
                        {
                            continue;
                        }
                        GameTexture tilesetTex = (GameTexture)layer[x, y].TilesetTex;

                        try
                        {
                            Vector3Int tilePosition = new Vector3Int(mapX + x, -mapY - y, 0);
                            TileBase tile = null;
                            AutoTile autoTile = null;
                            if (tilesetTex != null)
                            {
                                tile = tilesetTex.GetTile(tileX, tileY, layer[x, y].Autotile);
                                autoTile = tile as AutoTile;
                                if (autoTile != null)
                                {
                                    AutoTile.MapGridX = mapX / Options.MapWidth;
                                    AutoTile.MapGridY = mapY / Options.MapHeight;
                                }
                                tilemap.SetTile(tilePosition, tile);
                            }

                            if (x == 0)
                            {
                                //West
                                MapRenderer mapBorder = mapGrid[0, 1];
                                if (mapBorder != null)
                                {
                                    AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                    AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                    mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                }
                                if (y == 0)
                                {
                                    //North West
                                    mapBorder = mapGrid[0, 0];
                                    if (mapBorder != null)
                                    {
                                        AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                        AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                        mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                    }
                                }
                                else if (y == Options.MapHeight - 1)
                                {
                                    //South West
                                    mapBorder = mapGrid[0, 2];
                                    if (mapBorder != null)
                                    {
                                        AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                        AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                        mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                    }
                                }

                            }
                            else if (x == Options.MapWidth - 1)
                            {
                                //East
                                MapRenderer mapBorder = mapGrid[2, 1];
                                if (mapBorder != null)
                                {
                                    AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                    AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                    mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                }

                                if (y == 0)
                                {
                                    //North East
                                    mapBorder = mapGrid[2, 0];
                                    if (mapBorder != null)
                                    {
                                        AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                        AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                        mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                    }
                                }
                                else if (y == Options.MapHeight - 1)
                                {
                                    //South East
                                    mapBorder = mapGrid[2, 2];
                                    if (mapBorder != null)
                                    {
                                        AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                        AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                        mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                    }
                                }
                            }
                            if (y == 0)
                            {
                                //North
                                MapRenderer mapBorder = mapGrid[1, 0];
                                if (mapBorder != null)
                                {
                                    AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                    AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                    mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                }
                            }
                            else if (y == Options.MapHeight - 1)
                            {
                                //South
                                MapRenderer mapBorder = mapGrid[1, 2];
                                if (mapBorder != null)
                                {
                                    AutoTile.MapGridX = mapBorder.mapX / Options.MapWidth;
                                    AutoTile.MapGridY = mapBorder.mapY / Options.MapHeight;
                                    mapBorder.GetTilemap(l).SetTile(tilePosition, autoTile);
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"{e.Message}\nTexture:{tilesetTex} X: {tileX} Y: {tileY} sprite Size: {tilesetTex.Width}x{tilesetTex.Height}");
                        }
                    }
                }
            }

            SortLayers(Options.Map.Layers.LowerLayers, Core.Graphics.LOWER_LAYERS);
            SortLayers(Options.Map.Layers.MiddleLayers, Core.Graphics.MIDDLE_LAYERS);
            SortLayers(Options.Map.Layers.UpperLayers, Core.Graphics.UPPER_LAYERS);
        }

        private void SortLayers(List<string> layers, int baseSort)
        {
            int sort = 0;
            foreach (string layer in layers)
            {
                GetTilemap(layer).GetComponent<TilemapRenderer>().sortingOrder = sort + baseSort;
                sort++;
            }
        }

        private Tilemap GetTilemap(string l)
        {
            if (!tilemaps.TryGetValue(l, out Tilemap tilemap))
            {
                tilemap = UnityFactory.GetTilemap(l, myTransform);
                tilemaps.Add(l, tilemap);
            }

            return tilemap;
        }

        internal void SetPosition(float x, float y)
        {
            mapX = (int)x;
            mapY = (int)y;
        }

        //internal void RefreshTile(int x, int y, int layer) {
        //	tilemaps[layer].RefreshTile(new Vector3Int(x, y, 0));
        //}

        internal void Destroy()
        {
            Destroy(gameObject);

            //if (!wasRendered) {
            //	return;
            //}

            //for (int l = 0; l < tilemaps.Length; l++) {
            //	Tilemap tilemap = tilemaps[l];
            //	int width = Options.MapWidth;
            //	int height = Options.MapHeight;
            //	for (int x = 0; x < width; x++) {
            //		for (int y = 0; y < height; y++) {
            //			Vector3Int tilePosition = new Vector3Int(mapX + x, mapY - y, 0);
            //			tilemap.SetTile(tilePosition, null);
            //		}
            //	}
            //}
        }

        //internal TileBase GetTile(int x, int y, int layer) {
        //	return tilemaps[layer].GetTile(new Vector3Int(x, y, 0));
        //}

        internal void SetBorderTile(int x, int y, string layer, GameObjects.Maps.Tile tile)
        {
            if (tile.X < 0 || tile.Y < 0)
            {
                return;
            }

            GameTexture tilesetTex = (GameTexture)tile.TilesetTex;

            byte autotile = tile.Autotile;
            Vector3Int tilePosition = new Vector3Int(mapX + x, -mapY - y, 0);
            TileBase tileBase = default;
            if (tilesetTex != null)
            {
                tileBase = tilesetTex.GetTile(tile.X, tile.Y, autotile);
            }
            AutoTile.MapGridX = mapX / Options.MapWidth;
            AutoTile.MapGridY = mapY / Options.MapHeight;
            GetTilemap(layer).SetTile(tilePosition, tileBase);
        }
    }
}