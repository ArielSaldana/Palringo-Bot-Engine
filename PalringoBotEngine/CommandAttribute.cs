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
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Command;
        public Enums.CommandType CommandType;

        public CommandAttribute(string cmd, Enums.CommandType cmdType)
        {
            Command = cmd;
            CommandType = cmdType;
        }
    }
}
