namespace PalringoBotEngine
{
    public static class Enums
    {
        #region CommandType enum

        public enum CommandType
        {
            Group,
            Private
        }

        #endregion

        #region AdminAction enum

        public enum AdminAction
        {
            Admin = 1,
            Mod = 2,
            Silence = 8,
            Kick = 16,
            Ban = 4,
            Reset = 0
        }

        #endregion

        #region DeviceType enum

        public enum DeviceType
        {
            Macintosh,
            iPad,
            iPhone,
            Android,
            WindowsPC,
            WindowsPhone,
            Mobile
        }

        #endregion

        #region MessageTarget enum

        public enum MessageTarget
        {
            Group = 1,
            Private = 0,
        }

        #endregion

        #region MessageType enum

        public enum MessageType
        {
            PlainText,
            RichMessage,
            Image,
            Audio,
            VoiceMessage
        }

        #endregion

        #region OnlineStatus enum

        public enum OnlineStatus
        {
            Online = 1,
            Away = 2,
            AppearOffline = 3,
            Busy = 5,
            OffLine = 0
        }

        #endregion
    }
}
