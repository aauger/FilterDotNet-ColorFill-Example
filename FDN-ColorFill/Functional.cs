using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDN_ColorFill
{
    public static class Functional
    {
        public static T2 Then<T, T2>(this T lhs, Func<T, T2> func) => func(lhs);
    }
}
