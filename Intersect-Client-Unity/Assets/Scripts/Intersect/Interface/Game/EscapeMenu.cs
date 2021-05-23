using Intersect.Client.Core;
using Intersect.Client.General;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class EscapeMenu : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textOptions;
        [SerializeField]
        private Button buttonOptions;
        [SerializeField]
        private TextMeshProUGUI textCharacters;
        [SerializeField]
        private Button buttonCharacters;
        [SerializeField]
        private TextMeshProUGUI textLogout;
        [SerializeField]
        private Button buttonLogout;
        [SerializeField]
        private TextMeshProUGUI textDesktop;
        [SerializeField]
        private Button buttonDesktop;
        [SerializeField]
        private TextMeshProUGUI textClose;
        [SerializeField]
        private Button buttonClose;

        [SerializeField]
        private OptionsWindow optionsWindow;

        protected override void Awake()
        {
            base.Awake();

            textTitle.text = Strings.EscapeMenu.Title;
            textOptions.text = Strings.EscapeMenu.Options;
            textCharacters.text = Strings.EscapeMenu.CharacterSelect;
            textLogout.text = Strings.EscapeMenu.Logout;
            textDesktop.text = Strings.EscapeMenu.ExitToDesktop;
            textClose.text = Strings.EscapeMenu.Close;

            buttonOptions.onClick.AddListener(OnClickOptions);
            buttonCharacters.onClick.AddListener(OnClickCharacters);
            buttonLogout.onClick.AddListener(OnClickLogout);
            buttonDesktop.onClick.AddListener(OnClickDesktop);
            buttonClose.onClick.AddListener(OnClickClose);
        }

        internal void ToggleHidden()
        {
            if (optionsWindow.IsVisible)
            {
                return;
            }

            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        internal void OpenSettings()
        {
            optionsWindow.Show();
        }

        internal void Draw()
        {
            if (IsHidden)
            {
                return;
            }

            buttonCharacters.interactable = Globals.Me?.CombatTimer < Globals.System.GetTimeMs();
        }

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickDesktop()
        {
            ToggleHidden();
            if (Globals.Me.CombatTimer > Globals.System.GetTimeMs())
            {
                //Show Logout in Combat Warning
                Interface.InputBox.Show(
                    Strings.Combat.warningtitle, Strings.Combat.warningexitdesktop, true, InputBox.InputType.YesNo,
                    ExitToDesktop, null, null
                );
            }
            else
            {
                ExitToDesktop(null, null);
            }
        }


        private void OnClickLogout()
        {
            ToggleHidden();
            if (Globals.Me.CombatTimer > Globals.System.GetTimeMs())
            {
                //Show Logout in Combat Warning
                Interface.InputBox.Show(
                    Strings.Combat.warningtitle, Strings.Combat.warninglogout, true, InputBox.InputType.YesNo,
                    LogoutToMainMenu, null, null
                );
            }
            else
            {
                LogoutToMainMenu(null, null);
            }
        }


        private void OnClickCharacters()
        {
            ToggleHidden();
            if (Globals.Me.CombatTimer > Globals.System.GetTimeMs())
            {
                //Show Logout in Combat Warning
                Interface.InputBox.Show(
                    Strings.Combat.warningtitle, Strings.Combat.warningcharacterselect, true, InputBox.InputType.YesNo,
                    LogoutToCharacterSelect, null, null
                );
            }
            else
            {
                LogoutToCharacterSelect(null, null);
            }
        }

        private void OnClickOptions()
        {
            OpenSettings();
        }
        private void ExitToDesktop(object sender, EventArgs e)
        {
            if (Globals.Me != null)
            {
                Globals.Me.CombatTimer = 0;
            }

            Globals.IsRunning = false;
        }

        private void LogoutToMainMenu(object sender, EventArgs e)
        {
            if (Globals.Me != null)
            {
                Globals.Me.CombatTimer = 0;
            }

            Main.Logout(false);
        }

        private void LogoutToCharacterSelect(object sender, EventArgs e)
        {
            if (Globals.Me != null)
            {
                Globals.Me.CombatTimer = 0;
            }

            Main.Logout(true);
        }
    }

}
