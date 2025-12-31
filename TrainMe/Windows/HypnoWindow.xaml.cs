using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using TrainMe.Classes;
using TrainMe.ViewModels;

namespace TrainMe.Windows {
    public partial class HypnoWindow : Window {
        private HypnoViewModel _viewModel;
        private Screen _targetScreen;

        public HypnoWindow(Screen targetScreen = null) {
            InitializeComponent();
            _targetScreen = targetScreen;
            
            _viewModel = new HypnoViewModel();
            DataContext = _viewModel;
            // ... (keep event subscriptions)
            _viewModel.RequestPlay += (s, e) => FirstVideo.Play();
            _viewModel.RequestPause += (s, e) => {
                FirstVideo.Pause();
            };
            _viewModel.RequestStop += (s, e) => {
                FirstVideo.Stop();
                FirstVideo.Close();
            };
        }

        public HypnoViewModel ViewModel => _viewModel;

        private void FirstVideo_MediaEnded(object sender, RoutedEventArgs e) {
            _viewModel.OnMediaEnded();
        }

        private void FirstVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e) {
            _viewModel.OnMediaFailed(e.ErrorException);
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
