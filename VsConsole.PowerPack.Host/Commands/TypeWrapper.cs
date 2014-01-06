using System;
using System.Collections.Generic;

namespace VSConsole.PowerPack.Core
{
    public abstract class TypeWrapper<T> where T : class
    {
        private Dictionary<Type, T> _interfaceMap = new Dictionary<Type, T>((IEqualityComparer<Type>)TypeWrapper<T>.TypeEquivalenceComparer.Instance);
        private object _interfaceMapLock = new object();

        internal object WrappedObject { get; private set; }

        internal abstract MethodBinder Binder { get; }

        protected TypeWrapper(object wrappedObject)
        {
            UtilityMethods.ThrowIfArgumentNull<object>(wrappedObject);
            this.WrappedObject = wrappedObject;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
                return obj.Equals(this.WrappedObject);
        }

        public override int GetHashCode()
        {
            return this.WrappedObject.GetHashCode();
        }

        public override string ToString()
        {
            return this.WrappedObject.ToString();
        }

        protected T GetInterface(Type interfaceType)
        {
            if (!interfaceType.IsInstanceOfType(this.WrappedObject))
                return default(T);
            lock (this._interfaceMapLock)
            {
                T local_0;
                if (this._interfaceMap.TryGetValue(interfaceType, out local_0))
                    return local_0;
                T local_0_1 = this.CreateInterfaceWrapper(this, interfaceType);
                this._interfaceMap[interfaceType] = local_0_1;
                return local_0_1;
            }
        }

        protected abstract T CreateInterfaceWrapper(TypeWrapper<T> wrapper, Type interfaceType);

        protected static T GetInterface(object scriptObject, Type interfaceType, Func<object, TypeWrapper<T>> getTypeWrapper)
        {
            if (scriptObject == null)
                return default(T);
            UtilityMethods.ThrowIfArgumentNull<Type>(interfaceType);
            if (!interfaceType.IsInterface)
                throw new ArgumentException("interfaceType");
            else
                return getTypeWrapper(scriptObject).GetInterface(interfaceType);
        }

        private class TypeEquivalenceComparer : IEqualityComparer<Type>
        {
            public static readonly TypeWrapper<T>.TypeEquivalenceComparer Instance = new TypeWrapper<T>.TypeEquivalenceComparer();

            static TypeEquivalenceComparer()
            {
            }

            private TypeEquivalenceComparer()
            {
            }

            public bool Equals(Type x, Type y)
            {
                return x.IsEquivalentTo(y);
            }

            public int GetHashCode(Type obj)
            {
                return obj.GUID.GetHashCode();
            }
        }
    }
}