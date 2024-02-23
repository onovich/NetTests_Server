using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServer {

    class ServerMain {

        static void Main(string[] args) {

            var server = new TcpListener(IPAddress.Loopback, 8080);
            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:8080.{0}Waiting for a connection...", Environment.NewLine);

            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("A client connected.");

            NetworkStream stream = client.GetStream();

            // Send a message to the client    
            string message = "Hello from the server!";
            byte[] data = Encoding.ASCII.GetBytes(message);

            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent: {0}", message);

            // Shutdown and end connection
            client.Close();
            server.Stop();

        }

    }

}
