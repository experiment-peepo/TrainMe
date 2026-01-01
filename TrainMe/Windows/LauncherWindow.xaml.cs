using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TrainMe.Classes;
using TrainMe.ViewModels;
using Point = System.Windows.Point;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;

namespace TrainMe.Windows {
    public partial class LauncherWindow : Window {
        private LauncherViewModel ViewModel => DataContext as LauncherViewModel;

        public LauncherWindow() {
            InitializeComponent();
            DataContext = new LauncherViewModel();
        }

        private GlobalHotkeyService _hotkeys;

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            _hotkeys = new GlobalHotkeyService();
            _hotkeys.Initialize(helper.Handle);
            
            _hotkeys.OnPause += (s, args) => {
                 if (App.VideoService.IsPlaying) App.VideoService.PauseAll();
                 else App.VideoService.ContinueAll();
            };
            
            _hotkeys.OnPanic += (s, args) => {
                App.VideoService.StopAll();
            };
        }

        protected override void OnClosed(EventArgs e) {
            _hotkeys?.Dispose();
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }



        private Point _startPoint;

        private void AddedFilesList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(null);
        }

        private void AddedFilesList_MouseMove(object sender, MouseEventArgs e) {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) {
                
                // Don't trigger drag-drop if we are clicking on a Slider or ComboBox
                var originalSource = e.OriginalSource as DependencyObject;
                if (FindAncestor<Slider>(originalSource) != null || FindAncestor<ComboBox>(originalSource) != null) {
                    return;
                }

                var listView = sender as ListView;
                var listViewItem = FindAncestor<ListViewItem>(originalSource);
                if (listViewItem == null) return;

                var data = (VideoItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                
                DataObject dragData = new DataObject("VideoItem", data);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
            }
        }

        private void AddedFilesList_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            } else if (e.Data.GetDataPresent("VideoItem")) {
                e.Effects = DragDropEffects.Move;
            } else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void AddedFilesList_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ViewModel?.AddDroppedFiles(files);
            } else if (e.Data.GetDataPresent("VideoItem")) {
                var sourceItem = (VideoItem)e.Data.GetData("VideoItem");
                var targetItem = ((FrameworkElement)e.OriginalSource).DataContext as VideoItem;

                if (sourceItem != null && targetItem != null && sourceItem != targetItem) {
                    var newIndex = ViewModel.AddedFiles.IndexOf(targetItem);
                    ViewModel.MoveVideoItem(sourceItem, newIndex);
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject {
            do {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            var settingsWindow = new SettingsWindow {
                Owner = this
            };
            settingsWindow.ShowDialog();
        }
    }

    public class StringToVisibilityConverter : IValueConverter {
        public static readonly StringToVisibilityConverter Instance = new StringToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string str && !string.IsNullOrWhiteSpace(str)) {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class PluralConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int count) {
                return count == 1 ? "video" : "videos";
            }
            return "videos";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter {
        public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolValue && boolValue) {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Visibility visibility) {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    public class StatusMessageTypeToBrushConverter : IValueConverter {
        public static readonly StatusMessageTypeToBrushConverter Instance = new StatusMessageTypeToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ViewModels.StatusMessageType messageType) {
                return messageType switch {
                    ViewModels.StatusMessageType.Success => new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0xFF, 0x00)), // Green with transparency
                    ViewModels.StatusMessageType.Warning => new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0x00)), // Yellow with transparency
                    ViewModels.StatusMessageType.Error => new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x00, 0x00)),   // Red with transparency
                    _ => new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x00, 0x00)) // Default: dark with transparency
                };
            }
            return new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x00, 0x00));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class StatusMessageTypeToForegroundConverter : IValueConverter {
        public static readonly StatusMessageTypeToForegroundConverter Instance = new StatusMessageTypeToForegroundConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ViewModels.StatusMessageType messageType) {
                return messageType switch {
                    ViewModels.StatusMessageType.Success => new SolidColorBrush(Color.FromArgb(0xCC, 0x90, 0xEE, 0x90)), // Light green
                    ViewModels.StatusMessageType.Warning => new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0x00)), // Yellow
                    ViewModels.StatusMessageType.Error => new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0x69, 0xB4)),   // Hot pink for errors (theme consistency)
                    _ => new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)) // Default: white
                };
            }
            return new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ValidationStatusToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ViewModels.FileValidationStatus status) {
                return status switch {
                    ViewModels.FileValidationStatus.Valid => new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0xFF, 0x00)), // Green border
                    ViewModels.FileValidationStatus.Missing => new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0x00, 0x00)), // Red border
                    ViewModels.FileValidationStatus.Invalid => new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xA5, 0x00)), // Orange border
                    _ => new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)) // Default: white border (transparent)
                };
            }
            return new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ValidationStatusToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ViewModels.FileValidationStatus status) {
                return status switch {
                    ViewModels.FileValidationStatus.Valid => "✓", // Checkmark
                    ViewModels.FileValidationStatus.Missing => "⚠", // Warning
                    ViewModels.FileValidationStatus.Invalid => "✗", // X mark
                    _ => "▶" // Play icon for unknown/not validated
                };
            }
            return "▶";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ValidationStatusToOpacityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ViewModels.FileValidationStatus status) {
                return status == ViewModels.FileValidationStatus.Valid ? 1.0 : 0.7; // Gray out invalid files
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ValidationStatusToForegroundConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ViewModels.FileValidationStatus status) {
                return status switch {
                    ViewModels.FileValidationStatus.Valid => new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)), // Green
                    ViewModels.FileValidationStatus.Missing => new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)), // Red
                    ViewModels.FileValidationStatus.Invalid => new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)), // Orange
                    _ => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) // White for unknown
                };
            }
            return new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
