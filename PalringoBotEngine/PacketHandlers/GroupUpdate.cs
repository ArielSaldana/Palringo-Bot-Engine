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
    [Packet("GROUP UPDATE")]
    public class GroupUpdate : IPacketHandler
    {
        public void Process(Tcp tcp, Packet packet)
        {
            System.Console.WriteLine("GroupUpdate Here");
        }
    }
}
