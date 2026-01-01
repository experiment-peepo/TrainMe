using System.Windows.Input;
using TrainMe.Classes;

namespace TrainMe.ViewModels {
    /// <summary>
    /// ViewModel for the Settings window
    /// </summary>
    public class SettingsViewModel : ObservableObject {
        private double _defaultOpacity;
        private double _defaultVolume;
        private bool _autoLoadSession;
        private bool _preventOverlayMinimize;

        public SettingsViewModel() {
            // Load current settings
            var settings = App.Settings;
            _defaultOpacity = settings.DefaultOpacity;
            _defaultVolume = settings.DefaultVolume;
            _autoLoadSession = settings.AutoLoadSession;
            _preventOverlayMinimize = settings.PreventOverlayMinimize;

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        public double DefaultOpacity {
            get => _defaultOpacity;
            set => SetProperty(ref _defaultOpacity, value);
        }

        public double DefaultVolume {
            get => _defaultVolume;
            set => SetProperty(ref _defaultVolume, value);
        }

        public bool AutoLoadSession {
            get => _autoLoadSession;
            set => SetProperty(ref _autoLoadSession, value);
        }

        public bool PreventOverlayMinimize {
            get => _preventOverlayMinimize;
            set => SetProperty(ref _preventOverlayMinimize, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public event System.EventHandler RequestClose;

        private void Ok(object obj) {
            // Save settings
            var settings = App.Settings;
            settings.DefaultOpacity = DefaultOpacity;
            settings.DefaultVolume = DefaultVolume;
            settings.AutoLoadSession = AutoLoadSession;
            settings.PreventOverlayMinimize = PreventOverlayMinimize;
            settings.Save();

            RequestClose?.Invoke(this, System.EventArgs.Empty);
        }

        private void Cancel(object obj) {
            RequestClose?.Invoke(this, System.EventArgs.Empty);
        }
    }
}

