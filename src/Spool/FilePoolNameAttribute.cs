using System;
using System.Reflection;

namespace Spool
{
    /// <summary>
    /// File pool name attribute
    /// </summary>
    public class FilePoolNameAttribute : Attribute
    {
        /// <summary>
        /// The name of file pool
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">File pool name</param>
        public FilePoolNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Get name of file pool
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string GetName(Type type)
        {
            return Name;
        }

        /// <summary>
        /// Get the file pool name with type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetFilePoolName<T>()
        {
            return GetFilePoolName(typeof(T));
        }

        /// <summary>
        /// Get the file pool name with type
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
