using Intersect.Client.Core;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Maps;
using Intersect.Client.UnityGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class DebugMenu : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private TextMeshProUGUI textFPS;
        [SerializeField]
        private TextMeshProUGUI textPing;
        [SerializeField]
        private TextMeshProUGUI textDraws;
        [SerializeField]
        private TextMeshProUGUI textMap;
        [SerializeField]
        private TextMeshProUGUI textPosition;
        [SerializeField]
        private TextMeshProUGUI textMousePosition;
        [SerializeField]
        private TextMeshProUGUI textEntities;
        [SerializeField]
        private TextMeshProUGUI textMapsLoaded;
        [SerializeField]
        private TextMeshProUGUI textMapsDrawn;
        [SerializeField]
        private TextMeshProUGUI textEntitiesDrawn;
        [SerializeField]
        private TextMeshProUGUI textTime;

        private int fps;
        private float updateTime;

        private void Start()
        {
            textTitle.text = Strings.Debug.title;
            buttonClose.onClick.AddListener(() => Hide());
        }

        internal void Draw()
        {
            if (!IsVisible)
            {
                fps = 0;
                updateTime = 0;
                return;
            }

            fps++;
            updateTime += UnityEngine.Time.unscaledDeltaTime;

            if (updateTime >= 1f)
            {
                textFPS.text = Strings.Debug.fps.ToString(fps);
                updateTime = 0;
                fps = 0;
            }

            textPing.text = Strings.Debug.ping.ToString(Networking.Network.Ping);
            textDraws.text = Strings.Debug.draws.ToString(Core.Graphics.DrawCalls);
            if (MapInstance.Get(Globals.Me.CurrentMap) != null)
            {
                textMap.text = Strings.Debug.map.ToString(MapInstance.Get(Globals.Me.CurrentMap).Name);
                textPosition.text = $"{Strings.Debug.x.ToString(Globals.Me.X)} {Strings.Debug.y.ToString(Globals.Me.Y)} {Strings.Debug.z.ToString(Globals.Me.Z)}";
            }
            Pointf mousePos = Globals.InputManager.GetMousePosition();
            textMousePosition.text = $"{Strings.Debug.x.ToString(mousePos.X)} {Strings.Debug.y.ToString(mousePos.Y)}";

            int entityCount = Globals.Entities.Count;
            foreach (MapInstance map in MapInstance.Lookup.Values)
            {
                if (map != null)
                {
                    entityCount += map.LocalEntities.Count;
                }
            }

            textEntities.text = Strings.Debug.knownentities.ToString(Globals.Entities.Count);
            textMapsLoaded.text = Strings.Debug.knownmaps.ToString(MapInstance.Lookup.Count);
            textMapsDrawn.text = Strings.Debug.mapsdrawn.ToString(Core.Graphics.MapsDrawn);
            textEntitiesDrawn.text = Strings.Debug.entitiesdrawn.ToString(Core.Graphics.EntitiesDrawn);
            textTime.text = Strings.Debug.time.ToString(General.Time.GetTime());
        }
    }
}
