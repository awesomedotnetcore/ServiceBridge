using ServiceBridge.helper;
using ServiceBridge.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.extension
{
    public static class DictExtension
    {
        /// <summary>
        /// 字典变url格式(a=1&b=3)
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToUrlParam(this IDictionary<string, string> dict)
        {
            return Com.DictToUrlParams(dict.ToDictionary(x => x.Key, x => x.Value));
        }

        /// <summary>
        /// 把一个字典加入另一个字典，重复就覆盖
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<K, V> AddDict<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> data)
        {
            foreach (var kv in data)
            {
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dict, K key, V deft = default(V))
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return deft;
        }

        /// <summary>
        /// 返回元祖
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEnumerable<(K key, V value)> AsTupleEnumerable<K, V>(this IDictionary<K, V> dict)
        {
            foreach (var key in dict.Keys)
            {
                yield return (key, dict[key]);
            }
        }

        static void fasd()
        {
            var dict = new Dictionary<string, string>();

            foreach (var kv in dict.AsTupleEnumerable())
            {
                //
            }
        }
    }
}
