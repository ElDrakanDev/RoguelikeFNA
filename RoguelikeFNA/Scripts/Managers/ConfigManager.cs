using System;
using System.Collections.Generic;
using Nez;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace RoguelikeFNA
{
    public class ConfigManager : GlobalManager
    {
        const string CONFIG_PATH = "./data/config.data";

        public GameConfig Config { get; private set; }
        public override void OnEnabled()
        {
            base.OnEnabled();

            XmlSerializer serializer = new XmlSerializer(typeof(GameConfig));
            if(File.Exists(CONFIG_PATH))
            {
                try
                {
                    using(FileStream fs = File.OpenRead(CONFIG_PATH))
                    {
                        Config = (GameConfig)serializer.Deserialize(fs);
                        UpdateScreenConfig();
                    }
                }
                catch(Exception ex)
                {
                    Debug.Log(ex);
                    SetDefaultConfig();
                }
            }
            else
                SetDefaultConfig();
        }

        /// <summary>
        /// Saves changes to a file and updates screen if it was changed
        /// </summary>
        public void ApplyChanges()
        {
            UpdateScreenConfig();

            Directory.CreateDirectory(Directory.GetParent(CONFIG_PATH).FullName);
            using (FileStream fs = File.Create(CONFIG_PATH))
            {
                XmlSerializer serializer = new XmlSerializer (typeof(GameConfig));
                serializer.Serialize(fs, Config);
            }
        }

        void UpdateScreenConfig()
        {
            if (Config.Fullscreen != Screen.IsFullscreen)
            {
                Screen.IsFullscreen = Config.Fullscreen;
                Screen.ApplyChanges();
            }
            if (Config.ScreenResolution != ScreenResolution.Current())
                Screen.SetSize(Config.ScreenResolution.width, Config.ScreenResolution.height);
        }

        void SetDefaultConfig()
        {
            Config = new GameConfig();
            Config.Fullscreen = false;
            Config.MasterVolume = 100;
            Config.BackgroundMusicVolume = 100;
            Config.SoundEffectVolume = 100;
            Config.ScreenResolution = ScreenResolution.Current();
            Config.Language = "english";

            ApplyChanges();
        }
    }

    [System.Serializable]
    public class GameConfig
    {
        public bool Fullscreen = false;
        float _masterVolume;
        public float MasterVolume {
            get => Mathf.Clamp(_masterVolume, 0, 100);
            set => _masterVolume = Mathf.Clamp(value, 0, 100); 
        }
        float _backgroundMusicVolume;
        public float BackgroundMusicVolume
        {
            get => Mathf.Clamp(_backgroundMusicVolume, 0, 100);
            set => _backgroundMusicVolume = Mathf.Clamp(value, 0, 100);
        }
        float _soundEffectVolume;
        public float SoundEffectVolume
        {
            get => Mathf.Clamp(_soundEffectVolume, 0, 100);
            set => _soundEffectVolume = Mathf.Clamp(value, 0, 100);
        }
        public ScreenResolution ScreenResolution;
        public string Language;
    }

    [System.Serializable]
    public class ScreenResolution
    {
        public int width;
        public int height;

        public override string ToString() => $"{width}x{height}";
        public override bool Equals(object obj)
        {
            if (obj is ScreenResolution res)
                return res.width == width && res.height == height;
            return base.Equals(obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public static ScreenResolution Current() => new ScreenResolution() {width = Screen.Width, height = Screen.Height };

        public static List<ScreenResolution> GetAvailableResolutions()
        {
            HashSet<ScreenResolution> uniqueResolutions = new HashSet<ScreenResolution>();
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                uniqueResolutions.Add(new ScreenResolution(){
                    width = mode.Width, height = mode.Height,
                });
            }
            return uniqueResolutions.ToList();
        }

        public static ScreenResolution Parse(string resolution)
        {
            string[] parts = resolution.Split(new char[]{Char.Parse("x")}, count:2);
            int width = int.Parse(parts[0]);
            int height = int.Parse(parts[1]);
            return new ScreenResolution() { width=width, height=height };
        }
    }
}
