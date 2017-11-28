using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Lib.extension
{
    public static class CommonExtension
    {
        /// <summary>
        /// 获取过期的方法
        /// </summary>
        public static List<string> FindObsoleteFunctions(this Assembly ass)
        {
            var list = new List<string>();
            foreach (var tp in ass.GetTypes())
            {
                foreach (var func in tp.GetMethods())
                {
                    if (func.GetCustomAttributes<ObsoleteAttribute>().Any())
                    {
                        list.Add($"{tp.FullName}.{func.Name},{ass.FullName}");
                    }
                }
            }
            return list.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CloneObject<T>(this T obj) where T : ICloneable
        {
            return (T)((ICloneable)obj).Clone();
        }

        /// <summary>
        /// 从list中随机取出一个item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Choice<T>(this Random ran, IList<T> list)
        {
            return ran.ChoiceIndexAndItem(list).item;
        }

        /// <summary>
        /// 随机抽取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static (int index, T item) ChoiceIndexAndItem<T>(this Random ran, IList<T> list)
        {
            //The maxValue for the upper-bound in the Next() method is exclusive—
            //the range includes minValue, maxValue-1, and all numbers in between.
            var index = ran.RealNext(minValue: 0, maxValue: list.Count - 1);
            return (index, list[index]);
        }

        /// <summary>
        /// 带边界的随机范围
        /// </summary>
        /// <param name="ran"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int RealNext(this Random ran, int maxValue)
        {
            //The maxValue for the upper-bound in the Next() method is exclusive—
            //the range includes minValue, maxValue-1, and all numbers in between.
            return ran.RealNext(minValue: 0, maxValue: maxValue);
        }

        /// <summary>
        /// 带边界的随机范围
        /// </summary>
        /// <param name="ran"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int RealNext(this Random ran, int minValue, int maxValue)
        {
            //The maxValue for the upper-bound in the Next() method is exclusive—
            //the range includes minValue, maxValue-1, and all numbers in between.
            return ran.Next(minValue: minValue, maxValue: maxValue + 1);
        }

        /// <summary>
        /// 随机抽取一个后从list中移除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T PopChoice<T>(this Random ran, ref List<T> list)
        {
            var data = ran.ChoiceIndexAndItem(list);
            list.RemoveAt(data.index);
            return data.item;
        }

        /// <summary>
        /// 从list中随机抽取count个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> Sample<T>(this Random ran, IList<T> list, int count)
        {
            return new int[count].Select(x => ran.Choice(list)).ToList();
        }

        /// <summary>
        /// 随机选取索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<int> SampleIndexs<T>(this Random ran, IList<T> list, int count)
        {
            return ran.Sample(Com.Range(list.Count).ToList(), count);
        }

        /// <summary>
        /// 打乱list的顺序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        public static void Shuffle<T>(this Random ran, ref List<T> list)
        {
            var data = new List<T>();
            while (list.Count > 0)
            {
                data.Add(ran.PopChoice(ref list));
            }
            list.AddRange(data);
        }

        /// <summary>
        /// 根据权重选择
        /// </summary>
        public static T ChoiceByWeight<T>(this Random ran, IEnumerable<T> source, Func<T, int> selector)
        {
            if (source == null || source.Count() <= 0) { throw new ArgumentException(nameof(source)); }
            if (source.Count() == 1) { return source.First(); }

            if (source.Any(x => selector.Invoke(x) < 1)) { throw new ArgumentException("权重不能小于1"); }

            var total_weight = source.Sum(x => selector.Invoke(x));

            var weight = ran.RealNext(total_weight - 1);
            
            var cur = 0;

            foreach (var s in source)
            {
                //单个权重
                var w = selector.Invoke(s);

                var start = cur;
                var end = start + w;

                if (weight >= start && weight <= end)
                {
                    return s;
                }

                cur += end;
            }

            throw new Exception("权重取值异常");
        }
    }
}
