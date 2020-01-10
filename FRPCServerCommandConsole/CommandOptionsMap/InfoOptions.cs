using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;
using System.Data;
using FRPCServer;
using FRPCServer.Entity;

namespace FRPCServerCommandConsole.CommandOptionsMap
{
    [Verb("info", HelpText = "信息")]
   public class InfoOptions
    {
        [Option('a', "allSession", Required = false, HelpText = "展开全部session")]
        public bool isSelectSession { get; set; }

        public int Run(DateTime dateTime,List<Type> commandExecutionRPC, List<RRPCServer> socketServers)
        {
            Console.WriteLine("开始时间："+dateTime);
            Console.WriteLine($"共{socketServers.Count}个服务正在运行");
            foreach (var item in socketServers)
            {
                Console.WriteLine("==================");
                Console.WriteLine($"监听地址{item.Config.Ip}:{item.Config.Port}");

                if (isSelectSession)
                {
                    var sesions = item.GetAllSessions();
                    Console.WriteLine($"当前APPServer共有{sesions.Count()}个连接");
                    foreach (var session in sesions)
                    {
                        Console.WriteLine($"ID:{session.SessionID} StartTime:{session.StartTime} RemoteEndPoint:{session.RemoteEndPoint.ToString()} TaskCount:{session.RemoteCallQueue.MethodCallQueues.Count} ErrorTask{session.RemoteCallQueue.MethodCallQueues.Values.Where(d => d.State == ReceiveMessageState.Error).Count()}");
                    }
                }
                Console.WriteLine("==================");
            }
            Console.WriteLine("命令可调用对象数量："+commandExecutionRPC.Count);
            Console.WriteLine("运行异常数量(总)："+ LoggerAssembly.LoggerList.Where(d=>d.LoggerType== LoggerType.Error).Count());
            Console.WriteLine("Sever运行异常数量"+ LoggerAssembly.LoggerList.Where(d=>d.Triggers== Triggers.Server).Count());
            Console.WriteLine("Session运行异常数量" + LoggerAssembly.LoggerList.Where(d => d.Triggers == Triggers.Session).Count());
            return 0;
        }
    }
}
