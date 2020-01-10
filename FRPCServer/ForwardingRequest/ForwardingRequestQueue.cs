using FRPCServer.Entity;
using FRPCServer.ForwardingRequest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using Newtonsoft.Json;
using FRPCServer.CommunicationEntity;
using System.Linq;

namespace FRPCServer.ForwardingRequest
{
    /// <summary>
    /// 请求转发任务管理
    /// </summary>
    public class ForwardingRequestQueue
    {

        private System.Timers.Timer ScheduledCleaningThread;

        /// <summary>
        /// 执行事件中的方法
        /// </summary>
        public ConcurrentDictionary<Guid, ForwardingRequestEntity> MethodCallQueues { get; private set; }

        /// <summary>
        /// 开启队列管理
        /// </summary>
        public ForwardingRequestQueue()
        {
            MethodCallQueues = new ConcurrentDictionary<Guid, ForwardingRequestEntity>();

            Thread thread = new Thread(TimerInit);
            thread.Start();

        }

        private void TimerInit()
        {

            ScheduledCleaningThread = new System.Timers.Timer();
            ScheduledCleaningThread.Enabled = true;
            ScheduledCleaningThread.Interval = 60000; //执行间隔时间,单位为毫秒; 这里实际间隔为1分钟  
            ScheduledCleaningThread.Elapsed += new System.Timers.ElapsedEventHandler(ScheduledCleaningFunc);
            ScheduledCleaningThread.Start();
        }

        /// <summary>
        /// 添加一个任务到队列并进行发送操作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="info"></param>
        /// <param name="requestClient"></param>
        /// <param name="giveClient"></param>
        /// <returns></returns>
        public ForwardingRequestEntity AddTaskQueue(Guid id, RequestExecutiveInformation info, RRPCSession requestClient, RRPCSession giveClient)
        {
            var result = new ForwardingRequestEntity(id, info, ReceiveMessageState.Wait, DateTime.Now, requestClient, giveClient);
            MethodCallQueues.TryAdd(id, result);
            info.RequestClientSession = requestClient.SessionID;
            var msg = JsonConvert.SerializeObject(info);
            giveClient.SendMessage(msg);

            return result;

        }

        /// <summary>
        /// 定时清理函数
        /// </summary>
        private void ScheduledCleaningFunc(object source, ElapsedEventArgs e)
        {
            foreach (var item in MethodCallQueues.Where(d => DateTime.Now > d.Value.ExpirationTime.AddHours(60)).ToList())
            {
                MethodCallQueues.TryRemove(item.Key, out var value);
            }
        }


        /// <summary>
        /// 根据任务ID获取任务信息并修改状态为以完成
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="rpcResule">内容</param>
        /// <returns>true 找到并修改信息 false未找到</returns>
        public bool GetTaskIDAndSuccess(Guid id, string rpcResule)
        {
            if (MethodCallQueues.TryGetValue(id, out var value))
            {
                value.ProcessingFuncInvoke(ReceiveMessageState.Success, rpcResule);
                var msg = JsonConvert.SerializeObject(value.TaskInfo);
                value.RequestClient.SendMessage(msg);
                return true;
            }
            else
            {
                //没有找到这个任务的信息
                return false;
            }
        }
        /// <summary>
        /// 清空所有的任务
        /// </summary>
        /// <param name="message">说明</param>
        public void ErrorEmpty(string message)
        {
            foreach (var item in MethodCallQueues)
            {
                item.Value.ProcessingFuncInvoke(ReceiveMessageState.Error, message);
            }
        }
    }
}
