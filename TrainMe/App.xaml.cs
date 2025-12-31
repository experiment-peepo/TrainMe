/*
	Copyright (C) 2021 Damsel

	This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

	This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

	You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TrainMe {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application {
        public static Classes.VideoPlayerService VideoService => Classes.ServiceContainer.Get<Classes.VideoPlayerService>();
        public static Classes.UserSettings Settings => Classes.ServiceContainer.Get<Classes.UserSettings>();

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            
            // Enable hardware acceleration
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            
            // Add global exception handlers
            this.DispatcherUnhandledException += (s, args) => {
                MessageBox.Show($"Unhandled exception: {args.Exception.Message}\n\nStack Trace:\n{args.Exception.StackTrace}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
            
            AppDomain.CurrentDomain.UnhandledException += (s, args) => {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"Fatal exception: {ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}", 
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            
            // Register Services
            Classes.ServiceContainer.Register(Classes.UserSettings.Load());
            Classes.ServiceContainer.Register(new Classes.VideoPlayerService());
        }
    }
}
