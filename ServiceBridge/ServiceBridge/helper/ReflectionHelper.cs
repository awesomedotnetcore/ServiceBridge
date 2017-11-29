using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ServiceBridge.helper
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPropertyInfoList(Type t)
        {
            return t.GetProperties().ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<MethodInfo> GetMethodInfoList(Type t)
        {
            return t.GetMethods().ToList();
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public static object InvokeMethod(string className, string methodName, object[] Parameter)
        {
            var curAss = Assembly.GetExecutingAssembly();

            var ajaxClass = curAss.GetType(className, false, false);

            if (ajaxClass == null)
            {
                return null;
            }

            var obj = Activator.CreateInstance(ajaxClass);

            var method = ajaxClass.GetMethod(methodName);

            if (method == null)
            {
                return null;
            }

            if (Parameter != null)
            {
                Parameter = new object[] { Parameter };
            }
            else
            {
                Parameter = new object[] { };
            }

            return method.Invoke(obj, Parameter);
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="name_space"></param>
        /// <param name="class_name"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string assembly, string classNameWithNameSpace)
        {
            var ass = Assembly.Load(assembly);
            if (ass == null) { return default(T); }
            var instance = ass.CreateInstance(classNameWithNameSpace);
            if (ValidateHelper.Is<T>(instance))
            {
                return (T)instance;
            }
            return default(T);
        }
        public static T CreateInstanceFromFile<T>(string filePath, string classNameWithNameSpace)
        {
            var ass = Assembly.LoadFile(filePath);
            if (ass == null) { return default(T); }
            var instance = ass.CreateInstance(classNameWithNameSpace);
            if (ValidateHelper.Is<T>(instance))
            {
                return (T)instance;
            }
            return default(T);
        }
        /// <summary>
        /// class property method都继承值memberinfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static List<T> GetAttributes<T>(MemberInfo member)
        {
            if (member == null) { return null; }
            var attrs = member.GetCustomAttributes(typeof(T), false);
            if (!ValidateHelper.IsPlumpList(attrs)) { return null; }
            var list = new List<T>();
            foreach (T obj in attrs)
            {
                list.Add(obj);
            }
            return list;
        }
    }
}
