using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class PlayerFollow : Component, IUpdatable
    {
        float _frequency = 0.05f;
        float _counter;
        List<Entity> _players;

        public void Update()
        {
            _counter += Time.UnscaledDeltaTime;
            if(_counter > _frequency)
            {
                _counter = 0;
                _players = Entity.Scene.FindEntitiesWithTag((int)Tag.Player);
            }

            _players = _players.Where(p => p.Enabled && !p.IsDestroyed).ToList();
            if(_players.Count > 0)
            {
                Entity.Position = Vector2.Zero;
                foreach(var player in _players)
                    Entity.Position += player.Position;
                Entity.Position /= _players.Count;
            }
        }
    }
}