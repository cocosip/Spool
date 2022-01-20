using System;
using System.Reflection;

namespace Spool
{
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

        public static string GetFilePoolName<T>()
        {
            return GetFilePoolName(typeof(T));
        }

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