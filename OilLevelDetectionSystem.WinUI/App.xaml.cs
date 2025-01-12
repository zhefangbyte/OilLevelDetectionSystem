using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OilLevelDetectionSystem.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private IntPtr _windowHandle;

        /// <summary>
        /// 应用窗口对象.
        /// </summary>
        public static AppWindow AppWindow { get; private set; }

        /// <summary>
        /// 主窗口.
        /// </summary>
        public static Window MainWindow { get; private set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            // 获取当前窗口句柄
            _windowHandle = WindowNative.GetWindowHandle(MainWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(_windowHandle);

            // 获取应用窗口对象
            AppWindow = AppWindow.GetFromWindowId(windowId);
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            Color foregroundColor = (Color)Application.Current.Resources["ForegroundColor"];
            Color backgroundColor = (Color)Application.Current.Resources["BackgroundColor"];

            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;

            AppWindow.TitleBar.ButtonHoverBackgroundColor = backgroundColor;
            AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;

            AppWindow.TitleBar.ButtonPressedBackgroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonPressedForegroundColor = Colors.WhiteSmoke;

            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.DarkGray;
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;

            MainWindow.Activate();
        }
    }
}
