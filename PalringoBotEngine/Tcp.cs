/*
 * 
 * By: Ariel Saldana
 * Released under the MIT License
 * https://github.com/arielsaldana
 * http://ahhriel.com
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PalringoBotEngine
{
    public class Tcp
    {
        public static string Namespace = "";

        public static Tcp Current;

        public delegate void GeneralDelegate();

        public delegate void StringDelegate(string str);

        public delegate void PacketDelegate(Packet packet);

        public delegate void ExceptionDelegate(Exception exception);

        public event PacketDelegate PacketSent, PacketParsed;
        public event GeneralDelegate LoginSuccess;

        private TcpClient _tcpClient;
        private bool _loggedIn;
        private readonly string _host;
        private readonly int _port;
        private PacketParser _parser;
        private NetworkStream _networkStream;
        private byte[] _buffer = new byte[4096];

        public string Username { get; set; }
        public string Password { get; set; }

        [DefaultValue(Enums.OnlineStatus.Online)]
        public Enums.OnlineStatus OnlineStatus { get; set; }

        [DefaultValue(Enums.DeviceType.Android)]
        public Enums.DeviceType DeviceType { get; set; }

        //public Tcp(string host = "80.69.129.4", int port = 12345) 80.69.129.12
        //public Tcp(string host = "80.69.129.64", int port = 8080)
        public Tcp(string host = "80.69.129.123", int port = 443)
        {
            _host = host;
            _port = port;
            _parser = new PacketParser(packet =>
            {
                if (PacketParsed != null)
                    PacketParsed(packet);
                PacketManager.HandlePacket(this, packet);
            });
            Current = this;
        }

        public bool Login(
            string username,
            string password,
            Enums.OnlineStatus onlineStatus = Enums.OnlineStatus.Online,
            Enums.DeviceType deviceType = Enums.DeviceType.Android)
        {
            try
            {
                Current = this;
                PacketManager.InitializeHandlers();
                CommandManager.InitializeCommands();
                Username = username;
                Password = password;
                OnlineStatus = onlineStatus;
                DeviceType = deviceType;
                _tcpClient = new TcpClient(_host, _port);
                Current = this;
                _networkStream = _tcpClient.GetStream();
                SendPacket(PacketTemplates.Logon(Username, DeviceType));
                _networkStream.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
                Console.WriteLine(_buffer.Length);
                return true;
            }
            catch (Exception a)
            {
                return false;
            }
        }

        private void SendData(string data)
        {
            SendData(Encoding.GetEncoding("windows-1252").GetBytes(data));
        }

        private void SendData(byte[] data)
        {
            _networkStream.BeginWrite(data, 0, data.Length, WriteCallback, null);
        }

        public void SendPacket(Packet packet)
        {
            SendData(packet.Serialize());
            if (PacketSent != null)
                PacketSent(packet);
        }

        public void SendPacket(Packet[] packets)
        {
            foreach (var packet in packets)
                SendPacket(packet);
        }

        private void WriteCallback(IAsyncResult ar)
        {
            _networkStream.EndWrite(ar);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var result = _networkStream.EndRead(ar);
            if (result <= 0)
                return;
            var data = Encoding.GetEncoding("windows-1252").GetString(_buffer, 0, result);
            _parser.Process(data);
            _networkStream.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }


        // Raise Event

        internal void RaiseLoginSuccess()
        {
            if (LoginSuccess != null)
                LoginSuccess();
        }



        // Misc

        public void SendGroupTextMessage(int groupId, string message)
        {
            if (message.Length <= 512)
            {
                PacketTemplates.GroupMessage(groupId, message).Write();
            }
            else
            {
                PacketTemplates.ChunkGroupMessage(groupId, message).ToList().ForEach(p => p.Write());
            }
        }

        public void SendPrivateTextMessage(int userId, string message)
        {
            PacketTemplates.PrivateMessage(userId, message).Write();
        }

        public void SendGroupImage(int groupId, byte[] img)
        {
            PacketTemplates.GroupImage(groupId, img).ToList().ForEach(p => p.Write());
        }
    }
}
