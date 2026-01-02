using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TrainMeX.Classes {
    /// <summary>
    /// Standard implementation of IHtmlFetcher using HttpClient
    /// </summary>
    public class StandardHtmlFetcher : IHtmlFetcher {
        private static readonly HttpClient _httpClient;

        static StandardHtmlFetcher() {
            _httpClient = new HttpClient {
                Timeout = TimeSpan.FromSeconds(Constants.HttpRequestTimeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<string> FetchHtmlAsync(string url, CancellationToken cancellationToken = default) {
            try {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                Logger.Warning($"Error fetching HTML from {url}: {ex.Message}");
                return null;
            }
        }
    }
}
