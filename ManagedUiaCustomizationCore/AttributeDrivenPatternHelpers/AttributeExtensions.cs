using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UIAControls
{
    internal static class AttributeExtensions
    {
        internal static IEnumerable<TA> GetAttributes<TA>(this MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(typeof(TA), true).Cast<TA>();
        }
        
        internal static TA GetAttribute<TA>(this MemberInfo memberInfo)
        {
            return memberInfo.GetAttributes<TA>().FirstOrDefault();
        }

        internal static IEnumerable<MethodInfo> GetMethodsMarkedWith<TAttribute>(this Type type)
        {
            return from m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   where m.GetAttributes<TAttribute>().Any()
                   select m;
        }

        internal static IEnumerable<PropertyInfo> GetPropertiesMarkedWith<TAttribute>(this Type type)
        {
            return from m in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                   where m.GetAttributes<TAttribute>().Any()
                   select m;
        }
    }
}