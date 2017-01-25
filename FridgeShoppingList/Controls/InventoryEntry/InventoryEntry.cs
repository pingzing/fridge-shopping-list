using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace FridgeShoppingList.Controls.InventoryEntry
{
    [TemplatePart(Name = ControlRootKey, Type = typeof(Grid))]
    [TemplatePart(Name = StateGroupKey, Type = typeof(VisualStateGroup))]    
    public sealed class InventoryEntry : Control
    {
        /// <summary>
        /// Identifies the <see cref="LeftCommand"/> property
        /// </summary>
        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.Register(nameof(LeftCommand), typeof(ICommand), typeof(InventoryEntry), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightCommand"/> property
        /// </summary>
        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.Register(nameof(RightCommand), typeof(ICommand), typeof(InventoryEntry), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="LeftCommandParameter"/> property
        /// </summary>
        public static readonly DependencyProperty LeftCommandParameterProperty =
            DependencyProperty.Register(nameof(LeftCommandParameter), typeof(object), typeof(InventoryEntry), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightCommandParameter"/> property
        /// </summary>
        public static readonly DependencyProperty RightCommandParameterProperty =
            DependencyProperty.Register(nameof(RightCommandParameter), typeof(object), typeof(InventoryEntry), new PropertyMetadata(null));

        private const string ControlRootKey = "Part_ControlRoot";
        private const string StateGroupKey = "Part_Common";        

        private const string MinimizedStateName = "Minimized";
        private const string MaximizedStateName = "Maximized";

        private VisualStateGroup _commonStateGroup;
        private Grid _controlRoot;
        private ItemsControl _expiryDates;     

        public InventoryEntry()
        {
            this.DefaultStyleKey = typeof(InventoryEntry);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _controlRoot = GetTemplateChild(ControlRootKey) as Grid;
            if (_controlRoot != null)
            {
                _controlRoot.Tapped -= ControlRoot_Tapped;
                _controlRoot.Tapped += ControlRoot_Tapped;
            }

            _commonStateGroup = GetTemplateChild(StateGroupKey) as VisualStateGroup;
            if (_commonStateGroup != null)
            {
                bool success = VisualStateManager.GoToState(this, MinimizedStateName, false);
            }           
        }

        private void ControlRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_commonStateGroup != null)
            {
                if (_commonStateGroup.CurrentState.Name == MinimizedStateName)
                {
                    VisualStateManager.GoToState(this, MaximizedStateName, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, MinimizedStateName, true);
                }
            }
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
