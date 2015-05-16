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
    class StateObject
    {
        /* Contains the state information. */
        private int id;
        private bool close = false; // Used to close the socket after the message sent.

        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();

        public StateObject() { }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public bool Close
        {
            get { return this.close; }
            set { this.close = value; }
        }
    }
}
