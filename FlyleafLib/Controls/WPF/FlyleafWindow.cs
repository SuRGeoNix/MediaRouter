﻿/* This class is based on https://github.com/videolan/libvlcsharp/tree/3.x/src/LibVLCSharp.WPF */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

namespace FlyleafLib.Controls.WPF
{
    public class FlyleafWindow : Window
    {
        public   Window             WindowBack { get; private set; }
        readonly WindowsFormsHost   windowsFormsHost;
        public   VideoView          VideoView;
        readonly Point              _zeroPoint   = new Point(0, 0);

        private readonly Grid grid = new Grid();

        internal void SetContent(UIElement newContent)
        {
            grid.Children.Clear();
            if (newContent == null) return;
            grid.Children.Add(newContent);
        }

        public FlyleafWindow(WindowsFormsHost windowsFormsHost)
        {
            //System.Diagnostics.Debug.WriteLine("FlyleafWindow");

            Title               = "FlyleafWindow";
            Height              = 300;
            Width               = 300;
            WindowStyle         = WindowStyle.None;
            Background          = Brushes.Transparent;
            ResizeMode          = ResizeMode.NoResize;
            AllowsTransparency  = true;
            ShowInTaskbar       = false;
            Content             = grid;

            this.windowsFormsHost                       = windowsFormsHost;
            this.windowsFormsHost.DataContextChanged    += WFH_DataContextChanged;
            this.windowsFormsHost.Loaded                += WFH_Loaded;
            this.windowsFormsHost.Unloaded              += WFH_Unloaded;

            Closed += FlyleafWindow_Closed;
        }

        void WFH_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("WFH_DataContextChanged"); 
            if (e.NewValue == null) return;
            DataContext = e.NewValue;
        }

        void WFH_Unloaded(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("WFH_Unloaded");
            
            windowsFormsHost.SizeChanged    -= Wndhost_SizeChanged;

            if (WindowBack != null)
            {
                WindowBack.Closing          -= Wndhost_Closing;
                WindowBack.LocationChanged  -= Wndhost_LocationChanged;
            }

            Hide();
        }

        void WFH_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("WFH_Loaded");

            if (WindowBack != null)
            {
                WindowBack.Closing          += Wndhost_Closing;
                WindowBack.LocationChanged  += Wndhost_LocationChanged;
                windowsFormsHost.SizeChanged+= Wndhost_SizeChanged;
            }
            else
            {
                WindowBack = GetWindow(windowsFormsHost);
                if (WindowBack == null) return;

                Owner = WindowBack;

                WindowBack.Closing          += Wndhost_Closing;
                windowsFormsHost.SizeChanged+= Wndhost_SizeChanged;
                WindowBack.LocationChanged  += Wndhost_LocationChanged;
            }

            var locationFromScreen  = windowsFormsHost.PointToScreen(_zeroPoint);
            var source              = PresentationSource.FromVisual(WindowBack);
            var targetPoints        = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
            Left                    = targetPoints.X;
            Top                     = targetPoints.Y;
            var size                = new Point(windowsFormsHost.ActualWidth, windowsFormsHost.ActualHeight);
            Height                  = size.Y;
            Width                   = size.X;
            Show();
            WindowBack.Focus();
        }

        public void Wndhost_LocationChanged(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Wndhost_LocationChanged");

            var locationFromScreen  = windowsFormsHost.PointToScreen(_zeroPoint);
            var source              = PresentationSource.FromVisual(WindowBack);
            var targetPoints        = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
            Left                    = targetPoints.X;
            Top                     = targetPoints.Y;
        }

        public void Wndhost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Wndhost_SizeChanged");
            var source = PresentationSource.FromVisual(WindowBack);
            if (source == null)
            {
                return;
            }

            var locationFromScreen  = windowsFormsHost.PointToScreen(_zeroPoint);
            var targetPoints        = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
            Left                    = targetPoints.X;
            Top                     = targetPoints.Y;
            var size                = new Point(windowsFormsHost.ActualWidth, windowsFormsHost.ActualHeight);
            Height                  = size.Y;
            Width                   = size.X;
        }

        void Wndhost_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Wndhost_Closing");

            if (!Master.PreventAutoDispose) Dispose();
        }

        bool disposed = false;
        internal void Dispose()
        {
            //System.Diagnostics.Debug.WriteLine("FlyleafWindow_Dispose");

            lock (this)
            {
                if (disposed) return;

                if (windowsFormsHost != null)
                {
                    windowsFormsHost.DataContextChanged -= WFH_DataContextChanged;
                    windowsFormsHost.Loaded             -= WFH_Loaded;
                    windowsFormsHost.Unloaded           -= WFH_Unloaded;
                    windowsFormsHost.SizeChanged        -= Wndhost_SizeChanged;
                }

                if (WindowBack != null)
                {
                    WindowBack.Closing          -= Wndhost_Closing;
                    WindowBack.LocationChanged  -= Wndhost_LocationChanged;
                }

                Resources.MergedDictionaries.Clear();
                Resources.Clear();
                Template.Resources.MergedDictionaries.Clear();
                Content = null;
                SetContent(null);
                DataContext = null;
                grid.Children.Clear();
                windowsFormsHost?.Dispose();
                WindowBack = null;

                disposed = true;
            }
            VideoView?.Dispose();
            VideoView = null;

            Close();
        }

        private void FlyleafWindow_Closed(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("FlyleafWindow_Closed");

            if (!Master.PreventAutoDispose) Dispose();
        }

        protected override void OnKeyDown(KeyEventArgs e) { if (e.Key == Key.System && e.SystemKey == Key.F4) WindowBack?.Focus(); }
    }
}