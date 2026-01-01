using System.Windows;
using System.Windows.Input;
using TrainMe.ViewModels;

namespace TrainMe.Windows {
    public partial class SettingsWindow : Window {
        private SettingsViewModel _viewModel;

        public SettingsWindow() {
            InitializeComponent();
            _viewModel = new SettingsViewModel();
            _viewModel.RequestClose += (s, e) => this.Close();
            DataContext = _viewModel;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
