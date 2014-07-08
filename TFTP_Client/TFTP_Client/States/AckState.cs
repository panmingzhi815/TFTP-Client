using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TFTP_Client.States
{
    class AckState : ClientState
    {
        public override void init()
        {
            Console.WriteLine("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void get()
        {
            Console.WriteLine("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void put()
        {
            Console.WriteLine("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void receive()
        {
            Client.getInstance().setClientState(new ReceiveState());
        }

        public override void send()
        {
            Client.getInstance().setClientState(new SendState());
        }

        public override void ack()
        {
            Console.WriteLine("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }
    }
}
