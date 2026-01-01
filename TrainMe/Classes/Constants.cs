namespace TrainMe.Classes {
    /// <summary>
    /// Centralized constants for the application
    /// </summary>
    public static class Constants {
        /// <summary>
        /// Supported video file extensions
        /// </summary>
        public static readonly string[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv" };

        /// <summary>
        /// Maximum number of entries in the file existence cache
        /// </summary>
        public const int MaxFileCacheSize = 1000;

        /// <summary>
        /// Time to live for cache entries in minutes
        /// </summary>
        public const int CacheTtlMinutes = 5;

        /// <summary>
        /// Maximum number of retries for file operations
        /// </summary>
        public const int MaxRetryAttempts = 3;

        /// <summary>
        /// Base delay in milliseconds for retry operations
        /// </summary>
        public const int RetryBaseDelayMs = 100;

        /// <summary>
        /// Maximum file size in bytes (5GB)
        /// </summary>
        public const long MaxFileSizeBytes = 5L * 1024 * 1024 * 1024;

        /// <summary>
        /// Warning threshold for file size in bytes (1GB)
        /// </summary>
        public const long FileSizeWarningThreshold = 1024L * 1024 * 1024;
    }
}


