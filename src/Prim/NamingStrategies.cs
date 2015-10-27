using System;

namespace Prim
{
    public static class NamingStrategies
    {
        public static string ClassName(Type t)
        {
            return t.Name;
        }
    }
}