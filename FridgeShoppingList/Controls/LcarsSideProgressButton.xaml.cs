using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FridgeShoppingList.Controls
{
    public sealed partial class LcarsSideProgressButton : UserControl
    {
        private const string NormalStateName = "Normal";
        private const string ProgressStateName = "Progressing";

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(LcarsSideProgressButton), new PropertyMetadata(""));
                
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(LcarsSideProgressButton), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(LcarsSideProgressButton), new PropertyMetadata(null));        
        
        public static readonly DependencyProperty IsProgressingProperty =
            DependencyProperty.Register(nameof(IsProgressing), typeof(bool), typeof(LcarsSideProgressButton), new PropertyMetadata(false,
                (depObj, args) => 
                {
                    LcarsSideProgressButton _this = depObj as LcarsSideProgressButton;
                    if (_this == null
                        || args.OldValue == args.NewValue)
                    {
                        return;
                    }                    

                    bool isNowProgressing = (bool)args.NewValue;                    

                    if (isNowProgressing)
                    {                        
                        VisualStateManager.GoToState(_this, ProgressStateName, true);
                    }
                    else
                    {                                                
                        VisualStateManager.GoToState(_this, NormalStateName, true);
                    }
                }));

        public LcarsSideProgressButton()
        {
            this.InitializeComponent();
            VisualStateManager.GoToState(this, NormalStateName, false);
        }        

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsProgressing)
            {
                // Should this be overridable? Generally, if a button is "doing" something,
                // you don't want to start doing that thing again...
                return;
            }

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public bool IsProgressing
        {
            get { return (bool)GetValue(IsProgressingProperty); }
            set { SetValue(IsProgressingProperty, value); }
        }
    }
}
