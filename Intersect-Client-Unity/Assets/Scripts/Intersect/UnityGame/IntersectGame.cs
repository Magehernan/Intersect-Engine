using Intersect.Client.Core;
using Intersect.Client.Core.Controls;
using Intersect.Client.General;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame.Database;
using Intersect.Client.UnityGame.FileManagement;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Client.UnityGame.Input;
using Intersect.Client.UnityGame.System;
using Intersect.Configuration;
using Intersect.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Networking;

namespace Intersect.Client.UnityGame
{
    public class IntersectGame : MonoBehaviour
    {
        public const string RESOURCES_PATH = "Assets/IntersectResources";

        [SerializeField]
        private AssetReferences assetReferences;
        [SerializeField]
        private AudioSource musicSource;
        [SerializeField]
        private AudioMixer audioMixer;
        [SerializeField]
        private Light2D sunLight;

        [Header("Config")]
        [SerializeField]
        private TextAsset stringFile;

        [SerializeField]
        private TextAsset rsaKey;

        private bool started = false;
        #region Handle Exceptions
        public IntersectGame()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        void HandleException(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                Log.Error($"{logString}\n{stackTrace}");
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Error(exception.ToString());
            }
        }
        #endregion

        private IEnumerator Start()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

            Application.logMessageReceived += HandleException;

            //cambio de directorio para que sea compatible con android y pc
            Log.ChangeDirectory($"{Application.persistentDataPath}/logs/");

            Strings.Load(stringFile.text);

            Globals.ContentManager = new UnityContentManager(assetReferences);
            Globals.Database = new UnityDatabase();

            /* Load configuration */
            UnityWebRequest request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, "config.txt"));
            yield return request.SendWebRequest();
            string configJson = request.downloadHandler.text;

            JsonConvert.PopulateObject(configJson, ClientConfiguration.Instance);

            Globals.Database.LoadPreferences();

            UnityRenderer renderer = new UnityRenderer(this);

            Globals.InputManager = new UnityInput();

            Core.Graphics.Renderer = renderer;

            Globals.System = new UnitySystem();
            Controls.Init();

            Main.Start(rsaKey.bytes, sunLight, musicSource, audioMixer);
            Core.Audio.UpdateGlobalVolume();
            started = true;
        }

        private void Update()
        {
            if(!started)
            {
                return;
            }

            if (Globals.IsRunning)
            {
                lock (Globals.GameLock)
                {
                    Main.Update();
                }
                Draw();
            }
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        private void Draw()
        {
            if (Globals.IsRunning)
            {
                lock (Globals.GameLock)
                {
                    Core.Graphics.Render();
                }
            }
        }

        private static bool WantsToQuit()
        {
            if (Globals.Me != null && Globals.Me.CombatTimer > Globals.System?.GetTimeMs())
            {
                //Show Message Getting Exit Confirmation From Player to Leave in Combat
                Interface.Interface.InputBox.Show(
                    Strings.Combat.warningtitle, Strings.Combat.warningcharacterselect, true,
                    InputBox.InputType.YesNo, ExitToDesktop, null, null
                );

                return false;
            }

            Networking.Network.Close("quitting");
            return true;
        }

        private static void ExitToDesktop(object sender, EventArgs e)
        {
            if (Globals.Me != null)
            {
                Globals.Me.CombatTimer = 0;
            }

            Globals.IsRunning = false;
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
            Application.quitting += Main.Destroy;
        }
    }
}