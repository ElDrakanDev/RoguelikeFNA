using Microsoft.Xna.Framework.Audio;
using Nez;

namespace RoguelikeFNA
{
    public class SoundEffectManager : GlobalManager
    {
        static SoundEffectManager _instance;
        ConfigManager _config;
        float _volumeMultiplier => _config.Config.SoundEffectVolume * _config.Config.MasterVolume;

        public override void OnEnabled()
        {
            base.OnEnabled();
            _config = Core.GetGlobalManager<ConfigManager>();
            _instance = this;
        }

        public static void Play(SoundEffect sfx, float volume = 1, float pitch = 0, float pan = 0)
        {
            float finalVolume = _instance._volumeMultiplier * volume;
            if (finalVolume > 0)
                sfx.Play(finalVolume, pitch, pan);
        }
    }
}
