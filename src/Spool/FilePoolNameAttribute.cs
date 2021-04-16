using System;
using System.Reflection;

namespace Spool
{
    /// <summary>
    /// 文件池名称特性标签
    /// </summary>
    public class FilePoolNameAttribute : Attribute
    {
        public string Name { get; }

        public FilePoolNameAttribute(string name)
        {
            Name = name;
        }

        public virtual string GetName(Type type)
        {
            return Name;
        }

        /// <summary>
        /// 根据泛型获取文件池名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetFilePoolName<T>()
        {
            return GetFilePoolName(typeof(T));
        }

        /// <summary>
        /// 根据类型获取文件池名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetFilePoolName(Type type)
        {
            var nameAttribute = type.GetCustomAttribute<FilePoolNameAttribute>();

            if (nameAttribute == null)
            {
                return type.FullName;
            }

            return nameAttribute.GetName(type);
        }
    }
}
