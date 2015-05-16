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
    public interface IPacketHandler
    {
        void Process(Tcp tcp, Packet packet);
    }

    internal static class PacketManager
    {
        private static Dictionary<string, IPacketHandler> _packetHandlers = new Dictionary<string, IPacketHandler>();

        public static void InitializeHandlers()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var handlers in asms.Select(asm => (from type in asm.GetTypes()
                                                         where type.IsClass
                                                         where typeof(IPacketHandler).IsAssignableFrom(type)
                                                         let handler = (IPacketHandler)Activator.CreateInstance(type)
                                                         let attributes = type.GetCustomAttributes(typeof(PacketAttribute), false)
                                                         select new KeyValuePair<string, IPacketHandler>(((PacketAttribute)attributes[0]).Command, handler))))
            {
                foreach (var handler in handlers.ToDictionary(pair => pair.Key, pair => pair.Value))
                    _packetHandlers.Add(handler.Key, handler.Value);
            }
        }

        public static void HandlePacket(Tcp tcp, Packet packet)
        {
            if (_packetHandlers.ContainsKey(packet.Command))
                _packetHandlers[packet.Command].Process(tcp, packet);
            else
                Console.WriteLine("UNSUPPORTED COMMAND: " + packet.Command);
        }
    }
}