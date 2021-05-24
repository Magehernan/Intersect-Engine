using System;
using System.Collections.Generic;
using System.Linq;
using Intersect.Client.Core;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Maps;
using Intersect.Enums;
using Intersect.GameObjects.Events;
using Intersect.Network.Packets.Server;

namespace Intersect.Client.Entities.Events
{

    public class Event : Entity
    {
        private GameTexture entityTex;

        public string Desc = string.Empty;

        public bool DirectionFix;

        public bool DisablePreview;

        public string FaceGraphic = string.Empty;

        public EventGraphic Graphic = new EventGraphic();

        public int Layer;

        private GameTexture mCachedTileset;

        private string mCachedTilesetName;

        private int mOldRenderLevel;

        private MapInstance mOldRenderMap;

        private int mOldRenderY;

        public int RenderLevel = 1;

        public bool WalkingAnim = true;

        public Event(Guid id, EventEntityPacket packet) : base(id, packet, true)
        {
            mRenderPriority = 1;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void Load(EntityPacket packet)
        {
            base.Load(packet);
            EventEntityPacket pkt = (EventEntityPacket)packet;
            DirectionFix = pkt.DirectionFix;
            WalkingAnim = pkt.WalkingAnim;
            DisablePreview = pkt.DisablePreview;
            Desc = pkt.Description;
            Graphic = pkt.Graphic;
            RenderLevel = pkt.RenderLayer;
        }

        public override EntityTypes GetEntityType()
        {
            return EntityTypes.Event;
        }

        public override bool Update()
        {
            bool success = base.Update();
            if (!WalkingAnim)
            {
                WalkFrame = 0;
            }

            return success;
        }

        public override void Draw()
        {
            if (HideEntity)
            {
                if (entityRender != null)
                {
                    entityRender.HideAll();
                }


                return; //Don't draw if the entity is hidden
            }

            DrawName(null);

            WorldRect.Reset();
            if (MapInstance.Get(CurrentMap) == null || !Globals.GridMaps.Contains(CurrentMap))
            {
                return;
            }

            int d;
            switch (Graphic.Type)
            {
                case EventGraphicType.Sprite: //Sprite

                    if (entityTex is null)
                    {
                        entityTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, Graphic.Filename);
                    }

                    if (entityTex != null)
                    {
                        d = Graphic.Y;
                        if (!DirectionFix)
                        {
                            switch (Dir)
                            {
                                case 0:
                                    d = 3;
                                    break;
                                case 1:
                                    d = 0;
                                    break;
                                case 2:
                                    d = 1;
                                    break;
                                case 3:
                                    d = 2;
                                    break;
                            }
                        }

                        int spriteX;
                        int spriteY = d;


                        if (Options.AnimatedSprites.Contains(Graphic.Filename, StringComparer.OrdinalIgnoreCase))
                        {
                            spriteX = AnimationFrame;
                        }
                        else
                        {
                            if (WalkingAnim)
                            {
                                spriteX = WalkFrame;
                            }
                            else
                            {
                                spriteX = Graphic.X;
                            }
                        }
                        entityRender.Draw(0, entityTex.GetSprite(spriteX, spriteY), 255);
                        entityRender.SetHeight(entityTex.SpriteHeight);
                    }
                    break;
                case EventGraphicType.Tileset: //Tile
                    if (mCachedTilesetName != Graphic.Filename)
                    {
                        mCachedTilesetName = Graphic.Filename;
                        mCachedTileset = Globals.ContentManager.GetTexture(
                            GameContentManager.TextureType.Tileset, Graphic.Filename
                        );
                    }

                    if (mCachedTileset != null)
                    {
                        entityRender.Draw(0, mCachedTileset.GetSprite(Graphic.X, Graphic.Y), 255);
                        entityRender.SetHeight(mCachedTileset.SpriteHeight);
                    }
                    break;
            }

            //if (height > Options.TileHeight) {
            //	destRectangle.Y = map.GetY() + Y * Options.TileHeight + OffsetY - (height - Options.TileHeight);
            //} else {
            //	destRectangle.Y = map.GetY() + Y * Options.TileHeight + OffsetY;
            //}

            //if (width > Options.TileWidth) {
            //	destRectangle.X -= (width - Options.TileWidth) / 2;
            //}

            entityRender.SetPosition(worldPos.X, worldPos.Y);


            float width = entityTex != null ? entityTex.SpriteWidth / entityTex.PixelPerUnits : 1;
            float height = entityTex != null ? entityTex.SpriteHeight / entityTex.PixelPerUnits : 1;
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            WorldRect.X = worldPos.X + .5f - width * .5f;
            WorldRect.Y = worldPos.Y - height;
            WorldRect.Width = width;
            WorldRect.Height = height;

            Utils.Draw.Rectangle(WorldRect, Color.White);
        }

        public override HashSet<Entity> DetermineRenderOrder(HashSet<Entity> renderList, MapInstance map)
        {
            if (RenderLevel == 1)
            {
                return base.DetermineRenderOrder(renderList, map);
            }

            renderList?.Remove(this);
            if (map == null || Globals.Me == null || Globals.Me.MapInstance == null)
            {
                return null;
            }

            int gridX = Globals.Me.MapInstance.MapGridX;
            int gridY = Globals.Me.MapInstance.MapGridY;
            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (x >= 0 &&
                        x < Globals.MapGridWidth &&
                        y >= 0 &&
                        y < Globals.MapGridHeight &&
                        Globals.MapGrid[x, y] != Guid.Empty)
                    {
                        if (Globals.MapGrid[x, y] == CurrentMap)
                        {
                            if (RenderLevel == 0)
                            {
                                y--;
                            }

                            if (RenderLevel == 2)
                            {
                                y++;
                            }

                            byte priority = mRenderPriority;
                            if (Z != 0)
                            {
                                priority += 3;
                            }

                            HashSet<Entity> renderSet = null;

                            if (y == gridY - 2)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Y];
                            }
                            else if (y == gridY - 1)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight + Y];
                            }
                            else if (y == gridY)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight * 2 + Y];
                            }
                            else if (y == gridY + 1)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight * 3 + Y];
                            }
                            else if (y == gridY + 2)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight * 4 + Y];
                            }

                            renderSet?.Add(this);
                            renderList = renderSet;

                            return renderList;
                        }
                    }
                }
            }

            return renderList;
        }

        public override void DrawName(Color textColor, Color borderColor = null, Color backgroundColor = null)
        {
            if (HideName || string.IsNullOrWhiteSpace(Name))
            {
                entityRender.HideName();
                return;
            }

            if (MapInstance.Get(CurrentMap) == null || !Globals.GridMaps.Contains(CurrentMap))
            {
                entityRender.HideName();
                return;
            }

            entityRender.DrawName(Name, CustomColors.Names.Events.Name);
        }

        protected override void CalculateWorldPos()
        {
            MapInstance map = MapInstance.Get(CurrentMap);
            if (map == null)
            {
                worldPos = Pointf.Empty;
                return;
            }

            Pointf pos = new Pointf(
                map.GetX() + X + OffsetX,
                map.GetY() + Y + OffsetY
            );

            switch (Graphic.Type)
            {
                case EventGraphicType.Sprite: //Sprite
                                              //GameTexture entityTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, MySprite);
                                              //if (entityTex != null) {
                                              //	pos.Y += Options.TileHeight / 2;
                                              //	pos.Y -= entityTex.GetHeight() / 4 / 2;
                                              //}

                    break;
                case EventGraphicType.Tileset: //Tile
                    if (mCachedTilesetName != Graphic.Filename)
                    {
                        mCachedTilesetName = Graphic.Filename;
                        mCachedTileset = Globals.ContentManager.GetTexture(
                            GameContentManager.TextureType.Tileset, Graphic.Filename
                        );
                    }

                    if (mCachedTileset != null)
                    {
                        pos.Y += .5f;
                        pos.Y -= (Graphic.Height + 1) * .5f;
                    }

                    break;
            }

            worldPos = pos;
        }

        ~Event()
        {
            Dispose();
        }

    }

}
