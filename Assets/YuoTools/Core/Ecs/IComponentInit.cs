namespace YuoTools.Main.Ecs
{
    public interface IComponentInit<T>
    {
        public void ComponentInit(T data);
    }
}