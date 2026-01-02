using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using TrainMeX.Classes;
using Xunit;

namespace TrainMeX.Tests {
    public class GlobalHotkeyServiceTests {
        [Fact]
        public void Constructor_CreatesInstance() {
            var service = new GlobalHotkeyService();
            Assert.NotNull(service);
            service.Dispose();
        }

        [Fact]
        public void Dispose_WithoutInitialize_DoesNotThrow() {
            var service = new GlobalHotkeyService();
            service.Dispose();
            // Should not throw
            Assert.True(true);
        }

        [Fact]
        public void Dispose_MultipleTimes_DoesNotThrow() {
            var service = new GlobalHotkeyService();
            service.Dispose();
            service.Dispose();
            Assert.True(true);
        }
    }
}
