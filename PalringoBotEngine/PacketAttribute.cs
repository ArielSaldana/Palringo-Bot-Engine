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
using System.Threading.Tasks;

namespace PalringoBotEngine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PacketAttribute : Attribute
    {
        public string Command = "";

        public PacketAttribute(string cmd)
        {
            Command = cmd;
        }
    }
}
