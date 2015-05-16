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
using System.Linq;
using System.Text;

namespace PalringoBotEngine
{
    public sealed class Packet
    {
        public string Command = "";
        public Dictionary<string, string> Headers
            = new Dictionary<string, string>();
        public string Payload = "";

        public Packet()
        {
        }

        public Packet(string cmd)
        {
            this.Command = cmd;
        }

        public string this[string key]
        {
            get { return this.Headers[key]; }
        }

        public void AddHeader(string key, string value)
        {
            this.Headers.Add(key.ToUpper(), value);
        }

        public bool ContainsHeader(string key)
        {
            return Headers.ContainsKey(key.ToUpper());
        }

        public string Serialize()
        {
            if (!string.IsNullOrEmpty(this.Payload) && !this.Headers.ContainsKey("CONTENT-LENGTH"))
            {
                this.Headers.Add("CONTENT-LENGTH", this.Payload.Length.ToString());
            }
            var packetBuilder = new StringBuilder();
            packetBuilder.Append(this.Command + "\r\n");

            foreach (var header in this.Headers.Where(header => !String.IsNullOrEmpty(header.Value)))
            {
                packetBuilder.Append(header.Key.ToUpper() + ": " + header.Value + "\r\n");
            }
            packetBuilder.Append("\r\n");

            if (!string.IsNullOrEmpty(this.Payload))
            {
                packetBuilder.Append(this.Payload);
            }

            return packetBuilder.ToString();
        }

        public void Clear()
        {
            Command = "";
            Headers.Clear();
            Payload = "";
        }

        public override string ToString()
        {
            var build = new StringBuilder();
            build.AppendLine("Command - " + this.Command);
            foreach (var header in this.Headers)
            {
                build.AppendLine("Header - " + header.Key + " Value - " + header.Value);
            }
            build.AppendLine("Payload - " + (Payload.Length > 0));
            return build.ToString();
        }

        public void Write()
        {
            PalringoBotEngine.Tcp.Current.SendPacket(this);
        }
    }
}
