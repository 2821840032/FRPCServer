using SuperSocket.SocketBase;
using FRPCServer.CommunicationEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FRPCServer.ForwardingRequest;

namespace FRPCServer
{
    /// <summary>
    /// Session
    /// </summary>
    public class RRPCSession : AppSession<RRPCSession, RequestBaseInfo>
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        public RRPCServer RrpcAppServer => (RRPCServer)AppServer;

        /// <summary>
        /// 远程任务队列
        /// </summary>
        public RemoteCallQueue RemoteCallQueue { get; private set; }

        /// <summary>
        /// 请求转发任务队列
        /// </summary>
        public ForwardingRequestQueue ForwardingRequestQueue { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public Dictionary<string, Object> Identifications { get; private set; }


        /// <summary>
        /// session
        /// </summary>
        public RRPCSession()
        {
            ForwardingRequestQueue = new ForwardingRequestQueue();
            RemoteCallQueue = new RemoteCallQueue();
            Identifications = new Dictionary<string, object>();
        }
        /// <summary>
        /// session连接加入
        /// </summary>
        protected override void OnSessionStarted()
        {
            this.Log("新的连接：" + SessionID);

        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            
        }
        #region 异常

        /// <summary>
        /// 未知消息
        /// </summary>
        /// <param name="requestInfo"></param>
        protected override void HandleUnknownRequest(RequestBaseInfo requestInfo)
        {
            Logger.Error("收到未知消息：" + requestInfo.bodyMeg);
        }
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="e"></param>
        protected override void HandleException(Exception e)
        {

            Logger.Error("发送错误：" + e.Message);
        }
        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="reason"></param>
        protected override void OnSessionClosed(CloseReason reason)
        {
            this.Log($"远程连接断开 IP-Prot:{RemoteEndPoint.ToString()}、ID:{SessionID}");
            //释放所有的任务
            RemoteCallQueue.ErrorEmpty("远程对象被关闭 原因:"+reason);

            base.OnSessionClosed(reason);
        }

        #endregion

        /// <summary>
        /// 添加一个标识到session
        /// </summary>
        /// <typeparam name="T">标识类型</typeparam>
        /// <param name="Identification">标识对象</param>
        /// <param name="name">自定义的key 如果需要存放同种标识类型 你就会需要它</param>
        public void AddIdentification<T>(T Identification,string name="") where T:class {
            Identifications.Add(typeof(T).FullName+name, Identification);
        }

        /// <summary>
        /// 获取一个标识
        /// </summary>
        /// <typeparam name="T">标识类型</typeparam>
        /// <param name="identification">标识对象</param>
        /// <param name="name">自定义key</param>
        /// <returns></returns>
        public bool GetIdentification<T>(out T identification, string name = "") where T : class
        {
            if (Identifications.TryGetValue(typeof(T).FullName + name, out var value))
            {
                identification = (T)value;
                return true;
            }
            else
            {
                identification = null;
                return false;
            }
           
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            var dataBody = Encoding.UTF8.GetBytes(message);

            var dataLen = BitConverter.GetBytes(dataBody.Length);//int类型占4位，根据协议这里也只能4位，否则会出错

            var sendData = new byte[4 + dataBody.Length];//长度为4

            // +-------+-------------------------------+
            // |request|                               |
            // | name  |    request body               |
            // |  (4)  |                               |
            // |       |                               |
            // +-------+-------------------------------+

            Array.ConstrainedCopy(dataLen, 0, sendData, 0, 4);
            Array.ConstrainedCopy(dataBody, 0, sendData, 4, dataBody.Length);

            base.Send(sendData, 0, sendData.Length);
        }
    }
}
