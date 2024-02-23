using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServer {

    class ServerMain {

        static void Main(string[] args) {

            // Establish the local endpoint for the socket.
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback, 8080);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Server has started on 127.0.0.1:8080.\nWaiting for a connection...");
                Socket handler = listener.Accept();
                Console.WriteLine("A client connected.");

                // Send data to the client
                string data = "Hello from the server!";
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                handler.Send(byteData);
                Console.WriteLine("Sent: {0}", data);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}