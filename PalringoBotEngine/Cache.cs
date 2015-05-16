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
using System.Net;
using System.Text;

namespace PalringoBotEngine
{
    public class Contact
	{
        private byte[] _avatar = null;

		public uint ContactID;
		public string Nickname;
		public uint Privileges;
		public int Reputation;
		public string StatusMessage;
        public string RepLvl;
        public byte[] Avatar
        {
            get
            {
                if (_avatar == null)
                {
                    _avatar = new WebClient().DownloadData("http://www.palringo.com/showavatar.php?id=" + ContactID + "&size=9999");
                    return _avatar;
                }
                return _avatar;
            }
        }
		public Dictionary<string, byte[]> UserData;

        public void DownloadAvatarAsync()
        {
            var webClient = new WebClient();
            webClient.DownloadDataCompleted += (o, e) =>
                {
                    try
                    {
                        _avatar = e.Result;
                    }
                    catch { }
                };
            webClient.DownloadDataAsync(new Uri("http://www.palringo.com/showavatar.php?id=" + ContactID + "&size=9999"));
        }
	}

    public class Group
	{
		#region Attributes enum

		public enum Attributes
		{
			None,
			LongDesc,
			Locked,
			Discoverability,
			Consent,
			LangID,
			PayTier,
			PayPeriod,
			Avatar,
			Latitude,
			Longitude,
			Premium,
			Permanent,
			Adult,
			Weight,
			Tag1,
			Tag2,
			Tag3,
			Tag4,
			Tag5,
			Category1,
			Category2,
			Category3,
			Category4,
			Category5
		}

		#endregion

		#region GroupCategory enum

		public enum GroupCategory
		{
			Undefined = 0,
			None = 1,
			GroupCategoryBusiness = 8,
			GroupCategoryEducation = 10,
			GroupCategoryGaming = 12,
			GroupCategoryLifestyle = 13,
			GroupCategoryMusic = 14,
			GroupCategoryNewsAndPolitics = 15,
			GroupCategoryPhotography = 16,
			GroupCategorySocialAndPeople = 17,
			GroupCategoryTravel = 18,
			GroupCategorySports = 19,
			GroupCategoryScienceAndTech = 25,
			GroupCategoryEntertainment = 26
		}

		#endregion

        public Dictionary<uint, Contact> Members = new Dictionary<uint, Contact>();
		private int _memberCount;

		public string Name { get; set; }

		public string Description { get; set; }

		public int OwnerID { get; set; }

		public Contact Owner { get; set; }

		public uint GroupID { get; set; }

		public bool Premium { get; set; }

		public bool Mature { get; set; }

		public bool Adult { get; set; }

		public string LongDescription { get; set; }

		public bool Permanent { get; set; }

		public float Latitude { get; set; }

		public float Longitude { get; set; }

		public int MemberCount
		{
			get
			{
				if ( this.Members.Count != 0 )
					return this.Members.Count;
				return this._memberCount;
			}
			set { this._memberCount = value; }
		}

		public override string ToString ()
		{
			var build = new StringBuilder ();
			build.AppendLine ( "Name: " + this.Name );
			build.AppendLine ( "Group ID: " + this.GroupID );
			try
			{
				build.AppendLine ( "Adult: " + ( this.Adult ? "Yes" : "No" ) );
			}
			catch ( System.Exception )
			{
			}
			try
			{
				build.AppendLine ( "Mature: " + ( this.Mature ? "Yes" : "No" ) );
			}
			catch ( System.Exception )
			{
			}
			try
			{
				build.AppendLine ( "Owner Name: " + Owner.Nickname );
			}
			catch ( System.Exception )
			{
			}
			try
			{
				build.AppendLine ( "Owner ID: " + this.OwnerID );
			}
			catch ( System.Exception )
			{
			}
			try
			{
				build.AppendLine ( "Numbers of members: " + this.MemberCount );
			}
			catch ( System.Exception )
			{
			}
			return build.ToString ().Trim ();
		}
    }

    public class ContactRequest
    {
        public uint ContactID;
        public string Nickname = "";
        public string Message = "";
    }

    public static class Cache
    {
        public static Dictionary<uint, Contact> Users = new Dictionary<uint, Contact>();
        public static Dictionary<uint, Group> Groups = new Dictionary<uint, Group>();
        public static Contact Profile = new Contact();
        public static Dictionary<uint, ContactRequest> ContactRequests = new Dictionary<uint, ContactRequest>();
        public static Dictionary<uint, Contact> BlockedContacts = new Dictionary<uint, Contact>();

        public static Contact FetchUser(int userId)
        {
            return Users[(uint)userId] ?? null;
        }

        public static Group FetchGroup(int groupId)
        {
            return Groups[(uint)groupId] ?? null;
        }
    }
}
