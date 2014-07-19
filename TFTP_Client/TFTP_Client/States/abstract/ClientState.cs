using System;
using System.Collections.Generic; 
using System.Text; 

namespace TFTP_Client.States
{
    abstract class ClientState
    {

        abstract public void init();

        abstract public void get();

        abstract public void put();

        abstract public void receive();

        abstract public void send();
    
        abstract public void ack();
                 
    }
}
