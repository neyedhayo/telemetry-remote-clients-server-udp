using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace acRemoteServerUDP_Example
{
    public struct Vector3f
    {
        public float x, y, z;

        public override string ToString()
        {
            return $"[{x}, {y}, {z}]";
        }
    }

    class Program
    {
        static string readString(BinaryReader br)
        {
            var length = br.ReadByte();
            return new string(br.ReadChars(length));
        }

        static string readStringW(BinaryReader br)
        {
            var length = br.ReadByte();
            return Encoding.UTF32.GetString(br.ReadBytes(length * 4));
        }

        static void writeStringW(BinaryWriter bw, string message)
        {
            bw.Write((byte)message.Length);
            bw.Write(Encoding.UTF32.GetBytes(message));
        }

        static void testSetSessionInfo(UdpClient client)
        {
            var buffer = new byte[1000];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_SET_SESSION_INFO);

            byte session_index = 1;
            bw.Write(session_index);

            writeStringW(bw, "SuperCoolServer");

            bw.Write((byte)3);
            bw.Write((UInt32)250);
            bw.Write((UInt32)0);
            bw.Write((UInt32)60);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }

        static void testGetSessionInfo(UdpClient client)
        {
            var buffer = new byte[100];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_GET_SESSION_INFO);
            bw.Write((Int16)(-1));

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }

        static void testGetCarInfo(UdpClient client, byte carID)
        {
            var buffer = new byte[100];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_GET_CAR_INFO);
            bw.Write(carID);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }

        static void enableRealtimeReport(UdpClient client)
        {
            var buffer = new byte[100];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_REALTIMEPOS_INTERVAL);
            bw.Write((UInt16)1000); // 1Hz

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }

        static async Task SendTelemetryDataOverWebSocket(ClientWebSocket webSocket, string jsonData)
        {
            byte[] jsonBuffer = Encoding.UTF8.GetBytes(jsonData);
            await webSocket.SendAsync(new ArraySegment<byte>(jsonBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Data sent over WebSocket: " + jsonData);
        }

        // Main method with correct signature
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // The Run method remains as it is, and it will be called from MainForm.
        public static async Task Run(string udpIp, int udpPort, string wsIp, int wsPort)
        {
            // Set up UDP Client
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, udpPort);
            var client = new UdpClient(ep);

            // Set up WebSocket Client
            ClientWebSocket webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri($"ws://{wsIp}:{wsPort}"), CancellationToken.None);

            while (true)
            {
                var src_ep = new IPEndPoint(IPAddress.Any, 0);
                var bytes = client.Receive(ref src_ep);

                var br = new BinaryReader(new MemoryStream(bytes));
                var packet_id = br.ReadByte();
                Console.WriteLine("PACKET ID:" + packet_id);

                switch (packet_id)
                {
                    case ACSProtocol.ACSP_ERROR:
                        var msg = readStringW(br);
                        Console.WriteLine(msg);
                        break;

                    case ACSProtocol.ACSP_CHAT:
                        var car_id = br.ReadByte();
                        msg = readStringW(br);
                        Console.WriteLine("CHAT FROM CAR:" + (int)car_id + " MSG:" + msg);
                        break;

                    case ACSProtocol.ACSP_CLIENT_LOADED:
                        car_id = br.ReadByte();
                        Console.WriteLine("CLIENT LOADED:" + (int)car_id);
                        break;

                    case ACSProtocol.ACSP_VERSION:
                        var protocol_version = br.ReadByte();
                        Console.WriteLine("PROTOCOL VERSION IS:" + (int)protocol_version);
                        break;

                    case ACSProtocol.ACSP_NEW_SESSION:
                        Console.WriteLine("New session started");
                        goto case ACSProtocol.ACSP_SESSION_INFO;

                    case ACSProtocol.ACSP_SESSION_INFO:
                        Console.WriteLine("Session Info");

                        var version = br.ReadByte();
                        var sess_index = br.ReadByte();
                        var current_session_index = br.ReadByte();
                        var session_count = br.ReadByte();
                        var server_name = readStringW(br);
                        var track = readString(br);
                        var track_config = readString(br);
                        var name = readString(br);
                        var type = br.ReadByte();
                        var time = br.ReadUInt16();
                        var laps = br.ReadUInt16();
                        var waitTime = br.ReadUInt16();
                        var ambient_temp = br.ReadByte();
                        var road_temp = br.ReadByte();
                        var weather_graphics = readString(br);
                        var elapsedMS = br.ReadInt32();

                        Console.WriteLine("PROTOCOL VERSION:" + version);
                        Console.WriteLine("SESSION INDEX:" + sess_index + "/" + session_count + " CURRENT SESSION:" + current_session_index);
                        Console.WriteLine("SERVER NAME:" + server_name);
                        Console.WriteLine("TRACK:" + track + " [" + track_config + "]");
                        Console.WriteLine("NAME:" + name);
                        Console.WriteLine("TYPE:" + type);
                        Console.WriteLine("TIME:" + time);
                        Console.WriteLine("LAPS:" + laps);
                        Console.WriteLine("WAIT TIME:" + waitTime);
                        Console.WriteLine("WEATHER:" + weather_graphics + " AMBIENT:" + ambient_temp + " ROAD:" + road_temp);
                        Console.WriteLine("ELAPSED:" + elapsedMS);

                        var sessionInfo = new
                        {
                            ProtocolVersion = version,
                            SessionIndex = sess_index,
                            CurrentSessionIndex = current_session_index,
                            SessionCount = session_count,
                            ServerName = server_name,
                            Track = track,
                            TrackConfig = track_config,
                            Name = name,
                            Type = type,
                            Time = time,
                            Laps = laps,
                            WaitTime = waitTime,
                            AmbientTemp = ambient_temp,
                            RoadTemp = road_temp,
                            WeatherGraphics = weather_graphics,
                            ElapsedMS = elapsedMS
                        };

                        string jsonData = JsonConvert.SerializeObject(sessionInfo);
                        await SendTelemetryDataOverWebSocket(webSocket, jsonData);

                        break;

                    case ACSProtocol.ACSP_END_SESSION:
                        Console.WriteLine("ACSP_END_SESSION");
                        var filename = readStringW(br);
                        Console.WriteLine("REPORT JSON AVAILABLE AT:" + filename);
                        break;

                    case ACSProtocol.ACSP_CLIENT_EVENT:
                        HandleClientEvent(br);
                        break;

                    case ACSProtocol.ACSP_CAR_INFO:
                        HandleCarInfo(br, client);
                        break;

                    case ACSProtocol.ACSP_CAR_UPDATE:
                        await HandleCarUpdate(br, webSocket);
                        break;

                    case ACSProtocol.ACSP_NEW_CONNECTION:
                        HandleNewConnection(br, client);
                        break;

                    case ACSProtocol.ACSP_CONNECTION_CLOSED:
                        HandleConnectionClosed(br);
                        break;

                    case ACSProtocol.ACSP_LAP_COMPLETED:
                        HandleLapCompleted(br);
                        break;

                    default:
                        Console.WriteLine("Unhandled Packet ID:" + packet_id);
                        break;
                }
            }
        }

        static void HandleClientEvent(BinaryReader br)
        {
            var ev_type = br.ReadByte();
            var car_id = br.ReadByte();
            byte other_car_id = 255;

            switch (ev_type)
            {
                case ACSProtocol.ACSP_CE_COLLISION_WITH_CAR:
                    other_car_id = br.ReadByte();
                    break;

                case ACSProtocol.ACSP_CE_COLLISION_WITH_ENV:
                    break;
            }

            var speed = br.ReadSingle();
            var world_pos = readVector3f(br);
            var rel_pos = readVector3f(br);

            switch (ev_type)
            {
                case ACSProtocol.ACSP_CE_COLLISION_WITH_ENV:
                    Console.WriteLine($"COLLISION WITH ENV, CAR:{car_id} IMPACT SPEED:{speed} WORLD_POS:{world_pos} REL_POS:{rel_pos}");
                    break;

                case ACSProtocol.ACSP_CE_COLLISION_WITH_CAR:
                    Console.WriteLine($"COLLISION WITH CAR, CAR:{car_id} OTHER CAR:{other_car_id} IMPACT SPEED:{speed} WORLD_POS:{world_pos} REL_POS:{rel_pos}");
                    break;
            }
        }

        static void HandleCarInfo(BinaryReader br, UdpClient client)
        {
            Console.WriteLine("ACSP_CAR_INFO");

            var car_id = br.ReadByte();
            var is_connected = br.ReadByte() != 0;
            var model = readStringW(br);
            var skin = readStringW(br);
            var driver_name = readStringW(br);
            var driver_team = readStringW(br);
            var driver_guid = readStringW(br);

            Console.WriteLine($"CAR:{car_id} {model} [{skin}] DRIVER:{driver_name} TEAM:{driver_team} GUID:{driver_guid} CONNECTED:{is_connected}");

            testSetSessionInfo(client);
        }

        static async Task HandleCarUpdate(BinaryReader br, ClientWebSocket webSocket)
        {
            Console.WriteLine("ACSP_CAR_UPDATE");

            var carId = br.ReadByte();
            var pos = readVector3f(br);
            var velocity = readVector3f(br);
            var gear = br.ReadByte();
            var engineRpm = br.ReadUInt16();
            var normalizedSplinePos = br.ReadSingle();

            Console.Write($"CAR:{carId} POS:{pos} VEL:{velocity} GEAR:{gear} RPM:{engineRpm} NSP:{normalizedSplinePos}");

            var carUpdate = new
            {
                CarId = carId,
                Position = pos,
                Velocity = velocity,
                Gear = gear,
                EngineRPM = engineRpm,
                NormalizedSplinePos = normalizedSplinePos
            };

            string jsonData = JsonConvert.SerializeObject(carUpdate);
            await SendTelemetryDataOverWebSocket(webSocket, jsonData);
        }

        static void HandleNewConnection(BinaryReader br, UdpClient client)
        {
            Console.WriteLine("ACSP_NEW_CONNECTION");

            var driver_name = readStringW(br);
            var driver_guid = readStringW(br);
            var car_id = br.ReadByte();
            var car_model = readString(br);
            var car_skin = readString(br);

            Console.WriteLine($"DRIVER:{driver_name} GUID:{driver_guid}");
            Console.WriteLine($"CAR:{car_id} MODEL:{car_model} SKIN:{car_skin}");

            testGetCarInfo(client, car_id);
        }

        static void HandleConnectionClosed(BinaryReader br)
        {
            Console.WriteLine("ACSP_CONNECTION_CLOSED");

            var driver_name = readStringW(br);
            var driver_guid = readStringW(br);
            var car_id = br.ReadByte();
            var car_model = readString(br);
            var car_skin = readString(br);

            Console.WriteLine($"DRIVER:{driver_name} GUID:{driver_guid}");
            Console.WriteLine($"CAR:{car_id} MODEL:{car_model} SKIN:{car_skin}");
        }

        static void HandleLapCompleted(BinaryReader br)
        {
            Console.WriteLine("ACSP_LAP_COMPLETED");

            var car_id = br.ReadByte();
            var laptime = br.ReadUInt32();
            var cuts = br.ReadByte();
            Console.WriteLine($"CAR:{car_id} LAP:{laptime} CUTS:{cuts}");

            var cars_count = br.ReadByte();

            for (int i = 0; i < cars_count; i++)
            {
                var rcar_id = br.ReadByte();
                var rtime = br.ReadUInt32();
                var rlaps = br.ReadUInt16();
                var has_completed_flag = br.ReadByte() != 0;
                Console.WriteLine($"{i + 1}: CAR_ID:{rcar_id} TIME:{rtime} LAPS:{rlaps} HAS COMPLETED:{has_completed_flag}");
            }

            var grip_level = br.ReadSingle();
            Console.WriteLine("GRIP LEVEL:" + grip_level);
        }

        public static Vector3f readVector3f(BinaryReader br)
        {
            Vector3f res = new Vector3f
            {
                x = br.ReadSingle(),
                y = br.ReadSingle(),
                z = br.ReadSingle()
            };

            return res;
        }

        public static void sendChat(UdpClient client, byte carid, string message)
        {
            var buffer = new byte[255];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_SEND_CHAT);
            bw.Write(carid);
            writeStringW(bw, message);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }

        public static void broadcastChat(UdpClient client, string message)
        {
            var buffer = new byte[255];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_BROADCAST_CHAT);
            writeStringW(bw, message);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }

        public static void testKick(UdpClient client, byte userid)
        {
            var buffer = new byte[255];
            var bw = new BinaryWriter(new MemoryStream(buffer));

            bw.Write(ACSProtocol.ACSP_KICK_USER);
            bw.Write(userid);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            client.Send(buffer, (int)bw.BaseStream.Length, ep);
        }
    }
}
