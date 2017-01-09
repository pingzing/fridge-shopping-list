

using System;
/**
Copyright(c) Microsoft Open Technologies, Inc.All rights reserved.
Modified by Michael Osthege (2016)
The MIT License(MIT)
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
**/
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TCD.Controls.Keyboard
{
    public partial class OnScreenKeyBoard : UserControl
    {
        #region Properties
        public object Host { get; set; }
        
        public KeyboardLayouts InitialLayout
        {
            get { return (this.DataContext as KeyboardViewModel).Layout; }
            set { (this.DataContext as KeyboardViewModel).Layout = value; }
        }
        public static readonly DependencyProperty InitialLayoutProperty = DependencyProperty.Register("InitialLayout", typeof(KeyboardLayouts), typeof(OnScreenKeyBoard), new PropertyMetadata(KeyboardLayouts.English));

        public static OnScreenKeyBoard AppKeyboard { get; set; }

        #endregion

        public OnScreenKeyBoard()
        {
            KeyboardViewModel vm = new KeyboardViewModel(this);
            DataContext = vm;
            InitializeComponent();
            AppKeyboard = this;

            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsEnabled))
                {
                    bool newIsEnabled = vm.IsEnabled;
                    if (newIsEnabled)
                    {
                        VisualStateManager.GoToState(this, CommonStates.States[1].Name, true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, CommonStates.States[0].Name, true);
                    }
                }
            };
        }

        public void RegisterTarget(TextBox control)
        {
            RegisterBox(control);
        }
        public void RegisterTarget(PasswordBox control)
        {
            RegisterBox(control);
        }
        private void RegisterBox(Control control)
        {
            control.GotFocus += delegate { (DataContext as KeyboardViewModel).TargetBox = control; };
            control.LostFocus += delegate { (DataContext as KeyboardViewModel).TargetBox = null; };
        }

        public void UnregisterTarget(TextBox control)
        {
            UnregisterBox(control);
        }

        public void UnregisterTarget(PasswordBox control)
        {
            UnregisterBox(control);
        }

        private void UnregisterBox(Control control)
        {
            control.GotFocus -= delegate { (DataContext as KeyboardViewModel).TargetBox = control; };
            control.LostFocus -= delegate { (DataContext as KeyboardViewModel).TargetBox = null; };
        }

        public static readonly DependencyProperty OpensKeyboardProperty =
            DependencyProperty.RegisterAttached(
                "OpensKeyboard",
                typeof(bool),
                typeof(bool),
                new PropertyMetadata(false, OnOpensKeyboardChanged)
            );

        private static void OnOpensKeyboardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            TextBox _thisText = d as TextBox;
            PasswordBox _thisPassword = d as PasswordBox;

            if (newValue)
            {
                if (_thisText != null)
                {
                    AppKeyboard.RegisterBox(_thisText);
                }
                else if(_thisPassword != null)
                {
                    AppKeyboard.RegisterBox(_thisPassword);
                }
            }
            else
            {
                if (_thisText != null)
                {
                    AppKeyboard.UnregisterBox(_thisText);
                }
                else if (_thisPassword != null)
                {
                    AppKeyboard.UnregisterBox(_thisPassword);
                }
            }
        }

        public static void SetOpensKeyboard(UIElement element, bool value)
        {
            element.SetValue(OpensKeyboardProperty, value);
            
        }
        public static bool GetOpensKeyboard(UIElement element)
        {
            return (bool)element.GetValue(OpensKeyboardProperty);
        }


    }
}

