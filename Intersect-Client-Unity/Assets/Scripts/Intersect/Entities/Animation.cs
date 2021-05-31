using Intersect.Client.Core;
using Intersect.Client.Core.Sounds;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.UnityGame;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Client.Utils;
using Intersect.GameObjects;
using System;

namespace Intersect.Client.Entities
{

    public class Animation
    {

        public bool AutoRotate;

        private bool disposed = false;

        public bool Hidden;

        public bool InfiniteLoop;

        private bool mDisposeNextDraw;

        private int mLowerFrame;

        private int mLowerLoop;

        private long mLowerTimer;

        private Entity mParent;

        private int mRenderDir;

        private float mRenderX;

        private float mRenderY;

        private bool mShowLower = true;

        private bool mShowUpper = true;

        private MapSound mSound;

        private long mStartTime = Globals.System.GetTimeMs();

        private int mUpperFrame;

        private int mUpperLoop;

        private long mUpperTimer;

        public AnimationBase MyBase;

        private int mZDimension = -1;

        private AnimationRenderer animationRenderer;

        private LightRenderer lowerLightRenderer;
        private LightRenderer upperLightRenderer;

        public Animation(
            AnimationBase animBase,
            bool loopForever,
            bool autoRotate = false,
            int zDimension = -1,
            Entity parent = null
        )
        {
            MyBase = animBase;
            mParent = parent;
            if (MyBase != null)
            {
                animationRenderer = UnityFactory.GetAnimationRenderer(animBase.Name);
                mLowerLoop = animBase.Lower.LoopCount;
                mUpperLoop = animBase.Upper.LoopCount;
                mLowerTimer = Globals.System.GetTimeMs() + animBase.Lower.FrameSpeed;
                mUpperTimer = Globals.System.GetTimeMs() + animBase.Upper.FrameSpeed;
                InfiniteLoop = loopForever;
                AutoRotate = autoRotate;
                mZDimension = zDimension;
                mSound = Audio.AddMapSound(MyBase.Sound, 0, 0, Guid.Empty, loopForever, 0, 12, parent);

                lowerLightRenderer = UnityFactory.GetLightRender($"{animBase.Name} Lower");
                upperLightRenderer = UnityFactory.GetLightRender($"{animBase.Name} Upper");

                lock (Graphics.AnimationLock)
                {
                    Graphics.LiveAnimations.Add(this);
                }
            }
            else
            {
                Dispose();
            }
        }

        public void Draw(bool upper = false)
        {
            if (Hidden || disposed)
            {
                return;
            }

            float rotationDegrees = 0f;
            bool dontRotate = upper && MyBase.Upper.DisableRotations || !upper && MyBase.Lower.DisableRotations;
            if ((AutoRotate || mRenderDir != -1) && !dontRotate)
            {
                switch (mRenderDir)
                {
                    case 0: //Up
                        rotationDegrees = 0f;

                        break;
                    case 1: //Down
                        rotationDegrees = 180f;

                        break;
                    case 2: //Left
                        rotationDegrees = 90f;

                        break;
                    case 3: //Right
                        rotationDegrees = 270f;

                        break;
                    case 4: //NW
                        rotationDegrees = 45f;

                        break;
                    case 5: //NE
                        rotationDegrees = 315f;

                        break;
                    case 6: //SW
                        rotationDegrees = 135f;

                        break;
                    case 7: //SE
                        rotationDegrees = 225f;

                        break;
                }
            }

            if (!upper && mShowLower && mZDimension < 1 || !upper && mShowLower && mZDimension > 0)
            {
                //Draw Lower
                GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation, MyBase.Lower.Sprite);

                if (tex != null)
                {
                    if (MyBase.Lower.XFrames > 0 && MyBase.Lower.YFrames > 0)
                    {
                        animationRenderer.DrawLower(tex.GetSpriteAnimation(mLowerFrame), rotationDegrees, MyBase.Lower.AlternateRenderLayer);
                    }
                }

                LightBase lowerLight = MyBase.Lower.Lights[mLowerFrame];
                int offsetX = lowerLight.OffsetX;
                int offsetY = lowerLight.OffsetY;
                Point offset = RotatePoint(
                    new Point(offsetX, offsetY), new Point(0, 0), rotationDegrees + 180
                );

                lowerLightRenderer.SetPosition(mRenderX - offset.X, mRenderY - offset.Y);
                lowerLightRenderer.UpdateLight(lowerLight.Size, 255, lowerLight.Expand, lowerLight.Color.ToColor32());
            }

            if (upper && mShowUpper && mZDimension != 0 || upper && mShowUpper && mZDimension == 0)
            {
                //Draw Upper
                GameTexture tex = Globals.ContentManager.GetTexture(
                    GameContentManager.TextureType.Animation, MyBase.Upper.Sprite
                );

                if (tex != null)
                {
                    if (MyBase.Upper.XFrames > 0 && MyBase.Upper.YFrames > 0)
                    {
                        animationRenderer.DrawUpper(tex.GetSpriteAnimation(mUpperFrame), rotationDegrees, MyBase.Upper.AlternateRenderLayer);
                    }
                }

                LightBase upperLight = MyBase.Upper.Lights[mUpperFrame];
                int offsetX = upperLight.OffsetX;
                int offsetY = upperLight.OffsetY;
                Point offset = RotatePoint(
                    new Point(offsetX, offsetY), new Point(0, 0), rotationDegrees + 180
                );

                upperLightRenderer.SetPosition(mRenderX - offset.X, mRenderY - offset.Y);
                upperLightRenderer.UpdateLight(upperLight.Size, 255, upperLight.Expand, upperLight.Color.ToColor32());
            }

            animationRenderer.SetPosition(mRenderX, mRenderY);
        }

        public void EndDraw()
        {
            if (mDisposeNextDraw)
            {
                Dispose();
            }
        }

        static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);

            return new Point
            {
                X = (int)(cosTheta * (pointToRotate.X - centerPoint.X) -
                           sinTheta * (pointToRotate.Y - centerPoint.Y) +
                           centerPoint.X),
                Y = (int)(sinTheta * (pointToRotate.X - centerPoint.X) +
                           cosTheta * (pointToRotate.Y - centerPoint.Y) +
                           centerPoint.Y)
            };
        }

        public void Hide()
        {
            Hidden = true;
        }

        public void Show()
        {
            Hidden = false;
        }

        public bool ParentGone()
        {
            if (mParent != null && mParent.IsDisposed())
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            lock (Graphics.AnimationLock)
            {
                if (mSound != null)
                {
                    mSound.Loop = false;
                    if (!MyBase.CompleteSound)
                    {
                        mSound.Stop();
                    }

                    mSound = null;
                }

                animationRenderer.Destroy();
                animationRenderer = null;
                lowerLightRenderer.Destroy();
                lowerLightRenderer = null;
                upperLightRenderer.Destroy();
                upperLightRenderer = null;
                Graphics.LiveAnimations.Remove(this);
                disposed = true;
            }
        }

        public void DisposeNextDraw()
        {
            mDisposeNextDraw = true;
        }

        public bool Disposed()
        {
            return disposed;
        }

        public void SetPosition(float worldX, float worldY, int mapx, int mapy, Guid mapId, int dir, int z = 0)
        {
            if (disposed)
            {
                return;
            }
            mRenderX = worldX;
            mRenderY = worldY;
            if (mSound != null)
            {
                mSound.UpdatePosition(mapx, mapy, mapId);
            }

            if (dir > -1)
            {
                mRenderDir = dir;
            }

            mZDimension = z;
        }

        public void Update()
        {
            if (disposed)
            {
                return;
            }

            if (MyBase != null)
            {
                if (mSound != null)
                {
                    mSound.Update();
                }

                //Calculate Frames
                long elapsedTime = Globals.System.GetTimeMs() - mStartTime;

                //Lower
                if (MyBase.Lower.FrameCount > 0 && MyBase.Lower.FrameSpeed > 0)
                {
                    int realFrameCount = Math.Min(MyBase.Lower.FrameCount, MyBase.Lower.XFrames * MyBase.Lower.YFrames);
                    int lowerFrame = (int)Math.Floor(elapsedTime / (float)MyBase.Lower.FrameSpeed);
                    int lowerLoops = (int)Math.Floor(lowerFrame / (float)realFrameCount);
                    if (lowerLoops > mLowerLoop && !InfiniteLoop)
                    {
                        mShowLower = false;
                    }
                    else
                    {
                        mLowerFrame = lowerFrame - lowerLoops * realFrameCount;
                    }
                }

                //Upper
                if (MyBase.Upper.FrameCount > 0 && MyBase.Upper.FrameSpeed > 0)
                {
                    int realFrameCount = Math.Min(MyBase.Upper.FrameCount, MyBase.Upper.XFrames * MyBase.Upper.YFrames);
                    int upperFrame = (int)Math.Floor(elapsedTime / (float)MyBase.Upper.FrameSpeed);
                    int upperLoops = (int)Math.Floor(upperFrame / (float)realFrameCount);
                    if (upperLoops > mUpperLoop && !InfiniteLoop)
                    {
                        mShowUpper = false;
                    }
                    else
                    {
                        mUpperFrame = upperFrame - upperLoops * realFrameCount;
                    }
                }

                if (!mShowLower && !mShowUpper)
                {
                    Dispose();
                }
            }
        }

        public Point AnimationSize()
        {
            Point size = new Point(0, 0);

            GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation, MyBase.Lower.Sprite);
            if (tex != null)
            {
                if (MyBase.Lower.XFrames > 0 && MyBase.Lower.YFrames > 0)
                {
                    int frameWidth = tex.Width / MyBase.Lower.XFrames;
                    int frameHeight = tex.Height / MyBase.Lower.YFrames;
                    if (frameWidth > size.X)
                    {
                        size.X = frameWidth;
                    }

                    if (frameHeight > size.Y)
                    {
                        size.Y = frameHeight;
                    }
                }
            }

            tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation, MyBase.Upper.Sprite);
            if (tex != null)
            {
                if (MyBase.Upper.XFrames > 0 && MyBase.Upper.YFrames > 0)
                {
                    int frameWidth = tex.Width / MyBase.Upper.XFrames;
                    int frameHeight = tex.Height / MyBase.Upper.YFrames;
                    if (frameWidth > size.X)
                    {
                        size.X = frameWidth;
                    }

                    if (frameHeight > size.Y)
                    {
                        size.Y = frameHeight;
                    }
                }
            }

            foreach (LightBase light in MyBase.Lower.Lights)
            {
                if (light != null)
                {
                    if (light.Size + Math.Abs(light.OffsetX) > size.X)
                    {
                        size.X = light.Size + light.OffsetX;
                    }

                    if (light.Size + Math.Abs(light.OffsetY) > size.Y)
                    {
                        size.Y = light.Size + light.OffsetY;
                    }
                }
            }

            foreach (LightBase light in MyBase.Upper.Lights)
            {
                if (light != null)
                {
                    if (light.Size + Math.Abs(light.OffsetX) > size.X)
                    {
                        size.X = light.Size + light.OffsetX;
                    }

                    if (light.Size + Math.Abs(light.OffsetY) > size.Y)
                    {
                        size.Y = light.Size + light.OffsetY;
                    }
                }
            }

            return size;
        }

        public void SetDir(int dir)
        {
            mRenderDir = dir;
        }

    }

}
