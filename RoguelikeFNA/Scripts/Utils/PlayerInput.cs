using Microsoft.Xna.Framework.Input;
using Nez;
using System;

namespace RoguelikeFNA
{
    [Serializable]
    public class PlayerInput
    {
        public readonly int GamePadIndex;
        public bool IsGamepad;

        // Gamepad
        public Buttons GamePadLeftAlt = Buttons.DPadLeft;
        public Buttons GamePadRightAlt = Buttons.DPadRight;
        public Buttons GamePadUpAlt = Buttons.DPadUp;
        public Buttons GamePadDownAlt = Buttons.DPadDown;
        public Buttons GamePadJump = Buttons.A;
        public Buttons GamePadAttack = Buttons.RightShoulder;
        public Buttons GamePadSpecial = Buttons.LeftShoulder;
        public Buttons GamePadStart = Buttons.Start;
        public Buttons GamePadSelect = Buttons.Back;

        // Keyboard
        public Keys KeyLeft = Keys.A;
        public Keys KeyRight = Keys.D;
        public Keys KeyUp = Keys.W;
        public Keys KeyDown = Keys.S;
        public Keys KeyJump = Keys.Space;
        public Keys KeyAttack = Keys.J;
        public Keys KeySpecial = Keys.L;
        public Keys KeyStart = Keys.Enter;
        public Keys KeySelect = Keys.Tab;

        // Virtual Nodes
        public VirtualAxis Horizontal { get; private set; }
        public VirtualAxis Vertical { get; private set; }
        public VirtualButton Jump { get; private set; }
        public VirtualButton Attack { get; private set; }
        public VirtualButton Special { get; private set; }
        public VirtualButton Start { get; private set; }
        public VirtualButton Select { get; private set; }

        public PlayerInput()
        {
            IsGamepad = false;
            ResetVirtualNodes();
        }
        public PlayerInput(int? gamepadIndex)
        {
            if (gamepadIndex.HasValue)
            {
                GamePadIndex = (int)gamepadIndex;
                IsGamepad = true;
            }
            else
            {
                IsGamepad = false;
            }
            ResetVirtualNodes();
        }

        public void ResetVirtualNodes()
        {
            if(IsGamepad)
            {
                Horizontal = new VirtualAxis(
                    new VirtualAxis.GamePadLeftStickX(GamePadIndex),
                    new VirtualAxis.GamePadButtons(GamePadLeftAlt, GamePadRightAlt, GamePadIndex)
                );
                Vertical = new VirtualAxis(
                    new VirtualAxis.GamePadLeftStickY(GamePadIndex),
                    new VirtualAxis.GamePadButtons(GamePadLeftAlt, GamePadRightAlt, GamePadIndex, true)
                );
                Jump = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadJump));
                Attack = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadAttack));
                Special = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadSpecial));
                Start = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadStart));
                Select = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadSelect));
            }
            else
            {
                Horizontal = new VirtualAxis(
                    new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, KeyLeft, KeyRight)
                );
                Vertical = new VirtualAxis(
                    new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, KeyDown, KeyUp, true)
                );
                Jump = new VirtualButton(new VirtualButton.KeyboardKey(KeyJump));
                Attack = new VirtualButton(new VirtualButton.KeyboardKey(KeyAttack));
                Special = new VirtualButton(new VirtualButton.KeyboardKey(KeySpecial));
                Start = new VirtualButton(new VirtualButton.KeyboardKey(KeyStart));
                Select = new VirtualButton(new VirtualButton.KeyboardKey(KeySelect));
            }
        }
    }
}
