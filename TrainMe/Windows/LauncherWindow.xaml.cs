using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
    }
}
