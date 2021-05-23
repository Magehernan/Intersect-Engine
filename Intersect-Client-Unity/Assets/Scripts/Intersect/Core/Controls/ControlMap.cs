using Intersect.Client.Framework.Input;
using Intersect.Client.General;
using UnityEngine;

namespace Intersect.Client.Core.Controls
{

    public class ControlMap
    {
        public Control Control { get; }

        public KeyCode Key1 { get; set; }

        public KeyCode Key2 { get; set; }

        public ControlMap(Control control, KeyCode key1, KeyCode key2)
        {
            Control = control;
            Key1 = key1;
            Key2 = key2;
        }

        public bool KeyDown()
        {
            GameInput inputManager = Globals.InputManager;
            if (Key1 != KeyCode.None
                && (!inputManager.IsMouseButton(Key1) || !Interface.Interface.MouseHitGui())
                && inputManager.KeyDown(Key1))
            {
                return true;
            }

            if (Key2 != KeyCode.None
                && (!inputManager.IsMouseButton(Key2) || !Interface.Interface.MouseHitGui())
                && inputManager.KeyDown(Key2))
            {
                return true;
            }

            return false;
        }

    }

}
