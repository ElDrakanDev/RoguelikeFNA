using Nez;
using System.IO;
using System.Xml.Serialization;
using System;


namespace RoguelikeFNA
{
    public class InputManager : GlobalManager
    {
        public PlayerInput[] Players { get; private set; } = new PlayerInput[2];
        const string INPUT_PATH = "./data/input.data";

        public override void OnEnabled()
        {
            base.OnEnabled();

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerInput[]));
            if (File.Exists(INPUT_PATH))
            {
                try
                {
                    using (FileStream fs = File.OpenRead(INPUT_PATH))
                    {
                        Players = (PlayerInput[])serializer.Deserialize(fs);
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

            foreach (PlayerInput input in Players)
                input.ResetVirtualNodes();
        }

        void SetDefaultInput()
        {
            Players[0] = new PlayerInput();
            Players[1] = new PlayerInput(0);
            ApplyChanges();
        }

        public void ApplyChanges()
        {
            Directory.CreateDirectory(Directory.GetParent(INPUT_PATH).FullName);
            using (FileStream fs = File.Create(INPUT_PATH))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerInput[]));
                serializer.Serialize(fs, Players);
            }
        }
    }
}
