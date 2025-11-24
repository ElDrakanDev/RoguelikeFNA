using Microsoft.Xna.Framework.Input;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace RoguelikeFNA
{
    [Serializable]
    public class PlayerProfile
    {
        public string Name;
        public bool UseGamepad = false;
        public bool ReadonlyName = false;
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
        public Keys KeyLeft = Keys.A;
        public Keys KeyRight = Keys.D;
        public Keys KeyUp = Keys.W;
        public Keys KeyDown = Keys.S;
        public Keys KeyJump = Keys.Space;
        public Keys KeyAttack = Keys.J;
        public Keys KeyDash = Keys.L;
        public Keys KeySpecial = Keys.K;
        public Keys KeyStart = Keys.Enter;
        public Keys KeySelect = Keys.Tab;
        public Keys KeyInteract = Keys.E;

        public event Action onConfigChanged;

        public void OnConfigChanged() => onConfigChanged?.Invoke();

        public PlayerProfile(){}
    }

    public class PlayerInput
    {
        [XmlIgnore] public int GamePadIndex { get; private set; }
        public PlayerProfile Profile;
        public GamePadData GamePad
        {
            get
            {
                if (Profile.UseGamepad is false)
                    return null;
                var gamepad = Input.GamePads.ElementAtOrDefault(GamePadIndex);
                if(gamepad != null && gamepad.IsConnected() is false)
                    return null;
                return gamepad;
            }
        }

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

        public PlayerInput(PlayerProfile profile)
        {
            Profile = profile;
        }

        public PlayerInput(PlayerProfile profile, int gamepadIndex) : this(profile)
        {
            GamePadIndex = gamepadIndex;  
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
                var btn = new VirtualButton(new VirtualButton.GamePadButton(gamepadIdx, Buttons.Start));
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
            if(Profile.UseGamepad)
            {
                Horizontal = new VirtualAxis(
                    new VirtualAxis.GamePadLeftStickX(GamePadIndex),
                    new VirtualAxis.GamePadButtons(Profile.GamePadLeftAlt, Profile.GamePadRightAlt, GamePadIndex)
                );
                Vertical = new VirtualAxis(
                    new VirtualAxis.GamePadLeftStickY(GamePadIndex),
                    new VirtualAxis.GamePadButtons(Profile.GamePadLeftAlt, Profile.GamePadRightAlt, GamePadIndex, true)
                );
                Jump = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadJump));
                Attack = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadAttack));
                Special = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadSpecial));
                Dash = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadDash));
                Start = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadStart));
                Select = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadSelect));
                Interact = new VirtualButton(new VirtualButton.GamePadButton(GamePadIndex, Profile.GamePadInteract));
            }
            else
            {
                Horizontal = new VirtualAxis(
                    new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Profile.KeyLeft, Profile.KeyRight)
                );
                Vertical = new VirtualAxis(
                    new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Profile.KeyDown, Profile.KeyUp, true)
                );
                Jump = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeyJump));
                Attack = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeyAttack));
                Special = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeySpecial));
                Dash = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeyDash));
                Start = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeyStart));
                Select = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeySelect));
                Interact = new VirtualButton(new VirtualButton.KeyboardKey(Profile.KeyInteract));
            }
        }
    }
}
