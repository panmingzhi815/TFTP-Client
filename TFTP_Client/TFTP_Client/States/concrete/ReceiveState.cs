﻿using System;
using System.Collections.Generic; 
using System.Text; 
using System.Reflection;

namespace TFTP_Client.States
{
    class ReceiveState : ClientState
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
            throw new InvalidOperationException("Applying State " + MethodBase.GetCurrentMethod().Name + " from this State " + this.GetType().Name + " not allowed!");
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
