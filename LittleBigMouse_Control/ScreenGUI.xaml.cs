﻿/*
  MouseControl - Mouse Managment in multi DPI monitors environment
  Copyright (c) 2015 Mathieu GRENET.  All right reserved.

  This file is part of MouseControl.

    ArduixPL is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ArduixPL is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using LbmScreenConfig;
using WinAPI_Dxva2;
using WinAPI_User32;

namespace LittleBigMouse_Control
{
    public delegate void ScreenGuiSelectedChangedHandler(object sender, bool selected);
    /// <summary>
    /// Interaction logic for ScreenGUI.xaml
    /// </summary>
    public partial class ScreenGui : NotifyUserControl
    {
        private const double MinFontSize = 0.1;


        public Screen Screen { get; }
        public MultiScreensGui Presenter { get; }

        public ScreenGuiControl ScreenGuiControl
        {
            get { return _screenGuiControl; }
            set
            {

                if (Change.SetProperty(ref _screenGuiControl, value))
                {
                    grid.Children.Clear();

                    if (_screenGuiControl!=null)
                        grid.Children.Add(_screenGuiControl);
                }
            }
        }

        public event ScreenGuiSelectedChangedHandler SelectedChanged; 

        public ScreenGui(Screen s, MultiScreensGui screensGui)
        {
            Presenter = screensGui;
            Screen = s;

            InitializeComponent();

            Change.Watch(Screen, "Screen");
            Change.Watch(Screen.Config, "Config");
            Change.Watch(Presenter, "Presenter");
            Change.Watch(MainGui.Instance, "MainGui");

            DataContext = this;
        }

        [DependsOn("MainGui.GetScreenGuiControl", "Screen")]
        public void UpdateScreenGuiControl()
        {
            ScreenGuiControl = MainGui.Instance.GetScreenGuiControl !=null ? MainGui.Instance.GetScreenGuiControl(Screen) : null;
        }

        public IEnumerable<ScreenGui> OtherGuis
            => Presenter.AllScreenGuis.Where(elGui => !Equals(elGui, this));


        [DependsOn("Screen.Selected")]
        public void UpdateSelected()
        {
            if (Screen.Selected)
            {
                
            }
            else
            {
                TopBorderSelected = false;
                BottomBorderSelected = false;
                LeftBorderSelected = false;
                RightBorderSelected = false;
            }
            SelectedChanged?.Invoke(Screen, Screen.Selected);
        }


        [DependsOn("Screen.Selected")]
        public LinearGradientBrush BorderColor
        {
            get
            {
                LinearGradientBrush brush = new LinearGradientBrush()
                {
                    StartPoint = new Point(0,0.4),
                    EndPoint = new Point(1,0.6),
                    GradientStops =
                    {
                        new GradientStop { Color = Colors.Black },
                        new GradientStop { Color = Colors.Gray,Offset = 0.5},
                        new GradientStop { Color = new Color { A = 255, R = 30, G = 30, B = 30 }, Offset = 0.5 },
                        new GradientStop { Color = Colors.Black, Offset = 1 }
                        
                    }
                };

                //GradientStop gd0 = new GradientStop {Color =  false ? Colors.Lime : Colors.Gray};
                //GradientStop gd1 = new GradientStop {Color =  false ? Colors.DarkGreen : new Color {A=255,R=30,G=30,B=30},Offset =0.6};

                return brush;
            }
        }


        [DependsOn("Screen.Selected")]
        public Brush SelectedBrush
        {
            get
            {
                Color c1;
                Color c2;
                if (Screen.Selected)
                {
                    c1 = Colors.LightGreen;
                    c2 = Colors.DarkGreen;
                }
                else
                {
                    c1 = Color.FromArgb(0xFF, 0x72, 0x88, 0xC0);
                    c2 = Color.FromArgb(0xFF, 0x33, 0x3E, 0x9A);                   
                }

                return new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0.3),
                    EndPoint = new Point(1, 0.7),
                    GradientStops =
                    {
                         new GradientStop { Color = c1,Offset = 0},
                         new GradientStop { Color = c2, Offset = 1 }
                    }
                };

                //return new LinearGradientBrush()
                //{
                //    StartPoint = new Point(0, 0.3),
                //    EndPoint = new Point(1, 0.7),
                //    GradientStops =
                //    {
                //        new GradientStop { Color = c2 },
                //        new GradientStop { Color = c1,Offset = 0.5},
                //        new GradientStop { Color = c2, Offset = 0.5 },
                //        new GradientStop { Color = c2, Offset = 1 }
                //    }
                //};
            }
        }

        private bool _power = true;

        public Viewbox PowerButton => _power
            ? (Viewbox) Application.Current.FindResource("LogoPowerOn")
            : (Viewbox) Application.Current.FindResource("LogoPowerOff");

        public Viewbox Logo
        {
            get
            {
                switch (Screen.ManufacturerCode.ToLower())
                {
                    case "sam":
                        return (Viewbox)Application.Current.FindResource("LogoSam");
                    case "del":
                        return (Viewbox)Application.Current.FindResource("LogoDel");
                    case "api":
                        return (Viewbox)Application.Current.FindResource("LogoAcer");
                    case "atk":
                        return (Viewbox)Application.Current.FindResource("LogoAsus");
                    case "eiz":
                        return (Viewbox)Application.Current.FindResource("LogoEizo");
                    case "ben":
                    case "bnq":
                        return (Viewbox)Application.Current.FindResource("LogoBenq");
                    case "nec":
                        return (Viewbox)Application.Current.FindResource("LogoNec");
                    case "hpq":
                        return (Viewbox)Application.Current.FindResource("LogoHp");
                    case "lg":
                        return (Viewbox)Application.Current.FindResource("LogoLg");
                    case "Apl":
                        return (Viewbox)Application.Current.FindResource("LogoApple");
                    case "fuj":
                        return (Viewbox)Application.Current.FindResource("LogoFujitsu");
                    case "ibm":
                        return (Viewbox)Application.Current.FindResource("LogoIbm");
                    case "mat":
                        return (Viewbox)Application.Current.FindResource("LogoPanasonic");
                    case "sny":
                       return (Viewbox)Application.Current.FindResource("LogoSony");
                    case "tos":
                        return (Viewbox)Application.Current.FindResource("LogoToshiba");
                    case "aoc":
                        return (Viewbox)Application.Current.FindResource("LogoAoc");
                    case "ivm":
                        return (Viewbox)Application.Current.FindResource("LogoIiyama");
                    case "len":
                        return (Viewbox)Application.Current.FindResource("LogoLenovo");
                    case "phl":
                        return (Viewbox)Application.Current.FindResource("LogoPhilips");                    
                    case "hei":
                        return (Viewbox)Application.Current.FindResource("LogoYundai");
                    case "cpq":
                        return (Viewbox)Application.Current.FindResource("LogoCompaq");
                    case "htc":
                        return (Viewbox)Application.Current.FindResource("LogoHitachi");
                    case "hyo":
                        return (Viewbox)Application.Current.FindResource("LogoQnix");
                    case "nts": 
                        return (Viewbox)Application.Current.FindResource("LogoIolair");
                    default:
                        return (Viewbox)Application.Current.FindResource("LogoLbm");
                }
            }
        }

        [DependsOn("Presenter.Ratio")]
        public Thickness LogoPadding => new Thickness(4*Presenter.Ratio);



        [DependsOn("Screen.PhysicalOutsideBounds", "Presenter.Ratio", "Screen.Orientation")]
        public double ThisWidth => 
            Screen.PhysicalOutsideBounds.Width * Presenter.Ratio;

        [DependsOn("Screen.PhysicalOutsideBounds", "Presenter.Ratio", "Screen.Orientation")]
        public double ThisUnrotatedWidth => (Screen.Orientation % 2 == 0) ? ThisWidth : ThisHeight;

        [DependsOn("Screen.PhysicalOutsideBounds", "Presenter.Ratio", "Screen.Orientation")]
        public double ThisHeight => Screen.PhysicalOutsideBounds.Height * Presenter.Ratio;

        [DependsOn("Screen.PhysicalOutsideBounds", "Presenter.Ratio", "Screen.Orientation")]
        public double ThisUnrotatedHeight => (Screen.Orientation%2 == 0) ? ThisHeight : ThisWidth;


        [DependsOn("Presenter.Size", "Screen.PhysicalX", "Screen.PhysicalY", "Presenter.MovingPhysicalOutsideBounds", "Screen.LeftBorder","Screen.TopBorder", "Presenter.Ratio")]
        public Thickness ThisMargin => new Thickness(
                    Presenter.PhysicalToUiX(Screen.PhysicalOutsideBounds.X),
                    Presenter.PhysicalToUiY(Screen.PhysicalOutsideBounds.Y),
                    0, 0);

        [DependsOn("Screen.TopBorder", "Presenter.Ratio")]
        public GridLength TopBorder => new GridLength(Screen.TopBorder * Presenter.Ratio);
        [DependsOn("LeftBorder", "TopBorder", "RightBorder", "BottomBorder")]
        public GridLength UnrotatedTopBorder
        {
            get
            {
                switch (Screen.Orientation)
                {
                    default:
                        return TopBorder;
                    case 1:
                        return RightBorder;
                    case 2:
                        return BottomBorder;
                    case 3:
                        return LeftBorder;
                }
            }
        }
        [DependsOn("Screen.BottomBorder", "Presenter.Ratio")]
        public GridLength BottomBorder => new GridLength(Screen.BottomBorder * Presenter.Ratio);

        [DependsOn("LeftBorder", "TopBorder", "RightBorder", "BottomBorder")]
        public GridLength UnrotatedBottomBorder
        {
            get
            {
                switch (Screen.Orientation)
                {
                    default:
                        return BottomBorder;
                    case 1:
                        return LeftBorder;
                    case 2:
                        return TopBorder;
                    case 3:
                        return RightBorder;
                }
            }
        }
        [DependsOn("BottomBorder")]
        public double BottomFontSize => Math.Max(MinFontSize, BottomBorder.Value*0.6);

        [DependsOn("Screen.LeftBorder", "Presenter.Ratio")]
        public GridLength LeftBorder => new GridLength(Screen.LeftBorder * Presenter.Ratio);

        [DependsOn("LeftBorder", "TopBorder", "RightBorder", "BottomBorder")]
        public GridLength UnrotatedLeftBorder
        {
            get
            {
                switch (Screen.Orientation)
                {
                    default:
                        return LeftBorder;
                    case 1:
                        return TopBorder;
                    case 2:
                        return RightBorder;
                    case 3:
                        return BottomBorder;
                }
            }
        }

        [DependsOn("LeftBorder")]
        public double LeftFontSize => Math.Max(MinFontSize, LeftBorder.Value * 0.6);

        [DependsOn("Screen.RightBorder", "MainGui.Ratio")]
        public GridLength RightBorder => new GridLength(Screen.RightBorder * Presenter.Ratio);

        [DependsOn("LeftBorder", "TopBorder", "RightBorder", "BottomBorder")]
        public GridLength UnrotatedRightBorder
        {
            get
            {
                switch (Screen.Orientation)
                {
                    default:
                        return RightBorder;
                    case 1:
                        return BottomBorder;
                    case 2:
                        return LeftBorder;
                    case 3:
                        return TopBorder;
                }
            }
        }

        [DependsOn("RightBorder")]
        public double RightFontSize => Math.Max(MinFontSize, RightBorder.Value * 0.6);


        [DependsOn("TopBorder")]
        public double TopFontSize => Math.Max(MinFontSize, TopBorder.Value * .6);


        [DependsOn("ThisHeight", "TopBorder", "BottomBorder")]
        public double ThisFontSize => Math.Max(MinFontSize,
            Math.Pow(
                Math.Min(ThisHeight - TopBorder.Value - BottomBorder.Value,
                    ThisWidth - LeftBorder.Value - RightBorder.Value)/7,
                1/1.3)
            );

        [DependsOn("TopBorder")]
        public double PnpNameFontSize => Math.Max(MinFontSize, UnrotatedTopBorder.Value * .8);


        [DependsOn("Screen.Orientation", "ThisWidth", "ThisHeight")]
        public Transform ScreenOrientation
        {
            get
            {
                switch (Screen.Orientation)
                {
                    default:
                        return new TransformGroup {};
                    case 1:
                        return new TransformGroup {
                            Children = {
                                new RotateTransform(90),
                                new TranslateTransform(ThisWidth, 0) } };
                    case 2:
                        return new TransformGroup {
                            Children = {
                                new RotateTransform(180),
                                new TranslateTransform(ThisWidth, ThisHeight) } };
                    case 3:
                        return new TransformGroup
                        {
                            Children = {
                                new RotateTransform(270),
                                new TranslateTransform(0, ThisHeight) }
                        };
                }
            }
        }

        [DependsOn("Screen.Orientation")]
        public VerticalAlignment DpiVerticalAlignment
            => (Screen.Orientation==3) ? VerticalAlignment.Bottom : VerticalAlignment.Top;

        [DependsOn("Screen.Orientation")]
        public VerticalAlignment PnpNameVerticalAlignment
            => (Screen.Orientation == 2) ? VerticalAlignment.Bottom : VerticalAlignment.Top;


 

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox tBox = (TextBox)sender;

            double delta = (e.Delta > 0) ? 1 : -1;

            DependencyProperty prop = TextBox.TextProperty;

            BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
            binding?.Target.SetValue(prop, (double.Parse(binding?.Target.GetValue(prop).ToString()) + delta).ToString() );
            binding?.UpdateSource();
        }


        private Brush SelectedColor => new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
        private Brush OverflownColor => new SolidColorBrush(Color.FromArgb(32,0,255,0));
        private Brush UnselectedColor => new SolidColorBrush(Colors.Transparent);

        // Top Border
        private void TopBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!TopBorderSelected) return;

            double delta = (e.Delta > 0) ? 1 : -1;
            Screen.RealTopBorder += delta;
        }

        private void TopBorder_MouseEnter(object sender, MouseEventArgs e) { TopBorderOverflown = true; }
        private void TopBorder_MouseLeave(object sender, MouseEventArgs e) { TopBorderOverflown = false; }
        private void TopBorder_MouseDown(object sender, MouseButtonEventArgs e) { TopBorderSelected = !TopBorderSelected; }

        private bool _topBorderOverflown = false;
        public bool TopBorderOverflown
        {
            get { return _topBorderOverflown; }
            set { Change.SetProperty(ref _topBorderOverflown, value); }
        }
        private bool _topBorderSelected = false;
        public bool TopBorderSelected
        {
            get { return _topBorderSelected; }
            set
            {
                if(Change.SetProperty(ref _topBorderSelected, value) && value)
                {
                    LeftBorderSelected = false;
                    RightBorderSelected = false;
                    BottomBorderSelected = false;
                }

            }
        }

        [DependsOn("TopBorderOverflown", "TopBorderSelected")]
        public Brush TopBorderColor
            => TopBorderSelected ? SelectedColor : TopBorderOverflown ? OverflownColor : UnselectedColor;

        [DependsOn("TopBorderOverflown", "TopBorderSelected")]
        public Visibility TopBorderVisibility
            => TopBorderOverflown || TopBorderSelected ? Visibility.Visible : Visibility.Hidden;

        // Bottom Border
        private void BottomBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!BottomBorderSelected) return;

            double delta = (e.Delta > 0) ? 1 : -1;
            Screen.RealBottomBorder += delta;
        }

        private void BottomBorder_MouseEnter(object sender, MouseEventArgs e) { BottomBorderOverflown = true; }
        private void BottomBorder_MouseLeave(object sender, MouseEventArgs e) { BottomBorderOverflown = false; }
        private void BottomBorder_MouseDown(object sender, MouseButtonEventArgs e) { BottomBorderSelected = !BottomBorderSelected; }

        private bool _BottomBorderOverflown = false;
        public bool BottomBorderOverflown
        {
            get { return _BottomBorderOverflown; }
            set { Change.SetProperty(ref _BottomBorderOverflown, value); }
        }
        private bool _BottomBorderSelected = false;
        public bool BottomBorderSelected
        {
            get { return _BottomBorderSelected; }
            set
            {
                if (Change.SetProperty(ref _BottomBorderSelected, value) && value)
                {
                    LeftBorderSelected = false;
                    TopBorderSelected = false;
                    RightBorderSelected = false;
                }

            }
        }

        [DependsOn("BottomBorderOverflown", "BottomBorderSelected")]
        public Brush BottomBorderColor
            => BottomBorderSelected ? SelectedColor : BottomBorderOverflown ? OverflownColor : UnselectedColor;

        [DependsOn("BottomBorderOverflown", "BottomBorderSelected")]
        public Visibility BottomBorderVisibility
            => BottomBorderOverflown || BottomBorderSelected ? Visibility.Visible : Visibility.Hidden;

        // Left Border
        private void LeftBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!LeftBorderSelected) return;

            double delta = (e.Delta > 0) ? 1 : -1;
            Screen.RealLeftBorder += delta;
        }

        private void LeftBorder_MouseEnter(object sender, MouseEventArgs e) { LeftBorderOverflown = true; }
        private void LeftBorder_MouseLeave(object sender, MouseEventArgs e) { LeftBorderOverflown = false; }
        private void LeftBorder_MouseDown(object sender, MouseButtonEventArgs e) { LeftBorderSelected = !LeftBorderSelected; }

        private bool _LeftBorderOverflown = false;
        public bool LeftBorderOverflown
        {
            get { return _LeftBorderOverflown; }
            set { Change.SetProperty(ref _LeftBorderOverflown, value); }
        }
        private bool _LeftBorderSelected = false;
        public bool LeftBorderSelected
        {
            get { return _LeftBorderSelected; }
            set
            {
                if (Change.SetProperty(ref _LeftBorderSelected, value) && value)
                {
                    RightBorderSelected = false;
                    TopBorderSelected = false;
                    BottomBorderSelected = false;
                }

            }
        }

        [DependsOn("LeftBorderOverflown", "LeftBorderSelected")]
        public Brush LeftBorderColor
            => LeftBorderSelected ? SelectedColor : LeftBorderOverflown ? OverflownColor : UnselectedColor;

        [DependsOn("LeftBorderOverflown", "LeftBorderSelected")]
        public Visibility LeftBorderVisibility
            => LeftBorderOverflown || LeftBorderSelected ? Visibility.Visible : Visibility.Hidden;

        // Right Border
        private void RightBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!RightBorderSelected) return;

            double delta = (e.Delta > 0) ? 1 : -1;
            Screen.RealRightBorder += delta;
        }

        private void RightBorder_MouseEnter(object sender, MouseEventArgs e) { RightBorderOverflown = true; }
        private void RightBorder_MouseLeave(object sender, MouseEventArgs e) { RightBorderOverflown = false; }
        private void RightBorder_MouseDown(object sender, MouseButtonEventArgs e) { RightBorderSelected = !RightBorderSelected; }

        private bool _RightBorderOverflown = false;
        public bool RightBorderOverflown
        {
            get { return _RightBorderOverflown; }
            set { Change.SetProperty(ref _RightBorderOverflown, value); }
        }
        private bool _RightBorderSelected = false;
        private ScreenGuiControl _screenGuiControl;

        public bool RightBorderSelected
        {
            get { return _RightBorderSelected; }
            set
            {
                if (Change.SetProperty(ref _RightBorderSelected, value) && value)
                {
                    LeftBorderSelected = false;
                    TopBorderSelected = false;
                    BottomBorderSelected = false;
                }
            }
        }

        [DependsOn("RightBorderOverflown", "RightBorderSelected")]
        public Brush RightBorderColor
            => RightBorderSelected ? SelectedColor : RightBorderOverflown ? OverflownColor : UnselectedColor;

        [DependsOn("RightBorderOverflown", "RightBorderSelected")]
        public Visibility RightBorderVisibility
            => RightBorderOverflown || RightBorderSelected ? Visibility.Visible : Visibility.Hidden;


        [DependsOn("Screen.Selected")]
        public Visibility SelectedVisibility
             => Screen.Selected ? Visibility.Visible : Visibility.Collapsed;


        private void ResetPlace_Click(object sender, RoutedEventArgs e)
        {
            Screen.Config.SetPhysicalAuto();
        }

        private void ResetSize_Click(object sender, RoutedEventArgs e)
        {
            Screen.RealPhysicalHeight = double.NaN;
            Screen.RealPhysicalWidth = double.NaN;
        }



    }

    public class Anchor
    {
        public Screen Screen { get; }
        public double Pos { get; }

        public Brush Brush { get; }

        public Anchor(Screen screen, double pos, Brush brush)
        {
            Screen = screen;
            Pos = pos;
            Brush = brush;
        }
    }

}

