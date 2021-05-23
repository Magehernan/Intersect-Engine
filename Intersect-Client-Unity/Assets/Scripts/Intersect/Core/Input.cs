using Intersect.Admin.Actions;
using Intersect.Client.Core.Controls;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.General;
using Intersect.Client.Maps;
using Intersect.Client.Networking;
using Intersect.Client.Utils;
using System;
using U = UnityEngine;

namespace Intersect.Client.Core
{

    public static class Input
    {

        public delegate void HandleKeyEvent(U.KeyCode key);

        public static HandleKeyEvent KeyDown;

        public static HandleKeyEvent KeyUp;

        public static void OnKeyPressed(U.KeyCode key)
        {
            if (key == U.KeyCode.None)
            {
                return;
            }

            bool consumeKey = false;

            KeyDown?.Invoke(key);
            switch (key)
            {
                case U.KeyCode.Escape:
                    if (Globals.GameState != GameStates.Intro)
                    {
                        break;
                    }

                    Fade.FadeIn();
                    Globals.GameState = GameStates.Menu;

                    return;
            }

            if (Controls.Controls.ControlHasKey(Control.OpenMenu, key))
            {
                if (Globals.GameState != GameStates.InGame)
                {
                    return;
                }

                // First try and unfocus chat then close all UI elements, then untarget our target.. and THEN open the escape menu.
                // Most games do this, why not this?
                if (Interface.Interface.GameUi != null && Interface.Interface.GameUi.FocusChat)
                {
                    Interface.Interface.ResetInputFocus();
                }
                else if (Interface.Interface.GameUi != null && Interface.Interface.GameUi.CloseAllWindows())
                {
                    // We've closed our windows, don't do anything else. :)
                }
                else if (Globals.Me != null && Globals.Me.TargetIndex != Guid.Empty)
                {
                    Globals.Me.ClearTarget();
                }
                else
                {
                    Interface.Interface.GameUi.EscapeMenu.ToggleHidden();
                }
            }

            if (Interface.Interface.HasInputFocus())
            {
                return;
            }

            if (Globals.Me == null)
            {
                return;
            }

            if (Globals.InputManager.IsMouseButton(key))
            {
                if (Interface.Interface.MouseHitGui())
                {
                    return;
                }

                if (Globals.Me.TryTarget())
                {
                    return;
                }
            }



            Controls.Controls.GetControlsFor(key)
                ?.ForEach(
                    control =>
                    {
                        if (consumeKey)
                        {
                            return;
                        }

                        switch (control)
                        {
                            case Control.Screenshot:

                                Singleton.Unimplemented("Tomar una captura de pantalla");
                                break;

                            case Control.ToggleGui:
                                if (Globals.GameState == GameStates.InGame)
                                {
                                    Interface.Interface.HideUi = !Interface.Interface.HideUi;
                                }

                                break;
                        }

                        switch (Globals.GameState)
                        {
                            case GameStates.InGame:
                            {
                                switch (control)
                                {
                                    case Control.Block:
                                        Globals.Me.TryBlock();
                                        break;
                                    case Control.AutoTarget:
                                        Globals.Me.AutoTarget();
                                        break;
                                    case Control.PickUp:
                                        Globals.Me.TryPickupItem(Globals.Me.MapInstance.Id, Globals.Me.Y * Options.MapWidth + Globals.Me.X);
                                        break;
                                    case Control.Enter:
                                        Interface.Interface.GameUi.FocusChat = true;
                                        consumeKey = true;
                                        return;

                                    case Control.OpenInventory:
                                        Interface.Interface.GameUi.GameMenu.ToggleInventoryWindow();
                                        break;

                                    case Control.OpenQuests:
                                        Interface.Interface.GameUi.GameMenu.ToggleQuestsWindow();
                                        break;

                                    case Control.OpenCharacterInfo:
                                        Interface.Interface.GameUi.GameMenu.ToggleCharacterWindow();
                                        break;

                                    case Control.OpenParties:
                                        Interface.Interface.GameUi.GameMenu.TogglePartyWindow();
                                        break;

                                    case Control.OpenSpells:
                                        Interface.Interface.GameUi.GameMenu.ToggleSpellsWindow();
                                        break;

                                    case Control.OpenFriends:
                                        Interface.Interface.GameUi.GameMenu.ToggleFriendsWindow();
                                        break;

                                    case Control.OpenGuild:
                                        Interface.Interface.GameUi.GameMenu.ToggleGuildWindow();
                                        break;

                                    case Control.OpenSettings:
                                        Interface.Interface.GameUi.EscapeMenu.OpenSettings();
                                        break;

                                    case Control.OpenDebugger:
                                        Interface.Interface.GameUi.ShowHideDebug();
                                        break;

                                    case Control.OpenAdminPanel:
                                        PacketSender.SendOpenAdminWindow();

                                        break;
                                    case Control.MoveUp:
                                    case Control.MoveLeft:
                                    case Control.MoveDown:
                                    case Control.MoveRight:
                                    case Control.Hotkey1:
                                    case Control.Hotkey2:
                                    case Control.Hotkey3:
                                    case Control.Hotkey4:
                                    case Control.Hotkey5:
                                    case Control.Hotkey6:
                                    case Control.Hotkey7:
                                    case Control.Hotkey8:
                                    case Control.Hotkey9:
                                    case Control.Hotkey0:
                                    case Control.AttackInteract:
                                    case Control.Screenshot:
                                    case Control.OpenMenu:
                                    case Control.ToggleGui:
                                        break;

                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(control), control, null);
                                }
                            }
                            break;

                            case GameStates.Intro:
                            case GameStates.Menu:
                            case GameStates.Loading:
                            case GameStates.Error:
                                //no se hace nada por el momento
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(Globals.GameState), Globals.GameState, null);
                        }
                    }
                );
        }

        public static void OnKeyReleased(U.KeyCode key)
        {
            KeyUp?.Invoke(key);
            if (Interface.Interface.HasInputFocus())
            {
                return;
            }


            if (Globals.Me == null)
            {
                return;
            }

            if (Controls.Controls.ControlHasKey(Control.Block, key))
            {
                Globals.Me.StopBlocking();
            }

            #region Teleport Admin
            if (key != U.KeyCode.Mouse1)
            {
                return;
            }

            if (!Globals.InputManager.KeyDown(U.KeyCode.LeftShift))
            {
                return;
            }

            Pointf pos = Globals.InputManager.GetMousePosition();
            int x = (int)pos.X;
            int y = (int)pos.Y + 1;

            foreach (MapInstance map in MapInstance.Lookup.Values)
            {
                if (!(x >= map.GetX()) || !(x <= map.GetX() + Options.MapWidth))
                {
                    continue;
                }

                if (!(y >= map.GetY()) || !(y <= map.GetY() + Options.MapHeight))
                {
                    continue;
                }

                //Remove the offsets to just be dealing with pixels within the map selected
                x -= (int)map.GetX();
                y -= (int)map.GetY();

                Guid mapNum = map.Id;
                if (Globals.Me.GetRealLocation(ref x, ref y, ref mapNum))
                {
                    PacketSender.SendAdminAction(new WarpToLocationAction(mapNum, (byte)x, (byte)y));
                }

                return;
            }
            #endregion
        }
    }
}