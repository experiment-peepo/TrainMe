using System.Threading;
using System.Threading.Tasks;

namespace TrainMeX.Classes {
    /// <summary>
    /// Interface for fetching HTML content from URLs
    /// </summary>
    public interface IHtmlFetcher {
        /// <summary>
        /// Fetches HTML content from the specified URL
        /// </summary>
        Task<string> FetchHtmlAsync(string url, CancellationToken cancellationToken = default);
    }
}
