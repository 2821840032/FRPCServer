using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCServer
{
    /// <summary>
    /// 日志组件
    /// </summary>
    public static class LoggerAssembly
    {
        /// <summary>
        /// 日志列表
        /// </summary>
        public static List<LoggerInfo> LoggerList { get; private set; } = new List<LoggerInfo>();

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="socket">连接 服务器</param>
        /// <param name="loggerType">日志类型</param>
        /// <param name="message">信息</param>
        public static void Log(this RRPCServer socket, string message, LoggerType loggerType = LoggerType.Info)
        {
            message = "RRPPCServer "+ message;
            Console.WriteLine(DateTime.Now + ":" + loggerType + ":" + message);
            var loginfo = new LoggerInfo() { LoggerType = loggerType, Message = message, Triggers = Triggers.Server };
            LoggerList.Add(loginfo);
            LogFile(socket.Logger, loginfo);
        }
        /// <summary>
        /// 打印日志session
        /// </summary>
        /// <param name="socket">连接session 服务器</param>
        /// <param name="loggerType">日志类型</param>
        /// <param name="message">信息</param>
        public static void Log(this RRPCSession socket, string message, LoggerType loggerType = LoggerType.Info)
        {
            message = "RRPPCSession ID:" + socket.SessionID + message;
            Console.WriteLine(DateTime.Now + ":" + loggerType + ":" + message);
            var loginfo = new LoggerInfo() { LoggerType = loggerType, Message = message, Triggers= Triggers.Session };
            LoggerList.Add(loginfo);

            LogFile(socket.Logger, loginfo);
        }

        /// <summary>
        /// 写入到日志文件中
        /// </summary>
        private static void LogFile(ILog log,LoggerInfo loggerInfo) {
            switch (loggerInfo.LoggerType)
            {
                case LoggerType.Error:
                    log.Error(loggerInfo.Message);
                    break;
                case LoggerType.Warning:
                    log.Warn(loggerInfo.Message);
                    break;
                case LoggerType.Info:
                    log.Info(loggerInfo.Message);
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LoggerType
    {
        /// <summary>
        /// 异常
        /// </summary>
        Error = 0,
        /// <summary>
        /// 警告
        /// </summary>
        Warning = 1,
        /// <summary>
        /// 信息
        /// </summary>
        Info = 2
    }
    /// <summary>
    /// 触发者
    /// </summary>
    public enum Triggers
    {
        /// <summary>
        /// 服务
        /// </summary>
        Server = 0,
        /// <summary>
        /// 用户
        /// </summary>
        Session = 1,
    }
    /// <summary>
    /// 日志信息
    /// </summary>
    public class LoggerInfo
    {

        /// <summary>
        /// 日志类型
        /// </summary>
        public LoggerType LoggerType { get; set; }

        /// <summary>
        /// 触发者
        /// </summary>
        public Triggers Triggers { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
