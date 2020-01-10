using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCServer.Entity
{
    /// <summary>
    /// 接收信息的状态
    /// </summary>
    public enum ReceiveMessageState
    {
        /// <summary>
        /// 等待中
        /// </summary>
        Wait=0,
        /// <summary>
        /// 成功
        /// </summary>
        Success=1,
        /// <summary>
        /// 超时
        /// </summary>
        Overtime=2,
        /// <summary>
        /// 异常
        /// </summary>
        Error =3,
        /// <summary>
        /// 其他
        /// </summary>
        Other=4,
    }
}
