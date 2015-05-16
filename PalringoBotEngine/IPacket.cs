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
    interface IPacket
    {
        void Prepare();
    }

    public class AUTH : IPacket
    {
        public void Prepare()
        {

        }
    }

    public class P : IPacket
    {
        public void Prepare()
        {

        }
    }

}
