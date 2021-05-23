using Intersect.Client.Framework.GenericClasses;

namespace Intersect.Client.Framework.Input
{

    public abstract class GameInput
    {

        public enum KeyboardType
        {

            Normal,

            Password,

            Email,

            Numberic,

            Pin

        }

        public abstract bool IsMouseButton(UnityEngine.KeyCode key);

        public abstract bool KeyDown(UnityEngine.KeyCode key);

        public abstract Pointf GetMousePosition();

        public abstract void Update();

        public abstract void OpenKeyboard(
            KeyboardType type,
            string text,
            bool autoCorrection,
            bool multiLine,
            bool secure
        );

    }

}
