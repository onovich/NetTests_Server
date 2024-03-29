using System.Net;
using System.Net.Sockets;
using MortiseFrame.LitIO;
using MortiseFrame.Rill;

class ServerMain {

    static ServerCore serverCore;
    static Dictionary<ConnectionEntity, string> userTokens = new Dictionary<ConnectionEntity, string>();

    static void Main(string[] args) {

        try {

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback, 8080);
            serverCore = new ServerCore();
            Register();
            BindingEvent();

            serverCore.Start(localEndPoint.Address, localEndPoint.Port);

            while (true) {
                Tick();
                System.Threading.Thread.Sleep(1);
            }

        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }

    static void BindingEvent() {
        serverCore.OnConnect((conn) => OnConnectReq(conn));
        serverCore.On<LoginReqMessage>((msg, conn) => OnLoginReq((LoginReqMessage)msg, conn));
        serverCore.On<CloseReqMessage>((msg, conn) => OnCloseReq((CloseReqMessage)msg, conn));
    }

    static void Register() {
        serverCore.Register(typeof(ConnectResMessage));
        serverCore.Register(typeof(LoginResMessage));
        serverCore.Register(typeof(LoginReqMessage));
        serverCore.Register(typeof(CloseReqMessage));
    }

    static void OnCloseReq(CloseReqMessage msg, ConnectionEntity conn) {
        Console.WriteLine("Client Disconnected");
        conn.clientfd.Close();
    }

    static void OnConnectReq(ConnectionEntity conn) {
        Console.WriteLine("Receive A New Client Connect Request");
        SendConnectRes(conn);
    }

    static void OnLoginReq(LoginReqMessage msg, ConnectionEntity conn) {
        string userToken = msg.userToken;
        if (userToken == null) {
            Console.WriteLine("UserToken is Null");
            return;
        }
        userTokens[conn] = userToken;
        SendLoginRes(conn);
        Console.WriteLine("User Login Success: " + userToken);
    }

    static void SendConnectRes(ConnectionEntity conn) {
        ConnectResMessage message = new ConnectResMessage();
        serverCore.Send(message, conn);
        Console.WriteLine("Send ConnectRes");
    }

    static void SendLoginRes(ConnectionEntity conn) {
        LoginResMessage message = new LoginResMessage();
        message.status = 1;
        message.userToken = userTokens[conn];
        serverCore.Send(message, conn);
        Console.WriteLine("Send LoginRes");
    }

    static void Tick() {
        serverCore.Tick(0);
    }

}