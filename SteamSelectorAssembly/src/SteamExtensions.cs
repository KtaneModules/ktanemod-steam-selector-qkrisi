using Steamworks;

namespace SteamSelector
{
    public static class SteamExtensions
    {
        public static bool HasFlag(this EPersonaChange change, EPersonaChange value)
        {
            return (change & value) == value;
        }
    }
}