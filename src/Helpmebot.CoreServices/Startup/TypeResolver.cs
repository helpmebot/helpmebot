namespace Helpmebot.CoreServices.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class TypeResolver
    {
        public static Type GetType(string typeName)
        {
            return Type.GetType(typeName, ResolveAssembly, ResolveType, true);
        }

        private static Type ResolveType(Assembly assembly, string typeName, bool caseSensitive)
        {
            List<Type> typeList;
            if (assembly == null)
            {
                typeList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToList();
            }
            else
            {
                typeList = assembly.GetTypes().ToList();
            }

            var fullNameMatch = typeList.Where(x => x.FullName == typeName).ToList();
            if (fullNameMatch.Count == 1)
            {
                return fullNameMatch.First();
            }

            if (fullNameMatch.Count > 1)
            {
                return null;
            }
            
            var nameMatch = typeList.Where(x => x.Name == typeName).ToList();
            if (nameMatch.Count == 1)
            {
                return nameMatch.First();
            }

            return null;
        }

        private static Assembly ResolveAssembly(AssemblyName lookupName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == lookupName.Name);
        }
    }
}