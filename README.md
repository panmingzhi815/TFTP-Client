TFTP-Client
===========

TFTP Client written in C# (Visual Studio 2013)

The program strictly follows the TFTP PROTOCOL RFC 1350 (REVISION 2) that you can read here: http://tools.ietf.org/html/rfc1350

The whole program is written in C# in Visual Studio 2013. After setting up the values IP-Address, Port and Filename in the GUI, the values are being passed as String values to the Client which is then initiating a new connection after parsing the values. The Client has a Singleton interface. This is good for 2 reasons: The client can only be instantiated once. So there is no parallel uploading or downloading. For our purposes it's good. The design can be changed if needed for different user interfaces. The other reason is, that the states have an easy access to a static reference and can set the next state to go from one state into another. This design was chosen to keep protocol analysis and code design simple.


#Sending a request

Every data transfer is initiated with either a write request or a read request. After sending the request, the client awaits an Option Acknowledgement from the server.


#File transfer

After the Option Acknowledgement has been received correctly, the program blocks the current operation with a "MessageBox" and awaits the user interaction if he wants to continue the file transfer. When the user confirms with "YES", the client starts a new Thread (the data transfer thread). So the GUI (Graphical User Interface) which works on another thread, does not freeze when the socket blocks the current thread.

In the network thread the socket is first setup with the right TID's (transfer identifiers). Therefore the client connected to port 69. Now the client got an answer on his virtual port, but from another Port. The new port is accessable via the endpoint that has been passed to the receive-Method of the client. 

From MSDN:
>The ref keyword causes an argument to be passed by reference, not by value.

    udpClient.Receive(ref endpoint)
   
This is important for connecting to the new endpoint with the right TID.

The transfer is done within a loop where the states are changed from one state (send/receive) into another (ack).

#Error and timeout handling

##Wrong packet number

If another packet number is retrieved than expected (maybe because of packet loss). The client acts as if the last packet has not been sent, yet. The client then responds correctly. The client only resets, when the timeout occurs 3 times in a row or an unexpected operation code occurs.


##Operation Code handling

Everytime the client receives an error code instead of the expected operation code, the client resets completely and can be reused.


##Timeout handling

Whenever the Socket timeout exception occurs, the client retries the last command. When it occurs 3 times in a row, the client resets completely. This is done by:

	catch (SocketException e)
	{
		if (e.SocketErrorCode == SocketError.TimedOut)
		{
			Console.WriteLine("Timeout Exception");
			timedoutCountInARow++;

			if (timedoutCountInARow >= 3) {
				resetClient();
				return;
			}

			sendReceivedAck(blockCount);
		}
		else 
		{
			Console.WriteLine("ERROR RECEIVING DATA "+e.Message);
			return;
		}
	}


#Special Cases

##Datagram Count
When the client sends a file that has a file length that is a multiple of the packet size, the client has to send an empty packet. This is done by calculating the count of datagrams which have to be sent:

    int dataGramCount = 1 + ((int)(new FileInfo(filename).Length)) / DATA_PACKET_SIZE; 

If DATA_PACKET_SIZE is 512 and the file size is 512, the datagram count would be 2. That is correct. If DATA_PACKET_SIZE is 512 and the file size is 511, there would only be 1 datagram. The calculation works.


##States

The states are always updated within the methods, that have been called. The state checks if the client is allowed to change to the desired state. If yes, nothing happens. Otherwise an Exception (InvalidOperationException) would be thrown and the programmer can decide what happens next (e.g. to reset the client) inside the catch clause.


#User Interaction

The user is getting informed of any file transfer. He may confirm to continue a requested transfer. He can see the read and write requests on the graphical user interface. He can set the desired file via the option dialog. If a file cannot be accessed, the user gets informed.