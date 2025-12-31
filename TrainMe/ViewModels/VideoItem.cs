using System.IO;
using TrainMe.Classes;

namespace TrainMe.ViewModels {
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

        private double _volume = 1.0;
        public double Volume {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        public VideoItem(string filePath, ScreenViewer defaultScreen = null) {
            FilePath = filePath;
            AssignedScreen = defaultScreen;
        }

        public override string ToString() {
            return FileName;
        }
    }
}
