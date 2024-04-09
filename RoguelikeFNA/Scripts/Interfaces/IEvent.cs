namespace RoguelikeFNA
{
    public interface IEvent<T>
    {
        public void Fire(T param);
    }
}
