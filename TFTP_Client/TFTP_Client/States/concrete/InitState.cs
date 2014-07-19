using System;
using System.Collections.Generic; 
using System.Text; 
using System.Reflection;

namespace TFTP_Client.States
{
    class InitState : ClientState
    {
        public override void init()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void get()
        {
            Client.getInstance().setClientState(new GetState());
        }

        public override void put()
        {
            Client.getInstance().setClientState(new PutState());
        }

        public override void receive()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void send()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }

        public override void ack()
        {
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
        }
    }
}
