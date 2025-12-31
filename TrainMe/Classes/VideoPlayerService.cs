using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TrainMe.Windows;
using TrainMe.ViewModels;

namespace TrainMe.Classes {
    public class VideoPlayerService {
        readonly List<HypnoWindow> players = new List<HypnoWindow>();

        public bool IsPlaying => players.Count > 0;

        public void PlayOnScreens(IEnumerable<VideoItem> files, IEnumerable<ScreenViewer> screens) {
            StopAll();
            var queue = NormalizeItems(files).ToArray();
            foreach (var sv in screens ?? Enumerable.Empty<ScreenViewer>()) {
                var w = new HypnoWindow(sv.Screen);
                w.Show();
                
                w.ViewModel.SetQueue(queue); 
                
                players.Add(w);
            }
        }

        public void PauseAll() {
            foreach (var w in players) w.ViewModel.Pause();
        }

        public void ContinueAll() {
            foreach (var w in players) w.ViewModel.Play();
        }

        public void StopAll() {
            foreach (var w in players) w.Close();
            players.Clear();
        }

        public void SetVolumeAll(double volume) {
            foreach (var w in players) w.ViewModel.Volume = volume;
        }

        public void SetOpacityAll(double opacity) {
            foreach (var w in players) w.ViewModel.Opacity = opacity;
        }

        public void PlayPerMonitor(IDictionary<ScreenViewer, IEnumerable<VideoItem>> assignments) {
            StopAll();
            if (assignments == null) return;
            foreach (var kvp in assignments) {
                var sv = kvp.Key;
                var queue = NormalizeItems(kvp.Value).ToArray();
                if (queue.Length == 0) continue;
                
                var w = new HypnoWindow(sv.Screen);
                w.Show();
                
                w.ViewModel.SetQueue(queue);

                players.Add(w);
            }
        }

        IEnumerable<VideoItem> NormalizeItems(IEnumerable<VideoItem> files) {
            var list = new List<VideoItem>();
            foreach (var f in files ?? Enumerable.Empty<VideoItem>()) {
                if (Path.IsPathRooted(f.FilePath)) {
                    if (File.Exists(f.FilePath)) list.Add(f);
                }
            }
            return list;
        }
    }
}
