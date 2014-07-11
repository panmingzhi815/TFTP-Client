TFTP-Client
===========

TFTP Client written in C# (Visual Studio 2013)

The whole program is written in C# in Visual Studio 2013. After setting up the values IP-Address, Port and Filename in the GUI, the values are being passed as String values to the Client which is then initiating a new connection after parsing the values. The Client has a Singleton interface. This is good for 2 reasons. The client can only be instantiated once. So there is no parallel uploading or downloading. For our purposes it's good. The design can be changed if needed for different user interfaces. The other reason is, that the states have an easy access to a static reference and can set the next state to go from one state into another. This design was chosen to keep protocol analysis and code design simple.

#Sending a request
Every data transfer is inited with either a write request or a read request. After sending the request, the client awaits an Option Acknowledgment from the server.

#Sending a file


#Receiving a file