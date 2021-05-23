using Intersect.Client.General;
using Intersect.Client.UnityGame;
using System;

namespace Intersect.Client.Maps
{

    public class ActionMessage
    {

        public Color Clr;

        public string Msg = string.Empty;

        public long TransmittionTimer;

        public int X;

        public float XOffset;

        public int Y;

        private ActionMessageRenderer actionMessageRenderer;

        public ActionMessage(int x, int y, string message, Color color)
        {
            X = x;
            Y = y;
            Msg = message;
            Clr = color;
            XOffset = Globals.Random.Next(-10, 11) / 10f; //+- 16 pixels so action msg's don't overlap!
            TransmittionTimer = Globals.System.GetTimeMs() + 1000;
        }

        public void Remove()
        {
            if (actionMessageRenderer != null)
            {
                actionMessageRenderer.Destroy();
                actionMessageRenderer = null;
            }
        }

        public bool Draw(float mapX, float mapY)
        {
            if (actionMessageRenderer == null)
            {
                actionMessageRenderer = UnityFactory.GetActionMessageRenderer(Msg);
            }
            float y = (float)(mapY + Y - 2.0 * (1000 - (TransmittionTimer - Globals.System.GetTimeMs())) / 1000);
            float x = mapX + X + XOffset;

            actionMessageRenderer.Draw(Msg, x, y, Clr);

            return TransmittionTimer > Globals.System.GetTimeMs();
        }
    }

}
