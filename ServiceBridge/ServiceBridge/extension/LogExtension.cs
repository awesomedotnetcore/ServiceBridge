using Lib.helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Linq;
using System.Diagnostics;

namespace Lib.extension
{
    [Serializable]
    public class LogTarget
    {
        private string _;
        public LogTarget(string target)
        {
            this._ = target ?? throw new ArgumentNullException(nameof(target));
        }

        public static implicit operator LogTarget(Type t) => new LogTarget($"{t.Assembly.FullName}-{t.FullName}");

        public override string ToString() => this._;
    }

    public static class LogExtension
    {
        #region exception扩展
        /// <summary>
        /// 获取深层的异常信息
        /// </summary>
        public static string GetInnerExceptionAsJson(this Exception e)
        {
            return Com.GetExceptionMsgJson(e);
        }

        /// <summary>
        /// 获取深层的异常信息
        /// </summary>
        public static List<string> GetInnerExceptionAsList(this Exception e)
        {
            return Com.GetExceptionMsgList(e);
        }

        /// <summary>
        /// 输出
        /// </summary>
        public static void DebugInfo(this Exception e)
        {
            Debug.WriteLine(e.GetInnerExceptionAsJson());
        }

        /// <summary>
        /// 输出
        /// </summary>
        public static void DebugInfo(this string msg) =>
            Debug.WriteLine(msg);

        #endregion

        #region 日志扩展
        /// <summary>
        /// 使用log4net添加日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="name"></param>
        public static void AddLog(this Exception e, string name)
        {
            e.GetInnerExceptionAsJson().AddErrorLog(name);
        }
        /// <summary>
        /// 使用log4net添加日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        public static void AddLog(this Exception e, Type t)
        {
            e.GetInnerExceptionAsJson().AddErrorLog(t);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void AddErrorLog(this string log, string name)
        {
            LogHelper.Error(name, log);
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void AddErrorLog(this string log, Type t)
        {
            LogHelper.Error(t, log);
        }

        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void AddInfoLog(this string log, string name)
        {
            LogHelper.Info(name, log);
        }
        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void AddInfoLog(this string log, Type t)
        {
            LogHelper.Info(t, log);
        }

        /// <summary>
        /// 保存警告日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void AddWarnLog(this string log, string name)
        {
            LogHelper.Warn(name, log);
        }
        /// <summary>
        /// 保存警告日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void AddWarnLog(this string log, Type t)
        {
            LogHelper.Warn(t, log);
        }
        #endregion
    }

    public static class CommonLogExtension
    {
        public static readonly string LoggerName = ConfigurationManager.AppSettings["LoggerName"] ?? "WebLogger";

        public static readonly bool LogFullException = (ConfigurationManager.AppSettings["LogFullException"] ?? "true").ToBool();

        private static string FriendlyTime()
        {
            var now = DateTime.Now;
            try
            {
                var week = DateTimeHelper.GetWeek(now);

                return $"【{now.ToDateTimeString()}】-【{week}】";
            }
            catch
            {
                return $"{now}";
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        public static void AddErrorLog(this Exception e, string extra_data = null)
        {
            string ExceptionJson()
            {
                try
                {
                    if (!LogFullException)
                    {
                        return "无法记录整个异常对象，请修改配置文件";
                    }
                    return JsonHelper.ObjectToJson(e);
                }
                catch (Exception err)
                {
                    return $"无法把整个{nameof(Exception)}对象转为json，原因：{err.Message}";
                }
            }

            var json = new
            {
                error_msg = e.GetInnerExceptionAsList(),
                exception_type = $"异常类型：{e.GetType()?.FullName}",
                full_exception = ExceptionJson(),
                req_data = ReqData(),
                extra_data = extra_data,
                friendly_time = FriendlyTime(),
                tips = new string[] { "建议使用json格式化工具：http://json.cn/" }
            }.ToJson();

            json.AddErrorLog(LoggerName);
        }

        /// <summary>
        /// 业务日志
        /// </summary>
        /// <param name="log"></param>
        public static void AddBusinessInfoLog(this string log)
        {
            var json = new
            {
                msg = log,
                req_data = ReqData(),
                friendly_time = FriendlyTime(),
            }.ToJson();

            json.AddInfoLog(LoggerName);
        }

        /// <summary>
        /// 业务日志
        /// </summary>
        /// <param name="log"></param>
        public static void AddBusinessWarnLog(this string log)
        {
            var json = new
            {
                msg = log,
                req_data = ReqData(),
                friendly_time = FriendlyTime(),
            }.ToJson();

            json.AddWarnLog(LoggerName);
        }

        /// <summary>
        /// 请求上下文信息 
        /// </summary>
        /// <returns></returns>
        private static object ReqData() => new { };
    }
}
