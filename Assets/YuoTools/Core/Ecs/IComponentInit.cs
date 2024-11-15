namespace YuoTools.Core.Ecs
{
    public interface IComponentInit<T>
    {
        public void ComponentInit(T componentInitData);
    }
}