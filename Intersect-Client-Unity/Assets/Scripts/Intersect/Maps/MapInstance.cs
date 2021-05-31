using Intersect.Client.Core;
using Intersect.Client.Core.Sounds;
using Intersect.Client.Entities;
using Intersect.Client.Entities.Events;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Client.UnityGame.Graphics.Maps;
using Intersect.Client.Utils;
using Intersect.Compression;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Maps;
using Intersect.Logging;
using Intersect.Network.Packets.Server;
using Intersect.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using U = UnityEngine;

namespace Intersect.Client.Maps
{
    public class MapInstance : MapBase, IGameObject<Guid, MapInstance>
    {

        //Client Only Values
        public delegate void MapLoadedDelegate(MapInstance map);

        //Map State Variables
        public static Dictionary<Guid, long> MapRequests = new Dictionary<Guid, long>();

        public static MapLoadedDelegate OnMapLoaded;

        public new static MapInstances Lookup { get; } = new MapInstances(MapBase.Lookup);

        //private static MapInstances sLookup;

        public List<WeatherParticle> _removeParticles = new List<WeatherParticle>();
        //Weather
        public List<WeatherParticle> _weatherParticles = new List<WeatherParticle>();

        private long _weatherParticleSpawnTime;

        //Action Msg's
        public List<ActionMessage> ActionMsgs = new List<ActionMessage>();


        public List<MapSound> AttributeSounds = new List<MapSound>();

        //Map Animations
        public ConcurrentDictionary<Guid, MapAnimation> LocalAnimations = new ConcurrentDictionary<Guid, MapAnimation>();

        public Dictionary<Guid, Entity> LocalEntities = new Dictionary<Guid, Entity>();

        //Map Critters
        public Dictionary<Guid, Critter> Critters = new Dictionary<Guid, Critter>();

        //Map Items
        public Dictionary<int, List<MapItemInstance>> MapItems = new Dictionary<int, List<MapItemInstance>>();

        private readonly Dictionary<Guid, ItemRenderer> itemRenderers = new Dictionary<Guid, ItemRenderer>();

        //Map Players/Events/Npcs
        public List<Guid> LocalEntitiesToDispose = new List<Guid>();

        //Map Attributes
        private readonly Dictionary<MapAttribute, Animation> mAttributeAnimInstances = new Dictionary<MapAttribute, Animation>();
        private readonly Dictionary<MapAttribute, Entity> mAttributeCritterInstances = new Dictionary<MapAttribute, Entity>();

        protected float mCurFogIntensity;

        private readonly List<Event> mEvents = new List<Event>();

        protected float mFogCurrentX;

        protected float mFogCurrentY;

        //Fog Variables
        protected long mFogUpdateTime = -1;

        //Update Timer
        private long mLastUpdateTime;

        protected float mOverlayIntensity;

        //Overlay Image Variables
        protected long mOverlayUpdateTime = -1;

        protected float mPanoramaIntensity;

        //Panorama Variables
        protected long mPanoramaUpdateTime = -1;

        private bool mTexturesFound = false;

        public int MapGridX { get; internal set; }
        public int MapGridY { get; internal set; }
        public bool[] CameraHolds { get; internal set; }
        public bool MapLoaded { get; private set; }
        public MapSound BackgroundSound { get; set; }

        private MapRenderer mapRenderer;

        private readonly List<LightRenderer> lightRenderers = new List<LightRenderer>();

        public new static MapInstance Get(Guid id)
        {
            return Lookup.Get<MapInstance>(id);
        }

        private bool needsRender;

        //Initialization
        public MapInstance(Guid mapId) : base(mapId) { }

        public void Load(string json)
        {
            LocalEntitiesToDispose.AddRange(LocalEntities.Keys.ToArray());
            JsonConvert.PopulateObject(
                json, this, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }
            );

            if (mapRenderer != null)
            {
                Log.Error("MapRenderer Already Exist");
            }

            if (mapRenderer == null)
            {
                mapRenderer = UnityFactory.GetMapRenderer(Name);
            }

            DisposeLights();

            foreach (LightBase light in Lights)
            {
                LightRenderer lightRenderer = UnityFactory.GetLightRender(Name);
                lightRenderers.Add(lightRenderer);

                float x = GetX() + light.TileX + light.OffsetX + .5f;
                float y = GetY() + light.TileY + light.OffsetY - .5f;
                lightRenderer.SetPosition(x, y);
            }

            mapRenderer.SetPosition(GetX(), GetY());
            MapLoaded = true;
            OnMapLoaded -= HandleMapLoaded;
            OnMapLoaded += HandleMapLoaded;
            if (MapRequests.ContainsKey(Id))
            {
                MapRequests.Remove(Id);
            }
        }

        public void LoadTileData(byte[] packet)
        {
            Layers = JsonConvert.DeserializeObject<Dictionary<string, Tile[,]>>(LZ4.UnPickleString(packet), mJsonSerializerSettings);
            foreach (string layer in Options.Instance.MapOpts.Layers.All)
            {
                if (!Layers.ContainsKey(layer))
                {
                    Layers.Add(layer, new Tile[Options.MapWidth, Options.MapHeight]);
                }
            }
            needsRender = true;
        }

        private void CacheTextures()
        {
            if (mTexturesFound == false && GameContentManager.Current.TilesetsLoaded)
            {
                foreach (string layer in Options.Instance.MapOpts.Layers.All)
                {
                    for (int x = 0; x < Options.MapWidth; x++)
                    {
                        for (int y = 0; y < Options.MapHeight; y++)
                        {
                            TilesetBase tileset = TilesetBase.Get(Layers[layer][x, y].TilesetId);
                            if (tileset != null)
                            {
                                GameTexture tilesetTex = Globals.ContentManager.GetTexture(
                                    GameContentManager.TextureType.Tileset, tileset.Name
                                );

                                Layers[layer][x, y].TilesetTex = tilesetTex;
                            }
                        }
                    }
                }

                mTexturesFound = true;
            }
        }

        public void Update(bool isLocal)
        {
            if (isLocal)
            {
                mLastUpdateTime = Globals.System.GetTimeMs() + 10000;
                UpdateMapAttributes();

                if (BackgroundSound == null && !TextUtils.IsNone(Sound))
                {
                    BackgroundSound = Audio.AddMapSound(Sound, -1, -1, Id, true, 0, 10);
                }

                foreach (KeyValuePair<Guid, MapAnimation> anim in LocalAnimations)
                {
                    if (anim.Value.Disposed())
                    {
                        LocalAnimations.TryRemove(anim.Key, out MapAnimation removed);
                    }
                    else
                    {
                        anim.Value.Update();
                    }
                }

                foreach (KeyValuePair<Guid, Entity> en in LocalEntities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    en.Value.Update();
                }

                foreach (KeyValuePair<MapAttribute, Entity> critter in mAttributeCritterInstances)
                {
                    if (critter.Value == null)
                    {
                        continue;
                    }

                    critter.Value.Update();
                }

                for (int i = 0; i < LocalEntitiesToDispose.Count; i++)
                {
                    LocalEntities.Remove(LocalEntitiesToDispose[i]);
                }

                LocalEntitiesToDispose.Clear();
            }
            else
            {
                if (Globals.System.GetTimeMs() > mLastUpdateTime)
                {
                    Dispose();
                }

                HideActiveAnimations();
            }
        }

        public bool InView()
        {
            MapInstance myMap = Globals.Me.MapInstance;
            if (Globals.MapGridWidth == 0 || Globals.MapGridHeight == 0 || myMap == null)
            {
                return true;
            }

            int gridX = myMap.MapGridX;
            int gridY = myMap.MapGridY;
            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (x >= 0 && x < Globals.MapGridWidth && y >= 0 && y < Globals.MapGridHeight)
                    {
                        if (Globals.MapGrid[x, y] == Id)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void HandleMapLoaded(MapInstance map)
        {
            //See if this new map is on the same grid as us
            if (map != this && Globals.GridMaps.Contains(map.Id) && Globals.GridMaps.Contains(Id) && MapLoaded)
            {
                //UnityEngine.Debug.Log($"{nameof(HandleMapLoaded)} map Loaded: {map.Name} current: {Name}");
                if (map.MapGridX == MapGridX - 1)
                {
                    if (map.MapGridY == MapGridY - 1)
                    {
                        //Check Northwest
                        foreach (string l in Options.Instance.MapOpts.Layers.All)
                        {
                            map.mapRenderer.SetBorderTile(Options.MapWidth, Options.MapHeight, l, Layers[l][0, 0]);
                        }
                    }
                    else if (map.MapGridY == MapGridY)
                    {
                        //Check West
                        for (int y = 0; y < Options.MapHeight; y++)
                        {
                            foreach (string l in Options.Instance.MapOpts.Layers.All)
                            {
                                map.mapRenderer.SetBorderTile(Options.MapWidth, y, l, Layers[l][0, y]);
                            }
                        }
                    }
                    else if (map.MapGridY == MapGridY + 1)
                    {
                        //Check Southwest
                        foreach (string l in Options.Instance.MapOpts.Layers.All)
                        {
                            map.mapRenderer.SetBorderTile(Options.MapWidth, -1, l, Layers[l][0, Options.MapHeight - 1]);
                        }
                    }
                }
                else if (map.MapGridX == MapGridX)
                {
                    if (map.MapGridY == MapGridY - 1)
                    {
                        //Check North
                        for (int x = 0; x < Options.MapWidth; x++)
                        {
                            foreach (string l in Options.Instance.MapOpts.Layers.All)
                            {
                                map.mapRenderer.SetBorderTile(x, Options.MapHeight, l, Layers[l][x, 0]);
                            }
                        }
                    }
                    else if (map.MapGridY == MapGridY + 1)
                    {
                        //Check South
                        for (int x = 0; x < Options.MapWidth; x++)
                        {
                            foreach (string l in Options.Instance.MapOpts.Layers.All)
                            {
                                map.mapRenderer.SetBorderTile(x, -1, l, Layers[l][x, Options.MapHeight - 1]);
                            }
                        }
                    }
                }
                else if (map.MapGridX == MapGridX + 1)
                {
                    if (map.MapGridY == MapGridY - 1)
                    {
                        //Check Northeast
                        foreach (string l in Options.Instance.MapOpts.Layers.All)
                        {
                            map.mapRenderer.SetBorderTile(-1, Options.MapHeight, l, Layers[l][Options.MapWidth - 1, 0]);
                        }
                    }
                    else if (map.MapGridY == MapGridY)
                    {
                        //Check East
                        for (int y = 0; y < Options.MapHeight; y++)
                        {
                            foreach (string l in Options.Instance.MapOpts.Layers.All)
                            {
                                map.mapRenderer.SetBorderTile(-1, y, l, Layers[l][Options.MapWidth - 1, y]);
                            }
                        }
                    }
                    else if (map.MapGridY == MapGridY + 1)
                    {
                        //Check Southeast
                        foreach (string l in Options.Instance.MapOpts.Layers.All)
                        {
                            map.mapRenderer.SetBorderTile(-1, -1, l, Layers[l][Options.MapWidth - 1, Options.MapHeight - 1]);
                        }
                    }
                }
            }
        }

        //private GameTileBuffer[] CheckAutotile(int x, int y, MapBase[,] surroundingMaps) {
        //	List<GameTileBuffer> updated = new List<GameTileBuffer>();
        //	for (int layer = 0; layer < 5; layer++) {
        //		if (Autotiles.UpdateAutoTile(x, y, layer, surroundingMaps)) {
        //			//Find the VBO, update it.
        //			Dictionary<object, GameTileBuffer[]> tileBuffer = mTileBufferDict[layer];
        //			Tile tile = Layers[layer].Tiles[x, y];
        //			if (tile.TilesetTex == null) {
        //				continue;
        //			}

        //			GameTexture tilesetTex = (GameTexture)tile.TilesetTex;
        //			if (tile.X < 0 || tile.Y < 0) {
        //				continue;
        //			}

        //			if (tile.X * Options.TileWidth >= tilesetTex.GetWidth() ||
        //				tile.Y * Options.TileHeight >= tilesetTex.GetHeight()) {
        //				continue;
        //			}

        //			object platformTex = tilesetTex.GetTexture();
        //			if (tileBuffer.ContainsKey(platformTex)) {
        //				for (int autotileFrame = 0; autotileFrame < 3; autotileFrame++) {
        //					GameTileBuffer buffer = tileBuffer[platformTex][autotileFrame];
        //					float xoffset = GetX();
        //					float yoffset = GetY();
        //					DrawAutoTile(
        //						layer, x * Options.TileWidth + xoffset, y * Options.TileHeight + yoffset, 1, x, y,
        //						autotileFrame, tilesetTex, buffer, true
        //					); //Top Left

        //					DrawAutoTile(
        //						layer, x * Options.TileWidth + Options.TileWidth / 2 + xoffset,
        //						y * Options.TileHeight + yoffset, 2, x, y, autotileFrame, tilesetTex, buffer, true
        //					);

        //					DrawAutoTile(
        //						layer, x * Options.TileWidth + xoffset,
        //						y * Options.TileHeight + Options.TileHeight / 2 + yoffset, 3, x, y, autotileFrame,
        //						tilesetTex, buffer, true
        //					);

        //					DrawAutoTile(
        //						layer, +x * Options.TileWidth + Options.TileWidth / 2 + xoffset,
        //						y * Options.TileHeight + Options.TileHeight / 2 + yoffset, 4, x, y, autotileFrame,
        //						tilesetTex, buffer, true
        //					);

        //					if (!updated.Contains(buffer)) {
        //						updated.Add(buffer);
        //					}
        //				}
        //			}
        //		}
        //	}

        //	return updated.ToArray();
        //}

        //Helper Functions
        private MapRenderer[,] GenerateAutotileGridRenderer()
        {
            MapRenderer[,] mapBase = new MapRenderer[3, 3];
            if (Globals.MapGrid != null && Globals.GridMaps.Contains(Id))
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int x1 = MapGridX + x;
                        int y1 = MapGridY + y;
                        if (x1 >= 0 && y1 >= 0 && x1 < Globals.MapGridWidth && y1 < Globals.MapGridHeight)
                        {
                            if (x == 0 && y == 0)
                            {
                                mapBase[x + 1, y + 1] = mapRenderer;
                            }
                            else
                            {
                                mapBase[x + 1, y + 1] = Lookup.Get<MapInstance>(Globals.MapGrid[x1, y1])?.mapRenderer;
                            }
                        }
                    }
                }
            }
            else
            {
                string message = "Returning null mapgrid for map " + Name;
                U.Debug.LogError(message);
                Log.Error(message);
            }

            return mapBase;
        }

        //Retreives the X Position of the Left side of the map in world space.
        public float GetX()
        {
            return MapGridX * Options.MapWidth;
        }

        //Retreives the Y Position of the Top side of the map in world space.
        public float GetY()
        {
            return MapGridY * Options.MapHeight;
        }

        private void UpdateMapAttributes()
        {
            int width = Options.MapWidth;
            int height = Options.MapHeight;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MapAttribute att = Attributes[x, y];
                    if (att == null)
                    {
                        continue;
                    }

                    if (att.Type == MapAttributes.Animation)
                    {
                        AnimationBase anim = AnimationBase.Get(((MapAnimationAttribute)att).AnimationId);
                        if (anim == null)
                        {
                            continue;
                        }

                        if (!mAttributeAnimInstances.ContainsKey(att))
                        {
                            Animation animInstance = new Animation(anim, true);
                            animInstance.SetPosition(
                                GetX() + x,
                                GetY() + y, x, y, Id, 0
                            );

                            mAttributeAnimInstances.Add(att, animInstance);
                        }

                        mAttributeAnimInstances[att].Update();
                    }


                    if (att.Type == MapAttributes.Critter)
                    {
                        MapCritterAttribute critterAttribute = ((MapCritterAttribute)att);
                        string sprite = critterAttribute.Sprite;
                        AnimationBase anim = AnimationBase.Get(critterAttribute.AnimationId);
                        if (anim == null && TextUtils.IsNone(sprite))
                        {
                            continue;
                        }

                        if (!mAttributeCritterInstances.ContainsKey(att))
                        {
                            Critter critter = new Critter(this, (byte)x, (byte)y, critterAttribute);
                            Critters.Add(critter.Id, critter);
                            mAttributeCritterInstances.Add(att, critter);
                        }

                        mAttributeCritterInstances[att].Update();
                    }
                }
            }
        }

        private void ClearMapAttributes()
        {
            foreach (KeyValuePair<MapAttribute, Animation> attributeInstance in mAttributeAnimInstances)
            {
                attributeInstance.Value.Dispose();
            }

            foreach (KeyValuePair<MapAttribute, Entity> critter in mAttributeCritterInstances)
            {
                critter.Value.Dispose();
            }

            Critters.Clear();
            mAttributeCritterInstances.Clear();
            mAttributeAnimInstances.Clear();
        }

        //Sound Functions
        public void CreateMapSounds()
        {
            ClearAttributeSounds();
            for (int x = 0; x < Options.MapWidth; ++x)
            {
                for (int y = 0; y < Options.MapHeight; ++y)
                {
                    MapAttribute attribute = Attributes?[x, y];
                    if (attribute?.Type != MapAttributes.Sound)
                    {
                        continue;
                    }

                    if (TextUtils.IsNone(((MapSoundAttribute)attribute).File))
                    {
                        continue;
                    }

                    MapSound sound = Audio.AddMapSound(
                        ((MapSoundAttribute)attribute).File, x, y, Id, true, ((MapSoundAttribute)attribute).LoopInterval, ((MapSoundAttribute)attribute).Distance
                    );

                    AttributeSounds?.Add(sound);
                }
            }
        }

        private void ClearAttributeSounds()
        {
            AttributeSounds?.ForEach(Audio.StopSound);
            AttributeSounds?.Clear();
        }

        public void AddTileAnimation(Guid animId, int tileX, int tileY, int dir = -1, Entity owner = null)
        {
            AnimationBase animBase = AnimationBase.Get(animId);
            if (animBase == null)
            {
                return;
            }

            MapAnimation anim = new MapAnimation(animBase, tileX, tileY, dir, owner);
            LocalAnimations.TryAdd(anim.Id, anim);
            anim.SetPosition(GetX() + tileX, GetY() + tileY, tileX, tileY, Id, dir);
        }

        private void HideActiveAnimations()
        {
            LocalEntities?.Values.ToList().ForEach(entity => entity?.ClearAnimations(null));
            foreach (KeyValuePair<Guid, MapAnimation> anim in LocalAnimations)
            {
                anim.Value?.Dispose();
            }
            LocalAnimations.Clear();
            ClearMapAttributes();
        }

        public void Draw()
        { //Lower, Middle, Upper

            if (!MapLoaded || !needsRender)
            {
                return;
            }

            CacheTextures();
            if (!mTexturesFound)
            {
                return;
            }
            mapRenderer.Render(Layers, GenerateAutotileGridRenderer());

            needsRender = false;
        }

        public void MapItemsPacket(MapItemsPacket packet)
        {
            MapItems.Clear();
            foreach (MapItemUpdatePacket item in packet.Items)
            {
                MapItemInstance mapItem = new MapItemInstance(item.TileIndex, item.Id, item.ItemId, item.BagId, item.Quantity, item.StatBuffs);

                if (!MapItems.TryGetValue(mapItem.TileIndex, out List<MapItemInstance> items))
                {
                    items = new List<MapItemInstance>();
                    MapItems.Add(mapItem.TileIndex, items);
                }
                items.Add(mapItem);

                if (!itemRenderers.ContainsKey(mapItem.UniqueId))
                {
                    itemRenderers.Add(mapItem.UniqueId, UnityFactory.GetItemRenderer(mapItem.Base.Name));
                }
            }
        }

        public void MapItemsAdd(MapItemInstance mapItem)
        {
            if (!MapItems.TryGetValue(mapItem.TileIndex, out List<MapItemInstance> items))
            {
                items = new List<MapItemInstance>();
                MapItems.Add(mapItem.TileIndex, items);
            }

            // Check if the item already exists, if it does replace it. Otherwise just add it.
            if (items.Any(item => item.UniqueId == mapItem.UniqueId))
            {
                for (int index = 0; index < items.Count; index++)
                {
                    if (items[index].UniqueId == mapItem.UniqueId)
                    {
                        items[index] = mapItem;
                    }
                }
            }
            else
            {
                mapItem.hasFallen = 1f;
                items.Add(mapItem);
            }

            if (!itemRenderers.ContainsKey(mapItem.UniqueId))
            {
                itemRenderers.Add(mapItem.UniqueId, UnityFactory.GetItemRenderer(mapItem.Base.Name));
            }
        }

        public void MapItemsRemove(Guid itemId)
        {
            // Find our item based on our unique Id and remove it.
            foreach (int location in MapItems.Keys)
            {
                MapItemInstance tempItem = MapItems[location].Where(item => item.UniqueId == itemId).SingleOrDefault();
                if (tempItem != null)
                {
                    MapItems[location].Remove(tempItem);
                    if (itemRenderers.TryGetValue(itemId, out ItemRenderer itemRenderer))
                    {
                        itemRenderer.Destroy();
                        itemRenderers.Remove(itemId);
                    }
                }
            }
        }

        public void DrawItemsAndLights()
        {
            //Draw Map Items
            foreach (KeyValuePair<int, List<MapItemInstance>> itemCollection in MapItems)
            {
                for (int i = itemCollection.Value.Count - 1; i >= 0; i--)
                {
                    MapItemInstance item = itemCollection.Value[i];
                    ItemBase itemBase = ItemBase.Get(item.ItemId);
                    if (itemBase != null)
                    {
                        GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, itemBase.Icon);
                        if (itemTex != null)
                        {
                            float x = GetX() + item.X;
                            float y = GetY() + item.Y;
                            if (item.hasFallen > 0f)
                            {
                                y -= item.hasFallen;
                                item.hasFallen -= 0.1f;
                            }
                            if (itemRenderers.TryGetValue(item.UniqueId, out ItemRenderer itemRenderer))
                            {
                                itemRenderer.Draw(itemTex.GetSpriteDefault(), x, y);
                            }
                        }
                    }
                }

            }

            for (int i = 0; i < lightRenderers.Count; i++)
            {
                lightRenderers[i].UpdateLight(Lights[i].Size, Lights[i].Intensity, Lights[i].Expand, Lights[i].Color.ToColor32());
            }

        }

        /// <summary>
        /// Draws all names of the items on the tile the user is hovering over.
        /// </summary>
        public void DrawItemNames()
        {
            //hide all items names
            foreach (ItemRenderer itemRenderer in itemRenderers.Values)
            {
                itemRenderer.HideName();
            }

            // Get where our mouse is located and convert it to a tile based location.
            Pointf mousePos = Globals.InputManager.GetMousePosition();
            int x = (int)(mousePos.X - (int)GetX());
            int y = (int)(mousePos.Y + 1 - (int)GetY());
            Guid mapId = Id;

            // Is this an actual location on this map?
            if (!Globals.Me.GetRealLocation(ref x, ref y, ref mapId))
            {
                return;
            }

            // Apparently it is! Do we have any items to render here?
            if (!MapItems.TryGetValue(y * Options.MapWidth + x, out List<MapItemInstance> tileItems))
            {
                return;
            }

            int baseOffset = 0;

            // Loop through this in reverse to match client/server display and pick-up order.
            for (int index = tileItems.Count - 1; index >= 0; index--)
            {
                baseOffset++;
                MapItemInstance mapItem = tileItems[index];
                if (!itemRenderers.TryGetValue(mapItem.UniqueId, out ItemRenderer itemRenderer))
                {
                    continue;
                }

                // Set up all information we need to draw this name.
                ItemBase itemBase = ItemBase.Get(mapItem.ItemId);
                string name = mapItem.Base.Name;
                int quantity = mapItem.Quantity;
                if (quantity > 1)
                {
                    name = Strings.General.MapItemStackable.ToString(name, Strings.FormatQuantityAbbreviated(quantity));
                }

                if (!CustomColors.Items.MapRarities.TryGetValue(itemBase.Rarity, out LabelColor color))
                {
                    color = new LabelColor(Color.White, Color.Black, new Color(100, 0, 0, 0));
                }

                itemRenderer.DrawName(name, color, baseOffset);

                //// Do we need to draw a background?
                //if (color.Background != Color.Transparent)
                //{
                //    Graphics.DrawGameTexture(
                //        Graphics.Renderer.GetWhiteTexture(), new FloatRect(0, 0, 1, 1),
                //        new FloatRect(destX - 4, destY, textSize.X + 8, textSize.Y), color.Background
                //    );
                //}


                // Finaly, draw the actual name!
                //Graphics.Renderer.DrawString(name, Graphics.EntityNameFont, destX, destY, 1, color.Name, true, null, color.Outline);

            }
        }
        //private void DrawAutoTile(
        //	int layerNum,
        //	float destX,
        //	float destY,
        //	int quarterNum,
        //	int x,
        //	int y,
        //	int forceFrame,
        //	GameTexture tileset,
        //	GameTileBuffer buffer,
        //	bool update = false
        //) {
        //	int yOffset = 0, xOffset = 0;

        //	// calculate the offset
        //	switch (Layers[layerNum].Tiles[x, y].Autotile) {
        //		case MapAutotiles.AUTOTILE_WATERFALL:
        //			yOffset = (forceFrame - 1) * Options.TileHeight;

        //			break;

        //		case MapAutotiles.AUTOTILE_ANIM:
        //			xOffset = forceFrame * Options.TileWidth * 2;

        //			break;

        //		case MapAutotiles.AUTOTILE_ANIM_XP:
        //			xOffset = forceFrame * Options.TileWidth * 3;

        //			break;

        //		case MapAutotiles.AUTOTILE_CLIFF:
        //			yOffset = -Options.TileHeight;

        //			break;
        //	}

        //	if (update) {
        //		if (!buffer.UpdateTile(
        //			tileset, destX, destY,
        //			Autotiles.Autotile[x, y].Layer[layerNum].QuarterTile[quarterNum].X + xOffset,
        //			Autotiles.Autotile[x, y].Layer[layerNum].QuarterTile[quarterNum].Y + yOffset,
        //			Options.TileWidth / 2, Options.TileHeight / 2
        //		)) {
        //			throw new Exception("Failed to update tile to VBO!");
        //		}
        //	} else {
        //		if (!buffer.AddTile(
        //			tileset, destX, destY,
        //			Autotiles.Autotile[x, y].Layer[layerNum].QuarterTile[quarterNum].X + xOffset,
        //			Autotiles.Autotile[x, y].Layer[layerNum].QuarterTile[quarterNum].Y + yOffset,
        //			Options.TileWidth / 2, Options.TileHeight / 2
        //		)) {
        //			throw new Exception("Failed to add tile to VBO!");
        //		}
        //	}
        //}

        //Fogs/Panorama/Overlay
        public void DrawFog()
        {
            if (Globals.Me == null || Lookup.Get(Globals.Me.CurrentMap) == null)
            {
                return;
            }

            float ecTime = Globals.System.GetTimeMs() - mFogUpdateTime;
            mFogUpdateTime = Globals.System.GetTimeMs();

            if (Id == Globals.Me.CurrentMap)
            {
                if (mCurFogIntensity != 1)
                {
                    if (mCurFogIntensity < 1)
                    {
                        mCurFogIntensity += ecTime / 2000f;
                        if (mCurFogIntensity > 1)
                        {
                            mCurFogIntensity = 1;
                        }
                    }
                    else
                    {
                        mCurFogIntensity -= ecTime / 2000f;
                        if (mCurFogIntensity < 1)
                        {
                            mCurFogIntensity = 1;
                        }
                    }
                }

            }
            else
            {
                if (mCurFogIntensity != 0)
                {
                    mCurFogIntensity -= ecTime / 2000f;
                    if (mCurFogIntensity < 0)
                    {
                        mCurFogIntensity = 0;
                    }
                }
            }


            if (!string.IsNullOrEmpty(Fog))
            {
                GameTexture fogTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Fog, Fog);
                if (fogTex != null)
                {
                    mFogCurrentX -= ecTime / 1000f * FogXSpeed * -6;
                    mFogCurrentY -= ecTime / 1000f * FogYSpeed * -2;

                    int width = fogTex.Width;
                    if (mFogCurrentX < 0)
                    {
                        mFogCurrentX += width;
                    }

                    if (mFogCurrentX > width)
                    {
                        mFogCurrentX -= width;
                    }

                    int height = fogTex.Height;
                    if (mFogCurrentY < 0)
                    {
                        mFogCurrentY += height;
                    }
                    if (mFogCurrentY > height)
                    {
                        mFogCurrentY -= height;
                    }


                    byte alpha = (byte)(FogTransparency * mCurFogIntensity);
                    mapRenderer.fogRenderer.ChangeFog(fogTex.GetSpriteDefault(), alpha);

                    mapRenderer.fogRenderer.ChangePosition(mFogCurrentX - width / 2, mFogCurrentY - height / 2, GetX() + Options.MapWidth / 2, GetY() + Options.MapHeight / 2);
                }
            }
        }

        //Weather
        public void DrawWeather()
        {
            if (Globals.Me == null || Lookup.Get(Globals.Me.CurrentMap) == null)
            {
                return;
            }

            AnimationBase anim = AnimationBase.Get(WeatherAnimationId);

            if (anim == null || WeatherIntensity == 0)
            {
                return;
            }

            _removeParticles.Clear();

            if ((WeatherXSpeed != 0 || WeatherYSpeed != 0) && Globals.Me.MapInstance == this)
            {
                if (Globals.System.GetTimeMs() > _weatherParticleSpawnTime)
                {
                    _weatherParticles.Add(new WeatherParticle(_removeParticles, WeatherXSpeed, WeatherYSpeed, anim));
                    int spawnTime = 25 + (int)(475 * (float)(1f - (float)(WeatherIntensity / 100f)));
                    spawnTime = (int)(spawnTime * (480000f / (Graphics.Renderer.GetScreenWidth() * Graphics.Renderer.GetScreenHeight())));

                    _weatherParticleSpawnTime = Globals.System.GetTimeMs() + spawnTime;
                }
            }

            //Process and draw each weather particle
            foreach (WeatherParticle w in _weatherParticles)
            {
                w.Update();
            }

            //Remove all old particles from the weather particles list from the removeparticles list.
            foreach (WeatherParticle r in _removeParticles)
            {
                r.Dispose();
                _weatherParticles.Remove(r);
            }
        }

        private void ClearWeather()
        {
            foreach (WeatherParticle r in _weatherParticles)
            {
                r.Dispose();
            }

            _weatherParticles.Clear();
        }

        public void GridSwitched()
        {
            mPanoramaIntensity = 1f;
            mCurFogIntensity = 1f;
        }

        internal void DrawPanorama()
        {
            float ecTime = Globals.System.GetTimeMs() - mPanoramaUpdateTime;
            mPanoramaUpdateTime = Globals.System.GetTimeMs();
            if (Id == Globals.Me.CurrentMap)
            {
                if (mPanoramaIntensity != 1)
                {
                    mPanoramaIntensity += ecTime / 2000f;
                    if (mPanoramaIntensity > 1)
                    {
                        mPanoramaIntensity = 1;
                    }
                }
            }
            else
            {
                if (mPanoramaIntensity != 0)
                {
                    mPanoramaIntensity -= ecTime / 2000f;
                    if (mPanoramaIntensity < 0)
                    {
                        mPanoramaIntensity = 0;
                    }
                }
            }

            GameTexture imageTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Image, Panorama);
            if (imageTex != null)
            {
                //TODO: ver como combiene dibujar el panorama
                //Graphics.DrawFullScreenTexture(imageTex, mPanoramaIntensity);
            }
        }

        public void DrawOverlayGraphic()
        {
            float ecTime = Globals.System.GetTimeMs() - mOverlayUpdateTime;
            mOverlayUpdateTime = Globals.System.GetTimeMs();
            if (Id == Globals.Me.CurrentMap)
            {
                if (mOverlayIntensity != 1)
                {
                    mOverlayIntensity += ecTime / 2000f;
                    if (mOverlayIntensity > 1)
                    {
                        mOverlayIntensity = 1;
                    }
                }
            }
            else
            {
                if (mOverlayIntensity != 0)
                {
                    mOverlayIntensity -= ecTime / 2000f;
                    if (mOverlayIntensity < 0)
                    {
                        mOverlayIntensity = 0;
                    }
                }
            }

            GameTexture imageTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Image, OverlayGraphic);
            if (imageTex != null)
            {
                mapRenderer.overlayRenderer.Draw(imageTex.GetSpriteDefault(), mOverlayIntensity);
            }
        }

        public void CompareEffects(MapInstance oldMap)
        {
            //Check if fogs the same
            if (oldMap.Fog == Fog)
            {
                GameTexture fogTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Fog, Fog);
                if (fogTex != null)
                {
                    //Copy over fog values
                    mFogUpdateTime = oldMap.mFogUpdateTime;
                    float ratio = (float)oldMap.FogTransparency / FogTransparency;
                    mCurFogIntensity = ratio * oldMap.mCurFogIntensity;
                    mFogCurrentX = oldMap.mFogCurrentX;
                    mFogCurrentY = oldMap.mFogCurrentY;

                    if (GetX() > oldMap.GetX())
                    {
                        mFogCurrentX -= Options.TileWidth * Options.MapWidth % fogTex.Width;
                    }
                    else if (GetX() < oldMap.GetX())
                    {
                        mFogCurrentX += Options.TileWidth * Options.MapWidth % fogTex.Width;
                    }

                    if (GetY() > oldMap.GetY())
                    {
                        mFogCurrentY -= Options.TileWidth * Options.MapHeight % fogTex.Height;
                    }
                    else if (GetY() < oldMap.GetY())
                    {
                        mFogCurrentY += Options.TileWidth * Options.MapHeight % fogTex.Height;
                    }

                    oldMap.mCurFogIntensity = 0;
                }
            }

            if (oldMap.Panorama == Panorama)
            {
                mPanoramaIntensity = oldMap.mPanoramaIntensity;
                mPanoramaUpdateTime = oldMap.mPanoramaUpdateTime;
                oldMap.mPanoramaIntensity = 0;
            }

            if (oldMap.OverlayGraphic == OverlayGraphic)
            {
                mOverlayIntensity = oldMap.mOverlayIntensity;
                mOverlayUpdateTime = oldMap.mOverlayUpdateTime;
                oldMap.mOverlayIntensity = 0;
            }
        }

        public void DrawActionMsgs()
        {
            for (int n = ActionMsgs.Count - 1; n > -1; n--)
            {
                if (!ActionMsgs[n].Draw(GetX(), GetY()))
                {
                    //Try to remove
                    ActionMsgs[n].Remove();
                    ActionMsgs.RemoveAt(n);
                }
            }
        }

        //Events
        public void AddEvent(Guid evtId, EventEntityPacket packet)
        {
            if (MapLoaded)
            {
                if (LocalEntities.ContainsKey(evtId))
                {
                    LocalEntities[evtId].Load(packet);
                }
                else
                {
                    Event evt = new Event(evtId, packet);
                    LocalEntities.Add(evtId, evt);
                    mEvents.Add(evt);
                }
            }
        }


        public override void Delete()
        {
            if (Lookup != null)
            {
                Lookup.Delete(this);
            }
        }

        private void DisposeLights()
        {
            foreach (LightRenderer lightRenderer in lightRenderers)
            {
                lightRenderer.Destroy();
            }
            lightRenderers.Clear();
        }

        private void DisposeActionMessages()
        {
            foreach (ActionMessage actionMessage in ActionMsgs)
            {
                actionMessage.Remove();
            }
            ActionMsgs.Clear();
        }

        private void DisposeItemRenderers()
        {
            foreach (ItemRenderer itemRenderer in itemRenderers.Values)
            {
                itemRenderer.Destroy();
            }
            itemRenderers.Clear();
        }

        //Dispose
        public void Dispose(bool prep = true, bool killentities = true)
        {
            MapLoaded = false;
            OnMapLoaded -= HandleMapLoaded;

            foreach (Event evt in mEvents)
            {
                evt.Dispose();
            }

            mEvents.Clear();

            if (killentities)
            {
                foreach (KeyValuePair<Guid, Entity> en in Globals.Entities)
                {
                    if (en.Value.CurrentMap == Id)
                    {
                        Globals.EntitiesToDispose.Add(en.Key);
                    }
                }

                foreach (KeyValuePair<Guid, Entity> en in LocalEntities)
                {
                    en.Value.Dispose();
                }
            }
            mapRenderer.Destroy();
            mapRenderer = null;
            DisposeActionMessages();
            DisposeItemRenderers();
            DisposeLights();
            HideActiveAnimations();
            ClearWeather();
            ClearMapAttributes();
            ClearAttributeSounds();
            Delete();
        }
    }
}