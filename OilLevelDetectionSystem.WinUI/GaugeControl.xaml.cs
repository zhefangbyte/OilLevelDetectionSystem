using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OilLevelDetectionSystem.WinUI
{
    public sealed partial class GaugeControl : UserControl
    {

        public static readonly DependencyProperty HeaderProperty
= DependencyProperty.Register(
nameof(Header),
typeof(string),
typeof(GaugeControl),
new PropertyMetadata(string.Empty));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty ValueProperty
= DependencyProperty.Register(
nameof(Value),
typeof(double),
typeof(GaugeControl),
new PropertyMetadata(0.0));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MinProperty
= DependencyProperty.Register(
nameof(Min),
typeof(double),
typeof(GaugeControl),
new PropertyMetadata(0.0));

        public double Min
        {
            get => (double)GetValue(MinProperty);
            set
            {
                SetValue(MinProperty, value);
                RadialAxis.Interval = (Max - Min) / 5;
                Middle = (Max + Min) / 2;
            }
        }

        public static readonly DependencyProperty MaxProperty
= DependencyProperty.Register(
nameof(Max),
typeof(double),
typeof(GaugeControl),
new PropertyMetadata(0.0));

        public double Max
        {
            get => (double)GetValue(MaxProperty);
            set
            {
                SetValue(MaxProperty, value);
                RadialAxis.Interval = (Max - Min) / 5;
                Middle = (Max + Min) / 2;
            }
        }

        public static readonly DependencyProperty MiddleProperty
= DependencyProperty.Register(
nameof(Middle),
typeof(double),
typeof(GaugeControl),
new PropertyMetadata(0.0));

        public double Middle
        {
            get => (double)GetValue(MiddleProperty);
            set
            {
                SetValue(MiddleProperty, value);
            }
        }


        public GaugeControl()
        {
            this.InitializeComponent();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RootGrid.Height = e.NewSize.Width;
        }
    }
}
