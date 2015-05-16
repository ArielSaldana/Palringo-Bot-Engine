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
    public class Message
    {
        public string Payload;
        public int TargetId;
        public int SourceId;
        public Enums.MessageType MessageType;
        public Enums.MessageTarget MessageTarget;
    }
}
