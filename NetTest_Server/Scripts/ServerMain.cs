using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MortiseFrame.LitIO;

class ClientState {
    public Socket? socket;
}

class ServerMain {

    static Socket? listenfd;
    static Socket? clientfd;
    static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
    static Dictionary<Socket, string> userTokens = new Dictionary<Socket, string>();

    static void Main(string[] args) {

        try {

            // Bind
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback, 8080);
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenfd.Bind(localEndPoint);

            // Listen
            listenfd.Listen(10);

            Console.WriteLine("Server has started on 127.0.0.1:8080.\nWaiting for a connection...");

            while (true) {
                // 检查 listener 是否有新的连接
                if (listenfd.Poll(0, SelectMode.SelectRead)) {
                    AcceptAndBakeListenfd(listenfd);
                }
                // 检查 clients 是否有新的消息
                foreach (var client in clients.Values) {
                    Socket? handler = client.socket;
                    if (handler == null) {
                        continue;
                    }
                    if (handler.Poll(0, SelectMode.SelectRead)) {
                        byte[] data = new byte[1024];
                        int count = handler.Receive(data);
                        int offset = 0;
                        int id = ByteReader.Read<int>(data, ref offset);
                        switch (id) {
                            case 1:
                                OnLoginReq(handler, data);
                                break;
                            case 2:
                                break;
                            case -1:
                                OnClose(handler);
                                break;
                            case 0:
                                break;
                            default:
                                Console.WriteLine("Unknown message id: {0}", id);
                                break;
                        }
                    }
                }

                // 防止 CPU 占用过高
                System.Threading.Thread.Sleep(1);
            }



        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }

    static void AcceptAndBakeListenfd(Socket listenfd) {
        clientfd = listenfd.Accept();
        Console.WriteLine("New Client Connected");
        ClientState state = new ClientState();
        state.socket = clientfd;
        clients.Add(clientfd, state);
        byte[] data = new byte[1024];
        OnConnectReq(clientfd, data);
    }

    static void OnClose(Socket handler) {
        Console.WriteLine("Client Disconnected");
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }

    static void OnConnectReq(Socket handle, byte[] data) {
        SendConnectRes(handle);
    }

    static void SendConnectRes(Socket handler) {
        ConnectResMessage message = new ConnectResMessage();
        message.id = 100;
        byte[] data = message.ToBytes();
        handler.Send(data);
        Console.WriteLine("Send ConnectRes");
    }

    static void OnLoginReq(Socket handler, byte[] data) {
        int offset = 0;
        LoginReqMessage message = new LoginReqMessage();
        message.FromBytes(data, ref offset);
        Console.WriteLine("Received Login: {0}", message.userToken);
        string? userToken = message.userToken;
        if (userToken == null) {
            Console.WriteLine("UserToken is Null");
            return;
        }
        userTokens.Add(handler, userToken);
        SendLoginRes(handler);
    }

    static void SendLoginRes(Socket handler) {
        LoginResMessage message = new LoginResMessage();
        message.id = 2;
        message.status = 1;
        message.userToken = userTokens[handler];
        byte[] data = message.ToBytes();
        handler.Send(data);
    }

}