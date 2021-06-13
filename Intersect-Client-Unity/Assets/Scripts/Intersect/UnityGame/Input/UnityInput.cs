using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Input;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UInput = UnityEngine.Input;

namespace Intersect.Client.UnityGame.Input
{

    internal class UnityInput : GameInput
    {
        private static readonly KeyCode[] mouseButtons = new KeyCode[]
        {
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.Mouse2,
            KeyCode.Mouse3,
            KeyCode.Mouse4,
            KeyCode.Mouse5,
            KeyCode.Mouse6,
        };

        private static readonly KeyCode[] keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));


        public override Pointf GetMousePosition()
        {
            Vector3 pos = Core.Graphics.mainCamera.ScreenToWorldPoint(UInput.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            return new Pointf(pos.x, -pos.y);
        }

        public override bool IsMouseButton(KeyCode key)
        {
            foreach (KeyCode mouseKey in mouseButtons)
            {
                if (key == mouseKey)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool KeyDown(KeyCode key)
        {
            return UInput.GetKey(key);
        }

        public override void OpenKeyboard(KeyboardType type, string text, bool autoCorrection, bool multiLine, bool secure)
        {

        }



        public override void Update()
        {
            if (!UInput.anyKey)
            {
                return;
            }

            foreach (KeyCode key in keys)
            {
                if (UInput.GetKeyDown(key))
                {
                    Core.Input.OnKeyPressed(key);
                }
                else if (UInput.GetKeyUp(key))
                {
                    Core.Input.OnKeyReleased(key);
                }
            }


            if (UInput.GetKeyDown(KeyCode.Tab))
            {
                EventSystem system = EventSystem.current;
                GameObject curObj = system.currentSelectedGameObject;
                GameObject nextObj = null;
                if (!curObj)
                {
                    nextObj = system.firstSelectedGameObject;
                }
                else
                {
                    Selectable curSelect = curObj.GetComponent<Selectable>();
                    Selectable nextSelect =
                        UInput.GetKey(KeyCode.LeftShift) || UInput.GetKey(KeyCode.RightShift)
                            ? curSelect.FindSelectableOnUp()
                            : curSelect.FindSelectableOnDown();
                    if (nextSelect)
                    {
                        nextObj = nextSelect.gameObject;
                    }
                }
                if (nextObj)
                {
                    system.SetSelectedGameObject(nextObj, new BaseEventData(system));
                }
            }
        }
    }
}


