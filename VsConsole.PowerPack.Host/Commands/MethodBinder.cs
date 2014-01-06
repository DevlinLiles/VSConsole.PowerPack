using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSConsole.PowerPack.Core
{
    public abstract class MethodBinder
    {
        public bool TryInvoke(Type type, string name, object target, object[] args, out object result)
        {
            MemberInfo[] member = type.GetMember(name, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
            if (member.Length == 1 && member[0] is MethodInfo)
            {
                result = this.Invoke((MethodInfo)member[0], target, args);
                return true;
            }
            else
            {
                result = (object)null;
                return false;
            }
        }

        public object Invoke(MethodInfo method, object target, object[] args)
        {
            object[] objArray = this.UnwrapArgs(method, args);
            object result = method.Invoke(target, objArray);
            return this.WrapResult(method, args, result, objArray);
        }

        protected static object ChangeType(ParameterInfo paramInfo, object arg)
        {
            Type conversionType = paramInfo.ParameterType;
            if (conversionType.IsByRef)
                conversionType = conversionType.GetElementType();
            return Convert.ChangeType(arg, conversionType);
        }

        protected abstract bool TryConvertArg(ParameterInfo paramInfo, object arg, out object argValue);

        protected abstract bool TryReturnArg(ParameterInfo paramInfo, object arg, object argValue);

        protected virtual bool TryGetOptionalArg(ParameterInfo paramInfo, out object argValue)
        {
            if (paramInfo.IsOut || paramInfo.IsOptional)
            {
                argValue = paramInfo.RawDefaultValue;
                if (argValue == DBNull.Value)
                    argValue = (object)null;
                return true;
            }
            else
            {
                argValue = (object)null;
                return false;
            }
        }

        protected virtual object CreateResultTuple(IList<object> allResults)
        {
            return (object)allResults;
        }

        public virtual bool IsType(object arg)
        {
            return arg is Type;
        }

        public virtual Type ConvertToType(object arg)
        {
            return (Type)arg;
        }

        protected virtual bool IsUnwrapArgsNeeded(ParameterInfo[] paramInfos)
        {
            return Enumerable.Any<ParameterInfo>((IEnumerable<ParameterInfo>)paramInfos, (Func<ParameterInfo, bool>)(p => p.IsOut));
        }

        private object[] UnwrapArgs(MethodInfo m, object[] args)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (!this.IsUnwrapArgsNeeded(parameters))
                return args;
            object[] objArray = new object[parameters.Length];
            int index1 = 0;
            for (int index2 = 0; index2 < parameters.Length; ++index2)
            {
                ParameterInfo paramInfo = parameters[index2];
                object argValue;
                if (index1 < args.Length && this.TryConvertArg(paramInfo, args[index1], out argValue))
                {
                    objArray[index2] = argValue;
                    ++index1;
                }
                else
                {
                    if (!this.TryGetOptionalArg(paramInfo, out argValue))
                        throw new MissingMemberException();
                    objArray[index2] = argValue;
                }
            }
            return objArray;
        }

        private object WrapResult(MethodInfo m, object[] args, object result, object[] unwrappedArgs)
        {
            if (args == unwrappedArgs)
                return result;
            List<object> list = new List<object>();
            if (result != null && result.GetType() != typeof(void))
                list.Add(result);
            ParameterInfo[] parameters = m.GetParameters();
            int index1 = 0;
            for (int index2 = 0; index2 < parameters.Length; ++index2)
            {
                ParameterInfo paramInfo = parameters[index2];
                if (index1 < args.Length && this.TryReturnArg(paramInfo, args[index1], unwrappedArgs[index2]))
                    ++index1;
                else if (paramInfo.IsOut)
                    list.Add(unwrappedArgs[index2]);
            }
            if (list.Count <= 1)
                return result;
            else
                return this.CreateResultTuple((IList<object>)list);
        }
    }
}