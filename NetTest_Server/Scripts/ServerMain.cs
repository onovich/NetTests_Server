using System.Net;
using System.Net.Sockets;
using MortiseFrame.LitIO;

class ClientState {
    public Socket socket;
}

class ServerMain {

    static Socket listenfd;
    static Socket clientfd;
    static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
    static Dictionary<Socket, string> userTokens = new Dictionary<Socket, string>();
    static Queue<IMessage> messageQueue = new Queue<IMessage>();
    static List<Socket> checkReadList = new List<Socket>();
    static byte[] readBuff = new byte[1024];

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
                TickOn();
                TickSend();
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

    static void OnCloseReq(Socket handler) {
        Console.WriteLine("Client Disconnected");
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }
    static void OnConnectReq(Socket handle, byte[] data) {
        SendConnectRes(handle);
    }

    static void SendConnectRes(Socket handler) {
        ConnectResMessage message = new ConnectResMessage();
        messageQueue.Enqueue(message);
    }

    static void OnLoginReq(Socket handler, byte[] data, ref int offset) {
        LoginReqMessage message = new LoginReqMessage();
        message.FromBytes(data, ref offset);
        Console.WriteLine("Received Login: {0}", message.userToken);
        string userToken = message.userToken;
        if (userToken == null) {
            Console.WriteLine("UserToken is Null");
            return;
        }
        userTokens.Add(handler, userToken);
        SendLoginRes(handler);
    }

    static async void TickOn() {
        checkReadList.Clear();
        checkReadList.Add(listenfd);
        foreach (var client in clients.Values) {
            checkReadList.Add(client.socket);
        }
        Socket.Select(checkReadList, null, null, 1000);

        foreach (Socket s in checkReadList) {
            if (s == listenfd) {
                AcceptAndBakeListenfd(listenfd);
            } else {
                int count = await s.ReceiveAsync(readBuff);
                var offset = 0;
                int msgCount = ByteReader.Read<int>(readBuff, ref offset);
                for (int i = 0; i < msgCount; i++) {
                    int len = ByteReader.Read<int>(readBuff, ref offset);
                    if (len < 5) {
                        break;
                    }
                    byte id = ByteReader.Read<byte>(readBuff, ref offset);
                    Console.WriteLine("Receive Message: ID = " + id.ToString() + " Len = " + len.ToString());
                    On(id, s, readBuff, ref offset);
                }
            }

            System.Threading.Thread.Sleep(1);
        }
    }

    static void On(byte id, Socket handler, byte[] data, ref int offset) {
        switch (id) {
            case 104:
                Console.WriteLine("On Login Req");
                OnLoginReq(handler, data, ref offset);
                break;
            case 101:
                Console.WriteLine("On Close Req");
                OnCloseReq(handler);
                break;
            default:
                Console.WriteLine("Unknown message id: {0}", id);
                break;
        }
    }

    static void TickSend() {
        foreach (var client in clients.Values) {
            Socket handler = client.socket;
            if (handler == null) {
                continue;
            }
            byte[] data = new byte[1024];
            int offset = 0;
            int msgCount = messageQueue.Count;
            ByteWriter.Write<int>(data, msgCount, ref offset);
            while (messageQueue.TryDequeue(out IMessage message)) {
                byte[] src = message.ToBytes();
                int len = src.Length + 5;
                byte id = ProtocolDict.GetID(message);

                ByteWriter.Write<int>(data, len, ref offset);
                ByteWriter.Write<byte>(data, id, ref offset);
                // ByteWriter.WriteArray<byte>(data, src, ref offset);
                Buffer.BlockCopy(src, 0, data, offset, src.Length);
                offset += src.Length;
            }
            if (offset > 0) {
                handler.Send(data, 0, offset, SocketFlags.None);
            }
        }
    }

    static void SendLoginRes(Socket handler) {
        LoginResMessage message = new LoginResMessage();
        message.status = 1;
        message.userToken = userTokens[handler];
        messageQueue.Enqueue(message);
    }

}