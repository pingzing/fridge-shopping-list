using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FridgeShoppingList.Extensions
{
    public class VisualExtensions : DependencyObject
    {
        public static readonly DependencyProperty AnimatedVisibilityProperty = DependencyProperty.RegisterAttached(
           "AnimatedVisibility",
           typeof(Visibility),
           typeof(VisualExtensions),
           new PropertyMetadata(Visibility.Visible, OnAnimatedVisibilityChanged));

        private static async void OnAnimatedVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as UIElement;
            if (_this == null)
            {
                throw new ArgumentException("AnimatedVisibility can only be attached to elements that derive from UIElement.");
            }

            Visibility oldVis = (Visibility)e.OldValue;
            Visibility newVis = (Visibility)e.NewValue;

            if(oldVis == newVis)
            {
                return;
            }

            // If backing Visibility and AnimatedVisiblity don't match, sync them up and just bail out
            if (_this.Visibility != oldVis)
            {
                SetAnimatedVisibility(_this, _this.Visibility);
            }

                var animatedType = _this.GetValue(AnimatedVisibilityTypeProperty) as AnimatedVisibilityType?;
            if (animatedType == null)
            {
                animatedType = AnimatedVisibilityType.Fade;
            }

            if (newVis == Visibility.Visible)
            {                
                _this.Opacity = 0;
                _this.Visibility = Visibility.Visible;

                switch (animatedType)
                {
                    case AnimatedVisibilityType.Fade:
                        await _this.Fade(1, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.FadeWhoosh:
                        await _this.Offset(0, 200, 0, 0).StartAsync();
                        await _this.Fade(1, 333).Offset(0, 0, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.FadeZoom:
                        await _this.Scale(0, 0, 0, 0, 0).StartAsync();
                        await _this.Fade(1, 333).Scale(1, 1, 0, 0, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.Whoosh:
                        await _this.Offset(0, 200, 0, 0).Fade(1, 0, 0).StartAsync();
                        await _this.Offset(0, 0, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.Zoom:
                        await _this.Scale(0, 0, 0, 0, 0, 0).Fade(1, 0, 0).StartAsync();
                        await _this.Scale(1, 1, 0, 0, 333).StartAsync();
                        break;                        
                }
            }
            else
            {
                switch (animatedType)
                {
                    case AnimatedVisibilityType.Fade:
                        await _this.Fade(0, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.FadeWhoosh:                        
                        await _this.Fade(0, 333).Offset(0, 200, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.FadeZoom:                        
                        await _this.Fade(0, 333).Scale(0, 0, 0, 0, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.Whoosh:                        
                        await _this.Offset(0, 200, 333).StartAsync();
                        break;
                    case AnimatedVisibilityType.Zoom:                        
                        await _this.Scale(0, 0, 0, 0, 333).StartAsync();
                        break;
                }
                _this.Visibility = Visibility.Collapsed;
            }

        }

        public static Visibility GetAnimatedVisibility(UIElement element)
        {
            return (Visibility)element.GetValue(AnimatedVisibilityProperty);
        }

        public static void SetAnimatedVisibility(UIElement element, Visibility vis)
        {
            element.SetValue(AnimatedVisibilityProperty, vis);            
        }

        public enum AnimatedVisibilityType
        {
            Fade,
            Zoom,
            FadeZoom,
            Whoosh,
            FadeWhoosh
        }

        public static readonly DependencyProperty AnimatedVisibilityTypeProperty = DependencyProperty.RegisterAttached(
            "AnimatedVisibilityType",
            typeof(AnimatedVisibilityType),
            typeof(VisualExtensions),
            new PropertyMetadata(AnimatedVisibilityType.Fade));

        public static AnimatedVisibilityType GetAnimatedVisibilityType(UIElement element)
        {
            return (AnimatedVisibilityType)element.GetValue(AnimatedVisibilityTypeProperty);
        }

        public static void SetAnimatedVisibilityType(UIElement element, AnimatedVisibilityType visType)
        {
            element.SetValue(AnimatedVisibilityTypeProperty, visType);
        }
    }
}
