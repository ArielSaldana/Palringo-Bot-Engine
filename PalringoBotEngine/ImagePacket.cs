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
using System.IO;
using System.Linq;
using System.Text;

namespace PalringoBotEngine
{
    class ImagePacket
    {
        public static Packet[] Package(int id, Enums.MessageTarget target, byte[] img)
        {
            int totalPackets = img.Length % 512 > 1 ? img.Length / 512 : (img.Length / 512) - 1;
            bool first = true;
            var packets = new List<Packet>();
            while (true)
            {
                if (first)
                {
                    packets.Add(ProcessFirstChunk(img.Length, img.Take(512).ToArray(), id, target));
                    first = false;
                    img = img.Skip(512).ToArray();
                }
                else
                {
                    if (img.Length > 512)
                    {
                        packets.Add(ProcessBodyChunk(img.Take(512).ToArray(), id, target));
                        img = img.Skip(512).ToArray();
                    }
                    else
                    {
                        packets.Add(ProcessLastChunk(img, id, target));
                        break;
                    }
                }
            }
            return packets.ToArray();
        }

        private static Packet ProcessFirstChunk(int totalLength, byte[] data, int id, Enums.MessageTarget target)
        {
            Packet SendImage = new Packet();
            SendImage.Command = "MESG";
            SendImage.Headers.Add("content-type", "image/jpeg");
            SendImage.Headers.Add("mesg-id", "32840");
            SendImage.Headers.Add("mesg-target", ((int)target).ToString());
            SendImage.Headers.Add("target-id", id.ToString());
            SendImage.Headers.Add("total-Length", totalLength + "");
            SendImage.Payload = Encoding.GetEncoding("windows-1252").GetString(data);
            return SendImage;
        }

        private static Packet ProcessBodyChunk(byte[] data, int id, Enums.MessageTarget target)
        {
            Packet SendImage = new Packet();
            SendImage.Command = "MESG";
            SendImage.Headers.Add("content-type", "image/jpeg");
            SendImage.Headers.Add("correlation-id", "32840");
            SendImage.Headers.Add("mesg-id", id.ToString());
            SendImage.Headers.Add("mesg-target", ((int)target).ToString());
            SendImage.Headers.Add("target-id", id.ToString());
            SendImage.Payload = Encoding.GetEncoding("windows-1252").GetString(data);

            return SendImage;
        }

        private static Packet ProcessLastChunk(byte[] data, int id, Enums.MessageTarget target)
        {
            Packet SendImage = new Packet();
            SendImage.Command = "MESG";
            SendImage.Headers.Add("content-type", "image/jpeg");
            SendImage.Headers.Add("correlation-id", "32840");
            SendImage.Headers.Add("last", "1");
            SendImage.Headers.Add("mesg-id", "32862");
            SendImage.Headers.Add("mesg-target", ((int)target).ToString());
            SendImage.Headers.Add("target-id", id.ToString());
            SendImage.Payload = Encoding.GetEncoding("windows-1252").GetString(data);

            return SendImage;
        }
    }
}
