using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Template10.Common;
using Template10.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace FridgeShoppingList.Controls.LcarsModalDialog
{
    //TODO: Add animations on open and close, and switch out the background to be transparent, and a blur effect

    [TemplatePart(Name = PartButton1Host, Type = typeof(Border))]
    [TemplatePart(Name = PartButton2Host, Type = typeof(Border))]
    public class LcarsModalDialog : ContentControl
    {
        public event TypedEventHandler<LcarsModalDialog, object> PrimaryButtonClick;
        public event TypedEventHandler<LcarsModalDialog, object> SecondaryButtonClick;

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(object), typeof(LcarsModalDialog), new PropertyMetadata(null));
                
        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.Register(nameof(TitleTemplate), typeof(DataTemplate), typeof(LcarsModalDialog), new PropertyMetadata(null));        
        
        public static readonly DependencyProperty PrimaryButtonTextProperty =
            DependencyProperty.Register(nameof(PrimaryButtonText), typeof(string), typeof(LcarsModalDialog), new PropertyMetadata("Ok"));        
        
        public static readonly DependencyProperty PrimaryButtonCommandProperty =
            DependencyProperty.Register(nameof(PrimaryButtonCommand), typeof(ICommand), typeof(LcarsModalDialog), new PropertyMetadata(null));
                
        public static readonly DependencyProperty PrimaryButtonCommandParameterProperty =
            DependencyProperty.Register(nameof(PrimaryButtonCommandParameter), typeof(object), typeof(LcarsModalDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty IsPrimaryButtonEnabledProperty =
            DependencyProperty.Register(nameof(IsPrimaryButtonEnabled), typeof(bool), typeof(LcarsModalDialog), new PropertyMetadata(true));

        public static readonly DependencyProperty SecondaryButtonTextProperty =
           DependencyProperty.Register(nameof(SecondaryButtonText), typeof(string), typeof(LcarsModalDialog), new PropertyMetadata("Ok"));

        public static readonly DependencyProperty SecondaryButtonCommandProperty =
            DependencyProperty.Register(nameof(SecondaryButtonCommand), typeof(ICommand), typeof(LcarsModalDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SecondaryButtonCommandParameterProperty =
            DependencyProperty.Register(nameof(SecondaryButtonCommandParameter), typeof(object), typeof(LcarsModalDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty IsSecondaryButtonEnabledProperty =
            DependencyProperty.Register(nameof(IsSecondaryButtonEnabled), typeof(bool), typeof(LcarsModalDialog), new PropertyMetadata(true));

        private TaskCompletionSource<bool> _windowClosedTask;

        private const string PartButton1Host = "Button1Host";
        private const string PartButton2Host = "Button2Host";
        private Border _button1Host;
        private Border _button2Host;

        private enum DialogButtonType { Primary, Secondary} ;
        private enum DialogCloseReason { PrimaryClicked, SecondaryClicked, Cancelled, Programmatic };

        public LcarsModalDialog()
        {
            this.DefaultStyleKey = typeof(LcarsModalDialog);                       
        }

        public async Task OpenAsync()
        {
            _windowClosedTask = new TaskCompletionSource<bool>();
            WindowWrapper.Current().Dispatcher.Dispatch(() => 
            {
                var modal = Window.Current.Content as ModalDialog;
                var dialog = modal.ModalContent as LcarsModalDialog;                                
                modal.ModalContent = this;
                modal.IsModal = true;
            });

            await _windowClosedTask.Task; //This gets run to completion in Close().
        }

        public void Close()
        {
            Close(DialogCloseReason.Programmatic);
        }

        private void Close(DialogCloseReason closeReason)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                var dialog = modal.ModalContent as LcarsModalDialog;
                modal.IsModal = false;

                _windowClosedTask.SetResult(true);
            });
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _button1Host = GetTemplateChild(PartButton1Host) as Border;
            _button2Host = GetTemplateChild(PartButton2Host) as Border;

            if (_button1Host != null)
            {
                if (_button1Host.Child is Button)
                {
                    var oldButton = (Button)_button1Host.Child;
                    oldButton.Click -= PrimaryButton_Click;
                    oldButton.Click += PrimaryButton_Click;
                }
                else
                {
                    _button1Host.Child = CreateDialogButton(DialogButtonType.Primary);
                }
            }

            if(_button2Host != null)
            {
                if (_button2Host.Child is Button)
                {
                    var oldButton = (Button)_button2Host.Child;
                    oldButton.Click -= SecondaryButton_Click;
                    oldButton.Click += SecondaryButton_Click;
                }
                else
                {
                    _button2Host.Child = CreateDialogButton(DialogButtonType.Secondary);
                }
            }
        }

        private Button CreateDialogButton(DialogButtonType type)
        {
            Button button = new Button();
            Binding textBinding = new Binding
            {
                Mode = BindingMode.OneWay,
                Source = this,
                Path = type == DialogButtonType.Primary
                                ? new PropertyPath(nameof(PrimaryButtonText))
                                : new PropertyPath(nameof(SecondaryButtonText)),

            };
            button.SetBinding(Button.ContentProperty, textBinding);

            Binding commandBinding = new Binding
            {
                Mode = BindingMode.OneWay,
                Source = this,
                Path = type == DialogButtonType.Primary
                                ? new PropertyPath(nameof(PrimaryButtonCommand))
                                : new PropertyPath(nameof(SecondaryButtonCommand))
            };
            button.SetBinding(Button.CommandProperty, commandBinding);

            Binding commandParameterBinding = new Binding
            {
                Mode = BindingMode.OneWay,
                Source = this,
                Path = type == DialogButtonType.Primary
                                ? new PropertyPath(nameof(PrimaryButtonCommandParameter))
                                : new PropertyPath(nameof(SecondaryButtonCommandParameter))
            };
            button.SetBinding(Button.CommandParameterProperty, commandParameterBinding);

            Binding isEnabledBinding = new Binding { 
            
                Mode = BindingMode.OneWay,
                Source = this,
                Path = type == DialogButtonType.Primary
                        ? new PropertyPath(nameof(IsPrimaryButtonEnabled))
                        : new PropertyPath(nameof(IsSecondaryButtonEnabled))
            };
            button.SetBinding(Button.IsEnabledProperty, isEnabledBinding);

            if (type == DialogButtonType.Primary)
            {
                button.Click += PrimaryButton_Click;
            }
            else if(type == DialogButtonType.Secondary)
            {
                button.Click += SecondaryButton_Click;
            }

            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.VerticalAlignment = VerticalAlignment.Center;
            return button;
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            PrimaryButtonClick?.Invoke(this, e);
            Close(DialogCloseReason.PrimaryClicked);
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            SecondaryButtonClick?.Invoke(this, e);
            Close(DialogCloseReason.SecondaryClicked);
        }

        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate)GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }
        public string PrimaryButtonText
        {
            get { return (string)GetValue(PrimaryButtonTextProperty); }
            set { SetValue(PrimaryButtonTextProperty, value); }
        }

        public ICommand PrimaryButtonCommand
        {
            get { return (ICommand)GetValue(PrimaryButtonCommandProperty); }
            set { SetValue(PrimaryButtonCommandProperty, value); }
        }

        public object PrimaryButtonCommandParameter
        {
            get { return (object)GetValue(PrimaryButtonCommandParameterProperty); }
            set { SetValue(PrimaryButtonCommandParameterProperty, value); }
        }

        public bool IsPrimaryButtonEnabled
        {
            get { return (bool)GetValue(IsPrimaryButtonEnabledProperty); }
            set { SetValue(IsPrimaryButtonEnabledProperty, value); }
        }

        public string SecondaryButtonText
        {
            get { return (string)GetValue(SecondaryButtonTextProperty); }
            set { SetValue(SecondaryButtonTextProperty, value); }
        }

        public ICommand SecondaryButtonCommand
        {
            get { return (ICommand)GetValue(SecondaryButtonCommandProperty); }
            set { SetValue(SecondaryButtonCommandProperty, value); }
        }

        public object SecondaryButtonCommandParameter
        {
            get { return (object)GetValue(SecondaryButtonCommandParameterProperty); }
            set { SetValue(SecondaryButtonCommandParameterProperty, value); }
        }

        public bool IsSecondaryButtonEnabled
        {
            get { return (bool)GetValue(IsSecondaryButtonEnabledProperty); }
            set { SetValue(IsSecondaryButtonEnabledProperty, value); }
        }
    }
}
