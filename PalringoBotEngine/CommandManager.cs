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
using System.Reflection;
using System.Text;

namespace PalringoBotEngine
{
    public static class CommandManager
    {
        private static Dictionary<string, KeyValuePair<MethodInfo, CommandAttribute>>
            _commands = new Dictionary<string, KeyValuePair<MethodInfo, CommandAttribute>>();

        public static void InitializeCommands()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                var cmds = from type in asm.GetTypes()
                           let methods = type.GetMethods()
                           from method in methods
                           let attr = method.GetCustomAttributes(typeof(CommandAttribute), false)
                           where attr.Any()
                           select new KeyValuePair<string, KeyValuePair<MethodInfo, CommandAttribute>>(((CommandAttribute)attr[0]).Command.ToUpper(), new KeyValuePair<MethodInfo, CommandAttribute>(method, attr[0] as CommandAttribute));

                foreach (var cmd in cmds.ToDictionary(pair => pair.Key, pair => pair.Value))
                    _commands.Add(cmd.Key.ToLower(), cmd.Value);
            }
            ValidateCommands();
        }

        private static void ValidateCommands()
        {
            var strArray = new List<string>();
            foreach (var cmd in _commands)
            {
                var methodinfo = cmd.Value.Key;
                var cmdattr = cmd.Value.Value;
                Console.Out.Flush();
                var param = methodinfo.GetParameters();
                if (param.Length != 2)
                {
                    Console.Write(new string('-', Console.BufferWidth));
                    Console.WriteLine(
                        "The method {0} has {1} parameters.\r\nThere can only be 2 parameters in the following order, Sender and Message.",
                            methodinfo.Name, param.Length);
                    Console.Write(new string('-', Console.BufferWidth));
                    strArray.Add(cmd.Key);
                }
                else if (param[0].ParameterType != typeof(Tcp) || param[1].ParameterType != typeof(Message))
                {
                    Console.Write(new string('-', Console.BufferWidth));
                    Console.WriteLine(
                        "The method \"{0}\" has invalid parameters.\r\nFirst parameter must be Tcp, Second parameter must be Message",
                        methodinfo.Name);
                    Console.Write(new string('-', Console.BufferWidth));
                    strArray.Add(cmd.Key);
                }
                else
                {
                    Console.WriteLine("Loaded \"" + cmdattr.Command + "\" command for " + cmdattr.CommandType.ToString());
                }
            }
            foreach (var i in strArray)
            {
                _commands.Remove(i);
            }
        }

        public static void HandleMessage(Packet packet)
        {
            bool isGroup = packet.Headers.ContainsKey("TARGET-ID");
            var msgType = ParseMessageType(packet.Headers["CONTENT-TYPE"]);
            var data = packet.Payload;
            if (isGroup)
            {
                var groupId = int.Parse(packet.Headers["TARGET-ID"]);
                var sourceId = int.Parse(packet.Headers["SOURCE-ID"]);
                var cmds = _commands.Where(attr => attr.Value.Value.CommandType == Enums.CommandType.Group).ToList();
                foreach (var cmd in cmds)
                {
                    if (data.ToLower().StartsWith(Tcp.Namespace.ToLower() + " " + cmd.Value.Value.Command.ToLower()))
                    {
                        cmd.Value.Key.Invoke(null, new object[]
                        {
                            Tcp.Current,
                            new Message()
                                {
                                    MessageTarget = Enums.MessageTarget.Group,
                                    MessageType = msgType,
                                    Payload = new String (data.Skip(Tcp.Namespace.Length + 1).ToArray()),
                                    SourceId = sourceId,
                                    TargetId = groupId,
                                }
                        });
                    }
                }
            }
            else
            {
                var sourceId = int.Parse(packet.Headers["SOURCE-ID"]);
                var nickname = packet.Headers["NAME"];
                foreach (var cmd in _commands.Where(attr => attr.Value.Value.CommandType == Enums.CommandType.Private))
                {
                    if (!data.StartsWith(cmd.Value.Value.Command)) return;
                    cmd.Value.Key.Invoke(null, new object[]
                        {
                            Tcp.Current,
                            new Message()
                                {
                                    MessageTarget = Enums.MessageTarget.Private,
                                    MessageType = msgType,
                                    Payload = data,
                                    SourceId = sourceId,
                                }
                        });
                }
            }
        }

        private static Enums.MessageType ParseMessageType(string msgType)
        {
            switch (msgType.ToLower())
            {
                case "text/plain":
                    return Enums.MessageType.PlainText;
                case "image/jpeg":
                    return Enums.MessageType.Image;
                case "audio/x-speex":
                    return Enums.MessageType.Audio;
                case "image/jpeghtml":
                    return Enums.MessageType.RichMessage;
                default:
                    return Enums.MessageType.PlainText;
            }
        }
    }
}
