using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TrainMe.Classes;
using Microsoft.Win32;

namespace TrainMe.ViewModels {
    public class LauncherViewModel : ObservableObject {
        public ObservableCollection<VideoItem> AddedFiles { get; } = new ObservableCollection<VideoItem>();
        public ObservableCollection<ScreenViewer> AvailableScreens { get; } = new ObservableCollection<ScreenViewer>();
        
        private Random random = new Random();

        private bool _shuffle;
        public bool Shuffle {
            get => _shuffle;
            set => SetProperty(ref _shuffle, value);
        }

        private string _hypnotizeButtonText = "TRAIN ME!";
        public string HypnotizeButtonText {
            get => _hypnotizeButtonText;
            set => SetProperty(ref _hypnotizeButtonText, value);
        }

        private bool _isHypnotizeEnabled;
        public bool IsHypnotizeEnabled {
            get => _isHypnotizeEnabled;
            set => SetProperty(ref _isHypnotizeEnabled, value);
        }

        private bool _isDehypnotizeEnabled;
        public bool IsDehypnotizeEnabled {
            get => _isDehypnotizeEnabled;
            set => SetProperty(ref _isDehypnotizeEnabled, value);
        }

        private bool _isPauseEnabled;
        public bool IsPauseEnabled {
            get => _isPauseEnabled;
            set => SetProperty(ref _isPauseEnabled, value);
        }

        private string _pauseButtonText = "Pause";
        public string PauseButtonText {
            get => _pauseButtonText;
            set => SetProperty(ref _pauseButtonText, value);
        }

        private bool _pauseClicked;

        public ICommand HypnotizeCommand { get; }
        public ICommand DehypnotizeCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand MinimizeCommand { get; }

        private System.Windows.Threading.DispatcherTimer _saveTimer;

        public LauncherViewModel() {
            RefreshScreens();

            HypnotizeCommand = new RelayCommand(Hypnotize, _ => IsHypnotizeEnabled);
            DehypnotizeCommand = new RelayCommand(Dehypnotize);
            PauseCommand = new RelayCommand(Pause);
            BrowseCommand = new RelayCommand(Browse);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            ClearAllCommand = new RelayCommand(ClearAll);
            SavePlaylistCommand = new RelayCommand(SavePlaylist);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist);
            ExitCommand = new RelayCommand(Exit);
            MinimizeCommand = new RelayCommand(Minimize);

            UpdateButtons();
            LoadSession();
            
            _saveTimer = new System.Windows.Threading.DispatcherTimer();
            _saveTimer.Interval = TimeSpan.FromMilliseconds(500);
            _saveTimer.Tick += (s, e) => {
                _saveTimer.Stop();
                SaveSession();
            };
            
            AddedFiles.CollectionChanged += (s, e) => {
                if (e.NewItems != null) {
                    foreach (VideoItem item in e.NewItems) item.PropertyChanged += VideoItem_PropertyChanged;
                }
                if (e.OldItems != null) {
                    foreach (VideoItem item in e.OldItems) item.PropertyChanged -= VideoItem_PropertyChanged;
                }
            };
            foreach (var item in AddedFiles) item.PropertyChanged += VideoItem_PropertyChanged;
        }

        private void VideoItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(VideoItem.Opacity) || e.PropertyName == nameof(VideoItem.Volume) || e.PropertyName == nameof(VideoItem.AssignedScreen)) {
                TriggerDebouncedSave();
            }
        }

        private void TriggerDebouncedSave() {
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        public ICommand RemoveItemCommand { get; }
        public ICommand SavePlaylistCommand { get; }
        public ICommand LoadPlaylistCommand { get; }

        private void RemoveItem(object parameter) {
            if (parameter is VideoItem item) {
                AddedFiles.Remove(item);
                UpdateButtons();
                SaveSession();
            }
        }

        private void RefreshScreens() {
            AvailableScreens.Clear();
            foreach (var s in WindowServices.GetAllScreenViewers()) {
                AvailableScreens.Add(s);
            }
        }

        private void UpdateButtons() {
            bool hasFiles = AddedFiles.Count > 0;
            bool allAssigned = AllFilesAssigned();
            IsHypnotizeEnabled = hasFiles && allAssigned;
        }

        private bool AllFilesAssigned() {
            foreach (var f in AddedFiles) {
                if (f.AssignedScreen == null) return false;
            }
            return true;
        }

        private void Hypnotize(object parameter) {
            var selectedItems = parameter as System.Collections.IList;
            var assignments = BuildAssignmentsFromSelection(selectedItems);
            if (assignments == null || assignments.Count == 0) return;
            
            App.VideoService.PlayPerMonitor(assignments);
            IsDehypnotizeEnabled = true;
            IsPauseEnabled = true;
        }

        private Dictionary<ScreenViewer, IEnumerable<VideoItem>> BuildAssignmentsFromSelection(System.Collections.IList selectedItems) {
            var selectedFiles = new List<VideoItem>();
            if (selectedItems != null && selectedItems.Count > 0) {
                foreach (VideoItem f in selectedItems) selectedFiles.Add(f);
            } else {
                foreach (var f in AddedFiles) selectedFiles.Add(f);
            }

            if (selectedFiles.Count < 1) return null;

            // Simple validation again just in case
            if (selectedFiles.Any(x => x.AssignedScreen == null)) return null;

            if (Shuffle) selectedFiles = selectedFiles.OrderBy(a => random.Next()).ToList();

            var assignments = new Dictionary<ScreenViewer, IEnumerable<VideoItem>>();
            foreach (var f in selectedFiles) {
                var assigned = f.AssignedScreen;
                if (assigned == null) continue;
                if (!assignments.ContainsKey(assigned)) assignments[assigned] = new List<VideoItem>();
                ((List<VideoItem>)assignments[assigned]).Add(f);
            }
            return assignments;
        }

        private void Dehypnotize(object obj) {
            IsDehypnotizeEnabled = false;
            IsPauseEnabled = false;
            App.VideoService.StopAll();
        }

        private void Pause(object obj) {
            if (_pauseClicked) {
                _pauseClicked = false;
                PauseButtonText = "Pause";
                App.VideoService.ContinueAll();
            } else {
                _pauseClicked = true;
                PauseButtonText = "Continue";
                App.VideoService.PauseAll();
            }
        }

        private void Browse(object obj) {
            var dlg = new OpenFileDialog {
                Multiselect = true,
                Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv|All Files|*.*"
            };
            if (dlg.ShowDialog() == true) {
                // Ensure screens are up to date
                if (AvailableScreens.Count == 0) RefreshScreens();
                var primary = AvailableScreens.FirstOrDefault(v => v.Screen.Primary) ?? AvailableScreens.FirstOrDefault();
                
                foreach (var f in dlg.FileNames) {
                    if (!AddedFiles.Any(x => x.FilePath == f)) {
                        var item = new VideoItem(f, primary);
                        item.Opacity = 0.9;
                        item.Volume = 1.0;
                        AddedFiles.Add(item);
                    }
                }
                UpdateButtons();
                SaveSession();
            }
        }
        
        private void RemoveSelected(object parameter) {
            var selectedItems = parameter as System.Collections.IList;
            if (selectedItems == null) return;
            
            var toRemove = new List<VideoItem>();
            foreach (VideoItem f in selectedItems) toRemove.Add(f);
            foreach (var f in toRemove) {
                AddedFiles.Remove(f);
            }
            UpdateButtons();
            SaveSession();
        }

        private void ClearAll(object obj) {
            AddedFiles.Clear();
            UpdateButtons();
            SaveSession();
        }

        private void Exit(object obj) {
            if (MessageBox.Show("Exit program? All hypnosis will be terminated :(", "Exit program", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                SaveSession();
                Application.Current.Shutdown();
            }
        }

        private void Minimize(object obj) {
            if (obj is Window w) w.WindowState = WindowState.Minimized;
        }

        // Method to handle Drag & Drop from View
        public void AddDroppedFiles(string[] files) {
             if (AvailableScreens.Count == 0) RefreshScreens();
             var primary = AvailableScreens.FirstOrDefault(v => v.Screen.Primary) ?? AvailableScreens.FirstOrDefault();

             foreach (var f in files) {
                 var ext = System.IO.Path.GetExtension(f)?.ToLowerInvariant();
                 if (ext == ".mp4" || ext == ".mkv" || ext == ".avi" || ext == ".mov" || ext == ".wmv") {
                     if (!AddedFiles.Any(x => x.FilePath == f)) {
                         var item = new VideoItem(f, primary);
                         item.Opacity = 0.9;
                         item.Volume = 1.0;
                         AddedFiles.Add(item);
                     }
                 }
             }
             UpdateButtons();
             SaveSession();
        }

        public void MoveVideoItem(VideoItem item, int newIndex) {
            if (item == null) return;
            var oldIndex = AddedFiles.IndexOf(item);
            if (oldIndex < 0 || newIndex < 0 || newIndex >= AddedFiles.Count) return;
            
            AddedFiles.Move(oldIndex, newIndex);
            SaveSession();
        }

        private void SavePlaylist(object obj) {
            var dlg = new SaveFileDialog {
                Filter = "TrainMe Playlist|*.json",
                FileName = "playlist.json"
            };
            if (dlg.ShowDialog() == true) {
                var playlist = new Playlist();
                foreach (var item in AddedFiles) {
                    playlist.Items.Add(new PlaylistItem {
                        FilePath = item.FilePath,
                        ScreenDeviceName = item.AssignedScreen?.DeviceName,
                        Opacity = item.Opacity,
                        Volume = item.Volume
                    });
                }
                
                var json = System.Text.Json.JsonSerializer.Serialize(playlist);
                System.IO.File.WriteAllText(dlg.FileName, json);
            }
        }

        private void LoadPlaylist(object obj) {
            var dlg = new OpenFileDialog {
                Filter = "TrainMe Playlist|*.json"
            };
            if (dlg.ShowDialog() == true) {
                try {
                    var json = System.IO.File.ReadAllText(dlg.FileName);
                    var playlist = System.Text.Json.JsonSerializer.Deserialize<Playlist>(json);
                    
                    if (playlist != null) {
                        AddedFiles.Clear();
                        if (AvailableScreens.Count == 0) RefreshScreens();
                        
                        foreach (var item in playlist.Items) {
                            var screen = AvailableScreens.FirstOrDefault(s => s.DeviceName == item.ScreenDeviceName) ?? AvailableScreens.FirstOrDefault();
                            var videoItem = new VideoItem(item.FilePath, screen);
                            videoItem.Opacity = item.Opacity;
                            videoItem.Volume = item.Volume;
                            AddedFiles.Add(videoItem);
                        }
                        UpdateButtons();
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Failed to load playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void SaveSession() {
            try {
                var playlistItems = AddedFiles.Select(item => new PlaylistItem {
                    FilePath = item.FilePath,
                    ScreenDeviceName = item.AssignedScreen?.DeviceName,
                    Opacity = item.Opacity,
                    Volume = item.Volume
                }).ToList();

                System.Threading.Tasks.Task.Run(() => {
                    try {
                        var playlist = new Playlist { Items = playlistItems };
                        var json = System.Text.Json.JsonSerializer.Serialize(playlist);
                        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.json");
                        System.IO.File.WriteAllText(path, json);
                    } catch { /* Background save failed */ }
                });
            } catch { /* Snapshot creation failed */ }
        }

        private void LoadSession() {
            try {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.json");
                if (!System.IO.File.Exists(path)) return;

                var json = System.IO.File.ReadAllText(path);
                var playlist = System.Text.Json.JsonSerializer.Deserialize<Playlist>(json);
                
                if (playlist != null) {
                    AddedFiles.Clear();
                    if (AvailableScreens.Count == 0) RefreshScreens();
                    
                    foreach (var item in playlist.Items) {
                        var screen = AvailableScreens.FirstOrDefault(s => s.DeviceName == item.ScreenDeviceName) ?? AvailableScreens.FirstOrDefault(s => s.Screen.Primary) ?? AvailableScreens.FirstOrDefault();
                        var videoItem = new VideoItem(item.FilePath, screen);
                        videoItem.Opacity = item.Opacity;
                        videoItem.Volume = item.Volume;
                        AddedFiles.Add(videoItem);
                    }
                    UpdateButtons();
                }
            } catch { /* Ignore load errors */ }
        }
    }
}
