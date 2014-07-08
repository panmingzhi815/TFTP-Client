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
            Client.getInstance().setClientState(new InitState());
        }

        public override void get()
        {

        }

        public override void put()
        {

        }

        public override void receive()
        {

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
