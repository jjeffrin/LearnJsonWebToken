using Microsoft.AspNetCore.Components.Authorization;

namespace LearnJsonWebToken.Client.Utility
{
    public static class UserState
    {
        public static bool IsAuthenticated { get; set; } = false;
        public static string? Email { get; set; }
        public static string? Uid { get; set; }
    }
}
