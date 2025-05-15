using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsArrayOf<T>(this Type type)
        {
            return type == typeof(T[]);
        }
    }
}
