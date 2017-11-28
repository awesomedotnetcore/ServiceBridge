using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Lib.extension
{
    public static class EnumExtension
    {
        public static Dictionary<string, T> GetItems<T>(this Enum data)
        {
            //Enum.GetValues();
            //Enum.GetNames();

            var fields = data.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
            var keys = fields.Select(x => x.GetValue(data)).ToList();

            return fields.ToDictionary(x => x.Name, x => (T)x.GetValue(data));
        }

        public static Dictionary<string, object> GetItems(this Enum data)
        {
            var fields = data.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
            var keys = fields.Select(x => x.GetValue(data)).ToList();

            return fields.ToDictionary(x => x.Name, x => x.GetValue(data));
        }
    }
}
