using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using U = UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{
    internal class UnityRenderer : GameRenderer
    {
        private IntersectGame intersectGame;
        private FloatRect mCurrentView;
        private List<string> mValidVideoModes;
        private bool mInitializing = false;

        public UnityRenderer(IntersectGame intersectGame)
        {
            this.intersectGame = intersectGame;
        }

        public override void Init()
        {
            if (mInitializing)
            {
                return;
            }

            mInitializing = true;

            Framework.Database.GameDatabase database = Globals.Database;
            List<string> validVideoModes = GetValidVideoModes();
            int targetResolution = database.TargetResolution;

            if (targetResolution < 0 || validVideoModes.Count <= targetResolution)
            {
                Debug.Assert(database != null, "database != null");
                database.TargetResolution = 0;
                database.SavePreference("Resolution", database.TargetResolution.ToString());
            }

            string targetVideoMode = validVideoModes[targetResolution];
            if (Resolution.TryParse(targetVideoMode, out Resolution resolution))
            {
                PreferredResolution = resolution;
            }

            UpdateGraphicsState(ActiveResolution.X, ActiveResolution.Y);

            mInitializing = false;
        }

        public void UpdateGraphicsState(int width, int height)
        {
            if (Globals.Database.FullScreen)
            {
                bool supported = false;
                foreach (U.Resolution mode in U.Screen.resolutions)
                {
                    if (mode.width == width && mode.height == height)
                    {
                        supported = true;
                        break;
                    }
                }

                if (!supported)
                {
                    Globals.Database.FullScreen = false;
                    Globals.Database.SavePreferences();
                    Interface.Interface.MsgboxErrors.Add(
                        new KeyValuePair<string, string>(
                            Strings.Errors.displaynotsupported,
                            Strings.Errors.displaynotsupportederror.ToString(width + "x" + height)
                        )
                    );
                }
            }

            int targetFPS = 0;

            U.QualitySettings.vSyncCount = Globals.Database.TargetFps == 0 ? 1 : 0;
            if (Globals.Database.TargetFps == 1)
            {
                targetFPS = 30;
            }
            else if (Globals.Database.TargetFps == 2)
            {
                targetFPS = 60;
            }
            else if (Globals.Database.TargetFps == 3)
            {
                targetFPS = 90;
            }
            else if (Globals.Database.TargetFps == 4)
            {
                targetFPS = 120;
            }
            U.Application.targetFrameRate = targetFPS;
            U.Screen.SetResolution(width, height, Globals.Database.FullScreen, targetFPS);
        }

        public override void Close()
        {
            Singleton.Unimplemented(nameof(Close));
        }

        public override bool DisplayModeChanged()
        {
            return false;
        }

        public override int GetScreenHeight()
        {
            return U.Screen.height;
        }

        public override int GetScreenWidth()
        {
            return U.Screen.width;
        }

        public override List<string> GetValidVideoModes()
        {
            if (mValidVideoModes != null)
            {
                return mValidVideoModes;
            }

            mValidVideoModes = new List<string>();

            Resolution[] allowedResolutions = new[]
            {
                new Resolution(800, 600),
                new Resolution(1024, 720),
                new Resolution(1024, 768),
                new Resolution(1280, 720),
                new Resolution(1280, 768),
                new Resolution(1280, 1024),
                new Resolution(1360, 768),
                new Resolution(1366, 768),
                new Resolution(1440, 1050),
                new Resolution(1440, 900),
                new Resolution(1600, 900),
                new Resolution(1680, 1050),
                new Resolution(1920, 1080),
                new Resolution(2560, 1440),
            };

            int displayWidth = U.Screen.currentResolution.width;
            int displayHeight = U.Screen.currentResolution.height;

            foreach (Resolution resolution in allowedResolutions)
            {
                if (resolution.X > displayWidth)
                {
                    continue;
                }

                if (resolution.Y > displayHeight)
                {
                    continue;
                }

                if (!U.Screen.resolutions.Any(r => r.width == resolution.X && r.height == resolution.Y))
                {
                    continue;
                }

                mValidVideoModes.Add(resolution.ToString());
            }

            return mValidVideoModes;
        }

        public override FloatRect GetView()
        {
            return mCurrentView;
        }

        public override void SetView(FloatRect view)
        {
            mCurrentView = view;

            //ue.Screen.SetResolution((int)view.Width, (int)view.Height, false);
        }
    }
}