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
using System.IO;
using System.Linq;
using System.Text;

namespace PalringoBotEngine
{
    [Packet("SUB PROFILE")]
    internal class SubProfile : IPacketHandler
    {
        private static int i = 0;
        private static bool _loginPacket = true;
        public void Process(Tcp tcp, Packet packet)
        {
            // Gather info for deserialization
            int iv = 0;
            int rk = 0;
            if (packet.ContainsHeader("IV"))
                iv = int.Parse(packet["IV"]);
            if (iv > 0)
                rk = int.Parse(packet["RK"]);
            int l = packet.Payload.Length;
            int length = packet.Payload.Length - iv - rk;
            int startPos = iv;

            // Time to extract information
            var enc = Encoding.GetEncoding("windows-1252"); // Allows support for other OS (provided they have the windows-1252 encoding)
            var maps = Deserialize(enc.GetBytes(packet.Payload), startPos, length);

            try
            {
                var contactsMap = Deserialize(maps["contacts"], 0, maps["contacts"].Length);
                ParseContacts(contactsMap);
            }
            catch { }

            try
            {
                var groupsMap = Deserialize(maps["group_sub"], 0, maps["group_sub"].Length);
                ParseGroups(groupsMap);
            }
            catch { }

            try
            {
                var blockedMap = Deserialize(maps["blocked"], 0, maps["blocked"].Length);
                ParseBlockedUsers(blockedMap);
            }
            catch { }

            try
            {
                var requestsMap = Deserialize(maps["contact_add"], 0, maps["contact_add"].Length);
                ParseContactRequests(requestsMap);
            }
            catch { }

            if (_loginPacket)
            {
                var profile = new Contact();
                profile.ContactID = uint.Parse(Encoding.UTF8.GetString(maps["sub-id"]));
                profile.Reputation = int.Parse(Encoding.UTF8.GetString(maps["rep"]));
                profile.Privileges = uint.Parse(Encoding.UTF8.GetString(maps["privileges"]));
                profile.Nickname = Encoding.UTF8.GetString(maps["nickname"]);
                Cache.Profile = profile;
                tcp.RaiseLoginSuccess();
                _loginPacket = false;
            }
        }

        #region Groups Parser

        private static void ParseGroups(Dictionary<string, byte[]> maps)
        {
            foreach (var map in maps)
            {
                try
                {
                    uint groupId = uint.Parse(map.Key);
                    ParseGroup(groupId, Deserialize(map.Value, 0, map.Value.Length));
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid Group Id found. Skipped group");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("Invalid Group Id found. Skipped group");
                }
                catch { }
            }
        }

        private static void ParseGroup(uint groupId, Dictionary<string, byte[]> map)
        {


            if (!Cache.Groups.ContainsKey(groupId))
                Cache.Groups[groupId] = new Group();

            var group = Cache.Groups[groupId];
            group.GroupID = groupId;
            if (map.ContainsKey("name"))
                group.Name = Encoding.GetEncoding("windows-1252").GetString(map["name"]);

            if (map.ContainsKey("desc"))
                group.Description = Encoding.GetEncoding("windows-1252").GetString(map["desc"]);

            if (map.ContainsKey("owner"))
                group.OwnerID = int.Parse(Encoding.UTF8.GetString(map["owner"]));

            var attributeMap = Deserialize(map["attributes"], 0, map["attributes"].Length);
            if (attributeMap != null)
            {
                foreach (var attr in attributeMap)
                {
                    var attrMap = Deserialize(attr.Value);
                    var attribute = (Group.Attributes)int.Parse(Encoding.UTF8.GetString(attrMap["attribute_type"]));
                    switch (attribute)
                    {
                        case Group.Attributes.LongDesc:
                            group.LongDescription = Encoding.UTF8.GetString(attrMap["data"]);
                            continue;
                        case Group.Attributes.Premium:
                            group.Premium = attrMap["data"].Length != 0;
                            continue;
                        case Group.Attributes.Permanent:
                            group.Permanent = attrMap["data"].Length != 0;
                            continue;
                        case Group.Attributes.Adult:
                            int data = int.Parse(Encoding.UTF8.GetString(attrMap["data"]));
                            if ((data & 1) == 0)
                            {
                                if ((data & 2) == 0)
                                    continue;
                                group.Mature = true;
                                continue;
                            }
                            group.Adult = true;
                            continue;
                        default: continue;
                    }
                }
            }
            foreach (var contact in map.Where(kv => kv.Key.IsNumeric()))
            {
                var contactId = uint.Parse(contact.Key);
                var contactMap = Deserialize(contact.Value, 0, contact.Value.Length);
                if (contactMap != null)
                {
                    group.Members[contactId] = Cache.FetchUser((int)contactId);
             
                }
                else
                {
                    Console.WriteLine("Contact #{0} does not appear to have any data. Skipped contact", contactId);
                }
            }

            group.Owner = Cache.FetchUser(group.OwnerID);
            Cache.Groups[groupId] = group;
        }

        #endregion

        #region Contact Request Parser

        private static void ParseContactRequests(Dictionary<string, byte[]> maps)
        {
            foreach (var map in maps)
            {
                try
                {
                    var contactMap = Deserialize(map.Value);
                    uint contactId = uint.Parse(map.Key);
                    if (!Cache.Users.ContainsKey(contactId))
                        Cache.Users[contactId] = new Contact();
                    var contact = Cache.Users[contactId];
                    contact.ContactID = contactId;
                    contact.Nickname = Encoding.GetEncoding("windows-1252").GetString(contactMap["name"]);
                    Cache.Users[contactId] = contact;

                    var contactRequest = new ContactRequest();
                    contactRequest.ContactID = contactId;
                    contactRequest.Nickname = Encoding.GetEncoding("windows-1252").GetString(contactMap["nickname"]);
                    contactRequest.Message = Encoding.GetEncoding("windows-1252").GetString(contactMap["mesg"]);
                    Cache.ContactRequests[contactId] = contactRequest;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Unable to parse contact request. Ignoring contact");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("Unable to parse contact request. Ignoring contact");
                }
            }
        }

        #endregion

        #region Blocked Contact Parser

        private static void ParseBlockedUsers(Dictionary<string, byte[]> maps)
        {
            foreach (var map in maps)
            {
                try
                {
                    uint contactId = uint.Parse(map.Key);
                    var contact = Cache.FetchUser((int)contactId);
                    bool blocked = uint.Parse(Encoding.UTF8.GetString(map.Value)) == 1;
                    if (blocked)
                    {
                        if (Cache.BlockedContacts.ContainsKey(contactId))
                            Cache.BlockedContacts.Remove(contactId);
                    }
                    else
                    {
                        Cache.BlockedContacts[contactId] = contact;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Unable to parse blocked contact info. Ignoring contact");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("Unable to parse blocked contact info. Ignoring contact");
                }
            }
        }

        #endregion

        #region Contacts Parser

        private static void ParseContacts(Dictionary<string, byte[]> maps)
        {
            foreach (var map in maps)
            {
                uint contactId = uint.Parse(map.Key);
                if (map.Value.Length > 0)
                {
                    ParseContact(contactId, Deserialize(map.Value));
                }
            }
        }

        private static void ParseContact(uint userId, Dictionary<string, byte[]> maps)
        {
            if (!Cache.Users.ContainsKey(userId))
                Cache.Users[userId] = new Contact();
            var contact = Cache.Users[userId];
            contact.ContactID = userId;

            if (maps.ContainsKey("nickname"))
                contact.Nickname = Encoding.GetEncoding("windows-1252").GetString(maps["nickname"]);

            if (maps.ContainsKey("privileges"))
                contact.Privileges = uint.Parse(Encoding.UTF8.GetString(maps["privileges"]));

            if (maps.ContainsKey("rep"))
                contact.Reputation = int.Parse(Encoding.UTF8.GetString(maps["rep"]));

            if (maps.ContainsKey("status"))
                contact.StatusMessage = Encoding.GetEncoding("windows-1252").GetString(maps["status"]);

            if (maps.ContainsKey("rep_lvl"))
                contact.RepLvl = Encoding.GetEncoding("windows-1252").GetString(maps["rep_lvl"]);

            foreach (var map in maps)
            {
                if (map.Key == "user_data")
                {
                    contact.UserData = Deserialize(maps["user_data"]);
                }
            }

            Cache.Users[userId] = contact;
        }

        #endregion

        internal static Dictionary<string, byte[]> Deserialize(byte[] data, int start = 0)
        {
            int length = data.Length;
            var datamap = new Dictionary<string, byte[]>();
            string tempData = "";
            var maxLen = start + length;
            while (start < maxLen)
            {
                if (data[start] == 0)
                    break;
                var zeroIndex = data.IndexOf(0, start);
                var strData = Encoding.UTF8.GetString(data, start, zeroIndex - start).ToLower();
                start += strData.Length + 1;

                int prevPos = start;
                start++;
                int temp1 = data[prevPos] << 8;
                int temp2 = start;
                start = temp2 + 1;
                temp1 += data[temp2];

                if (tempData.Length <= 0 || tempData.CompareTo(strData) != 0)
                {
                    var newBytes = new byte[temp1];
                    Buffer.BlockCopy(data, start, newBytes, 0, temp1);
                    datamap[strData] = newBytes;
                }
                else
                {
                    var item = datamap[strData];
                    var newBytes = new byte[item.Length + temp1];
                    item.CopyTo(newBytes, 0);
                    Buffer.BlockCopy(data, start, newBytes, item.Length, temp1);
                    datamap[strData] = newBytes;
                }
                tempData = strData;
                start += temp1;
            }
            return datamap;
        }

        internal static Dictionary<string, byte[]> Deserialize(byte[] data, int start, int length)
        {
            var datamap = new Dictionary<string, byte[]>();
            string tempData = "";
            var maxLen = start + length;
            while (start < maxLen)
            {
                if (data[start] == 0)
                    break;
                var zeroIndex = data.IndexOf(0, start);
                var strData = Encoding.UTF8.GetString(data, start, zeroIndex - start).ToLower();
                start += strData.Length + 1;

                int prevPos = start;
                start++;
                int temp1 = data[prevPos] << 8;
                int temp2 = start;
                start = temp2 + 1;
                temp1 += data[temp2];

                if (tempData.Length <= 0 || tempData.CompareTo(strData) != 0)
                {
                    var newBytes = new byte[temp1];
                    Buffer.BlockCopy(data, start, newBytes, 0, temp1);
                    datamap[strData] = newBytes;
                }
                else
                {
                    var item = datamap[strData];
                    var newBytes = new byte[item.Length + temp1];
                    item.CopyTo(newBytes, 0);
                    Buffer.BlockCopy(data, start, newBytes, item.Length, temp1);
                    datamap[strData] = newBytes;
                }
                tempData = strData;
                start += temp1;
            }
            return datamap;
        }
    }
}
