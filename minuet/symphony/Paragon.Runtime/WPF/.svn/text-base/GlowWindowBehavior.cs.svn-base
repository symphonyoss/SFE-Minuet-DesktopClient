using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Paragon.Runtime.WPF
{
    public class GlowWindowBehavior : Behavior<Window>
    {
        private GlowWindow _bottom;
        private GlowWindow _left;
        private GlowWindow _right;
        private GlowWindow _top;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject.IsLoaded)
            {
                Init();
            }
            else
            {
                AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.IsVisibleChanged -= AssociatedObjectOnIsVisibleChanged;
            AssociatedObject.Closing -= AssociatedObjectOnClosing;

            _left.Close();
            _right.Close();
            _top.Close();
            _bottom.Close();
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs e)
        {
            Init();
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
        }

        private void Init()
        {
            _left = new GlowWindow(AssociatedObject, GlowDirection.Left);
            _right = new GlowWindow(AssociatedObject, GlowDirection.Right);
            _top = new GlowWindow(AssociatedObject, GlowDirection.Top);
            _bottom = new GlowWindow(AssociatedObject, GlowDirection.Bottom);

            Show();
            Update();

            StartOpacityStoryboard();
            AssociatedObject.IsVisibleChanged += AssociatedObjectOnIsVisibleChanged;
            AssociatedObject.Closing += AssociatedObjectOnClosing;
        }

        private void AssociatedObjectOnClosing(object sender, CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                AssociatedObject.IsVisibleChanged -= AssociatedObjectOnIsVisibleChanged;
            }
        }

        private void AssociatedObjectOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Update();

            if (!AssociatedObject.IsVisible)
            {
                SetOpacityTo(0);
            }
            else
            {
                StartOpacityStoryboard();
            }
        }

        private void Update()
        {
            if (_left != null
                && _right != null
                && _top != null
                && _bottom != null)
            {
                _left.Update();
                _right.Update();
                _top.Update();
                _bottom.Update();
            }
        }

        private void SetOpacityTo(double newOpacity)
        {
            if (_left != null
                && _right != null
                && _top != null
                && _bottom != null)
            {
                _left.Opacity = newOpacity;
                _right.Opacity = newOpacity;
                _top.Opacity = newOpacity;
                _bottom.Opacity = newOpacity;
            }
        }

        private void StartOpacityStoryboard()
        {
            if (_left != null && _left.OpacityStoryboard != null
                && _right != null && _right.OpacityStoryboard != null
                && _top != null && _top.OpacityStoryboard != null
                && _bottom != null && _bottom.OpacityStoryboard != null)
            {
                _left.BeginStoryboard(_left.OpacityStoryboard);
                _right.BeginStoryboard(_right.OpacityStoryboard);
                _top.BeginStoryboard(_top.OpacityStoryboard);
                _bottom.BeginStoryboard(_bottom.OpacityStoryboard);
            }
        }

        private void Show()
        {
            _left.Show();
            _right.Show();
            _top.Show();
            _bottom.Show();
        }
    }

    public class Glow : Control
    {
        public static readonly DependencyProperty GlowBrushProperty = DependencyProperty.Register("GlowBrush", typeof (SolidColorBrush), typeof (Glow), new UIPropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty IsGlowProperty = DependencyProperty.Register("IsGlow", typeof (bool), typeof (Glow), new UIPropertyMetadata(true));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof (Orientation), typeof (Glow), new UIPropertyMetadata(Orientation.Vertical));

        static Glow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (Glow), new FrameworkPropertyMetadata(typeof (Glow)));
        }

        public SolidColorBrush GlowBrush
        {
            get { return (SolidColorBrush) GetValue(GlowBrushProperty); }
            set { SetValue(GlowBrushProperty, value); }
        }

        public bool IsGlow
        {
            get { return (bool) GetValue(IsGlowProperty); }
            set { SetValue(IsGlowProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
    }

    public enum GlowDirection
    {
        Left,
        Right,
        Top,
        Bottom,
    }
}