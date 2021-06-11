using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class PictureWindow : Window, IPointerClickHandler
    {
        private enum PictureSize
        {
            Original = 0,
            FullScreen,
            HalfScreen,
            StretchToFit,
        }

        [SerializeField]
        private Image image = default;
        public string Picture { get; private set; }
        public int Size { get; private set; }
        public bool Clickable { get; private set; }


        internal void Setup(string picture, int pictureSize, bool pictureClickable)
        {
            Picture = picture;
            Size = pictureSize;
            Clickable = pictureClickable;

            GameTexture gameTexture = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Image, picture);
            if (gameTexture == null)
            {
                Hide();
                return;
            }

            Sprite sprite = gameTexture.GetSpriteDefault();
            image.sprite = sprite;
            image.raycastTarget = Clickable;

            switch ((PictureSize)pictureSize)
            {
                case PictureSize.Original:
                {
                    MyRectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height) / Interface.CanvasScale;
                    image.preserveAspect = true;
                }
                break;
                case PictureSize.StretchToFit:
                {
                    MyRectTransform.sizeDelta = Interface.CanvasSize / Interface.CanvasScale;
                    image.preserveAspect = false;
                }
                break;
                case PictureSize.FullScreen:
                case PictureSize.HalfScreen:
                {
                    image.preserveAspect = false;
                    int n = 1;

                    //If you want half fullscreen size set n to 2.
                    if (Size == (int)PictureSize.HalfScreen)
                    {
                        n = 2;
                    }

                    Vector2 canvasSize = Interface.CanvasSize * Interface.CanvasScale;

                    float ar = sprite.rect.width / sprite.rect.height;
                    bool heightLimit = true;
                    if (canvasSize.x < canvasSize.y * ar)
                    {
                        heightLimit = false;
                    }
                    float width;
                    float height;
                    if (heightLimit)
                    {
                        width = canvasSize.y * ar;
                        height = canvasSize.y;
                    }
                    else
                    {
                        width = canvasSize.x;
                        height = width / ar;
                    }
                    MyRectTransform.sizeDelta = new Vector2(width / n, height / n) / Interface.CanvasScale;
                }
                break;
            }


            Show();
        }

        public void Draw()
        {
            if (Picture != null)
            {
                if (Globals.Picture != null && Globals.Picture.HideTime > 0 && Globals.System.GetTimeMs() > Globals.Picture.ReceiveTime + Globals.Picture.HideTime)
                {
                    //Should auto close this picture
                    Hide();
                }
            }
        }

        public override void Hide(object obj = null)
        {
            if (Picture != null)
            {
                base.Hide(obj);
                PacketSender.SendClosePicture(Globals.Picture?.EventId ?? Guid.Empty);
                Globals.Picture = null;
                Picture = string.Empty;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}
