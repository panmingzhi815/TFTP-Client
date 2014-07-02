using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TFTP_Client.States
{
    class InitState : ClientState
    {
        public override void init()
        {
            Console.WriteLine("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void get()
        {
            GetState state = new GetState();
            Client.getInstance().get();
        }

        public override void put()
        {

        }

        public override void receive()
        {

        }

        public override void send()
        {

        }

        public override void ack()
        {

        }
    }
}
