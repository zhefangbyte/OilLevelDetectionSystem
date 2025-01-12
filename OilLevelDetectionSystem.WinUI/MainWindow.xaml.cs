using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Graphics;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using WinRT.Interop;
using WindowId = Microsoft.UI.WindowId;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OilLevelDetectionSystem.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow m_appWindow;

        private DispatcherTimer _timer;
        private const int HOURS = 5;
        private DateTime _dateTime;
        private string port = "";

        private Random _random = new Random();

        private const string ERROR_NO = "No abnormality detected";
        private const string ERROR_OVERHOT = "Overheating fault";
        private const string ERROR_DISCHARGE = "Discharge failure";
        public ObservableCollection<GasInfo> GasInfos { get; set; } = new ObservableCollection<GasInfo>();
        private List<string> _names = new List<string> { "CO", "CH4", "C2H2", "H2" };

        private double _co = 0, _ch4 = 0, _c2h2 = 0, _h2 = 0;

        private ContentDialog _contentDialog;
        private bool _isContentDialogOpen;

        public ObservableCollection<WarnInfo> WarnInfos { get; set; } = new ObservableCollection<WarnInfo>();


        public MainWindow()
        {
            this.InitializeComponent();

            Title = "Oil level “supervisor”—— oil-immersed power transformer oil level gas detection system";

            // Get the AppWindow for our XAML Window
            m_appWindow = GetAppWindowForCurrentWindow();
            if (m_appWindow != null)
            {
                // You now have an AppWindow object and can call its methods to manipulate the window.
                // Just to do something here, let's change the title of the window...
                m_appWindow.Title = Title;
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += _timer_Tick;

            for (int i = 0; i < _names.Count; i++)
            {
                GasInfos.Add(new GasInfo { Name = _names[i], GasPoints = new ObservableCollection<GasPoint>() });
            }

            _dateTime = DateTime.Now;
            _dateTime -= new TimeSpan(HOURS, 0, 0);

            for (int j = 0; j < HOURS * 60 * 60; j++)
            {
                for (int i = 0; i < _names.Count; i++)
                {
                    GasInfos[i].GasPoints.Add(new GasPoint { DateTime = _dateTime });
                }
                _dateTime = _dateTime.AddSeconds(1);
            }

            _timer.Start();
            _contentDialog = new ContentDialog();
            _contentDialog.Title = "Warning";
            _contentDialog.DefaultButton = ContentDialogButton.Close;
            _contentDialog.CloseButtonText = "Close";
            _contentDialog.Closed += _contentDialog_Closed;
            _contentDialog.Opened += _contentDialog_Opened;

            _isContentDialogOpen = false;
        }

        private void _contentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            _isContentDialogOpen = true;
        }

        private void _contentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            _isContentDialogOpen = false;
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        private double GetRealValue(string gasName)
        {
            if (gasName == "CO")
            {
                return _co;
                //return _random.Next(500, 800);
            }
            else if (gasName == "CH4")
            {
                return _ch4;
                //return _random.Next(101, 389);
            }
            else if (gasName == "C2H2")
            {
                return _c2h2;
                //return _random.Next(400, 500);
            }
            else if (gasName == "H2")
            {
                return _h2;
                //return _random.Next(78, 200);
            }
            return 0;
        }

        private void Logout(string info)
        {
            Debug.WriteLine(info);
        }

        //监听串口
        private async Task Listen()
        {
            Logout("开始监听...");
            try
            {
                byte[] recvData = new byte[0];
                SerialPortUtils.OpenClosePort(port, 115200);
                SerialPortUtils.SerialPort.DataReceived += (s, e) =>
                {
                    SerialPort _SerialPort = (SerialPort)s;

                    int _bytesToRead = _SerialPort.BytesToRead;
                    recvData = new byte[_bytesToRead];

                    _SerialPort.Read(recvData, 0, _bytesToRead);
                };
                while (true)
                {
                    await Task.Delay(1000);
                    _c2h2 = recvData[0];
                    _co = recvData[1];
                    _ch4 = recvData[2];
                }
            }
            catch (Exception ex)
            {
                Logout(ex.Message);
            }
            finally
            {
                Logout("停止监听");
            }
        }

        private async Task MockListen()
        {
            Logout("开始监听...");
            while (true)
            {
                Random random = new();
                await Task.Delay(1000);
                _c2h2 = 500;
                _co = 100;
                _ch4 = 400;
            }
        }

        private void _timer_Tick(object sender, object e)
        {
            UpdateGasInfos();

            TimeTextBlock.Text = _dateTime.ToString();
            _dateTime = _dateTime.AddSeconds(1);
        }

        private void UpdateGasInfos()
        {
            for (int i = 0; i < GasInfos.Count; i++)
            {
                GasInfos[i].GasPoints.RemoveAt(0);
                double value = GetRealValue(GasInfos[i].Name); //GetSimulatedValue(GasInfos[i].Name);
                GasInfos[i].GasPoints.Add(new GasPoint { DateTime = _dateTime, Value = value });
                GasInfos[i].Value = value;
            }

            CheckError();
        }

        private double GetCODevidedByH2()
        {
            return GasInfos[3].Value / GasInfos[0].Value;
        }

        private double GetC2H2DevidedByCH4()
        {
            return GasInfos[1].Value / GasInfos[2].Value;
        }

        private async void CheckError()
        {
            if (GetCODevidedByH2() >= 5 && GetC2H2DevidedByCH4() >= 50)
            {
                InfoTextBlock.Text = "Minor discharge failure";
                SensorPanel.WarnColor = Colors.Yellow;
            }
            else if (GetCODevidedByH2() >= 2 && GetC2H2DevidedByCH4() >= 1)
            {
                InfoTextBlock.Text = "Moderate discharge failure";
                SensorPanel.WarnColor = Colors.Orange;
            }
            else if (GetCODevidedByH2() < 2 && GetC2H2DevidedByCH4() < 1)
            {
                InfoTextBlock.Text = "Severe discharge failure";
                SensorPanel.WarnColor = Colors.Red;
            }
            else
            {
                InfoTextBlock.Text = "Normal work";
                SensorPanel.WarnColor = (Color)Application.Current.Resources["ForegroundColor"];
            }
            if (InfoTextBlock.Text != "Normal work")
            {
                WarnInfos.Insert(0, new WarnInfo
                {
                    Date = _dateTime.ToString(),
                    Message = InfoTextBlock.Text,
                    WarnBrush = new SolidColorBrush(SensorPanel.WarnColor)
                });
                if (!_isContentDialogOpen)
                {
                    if (_contentDialog.XamlRoot == null)
                    {
                        _contentDialog.XamlRoot = this.Content.XamlRoot;
                    }
                    _contentDialog.Content = InfoTextBlock.Text;
                    try
                    {
                        //await _contentDialog.ShowAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            InfoTextBlock.Foreground = new SolidColorBrush(SensorPanel.WarnColor);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ListenButton.IsEnabled = false;
            // await Listen();
            await MockListen();
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in GasInfos)
            {
                item.Width = (e.NewSize.Width - 10 * (_names.Count - 1)) / _names.Count;
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in GasInfos)
            {
                item.Height = e.NewSize.Height / 3;
            }
        }

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                XamlRoot = RootGrid.XamlRoot,
                Title = "确认导出数据？",
                PrimaryButtonText = "导出",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };
            var result = await contentDialog.ShowAsync();
        }

        private void RootGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                var point = e.GetCurrentPoint(null).Position;
                double left = point.X;
                double top = point.Y;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuBarItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (AboutMenuBarItem).ContextFlyout.ShowAt(AboutMenuBarItem);
        }

        /// <summary>
        /// 在设置拖动区域时，需要考虑到系统缩放比例对像素的影响.
        /// </summary>
        /// <param name="pixel">像素值.</param>
        /// <returns>转换后的结果.</returns>
        private static int GetActualPixel(double pixel)
        {
            var windowHandle = WindowNative.GetWindowHandle(App.MainWindow);
            var currentDpi = PInvoke.User32.GetDpiForWindow(windowHandle);
            return Convert.ToInt32(pixel * (currentDpi / 96.0));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            port = (sender as TextBox).Text;
            Debug.WriteLine(port);
        }

        private void Grid_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            var titleBar = App.AppWindow.TitleBar;

            // 当前控件的实际宽度.
            var totalSpace = AppBarTitle.ActualWidth;
            var height = AppBarTitle.ActualHeight;

            // 可交互元素左边界相对于整个控件左边界的偏移值.
            var elementLeftOffset = CommandArea.ActualOffset.X;

            // 可交互元素的右边界相对于整个控件左边界的偏移值.
            var elementRightOffset = CommandArea.ActualOffset.X + CommandArea.ActualWidth;

            var leftSpace = elementLeftOffset;
            var rightSpace = totalSpace - elementRightOffset;

            var leftRect = new RectInt32(0, 0, GetActualPixel(leftSpace), GetActualPixel(height));
            var rightRect = new RectInt32(GetActualPixel(elementRightOffset), 0, GetActualPixel(rightSpace), GetActualPixel(height));

            titleBar.SetDragRectangles(new RectInt32[] { leftRect, rightRect });
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            (AboutMenuBarItem).ContextFlyout.ShowAt(AboutMenuBarItem);
        }

        private void ToggleMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!FullScreen.IsChecked)
            {
                m_appWindow.SetPresenter(AppWindowPresenterKind.Default);
            }
            else
            {
                m_appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
        }
    }

    public class GasInfo : INotifyPropertyChanged
    {
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                _value = Math.Round(value, 2);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Width)));
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
            }
        }


        public string Name { get; set; }
        public ObservableCollection<GasPoint> GasPoints { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class GasPoint : INotifyPropertyChanged
    {
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        private int _time;
        public int Time
        {
            get => _time;
            set
            {
                _time = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
            }
        }

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                _dateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateTime)));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class WarnInfo
    {
        public string Date { get; set; }
        public string Message { get; set; }
        public SolidColorBrush WarnBrush { get; set; }
    }

}
