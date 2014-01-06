using System;
using System.Management.Automation;
using System.Reflection;
using VsConsole.PowerPack.Host.Properties;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    public class PSTypeWrapper : TypeWrapper<PSObject>
    {
        private static ScriptBlock _addWrapperMembersScript;

        internal override MethodBinder Binder
        {
            get
            {
                return (MethodBinder)PSTypeWrapper.PSMethodBinder.Instance;
            }
        }

        private static ScriptBlock AddWrapperMembersScript
        {
            get
            {
                return PSTypeWrapper._addWrapperMembersScript ?? (PSTypeWrapper._addWrapperMembersScript = ScriptBlock.Create(Resources.Add_WrapperMembers));
            }
        }

        private PSTypeWrapper(object wrappedObject)
            : base(wrappedObject)
        {
        }

        protected override PSObject CreateInterfaceWrapper(TypeWrapper<PSObject> wrapper, Type interfaceType)
        {
            PSObject psObject = new PSObject((object)wrapper);
            PSTypeWrapper.AddWrapperMembersScript.Invoke((object)psObject, wrapper.WrappedObject, (object)interfaceType);
            return psObject;
        }

        public static PSObject GetInterface(object scriptObject, Type interfaceType)
        {
            return TypeWrapper<PSObject>.GetInterface(scriptObject, interfaceType, (Func<object, TypeWrapper<PSObject>>)(obj => (TypeWrapper<PSObject>)(obj as PSTypeWrapper) ?? (TypeWrapper<PSObject>)new PSTypeWrapper(obj)));
        }

        public static object InvokeMethod(object target, MethodInfo method, PSObject[] parameters)
        {
            return PSTypeWrapper.PSMethodBinder.Instance.Invoke(method, target, (object[])parameters);
        }

        private class PSMethodBinder : MethodBinder
        {
            public static readonly PSTypeWrapper.PSMethodBinder Instance = new PSTypeWrapper.PSMethodBinder();

            static PSMethodBinder()
            {
            }

            private PSMethodBinder()
            {
            }

            private static object GetBaseObject(PSObject arg)
            {
                if (arg == null)
                    return (object)null;
                else
                    return arg.BaseObject;
            }

            protected override bool IsUnwrapArgsNeeded(ParameterInfo[] paramInfos)
            {
                return true;
            }

            protected override bool TryConvertArg(ParameterInfo paramInfo, object arg, out object argValue)
            {
                object baseObject = PSTypeWrapper.PSMethodBinder.GetBaseObject(arg as PSObject);
                if (!paramInfo.IsOut)
                {
                    argValue = MethodBinder.ChangeType(paramInfo, baseObject);
                    return true;
                }
                else if (baseObject is PSReference)
                {
                    argValue = paramInfo.IsIn ? MethodBinder.ChangeType(paramInfo, ((PSReference)baseObject).Value) : (object)null;
                    return true;
                }
                else
                {
                    argValue = (object)null;
                    return false;
                }
            }

            protected override bool TryReturnArg(ParameterInfo paramInfo, object arg, object argValue)
            {
                if (!paramInfo.IsOut)
                    return true;
                object baseObject = PSTypeWrapper.PSMethodBinder.GetBaseObject(arg as PSObject);
                if (!(baseObject is PSReference))
                    return false;
                ((PSReference)baseObject).Value = argValue;
                return true;
            }
        }
    }
}