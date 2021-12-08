using System.Net.Http.Json;
using System.Text;
using System.Web;
using Moonlight.Api.Mojang;

namespace Moonlight.Api.Networking
{
    public static class MinecraftAPI
    {
        // TODO:
        public static readonly HttpClient HttpClient = new();

        public static async Task<List<MojangUser>?> GetUsersAsync(CancellationToken cancellationToken = default, params string[] usernames)
        {
            using HttpResponseMessage response = await HttpClient.PostAsJsonAsync("https://api.mojang.com/profiles/minecraft", usernames.Select(username => HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(username))));
            return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<List<MojangUser>>(cancellationToken: cancellationToken)) : null;
        }

        public static async Task<MojangUser?> GetUserAsync(string username, CancellationToken cancellationToken = default) => (await GetUsersAsync(cancellationToken, (string)HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(username))))?.FirstOrDefault();
        public static async Task<MojangUser?> GetUserAndSkinAsync(string uuid, CancellationToken cancellationToken = default) => await HttpClient.GetFromJsonAsync<MojangUser>($"https://sessionserver.mojang.com/session/minecraft/profile/{HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(uuid))}", cancellationToken);
        public static async Task<SessionServerResponse?> HasJoined(string username, IEnumerable<byte> serverId, CancellationToken cancellationToken = default) => await HasJoined(username, HttpUtility.UrlEncode(serverId.ToArray().MinecraftShaDigest()), cancellationToken);
        public static async Task<SessionServerResponse?> HasJoined(string username, string serverId, CancellationToken cancellationToken = default) => await HttpClient.GetFromJsonAsync<SessionServerResponse>($"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(username))}&serverId={HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(serverId))}", cancellationToken: cancellationToken);
    }
}