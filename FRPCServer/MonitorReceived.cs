using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using FRPCServer.CommunicationEntity;
using System.Threading;

namespace FRPCServer
{
    /// <summary>
    /// 所有请求的入口
    /// </summary>
    public class MonitorReceived : CommandBase<RRPCSession, RequestBaseInfo>
    {
        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="session">连接对象</param>
        /// <param name="requestInfo">请求信息</param>
        public  override void ExecuteCommand(RRPCSession session, RequestBaseInfo requestInfo)
        {
            RequestExecutiveInformation info = null;
            try
            {
                info = JsonConvert.DeserializeObject<RequestExecutiveInformation>(requestInfo.bodyMeg);
            }
            catch (Exception e)
            {
                session.Log($"解析失败 ex:{e} Body:{requestInfo.bodyMeg }EndAddress:{session.RemoteEndPoint.ToString()}", LoggerType.Error);
                return;
            }
            if (info.ReturnValue != null && session.RemoteCallQueue.GetTaskIDAndSuccess(info.ID, info.ReturnValue))
            {
                //处理完成
                return;
            }
            if (info.ReturnValue != null && session.ForwardingRequestQueue.GetTaskIDAndSuccess(info.ID, info.ReturnValue))
            {
                //处理转发请求完成
                return;
            }
            else if (info.ReturnValue != null)
            {

                session.Log($"收到一个意外的请求 它有结果但是没有找到该任务的信息 ID:{info.ID} FullName:{info.FullName} Return:{info.ReturnValue} 来自于:{session.RemoteEndPoint.ToString()}", LoggerType.Error);
            }
            else
            {
                ImplementFunc(info, session, requestInfo);
            }
        }

        /// <summary>
        /// 执行RPC的调用
        /// </summary>
        /// <param name="info">请求信息</param>
        /// <param name="session">连接对象</param>
        /// <param name="requestInfo">请求基础类</param>
      async  void ImplementFunc(RequestExecutiveInformation info, RRPCSession session, RequestBaseInfo requestInfo)
        {
            await Task.Yield();
            //首先查询是否为此提供服务
            if (session.RrpcAppServer.Container.GetService(info.FullName, session, info, requestInfo, ((RRPCServer)session.AppServer).UnityContainer,null, out object executionObj, out var iServerType))
            {
                var methodType = iServerType.GetMethod(info.MethodName);
                List<object> paraList = new List<object>();
                var paras = methodType.GetParameters();
                for (int i = 0; i < info.Arguments.Count; i++)
                {
                    paraList.Add(JsonConvert.DeserializeObject(info.Arguments[i], paras[i].ParameterType));
                }
                info.ReturnValue = JsonConvert.SerializeObject(methodType.Invoke(executionObj, paraList.ToArray()));
                var msg = JsonConvert.SerializeObject(info);
                session.SendMessage(msg);
            }
            else
            {
                if (RRPCSetupEntrance.Single.ForwardingRequestUnity.GetService(info, RRPCServer.RRPCServerList.Select(d=>d.Value).ToList(),session.SessionID, out var sessionLo))
               {
                   
                    sessionLo.ForwardingRequestQueue.AddTaskQueue(info.ID, info, session, sessionLo);
                    return;
                }
                else {
                    session.Log("收到一个未知的请求" + requestInfo.bodyMeg, LoggerType.Error);
                }
            }
        }
    }
}
