using System.IO;
using TrainMe.Classes;

namespace TrainMe.ViewModels {
    /// <summary>
    /// Validation status for a video file
    /// </summary>
    public enum FileValidationStatus {
        Unknown,
        Valid,
        Missing,
        Invalid
    }

    public class VideoItem : ObservableObject {
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);

        private ScreenViewer _assignedScreen;
        public ScreenViewer AssignedScreen {
            get => _assignedScreen;
            set => SetProperty(ref _assignedScreen, value);
        }

        private double _opacity = 0.9;
        public double Opacity {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        private double _volume = 0.5;
        public double Volume {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        private FileValidationStatus _validationStatus = FileValidationStatus.Unknown;
        public FileValidationStatus ValidationStatus {
            get => _validationStatus;
            set => SetProperty(ref _validationStatus, value);
        }

        private string _validationError;
        public string ValidationError {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        /// <summary>
        /// Gets whether the file is valid and exists
        /// </summary>
        public bool IsValid => ValidationStatus == FileValidationStatus.Valid;

        public VideoItem(string filePath, ScreenViewer defaultScreen = null) {
            FilePath = filePath;
            AssignedScreen = defaultScreen;
        }

        public override string ToString() {
            return FileName;
        }

        /// <summary>
        /// Validates the file and updates the validation status
        /// </summary>
        public void Validate() {
            if (string.IsNullOrWhiteSpace(FilePath)) {
                ValidationStatus = FileValidationStatus.Invalid;
                ValidationError = "File path is empty";
                return;
            }

            if (!FileValidator.ValidateVideoFile(FilePath, out string errorMessage)) {
                ValidationStatus = File.Exists(FilePath) ? FileValidationStatus.Invalid : FileValidationStatus.Missing;
                ValidationError = errorMessage;
            } else {
                ValidationStatus = FileValidationStatus.Valid;
                ValidationError = null;
            }
        }
    }
}
