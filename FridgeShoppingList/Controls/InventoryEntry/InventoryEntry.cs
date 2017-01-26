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
    [TemplatePart(Name = StateGroupKey, Type = typeof(VisualStateGroup))]    
    [TemplatePart(Name = MinimizedContainerKey, Type = typeof(LcarsSlidableListItem.LcarsSlidableListItem))]
    [TemplatePart(Name = TopContentHostKey, Type = typeof(Grid))]
    [TemplatePart(Name = MaximizedContainerKey, Type = typeof(Grid))]
    [TemplateVisualState(GroupName = StateGroupKey, Name = MinimizedStateName)]
    [TemplateVisualState(GroupName = StateGroupKey, Name = MaximizedStateName)]
    public sealed class InventoryEntry : Control
    {                
        private const string StateGroupKey = "Part_CommonStateGroup";
        private const string MinimizedContainerKey = "Part_MinimizedContainer";
        private const string TopContentHostKey = "Part_TopContentHost";
        private const string MaximizedContainerKey = "Part_MaximizedContainer";
        private const string MinimizedStateName = "Minimized";
        private const string MaximizedStateName = "Maximized";        

        private VisualStateGroup _commonStateGroup;
        private Grid _controlRoot;
        private LcarsSlidableListItem.LcarsSlidableListItem _minimizedContainer;
        private Grid _topContentHost;        

        public InventoryEntry()
        {
            this.DefaultStyleKey = typeof(InventoryEntry);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();           

            _commonStateGroup = GetTemplateChild(StateGroupKey) as VisualStateGroup;
            if (_commonStateGroup != null)
            {
                bool success = VisualStateManager.GoToState(this, MinimizedStateName, false);
            }

            _minimizedContainer = GetTemplateChild(MinimizedContainerKey) as LcarsSlidableListItem.LcarsSlidableListItem;
            if (_minimizedContainer != null)
            {
                _minimizedContainer.Tapped -= MinimizedContainer_Tapped;
                _minimizedContainer.Tapped += MinimizedContainer_Tapped;
            }                        
        }       

        private void MinimizedContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, MaximizedStateName, true);
            _topContentHost = GetTemplateChild(TopContentHostKey) as Grid;
            if (_topContentHost != null)
            {
                _topContentHost.Tapped -= TopContentHost_Tapped;
                _topContentHost.Tapped += TopContentHost_Tapped;
            }
        }

        private void TopContentHost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, MinimizedStateName, true);
        }               
    }
}
