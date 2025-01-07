namespace YuoTools.Main.Ecs
{
    public interface IComponentInit<T>
    {
        public void ComponentInit(T data);
    }
    
    public interface IComponentInit<T1, T2>
    {
        public void ComponentInit(T1 data, T2 data2);
    }
}