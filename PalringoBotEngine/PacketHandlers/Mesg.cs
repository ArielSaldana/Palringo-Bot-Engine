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
// include javascript engine dll
using System.IO;
namespace PalringoBotEngine
{

    [Packet("MESG")]
    internal class Mesg : IPacketHandler
    {
        public void Process(Tcp tcp, Packet packet)
        {
        
            //tcp.SendPacket(PacketTemplates.PING());
            CommandManager.HandleMessage(packet);
        }

        

        [Command("test", Enums.CommandType.Group)]
        public static void testCommand(Tcp tcp, Message message)
        {
            tcp.SendGroupTextMessage(message.TargetId, "test working");
        }


    }
}
