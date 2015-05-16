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

    [Packet("P")]
    class Ping : IPacketHandler
    {

        public void Process(Tcp tcp, Packet packet)
        {
            tcp.SendPacket(PacketTemplates.PING());
        }
    }
}
