using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using System.Linq;
using System.Reflection;
using FRPCServer.CommunicationEntity;
using Newtonsoft.Json;

namespace FRPCServer.ForwardingRequest
{
    /// <summary>
    /// 用于转发请求的存储容器
    /// </summary>
   public class ForwardingRequestUnity
    {
        /// <summary>*
        /// 存储的FullName 转发标识列表
        /// </summary>
        public Dictionary<string, ForwardingRequestEnity> ForwardingRequestunity { get; set; }

        /// <summary>
        /// 初始化转发容器
        /// </summary>
        public ForwardingRequestUnity() {
            ForwardingRequestunity = new Dictionary<string, ForwardingRequestEnity>();
        }
        /// <summary>
        /// 添加转发服务规则-某个指定服务
        /// </summary>
        /// <typeparam name="IT">待转发请求的接口</typeparam>
        /// <param name="SelectRRPCServer">选择转发的服务器 不能为空</param>
        /// <param name="SelectRRPCSession">选择转发session 若为空则从列表中随机选取一个</param>
        public void AddForwardingRequestServer<IT>(Func<List<RRPCServer>, RRPCServer> SelectRRPCServer, Func<IEnumerable<RRPCSession>, RRPCSession> SelectRRPCSession=null)
        {
            if (SelectRRPCServer==null)
            {
                throw new Exception("SelectRRPCServer 项不能为空");
            }
            ForwardingRequestunity.Add(typeof(IT).FullName, new ForwardingRequestEnity() { ID = typeof(IT).FullName, SelectRRPCServer = SelectRRPCServer, SelectRRPCSession = SelectRRPCSession });
        }
        /// <summary>
        /// 添加转发服务规则-某个命名空间
        /// </summary>
        /// <param name="RRPCInterfaceName">远程接口 名称</param>
        /// <param name="SelectRRPCServer">选择转发的服务器 不能为空</param>
        /// <param name="SelectRRPCSession">选择转发session 若为空则从列表中随机选取一个</param>
        public void AddForwardingRequestNamespace(string RRPCInterfaceName, Func<List<RRPCServer>, RRPCServer> SelectRRPCServer, Func<IEnumerable<RRPCSession>, RRPCSession> SelectRRPCSession = null)
        {

            if (SelectRRPCServer == null)
            {
                throw new Exception("SelectRRPCServer 项不能为空");
            }

            var assembly = Assembly.Load(RRPCInterfaceName);
            if (assembly == null)
            {
                throw new Exception("没有找到此项目"+RRPCInterfaceName);
            }
            ForwardingRequestunity.Add(assembly.FullName, new ForwardingRequestEnity() { ID = assembly.FullName, SelectRRPCServer = SelectRRPCServer, SelectRRPCSession = SelectRRPCSession });
        }

        /// <summary>
        /// 查询转发的请求
        /// </summary>
        /// <param name="info">请求信息</param>
        /// <param name="session">查询到的session</param>
        /// <param name="RRPCServers">服务列表</param>
        /// <param name="ExcludeSessID">排除的ID</param>
        /// <returns></returns>
        public bool GetService(RequestExecutiveInformation info,List<RRPCServer> RRPCServers,string ExcludeSessID, out RRPCSession session) {
            ForwardingRequestEnity value;
            if (ForwardingRequestunity.TryGetValue(info.FullName, out value)|| ForwardingRequestunity.TryGetValue(info.AssemblyFullName, out value))
            {
                var  rrpcServer = value.SelectRRPCServer(RRPCServers);

                if (rrpcServer == null)
                {
                    RRPCServer.RRPCServerList.FirstOrDefault().Value.Log($"转发请求失败{info.FullName} 无法找到指定的RRPCServer 没有找到转发该请求的服务配置", LoggerType.Error);
                    session = null;
                    return false; ;
                }
                var count = rrpcServer.GetAllSessions().Count();
                if (count==0)
                {
                    rrpcServer.Log($"转发请求失败{info.FullName} 由于{rrpcServer.Config.Name} 连接客户端为0", LoggerType.Error);
                    session = null;
                    return false;
                }
                return SelectSession(info, rrpcServer, rrpcServer.GetAllSessions(), value, 0, ExcludeSessID, out session);
            }
            else {
                session = null;
                return false;
            }
        }
        /// <summary>
        /// 选择Session
        /// </summary>
        /// <param name="sessions">Session列表</param>
        /// <param name="info">请求信息</param>
        /// <param name="server">选中的Server服务</param>
        /// <param name="session">匹配到的Session</param>
        /// <param name="value">保存的配置</param>
        /// <param name="ExcludeSessID">排除的ID</param>
        /// <param name="state">0:优先SelectRRPCSession 1：执行指定sessionID选择 2：随机选择 其他：记录日志没有找到合适的session</param>
        private bool SelectSession(RequestExecutiveInformation info, RRPCServer server, IEnumerable<RRPCSession> sessions, ForwardingRequestEnity value,int state, string ExcludeSessID, out RRPCSession session) {
            switch (state)
            {
                case 0:
                    session = value.SelectRRPCSession?.Invoke(sessions);
                    if (session == null)
                    {
                       return SelectSession(info, server, sessions, value, state + 1,ExcludeSessID, out session);
                    }
                    return true;
                case 1:
                    session = sessions.FirstOrDefault(d => d.SessionID.Equals(info.RRPCSessionID?.ToString()));
                    if (session == null)
                    {
                        return SelectSession(info, server, sessions, value, state + 1, ExcludeSessID, out session);
                    }
                    return true;
                case 2:
                    session = sessions.Where(d=>!d.SessionID.Equals(ExcludeSessID)).OrderBy(d=>Guid.NewGuid()).FirstOrDefault();
                    if (session == null)
                    {
                        return SelectSession(info, server, sessions, value, state + 1, ExcludeSessID, out session);
                    }
                    return true;
                default:
                    server.Log("没有找到匹配的Session 本次请求作废" + JsonConvert.SerializeObject(info));
                    session = null;
                    return false;
            }
        }
    }
    /// <summary>
    /// 存储转发请求的标识实体
    /// </summary>
    public class ForwardingRequestEnity {
        /// <summary>
        /// 存储接口标识
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 选择RRPCServer函数
        /// </summary>
        public Func<List<RRPCServer>, RRPCServer> SelectRRPCServer { get; set; }

        /// <summary>
        /// 选择RRPCSession函数
        /// </summary>
        public Func<IEnumerable<RRPCSession>, RRPCSession> SelectRRPCSession { get; set; }
    }
}
