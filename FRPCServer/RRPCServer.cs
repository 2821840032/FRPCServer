using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using FRPCServer.CommunicationEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRPCServer;
using System.Reflection;
using FRPCServer.Entity;
using Unity;

namespace FRPCServer
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public class RRPCServer : AppServer<RRPCSession, RequestBaseInfo>
    {
        /// <summary>
        /// 容器对象
        /// </summary>
       public UnityInIt<RRPCSession, RequestExecutiveInformation, RequestBaseInfo,BaseProvideServices> Container { get; private set; }

        /// <summary>
        /// unity 容器对象 一般用来存储如 数据库连接对象 工具之类的单例或者工厂
        /// 在RPCsetup中全局唯一 且能在服务中获取到它
        /// </summary>
        public IUnityContainer UnityContainer { get; set; }

        /// <summary>
        /// 所有的ServerList服务
        /// </summary> 
        public static Dictionary<Guid, RRPCServer> RRPCServerList { get; private set; } = new Dictionary<Guid, RRPCServer>();

        /// <summary>
        /// 通过配置文件安装服务从这里启动
        /// </summary>
        public RRPCServer()
            : base(new DefaultReceiveFilterFactory<RRPCReceiveFilter, RequestBaseInfo>())
        {

            Type baseProvideServicesType = typeof(BaseProvideServices);

            Container = new UnityInIt<RRPCSession, RequestExecutiveInformation, RequestBaseInfo, BaseProvideServices>(baseProvideServicesType.GetProperty("Socket"), baseProvideServicesType.GetProperty("Info"), baseProvideServicesType.GetProperty("RequestInfo"), baseProvideServicesType.GetProperty("Container"),baseProvideServicesType.GetProperty("RequestClientSession"));


            UnityContainer = new UnityContainer();


            RRPCServerList.Add(Guid.NewGuid(),this);

            RRPCSetupEntrance.Single.GlobalContainerInjection?.Invoke(UnityContainer);

            RRPCSetupEntrance.Single.WholeUnitys?.Invoke(Container);

        }

        /// <summary>
        /// 启动
        /// </summary>
        protected override void OnStarted()
        {
            this.Log(string.Format("Socket启动成功：{0}", string.Join("|", Config.Listeners.Select(d => d.Ip + ":" + d.Port))));
            //启动成功
        }
    }
}
