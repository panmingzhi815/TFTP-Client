﻿using System;
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

        private String host;
        //private Socket sock = null;
        private Int16 dstPort = 69;
        private static byte[] DELIMITER = new byte[] { 0x00 };
        private Boolean connected = false;
        private FileStream sendingFileStream = null;
        private String sendingFilename = null;
        private UdpClient udpClient;

        public void setSendingFilename(String f) {
            this.sendingFilename = f;
        }

        /**
         *   OPERATION CODES RFC1350
         * 
         *   1     Read request (RRQ)
         *   2     Write request (WRQ)
         *   3     Data (DATA)
         *   4     Acknowledgment (ACK)
         *   5     Error (ERROR)
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
            if (_instance == null) {

                //for the public static access getInstance we set the variable first
                _instance = new Client();
                Console.WriteLine("FATAL @ Client.getInstance: _instance is null");
            }
            

            return _instance;
        }

        private Client() {
            
            //The first time we have to generate a state on our own and not by the state mechanism
            //Then we go into the init-Method according to the state
            clientState = new InitState();
 
        }
         
 
        private bool isReady() {
            return this.clientState.GetType() == typeof(InitState);
        }

        public void upload(String filename, String host, String port)
        {
            try
            {
                //always ensure that we are in the init state. 
                //the current state would throw an exception if it is not the initstate (put would not be applicable)
                this.clientState.put(); 
                this.sendingFilename = filename;
                this.host = host;
                this.dstPort = Int16.Parse(port);

                //change to send state and send a write request
                this.clientState.send();
                connect();
                sendWriteRequest();

                //wait for acknowledgement
                this.clientState.ack();
                receiveOptAck();
                
            }
            catch (InvalidOperationException e) {
                Console.WriteLine("Error: " + e);
            }
        }

        private short toShort(byte[] buf)
        {
            //buf should contain 2 values little endian
            return (short)(buf[0] * 255 + buf[1]);
        }

        private void receiveOptAck() { 

            //the operation code should be 6
            byte[] buff = new byte[2];
            sock.Receive(buff, 2, SocketFlags.None);

            if (toShort(buff) == 6)
            {
                //good case
                byte[] wholebuff = new byte[2048];
                byte[] lastCapture = new byte[2048];
                bool loopEntered = false;

                int count;

                while (true) {

                    count = sock.Receive(lastCapture, 2048, SocketFlags.None);

                    lastCapture = Utils.partByteArray(lastCapture, 0, count);

                    if (!loopEntered)
                    {
                        loopEntered = true;
                        wholebuff = lastCapture;
                    }
                    else {
                        wholebuff = Utils.concatByteArrays(wholebuff, lastCapture);
                    }

                    //now when all is concatenated we test if there's a timeout option given from the server, just test the string timeout
                    if (Encoding.ASCII.GetString(wholebuff).Contains("timeout") &&
                        wholebuff[wholebuff.Length - 1] == (byte)0x00) { 

                        //we can break the loop
                        break;
                    }

                }


                //everythings alright
                Console.WriteLine("Alright! We can start uploading! But ask user!");


                //change the state to sending an then send the data block #1
                clientState.send();
                sendDataBlock(1);



            }
            else {

                //bad case
                Console.WriteLine("Unknown operation from Server: " + toShort(buff));
            
            }

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

        /**
         * This sends a data block from the file
         * 
         * From http://tools.ietf.org/html/rfc1350 Sending a
         * DATA packet is an acknowledgment for the first ACK packet of the
         * previous DATA packet. The WRQ and DATA packets are acknowledged by
         * ACK or ERROR packets, while RRQ
         */
        private void sendDataBlock(int blockNumber) {

            //as we have a standard of about 2048 bytes we send them immediately, we use the same file pointer again
            //lets first read the data into an array of bytes
            if (sendingFileStream == null)
                sendingFileStream = File.OpenRead(sendingFilename);

            byte[] blockData = new byte[2048];
            sendingFileStream.Read(blockData, 2048 * (blockNumber - 1), 2048);

            //send the operation code for sending a file 
            sendOpCode(OpCode.Data);

            //since the block number is contained in the same format like the operation code (2 bytes) we can use the same method
            sendOpCode((short)blockNumber);
            
            //send the data
            sock.Send(blockData);

            clientState.ack();
        }


        /**
         * This method sends a write request to the server, with the given Filename
         * Argument must include the full path to the file
         */
        private void sendWriteRequest()
        {
            sendOpCode(OpCode.WriteReq);

            //Send the filename string into the socket
            sendString(Path.GetFileName(sendingFilename), true); 

            //send octet format information
            sendString("octet", true);

            //send size information
            sendString("tsize", true); 

            //send the actual size
            FileInfo fi = new FileInfo(sendingFilename);
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
            try
            {
                //set a new endpoint
                IPEndPoint ipEo = new IPEndPoint(IPAddress.Parse(host), dstPort);

                //assign a new socket with the given endpoint to the sock instance variable
                sock = new Socket(ipEo.AddressFamily,
                                  SocketType.Stream,
                                  ProtocolType.Udp);

                udpClient = new UdpClient(ipEo);
                

            }
            catch (SocketException e) {
                Console.WriteLine("FATAL, cannot connect to host with address " + host + " " + e.Message);
            }
        }


        private void receiveData() {

            int byteCount;

            try
            {
                while (true)
                {

                    byte[] bbb = new byte[2048];
                    byteCount = sock.Receive(bbb, 2048, SocketFlags.None);
                    
                }
            }

            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine(e.ErrorCode+":  "+e.Message);
            }
        
        }
                
    }
}
