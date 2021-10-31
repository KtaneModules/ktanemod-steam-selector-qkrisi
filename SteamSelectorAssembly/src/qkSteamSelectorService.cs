using System.Collections.Generic;
using Steamworks;
using SteamSelector;
using UnityEngine;

public class qkSteamSelectorService : MonoBehaviour
{
    internal static qkSteamSelectorService Instance;
    
    internal List<SteamMessage> Messages = new List<SteamMessage>();
    internal List<SteamAchievement> Achievements = new List<SteamAchievement>();
    internal Dictionary<ulong, SteamFriend> Friends = new Dictionary<ulong, SteamFriend>();

    void Update()
    {
        if(initialized)
            SteamAPI.RunCallbacks();
    }
    
    void Awake()
    {
        Instance = this;
        initialized = SteamAPI.Init();
        ModConfigHelper.ReadConfig<qkSteamSelector.SteamSelectorSettings>(qkSteamSelector.SettingsFile);        //Create settings
    }

    internal bool initialized;

    private const EFriendFlags FriendFlags = EFriendFlags.k_EFriendFlagImmediate;
    private readonly Dictionary<string, string> AchievementNames = new Dictionary<string, string>()
    {
        { "firstbomb", "Action Hero" },
        { "allthebasics", "Bomb Defusing 101" },
        { "allmoderate", "All in Moderation" },
        { "allneedy", "Multitasker" },
        { "allchallenging", "Challenge Accepted" },
        { "allextreme", "To the Extreme!" },
        { "allexotic", "Seasoned Traveller" },
        { "disarmedeachmodule", "Experience is the Best Teacher" },
        { "100modulesdisarmed", "Trust the Expert" },
        { "100defused", "Bomb Squad" }
    };
    
    #region Callbacks
    #pragma warning disable 414
    
    private Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFriendChatMsg;
    private Callback<PersonaStateChange_t> m_PersonaStateChange;
    private Callback<UserStatsReceived_t> m_UserStatsReceived;

    #pragma warning restore 414
    #endregion

    private void UpdateFriends()
    {
        int friends = SteamFriends.GetFriendCount(FriendFlags);
        for (int i = 0; i < friends; i++)
        {
            var ID = SteamFriends.GetFriendByIndex(i, FriendFlags);
            if(!Friends.ContainsKey(ID.m_SteamID))
                Friends.Add(ID.m_SteamID, new SteamFriend(ID));
        }
    }
    
    private void OnEnable()
    {
        if(!initialized)
            return;
        UpdateFriends();
        m_GameConnectedFriendChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(SteamMessage.Create);
        m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(callback =>
        {
            ulong uID = callback.m_ulSteamID;
            var ID = new CSteamID(uID);
            var relationship = SteamFriends.GetFriendRelationship(ID);
            if (relationship != EFriendRelationship.k_EFriendRelationshipFriend)
            {
                if (Friends.ContainsKey(uID))
                    Friends.Remove(uID);
            }
            else
            {
                if (!Friends.ContainsKey(uID))
                    Friends.Add(uID, new SteamFriend(ID));
                else Friends[uID].HandleChange(callback.m_nChangeFlags);
            }
        });
        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(cb =>
        {
            if (cb.m_eResult == EResult.k_EResultOK)
            {
                foreach(var pair in AchievementNames)
                    Achievements.Add(new SteamAchievement(pair.Key, pair.Value));
            }
        });
        SteamFriends.SetListenForFriendsMessages(true);
        SteamUserStats.RequestCurrentStats();
    }
        
    private void OnApplicationQuit()
    {
        SteamAPI.Shutdown();
    }
}