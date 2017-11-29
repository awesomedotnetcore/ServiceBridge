using ServiceBridge.core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.helper
{
    /// <summary>
    /// 映射，推荐使用更专业的组件，比如AutoMapper
    /// </summary>
    public static class MapperHelper
    {
        /// <summary>
        /// 获取map(测试可用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static T GetMappedEntity<T>(object model)
        {
            var entity = (T)Activator.CreateInstance(typeof(T));

            MapEntity<T>(ref entity, model);

            return entity;
        }

        /// <summary>
        /// 获取map(测试可用)
        /// </summary>
        public static void MapEntity<T>(ref T entity, object model, string[] notmap = null)
        {
            if (model == null) { throw new Exception("对象为空"); }

            //读取
            var modelproperties = ConvertHelper.NotNullList(model.GetType().GetProperties());
            modelproperties = modelproperties.Where(x => x.CanRead).ToList();

            //写入
            var entityproperties = ConvertHelper.NotNullList(entity.GetType().GetProperties());
            entityproperties = entityproperties.Where(x => x.CanWrite).ToList();
            if (ValidateHelper.IsPlumpList(notmap))
            {
                entityproperties = entityproperties.Where(x => !notmap.Contains(x.Name)).ToList();
            }

            foreach (var pi in entityproperties)
            {
                //属性名和属性类型一样
                var modelpi = modelproperties
                    .Where(x => x.Name == pi.Name)
                    .Where(x => x.GetType() == pi.GetType())
                    .FirstOrDefault();

                if (modelpi == null) { continue; }

                pi.SetValue(entity, modelpi.GetValue(model), null);
            }
        }

        /// <summary>
        /// 读取reader当前行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T GetModelFromReader<T>(IDataReader reader) where T : class, new()
        {
            if (reader == null || reader.IsClosed) { throw new Exception("reader为空"); }

            var model = new T();

            var props = model.GetType().GetProperties().Where(x => x.CanWrite);
            var cols = new List<string>();
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                cols.Add(reader.GetName(i));
            }
            foreach (var property in props)
            {
                if (!cols.Contains(property.Name)) { continue; }

                //这里需要注意value值的类型必须和属性类型一致，否则会抛出TargetException异常
                //property.SetValue(model, dr.GetValue(i), null);//为model赋值  
                var val = reader[property.Name];
                property.SetValue(model, (val == DBNull.Value) ? null : val);//类型转换。 
            }
            return model;
        }

        /// <summary>
        /// 从reader中读取list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<T> GetListFromReader<T>(IDataReader reader) where T : class, new()
        {
            var list = new List<T>();
            while (reader != null && reader.Read())
            {
                list.Add(GetModelFromReader<T>(reader));
            }
            return list;
        }

    }
}
