using Intersect.Client.General;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Intersect.Client.Core.Controls
{

    public class Controls
    {

        public readonly IDictionary<Control, ControlMap> ControlMapping;

        public Controls(Controls gameControls = null)
        {
            ControlMapping = new Dictionary<Control, ControlMap>();

            if (gameControls != null)
            {
                foreach (KeyValuePair<Control, ControlMap> mapping in gameControls.ControlMapping)
                {
                    CreateControlMap(mapping.Key, mapping.Value.Key1, mapping.Value.Key2);
                }
            }
            else
            {
                ResetDefaults();
                foreach (Control control in Enum.GetValues(typeof(Control)))
                {
                    string name = Enum.GetName(typeof(Control), control);
                    string key1 = Globals.Database.LoadPreference(name + "_key1");
                    string key2 = Globals.Database.LoadPreference(name + "_key2");
                    if (string.IsNullOrEmpty(key1) || string.IsNullOrEmpty(key2))
                    {
                        Globals.Database.SavePreference(
                            name + "_key1", ((int)ControlMapping[control].Key1).ToString()
                        );

                        Globals.Database.SavePreference(
                            name + "_key2", ((int)ControlMapping[control].Key2).ToString()
                        );
                    }
                    else
                    {
                        CreateControlMap(control, (KeyCode)int.Parse(key1), (KeyCode)int.Parse(key2));
                    }
                }
            }
        }

        public static Controls ActiveControls { get; set; }

        public void ResetDefaults()
        {
            CreateControlMap(Control.MoveUp, KeyCode.UpArrow, KeyCode.W);
            CreateControlMap(Control.MoveDown, KeyCode.DownArrow, KeyCode.S);
            CreateControlMap(Control.MoveLeft, KeyCode.LeftArrow, KeyCode.A);
            CreateControlMap(Control.MoveRight, KeyCode.RightArrow, KeyCode.D);
            CreateControlMap(Control.AttackInteract, KeyCode.E, KeyCode.Mouse0);
            CreateControlMap(Control.Block, KeyCode.Q, KeyCode.Mouse1);
            CreateControlMap(Control.AutoTarget, KeyCode.Tab, KeyCode.None);
            CreateControlMap(Control.PickUp, KeyCode.Space, KeyCode.None);
            CreateControlMap(Control.Enter, KeyCode.Return, KeyCode.None);
            CreateControlMap(Control.Hotkey1, KeyCode.Alpha1, KeyCode.None);
            CreateControlMap(Control.Hotkey2, KeyCode.Alpha2, KeyCode.None);
            CreateControlMap(Control.Hotkey3, KeyCode.Alpha3, KeyCode.None);
            CreateControlMap(Control.Hotkey4, KeyCode.Alpha4, KeyCode.None);
            CreateControlMap(Control.Hotkey5, KeyCode.Alpha5, KeyCode.None);
            CreateControlMap(Control.Hotkey6, KeyCode.Alpha6, KeyCode.None);
            CreateControlMap(Control.Hotkey7, KeyCode.Alpha7, KeyCode.None);
            CreateControlMap(Control.Hotkey8, KeyCode.Alpha8, KeyCode.None);
            CreateControlMap(Control.Hotkey9, KeyCode.Alpha9, KeyCode.None);
            CreateControlMap(Control.Hotkey0, KeyCode.Alpha0, KeyCode.None);
            CreateControlMap(Control.Screenshot, KeyCode.F12, KeyCode.None);
            CreateControlMap(Control.OpenMenu, KeyCode.Escape, KeyCode.None);
            CreateControlMap(Control.OpenInventory, KeyCode.I, KeyCode.None);
            CreateControlMap(Control.OpenQuests, KeyCode.L, KeyCode.None);
            CreateControlMap(Control.OpenCharacterInfo, KeyCode.C, KeyCode.None);
            CreateControlMap(Control.OpenParties, KeyCode.P, KeyCode.None);
            CreateControlMap(Control.OpenSpells, KeyCode.X, KeyCode.None);
            CreateControlMap(Control.OpenFriends, KeyCode.F, KeyCode.None);
            CreateControlMap(Control.OpenGuild, KeyCode.G, KeyCode.None);
            CreateControlMap(Control.OpenSettings, KeyCode.None, KeyCode.None);
            CreateControlMap(Control.OpenDebugger, KeyCode.F2, KeyCode.None);
            CreateControlMap(Control.OpenAdminPanel, KeyCode.Insert, KeyCode.None);
            CreateControlMap(Control.ToggleGui, KeyCode.F11, KeyCode.None);
        }

        public void Save()
        {
            foreach (Control control in Enum.GetValues(typeof(Control)))
            {
                string name = Enum.GetName(typeof(Control), control);
                Globals.Database.SavePreference(name + "_key1", ((int)ControlMapping[control].Key1).ToString());
                Globals.Database.SavePreference(name + "_key2", ((int)ControlMapping[control].Key2).ToString());
            }
        }

        public static void Init()
        {
            ActiveControls = new Controls();
        }

        public static bool KeyDown(Control control)
        {
            if (!UnityEngine.Input.anyKey)
            {
                return false;
            }

            if (ActiveControls.ControlMapping.TryGetValue(control, out ControlMap controlMap))
            {
                return controlMap.KeyDown();
            }

            return false;
        }

        public static List<Control> GetControlsFor(KeyCode key)
        {
            return Enum.GetValues(typeof(Control))
                .Cast<Control>()
                .Where(control => ControlHasKey(control, key))
                .ToList();
        }

        public static bool ControlHasKey(Control control, KeyCode key)
        {
            if (key == KeyCode.None)
            {
                return false;
            }

            if (!(ActiveControls?.ControlMapping.ContainsKey(control) ?? false))
            {
                return false;
            }

            ControlMap mapping = ActiveControls.ControlMapping[control];

            return mapping?.Key1 == key || mapping?.Key2 == key;
        }

        public void UpdateControl(Control control, int keyNum, KeyCode key)
        {
            ControlMap mapping = ControlMapping[control];
            if (mapping == null)
            {
                return;
            }

            if (keyNum == 1)
            {
                mapping.Key1 = key;
            }
            else
            {
                mapping.Key2 = key;
            }
        }

        private void CreateControlMap(Control control, KeyCode key1, KeyCode key2)
        {
            if (ControlMapping.ContainsKey(control))
            {
                ControlMapping[control] = new ControlMap(control, key1, key2);
            }
            else
            {
                ControlMapping.Add(control, new ControlMap(control, key1, key2));
            }
        }
    }
}
