using System;
using System.Collections.Generic; 
using System.Text; 
using System.Reflection;

namespace TFTP_Client.States
{
    class AckState : ClientState
    {
        public override void init()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
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
            Client.getInstance().setClientState(new SendState());
        }

        public override void ack()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }
    }
}
