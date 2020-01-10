using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading;
using FRPCServer.Entity;
using FRPCServer.CommunicationEntity;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FRPCServer
{
    /// <summary>
    /// 任务队列管理
    /// </summary>
    public class RemoteCallQueue
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        public int second { get; private set; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; private set; }


        private System.Timers.Timer HealthExaminationThread;

        private System.Timers.Timer ScheduledCleaningThread;

        /// <summary>
        /// 执行事件中的方法
        /// </summary>
        public ConcurrentDictionary<Guid,RemoteCallEntrity> MethodCallQueues { get;private set; }

        /// <summary>
        /// 开启队列管理
        /// </summary>
        /// <param name="second">超时时间</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        public RemoteCallQueue(int second=10,int maxRetryCount=3)
        {
            this.second = second;
            MethodCallQueues = new ConcurrentDictionary<Guid, RemoteCallEntrity>();

            MaxRetryCount = maxRetryCount;
            Thread thread = new Thread(TimerInit);
            thread.Start();

        }

        private void TimerInit() {
            
            HealthExaminationThread = new System.Timers.Timer();
            HealthExaminationThread.Enabled = true;
            HealthExaminationThread.Interval = 1000* 5; //执行间隔时间,单位为毫秒; 这里实际间隔为  second
            HealthExaminationThread.Elapsed += new System.Timers.ElapsedEventHandler(healthExaminationFunc);
            HealthExaminationThread.Start();

            ScheduledCleaningThread = new System.Timers.Timer();
            ScheduledCleaningThread.Enabled = true;
            ScheduledCleaningThread.Interval = 60000; //执行间隔时间,单位为毫秒; 这里实际间隔为1分钟  
            ScheduledCleaningThread.Elapsed += new System.Timers.ElapsedEventHandler(ScheduledCleaningFunc);
            ScheduledCleaningThread.Start();
        }

        /// <summary>
        /// 添加一个任务到队列
        /// </summary>
        public RemoteCallEntrity AddTaskQueue(Guid id, RequestExecutiveInformation info, RRPCSession socket) {
            var result = new RemoteCallEntrity(id, info, ReceiveMessageState.Wait, DateTime.Now.AddSeconds(second), socket);
            MethodCallQueues.TryAdd(id,result);
            return result;
            
        }

        /// <summary>
        /// 监控检查函数
        /// </summary>
        private void healthExaminationFunc(object source, ElapsedEventArgs e)
        {
            foreach (var item in MethodCallQueues.Where(d => DateTime.Now>d.Value.ExpirationTime&& d.Value.State== ReceiveMessageState.Wait).ToList())
            {
                if (item.Value.RetryCount < MaxRetryCount)
                {
                    item.Value.RetryCount++;
                    item.Value.ExpirationTime = DateTime.Now.AddSeconds(second);
                    //重发
                    RemoteExecutionFuncAsync(item.Value);
                }
                else
                {
                    item.Value.ProcessingFuncInvoke(ReceiveMessageState.Overtime, $"Timeout to {second} Second");
                }
            
            }
        }
        /// <summary>
        /// 定时清理函数
        /// </summary>
        private void ScheduledCleaningFunc(object source, ElapsedEventArgs e)
        {
            foreach (var item in MethodCallQueues.Where(d => DateTime.Now> d.Value.ExpirationTime.AddHours(60)).ToList())
            {
                MethodCallQueues.TryRemove(item.Key, out var value);
            }
        }


        /// <summary>
        /// 进行远程调用
        /// </summary>
        /// <param name="info">通讯的信息</param>
        public async Task RemoteExecutionFuncAsync(RemoteCallEntrity info)
        {
            await Task.Yield();
            RemoteExecutionFunc(info);
        }

        /// <summary>
        /// 进行远程调用
        /// </summary>
        /// <param name="info">通讯的信息</param>
        public void RemoteExecutionFunc(RemoteCallEntrity info)
        {
            var msg = JsonConvert.SerializeObject(info.TaskInfo);
            try
            {
                info.ClientSocket.SendMessage(msg);
            }
            catch (Exception e)
            {
                info.ClientSocket.Log("通讯出现异常" + e.Message, LoggerType.Error);
                info.ProcessingFuncInvoke(ReceiveMessageState.Error, e.Message);
            }
        }

        /// <summary>
        /// 根据任务ID获取任务信息并修改状态为以完成
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="rpcResule">内容</param>
        /// <returns>true 找到并修改信息 false未找到</returns>
        public bool GetTaskIDAndSuccess(Guid id,string rpcResule) {
            if (MethodCallQueues.TryGetValue(id, out var value))
            {
                if (value.State == ReceiveMessageState.Wait)
                {
                    value.ProcessingFuncInvoke(ReceiveMessageState.Success,rpcResule);
                    return true;
                }
                else {
                    value.ClientSocket.Log($"任务状态已经被更改过一次 现在它又收到了一个结果 ID:{value.ID} Result:{rpcResule} State:{value.State}", LoggerType.Warning);
                    return true;
                }
             
            }
            else {
                //没有找到这个任务的信息
                return false;
            }
        }
        /// <summary>
        /// 清空所有的任务
        /// </summary>
        /// <param name="message">说明</param>
        public void ErrorEmpty(string message) {
            foreach (var item in MethodCallQueues)
            {
                item.Value.ProcessingFuncInvoke(ReceiveMessageState.Error, message);
            }
        }
    }
}
