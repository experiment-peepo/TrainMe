using System;
using System.IO;
using System.Linq;

namespace TrainMe.Classes {
    /// <summary>
    /// Validates file paths, extensions, sizes, and sanitizes inputs
    /// </summary>
    public static class FileValidator {
        /// <summary>
        /// Validates if a file path is valid and safe
        /// </summary>
        /// <param name="filePath">The file path to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidPath(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            try {
                // Normalize and validate path - Path.GetFullPath will:
                // 1. Resolve ".." and "." segments
                // 2. Throw on invalid paths (malformed characters, etc.)
                var fullPath = Path.GetFullPath(filePath);
                
                // Ensure path is rooted (absolute) after normalization
                if (!Path.IsPathRooted(fullPath)) return false;
                
                // After normalization, ".." should not appear in a valid path
                // If it does, the path couldn't be fully normalized and is problematic
                if (fullPath.Contains("..")) return false;
                
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Validates if a file has a supported video extension
        /// </summary>
        /// <param name="filePath">The file path to check</param>
        /// <returns>True if extension is supported, false otherwise</returns>
        public static bool HasValidExtension(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return Constants.VideoExtensions.Contains(extension);
        }

        /// <summary>
        /// Gets the file size in bytes, or null if file doesn't exist or can't be accessed
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>File size in bytes, or null if unavailable</returns>
        public static long? GetFileSize(string filePath) {
            try {
                if (!File.Exists(filePath)) return null;
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Gets file size and warning threshold (no size limit enforced)
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="sizeBytes">Output parameter for file size</param>
        /// <param name="warningThreshold">Output parameter indicating if file exceeds warning threshold</param>
        /// <returns>True if file exists and size was retrieved, false otherwise</returns>
        public static bool ValidateFileSize(string filePath, out long sizeBytes, out bool warningThreshold) {
            sizeBytes = 0;
            warningThreshold = false;
            
            var size = GetFileSize(filePath);
            if (!size.HasValue) return false;
            
            sizeBytes = size.Value;
            warningThreshold = sizeBytes > Constants.FileSizeWarningThreshold;
            
            // No size limit - always return true if file exists
            return true;
        }

        /// <summary>
        /// Sanitizes and normalizes a file path
        /// </summary>
        /// <param name="filePath">The file path to sanitize</param>
        /// <returns>Sanitized full path, or null if invalid</returns>
        public static string SanitizePath(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            
            try {
                // Path.GetFullPath normalizes the path and resolves ".." segments
                var fullPath = Path.GetFullPath(filePath);
                
                // After normalization, ".." should not appear - if it does, something went wrong
                if (fullPath.Contains("..")) return null;
                
                // Ensure path is rooted (absolute)
                if (!Path.IsPathRooted(fullPath)) return null;
                
                return fullPath;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Gets a user-friendly list of supported file extensions
        /// </summary>
        public static string GetSupportedExtensionsList() {
            return string.Join(", ", Constants.VideoExtensions.Select(ext => ext.ToUpperInvariant().TrimStart('.')));
        }

        /// <summary>
        /// Comprehensive validation of a video file
        /// </summary>
        /// <param name="filePath">The file path to validate</param>
        /// <param name="errorMessage">Output parameter for error message if validation fails</param>
        /// <returns>True if file is valid, false otherwise</returns>
        public static bool ValidateVideoFile(string filePath, out string errorMessage) {
            errorMessage = null;
            
            if (!IsValidPath(filePath)) {
                errorMessage = "Invalid file path. Please check the path and try again.";
                return false;
            }
            
            if (!HasValidExtension(filePath)) {
                var extension = Path.GetExtension(filePath)?.ToUpperInvariant() ?? "unknown";
                var supportedList = GetSupportedExtensionsList();
                errorMessage = $"File format '{extension}' is not supported. Supported formats: {supportedList}";
                return false;
            }
            
            if (!File.Exists(filePath)) {
                errorMessage = "File does not exist. The file may have been moved or deleted.";
                return false;
            }
            
            // File size validation removed - no limit enforced
            
            return true;
        }
    }
}

