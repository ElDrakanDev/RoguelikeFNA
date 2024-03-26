using Nez;

namespace RoguelikeFNA
{
    public interface IInteractListener
    {
        public void OnHover(Entity source);
        public void OnInteract(Entity source);
    }
}
