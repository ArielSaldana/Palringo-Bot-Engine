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
using System.Runtime.InteropServices;
using System.Text;

namespace PalringoBotEngine
{
    public static class PacketTemplates
    {
        public static int LastMessageID = 358435;

        #region Login Stuff

        public static Packet Auth(byte[] password, Enums.OnlineStatus onlineStatus)
        {
            return new Packet("AUTH")
            {
                Headers = new Dictionary<string, string>
						{
							{"encryption-type", "1"},
							{"online-status", ( (int) onlineStatus ).ToString ()},
						},
                Payload = Encoding.GetEncoding("windows-1252").GetString(password)
            };
        }

        public static Packet Logon(string email, Enums.DeviceType deviceType)
        {
            var packet = new Packet("LOGON");
            packet.Headers.Add("protocol-version", "2.0");
            switch (deviceType)
            {
                case Enums.DeviceType.Android:
                    packet.Headers.Add("app-type", "android");
                    packet.Headers.Add("Operator", "Verizon");
                    break;

                case Enums.DeviceType.iPad:
                    packet.Headers.Add("app-type", "Apple/iPad/Premium");
                    packet.Headers.Add("Operator", "");
                    break;

                case Enums.DeviceType.iPhone:
                    packet.Headers.Add("app-type", "Apple/iPhone/Premium");
                    packet.Headers.Add("Operator", "");
                    break;

                case Enums.DeviceType.Macintosh:
                    packet.Headers.Add("app-type", "Apple/Intel");
                    packet.Headers.Add("client-version", "4.6.4 (null)");
                    packet.Headers.Add("Operator", "OSX_CLIENT");
                    break;

                case Enums.DeviceType.WindowsPC:
                    packet.Headers.Add("app-type", "Windows x86");
                    packet.Headers.Add("Operator", "PC_Client");
                    break;

                case Enums.DeviceType.WindowsPhone:
                    packet.Headers.Add("app-type", "Win/P7");
                    packet.Headers.Add("Operator", "PHONE_7");
                    break;

                case Enums.DeviceType.Mobile:
                    packet.Headers.Add("app-type", "Java");
                    packet.Headers.Add("Operator", "Verizon");
                    break;
            }
            packet.Headers.Add("name", email);
            packet.Headers.Add("capabilities", "4");
            return packet;
        }

        public static Packet PING()
        {
            return new Packet("P");
        }

        #endregion Login Stuff

        #region Admin Actions

        private static Packet AdminAction(Enums.AdminAction action, int groupID, int targetID)
        {
            var packet = new Packet("GROUP ADMIN");
            packet.Headers.Add("group-id", groupID.ToString());
            packet.Headers.Add("target-id", targetID.ToString());
            packet.Headers.Add("last", "1");
            LastMessageID++;
            packet.Headers.Add("mesg-id", LastMessageID.ToString());
            packet.Headers.Add("action", ((int)action).ToString());
            return packet;
        }

        public static Packet AdminUser(int GroupID, int TargetID)
        {
            return AdminAction(Enums.AdminAction.Admin, GroupID, TargetID);
        }

        public static Packet ModUser(int GroupID, int TargetID)
        {
            return AdminAction(Enums.AdminAction.Mod, GroupID, TargetID);
        }

        public static Packet SilenceUser(int GroupID, int TargetID)
        {
            return AdminAction(Enums.AdminAction.Silence, GroupID, TargetID);
        }

        public static Packet KickUser(int GroupID, int TargetID)
        {
            return AdminAction(Enums.AdminAction.Kick, GroupID, TargetID);
        }

        public static Packet BanUser(int GroupID, int TargetID)
        {
            return AdminAction(Enums.AdminAction.Ban, GroupID, TargetID);
        }

        public static Packet ResetUser(int GroupID, int TargetID)
        {
            return AdminAction(Enums.AdminAction.Reset, GroupID, TargetID);
        }

        #endregion Admin Actions

        #region Group Stuff

        public static Packet JoinGroup(string groupName, [Optional] string password)
        {
            LastMessageID++;
            return new Packet("GROUP SUBSCRIBE")
            {
                Headers = new Dictionary<string, string>
						{
							{"mesg-id", LastMessageID.ToString ()},
							{"name", groupName}
						},
                Payload = password
            };
        }

        public static Packet LeaveGroup(int groupID)
        {
            LastMessageID++;
            return new Packet("GROUP UNSUB")
            {
                Headers = new Dictionary<string, string>
						{
							{"mesg-id", LastMessageID.ToString ()},
							{"group-id", groupID.ToString ()}
						}
            };
        }

        public static Packet CreateGroup(string groupName, [Optional] string description, [Optional] string password)
        {
            LastMessageID++;
            return new Packet("GROUP CREATE")
            {
                Headers = new Dictionary<string, string>
						{
							{"mesg-id", LastMessageID.ToString ()},
							{"name", groupName},
							{"Desc", description}
						},
                Payload = password
            };
        }

        #endregion Group Stuff

        #region Contacts Stuff

        public static Packet AddContact(int targetID, [Optional] string contactAddMessage)
        {
            return new Packet("CONTACT ADD")
            {
                Headers = new Dictionary<string, string>
						{
							{"last", "1"},
							{"mesg-id", "33661"},
							{"target-id", targetID.ToString ()},
						},
                Payload = contactAddMessage
            };
        }

        public static Packet AddContactName(string target, [Optional] string contactAddMessage)
        {
            return new Packet("CONTACT ADD")
            {
                Headers = new Dictionary<string, string>
						{
							{"last", "1"},
							{"mesg-id", "33661"},
                            {"name",target},
						},
                Payload = contactAddMessage
            };
        }

        public static Packet AcceptContact(int targetID)
        {
            return new Packet("CONTACT ADD RESP")
            {
                Headers = new Dictionary<string, string>
						{
							{"accepted", "1"},
							{"last", "1"},
							{"mesg-id", "32831"},
							{"source-id", targetID.ToString ()}
						}
            };
        }

        public static Packet DeleteContact(int targetID)
        {
            return new Packet("CONTACT UPDATE")
            {
                Headers = new Dictionary<string, string>
						{
							{"contact-id", targetID.ToString ()},
							{"remove", true.ToString ()}
						}
            };
        }

        #endregion Contacts Stuff

        #region User Profile Stuff

        public static Packet ChangeOnlineStatus(Enums.OnlineStatus onlineStatus)
        {
            return new Packet("CONTACT DETAIL")
            {
                Headers = new Dictionary<string, string>
						{
							{"last", "1"},
							{"mesg-id", "33274"},
							{"online-status", ( (int) onlineStatus ).ToString ()}
						}
            };
        }

        public static Packet UpdateNameStatus(string nickname, string status)
        {
            return new Packet("CONTACT DETAIL")
            {
                Headers = new Dictionary<string, string>
						{
							{"last", "1"},
							{"mesg-id", "33146"},
							{"nickname", nickname},
							{"status", status}
						}
            };
        }

        public static Packet UpdateNameStatus(string nickname)
        {
            return UpdateNameStatus(nickname, "");
        }

        /*public static Packet[] ChangeProfilePicture(byte[] image)
        {
            return IconPacket.Package(image);
        }

        public static Packet RequestProfile(int contactID)
        {
            var packet = new Packet("SUB PROFILE QUERY");
            packet.AddHeader("mesg-id", "33925");
            var dataMaps = new DataMap();
            dataMaps.SetValue("Sub-Id", contactID);
            packet.Payload = Encoding.GetEncoding("windows-1252").GetString(dataMaps.Serialize());
            return packet;
        }

        public static Packet RequestProfile(int contactID, int targetGroup)
        {
            var req = new Profiles { ContactID = contactID, GroupID = targetGroup };
            if (!Static.RequestedProfiles.Contains(req))
                Static.RequestedProfiles.Add(req);
            return RequestProfile(contactID);
        }*/

        #endregion User Profile Stuff

        #region Messages

        private static Packet MessagePacket(Enums.MessageTarget Target, int TargetID, string Message)
        {
            return new Packet("MESG")
            {
                Headers = new Dictionary<string, string>
						{
							{"content-type", "text/plain"},
                            {"total-length", Message.Length.ToString()},
							{"last", "T"},
							{"mesg-id", "1"},
							{"mesg-target", ( (int) Target ).ToString ()},
							{"target-id", TargetID.ToString ()}
						},
                Payload = Message,
            };
        }

        public static Packet GroupMessage(int targetID, string message)
        {
            return MessagePacket(Enums.MessageTarget.Group, targetID, message);
        }

        public static Packet PrivateMessage(int targetID, string message)
        {
            return MessagePacket(Enums.MessageTarget.Private, targetID, message);
        }

        /*public static Packet[] BigGroupMessage(int TargetID, string Message)
        {
            //return BigMessagePacket.Package(TargetID, Enums.MessageTarget.Group, Message);
            return Solid.BigMessage.Packet(TargetID, Enums.MessageTarget.Group, Message);
        }*/

        public static Packet[] GroupImage(int TargetID, byte[] image)
        {
            return ImagePacket.Package(TargetID, Enums.MessageTarget.Group, image);
        }

        public static Packet[] PrivateImage(int TargetID, byte[] image)
        {
            return ImagePacket.Package(TargetID, Enums.MessageTarget.Private, image);
        }

        #endregion Messages

        #region Misc

        /*public static Packet Create(DataMap map, object target, int sourceId, bool isName)
        {
            try
            {
                if (isName)
                {
                    Static.RequestedGroups.Add(new RequestGroup
                    {
                        Name = (string)target,
                        SourceId = sourceId,
                        IsName = true
                    });
                }
                else
                {
                    Static.RequestedGroups.Add(new RequestGroup
                    {
                        TargetId = (int)target,
                        SourceId = sourceId,
                        IsName = false
                    });
                }
            }
            catch (Exception)
            {
            }
            return new Packet("PROVIF QUERY")
            {
                Headers = { { "mesg-id", "34163" }, { "last", "1" } },
                Payload = Encoding.GetEncoding("windows-1252").GetString(map.Serialize())
            };
        }

        public static Packet Create(DataMap map, object target, int sourceId)
        {
            bool isName = false;
            try
            {
                if (isName)
                {
                    Static.RequestedGroups.Add(new RequestGroup
                    {
                        Name = (string)target,
                        SourceId = sourceId,
                        IsName = true
                    });
                }
                else
                {
                    Static.RequestedGroups.Add(new RequestGroup
                    {
                        TargetId = (int)target,
                        SourceId = sourceId,
                        IsName = false
                    });
                }
            }
            catch (Exception)
            {
            }
            return new Packet("PROVIF QUERY")
            {
                Headers = { { "mesg-id", "34163" }, { "last", "1" } },
                Payload = Encoding.GetEncoding("windows-1252").GetString(map.Serialize())
            };
        }

        public static Packet QueryGroup(string name, int targetGroup)
        {
            var search = new GroupSearch(1, true);
            search.AddGroupNameFilter(name, true);
            var datamap = search.ConstructDataMap();
            return Create(datamap, name, targetGroup, true);
        }

        public static Packet QueryGroup(int id, int targetGroup)
        {
            var search = new GroupSearch(1, true);
            search.AddGroupIDCriteria((ulong)id);
            var datamap = search.ConstructDataMap();
            return Create(datamap, id, targetGroup);
        }

        public static Packet QueryGroup(int id)
        {
            return QueryGroup(id, -1);
        }

        public static Packet QueryGroup(string name)
        {
            return QueryGroup(name, -1);
        }*/

        #endregion


        #region Queries

        public static Packet FetchUserAvatar(int userId)
        {
            var packet = new Packet("AVATAR");
            packet.AddHeader("id", userId.ToString());
            packet.AddHeader("MESG-id", LastMessageID++.ToString());
            packet.AddHeader("size", "2000");
            return packet;
        }

        public static Packet FetchGroupAvatar(int groupId)
        {
            var packet = new Packet("AVATAR");
            packet.AddHeader("group", "T");
            packet.AddHeader("id", groupId.ToString());
            packet.AddHeader("MESG-id", LastMessageID++.ToString());
            packet.AddHeader("size", "2000");
            return packet;
        }

        #endregion

        public static Packet[] ChunkGroupMessage(int groupId, string message)
        {
            return PalringoBotEngine.MessagePacket.Package(groupId, Enums.MessageTarget.Group, message);
        }
    }
}
