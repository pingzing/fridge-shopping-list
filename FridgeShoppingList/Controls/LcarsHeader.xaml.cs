using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

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

        /// <summary>
        /// A reference to the application <see cref="Frame"/>, used to feed the back button's NavButtonBehavior.
        /// </summary>
        public Frame FrameReference
        {
            get { return (Frame)GetValue(FrameReferenceProperty); }
            set { SetValue(FrameReferenceProperty, value); }
        }
        
        public static readonly DependencyProperty FrameReferenceProperty =
            DependencyProperty.Register(nameof(FrameReference), typeof(Frame), typeof(LcarsHeader), new PropertyMetadata(null));



        public LcarsHeader()
        {
            this.InitializeComponent();            
            HeaderContentTemplate = DefaultHeaderContentTemplate;
            HeaderContent = Text;
        }

        bool animating = false;
        public async void PlayButtonAppearAnimation()
        {
            if(animating)
            {
                return;
            }

            //setup
            animating = true;
            BackButton.Visibility = Visibility.Collapsed;
            BackButtonCover.Visibility = Visibility.Visible;
            BackButtonCover.Opacity = 1;                       
            await BackButtonCover.Scale(0, 1, duration: 0)
            
            //Whoosh in the black cover
                .Then().Scale(1, 1, duration: 500).StartAsync();

            //fade in the back button by hiding the cover rectangle
            BackButton.Visibility = Visibility.Visible;
            await BackButtonCover.Fade(0, 200).StartAsync();
            
            //tear down                                   
            BackButtonCover.Visibility = Visibility.Collapsed;            
            animating = false;
        }
    }
}
