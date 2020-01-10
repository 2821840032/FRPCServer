using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPCServer.CommunicationEntity
{
    /// <summary>
    /// 最底层传输对象
    /// </summary>
    public class RequestBaseInfo : IRequestInfo
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 流对象
        /// </summary>
        public byte[] body { get; set; }

        /// <summary>
        /// 最底层传输对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="body"></param>
        public RequestBaseInfo(string key, byte[] body) {
            this.Key = key;
            this.body = body;
        }
        /// <summary>
        /// Utf-8格转换
        /// </summary>
        public string bodyMeg => Encoding.UTF8.GetString(body); 
    }
}
