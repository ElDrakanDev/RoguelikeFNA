using Microsoft.Xna.Framework.Input;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace RoguelikeFNA
{
    [Serializable]
    public class PlayerInput
    {
        [XmlIgnore] public int GamePadIndex { get; private set; }
        public bool IsGamepad;
        public GamePadData GamePad
        {
            get
            {
                if(IsGamepad is false)
                    return null;
                var gamepad = Input.GamePads.ElementAtOrDefault(GamePadIndex);
                if(gamepad != null && gamepad.IsConnected() is false)
                    return null;
                return gamepad;
            }
        }

        // Gamepad
        public Buttons GamePadLeftAlt = Buttons.DPadLeft;
        public Buttons GamePadRightAlt = Buttons.DPadRight;
        public Buttons GamePadUpAlt = Buttons.DPadUp;
        public Buttons GamePadDownAlt = Buttons.DPadDown;
        public Buttons GamePadJump = Buttons.A;
        public Buttons GamePadAttack = Buttons.RightShoulder;
        public Buttons GamePadDash = Buttons.LeftShoulder;
        public Buttons GamePadSpecial = Buttons.LeftShoulder;
        public Buttons GamePadStart = Buttons.Start;
        public Buttons GamePadSelect = Buttons.Back;
        public Buttons GamePadInteract = Buttons.B;

        // Keyboard
        public Keys KeyLeft;
        public Keys KeyRight;
        public Keys KeyUp;
        public Keys KeyDown;
        public Keys KeyJump;
        public Keys KeyAttack;
        public Keys KeyDash;
        public Keys KeySpecial;
        public Keys KeyStart;
        public Keys KeySelect;
        public Keys KeyInteract;

        // Virtual Nodes
        [XmlIgnore] public VirtualAxis Horizontal { get; private set; }
        [XmlIgnore] public VirtualAxis Vertical { get; private set; }
        [XmlIgnore] public VirtualButton Jump { get; private set; }
        [XmlIgnore] public VirtualButton Attack { get; private set; }
        [XmlIgnore] public VirtualButton Special { get; private set; }
        [XmlIgnore] public VirtualButton Dash { get; private set; }
        [XmlIgnore] public VirtualButton Start { get; private set; }
        [XmlIgnore] public VirtualButton Select { get; private set; }
        [XmlIgnore] public VirtualButton Interact { get; private set; }

        public PlayerInput()
        {
            IsGamepad = false;
        }
        public PlayerInput(int? gamepadIndex)
        {
            if (gamepadIndex.HasValue)
            {
                GamePadIndex = gamepadIndex.Value;
                IsGamepad = true;
            }
            else
            {
                IsGamepad = false;
            }
        }

        public void SetGamepadIndex(int gamepadIndex)
        {
            GamePadIndex = gamepadIndex;
            ResetVirtualNodes();
        }

        public bool StartPressedOnGamepads(IList<int> gamepads, out int index)
        {
            index = -1;
            foreach(var gamepadIdx in gamepads)
            {
                var btn = new VirtualButton(new VirtualButton.GamePadButton(gamepadIdx, GamePadStart));
                if (btn.IsPressed)
                {
                    index = gamepadIdx;
                    return true;
                }
            }
            return false;
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
                Dash = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadDash));
                Start = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadStart));
                Select = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadSelect));
                Interact = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, GamePadInteract));
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
                Dash = new VirtualButton(new VirtualButton.KeyboardKey(KeyDash));
                Start = new VirtualButton(new VirtualButton.KeyboardKey(KeyStart));
                Select = new VirtualButton(new VirtualButton.KeyboardKey(KeySelect));
                Interact = new VirtualButton(new VirtualButton.KeyboardKey(KeyInteract));
            }
        }
    }
}
