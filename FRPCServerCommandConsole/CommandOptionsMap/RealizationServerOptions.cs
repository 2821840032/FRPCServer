using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using FRPCServerAOPContainer;
using FRPCServer;

namespace FRPCServerCommandConsole.CommandOptionsMap
{
    [Verb("IMP", HelpText = "执行远程访问")]
    public class RealizationServerOptions
    {

        [Option('i', "socketSessionId", Required = true, HelpText = "Session的ID")]
        public string socketSessionId { get; set; }

        [Option('s', "socketServerId", Required = true, HelpText = "Server的ID")]
        public string socketServerId { get; set; }

        [Option('f', "fullname", Required = true, HelpText = "执行的FullName")]
        public string fullname { get; set; }

        [Option('n', "name", Required = true, HelpText = "函数名称")]
        public string name { get; set; }

        [Option('p', "para", Required = false, HelpText = "参数")]
        public IEnumerable<string> para { get; set; }

        public int Run(List<Type> commandExecutionRPClist, AOPContainer aOPContainer)
        {
            try
            {


                MethodInfo objMethod;
                var objType = commandExecutionRPClist.FirstOrDefault(d => d.FullName.Equals(fullname));
                if (objType == null)
                {
                    Console.WriteLine($"没有找到{fullname}的类型");
                    return 1;
                }


                objMethod = objType.GetMethods().FirstOrDefault(d => d.Name.Equals(name));
                if (objMethod == null)
                {
                    Console.WriteLine($"没有找到{name}函数");
                    return 1;
                }

                var paras = objMethod.GetParameters();
                if (para.Count() != paras.Count())
                {
                    Console.WriteLine($"参数数量不对 应为{paras.Count()}");
                    return 1;
                }
                List<object> paraList = new List<object>();

                var paraToList = para.ToList();
                try
                {
                    for (int i = 0; i < paraToList.Count(); i++)
                    {
                        paraList.Add(JsonConvert.DeserializeObject(paraToList[i], paras[i].ParameterType));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("格式化参数失败" + e.Message);
                }
                if (RRPCServer.RRPCServerList.TryGetValue(Guid.Parse(socketServerId), out var value))
                {
                    var client = value.GetAllSessions().FirstOrDefault(d => d.SessionID == socketSessionId);
                    if (client != null)
                    {
                        var obj = aOPContainer.GetServices(client, objType);

                        var result = objMethod.Invoke(obj, paraList.ToArray());

                        Console.WriteLine("调用完成:" + JsonConvert.SerializeObject(result));
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("没有找到Session ID" + socketSessionId);
                        return 1;
                    }

                }
                else
                {
                    Console.WriteLine("没有找到Server ID" + socketServerId);
                    return 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("执行IMP出现错误"+e.Message);
                return 1;
            }
        }
    }
}
