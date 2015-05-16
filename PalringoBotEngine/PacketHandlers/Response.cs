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
    [Packet("RESPONSE")]
    internal class Response : IPacketHandler
    {
        public void Process(Tcp tcp, Packet packet)
        {
            return;
            Console.WriteLine("[Start Response Packet]");
            Console.WriteLine(packet.ToString());
            var what = int.Parse(packet["WHAT"]);
            Console.WriteLine("[Stop Response Packet]");
        }
    }
}
