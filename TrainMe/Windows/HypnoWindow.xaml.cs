using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using TrainMe.Classes;
using TrainMe.ViewModels;

namespace TrainMe.Windows {
    [SupportedOSPlatform("windows")]
    public partial class HypnoWindow : Window, IDisposable {
        private HypnoViewModel _viewModel;
        private Screen _targetScreen;
        private bool _disposed = false;
        private bool _isRestoringState = false;

        public HypnoWindow(Screen targetScreen = null) {
            InitializeComponent();
            _targetScreen = targetScreen;
            
            _viewModel = new HypnoViewModel();
            DataContext = _viewModel;
            // ... (keep event subscriptions)
            _viewModel.RequestPlay += ViewModel_RequestPlay;
            _viewModel.RequestPause += ViewModel_RequestPause;
            _viewModel.RequestStop += ViewModel_RequestStop;
            _viewModel.RequestStopBeforeSourceChange += ViewModel_RequestStopBeforeSourceChange;
            _viewModel.MediaErrorOccurred += ViewModel_MediaErrorOccurred;
            
            // Apply prevent minimize setting
            ApplyPreventMinimizeSetting();
            
            // Handle window state changes to prevent minimization
            StateChanged += HypnoWindow_StateChanged;
        }
        
        private void ViewModel_RequestPlay(object sender, EventArgs e) {
            FirstVideo?.Play();
        }

        private void ViewModel_RequestPause(object sender, EventArgs e) {
            FirstVideo?.Pause();
        }

        private void ViewModel_RequestStop(object sender, EventArgs e) {
            FirstVideo?.Stop();
            FirstVideo?.Close();
        }

        private void ViewModel_RequestStopBeforeSourceChange(object sender, EventArgs e) {
            // Stop the current video before changing source to ensure MediaEnded fires reliably
            // This prevents WPF MediaElement from missing MediaEnded events when Source changes
            if (FirstVideo != null) {
                FirstVideo.Stop();
            }
        }
        
        private void HypnoWindow_StateChanged(object sender, EventArgs e) {
            if (_isRestoringState) return;
            
            if (App.Settings.PreventOverlayMinimize && WindowState == WindowState.Minimized) {
                _isRestoringState = true;
                WindowState = WindowState.Normal;
                _isRestoringState = false;
            }
        }
        
        public void ApplyPreventMinimizeSetting() {
            // The StateChanged event handler will prevent minimization if enabled
            // No need to change ResizeMode as the window is already fullscreen
        }

        public HypnoViewModel ViewModel => _viewModel;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // Unsubscribe from events
                    if (_viewModel != null) {
                        _viewModel.RequestPlay -= ViewModel_RequestPlay;
                        _viewModel.RequestPause -= ViewModel_RequestPause;
                        _viewModel.RequestStop -= ViewModel_RequestStop;
                        _viewModel.RequestStopBeforeSourceChange -= ViewModel_RequestStopBeforeSourceChange;
                        _viewModel.MediaErrorOccurred -= ViewModel_MediaErrorOccurred;
                    }
                    
                    StateChanged -= HypnoWindow_StateChanged;
                    
                    // Dispose MediaElement
                    if (FirstVideo != null) {
                        FirstVideo.Stop();
                        FirstVideo.Close();
                        FirstVideo = null;
                    }
                }
                _disposed = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected override void OnClosed(EventArgs e) {
            Dispose();
            base.OnClosed(e);
        }

        private void FirstVideo_MediaEnded(object sender, RoutedEventArgs e) {
            _viewModel.OnMediaEnded();
        }

        private void FirstVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e) {
            _viewModel.OnMediaFailed(e.ErrorException);
        }

        private void ViewModel_MediaErrorOccurred(object sender, MediaErrorEventArgs e) {
            // Forward the error to the VideoPlayerService so it can notify subscribers
            App.VideoService.OnMediaError(e.ErrorMessage);
        }

        private void FirstVideo_MediaOpened(object sender, RoutedEventArgs e) {
            // Ensure the video starts playing when it's loaded
            FirstVideo.Play();
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_SHOWWINDOW = 0x0040;
        const uint SWP_FRAMECHANGED = 0x0020;
        const uint SWP_ASYNCWINDOWPOS = 0x4000;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_MAXIMIZE = 3;
        const int SW_SHOW = 5;

        [SupportedOSPlatform("windows")]
        private void Window_SourceInitialized(object sender, EventArgs e) {
            if (_targetScreen != null) {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                
                // 1. Transparency
                int extendedStyle = WindowServices.GetWindowLong(hwnd, WindowServices.GWL_EXSTYLE);
                WindowServices.SetWindowLong(hwnd, WindowServices.GWL_EXSTYLE, extendedStyle | WindowServices.WS_EX_TRANSPARENT);

                // 2. Physical Placement
                var b = _targetScreen.Bounds;
                SetWindowPos(hwnd, new IntPtr(-1), b.Left, b.Top, b.Width, b.Height, 
                    SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

                // 3. WPF Metadata
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.WindowState = WindowState.Normal;

                // 4. Delayed WPF logical sync for scaling
                this.Dispatcher.BeginInvoke(new Action(() => {
                    var dpi = VisualTreeHelper.GetDpi(this);
                    this.Left = b.Left / dpi.DpiScaleX;
                    this.Top = b.Top / dpi.DpiScaleY;
                    this.Width = b.Width / dpi.DpiScaleX;
                    this.Height = b.Height / dpi.DpiScaleY;
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
    }
}
