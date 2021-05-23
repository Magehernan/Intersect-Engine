using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Menu;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface
{

    public class Interface : MonoBehaviour
    {
        #region NoStatic
        private static Interface instance;
        [SerializeField]
        private MenuGuiBase mainMenu = default;
        [SerializeField]
        private GameInterface gameInterface = default;
        [SerializeField]
        private InputBox inputBox;
        [SerializeField]
        private BanMuteBox banMuteBox;

        private RectTransform myRectTransform;

        private void Awake()
        {
            instance = this;
            myRectTransform = transform as RectTransform;
        }
        #endregion

        public static readonly List<KeyValuePair<string, string>> MsgboxErrors = new List<KeyValuePair<string, string>>();

        public static bool HideUi;
        public static GameInterface GameUi => instance.gameInterface;
        public static MenuGuiBase MenuUi => instance.mainMenu;
        public static InputBox InputBox => instance.inputBox;
        public static BanMuteBox BanMuteBox => instance.banMuteBox;

        public static Vector2 CanvasSize => instance.myRectTransform.sizeDelta;

        public static float CanvasScale => instance.myRectTransform.localScale.x;

        public static bool HasInputFocus()
        {
            return EventSystem.current.currentSelectedGameObject != null
                || GameUi.EscapeMenu.IsVisible
                || InputBox.IsVisible
                || BanMuteBox.IsVisible;
        }

        internal static void ResetInputFocus()
        {
            instance.StartCoroutine(UnFocusCorroutine());
        }

        private static IEnumerator UnFocusCorroutine()
        {
            yield return null;

            EventSystem eventSystem = EventSystem.current;
            if (!eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        internal static bool MouseHitGui()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        internal static void DrawGui()
        {
            ShowErrors();
            //sGameCanvas.RestrictToParent = false;
            if (Globals.GameState == GameStates.Menu)
            {
                MenuUi.Draw();
                GameUi.Hide();
            }
            else if (Globals.GameState == GameStates.InGame && (!GameUi.EscapeMenu.IsHidden || !HideUi))
            {
                GameUi.Draw();
                MenuUi.Hide();
            }
        }

        internal static string[] WrapText(string mSourceText, int v, GameFont chatBubbleFont)
        {
            Singleton.Unimplemented(nameof(WrapText));
            return null;
        }

        private static void ShowErrors()
        {
            if (InputBox.IsHidden && MsgboxErrors.Count > 0)
            {
                KeyValuePair<string, string> message = MsgboxErrors[0];
                MsgboxErrors.RemoveAt(0);

                InputBox.Show(!string.IsNullOrEmpty(message.Key) ? message.Key : Strings.Errors.title.ToString(), message.Value, false, InputBox.InputType.OkayOnly, null, null, null);
            }
        }
    }
}
