using System.Collections.Generic;

namespace TrainMe.Classes {
    public class Playlist {
        public List<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
    }

    public class PlaylistItem {
        public string FilePath { get; set; }
        public string ScreenDeviceName { get; set; }
        public double Opacity { get; set; } = 0.9;
        public double Volume { get; set; } = 1.0;
    }
}
