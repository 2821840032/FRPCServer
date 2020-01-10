using FRPCServer;
using FRPCServerAOPContainer;
using FRPCServerCommandConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //AOP容器对象
            AOPContainer container = new AOPContainer();
            //socket对象
            RRPCSetupEntrance superMain = new RRPCSetupEntrance((unity) => {
            }, (unity) => {

            }, (unitytoo) => {
                unitytoo.AddForwardingRequestNamespace("ITestServer", (x) => x.FirstOrDefault());
            });

            while ("q" != Console.ReadLine())
            {
                CommandConsole command = new CommandConsole(container);
                command.MonitorCommand();
            }
        }
    }
}
