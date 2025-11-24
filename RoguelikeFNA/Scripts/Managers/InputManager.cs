using Nez;
using Nez.Persistence;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Linq;

namespace RoguelikeFNA
{
    public class InputManager : GlobalManager
    {
        public const string DEFAULT_KEYBOARD_NAME = "Default (Keyboard)";
        public const string DEFAULT_GAMEPAD_NAME = "Default (GamePad)";

        public List<PlayerProfile> Profiles { get; private set; } = new List<PlayerProfile>();
        public List<PlayerInput> AvailablePlayers { get; private set; } = new List<PlayerInput> { };
        List<int> _availableGamepads = new List<int>();
        Dictionary<int, PlayerInput> _gamepadPlayers = new Dictionary<int, PlayerInput>();
        const string INPUT_PATH = "./data/input.data";

        public event Action<PlayerInput> OnPlayerJoined;
        public event Action<PlayerInput> OnPlayerLeft;

        public override void OnEnabled()
        {
            base.OnEnabled();

            _availableGamepads = new List<int>() { 0, 1, 2, 3 };
            _availableGamepads = _availableGamepads.Where(idx => idx < Input.GamePads.Length && Input.GamePads[idx].IsConnected()).ToList();

            if (File.Exists(INPUT_PATH))
            {
                try
                {
                    var text = File.ReadAllText(INPUT_PATH);
                    Profiles = Json.FromJson<List<PlayerProfile>>(text);
                }
                catch (Exception ex)
                {
                    Debug.Error(ex.ToString());
                    SetDefaultInput();
                }
            }
            else
                SetDefaultInput();

            // foreach (PlayerInput input in )
            //     input.ResetVirtualNodes();

            // TrySetGamepadsOnInput();
            AddPlayer(new PlayerInput(GetProfile(DEFAULT_KEYBOARD_NAME)));

            Input.Emitter.AddObserver(InputEventType.GamePadConnected, evt => _availableGamepads.Add(evt.GamePadIndex));
            Input.Emitter.AddObserver(InputEventType.GamePadDisconnected, evt => _availableGamepads.Remove(evt.GamePadIndex));
        }

        // void TrySetGamepadsOnInput()
        // {
        //     foreach(PlayerInput input in InputConfigs)
        //     {
        //         if (input.IsGamepad && _availableGamepads.Count > 0)
        //         {
        //             input.SetGamepadIndex(_availableGamepads[0]);
        //             _availableGamepads.Remove(_availableGamepads[0]);
        //         }
        //         else
        //             input.IsGamepad = false;
        //     }
        // }

        void SetDefaultInput()
        {
            var defaultKeyboardProfile = new PlayerProfile(){
                Name = DEFAULT_KEYBOARD_NAME,
                ReadonlyName = true
            };
            var defaultGamepadProfile = new PlayerProfile() {
                Name = DEFAULT_GAMEPAD_NAME,
                ReadonlyName = true,
                UseGamepad = true
            };
            Profiles.Add(defaultKeyboardProfile);
            Profiles.Add(defaultGamepadProfile);
            ApplyChanges();
        }

        public PlayerProfile GetProfile(string name)
        {
            return Profiles.Find(p => p.Name == name);
        }

        public void AddPlayer(PlayerInput input)
        {
            AvailablePlayers.Add(input);
            input.ResetVirtualNodes();
            OnPlayerJoined?.Invoke(input);
        }

        public override void Update()
        {
            // foreach(PlayerInput input in Profiles)
            // {
            //     if(input.Start.IsPressed && AvailablePlayers.Contains(input) is false)
            //     {
            //         input.IsGamepad = false;
            //         AvailablePlayers.Add(input);
            //         OnPlayerJoined?.Invoke(input);
            //     }
            //     else if (input.StartPressedOnGamepads(_availableGamepads, out int idx) && AvailablePlayers.Contains(input) is false)
            //     {
            //         // TODO: Check code
            //         input.IsGamepad = true;
            //         input.SetGamepadIndex(idx);
            //         AvailablePlayers.Add(input);
            //         OnPlayerJoined?.Invoke(input);
            //     }
            // }

            // foreach(PlayerInput player in AvailablePlayers)
            // {
            //     if (player.GamePad?.IsConnected() is false)
            //     {
            //         AvailablePlayers.Remove(player);
            //         OnPlayerLeft?.Invoke(player);
            //     }
            // }
        }

        public void ApplyChanges()
        {
            Directory.CreateDirectory(Directory.GetParent(INPUT_PATH).FullName);
            var text = JsonEncoder.ToJson(Profiles, new(){PrettyPrint=true, TypeNameHandling=TypeNameHandling.All});
            File.WriteAllText(INPUT_PATH, text);
        }
    }
}
