using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using TrainMe.Classes;

namespace TrainMe.ViewModels {
    public class HypnoViewModel : ObservableObject {
        private VideoItem[] _files;
        private int _currentPos = 0;
        
        private Uri _currentSource;
        public Uri CurrentSource {
            get => _currentSource;
            set {
                SetProperty(ref _currentSource, value);
            }
        }

        private double _opacity;
        public double Opacity {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        private double _volume;
        public double Volume {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        private MediaState _mediaState = MediaState.Manual;
        public MediaState MediaState {
            get => _mediaState;
            set => SetProperty(ref _mediaState, value);
        }
        
        public event EventHandler RequestPlay;
        public event EventHandler RequestPause;
        public event EventHandler RequestStop;

        public HypnoViewModel() {
        }

        public void SetQueue(IEnumerable<VideoItem> files) {
            _files = files?.ToArray() ?? new VideoItem[0];
            _currentPos = -1;
            PlayNext();
        }

        private VideoItem _currentItem;

        public void PlayNext() {
            if (_files == null || _files.Length == 0) return;

            if (_currentPos + 1 < _files.Length) {
                _currentPos++;
            } else {
                _currentPos = 0; // Loop
            }

            LoadCurrentVideo();
        }

        private void LoadCurrentVideo() {
            if (_currentItem != null) {
                _currentItem.PropertyChanged -= CurrentItem_PropertyChanged;
            }

            if (_files == null || _files.Length == 0 || _currentPos < 0 || _currentPos >= _files.Length) return;

            _currentItem = _files[_currentPos];
            _currentItem.PropertyChanged += CurrentItem_PropertyChanged;
            
            var path = _currentItem.FilePath;
            
            if (!System.IO.Path.IsPathRooted(path)) {
                return;
            }
            
            // Apply per-monitor/per-item settings
            Opacity = _currentItem.Opacity;
            Volume = _currentItem.Volume;
            
            CurrentSource = new Uri(path, UriKind.Absolute);
            RequestPlay?.Invoke(this, EventArgs.Empty);
        }

        private void CurrentItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (_currentItem == null) return;
            if (e.PropertyName == nameof(VideoItem.Opacity)) {
                Opacity = _currentItem.Opacity;
            } else if (e.PropertyName == nameof(VideoItem.Volume)) {
                Volume = _currentItem.Volume;
            }
        }

        public void OnMediaEnded() {
            PlayNext();
        }

        public void OnMediaFailed(Exception ex) {
            // Log or show error?
            // For now, just skip to next to avoid getting stuck?
             PlayNext();
        }

        public void Play() {
            RequestPlay?.Invoke(this, EventArgs.Empty);
        }

        public void Pause() {
            RequestPause?.Invoke(this, EventArgs.Empty);
        }

        public void Stop() {
            RequestStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
