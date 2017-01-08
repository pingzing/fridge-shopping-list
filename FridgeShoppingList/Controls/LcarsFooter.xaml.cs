using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FridgeShoppingList.Controls
{
    public sealed partial class LcarsFooter : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(LcarsFooter), new PropertyMetadata(string.Empty, (d, e) =>
            {
                (d as LcarsFooter).FooterContent = e.NewValue;
            }));

        public object FooterContent
        {
            get { return (object)GetValue(FooterContentProperty); }
            set { SetValue(FooterContentProperty, value); }
        }

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register(nameof(FooterContent), typeof(object), typeof(LcarsFooter), new PropertyMetadata(null));

        public DataTemplate FooterContentTemplate
        {
            get { return (DataTemplate)GetValue(FooterContentTemplateProperty); }
            set { SetValue(FooterContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty FooterContentTemplateProperty =
            DependencyProperty.Register(nameof(FooterContentTemplate), typeof(DataTemplate), typeof(LcarsFooter), new PropertyMetadata(null));


        public LcarsFooter()
        {
            this.InitializeComponent();
            FooterContentTemplate = DefaultFooterContentTemplate;
        }
    }
}
