using Intersect.Client.Entities;
using Intersect.Client.Entities.Events;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Maps;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Client.Utils;
using Intersect.Enums;
using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.Universal;
using U = UnityEngine;

namespace Intersect.Client.Core
{
    public static class Graphics
    {
        public static object AnimationLock = new object();

        //Darkness Stuff
        public static byte BrightnessLevel;

        public static FloatRect CurrentView;

        public static GameShader DefaultShader;

        //Rendering Variables
        public static int DrawCalls;

        public static int EntitiesDrawn;

        public static object GfxLock = new object();

        //Grid Switched
        public static bool GridSwitched;

        //Animations
        public static List<Animation> LiveAnimations = new List<Animation>();

        public static int MapsDrawn;

        //Overlay Stuff
        public static Color OverlayColor = Color.Transparent;

        public static ColorF PlayerLightColor = ColorF.White;

        //Game Renderer
        public static GameRenderer Renderer;

        //Cache the Y based rendering
        public static HashSet<Entity>[,] RenderingEntities;

        private static GameContentManager sContentManager;

        private static long sFadeTimer;

        private static long sLightUpdate;

        private static int sOldHeight;

        //Resolution
        private static int sOldWidth;

        private static long sOverlayUpdate;

        private static float sPlayerLightExpand;

        private static float sPlayerLightIntensity = 255;

        private static float sPlayerLightSize;

        public static U.Camera mainCamera;
        private static Light2D sunLight;

        public static U.Color32 SunLightColor => sunLight.color;

        public static FogRenderer FogRenderer { get; internal set; }

        #region const
        public const int LOWER_LAYERS = 0;
        public const int MIDDLE_LAYERS = 100;
        public const int UPPER_LAYERS = 200;
        #endregion

        //Init Functions
        public static void InitGraphics(Light2D sunLight)
        {
            mainCamera = U.Camera.main;
            Renderer.Init();
            Graphics.sunLight = sunLight;
            if (mainCamera is null)
            {
                throw new ArgumentNullException(nameof(mainCamera));
            }
            sContentManager = Globals.ContentManager;
            sContentManager.LoadAll();
        }

        public static void InitInGame()
        {
            RenderingEntities = new HashSet<Entity>[6, Options.MapHeight * 5];
            for (int z = 0; z < 6; z++)
            {
                for (int i = 0; i < Options.MapHeight * 5; i++)
                {
                    RenderingEntities[z, i] = new HashSet<Entity>();
                }
            }
        }

        public static void DrawInGame()
        {
            MapInstance currentMap = Globals.Me.MapInstance;
            if (currentMap == null)
            {
                return;
            }

            if (Globals.NeedsMaps)
            {
                return;
            }

            if (GridSwitched)
            {
                //Brightness
                BrightnessLevel = (byte)(currentMap.Brightness / 100f * 255);
                PlayerLightColor.R = currentMap.PlayerLightColor.R;
                PlayerLightColor.G = currentMap.PlayerLightColor.G;
                PlayerLightColor.B = currentMap.PlayerLightColor.B;
                sPlayerLightSize = currentMap.PlayerLightSize;
                sPlayerLightIntensity = currentMap.PlayerLightIntensity;
                sPlayerLightExpand = currentMap.PlayerLightExpand;

                //Overlay
                OverlayColor.A = (byte)currentMap.AHue;
                OverlayColor.R = (byte)currentMap.RHue;
                OverlayColor.G = (byte)currentMap.GHue;
                OverlayColor.B = (byte)currentMap.BHue;

                //Fog && Panorama
                currentMap.GridSwitched();
                GridSwitched = false;
            }

            Animation[] animations = LiveAnimations.ToArray();
            foreach (Animation animInstance in animations)
            {
                if (animInstance.ParentGone())
                {
                    animInstance.Dispose();
                }
            }

            int gridX = currentMap.MapGridX;
            int gridY = currentMap.MapGridY;


            //Draw Panoramas First...
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
                        DrawMapPanorama(Globals.MapGrid[x, y]);
                    }
                }
            }

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
                        DrawMap(Globals.MapGrid[x, y]);
                    }
                }
            }

            for (int y = 0; y < Options.MapHeight * 5; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    foreach (Entity entity in RenderingEntities[x, y])
                    {
                        entity.Draw();
                        EntitiesDrawn++;
                    }

                    if (x == 0 && y > 0 && y % Options.MapHeight == 0)
                    {
                        for (int x1 = gridX - 1; x1 <= gridX + 1; x1++)
                        {
                            int y1 = gridY - 2 + (int)Math.Floor(y / (float)Options.MapHeight);
                            if (x1 >= 0 &&
                                x1 < Globals.MapGridWidth &&
                                y1 >= 0 &&
                                y1 < Globals.MapGridHeight &&
                                Globals.MapGrid[x1, y1] != Guid.Empty)
                            {
                                MapInstance map = MapInstance.Get(Globals.MapGrid[x1, y1]);
                                if (map != null)
                                {
                                    map.DrawItemsAndLights();
                                }
                            }
                        }
                    }
                }
            }

            foreach (Animation animInstance in animations)
            {
                animInstance.Draw(false);
                animInstance.Draw(true);
            }


            for (int y = 0; y < Options.MapHeight * 5; y++)
            {
                for (int x = 3; x < 6; x++)
                {
                    foreach (Entity entity in RenderingEntities[x, y])
                    {
                        entity.Draw();
                        EntitiesDrawn++;
                    }
                }
            }

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
                        MapInstance map = MapInstance.Get(Globals.MapGrid[x, y]);
                        if (map != null)
                        {
                            map.DrawWeather();
                            map.DrawFog();
                            map.DrawOverlayGraphic();
                            map.DrawItemNames();
                        }
                    }
                }
            }

            //Draw the players targets
            Globals.Me.DrawTargets();

            DrawOverlay();

            DrawSunLight();

            for (int y = 0; y < Options.MapHeight * 5; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    foreach (Entity entity in RenderingEntities[x, y])
                    {
                        entity.DrawName(null);
                        if (entity.GetType() != typeof(Event))
                        {
                            entity.DrawHpBar();
                            entity.DrawCastingBar();
                        }
                    }
                }
            }

            for (int y = 0; y < Options.MapHeight * 5; y++)
            {
                for (int x = 3; x < 6; x++)
                {
                    foreach (Entity entity in RenderingEntities[x, y])
                    {
                        entity.DrawName(null);
                        if (entity.GetType() != typeof(Event))
                        {
                            entity.DrawHpBar();
                            entity.DrawCastingBar();
                        }
                    }
                }
            }

            //Draw action msg's
            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (x < 0 ||
                        x >= Globals.MapGridWidth ||
                        y < 0 ||
                        y >= Globals.MapGridHeight ||
                        Globals.MapGrid[x, y] == Guid.Empty)
                    {
                        continue;
                    }

                    MapInstance map = MapInstance.Get(Globals.MapGrid[x, y]);
                    map?.DrawActionMsgs();
                }
            }

            foreach (Animation animInstance in animations)
            {
                animInstance.EndDraw();
            }
        }

        //Game Rendering
        public static void Render()
        {
            if (Renderer.GetScreenWidth() != sOldWidth ||
                Renderer.GetScreenHeight() != sOldHeight ||
                Renderer.DisplayModeChanged())
            {
                sOldWidth = Renderer.GetScreenWidth();
                sOldHeight = Renderer.GetScreenHeight();
            }

            DrawCalls = 0;
            MapsDrawn = 0;
            EntitiesDrawn = 0;

            UpdateView();

            switch (Globals.GameState)
            {
                case GameStates.Intro:
                    break;
                case GameStates.Menu:
                    break;
                case GameStates.Loading:
                    break;
                case GameStates.InGame:
                    DrawInGame();
                    break;
                case GameStates.Error:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Interface.Interface.DrawGui();
        }

        private static void DrawMap(Guid mapId)
        {
            MapInstance map = MapInstance.Get(mapId);
            if (map == null)
            {
                return;
            }

            FloatRect floatRect = new FloatRect(map.GetX(), map.GetY(), Options.MapWidth, Options.MapHeight);
            if (!floatRect.IntersectsWith(CurrentView))
            {
                return;
            }

            map.Draw();
            MapsDrawn++;
        }

        private static void DrawMapPanorama(Guid mapId)
        {
            MapInstance map = MapInstance.Get(mapId);
            if (map != null)
            {
                if (!new FloatRect(
                    map.GetX(), map.GetY(), Options.TileWidth * Options.MapWidth, Options.TileHeight * Options.MapHeight
                ).IntersectsWith(CurrentView))
                {
                    return;
                }

                map.DrawPanorama();
            }
        }
        public static void DrawOverlay()
        {
            MapInstance map = MapInstance.Get(Globals.Me.CurrentMap);
            if (map != null)
            {
                float ecTime = Globals.System.GetTimeMs() - sOverlayUpdate;

                if (OverlayColor.A != map.AHue ||
                    OverlayColor.R != map.RHue ||
                    OverlayColor.G != map.GHue ||
                    OverlayColor.B != map.BHue)
                {
                    if (OverlayColor.A < map.AHue)
                    {
                        if (OverlayColor.A + (int)(255 * ecTime / 2000f) > map.AHue)
                        {
                            OverlayColor.A = (byte)map.AHue;
                        }
                        else
                        {
                            OverlayColor.A += (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.A > map.AHue)
                    {
                        if (OverlayColor.A - (int)(255 * ecTime / 2000f) < map.AHue)
                        {
                            OverlayColor.A = (byte)map.AHue;
                        }
                        else
                        {
                            OverlayColor.A -= (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.R < map.RHue)
                    {
                        if (OverlayColor.R + (int)(255 * ecTime / 2000f) > map.RHue)
                        {
                            OverlayColor.R = (byte)map.RHue;
                        }
                        else
                        {
                            OverlayColor.R += (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.R > map.RHue)
                    {
                        if (OverlayColor.R - (int)(255 * ecTime / 2000f) < map.RHue)
                        {
                            OverlayColor.R = (byte)map.RHue;
                        }
                        else
                        {
                            OverlayColor.R -= (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.G < map.GHue)
                    {
                        if (OverlayColor.G + (int)(255 * ecTime / 2000f) > map.GHue)
                        {
                            OverlayColor.G = (byte)map.GHue;
                        }
                        else
                        {
                            OverlayColor.G += (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.G > map.GHue)
                    {
                        if (OverlayColor.G - (int)(255 * ecTime / 2000f) < map.GHue)
                        {
                            OverlayColor.G = (byte)map.GHue;
                        }
                        else
                        {
                            OverlayColor.G -= (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.B < map.BHue)
                    {
                        if (OverlayColor.B + (int)(255 * ecTime / 2000f) > map.BHue)
                        {
                            OverlayColor.B = (byte)map.BHue;
                        }
                        else
                        {
                            OverlayColor.B += (byte)(255 * ecTime / 2000f);
                        }
                    }

                    if (OverlayColor.B > map.BHue)
                    {
                        if (OverlayColor.B - (int)(255 * ecTime / 2000f) < map.BHue)
                        {
                            OverlayColor.B = (byte)map.BHue;
                        }
                        else
                        {
                            OverlayColor.B -= (byte)(255 * ecTime / 2000f);
                        }
                    }
                }
            }

            Interface.Interface.GameUi.ChangeOverlayColor(OverlayColor);
            sOverlayUpdate = Globals.System.GetTimeMs();
        }

        public static FloatRect GetSourceRect(GameTexture gameTexture)
        {
            return gameTexture == null
                ? new FloatRect()
                : new FloatRect(0, 0, gameTexture.Width, gameTexture.Height);
        }

        private static void UpdateView()
        {
            if (Globals.Me == null)
            {
                CurrentView = new FloatRect(0, 0, Renderer.GetScreenWidth(), Renderer.GetScreenHeight());
                Renderer.SetView(CurrentView);
                return;
            }

            MapInstance map = MapInstance.Get(Globals.Me.CurrentMap);
            if (Globals.GameState == GameStates.InGame && map != null)
            {
                Player entity = Globals.Me;

                U.Bounds bounds = mainCamera.OrthographicBounds();

                float halfWidth = bounds.size.x * .5f;
                float halfHeight = bounds.size.y * .5f;
                //buscamos la posicion del player y sacamos el el rect de la pantalla
                Pointf centerPos = entity.GetCenterPos();
                CurrentView = new FloatRect(centerPos.X - halfWidth, centerPos.Y - halfHeight + 1, bounds.size.x, bounds.size.y);

                if (map.CameraHolds[(int)Directions.Left])
                {
                    float leftLimit = map.GetX();
                    if (leftLimit > CurrentView.X)
                    {
                        CurrentView.X = leftLimit;
                    }
                }

                if (map.CameraHolds[(int)Directions.Right])
                {
                    float rightLimit = map.GetX() + Options.MapWidth;
                    if (rightLimit < CurrentView.X + CurrentView.Width)
                    {
                        CurrentView.X = rightLimit - CurrentView.Width;
                    }
                }

                if (map.CameraHolds[(int)Directions.Up])
                {
                    float topLimit = map.GetY();
                    if (topLimit > CurrentView.Y)
                    {
                        CurrentView.Y = topLimit;
                    }
                }

                if (map.CameraHolds[(int)Directions.Down])
                {
                    float bottomLimit = map.GetY() + Options.MapHeight;
                    if (bottomLimit < CurrentView.Y + CurrentView.Height)
                    {
                        CurrentView.Y = bottomLimit - CurrentView.Height;
                    }
                }

                mainCamera.transform.position = new U.Vector3(CurrentView.X + halfWidth, -CurrentView.Y - halfHeight + 1, -5);
            }
            else
            {
                CurrentView = new FloatRect(0, 0, Renderer.GetScreenWidth(), Renderer.GetScreenHeight());
            }

            mainCamera.orthographicSize = Renderer.GetScreenHeight() / 64f;
            Renderer.SetView(CurrentView);
        }

        private static void DrawSunLight()
        {
            MapInstance map = MapInstance.Get(Globals.Me.CurrentMap);
            if (map == null)
            {
                return;
            }
            if (map.IsIndoors)
            {
                sunLight.color = new U.Color32(BrightnessLevel, BrightnessLevel, BrightnessLevel, 255);
            }
            else
            {
                U.Color32 sunColor = Time.GetTintColor();
                sunLight.color = U.Color32.LerpUnclamped(U.Color.white, sunColor, sunColor.a / 255f);
            }

            Globals.Me.DrawLight(sPlayerLightSize, sPlayerLightIntensity, sPlayerLightExpand, PlayerLightColor.ToColor32());
        }

        public static void UpdatePlayerLight()
        {
            //Draw Light Around Player
            MapInstance map = MapInstance.Get(Globals.Me.CurrentMap);
            if (map != null)
            {
                float ecTime = Globals.System.GetTimeMs() - sLightUpdate;
                float valChange = 255 * ecTime / 2000f;
                byte brightnessTarget = (byte)(map.Brightness / 100f * 255);
                if (BrightnessLevel < brightnessTarget)
                {
                    if (BrightnessLevel + valChange > brightnessTarget)
                    {
                        BrightnessLevel = brightnessTarget;
                    }
                    else
                    {
                        BrightnessLevel += (byte)valChange;
                    }
                }

                if (BrightnessLevel > brightnessTarget)
                {
                    if (BrightnessLevel - valChange < brightnessTarget)
                    {
                        BrightnessLevel = brightnessTarget;
                    }
                    else
                    {
                        BrightnessLevel -= (byte)valChange;
                    }
                }

                if (PlayerLightColor.R != map.PlayerLightColor.R ||
                    PlayerLightColor.G != map.PlayerLightColor.G ||
                    PlayerLightColor.B != map.PlayerLightColor.B)
                {
                    if (PlayerLightColor.R < map.PlayerLightColor.R)
                    {
                        if (PlayerLightColor.R + valChange > map.PlayerLightColor.R)
                        {
                            PlayerLightColor.R = map.PlayerLightColor.R;
                        }
                        else
                        {
                            PlayerLightColor.R += valChange;
                        }
                    }

                    if (PlayerLightColor.R > map.PlayerLightColor.R)
                    {
                        if (PlayerLightColor.R - valChange < map.PlayerLightColor.R)
                        {
                            PlayerLightColor.R = map.PlayerLightColor.R;
                        }
                        else
                        {
                            PlayerLightColor.R -= valChange;
                        }
                    }

                    if (PlayerLightColor.G < map.PlayerLightColor.G)
                    {
                        if (PlayerLightColor.G + valChange > map.PlayerLightColor.G)
                        {
                            PlayerLightColor.G = map.PlayerLightColor.G;
                        }
                        else
                        {
                            PlayerLightColor.G += valChange;
                        }
                    }

                    if (PlayerLightColor.G > map.PlayerLightColor.G)
                    {
                        if (PlayerLightColor.G - valChange < map.PlayerLightColor.G)
                        {
                            PlayerLightColor.G = map.PlayerLightColor.G;
                        }
                        else
                        {
                            PlayerLightColor.G -= valChange;
                        }
                    }

                    if (PlayerLightColor.B < map.PlayerLightColor.B)
                    {
                        if (PlayerLightColor.B + valChange > map.PlayerLightColor.B)
                        {
                            PlayerLightColor.B = map.PlayerLightColor.B;
                        }
                        else
                        {
                            PlayerLightColor.B += valChange;
                        }
                    }

                    if (PlayerLightColor.B > map.PlayerLightColor.B)
                    {
                        if (PlayerLightColor.B - valChange < map.PlayerLightColor.B)
                        {
                            PlayerLightColor.B = map.PlayerLightColor.B;
                        }
                        else
                        {
                            PlayerLightColor.B -= valChange;
                        }
                    }
                }

                if (sPlayerLightSize != map.PlayerLightSize)
                {
                    if (sPlayerLightSize < map.PlayerLightSize)
                    {
                        if (sPlayerLightSize + 500 * ecTime / 2000f > map.PlayerLightSize)
                        {
                            sPlayerLightSize = map.PlayerLightSize;
                        }
                        else
                        {
                            sPlayerLightSize += 500 * ecTime / 2000f;
                        }
                    }

                    if (sPlayerLightSize > map.PlayerLightSize)
                    {
                        if (sPlayerLightSize - 500 * ecTime / 2000f < map.PlayerLightSize)
                        {
                            sPlayerLightSize = map.PlayerLightSize;
                        }
                        else
                        {
                            sPlayerLightSize -= 500 * ecTime / 2000f;
                        }
                    }
                }

                if (sPlayerLightIntensity < map.PlayerLightIntensity)
                {
                    if (sPlayerLightIntensity + valChange > map.PlayerLightIntensity)
                    {
                        sPlayerLightIntensity = map.PlayerLightIntensity;
                    }
                    else
                    {
                        sPlayerLightIntensity += valChange;
                    }
                }

                if (sPlayerLightIntensity > map.AHue)
                {
                    if (sPlayerLightIntensity - valChange < map.PlayerLightIntensity)
                    {
                        sPlayerLightIntensity = map.PlayerLightIntensity;
                    }
                    else
                    {
                        sPlayerLightIntensity -= valChange;
                    }
                }

                if (sPlayerLightExpand < map.PlayerLightExpand)
                {
                    if (sPlayerLightExpand + 100f * ecTime / 2000f > map.PlayerLightExpand)
                    {
                        sPlayerLightExpand = map.PlayerLightExpand;
                    }
                    else
                    {
                        sPlayerLightExpand += 100f * ecTime / 2000f;
                    }
                }

                if (sPlayerLightExpand > map.PlayerLightExpand)
                {
                    if (sPlayerLightExpand - 100f * ecTime / 2000f < map.PlayerLightExpand)
                    {
                        sPlayerLightExpand = map.PlayerLightExpand;
                    }
                    else
                    {
                        sPlayerLightExpand -= 100f * ecTime / 2000f;
                    }
                }

                sLightUpdate = Globals.System.GetTimeMs();
            }
        }

        //Helper Functions
        /// <summary>
        /// Convert a position on the screen to a position on the actual map for rendering.
        /// </summary>
        /// <param name="windowPoint">The point to convert.</param>
        /// <returns>The converted point.</returns>
        public static Pointf ConvertToWorldPoint(Pointf windowPoint)
        {
            return new Pointf((int)Math.Floor(windowPoint.X + CurrentView.Left), (int)Math.Floor(windowPoint.Y + CurrentView.Top));
        }

    }
}