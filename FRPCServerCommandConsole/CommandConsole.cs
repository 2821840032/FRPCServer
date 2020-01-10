using System;
using System.Collections.Generic;
using SuperSocketRRPCServerCommandConsole.CommandOptionsMap;
using System.Linq;
using FRPCServer;
using FRPCServerAOPContainer;
using CommandLine;
using FRPCServerCommandConsole.CommandOptionsMap;

namespace FRPCServerCommandConsole
{
    /// <summary>
    /// 命令控制台
    /// </summary>
    public class CommandConsole
    {
        /// <summary>
        /// 可以被命令执行的远程调用
        /// </summary>
        List<Type> CommandExecutionRPC { get; set; }

        /// <summary>
        /// 程序启动时间
        /// </summary>
        DateTime StartDateTime { get; set; }

        Dictionary<Guid, RRPCServer> sockerServer =>RRPCServer.RRPCServerList;
        AOPContainer aOPContainer;
        /// <summary>
        /// 初始化
        /// </summary>
        public CommandConsole(AOPContainer aOPContaine) {
            this.aOPContainer = aOPContaine;
            StartDateTime = DateTime.Now;
            CommandExecutionRPC = new List<Type>();
        }
        /// <summary>
        /// 启动监听命令
        /// </summary>
        public void MonitorCommand() {
            while (true)
            {
                Monitor();
            }
        }
        int Monitor()
        {
            var args = Console.ReadLine().Split(' ');

            var exitCode = Parser.Default.ParseArguments<SelectOptions, RealizationServerOptions, InfoOptions>(args)
                .MapResult(
                     (InfoOptions o) => o.Run(StartDateTime, CommandExecutionRPC, sockerServer.Values.ToList()),
                     (SelectOptions o) => o.Run(CommandExecutionRPC),
                     (RealizationServerOptions o) => o.Run(CommandExecutionRPC, aOPContainer),
                    error => 1); ;
            return exitCode;
        }

        /// <summary>
        /// 添加一个可以用命令访问的接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ADDCommandExecutionRPC<T>()
        {
            CommandExecutionRPC.Add(typeof(T));

        }

    }
}
