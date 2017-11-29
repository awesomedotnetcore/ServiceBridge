using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace ServiceBridge.helper
{
    /// <summary>
    /// 正则表达式公共类
    /// </summary>
    public class RegexHelper
    {
        /// <summary>
        /// 检查是否匹配
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool IsMatch(string str, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.IsMatch(str, pattern, options);
        }
        /// <summary>
        /// 获取匹配对象
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Match GetMatch(string str, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.Match(str, pattern, options);
        }
        /// <summary>
        /// 从字符串中找到匹配的字符
        /// </summary>
        /// <param name="str">原始字符</param>
        /// <param name="pattern">正则</param>
        /// <param name="spliter">分隔符</param>
        /// <returns></returns>
        public static List<Match> FindMatchs(string str, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            var list = new List<Match>();

            var matchs = Regex.Matches(str, pattern, options);
            foreach (Match matcher in matchs)
            {
                if (matcher == null || !matcher.Success) { continue; }
                list.Add(matcher);
            }

            return list;
        }

        /// <summary>
        /// 用正则替换
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string Replace(string str, string replacement, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.Replace(str, pattern, replacement, options);
        }

    }
}