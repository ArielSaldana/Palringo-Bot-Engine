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
    public class PacketParser
    {
        private Action<Packet> _packetCallback;

        private Packet _packet;
        private int _contentLength;
        private StringBuilder _rawData;
        private bool _headersParsed;

        public PacketParser(Action<Packet> packetCallback)
        {
            _packet = new Packet();
            _contentLength = 0;
            _rawData = new StringBuilder();
            _packetCallback = packetCallback;
        }

        public void Process(string data)
        {
            _rawData.Append(data);
            AttemptParse();
        }

        private void AttemptParse()
        {
            if (!_headersParsed)
            {
                var raw = _rawData.ToString();
                int index = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                if (index == -1)
                    return;
                ParseHeaders(_rawData.ToString(0, index));
                _rawData.Remove(0, index + 4);
                AttemptParse();
            }
            if (this._rawData.Length == this._contentLength)
            {
                //set the payload to our string stored in memory
                this._packet.Payload = this._rawData.ToString();
                this.Collect();
            }

                //got over the content we needed (start of another packet..)
            else if (this._rawData.Length >= this._contentLength)
            {
                //only get the stuff we need
                this._packet.Payload = this._rawData.ToString(0, this._contentLength);
                this.Collect();
                AttemptParse();
            }
        }

        private void ParseHeaders(string rawHeaders)
        {
            string cmd = "";
            string[] lines = rawHeaders.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var headers = new Dictionary<string, string>();
            int lineCount = lines.Count();
            for (int i = 0; i < lineCount; i++)
            {
                if (i == 0)
                    cmd = lines[i];
                else
                {
                    if (string.IsNullOrEmpty(lines[i])) continue;
                    var lnindex = lines[i].IndexOf(':');
                    if (lnindex > -1)
                    {
                        headers.Add(lines[i].Substring(0, lnindex).Trim().ToUpper(),
                                      lines[i].Substring(lnindex + 2).Trim());
                    }
                }
            }
            _packet.Headers = headers;
            _packet.Command = cmd.ToUpper();
            if (_packet.Headers.ContainsKey("CONTENT-LENGTH"))
            {
                _contentLength = int.Parse(_packet.Headers["CONTENT-LENGTH"]);
            }
            _headersParsed = true;
        }

        private void Collect()
        {
            if (_packet.Command == "")
                return;
            _headersParsed = false;

            if (_packetCallback != null)
                _packetCallback(_packet);

            _packet.Clear();

            if (_contentLength > 0)
            {
                _rawData.Remove(0, _contentLength);
                _contentLength = 0;
            }
        }
    }
}
