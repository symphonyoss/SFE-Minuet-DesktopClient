//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using Paragon.Plugins.Notifications.Views;
using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Paragon.Plugins.Notifications.Controls
{
    public class Notification : Control
    {
        public static readonly DependencyProperty BlinkBackgroundProperty =
            DependencyProperty.Register("BlinkBackground", typeof (Brush), typeof (Notification), new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.Transparent),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        public static readonly DependencyProperty CanBlinkProperty =
            DependencyProperty.Register("CanBlink", typeof (bool), typeof (Notification), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CanPlaySoundProperty =
            DependencyProperty.Register("CanPlaySound", typeof (bool), typeof (Notification), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof (ICommand), typeof (Notification), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof (ICommand), typeof (Notification), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty IconUrlProperty =
            DependencyProperty.Register("IconUrl", typeof (string), typeof (Notification), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof (string), typeof (Notification), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof (Position), typeof (Notification), new PropertyMetadata(default(Position)));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (Notification), new PropertyMetadata(default(string)));

        private readonly DispatcherTimer blinkTimer;

        private Object lockNotification = new Object();  
        
        private NotificationWindow notificationWindow;

        private readonly Storyboard loadedStoryboard;
        private readonly Storyboard mouseEnterStoryboard;
        private readonly Storyboard mouseLeaveStoryboard;
        private bool blinkActive;
        private Brush originalBackground;

        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (Notification),
                new FrameworkPropertyMetadata(typeof (Notification)));
        }

        public Notification()
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform());

            RenderTransform = transformGroup;

            loadedStoryboard = new Storyboard();
            mouseEnterStoryboard = new Storyboard();
            mouseLeaveStoryboard = new Storyboard();

            blinkTimer = new DispatcherTimer();
            blinkTimer.Interval = TimeSpans.Blink;
            blinkTimer.Tick += OnBlinkTimer;

            Loaded += (sender, args) =>
            {
                lock (lockNotification)
                {
                    originalBackground = Background;

                    var backgroundOverBrush = GetBackgroundOver();
                    var color = GetColor(backgroundOverBrush);

                    var mouseEnterAnimation = new BackgroundColorAnimationBuilder()
                        .Duration(AnimationDurations.MouseEvents)
                        .To(color)
                        .Build();

                    var mouseLeaveAnimation = new BackgroundColorAnimationBuilder()
                        .Duration(AnimationDurations.MouseEvents)
                        .From(color)
                        .Build();

                    mouseEnterStoryboard.Children.Add(mouseEnterAnimation);
                    mouseLeaveStoryboard.Children.Add(mouseLeaveAnimation);

                    if (CanBlink)
                    {
                        blinkTimer.Start();
                    }

                    if (CanPlaySound)
                    {
                        SystemSounds.Asterisk.Play();
                    }

                    var loadAnimation = new TranslateXAnimationBuilder()
                        .Duration(AnimationDurations.Loaded)
                        .From(GetOffScreenXPosition())
                        .Build();

                    loadedStoryboard.Children.Add(loadAnimation);
                    loadedStoryboard.Begin(this, true);

                    //resize and move NotificationWindow.
                    notificationWindow = GetNotificationWindow(this);
                    if ((notificationWindow.Height + this.ActualHeight + 8) > 0)                    
                        notificationWindow.Height += this.ActualHeight + 8;
                    else
                        notificationWindow.Height = 0;
                    
                    notificationWindow.MoveNotificationWindow();

                }
            };

            Unloaded += (sender, args) =>
            {
                lock (lockNotification)
                {
                    if (blinkTimer.IsEnabled)
                    {
                        blinkTimer.Stop();
                    }

                    //resize and move NotificationWindow.
                    if ((notificationWindow.Height - this.ActualHeight + 8) > 0)
                        notificationWindow.Height -= this.ActualHeight + 8;
                    else
                        notificationWindow.Height = 0;

                    notificationWindow.MoveNotificationWindow();
                }
            };

            MouseEnter += (sender, args) =>
            {
                if (CanBlink)
                {
                    blinkTimer.Stop();
                }

                Background = originalBackground;

                mouseEnterStoryboard.Begin(this, true);
            };

            MouseLeave += (sender, args) =>
            {
                Background = originalBackground;

                mouseLeaveStoryboard.Begin(this, true);

                if (CanBlink)
                {
                    blinkTimer.Start();
                }
            };
        }

        private NotificationWindow GetNotificationWindow(DependencyObject obj){
            //find NotificationWindow.
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent.GetType() != typeof(NotificationWindow))
            {
                parent = GetNotificationWindow(parent);
            }
            return (NotificationWindow)parent;
        }

        public Brush BlinkBackground
        {
            get { return (Brush) GetValue(BlinkBackgroundProperty); }
            set { SetValue(BlinkBackgroundProperty, value); }
        }

        public bool CanBlink
        {
            get { return (bool) GetValue(CanBlinkProperty); }
            set { SetValue(CanBlinkProperty, value); }
        }

        public bool CanPlaySound
        {
            get { return (bool) GetValue(CanPlaySoundProperty); }
            set { SetValue(CanPlaySoundProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ICommand CloseCommand
        {
            get { return (ICommand) GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        public string IconUrl
        {
            get { return (string) GetValue(IconUrlProperty); }
            set { SetValue(IconUrlProperty, value); }
        }

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public Position Position
        {
            get { return (Position) GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private Brush GetBackgroundOver()
        {
            var brush = (SolidColorBrush) Background;
            var color = brush.Color;
            var overColor = Color.FromArgb(255, color.R, color.G, color.B);

            return new SolidColorBrush(overColor);
        }

        private static Color GetColor(Brush brush)
        {
            return GetColor(brush as SolidColorBrush);
        }

        private static Color GetColor(SolidColorBrush brush)
        {
            if (brush == null)
            {
                return Colors.Transparent;
            }
            return brush.Color;
        }

        private int GetOffScreenXPosition()
        {
            var offscreen = -400;

            if (Position == Position.TopRight || Position == Position.BottomRight)
            {
                offscreen = 400;
            }

            return offscreen;
        }

        private void OnBlinkTimer(object sender, EventArgs args)
        {
            blinkActive = !blinkActive;

            if (blinkActive && !IsMouseOver)
            {
                Background = BlinkBackground;
            }
            else
            {
                Background = originalBackground;
            }
        }

        private static class AnimationDurations
        {
            public static readonly Duration MouseEvents = new Duration(TimeSpans.MouseEvents);
            public static readonly Duration Loaded = new Duration(TimeSpans.Loaded);
        }

        private class BackgroundColorAnimationBuilder
        {
            private Duration duration;
            private Color? from;
            private Color? to;

            public BackgroundColorAnimationBuilder Duration(Duration duration)
            {
                this.duration = duration;
                return this;
            }

            public BackgroundColorAnimationBuilder From(Color color)
            {
                @from = color;
                return this;
            }

            public BackgroundColorAnimationBuilder To(Color color)
            {
                to = color;
                return this;
            }

            public ColorAnimation Build()
            {
                var animation = new ColorAnimation {Duration = duration, To = to, From = @from};

                Storyboard.SetTargetProperty(animation, new PropertyPath("(Background).(SolidColorBrush.Color)"));

                return animation;
            }
        }

        public static class TimeSpans
        {
            public static readonly TimeSpan Blink = TimeSpan.FromMilliseconds(800);
            public static readonly TimeSpan MouseEvents = TimeSpan.FromMilliseconds(150);
            public static readonly TimeSpan Loaded = TimeSpan.FromMilliseconds(300);
        }

        private class TranslateXAnimationBuilder
        {
            private Duration duration;
            private int from;

            public TranslateXAnimationBuilder Duration(Duration duration)
            {
                this.duration = duration;
                return this;
            }

            public TranslateXAnimationBuilder From(int from)
            {
                this.from = from;
                return this;
            }

            public DoubleAnimation Build()
            {
                var animation = new DoubleAnimation {Duration = duration, From = from};

                Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

                return animation;
            }
        }
    }
}