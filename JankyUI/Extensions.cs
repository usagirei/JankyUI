using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JankyUI
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null)
                return true;

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

            if (input.Length == 0)
                return new string[0];

            return Enumerate().ToArray();
        }

        public static TypeConverter GetConverter(this Type type)
        {
            if (type == typeof(UnityEngine.Texture))
                return TextureConverter.Instance;
            if (type == typeof(UnityEngine.Texture2D))
                return TextureConverter.Instance;
            return TypeDescriptor.GetConverter(type);
        }

        public static bool TryConvertTo(this string input, Type type, out object output, char arraySeparator = ';')
        {
            try
            {
                if (input == null)
                {
                    output = type.IsValueType ? Activator.CreateInstance(type) : null;
                    return true;
                }

                if (type.IsArray)
                {
                    var elemType = type.GetElementType();
                    var converter = elemType.GetConverter();
                    var elements = input.SplitEx('\\', arraySeparator);

                    if (elemType == typeof(string))
                    {
                        output = elements;
                        return true;
                    }

                    var array = (Array)Activator.CreateInstance(type, elements.Length);
                    for (int i = 0; i < elements.Length; i++)
                    {
                        var converted = converter.ConvertFromInvariantString(elements[i]);
                        array.SetValue(converted, i);
                    }
                    output = array;
                    return true;
                }
                else
                {
                    var converter = type.GetConverter();
                    output = converter.ConvertFromInvariantString(input);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[JankConverter]" + ex.Message);
                output = type.IsValueType ? Activator.CreateInstance(type) : null;
                return false;
            }
        }

        public static bool TryConvertTo<T>(this string input, out T output, char arraySeparator = ';')
        {
            output = default(T);
            if (!TryConvertTo(input, typeof(T), out var cvt, arraySeparator))
                return false;

            output = (T)cvt;
            return true;
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
