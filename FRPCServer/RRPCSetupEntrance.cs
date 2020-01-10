using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using FRPCServer.CommunicationEntity;
using FRPCServer;
using Unity;
using FRPCServer.ForwardingRequest;
using FRPCServer.Entity;

namespace FRPCServer
{
    /// <summary>
    /// RRPC初始化入口
    /// </summary>
    public class RRPCSetupEntrance
    {
        /// <summary>
        /// 单例的入口类
        /// </summary>
        internal static RRPCSetupEntrance Single { get; private set; }
        /// <summary>
        /// 全局容器对象注入函数
        /// 在此处注入则所有的AppServer都会调用此函数
        /// </summary>
        internal Action<IUnityContainer> GlobalContainerInjection { get; private set; }

        /// <summary>
        /// 全局服务注入函数
        /// 在此处注入则所有的AppServer都会调用此函数
        /// </summary>
        internal Action<UnityInIt<RRPCSession, RequestExecutiveInformation, RequestBaseInfo, BaseProvideServices>> WholeUnitys { get; private set; }

        /// <summary>
        /// 转发请求规则容器 
        /// </summary>
        internal ForwardingRequestUnity ForwardingRequestUnity { get; private set; }

        /// <summary>
        /// RRPC初始化入口
        /// </summary>
        /// <param name="GlobalContainerInjection">BaseProvideServices 容器对象注入 给所有的APPServer</param>
        /// <param name="WholeUnitys">提供给远程调用的服务</param>
        /// <param name="ForwardingRequest">转发服务请求</param>
        public RRPCSetupEntrance(Action<IUnityContainer> GlobalContainerInjection=null, Action<UnityInIt<RRPCSession, RequestExecutiveInformation, RequestBaseInfo, BaseProvideServices>> WholeUnitys = null, Action<ForwardingRequestUnity> ForwardingRequest =null)
        {
            if (Single!=null)
            {
                throw new Exception("已经初始化过一次RRPC");
            }
            Single = this;
            this.GlobalContainerInjection = GlobalContainerInjection;
            this.WholeUnitys = WholeUnitys;

            ForwardingRequestUnity = new ForwardingRequestUnity();
            ForwardingRequest?.Invoke(this.ForwardingRequestUnity);

            SetupAPPServers();
        }

        /// <summary>
        /// 启动监听服务
        /// </summary>
        private void SetupAPPServers() {
            var bootstrap = BootstrapFactory.CreateBootstrap();
            if (!bootstrap.Initialize())
            {
                Console.WriteLine("初始化失败");
                Console.ReadKey();
                return;
            }

            var result = bootstrap.Start();

            Console.WriteLine("Start result: {0}!", result);

            if (result == StartResult.Failed)
            {
                Console.WriteLine("程序没有被启动");
                Console.ReadKey();
                return;
            }
        }
    }
}
