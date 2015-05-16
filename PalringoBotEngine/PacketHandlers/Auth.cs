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
    [Packet("AUTH")]
    public class Auth : IPacketHandler
    {
        public void Process(Tcp tcp, Packet packet)
        {
            var payload = Encoding.Default.GetBytes(packet.Payload); // windows-1252
            var password = Encoding.Default.GetBytes(tcp.Password);
            var encrypted = Crypto.GenerateAuth(payload, password);
            var authpacket = PacketTemplates.Auth(encrypted, Enums.OnlineStatus.Online);
            tcp.SendPacket(authpacket);
        }
    }
}
