using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using TFTP_Client.States;


namespace TFTP_Client
{
     
    class Client
    {
        
        private String host;
        private static Int16 DATA_PACKET_SIZE = 512;
        //private Socket sock = null;
        private Int16 dstPort = 69;
        
        private String retrPath;
        private static byte[] DELIMITER = new byte[] { 0x00 }; 
        private FileStream sendingFileStream = null;
        private FileStream receivingFileStream = null;
        private String sendingFilename = null;
        private UdpClient udpClient;
        private IPEndPoint endpoint;
        private String currentLog;
        //virtual ports
        private int localVirtualPort; 
        private IPEndPoint ipEndPoint;
        byte[] sendingPacket;
        int bytesToBeSend;
        private int[] transmissionCount;

        private TFTPClientWindow context;

        public void setRetrPath(String p) {
            this.retrPath = sanitizePath(p.Trim());
        }

        private String sanitizePath(String s) {

            if (!s.EndsWith("\\"))
            { 
                s += "\\"; 
            }
            return s;

        }

        public void setContext(TFTPClientWindow context) {
            this.context = context;
        }

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
            public static readonly short ReadReq = 0x01,
             WriteReq = 0x02,
             Data = 0x03,
             Ack = 0x04,
             Error = 0x05;
        };

        /**
         *  ClientConfiguration
         * 
         *  most constants for configuration of the client are declared here
         */
        static class ClientConfiguration
        {
            public static readonly int READ_WRITE_REQUEST_TIMEOUT = 10000;
            public static readonly int RECEIVE_TIMEOUT = 4000;
        };

        ClientState clientState;

        private static Client _instance;

        public static Client getInstance() {

            //a static access for the states, so they can apply the next state
            if (_instance == null) {

                //for the public static access getInstance we set the variable first
                _instance = new Client();
                _instance.sendingPacket = new byte[2052];
                 
            }
            
            return _instance;
        }

        private Client() {
            
            //The first time we have to generate a state on our own and not by the state mechanism
            //Then we go into the init-Method according to the state
            clientState = new InitState();
 
        }

         private void log(String s){
             currentLog += s;
         }
        
        public void put(String filename, String host, String port)
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
                connectInitial();
                log("WriteRequest: ");
                sendWriteRequest();

                Console.WriteLine("waiting for ack");
                //wait for acknowledgement
                System.Threading.Thread newThread = new System.Threading.Thread(this.receiveOptionAcknowledgment);
                newThread.Start();

            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
        public void get(String filename, String host, String port)
        {
            try
            {
                //always ensure that we are in the init state. 
                //the current state would throw an exception if it is not the initstate (put would not be applicable)
                this.clientState.get();
                this.sendingFilename = filename;
                this.host = host;
                this.dstPort = Int16.Parse(port);

                //change to send state and send a write request
                connectInitial();
                sendReadRequest();

                Console.WriteLine("waiting for ack");
                
                //wait for acknowledgement (in another thread)
                System.Threading.Thread newThread = new System.Threading.Thread(this.receiveOptionAcknowledgment);
                newThread.Start();
                

            } catch (InvalidOperationException e) {
                Console.WriteLine("Error: " + e);
            }
        }

        /**
         * The first two fields of this array should be assigned. this will be the int16 (short). the rest will be ignored.
         */
        private int twoBytesToInt(byte[] buf)
        {
            //buf should contain 2 values little endian
            return (int)(buf[0] * 256 + buf[1]);
        }

        private void receiveOptionAcknowledgment() {

            //prepare client
            udpClient.Client.Close();
            udpClient = new UdpClient(localVirtualPort);
            endpoint = new IPEndPoint(IPAddress.Parse(host), 0);
            udpClient.Client.ReceiveTimeout = ClientConfiguration.READ_WRITE_REQUEST_TIMEOUT;

            byte[] receiveBytes = null; 

            try {
                 receiveBytes = udpClient.Receive(ref endpoint);
            }
            catch (SocketException e)
            {
                Console.WriteLine("error: " + e.Message);

                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    MessageBox.Show("Timeout! Server not answering! Try again!");
                }
                else {
                    MessageBox.Show("error: " + e.Message);    
                }
                resetClient();
                return;
            }
            

            //Console.WriteLine("output " + receiveBytes.Length);
            
            //the operation code should be 6
            if (twoBytesToInt(receiveBytes) == 6)
            {
                //ok 
                //everythings alright

                System.Type state = this.clientState.GetType();

                if (state == typeof(PutState))
                {
                    if (MessageBox.Show("The server acknowledged the Write Request. Would you like to continue uploading?", "Continue", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Console.WriteLine("beginUploadingDataPackets");

                        beginSend();
                        
                        
                    }
                    else {

                        resetClient();
                        
                    }
                    
                }
                else if (state == typeof(GetState))
                {

                    //LIKE in the Lecture
                    if (MessageBox.Show("The server agreed sending you the file. Would you like to continue?", "Continue", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        resetClient();
                        return;
                    } 

                    udpClient.Connect(endpoint);
                    //send acknowledge <BLOCK 0>
                    startPacket();
                    sendOpCode(OpCode.Ack);
                    sendOpCode(0x00); //send the block no. 0
                    commit();

                    //go into receive ;)
                    receive();
                    
                }
                 
            }
            else {


                if (twoBytesToInt(receiveBytes) == 5)
                {
                    //bad case
                    Console.WriteLine("ERROR Code 5 from Server: " + twoBytesToInt(receiveBytes));
                }
                else {
                    //bad case
                    Console.WriteLine("Unknown operation from Server: " + twoBytesToInt(receiveBytes));
                
                }
                resetClient();
            }

        }

        void resetClient()
        {  //reset client
            udpClient.Close();
            udpClient = null;
            this.clientState = new InitState();
        
        }
        void writeToFile(byte[] buf) {
            if (receivingFileStream == null) {
                receivingFileStream = File.OpenWrite(this.retrPath + this.sendingFilename);
            }
            receivingFileStream.Write(buf, 0, buf.Length);
        }
      
        void receive()
        {
            int blockCount = 0;
            byte[] data;
 
            //receive data
            while (true)
            {

                //set state receive
                clientState.receive();

                data = udpClient.Receive(ref endpoint);
                blockCount++;
                byte[] blockNumber = Utils.partByteArray(data, 2, 4);

//                Console.WriteLine("output " + data.Length);
                if (twoBytesToInt(blockNumber) == blockCount && twoBytesToInt(data) == OpCode.Data)
                {
                     
                    //everything alright, we can save the data
                    writeToFile(Utils.partByteArray(data, 4,data.Length));

                    //sent ack
                    clientState.ack();

                    startPacket();
                    sendOpCode(OpCode.Ack);
                    sendOpCode((short)blockCount); //send the block no. 0
                    commit();

                    //check if download finished
                    if (data.Length < DATA_PACKET_SIZE + 4)
                    {
                        Console.WriteLine("Finished transfer");
                        receivingFileStream.Close();
                        receivingFileStream = null;
                        resetClient();
                        break;
                    }
                }
                else
                {
                    resetClient();
                    Console.WriteLine("ERROR RECEIVING DATA");
                    break;
                }

            }
        
        }
         

        private void beginSend(){

 
            udpClient.Connect(endpoint);
            udpClient.Client.ReceiveTimeout = ClientConfiguration.RECEIVE_TIMEOUT;
            int dataGramCount = 1 + ((int)(new FileInfo(sendingFilename).Length)) / DATA_PACKET_SIZE;
            transmissionCount = new int[dataGramCount];
            send(1, dataGramCount);

        }

        
        private int retrSendAck(int i, int timedOutCountInARow) {

            //retr ack for sending packets
            this.clientState.ack();
             
            try
            {
                byte[] datagram = udpClient.Receive(ref endpoint);

                //reset  timedOutCountInARow (because we received data)
                timedOutCountInARow = 0;

                //get the opCode from the datagram
                int opCode = twoBytesToInt(datagram);

                //get the block nr from the datagram
                int blockNr = twoBytesToInt(Utils.partByteArray(datagram, 2, 4));

                //should be the opcode for ack
                if (opCode == OpCode.Ack)
                {

                    if ((blockNr) == i)
                    {
                        //good one no adaptations needed because behaviour is like we expected!
                        return i;
                    }
                    else {

                        //example he sends ack 1
                        //so we have to resend 2
                        //so the filepointer musst behind packet 1

                        sendingFileStream.Seek(DATA_PACKET_SIZE * blockNr, SeekOrigin.Begin);
                        Console.WriteLine("Ack of wrong packet " + blockNr + "! Assuming packet loss! Resending packet of " + (blockNr + 1));
                        return blockNr;
                    }
                }
                else
                {
                    throw new InvalidDataException("invalid data received: received " + opCode + " and " + blockNr + ", but expected: 4 and " + i);
                }

            }
            catch (SocketException e)
            {

                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    //retransmission! //set the filepointer
                    sendingFileStream.Seek((i - 1) * DATA_PACKET_SIZE, SeekOrigin.Begin);

                    timedOutCountInARow++;

                    if (timedOutCountInARow >= 3)
                    {
                        Console.WriteLine("sorry, timed out 3 times in a row! ERROR!");
                        return -1;
                    }

                    //ack the previous packet for the sender! the sender will then retransmit
                    return i-1;

                }
                else
                {
                    Console.WriteLine("error: " + e.Message);
                    return -1;
                }
            }
        }



        private void send(int i, int dataGramCount)
        {
            //avoid retransmission too often (this packet has been transmitted too often!)
            if (transmissionCount[i - 1] >= 3) { 
                //don't do this again! Connection looks broken!
                MessageBox.Show("The Server is unreachable!");
                resetClient();
                return;
            }

            this.clientState.send();
            
            //send packet
            this.startPacket();
            this.sendDataBlock(i);
            this.commit();

            //avoid retransmission too often
            transmissionCount[i - 1] = transmissionCount[i - 1] + 1;
            Console.WriteLine("transmission count for packet " + i + " is: " + transmissionCount[i - 1]);
            

            //get the ack (read the ack number)
            int receivedAck = retrSendAck(i, 0);

            if (receivedAck > 0 && receivedAck < dataGramCount)
            {
                //recursion, send the next packet
                send(receivedAck+1, dataGramCount);
            }
            else if (receivedAck == dataGramCount)
            {
                Console.WriteLine("Packages sent successfully");
                //go back to init state
                this.clientState = new InitState();
                sendingFileStream.Close();
                sendingFileStream = null;
            }
            else if (receivedAck == -1)
            {
                Console.WriteLine("Error! Client reset!"); 
                resetClient();
            }
            else if (receivedAck > dataGramCount)
            {
                Console.WriteLine("Strange error");
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
        private void sendDataBlock(int blockNumber)
        {

            //as we have a standard of about DATA_PACKET_SIZE bytes we send them immediately, we use the same file pointer again
            //lets first read the data into an array of bytes
            if (sendingFileStream == null) {

                sendingFileStream = File.OpenRead(sendingFilename); 
                
                if (!sendingFileStream.CanSeek)
                {
                    MessageBox.Show("sorry, the filestream cannot seek! FATAL ERROR! please contact the developer!");
                    Console.WriteLine("sorry, the filestream cannot seek! FATAL ERROR!");
                }
            
            }
              
            byte[] blockData = new byte[DATA_PACKET_SIZE];
            
            int bytes = sendingFileStream.Read(blockData, 0, DATA_PACKET_SIZE);
              
            //send the operation code for sending a file 
            sendOpCode(OpCode.Data);

            //since the block number is contained in the same format like the operation code (2 bytes) we can use the same method
            sendOpCode((short)blockNumber);

            //send the data
            appendToSendingPacket(blockData, bytes);

        }



        /**
         * This method sends a write request to the server, with the given Filename
         * Argument must include the full path to the file
         */
        private void sendWriteRequest()
        {

            startPacket();

            sendOpCode(OpCode.WriteReq);

            //Send the filename string into the socket
            sendString(Path.GetFileName(sendingFilename), true, true);

            //send octet format information
            sendString("octet", true, true);

            //send size information
            sendString("tsize", true, true);

            //send the actual size
            sendIntAsString(new FileInfo(sendingFilename).Length, true, true);

            //send blksize information key
            sendString("blksize", true, true);

            //send blksize information value
            sendIntAsString(DATA_PACKET_SIZE, true, true);

            //send timeout information 
            sendString("timeout", true, true);

            //send timeout information value (as string!)
            sendString(""+(ClientConfiguration.READ_WRITE_REQUEST_TIMEOUT/1000), true, true);

            //send resume information key
            sendString("x-resume", true, true);

            //send resume information value
            sendString("0", true, true);

            commit(true);
        }

        private void sendIntAsString(Int64 a, bool withDel, bool forLogging)
        {
            sendString(a.ToString(), withDel, forLogging);
        }

        private void sendIntAsString(Int64 a, bool withDel)
        {
            sendIntAsString(a, withDel, false);
        }

        /**
         * This method sends a read request to the server, with the given Filename
         * Argument must include the full path to the file
         */
        private void sendReadRequest()
        {

            startPacket();

            sendOpCode(OpCode.ReadReq);

            //Send the filename string into the socket
            sendString(Path.GetFileName(sendingFilename), true, true);

            //send octet format information
            sendString("octet", true, true);

            //send size information  //key
            sendString("tsize", true, true);

            //send the actual size //value
            sendString("0", true, true);

            //send blksize information key
            sendString("blksize", true, true);

            //send blksize information value
            sendIntAsString(DATA_PACKET_SIZE, true, true);

            //send timeout information 
            sendString("timeout", true, true);

            //send timeout information value (as string!)
            sendString("" + (ClientConfiguration.READ_WRITE_REQUEST_TIMEOUT / 1000), true, true);

            /* commented as not seen in protocol this time :(
            //send resume information key
            sendString("x-resume", true);

            //send resume information value
            sendString("0", true);
            */

            commit(true);
        }

        private void commit(bool log)
        {
            if (log) {

                context.protocolMSGInv(currentLog);

            }
            currentLog = "";
            udpClient.Send(sendingPacket, bytesToBeSend);
            bytesToBeSend = 0;
        }
        private void commit()
        {
            commit(false);
        }

        private void sendString(String str, bool withDelimiter, bool forLogging) {

            byte[] msg = Encoding.ASCII.GetBytes(str);
            
            try
            {
                appendToSendingPacket(msg, msg.Length);

                if (withDelimiter)
                    //send the delimiter which is the byte 0x00
                    appendToSendingPacket(DELIMITER, 1);

                //since we only use this for small
                if (forLogging)
                {
                    currentLog = currentLog + str + (withDelimiter ? "." : "");
                }
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
            appendToSendingPacket(opcode, 2);
            
        }

        private void appendToSendingPacket(byte[] arr, int length)
        {
            
            for (int i = 0; i < length; i++) { 
                sendingPacket[bytesToBeSend+i] = arr[i];
            }

            bytesToBeSend += length;
        }

        private void startPacket()
        {
            bytesToBeSend = 0;
        }
 
        private void connectInitial()
        {
            try
            {

                //set a new endpoint
                ipEndPoint = new IPEndPoint(IPAddress.Parse(host), dstPort);

                //assign a new udp Client with the given endpoint to the sock instance variable
                udpClient = new UdpClient();

                udpClient.Connect(ipEndPoint);

                localVirtualPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                 
            }
            catch (SocketException e)
            {
                Console.WriteLine("FATAL, cannot connect to host with address " + host + " " + e.Message);
            }
        }
    }
}
