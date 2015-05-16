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
    public class MessagePacket
    {
        public static Packet[] Package(int id, Enums.MessageTarget target, string message)
        {
            int totalPackets = message.Length % 512 > 1 ? message.Length / 512 : (message.Length / 512) - 1;
            bool first = true;
            var packets = new List<Packet>();
            while (true)
            {
                if (first)
                {
                    packets.Add(ProcessFirstChunk(message.Length, message.Substring(0, 512), id, target));
                    first = false;
                    message = message.Substring(512);
                }
                else
                {
                    if (message.Length > 512)
                    {
                        packets.Add(ProcessBodyChunk(message.Substring(0, 512), id, target));
                        message = message.Substring(512);
                    }
                    else
                    {
                        packets.Add(ProcessLastChunk(message, id, target));
                        break;
                    }
                }
            }
            return packets.ToArray();
        }

        private static Packet ProcessFirstChunk(int totalLength, string message, int id, Enums.MessageTarget target)
        {
            Packet SendImage = new Packet();
            SendImage.Command = "MESG";
            SendImage.Headers.Add("content-type", "text/plain");
            SendImage.Headers.Add("mesg-id", "32840");
            SendImage.Headers.Add("mesg-target", ((int)target).ToString());
            SendImage.Headers.Add("target-id", id.ToString());
            SendImage.Headers.Add("total-Length", totalLength + "");
            SendImage.Payload = message;
            return SendImage;
        }

        private static Packet ProcessBodyChunk(string message, int id, Enums.MessageTarget target)
        {
            Packet SendImage = new Packet();
            SendImage.Command = "MESG";
            SendImage.Headers.Add("content-type", "text/plain");
            SendImage.Headers.Add("correlation-id", "32840");
            SendImage.Headers.Add("mesg-id", id.ToString());
            SendImage.Headers.Add("mesg-target", ((int)target).ToString());
            SendImage.Headers.Add("target-id", id.ToString());
            SendImage.Payload = message;

            return SendImage;
        }

        private static Packet ProcessLastChunk(string message, int id, Enums.MessageTarget target)
        {
            Packet SendImage = new Packet();
            SendImage.Command = "MESG";
            SendImage.Headers.Add("content-type", "text/plain");
            SendImage.Headers.Add("correlation-id", "32840");
            SendImage.Headers.Add("last", "1");
            SendImage.Headers.Add("mesg-id", "32862");
            SendImage.Headers.Add("mesg-target", ((int)target).ToString());
            SendImage.Headers.Add("target-id", id.ToString());
            SendImage.Payload = message;

            return SendImage;
        }
    }
}
