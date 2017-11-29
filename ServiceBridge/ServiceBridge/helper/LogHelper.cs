using Common.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace ServiceBridge.helper
{
    /// <summary>
    /// 记录日志
    /// </summary>
    public static class LogHelper
    {
        //获取当前方法
        //var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
        /// <summary>
        /// 不自动加载配置，防止和其他组件冲突
        /// </summary>
        static LogHelper()
        {
            //Config();
        }

        public static string Log4ConfigPath = null;
        private static bool configed = false;
        private static void Config()
        {
            if (configed) { return; }
            if (ValidateHelper.IsPlumpString(Log4ConfigPath) && File.Exists(Log4ConfigPath))
            {
                //log4net.Config.XmlConfigurator.Configure(new FileInfo(Log4ConfigPath));
            }
            else
            {
                //log4net.Config.XmlConfigurator.Configure();
            }
            configed = true;
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public static void Error(Type t, string msg, Exception err = null)
        {
            try
            {
                Config();

                var logger = LogManager.GetLogger(t);
                if (err == null)
                {
                    logger.Error(msg);
                }
                else
                {
                    logger.Error(msg, err);
                }
            }
            catch (Exception e)
            {
                Debug.Write(Com.GetExceptionMsgJson(e));
            }
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public static void Error(string name, string msg, Exception err = null)
        {
            try
            {
                Config();

                var logger = LogManager.GetLogger(name);
                if (err == null)
                {
                    logger.Error(msg);
                }
                else
                {
                    logger.Error(msg, err);
                }
            }
            catch (Exception e)
            {
                Debug.Write(Com.GetExceptionMsgJson(e));
            }
        }

        /// <summary>
        /// 记录日常信息
        /// </summary>
        public static void Info(Type t, string msg)
        {
            try
            {
                Config();

                var logger = LogManager.GetLogger(t);
                logger.Info(msg);
            }
            catch (Exception e)
            {
                Debug.Write(Com.GetExceptionMsgJson(e));
            }
        }

        /// <summary>
        /// 记录日常信息
        /// </summary>
        public static void Info(string name, string msg)
        {
            try
            {
                Config();

                var logger = LogManager.GetLogger(name);
                logger.Info(msg);
            }
            catch (Exception e)
            {
                Debug.Write(Com.GetExceptionMsgJson(e));
            }
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        public static void Warn(Type t, string msg)
        {
            try
            {
                Config();

                var logger = LogManager.GetLogger(t);
                logger.Warn(msg);
            }
            catch (Exception e)
            {
                Debug.Write(Com.GetExceptionMsgJson(e));
            }
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        public static void Warn(string name, string msg)
        {
            try
            {
                Config();

                var logger = LogManager.GetLogger(name);
                logger.Warn(msg);
            }
            catch (Exception e)
            {
                Debug.Write(Com.GetExceptionMsgJson(e));
            }
        }
    }
}
