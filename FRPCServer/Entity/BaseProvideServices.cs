using FRPCServer.CommunicationEntity;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
namespace FRPCServer.Entity
{
    /// <summary>
    /// 提供服务的基础类 如果你需要用到其中的信息的话
    /// </summary>
   public class BaseProvideServices
    {
        /// <summary>
        /// 操作连接对象
        /// </summary>
        public RRPCSession Socket { get;internal set; }

        /// <summary>
        /// 本次任务的信息
        /// </summary>
        public RequestExecutiveInformation Info { get; internal set; }

        /// <summary>
        /// 最基础的请求信息
        /// </summary>
        public RequestBaseInfo RequestInfo { get; internal set; }

        /// <summary>
        /// 容器对象
        /// </summary>
        public IUnityContainer Container { get; internal set; }

    }
}
