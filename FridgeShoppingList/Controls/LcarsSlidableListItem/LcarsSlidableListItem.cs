using System;
using System.Windows.Input;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace FridgeShoppingList.Controls.LcarsSlidableListItem
{
    /// <summary>
    /// ContentControl providing functionality for sliding left or right to expose functions
    /// </summary>
    [TemplatePart(Name = PartContentGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PartCommandContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PartLeftCommandPanel, Type = typeof(StackPanel))]
    [TemplatePart(Name = PartRightCommandPanel, Type = typeof(StackPanel))]
    [TemplatePart(Name = EndcapPath, Type = typeof(Path))]
    public class LcarsSlidableListItem : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="ExtraSwipeThreshold"/> property
        /// </summary>
        public static readonly DependencyProperty ExtraSwipeThresholdProperty =
            DependencyProperty.Register(nameof(ExtraSwipeThreshold), typeof(int), typeof(LcarsSlidableListItem), new PropertyMetadata(default(int)));

        /// <summary>
        /// Identifies the <see cref="IsOffsetLimited"/> property
        /// </summary>
        public static readonly DependencyProperty IsOffsetLimitedProperty =
            DependencyProperty.Register(nameof(IsOffsetLimited), typeof(bool), typeof(LcarsSlidableListItem), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsRightCommandEnabled"/> property
        /// </summary>
        public static readonly DependencyProperty IsRightCommandEnabledProperty =
            DependencyProperty.Register(nameof(IsRightCommandEnabled), typeof(bool), typeof(LcarsSlidableListItem), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsLeftCommandEnabled"/> property
        /// </summary>
        public static readonly DependencyProperty IsLeftCommandEnabledProperty =
            DependencyProperty.Register(nameof(IsLeftCommandEnabled), typeof(bool), typeof(LcarsSlidableListItem), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="ActivationWidth"/> property
        /// </summary>
        public static readonly DependencyProperty ActivationWidthProperty =
            DependencyProperty.Register(nameof(ActivationWidth), typeof(double), typeof(LcarsSlidableListItem), new PropertyMetadata(80));

        /// <summary>
        /// Indeifies the <see cref="LeftIcon"/> property
        /// </summary>
        public static readonly DependencyProperty LeftIconProperty =
            DependencyProperty.Register(nameof(LeftIcon), typeof(Symbol), typeof(LcarsSlidableListItem), new PropertyMetadata(Symbol.Favorite));

        /// <summary>
        /// Identifies the <see cref="RightIcon"/> property
        /// </summary>
        public static readonly DependencyProperty RightIconProperty =
            DependencyProperty.Register(nameof(RightIcon), typeof(Symbol), typeof(LcarsSlidableListItem), new PropertyMetadata(Symbol.Delete));

        /// <summary>
        /// Identifies the <see cref="LeftLabel"/> property
        /// </summary>
        public static readonly DependencyProperty LeftLabelProperty =
            DependencyProperty.Register(nameof(LeftLabel), typeof(string), typeof(LcarsSlidableListItem), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="RightLabel"/> property
        /// </summary>
        public static readonly DependencyProperty RightLabelProperty =
            DependencyProperty.Register(nameof(RightLabel), typeof(string), typeof(LcarsSlidableListItem), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="LeftForeground"/> property
        /// </summary>
        public static readonly DependencyProperty LeftForegroundProperty =
            DependencyProperty.Register(nameof(LeftForeground), typeof(Brush), typeof(LcarsSlidableListItem), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        /// <summary>
        /// Identifies the <see cref="RightForeground"/> property
        /// </summary>
        public static readonly DependencyProperty RightForegroundProperty =
            DependencyProperty.Register(nameof(RightForeground), typeof(Brush), typeof(LcarsSlidableListItem), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        /// <summary>
        /// Identifies the <see cref="LeftBackground"/> property
        /// </summary>
        public static readonly DependencyProperty LeftBackgroundProperty =
            DependencyProperty.Register(nameof(LeftBackground), typeof(Brush), typeof(LcarsSlidableListItem), new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

        /// <summary>
        /// Identifies the <see cref="RightBackground"/> property
        /// </summary>
        public static readonly DependencyProperty RightBackgroundProperty =
            DependencyProperty.Register(nameof(RightBackground), typeof(Brush), typeof(LcarsSlidableListItem), new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        /// <summary>
        /// Identifies the <see cref="MouseSlidingEnabled"/> property
        /// </summary>
        public static readonly DependencyProperty MouseSlidingEnabledProperty =
            DependencyProperty.Register(nameof(MouseSlidingEnabled), typeof(bool), typeof(LcarsSlidableListItem), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="LeftCommand"/> property
        /// </summary>
        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.Register(nameof(LeftCommand), typeof(ICommand), typeof(LcarsSlidableListItem), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightCommand"/> property
        /// </summary>
        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.Register(nameof(RightCommand), typeof(ICommand), typeof(LcarsSlidableListItem), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="LeftCommandParameter"/> property
        /// </summary>
        public static readonly DependencyProperty LeftCommandParameterProperty =
            DependencyProperty.Register(nameof(LeftCommandParameter), typeof(object), typeof(LcarsSlidableListItem), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightCommandParameter"/> property
        /// </summary>
        public static readonly DependencyProperty RightCommandParameterProperty =
            DependencyProperty.Register(nameof(RightCommandParameter), typeof(object), typeof(LcarsSlidableListItem), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SwipeStatus"/> property
        /// </summary>
        public static readonly DependencyProperty SwipeStatusProperty =
            DependencyProperty.Register(nameof(SwipeStatus), typeof(object), typeof(LcarsSlidableListItem), new PropertyMetadata(SwipeStatus.Idle));

        /// <summary>
        /// Identifues the <see cref="IsPointerReleasedOnSwipingHandled"/> property
        /// </summary>
        public static readonly DependencyProperty IsPointerReleasedOnSwipingHandledProperty =
            DependencyProperty.Register(nameof(IsPointerReleasedOnSwipingHandled), typeof(bool), typeof(LcarsSlidableListItem), new PropertyMetadata(false));        

        /// <summary>
        /// Identifies the <see cref="EndcapBrush"/> property.
        /// </summary>
        public static readonly DependencyProperty EndcapBrushProperty =
            DependencyProperty.Register(nameof(EndcapBrush), typeof(Brush), typeof(LcarsSlidableListItem), new PropertyMetadata(new SolidColorBrush()));

        /// <summary>
        /// Occurs when SwipeStatus has changed
        /// </summary>
        public event TypedEventHandler<LcarsSlidableListItem, SwipeStatusChangedEventArgs> SwipeStatusChanged;

        private const string PartContentGrid = "ContentGrid";
        private const string PartCommandContainer = "CommandContainer";
        private const string PartLeftCommandPanel = "LeftCommandPanel";
        private const string PartRightCommandPanel = "RightCommandPanel";
        private const string EndcapPath = "EndcapPath";
        private const string StartcapPath = "StartcapPath";
        private const int FinishAnimationDuration = 150;
        private const int RightSnappedCommandMargin = 20;
        private const int LeftSnappedCommandMargin = 10;
        private const int AnimationSetDuration = 200;
        private Path _endcapPath;
        private Path _startcapPath;
        private CompositeTransform _endcapPathTransform;
        private CompositeTransform _startcapPathTransform;
        private Grid _contentGrid;
        private CompositeTransform _transform;
        private Grid _commandContainer;
        private CompositeTransform _commandContainerTransform;
        private DoubleAnimation _commandContainerClipTranslateAnimation;
        private StackPanel _leftCommandPanel;
        private CompositeTransform _leftCommandTransform;
        private StackPanel _rightCommandPanel;
        private CompositeTransform _rightCommandTransform;
        private DoubleAnimation _contentAnimation;
        private DoubleAnimation _endcapPathAnimation;
        private DoubleAnimation _startcapAnimation;
        private Storyboard _contentStoryboard;
        private AnimationSet _leftCommandAnimationSet;
        private AnimationSet _rightCommandAnimationSet;
        private bool _sizechangedSubscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LcarsSlidableListItem"/> class.
        /// Creates a new instance of <see cref="LcarsSlidableListItem"/>
        /// </summary>
        public LcarsSlidableListItem()
        {
            DefaultStyleKey = typeof(LcarsSlidableListItem);
        }

        /// <summary>
        /// Occurs when the user swipes to the left to activate the right action
        /// </summary>
        public event EventHandler RightCommandRequested;

        /// <summary>
        /// Occurs when the user swipes to the right to activate the left action
        /// </summary>
        public event EventHandler LeftCommandRequested;

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding
        /// layout pass) call <see cref="OnApplyTemplate"/>. In simplest terms, this means the method
        /// is called just before a UI element displays in an application. Override this
        /// method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            if (_contentGrid != null)
            {
                _contentGrid.PointerReleased -= ContentGrid_PointerReleased;
                _contentGrid.ManipulationStarted -= ContentGrid_ManipulationStarted;
                _contentGrid.ManipulationDelta -= ContentGrid_ManipulationDelta;
                _contentGrid.ManipulationCompleted -= ContentGrid_ManipulationCompleted;
            }

            _contentGrid = GetTemplateChild(PartContentGrid) as Grid;

            if (_contentGrid != null)
            {
                _contentGrid.PointerReleased += ContentGrid_PointerReleased;

                _transform = _contentGrid.RenderTransform as CompositeTransform;
                _contentGrid.ManipulationStarted += ContentGrid_ManipulationStarted;
                _contentGrid.ManipulationDelta += ContentGrid_ManipulationDelta;
                _contentGrid.ManipulationCompleted += ContentGrid_ManipulationCompleted;
            }

            _endcapPath = GetTemplateChild(EndcapPath) as Path;

            if(_endcapPath != null)
            {
                _endcapPathTransform = _endcapPath.RenderTransform as CompositeTransform;
                if (!_sizechangedSubscribed)
                {
                    _sizechangedSubscribed = true;
                    this.SizeChanged += LcarsSlidableListItem_SizeChanged;
                }
            }

            _startcapPath = GetTemplateChild(StartcapPath) as Path;

            if(_startcapPath != null)
            {
                _startcapPathTransform = _startcapPath.RenderTransform as CompositeTransform;
                if (!_sizechangedSubscribed)
                {
                    _sizechangedSubscribed = true;
                    this.SizeChanged += LcarsSlidableListItem_SizeChanged;
                }
            }

            base.OnApplyTemplate();
        }

        bool _startCapSizeChangedSubscribed;
        private void LcarsSlidableListItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size newSize = e.NewSize;

            PathGeometry geometry = new PathGeometry();
            PathFigureCollection figures = new PathFigureCollection();
            PathFigure halfCircleFigure = new PathFigure { StartPoint = new Point(0, 0) };
            halfCircleFigure.Segments.Add(new ArcSegment {
                Size = new Size(newSize.Height / 2, newSize.Height /2),
                IsLargeArc = false,
                Point = new Point(0, newSize.Height),
                RotationAngle = 0,
                SweepDirection = SweepDirection.Clockwise
            });
            figures.Add(halfCircleFigure);
            geometry.Figures = figures;
            _endcapPath.Data = geometry;
            _endcapPath.StrokeThickness = 0;

            PathGeometry startCapGeometry = new PathGeometry();
            PathFigureCollection startFigures = new PathFigureCollection();
            PathFigure startHalfCircleFigure = new PathFigure { StartPoint = new Point(0, 0) };
            startHalfCircleFigure.Segments.Add(new ArcSegment
            {
                Size = new Size(newSize.Height / 2, newSize.Height / 2),
                IsLargeArc = false,
                Point = new Point(0, newSize.Height),
                RotationAngle = 0,
                SweepDirection = SweepDirection.Clockwise
            });

            startFigures.Add(startHalfCircleFigure);
            startCapGeometry.Figures = startFigures;
            _startcapPath.Data = startCapGeometry;            
            _startcapPath.StrokeThickness = 0;

            if (!_startCapSizeChangedSubscribed)
            {
                _startCapSizeChangedSubscribed = true;
                _startcapPath.SizeChanged += (s, arg) => _startcapPath.Margin = new Thickness(-(_startcapPath.ActualWidth), 0, 0, 0);
            }
        }

        private void ContentGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (SwipeStatus != SwipeStatus.Idle && IsPointerReleasedOnSwipingHandled)
            {
                e.Handled = true;
            }
        }

        private void ContentGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if ((!MouseSlidingEnabled && e.PointerDeviceType == PointerDeviceType.Mouse) || (!IsLeftCommandEnabled && !IsRightCommandEnabled))
            {
                return;
            }

            if (_contentStoryboard == null)
            {
                _contentAnimation = new DoubleAnimation();
                Storyboard.SetTarget(_contentAnimation, _transform);
                Storyboard.SetTargetProperty(_contentAnimation, "TranslateX");
                _contentAnimation.To = 0;
                _contentAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(FinishAnimationDuration));

                _contentStoryboard = new Storyboard();
                _contentStoryboard.Children.Add(_contentAnimation);

                if (_endcapPath != null && _endcapPathTransform != null)
                {
                    _endcapPathAnimation = new DoubleAnimation();
                    Storyboard.SetTarget(_endcapPathAnimation, _endcapPathTransform);
                    Storyboard.SetTargetProperty(_endcapPathAnimation, "TranslateX");
                    _endcapPathAnimation.To = 0;
                    _endcapPathAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(FinishAnimationDuration));
                    _contentStoryboard.Children.Add(_endcapPathAnimation);
                }

                if(_startcapPath != null && _startcapPathTransform != null)
                {
                    _startcapAnimation = new DoubleAnimation();
                    Storyboard.SetTarget(_startcapAnimation, _startcapPathTransform);
                    Storyboard.SetTargetProperty(_startcapAnimation, "TranslateX");
                    _startcapAnimation.To = 0;                                        
                    _contentStoryboard.Children.Add(_startcapAnimation);
                }
            }
            

            if (_commandContainer == null)
            {
                _commandContainer = GetTemplateChild(PartCommandContainer) as Grid;
                if (_commandContainer != null)
                {
                    _commandContainer.Background = LeftBackground as SolidColorBrush;
                    _commandContainer.Clip = new RectangleGeometry();
                    _commandContainerTransform = new CompositeTransform();
                    _commandContainer.Clip.Transform = _commandContainerTransform;

                    _commandContainerClipTranslateAnimation = new DoubleAnimation();
                    Storyboard.SetTarget(_commandContainerClipTranslateAnimation, _commandContainerTransform);
                    Storyboard.SetTargetProperty(_commandContainerClipTranslateAnimation, "TranslateX");
                    _commandContainerClipTranslateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(FinishAnimationDuration));
                    _contentStoryboard.Children.Add(_commandContainerClipTranslateAnimation);
                }
            }

            if (_leftCommandPanel == null)
            {
                _leftCommandPanel = GetTemplateChild(PartLeftCommandPanel) as StackPanel;
                if (_leftCommandPanel != null)
                {
                    _leftCommandTransform = _leftCommandPanel.RenderTransform as CompositeTransform;
                }
            }

            if (_rightCommandPanel == null)
            {
                _rightCommandPanel = GetTemplateChild(PartRightCommandPanel) as StackPanel;
                if (_rightCommandPanel != null)
                {
                    _rightCommandTransform = _rightCommandPanel.RenderTransform as CompositeTransform;
                }
            }

            _contentStoryboard.Stop();            
            _commandContainer.Opacity = 0;
            _commandContainerTransform.TranslateX = 0;
            _transform.TranslateX = 0;
            _endcapPathTransform.TranslateX = 0;
            _startcapPathTransform.TranslateX = 0;
            SwipeStatus = SwipeStatus.Starting;
        }

        /// <summary>
        /// Handler for when slide manipulation is complete
        /// </summary>
        private void ContentGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if ((!MouseSlidingEnabled && e.PointerDeviceType == PointerDeviceType.Mouse) || (!IsLeftCommandEnabled && !IsRightCommandEnabled))
            {
                return;
            }

            var endcapX = _endcapPathTransform.TranslateX;
            _endcapPathAnimation.From = endcapX;

            var startcapX = _startcapPathTransform.TranslateX;
            _startcapAnimation.From = startcapX;

            var x = _transform.TranslateX;
            _contentAnimation.From = x;
            _commandContainerClipTranslateAnimation.From = 0;
            _commandContainerClipTranslateAnimation.To = -x;

            if(x > _startcapPath.ActualWidth)
            {
                double unitsPerMs = x / FinishAnimationDuration;
                double distanceTillStartCap = x - _startcapPath.ActualWidth;
                double millisecondsTillStartCap = distanceTillStartCap / unitsPerMs;
                _startcapAnimation.BeginTime = TimeSpan.FromMilliseconds(millisecondsTillStartCap);

                double millisecondsFromStartCapToEnd = _startcapPath.ActualWidth / unitsPerMs;
                _startcapAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(millisecondsFromStartCapToEnd));
            }
            else
            {
                _startcapAnimation.BeginTime = null;
                _startcapAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(FinishAnimationDuration));
            }

            _contentStoryboard.Begin();            

            if (SwipeStatus == SwipeStatus.SwipingPassedLeftThreshold)
            {
                RightCommandRequested?.Invoke(this, new EventArgs());
                RightCommand?.Execute(RightCommandParameter);
            }
            else if (SwipeStatus == SwipeStatus.SwipingPassedRightThreshold)
            {
                LeftCommandRequested?.Invoke(this, new EventArgs());
                LeftCommand?.Execute(LeftCommandParameter);
            }

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { SwipeStatus = SwipeStatus.Idle; }).AsTask();
        }
        
        /// <summary>
        /// Handler for when slide manipulation is underway
        /// </summary>
        private void ContentGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SwipeStatus == SwipeStatus.Idle)
            {
                return;
            }

            var newTranslationX = _transform.TranslateX + e.Delta.Translation.X;            
            bool swipingInDisabledArea = false;
            SwipeStatus newSwipeStatus = SwipeStatus.Idle;

            if (newTranslationX > 0)
            {
                // Swiping from left to right                
                if (!IsLeftCommandEnabled)
                {                    
                    // If swipe is not enabled, only allow swipe a very short distance
                    if (newTranslationX > 0)
                    {
                        swipingInDisabledArea = true;
                        newSwipeStatus = SwipeStatus.DisabledSwipingToRight;
                    }

                    double disabledSwipeDistance = _startcapPath.ActualWidth;
                    if (newTranslationX > disabledSwipeDistance)
                    {
                        newTranslationX = disabledSwipeDistance;
                    }
                }
                else if (IsOffsetLimited)
                {
                    // If offset is limited, there will be a limit how much swipe is possible.
                    // This will be the value of the command panel plus some threshold.
                    // This value can't be less than the ActivationWidth.
                    var swipeThreshold = _leftCommandPanel.ActualWidth + ExtraSwipeThreshold;
                    if (swipeThreshold < ActivationWidth)
                    {
                        swipeThreshold = ActivationWidth;
                    }

                    if (Math.Abs(newTranslationX) > swipeThreshold)
                    {
                        newTranslationX = swipeThreshold;
                    }
                }

                // Don't allow swiping more than almost the whole content grid width
                // (doing this will cause the control to change size).
                if (newTranslationX > (_contentGrid.ActualWidth - 4))
                {
                    newTranslationX = _contentGrid.ActualWidth - 4;
                }                
            }
            else
            {
                // Swiping from right to left
                if (!IsRightCommandEnabled)
                {
                    // If swipe is not enabled, only allow swipe a very short distance
                    if (newTranslationX < 0)
                    {
                        swipingInDisabledArea = true;
                        newSwipeStatus = SwipeStatus.DisabledSwipingToLeft;
                    }

                    if (newTranslationX < -16)
                    {
                        newTranslationX = -16;
                    }
                }
                else if (IsOffsetLimited)
                {
                    // If offset is limited, there will be a limit how much swipe is possible.
                    // This will be the value of the command panel plus some threshold.
                    // This value can't be less than the ActivationWidth.
                    var swipeThreshold = _rightCommandPanel.ActualWidth + ExtraSwipeThreshold;
                    if (swipeThreshold < ActivationWidth)
                    {
                        swipeThreshold = ActivationWidth;
                    }

                    if (Math.Abs(newTranslationX) > swipeThreshold)
                    {
                        newTranslationX = -swipeThreshold;
                    }
                }

                // Don't allow swiping more than almost the whole content grid width
                // (doing this will cause the control to change size).
                if (newTranslationX < -(_contentGrid.ActualWidth + (_endcapPath?.ActualWidth ?? 0) + (_startcapPath?.ActualWidth ?? 0) - 4))
                {
                    newTranslationX = -(_contentGrid.ActualWidth + (_endcapPath?.ActualWidth ?? 0) + (_startcapPath?.ActualWidth ?? 0) - 4);
                }
            }           

            bool hasPassedThreshold = !swipingInDisabledArea && Math.Abs(newTranslationX) >= ActivationWidth;

            if (swipingInDisabledArea)
            {
                // Don't show any command if swiping in disabled area.
                _commandContainer.Opacity = 0;
                _leftCommandPanel.Opacity = 0;
                _rightCommandPanel.Opacity = 0;
            }
            else if (newTranslationX > 0)
            {
                // If swiping from left to right, show left command panel.
                _rightCommandPanel.Opacity = 0;

                _commandContainer.Background = LeftBackground as SolidColorBrush;
                (_commandContainer.RenderTransform as CompositeTransform).TranslateX = _startcapPath.ActualWidth;
                _commandContainer.Opacity = 1;
                _leftCommandPanel.Opacity = 1;

                _commandContainer.Clip.Rect = new Rect(0, 0, Math.Max(newTranslationX - 1, 0), _commandContainer.ActualHeight);

                if (newTranslationX < ActivationWidth)
                {
                    _leftCommandAnimationSet?.Stop();
                    _leftCommandPanel.RenderTransform = _leftCommandTransform;
                    _leftCommandTransform.TranslateX = newTranslationX / 2;

                    newSwipeStatus = SwipeStatus.SwipingToRightThreshold;
                }
                else
                {
                    if (SwipeStatus == SwipeStatus.SwipingToRightThreshold)
                    {
                        // The control was just put below the threshold.
                        // Run an animation to put the text and icon
                        // in the correct position.
                        _leftCommandAnimationSet = _leftCommandPanel.Offset((float)(LeftSnappedCommandMargin - _leftCommandTransform.TranslateX), duration: AnimationSetDuration);
                        _leftCommandAnimationSet.Start();
                    }
                    else if (SwipeStatus != SwipeStatus.SwipingPassedRightThreshold)
                    {
                        // This will cover extrem cases when previous state wasn't
                        // below threshold.
                        _leftCommandAnimationSet?.Stop();
                        _leftCommandPanel.RenderTransform = _leftCommandTransform;
                        _leftCommandTransform.TranslateX = LeftSnappedCommandMargin;
                    }

                    newSwipeStatus = SwipeStatus.SwipingPassedRightThreshold;
                }
            }
            else
            {
                // If swiping from right to left, show right command panel.
                _leftCommandPanel.Opacity = 0;

                _commandContainer.Background = RightBackground as SolidColorBrush;
                (_commandContainer.RenderTransform as CompositeTransform).TranslateX = 0;
                _commandContainer.Opacity = 1;
                _rightCommandPanel.Opacity = 1;

                _commandContainer.Clip.Rect = new Rect(_commandContainer.ActualWidth + newTranslationX + 1, 0, Math.Max(-newTranslationX - 1, 0), _commandContainer.ActualHeight);

                if (-newTranslationX < ActivationWidth)
                {
                    _rightCommandAnimationSet?.Stop();
                    _rightCommandPanel.RenderTransform = _rightCommandTransform;
                    _rightCommandTransform.TranslateX = newTranslationX / 2;

                    newSwipeStatus = SwipeStatus.SwipingToLeftThreshold;
                }
                else
                {
                    if (SwipeStatus == SwipeStatus.SwipingToLeftThreshold)
                    {
                        // The control was just put below the threshold.
                        // Run an animation to put the text and icon
                        // in the correct position.
                        _rightCommandAnimationSet = _rightCommandPanel.Offset((float)(-RightSnappedCommandMargin - _rightCommandTransform.TranslateX), duration: AnimationSetDuration);
                        _rightCommandAnimationSet.Start();
                    }
                    else if (SwipeStatus != SwipeStatus.SwipingPassedLeftThreshold)
                    {
                        // This will cover extrem cases when previous state wasn't
                        // below threshold.
                        _rightCommandAnimationSet?.Stop();
                        _rightCommandPanel.RenderTransform = _rightCommandTransform;
                        _rightCommandTransform.TranslateX = -RightSnappedCommandMargin;
                    }

                    newSwipeStatus = SwipeStatus.SwipingPassedLeftThreshold;
                }
            }

            _transform.TranslateX = newTranslationX;            
            if (newTranslationX > 0)
            {
                _endcapPathTransform.TranslateX = newTranslationX;
                if(newTranslationX >= _startcapPath.ActualWidth )
                {
                    _startcapPathTransform.TranslateX = _startcapPath.ActualWidth;
                }
                else
                {
                    _startcapPathTransform.TranslateX = newTranslationX;
                }
            }
            else
            {
                _startcapPathTransform.TranslateX = newTranslationX;
                if (_endcapPathTransform.TranslateX + newTranslationX != 0)
                {
                    _endcapPathTransform.TranslateX = 0;
                }
            }
            SwipeStatus = newSwipeStatus;
        }

        /// <summary>
        /// Gets or sets the amount of extra pixels for swipe threshold when <see cref="IsOffsetLimited"/> is enabled.
        /// </summary>
        public int ExtraSwipeThreshold
        {
            get { return (int)GetValue(ExtraSwipeThresholdProperty); }
            set { SetValue(ExtraSwipeThresholdProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether maximum swipe offset is limited or not.
        /// </summary>
        public bool IsOffsetLimited
        {
            get { return (bool)GetValue(IsOffsetLimitedProperty); }
            set { SetValue(IsOffsetLimitedProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether right command is enabled or not.
        /// </summary>
        public bool IsRightCommandEnabled
        {
            get { return (bool)GetValue(IsRightCommandEnabledProperty); }
            set { SetValue(IsRightCommandEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether left command is enabled or not.
        /// </summary>
        public bool IsLeftCommandEnabled
        {
            get { return (bool)GetValue(IsLeftCommandEnabledProperty); }
            set { SetValue(IsLeftCommandEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the amount of pixels the content needs to be swiped for an
        /// action to be requested
        /// </summary>
        public double ActivationWidth
        {
            get { return (double)GetValue(ActivationWidthProperty); }
            set { SetValue(ActivationWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the left icon symbol
        /// </summary>
        public Symbol LeftIcon
        {
            get { return (Symbol)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right icon symbol
        /// </summary>
        public Symbol RightIcon
        {
            get { return (Symbol)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        /// <summary>
        /// Gets or sets the left label
        /// </summary>
        public string LeftLabel
        {
            get { return (string)GetValue(LeftLabelProperty); }
            set { SetValue(LeftLabelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right label
        /// </summary>
        public string RightLabel
        {
            get { return (string)GetValue(RightLabelProperty); }
            set { SetValue(RightLabelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the left foreground color
        /// </summary>
        public Brush LeftForeground
        {
            get { return (Brush)GetValue(LeftForegroundProperty); }
            set { SetValue(LeftForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right foreground color
        /// </summary>
        public Brush RightForeground
        {
            get { return (Brush)GetValue(RightForegroundProperty); }
            set { SetValue(RightForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the left background color
        /// </summary>
        public Brush LeftBackground
        {
            get { return (Brush)GetValue(LeftBackgroundProperty); }
            set { SetValue(LeftBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right background color
        /// </summary>
        public Brush RightBackground
        {
            get { return (Brush)GetValue(RightBackgroundProperty); }
            set { SetValue(RightBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it has the ability to slide the control with the mouse. False by default
        /// </summary>
        public bool MouseSlidingEnabled
        {
            get { return (bool)GetValue(MouseSlidingEnabledProperty); }
            set { SetValue(MouseSlidingEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ICommand for left command request
        /// </summary>
        public ICommand LeftCommand
        {
            get
            {
                return (ICommand)GetValue(LeftCommandProperty);
            }

            set
            {
                SetValue(LeftCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the ICommand for right command request
        /// </summary>
        public ICommand RightCommand
        {
            get
            {
                return (ICommand)GetValue(RightCommandProperty);
            }

            set
            {
                SetValue(RightCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the CommandParameter for left command request
        /// </summary>
        public object LeftCommandParameter
        {
            get
            {
                return GetValue(LeftCommandParameterProperty);
            }

            set
            {
                SetValue(LeftCommandParameterProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the CommandParameter for right command request
        /// </summary>
        public object RightCommandParameter
        {
            get
            {
                return GetValue(RightCommandParameterProperty);
            }

            set
            {
                SetValue(RightCommandParameterProperty, value);
            }
        }

        /// <summary>
        /// Gets the SwipeStatus for current swipe status
        /// </summary>
        public SwipeStatus SwipeStatus
        {
            get
            {
                return (SwipeStatus)GetValue(SwipeStatusProperty);
            }

            private set
            {
                var oldValue = SwipeStatus;

                if (value != oldValue)
                {
                    SetValue(SwipeStatusProperty, value);

                    var eventArguments = new SwipeStatusChangedEventArgs()
                    {
                        OldValue = oldValue,
                        NewValue = value
                    };

                    SwipeStatusChanged?.Invoke(this, eventArguments);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the PointerReleased event is handled when swiping.
        /// Set this to <value>true</value> to prevent ItemClicked or selection to occur when swiping in a <see cref="ListView"/>
        /// </summary>
        public bool IsPointerReleasedOnSwipingHandled
        {
            get { return (bool)GetValue(IsPointerReleasedOnSwipingHandledProperty); }
            set { SetValue(IsPointerReleasedOnSwipingHandledProperty, value); }
        }

        /// <summary>
        /// Determines the color used for the endcap at the far right of the list item.
        /// </summary>
        public Brush EndcapBrush
        {
            get { return (Brush)GetValue(EndcapBrushProperty); }
            set { SetValue(EndcapBrushProperty, value); }
        }
    }
}