using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Nez
{
    public static class GamePadDataExt
    {
        static readonly Buttons[] _buttons = Enum.GetValues(typeof(Buttons)).Cast<Buttons>().ToArray();
        public static Buttons? GetFirstPressedButton(this GamePadData gamepad)
        {
            Buttons? pressed = null;
            foreach (var button in _buttons)
            {
                if (gamepad.IsButtonPressed(button))
                {
                    pressed = button;
                    break;
                }
            }
            return pressed;
        }
    }
}
