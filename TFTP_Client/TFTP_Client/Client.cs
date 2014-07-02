using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFTP_Client.States;

namespace TFTP_Client
{
    class Client
    {

        ClientState clientState;

        private static Client _instance;

        public static Client getInstance() {

            if (_instance == null)
                Console.WriteLine("FATAL @ Client.getInstance: _instance is null");

            return _instance;
        }

        public Client() {
            _instance = this; 
            init();
        }

        public void init()
        {
            clientState = new InitState();
        }

        public void get()
        {

        }

        public void put()
        {

        }

        public void receive()
        {

        }

        public void send()
        {

        }

        public void ack()
        {

        }

        //Setter and getter for ClientState
        public void setClientState(ClientState state){
            this.clientState = state;
        }

        public ClientState getClientState(){
            return this.clientState;
        }
        
            

    }
}
