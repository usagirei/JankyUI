using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JankyUI
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null) return true;
            return string.IsNullOrEmpty(value.Trim());
        }

        public static T GetCustomAttribute<T>(this Type target, bool inherit)
            where T : Attribute
        {
            var attributes = target.GetCustomAttributes<T>(inherit);
            return attributes.FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type target, bool inherit)
            where T : Attribute
        {
            return target.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x);
        }

        public static string[] SplitEx(this string input, char escape, char separator)
        {
            IEnumerable<string> Enumerate()
            {
                string[] spl = input.Split(separator);
                StringBuilder builder = null;

                foreach (string str in spl)
                {
                    if (str.Last() == escape)
                    {
                        if (builder == null)
                            builder = new StringBuilder();

                        builder.Append(str, 0, str.Length - 1);
                        builder.Append(separator);
                    }
                    else
                    {
                        if (builder == null || builder.Length == 0)
                        {
                            yield return str;
                        }
                        else
                        {
                            builder.Append(str);
                            yield return builder.ToString();
                            builder.Length = 0;
                        }
                    }
                }
                if (builder != null && builder.Length != 0)
                {
                    yield return builder.ToString();
                }
            }

            return Enumerate().ToArray();
        }

        public static bool IsCompatibleWithDelegate<TDelegate>(this MethodInfo method)
            where TDelegate : class
        {
            Type delegateType = typeof(TDelegate);
            MethodInfo delegateSignature = delegateType.GetMethod("Invoke");

            bool parametersEqual = delegateSignature
                .GetParameters()
                .Select(x => x.ParameterType)
                .SequenceEqual(method.GetParameters()
                    .Select(x => x.ParameterType));

            return delegateSignature.ReturnType == method.ReturnType && parametersEqual;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
