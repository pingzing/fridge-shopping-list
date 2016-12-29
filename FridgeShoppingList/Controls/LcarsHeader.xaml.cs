using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FridgeShoppingList.Controls
{
    [ContentProperty(Name = nameof(HeaderContent))]
    public sealed partial class LcarsHeader : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(LcarsHeader), new PropertyMetadata(string.Empty, (d, e) =>
            {
                (d as LcarsHeader).HeaderContent = e.NewValue;
            }));

        public object HeaderContent
        {
            get { return (object)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }
        
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(LcarsHeader), new PropertyMetadata(null));

        public DataTemplate HeaderContentTemplate
        {
            get { return (DataTemplate)GetValue(HeaderContentTemplateProperty); }
            set { SetValue(HeaderContentTemplateProperty, value); }
        }
        
        public static readonly DependencyProperty HeaderContentTemplateProperty =
            DependencyProperty.Register(nameof(HeaderContentTemplate), typeof(DataTemplate), typeof(LcarsHeader), new PropertyMetadata(null));

        public LcarsHeader()
        {
            this.InitializeComponent();            
            HeaderContentTemplate = DefaultHeaderContentTemplate;
        }
    }
}
