using System;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace SteamSelector
{
    internal struct SteamMessage
    {
        internal string Message;
        internal string User;

        internal static void Create(GameConnectedFriendChatMsg_t callback)
        {
            SteamMessage msg;
            EChatEntryType chatEntryType;
            SteamFriends.GetFriendMessage(callback.m_steamIDUser, callback.m_iMessageID, out msg.Message, 2048,
                out chatEntryType);
            if (chatEntryType == EChatEntryType.k_EChatEntryTypeChatMsg)
            {
                msg.User = SteamFriends.GetFriendPersonaName(callback.m_steamIDUser);
                qkSteamSelectorService.Instance.Messages.Add(msg);
            }
        }
    }
    
    internal class SteamFriend
    {
        internal string Name { get; private set; }
        internal int Level { get; private set; }
        internal SteamAvatar Avatar { get; private set; }

        private readonly CSteamID ID;
        
        internal void HandleChange(EPersonaChange Change)
        {
            if (Change.HasFlag(EPersonaChange.k_EPersonaChangeName))
                Name = SteamFriends.GetFriendPersonaName(ID);
            if (Change.HasFlag(EPersonaChange.k_EPersonaChangeSteamLevel))
                Level = SteamFriends.GetFriendSteamLevel(ID);
            if (Change.HasFlag(EPersonaChange.k_EPersonaChangeAvatar))
            {
                int avatar_hanndle = SteamFriends.GetMediumFriendAvatar(ID);
                if(avatar_hanndle != 0)
                    Avatar = new SteamAvatar(avatar_hanndle, Name);
            }
        }
        
        internal SteamFriend(CSteamID FriendID)
        {
            ID = FriendID;
            HandleChange(EPersonaChange.k_EPersonaChangeName | EPersonaChange.k_EPersonaChangeSteamLevel |
                         EPersonaChange.k_EPersonaChangeAvatar);
        }
    }

    internal class SteamAvatar : IEquatable<SteamAvatar>
    {
        internal readonly uint Width;
        internal readonly uint Height;
        internal readonly byte[] RGBA;
        internal readonly Texture2D AvatarTexture;
        internal readonly string ColorString;
        internal readonly string Username;

        public bool Equals(SteamAvatar other)
        {
            return ColorString == other.ColorString;
        }

        public override int GetHashCode()
        {
            return ColorString.GetHashCode();
        }
        
        internal SteamAvatar(int handle, string username)
        {
            Username = username;
            SteamUtils.GetImageSize(handle, out Width, out Height);
            uint size = 4 * Height * Width;
            RGBA = new byte[size];
            SteamUtils.GetImageRGBA(handle, RGBA, (int)size);
            ColorString = String.Join("", RGBA.Select(b => b.ToString()).ToArray());
            AvatarTexture = new Texture2D((int)Width, (int)Height, TextureFormat.RGBA32, false, true);
            AvatarTexture.LoadRawTextureData(RGBA);
            AvatarTexture.Apply();
        }
    }

    internal class SteamAchievement
    {
        internal readonly string Name;
        internal readonly bool Unlocked;
        internal readonly DateTime UnlockDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        
        internal SteamAchievement(string api_name, string name)
        {
            Name = name;
            uint unlock_time;
            SteamUserStats.GetAchievementAndUnlockTime(api_name, out Unlocked, out unlock_time);
            if (Unlocked)
                UnlockDate = UnlockDate.AddSeconds(unlock_time).ToLocalTime();
        }
    }
}   