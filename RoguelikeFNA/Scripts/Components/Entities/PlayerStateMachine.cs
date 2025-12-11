using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using RoguelikeFNA.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    public abstract class PlayerState : State<GameEntity>
    {
        
    }

    public class PlayerStateMachine<GameEntity> : StateMachine<GameEntity>
    {
        public PlayerInput Input {get; protected set;}

        public PlayerStateMachine(GameEntity context, State<GameEntity> initialState) : base(context, initialState)
        {
        }
    }
}