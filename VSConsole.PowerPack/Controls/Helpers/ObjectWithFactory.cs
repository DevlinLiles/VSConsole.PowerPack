namespace VSConsole.PowerPack.Core
{
    public class ObjectWithFactory<T>
    {
        public ObjectWithFactory(T factory)
        {
            UtilityMethods.ThrowIfArgumentNull(factory);
            Factory = factory;
        }

        public T Factory { get; private set; }
    }
}