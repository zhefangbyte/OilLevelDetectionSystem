using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OilLevelDetectionSystem.WinUI
{
    [ContentProperty(Name = nameof(ContentElement))]
    public sealed partial class HeaderContentPanel : UserControl
    {
        public FrameworkElement ContentElement { get; set; }

        public static readonly DependencyProperty HeaderProperty
    = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(HeaderContentPanel),
        new PropertyMetadata(string.Empty));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty WarnColorProperty
= DependencyProperty.Register(
nameof(WarnColor),
typeof(Color),
typeof(HeaderContentPanel),
new PropertyMetadata(Application.Current.Resources["ComponentBackgroundColor"]));

        public Color WarnColor
        {
            get => (Color)GetValue(WarnColorProperty);
            set
            {
                SetValue(WarnColorProperty, value);
                var color = (Color)Application.Current.Resources["ComponentBackgroundColor"];
                if (color == value)
                {
                    RootGrid.BorderThickness = (Thickness)Application.Current.Resources["BorderThickness"];
                }
                else
                {
                    RootGrid.BorderThickness = new Thickness(0);
                }
                RadialGradientBrush brush = new RadialGradientBrush
                {
                    MappingMode = BrushMappingMode.RelativeToBoundingBox,
                    Center = new Point(0.5, 0.5),
                    RadiusX = 1,
                    RadiusY = 1,
                    GradientOrigin = new Point(0.5, 0.5)
                };
                brush.GradientStops.Add(new GradientStop { Color = color, Offset = 0 });
                brush.GradientStops.Add(new GradientStop { Color = value, Offset = 1 });
                RootGrid.Background = brush;
            }
        }


        public HeaderContentPanel()
        {
            this.InitializeComponent();
        }
    }

}
