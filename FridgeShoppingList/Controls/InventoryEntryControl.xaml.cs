using FridgeShoppingList.ViewModels.ControlViewModels;
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
    public sealed partial class InventoryEntryControl : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="LeftCommand"/> property
        /// </summary>
        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.Register(nameof(LeftCommand), typeof(ICommand), typeof(InventoryEntryControl), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightCommand"/> property
        /// </summary>
        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.Register(nameof(RightCommand), typeof(ICommand), typeof(InventoryEntryControl), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="LeftCommandParameter"/> property
        /// </summary>
        public static readonly DependencyProperty LeftCommandParameterProperty =
            DependencyProperty.Register(nameof(LeftCommandParameter), typeof(object), typeof(InventoryEntryControl), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightCommandParameter"/> property
        /// </summary>
        public static readonly DependencyProperty RightCommandParameterProperty =
            DependencyProperty.Register(nameof(RightCommandParameter), typeof(object), typeof(InventoryEntryControl), new PropertyMetadata(null));

        public InventoryEntryViewModel ViewModel { get; private set; }

        public InventoryEntryControl()
        {            
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => 
            {
                if (e.NewValue != null)
                {
                    ViewModel = DataContext as InventoryEntryViewModel;
                    Bindings.Update();

                    if (ViewModel.IsExpiredAnimationPlaying)
                    {
                        ExpiredGlowStoryboard.Begin();
                    }

                    ViewModel.PropertyChanged += (vmS, vmE) =>
                    {
                        if (vmE.PropertyName == "IsExpiredAnimationPlaying")
                        {
                            bool newValue = ViewModel.IsExpiredAnimationPlaying;
                            if (newValue)
                            {
                                ExpiredGlowStoryboard.Begin();
                            }
                            else
                            {
                                ExpiredGlowStoryboard.Stop();
                            }
                        }
                    };
                }
            };                 
        }

        /// <summary>
        /// Gets or sets the ICommand for left command request
        /// </summary>
        public ICommand LeftCommand
        {
            get { return (ICommand)GetValue(LeftCommandProperty); }
            set { SetValue(LeftCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ICommand for right command request
        /// </summary>
        public ICommand RightCommand
        {
            get { return (ICommand)GetValue(RightCommandProperty); }
            set { SetValue(RightCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the CommandParameter for left command request
        /// </summary>
        public object LeftCommandParameter
        {
            get { return GetValue(LeftCommandParameterProperty); }
            set { SetValue(LeftCommandParameterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the CommandParameter for right command request
        /// </summary>
        public object RightCommandParameter
        {
            get { return GetValue(RightCommandParameterProperty); }
            set { SetValue(RightCommandParameterProperty, value); }
        }
    }
}
