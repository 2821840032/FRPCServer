using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FRPCServerAOPContainer
{
    /// <summary>
    /// AOP
    /// </summary>
    public class AOPRPCInterceptor: StandardInterceptor
    {
        Func<IInvocation, object> Implement { get; set; }
        /// <summary>
        /// AOP
        /// </summary>
        /// <param name="implement"></param>
        public AOPRPCInterceptor(Func<IInvocation, object> implement) {
            this.Implement = implement;
        }
        /// <summary>
        /// AOP
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PreProceed(IInvocation invocation)
        {
           
        }


        /// <summary>
        /// AOP
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PerformProceed(IInvocation invocation)
        {
            try
            {
                invocation.ReturnValue=Implement.Invoke(invocation);
            }
            catch (Exception ex)
            {
                HandleException(ex);
              
            }
        }
        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PostProceed(IInvocation invocation)
        {

        }

        private void HandleException(Exception ex)
        {
            Console.WriteLine("发送了错误" + ex);
        }
    }
}
