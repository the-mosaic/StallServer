using System.Net.Sockets;
using System.Text;
using TinyJson;

namespace StallServer
{
    public class StallID
    {
        public string text;
        public int? stallNumber;
        public ConsoleColor color;

        public StallID(string text, ConsoleColor color)
        {
            this.text = text;
            this.color = color;
        }

        public string GetPreferredText()
        {
            return stallNumber != null ? $"#{stallNumber}": text;
        }
    }

    public class Message
    {
        public string command;
        public Dictionary<string, string> headers;
        public string body;

        public Message(string command, Dictionary<string, string> headers, string body)
        {
            this.command = command;
            this.headers = headers != null ? headers : new Dictionary<string, string>();
            this.body = body;
        }

        public Message(string command)
        {
            this.command = command;
            this.headers = new Dictionary<string, string>();
            this.body = "";
        }

        private Message()
        {

        }

        public static Message CreateJSONMessage(string command, Dictionary<string, string> headers, object body)
        {
            Message result = new Message();

            result.command = command;
            result.headers = headers != null ? headers : new Dictionary<string, string>();
            result.body = body != null ? body.ToJson() : "{}";

            result.headers.Add("Content-Type", "text/json");
            result.headers.Add("Content-Length", Encoding.UTF8.GetByteCount(result.body).ToString());

            return result;
        }

        public static Message CreateDataMessage(string Type, string For, object Payload)
        {
            return CreateJSONMessage("DATA", null, new DataTypes.Data(Type, For, Payload));
        }

        public static Message CreateNestedDataMessage(string Type, string For, object Payload)
        {
            return CreateDataMessage("DATA", null, new DataTypes.Data(Type, For, Payload));
        }

        public string Pack()
        {
            string result = $"{command}\n";

            foreach (KeyValuePair<string, string> pair in headers)
            {
                result = $"{result}{pair.Key}: {pair.Value}\n";
            }

            result = $"{result}\n{body}";
            return result.Replace("\n", "\r\n");
        }

        public static Message Parse(string text)
        {
            string[] splitText = text.Split("\r\n\r\n");

            string body = splitText[1].Split("\n")[0];
            string[] header = splitText[0].Split("\r\n");

            string command = header[0];
            string[] unparsedHeaders = header.Skip(1).ToArray();
            Dictionary<string, string> headers = new Dictionary<string, string>();

            foreach (string line in unparsedHeaders)
            {
                string[] splitLine = line.Split(": ");
                headers.Add(splitLine[0], splitLine[1]);
            }

            return new Message(command, headers, body);
        }
    }

    public class Stall
    {
        private TcpClient client;
        private NetworkStream stream;
        private StallID ID;
        private bool handshakeFinished = false;

        public Stall(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();

            this.ID = Util.CreateStallID(12);
            Util.LogStall(this.ID, "Got connection.");
        }

        public void Serve()
        {
            while (true)
            {
                if (!client.Connected)
                {
                    Util.LogStall(this.ID, "Stall disconnected.");
                    return;
                }

                if (client.ReceiveBufferSize > 0)
                {
                    byte[] data = new byte[client.ReceiveBufferSize];

                    try
                    {
                        this.stream.Read(data, 0, client.ReceiveBufferSize);
                    }
                    catch (Exception ex)
                    {
                        Util.LogStall(this.ID, "Stall disconnected.");
                        return;
                    }

                    if (Util.IsByteArrayEmpty(data))
                    {
                        Util.LogStall(this.ID, "Stall disconnected.");
                        return;
                    }

                    Message message = Message.Parse(Encoding.UTF8.GetString(data));
                    RespondToMessage(message);
                }
            }
        }

        private void RespondToMessage(Message message)
        {
            switch (message.command.ToLower())
            {
                case "cmd hi":
                    Util.LogStall(this.ID, $"-> {message.command}");
                    SendMessage(new Message("RESP hi"));
                    break;
                case "cmd capabilities":
                    Util.LogStall(this.ID, $"-> {message.command}");
                    SendMessage(Message.CreateJSONMessage("RESP capabilities", null, null));
                    break;
            }

            if (message.command.ToLower().Contains("data"))
            {
                DataTypes.Data data = message.body.FromJson<DataTypes.Data>();

                switch (data.For.ToLower())
                {
                    case "devicetype":
                        Util.LogStall(this.ID, $"-> {data.Type} {data.For}");

                        SendMessage(Message.CreateDataMessage("RESP", "devicetype", null));
                        break;
                    case "stalllogin":
                        Util.LogStall(this.ID, $"-> {data.Type} {data.For}");

                        DataTypes.StallLogin stallLogin = data.GetDecodedPayload().FromJson<DataTypes.StallLogin>();
                        this.ID.stallNumber = stallLogin.StallNumber;

                        SendMessage(Message.CreateDataMessage("RESP", "stalllogin", null));
                        break;
                }

                if (data.Type.ToLower() == "data")
                {
                    data = data.GetDecodedPayload().FromJson<DataTypes.Data>();
                    DataTypes.Data payload = data.GetDecodedPayload().FromJson<DataTypes.Data>();

                    if (!string.IsNullOrWhiteSpace(payload.Type) || !string.IsNullOrWhiteSpace(payload.For))
                        Util.LogStall(this.ID, $"-> {payload.Type} {payload.For}");

                    switch (payload.For)
                    {
                        case "/sync/changes":
                            DataTypes.SyncChanges syncChanges = data.GetDecodedPayload().FromJson<DataTypes.SyncChanges>();

                            if (syncChanges.SyncInfo.Streams == null) return;
                            foreach (DataTypes.SyncInfo.Stream stream in syncChanges.SyncInfo.Streams)
                            {
                                Util.LogStall(ID, $"   {stream.ToString()}");

                                switch(stream.Type)
                                {
                                    case "RedButton":
                                        Util.LogStall(ID, $"   Red button");
                                        break;
                                }
                            }

                            break;
                    }
                }
            }
        }

        private void SendData(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            this.stream.Write(data, 0, data.Length);
        }

        private void SendMessage(Message message)
        {
            switch (message.command.ToLower())
            {
                case "data":
                    DataTypes.Data data = message.body.FromJson<DataTypes.Data>();

                    if (data.Type.ToLower() == "data")
                    {
                        data = data.GetDecodedPayload().FromJson<DataTypes.Data>();
                    }

                    Util.LogStall(this.ID, $"<- {data.Type} {data.For}");
                    break;
                default:
                    Util.LogStall(this.ID, $"<- {message.command}");
                    break;
            }

            SendData(message.Pack());
        }
    }
}
