using Nez;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace RoguelikeFNA
{
    public class InputManager : GlobalManager
    {
        public List<PlayerInput> InputConfigs { get; private set; } = new List<PlayerInput>();
        public List<PlayerInput> AvailablePlayers { get; private set; } = new List<PlayerInput> { };
        List<int> _availableGamepads = new List<int>();
        Dictionary<int, PlayerInput> _gamepadPlayers = new Dictionary<int, PlayerInput>();
        const string INPUT_PATH = "./data/input.data";
        public event Action<PlayerInput> OnPlayerJoined;

        public override void OnEnabled()
        {
            base.OnEnabled();

            _availableGamepads = new List<int>() {0, 1, 2, 3};
            _availableGamepads = _availableGamepads.Where(idx => idx < Input.GamePads.Length && Input.GamePads[idx].IsConnected()).ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerInput[]));
            if (File.Exists(INPUT_PATH))
            {
                try
                {
                    using (FileStream fs = File.OpenRead(INPUT_PATH))
                    {
                        InputConfigs = (List<PlayerInput>)serializer.Deserialize(fs);
                    }

                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                    SetDefaultInput();
                }
            }
            else
                SetDefaultInput();

            foreach (PlayerInput input in InputConfigs)
                input.ResetVirtualNodes();

            TrySetGamepadsOnInput();
            AvailablePlayers.Add(InputConfigs[0]);
            OnPlayerJoined?.Invoke(InputConfigs[0]);

            Input.Emitter.AddObserver(InputEventType.GamePadConnected, evt => _availableGamepads.Add(evt.GamePadIndex));
            Input.Emitter.AddObserver(InputEventType.GamePadDisconnected, evt => _availableGamepads.Remove(evt.GamePadIndex));
        }

        void TrySetGamepadsOnInput()
        {
            foreach(PlayerInput input in InputConfigs)
            {
                if (input.IsGamepad && _availableGamepads.Count > 0)
                {
                    input.SetGamepadIndex(_availableGamepads[0]);
                    _availableGamepads.Remove(_availableGamepads[0]);
                }
                else
                    input.IsGamepad = false;
            }
        }

        void SetDefaultInput()
        {
            InputConfigs.Add(new PlayerInput() {
                KeyLeft = Keys.A,
                KeyRight = Keys.D,
                KeyUp = Keys.W,
                KeyDown = Keys.S,
                KeyJump = Keys.Space,
                KeyAttack = Keys.J,
                KeySpecial = Keys.K,
                KeyDash = Keys.L,
                KeyStart = Keys.Enter,
                KeySelect = Keys.Tab,
                KeyInteract = Keys.E,
            });
            InputConfigs.Add(new PlayerInput() {
                KeyLeft = Keys.Left, KeyRight = Keys.Right, KeyDown = Keys.Down, KeyUp = Keys.Up,
                KeyAttack = Keys.NumPad1, KeyJump = Keys.NumPad0, KeySpecial = Keys.NumPad2, KeyDash = Keys.NumPad3,
                KeySelect = Keys.Subtract, KeyStart = Keys.Multiply, KeyInteract = Keys.NumPad9
            });
            ApplyChanges();
        }

        public override void Update()
        {
            foreach(PlayerInput input in InputConfigs)
            {
                if(input.Start.IsPressed && AvailablePlayers.Contains(input) is false)
                {
                    AvailablePlayers.Add(input);
                    OnPlayerJoined?.Invoke(input);
                }
                else if (input.StartPressedOnGamepads(_availableGamepads, out int idx) && AvailablePlayers.Contains(input) is false)
                {
                    // TODO: Check code
                    input.SetGamepadIndex(idx);
                    AvailablePlayers.Add(input);
                    OnPlayerJoined.Invoke(input);
                }
            }

            foreach(PlayerInput player in AvailablePlayers)
            {
                if(player.GamePad?.IsConnected() is false)
                {
                    Debug.Log($"Player disconnected");
                }
            }
        }

        public void ApplyChanges()
        {
            Directory.CreateDirectory(Directory.GetParent(INPUT_PATH).FullName);
            using (FileStream fs = File.Create(INPUT_PATH))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<PlayerInput>));
                serializer.Serialize(fs, InputConfigs);
            }
        }
    }
}
