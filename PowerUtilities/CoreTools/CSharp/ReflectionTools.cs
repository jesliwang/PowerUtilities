﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class ReflectionTools
    {
        /// <summary>
        /// flags : private instance
        /// </summary>
        public const BindingFlags instanceBindings = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
        public const BindingFlags callBindings = instanceBindings | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty;


        /// <summary>
        /// type's methods has attribute
        /// 
        /// {type : [methodInfos]}
        /// </summary>
        static Dictionary<Type, MethodInfo[]> typeHasAttributeMethodsDict = new Dictionary<Type, MethodInfo[]>();

        /// <summary>
        /// types has attributeType
        /// 
        /// {attrType : [types]}
        /// </summary>
        static Dictionary<Type, Type[]> attributeTypesDict = new Dictionary<Type, Type[]>();


        static ReflectionTools()
        {
            typeHasAttributeMethodsDict.Clear();
            attributeTypesDict.Clear();
        }

        /// <summary>
        /// Get type's methods have attribute<T> with cached
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsHasCustomAttribute<T>(Type type) where T : Attribute
        {
            if (!typeHasAttributeMethodsDict.TryGetValue(type, out var methods))
            {
                methods = type.GetMethods(callBindings);
            }

            var methodArr = methods.Where(m => m.GetCustomAttribute<T>() != null).ToArray();
            typeHasAttributeMethodsDict[type] = methodArr;
            return methodArr;
        }

        /// <summary>
        /// Get types has attribute<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Type[] GetTypesHasAttribute<T>() where T : Attribute
        {
            var attrType = typeof(T);
            if(!attributeTypesDict.TryGetValue(attrType,out var types) || types.Length == 0)
            {
                types = typeof(T).Assembly.GetTypes();
            }
            var typeArr = types.Where(t => t.GetCustomAttribute<T>() != null)
                .ToArray();

            attributeTypesDict[attrType] = typeArr;
            return typeArr;
        }

        /// <summary>
        /// Get methodInfo[] from all types
        /// 
        /// first time cost 11ms
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsHasAttribute<T>(params string[] dllStartWithNames) where T : Attribute
        {

            var list = new List<MethodInfo>();
            var attrType = typeof(T);
            if (!typeHasAttributeMethodsDict.TryGetValue(attrType, out var methods))
            {
                var dlls = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(dll => IsContains(dll.FullName))
                    .ToArray();

                foreach (var dll in dlls)
                {
                    var types = dll.GetTypes();
                    //var types = typeof(T).Assembly.GetTypes();
                    methods = types.SelectMany(t => GetMethodsHasCustomAttribute<T>(t))
                        .Where(ms => ms != null)
                        .ToArray();
                    list.AddRange(methods);
                }
            }

            return typeHasAttributeMethodsDict[attrType] = list.ToArray();

            //==========
            bool IsContains(string fullname)
            {
                if (dllStartWithNames.Length == 0)
                    return true;

                foreach (var name in dllStartWithNames)
                {

                    if(fullname.StartsWith(name))
                        return true;
                }
                return false;
            }
        }

        public static IEnumerable<Type> GetTypesDerivedFrom<T>(Func<Type, bool> predication)
        {
            var types = typeof(T).Assembly.GetTypes();

            if (predication == null)
                return Enumerable.Empty<Type>();

            return types.Where(predication);
        }

        public static IEnumerable<Type> GetTypesDerivedFrom<T>()
        {
            var tType = typeof(T);
            return GetTypesDerivedFrom<T>(t => t.BaseType != null && t.BaseType == tType);
        }

        public static IEnumerable<Type> GetAppDomainTypesDerivedFrom<T>()
        {
            var tType = typeof(T);
            return GetAppDomainTypesDerivedFrom<T>(t => t.BaseType != null && t.BaseType == tType);
        }

        public static IEnumerable<Type> GetAppDomainTypesDerivedFrom<T>(Func<Type, bool> predicate)
        {
            if (predicate == null)
                return Enumerable.Empty<Type>();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().Where(predicate))
            ;
        }

        /// <summary>
        /// Is type implements interface?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsImplementOf(this Type type, Type interfaceType)
        {
            return type.GetInterfaces()
                .Any(obj => (obj.IsGenericType && obj.GetGenericTypeDefinition() == interfaceType)
                    || interfaceType.IsAssignableFrom(type)
                )
                ;
        }

        /// <summary>
        /// Get a private field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetFieldValue<T>(this Type type, object instance, string fieldName, BindingFlags flags = instanceBindings)
        {
            var obj = GetFieldValue(type, instance, fieldName, flags);
            return obj != null ? (T)obj : default;
        }
        public static object GetFieldValue(this Type type, object instance, string fieldName, BindingFlags flags = instanceBindings)
        {
            var field = type.GetField(fieldName, flags);
            return field != null ? field.GetValue(instance) : default;
        }

        public static void SetFieldValue(this Type type, object instance, string fieldName, object value, BindingFlags flags = instanceBindings)
        {
            var field = type.GetField(fieldName, flags);
            if (field != null)
                field.SetValue(instance, value);
        }

        /// <summary>
        /// Get a private Property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="caller"></param>
        /// <param name="propertyName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this Type type, object caller, string propertyName, BindingFlags flags = instanceBindings)
        {
            var obj = GetPropertyValue(type,caller, propertyName, flags);
            return obj != null ? (T)obj : default;
        }

        public static object GetPropertyValue(this Type type,object caller,string propertyName,BindingFlags flags = instanceBindings)
        {
            var prop = type.GetProperty(propertyName, flags);
            return prop != null ? prop.GetValue(caller) : default;
        }

        /// <summary>
        /// Get hierarchy object value use Reflection API(slow)
        /// fieldExpress : like ( object1.object2.object3.name )
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldExpress"></param>
        /// <returns></returns>
        public static object GetObjectHierarchy(this object instance, string fieldExpress)
        {
            if (instance == null || string.IsNullOrEmpty(fieldExpress))
                return null;

            var fieldNames = fieldExpress.SplitBy('.');

            Type instType = instance.GetType();
            FieldInfo field = null;

            foreach (var fieldName in fieldNames)
            {
                field = instType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null)
                {
                    throw new ArgumentException($"{instance} dont have path: {fieldExpress}");
                }

                instance = field.GetValue(instance);
                // current object is null
                if (instance == null)
                    return null;
                // next field
                instType = instance.GetType();
            }
            return instance;
        }

        /// <summary>
        /// chain call method,field,property.
        /// 
        /// a.Test().Length();
        /// 
        /// var a = new A();
        /// var lastResult = InvokeMemberChain(typeof(A), a, new[] { "Test", "Length" }, new List<object[]> { new object[] { "123" }, null });
        /// WriteLine(lastResult);
        /// </summary>
        /// <param name="fistCallerType"></param>
        /// <param name="firstCaller"></param>
        /// <param name="memberNames"></param>
        /// <param name="args"></param>
        /// <returns></returns>

        public static object InvokeMemberChain(this Type fistCallerType, object firstCaller, string[] memberNames, List<object[]> args)
        {

            var caller = firstCaller;
            var callerType = fistCallerType;
            for (int i = 0; i < memberNames.Length; i++)
            {
                var member = memberNames[i];
                var param = args[i];

                // next method
                caller = callerType?.InvokeMember(member, callBindings, null, caller, param);
                callerType = caller?.GetType();
            }
            return caller;
        }

        public static object InvokeMember(this Type type, string name, object caller, object[] args)
        {
            return type.InvokeMember(name, callBindings, null, caller, args);
        }

        public static object InvokeMethod(this Type type, string name, Type[] argTypes, object caller, object[] args)
        {
            argTypes = argTypes ?? Type.EmptyTypes;
            if(argTypes == Type.EmptyTypes)
            {
                return type.GetMethod(name).Invoke(caller, args);
            }

            return type.GetMethod(name, instanceBindings, null, argTypes, null)?.Invoke(caller, args);
        }

        public static object GetPropertyValue(this Type type, string name,object caller, object[] args)
        {
            return type.GetProperty(name).GetValue(caller, args);
        }

    }
}
