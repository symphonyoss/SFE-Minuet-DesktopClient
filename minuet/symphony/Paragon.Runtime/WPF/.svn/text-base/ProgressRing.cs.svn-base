using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Paragon.Runtime.WPF
{
    public class ProgressRing : Control
    {
        public static readonly DependencyProperty BindableWidthProperty = DependencyProperty.Register("BindableWidth", typeof (double), typeof (ProgressRing), new PropertyMetadata(default(double), BindableWidthCallback));
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof (bool), typeof (ProgressRing), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsLargeProperty = DependencyProperty.Register("IsLarge", typeof (bool), typeof (ProgressRing), new PropertyMetadata(true));
        public static readonly DependencyProperty MaxSideLengthProperty = DependencyProperty.Register("MaxSideLength", typeof (double), typeof (ProgressRing), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register("EllipseDiameter", typeof (double), typeof (ProgressRing), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty EllipseOffsetProperty = DependencyProperty.Register("EllipseOffset", typeof (Thickness), typeof (ProgressRing), new PropertyMetadata(default(Thickness)));
        private List<Action> _deferredActions = new List<Action>();

        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ProgressRing),
                new FrameworkPropertyMetadata(typeof (ProgressRing)));

            VisibilityProperty.OverrideMetadata(
                typeof (ProgressRing),
                new FrameworkPropertyMetadata(
                    (ringObject, e) =>
                    {
                        if (e.NewValue != e.OldValue)
                        {
                            var ring = (ProgressRing) ringObject;
                            if ((Visibility) e.NewValue != Visibility.Visible)
                            {
                                ring.SetValue(IsActiveProperty, false);
                            }
                            else
                            {
                                ring.IsActive = true;
                            }
                        }
                    }));
        }

        public ProgressRing()
        {
            SizeChanged += OnSizeChanged;
        }

        public double BindableWidth
        {
            get { return (double) GetValue(BindableWidthProperty); }
            private set { SetValue(BindableWidthProperty, value); }
        }

        public double EllipseDiameter
        {
            get { return (double) GetValue(EllipseDiameterProperty); }
            private set { SetValue(EllipseDiameterProperty, value); }
        }

        public Thickness EllipseOffset
        {
            get { return (Thickness) GetValue(EllipseOffsetProperty); }
            private set { SetValue(EllipseOffsetProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public bool IsLarge
        {
            get { return (bool) GetValue(IsLargeProperty); }
            set { SetValue(IsLargeProperty, value); }
        }

        public double MaxSideLength
        {
            get { return (double) GetValue(MaxSideLengthProperty); }
            private set { SetValue(MaxSideLengthProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_deferredActions != null)
            {
                foreach (var action in _deferredActions)
                {
                    action();
                }
            }

            _deferredActions = null;
        }

        private static void BindableWidthCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var ring = dependencyObject as ProgressRing;
            if (ring == null)
            {
                return;
            }

            var action = new Action(() =>
            {
                ring.SetEllipseDiameter(
                    (double) dependencyPropertyChangedEventArgs.NewValue);
                ring.SetEllipseOffset(
                    (double) dependencyPropertyChangedEventArgs.NewValue);
                ring.SetMaxSideLength(
                    (double) dependencyPropertyChangedEventArgs.NewValue);
            });

            if (ring._deferredActions != null)
            {
                ring._deferredActions.Add(action);
            }
            else
            {
                action();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            BindableWidth = ActualWidth;
        }

        private void SetEllipseDiameter(double width)
        {
            EllipseDiameter = width/8;
        }

        private void SetEllipseOffset(double width)
        {
            EllipseOffset = new Thickness(0, width/2, 0, 0);
        }

        private void SetMaxSideLength(double width)
        {
            MaxSideLength = width <= 20 ? 20 : width;
        }
    }

    internal class WidthToMaxSideLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var width = (double) value;
                return width <= 20 ? 20 : width;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}