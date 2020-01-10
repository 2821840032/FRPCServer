using FRPCServer.CommunicationEntity;
using FRPCServer.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCServer.ForwardingRequest
{
    /// <summary>
    /// 请求转发任务存储的信息
    /// </summary>
    public class ForwardingRequestEntity
    {

        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public ReceiveMessageState State { get; set; }

        /// <summary>
        /// 清理时间
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 远程任务处理结果
        /// </summary>
        public string ReturnValue { get; set; }

        /// <summary>
        /// 任务信息
        /// </summary>
        public RequestExecutiveInformation TaskInfo { get; set; }

        /// <summary>
        /// 请求对象
        /// </summary>
        public RRPCSession RequestClient { get; set; }

        /// <summary>
        /// 转发对象
        /// </summary>
        public RRPCSession GiveClient { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 任务存储
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="taskInfo">任务信息</param>
        /// <param name="state">状态</param>
        /// <param name="expirationTime">过期时间</param>
        /// <param name="requestClient">请求对象</param>
        /// <param name="giveClient">转发对象</param>
        public ForwardingRequestEntity(Guid id, RequestExecutiveInformation taskInfo, ReceiveMessageState state, DateTime expirationTime, RRPCSession requestClient, RRPCSession giveClient)
        {
            this.ID = id;
            this.TaskInfo = taskInfo;
            this.State = state;
            this.ExpirationTime = expirationTime;
            this.RequestClient = requestClient;
            this.GiveClient = giveClient;

        }
        /// <summary>
        /// 修改结果
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="rpcResult">结果</param>
        public void ProcessingFuncInvoke(ReceiveMessageState state, string rpcResult)
        {
            TaskInfo.ReturnValue = rpcResult;
            State = state;
            ReturnValue = rpcResult;

        }
    }
}
