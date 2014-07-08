using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TFTP_Client.States
{
    class SendState : ClientState
    {
        public override void init()
        {
            Client.getInstance().setClientState(new InitState());
        }

        public override void get()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void put()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void receive()
        {
            Client.getInstance().setClientState(new ReceiveState());
        }

        public override void send()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void ack()
        {
            Client.getInstance().setClientState(new AckState());
        }
    }
}
