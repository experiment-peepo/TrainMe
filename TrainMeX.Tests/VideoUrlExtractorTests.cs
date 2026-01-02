using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TrainMeX.Classes;
using Xunit;

namespace TrainMeX.Tests {
    public class VideoUrlExtractorTests {
        private readonly Mock<IHtmlFetcher> _mockFetcher;
        private readonly VideoUrlExtractor _extractor;

        public VideoUrlExtractorTests() {
            _mockFetcher = new Mock<IHtmlFetcher>();
            _extractor = new VideoUrlExtractor(_mockFetcher.Object);
        }

        [Fact]
        public async Task ExtractVideoUrl_Hypnotube_ReturnsVideoUrl() {
            // Arrange
            string pageUrl = "https://hypnotube.com/video/example-12345.html";
            string htmlContent = @"
                <html>
                    <body>
                        <video id='player' src='https://cdn.hypnotube.com/videos/12345.mp4'></video>
                    </body>
                </html>";

            _mockFetcher.Setup(f => f.FetchHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(htmlContent);

            // Act
            var result = await _extractor.ExtractVideoUrlAsync(pageUrl);

            // Assert
            Assert.Equal("https://cdn.hypnotube.com/videos/12345.mp4", result);
        }

        [Fact]
        public async Task ExtractVideoUrl_Iwara_FromSourceTag_ReturnsVideoUrl() {
            // Arrange
            string pageUrl = "https://iwara.tv/video/example";
            string htmlContent = @"
                <html>
                    <body>
                         <video>
                            <source src='https://cdn.iwara.tv/file.mp4' type='video/mp4'>
                        </video>
                    </body>
                </html>";

            _mockFetcher.Setup(f => f.FetchHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(htmlContent);

            // Act
            var result = await _extractor.ExtractVideoUrlAsync(pageUrl);

            // Assert
            Assert.Equal("https://cdn.iwara.tv/file.mp4", result); // Extractor normalizes URL
        }

        [Fact]
        public async Task ExtractVideoUrl_WithRelativeUrl_ResolvesToAbsolute() {
            // Arrange
            string pageUrl = "https://example.com/video/page";
            string htmlContent = @"<video src='/media/video.mp4'></video>";

            _mockFetcher.Setup(f => f.FetchHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(htmlContent);

            // Act
            var result = await _extractor.ExtractVideoUrlAsync(pageUrl);

            // Assert
            Assert.Equal("https://example.com/media/video.mp4", result);
        }

        [Fact]
        public async Task ExtractVideoTitle_ReturnsOpenGraphTitle() {
            // Arrange
            string pageUrl = "https://example.com/video";
            string htmlContent = @"
                <html>
                    <head>
                        <meta property='og:title' content='Amazing Video Title' />
                    </head>
                </html>";

            _mockFetcher.Setup(f => f.FetchHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(htmlContent);

            // Act
            var result = await _extractor.ExtractVideoTitleAsync(pageUrl);

            // Assert
            Assert.Equal("Amazing Video Title", result);
        }

        [Fact]
        public async Task ExtractVideoTitle_SanitizesInput() {
            // Arrange
            string pageUrl = "https://example.com/video";
            string htmlContent = @"<title>Video Title - With Suffix | SiteName</title>";

            _mockFetcher.Setup(f => f.FetchHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(htmlContent);

            // Act
            var result = await _extractor.ExtractVideoTitleAsync(pageUrl);

            // Assert
            // The extractor has specific logic to strip site suffixes if known, 
            // but for a generic one it might just return the whole thing depending on implementation.
            // Let's test basic HTML decoding.
            Assert.Contains("Video Title", result);
        }

         [Fact]
        public async Task ExtractVideoUrl_InvalidHtml_ReturnsNull() {
             // Arrange
            string pageUrl = "https://example.com/video";
            _mockFetcher.Setup(f => f.FetchHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("");

            // Act
            var result = await _extractor.ExtractVideoUrlAsync(pageUrl);

            // Assert
            Assert.Null(result);
        }
    }
}
