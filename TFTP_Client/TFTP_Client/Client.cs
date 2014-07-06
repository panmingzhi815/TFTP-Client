using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using TFTP_Client.States;


namespace TFTP_Client
{


    class Client
    {
        private String host = "newsreader88.eweka.nl";
        private int port = 119;
        private Socket sock = null;
        private static byte[] DELIMITER = new byte[] { 0x00 };


        /**
         *   OPERATION CODES RFC1350
         * 
         *   1     Read request (RRQ)
         *   2     Write request (WRQ)
         *   3     Data (DATA)
         *   4     Acknowledgment (ACK)
         *   5     Error (ERROR)
         * 
         */
        static class OpCode {
            public static short ReadReq = 0x01,
             WriteReq = 0x02,
             Data = 0x03,
             Ack = 0x04,
             Error = 0x05;
        };
         

        ClientState clientState;

        private static Client _instance;

        public static Client getInstance() {

            //a static access for the states, so they can apply the next state
            if (_instance == null)
                Console.WriteLine("FATAL @ Client.getInstance: _instance is null");

            return _instance;
        }

        public Client() {
            //for the public static access getInstance we set the variable first
            _instance = this;

            //The first time we have to generate a state on our own and not by the state mechanism
            //Then we go into the init-Method according to the state
            clientState = new InitState();
            init();

        }

        //STATE HANDLING
        //this handles the init state
        public void init()
        {
            
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

        /* maybe not needed
        public ClientState getClientState(){
            return this.clientState;
        }
        */

        private void sendWriteRequest(String filename)
        {
            sendOpCode(OpCode.WriteReq);

            //Send the filename string into the socket
            sendString(Path.GetFileName(filename), true); 

            //send octet format information
            sendString("octet", true);

            //send size information
            sendString("tsize", true); 

            //send the actual size
            FileInfo fi = new FileInfo(filename);
            String size = ((Int64)fi.Length).ToString();
            sendString(size, true);

            //send blksize information key
            sendString("blksize", true);

            //send blksize information value
            sendString("2048", true);

            //send timeout information 
            sendString("timeout", true);

            //send timeout information value (as string!)
            sendString("30", true);

            //send resume information key
            sendString("x-resume", true);

            //send resume information value
            sendString("0", true);   
        }

        private void sendString(String str, bool withDelimiter) {

            byte[] msg = Encoding.ASCII.GetBytes(str);
            
            try
            {
                sock.Send(msg, msg.Length, SocketFlags.None);

                if (withDelimiter)
                    //send the delimiter which is the byte 0x00
                    sock.Send(DELIMITER, 1, SocketFlags.None);
            }
            catch (SocketException e) {
                Console.Write("Exception in writing socket string: " + e.Message);
            }
            
        
        }

        private byte[] shortToBytes(short number){
            byte[] b = new byte[2];
            
            b[0] = (byte)(number >> 8);
            b[1] = (byte)(number & 255);

            return b;
        }

        private void sendOpCode(short operationCode)
        {
            //the short consists of about 2 bytes, little endian
            
            //convert the short to a byte array
            byte[] opcode = shortToBytes(operationCode);

            //put into the socket
            sock.Send(opcode);
        }

        private void connect()
        {

            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress[] ipAddresses = hostEntry.AddressList;

            // Console.WriteLine(host + " is mapped to the IP-Address(es): ");

            foreach (IPAddress ipAddress in ipAddresses)
            {
                // Console.Write(ipAddress.ToString());
            }

            IPEndPoint ipEo = new IPEndPoint(ipAddresses[0], port);


            sock = new Socket(ipEo.AddressFamily,
                              SocketType.Stream,
                              ProtocolType.Udp);


            sock.Connect(ipEo);


            if (sock.Connected)
            {
                sock.Receive(new byte[256], 256, SocketFlags.None);
                try  {
                    byte[] msg = Encoding.ASCII.GetBytes();

                    byte[] bytes = new byte[512];

                    int i = sock.Send(msg, msg.Length, SocketFlags.None);
                    int byteCount = sock.Receive(bytes, 512, SocketFlags.None);
                    
                }  catch (System.Net.Sockets.SocketException e) {
                    Console.WriteLine("Error: "+ e.Message);
                }
            }
        }
                
    }
}
