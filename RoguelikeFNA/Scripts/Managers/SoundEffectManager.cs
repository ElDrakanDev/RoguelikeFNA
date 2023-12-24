using Microsoft.Xna.Framework.Audio;
using Nez;

namespace RoguelikeFNA
{
    public class SoundEffectManager : GlobalManager
    {
        ConfigManager _config;
        float _volumeMultiplier => _config.Config.SoundEffectVolume * _config.Config.MasterVolume;

        public override void OnEnabled()
        {
            base.OnEnabled();
            _config = Core.GetGlobalManager<ConfigManager>();
        }

        public void Play(SoundEffect sfx)
        {
            float volume = _volumeMultiplier;
            if(volume > 0)
                sfx.Play(volume, 0 ,0);
        }

        public void Play(SoundEffect sfx, float volume = 1, float pitch = 0, float pan = 0)
        {
            float finalVolume = _volumeMultiplier * volume;
            if (finalVolume > 0)
                sfx.Play(finalVolume, pitch, pan);
        }
    }
}
