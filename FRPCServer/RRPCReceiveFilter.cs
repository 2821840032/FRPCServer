using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;
using FRPCServer.CommunicationEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPCServer
{
    /// <summary>
    /// 基础请求转换器
    /// </summary>
    public class RRPCReceiveFilter : FixedHeaderReceiveFilter<RequestBaseInfo>
    {
        /// <summary>
        /// 基础请求转换器
        /// </summary>
        public RRPCReceiveFilter()
       : base(4)
        {

        }
         /// <summary>
         /// 获取Body长度
         /// </summary>
         /// <param name="header"></param>
         /// <param name="offset"></param>
         /// <param name="length"></param>
         /// <returns></returns>
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            var headerData = new byte[4];
            Array.Copy(header, offset, headerData, 0, 4);
            return BitConverter.ToInt32(headerData, 0);
        }

        /// <summary>
        /// 转换成最小包装
        /// </summary>
        /// <param name="header"></param>
        /// <param name="bodyBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override RequestBaseInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            return new RequestBaseInfo("MonitorReceived", bodyBuffer.CloneRange(offset, length));
        }
    }
}
