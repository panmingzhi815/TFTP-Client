using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFTP_Client.States
{
    abstract class ClientState
    {

        abstract public void init();

        abstract public void get();

        abstract public void optAck();

        abstract public void receive();

        abstract public void send();
    
        abstract public void ack();
                 
    }
}
