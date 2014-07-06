using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TFTP_Client.States
{
    class ReceiveState : ClientState
    {
        public override void init()
        {

        }

        public override void get()
        {

        }

        public override void optAck()
        {

        }

        public override void receive()
        {
            Console.WriteLine("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void send()
        {

        }

        public override void ack()
        {

        }
    }
}
