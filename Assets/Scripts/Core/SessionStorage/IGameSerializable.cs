namespace Core.SessionStorage
{
    public interface IGameSerializable<T>
    {
        public T SerializeSelf();
        public void Deserialize(T self);
    }
}