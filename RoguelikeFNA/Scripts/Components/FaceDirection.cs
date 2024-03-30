using Nez;
using Microsoft.Xna.Framework;

namespace RoguelikeFNA
{
    public class FaceDirection : Component
    {
        public bool FacingRight { get; private set; } = false;

        public override void OnAddedToEntity()
        {
            FacingRight = Entity.Scale.X >= 0;
        }

        /// <summary>
        /// Update entity scale based on the input faced direction
        /// </summary>
        /// <param name="xInput"></param>
        public void CheckFacingSide(float xInput)
        {
            if ((xInput > 0 && FacingRight is false) || (xInput < 0 && FacingRight is true))
            {
                FacingRight = !FacingRight;
                Entity.Scale *= new Vector2(-1, 1);
            }
        }
    }
}
