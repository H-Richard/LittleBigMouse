﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LbmScreenConfig;
using WinAPI_Dxva2;

namespace LittleBigMouse_Control
{
    /// <summary>
    /// Logique d'interaction pour ScreenGuiSizer.xaml
    /// </summary>
    public partial class ScreenGuiSizer : ScreenGuiControl
    {

        public ScreenGuiSizer(Screen screen) : base(screen)
        {
            InitializeComponent();
        }

        private SizerPlugin.SizerPlugin Plugin => SizerPlugin.SizerPlugin.Instance;

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            center.Height = Math.Min(grid.ActualHeight, grid.ActualWidth) / 3;
            center.Width = center.Height;
            center.CornerRadius = new CornerRadius(center.Height / 2);

            if (center.Height > 0)
                lblName.FontSize = center.Height / 2;
        }

        [DependsOn("Screen.Selected")]
        public LinearGradientBrush ScreenColor
        {
            get
            {
                var c1 = Color.FromArgb(0xFF, 0x72, 0x88, 0xC0);
                var c2 = Color.FromArgb(0xFF, 0x52, 0x66, 0x9D);

                return new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0.3),
                    EndPoint = new Point(1, 0.7),
                    GradientStops =
                    {
                        new GradientStop {Color = c1, Offset = 0},
                        new GradientStop {Color = c2, Offset = 1}
                    }
                };
                //GradientStop gd0 = new GradientStop {Color =  false ? Colors.Lime : Colors.Gray};
                //GradientStop gd1 = new GradientStop {Color =  false ? Colors.DarkGreen : new Color {A=255,R=30,G=30,B=30},Offset =0.6};

            }
        }

        private void PhysicalWidth_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Screen.Selected) return;

            double delta = (e.Delta > 0) ? 1 : -1;
            Screen.RealPhysicalWidth += delta;
        }
        private void PhysicalHeight_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Screen.Selected) return;

            double delta = (e.Delta > 0) ? 1 : -1;
            Screen.RealPhysicalHeight += delta;
        }

        private Point _guiStartPosition;
        private Point _guiLastPosition;
        private Point _dragStartPosition;
        private Point _dragLastPosition;

        private Vector ShiftScreen(Vector offset)
        {
            Point pos = _dragStartPosition + offset;
            Screen.PhysicalLocation = pos;
            Vector shift = Screen.PhysicalLocation - pos;
            MainGui.Instance.Config.ShiftMovingPhysicalBounds(shift);
            _dragStartPosition += shift;
            return shift;
        }
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
                StartMove(e);
        }
        private void StartMove(MouseEventArgs e)
        {
            _guiStartPosition = e.GetPosition(Gui.ScreensGrid);
            _guiLastPosition = _guiStartPosition;
            _dragStartPosition = Screen.PhysicalLocation;
            _dragLastPosition = _dragStartPosition;

            Gui.BringToFront();

            CaptureMouse();

            MainGui.Instance.Config.Moving = true;

            Screen.Selected = true;

            e.Handled = true;
        }
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndMove();
        }
        private void EndMove()
        {
            if (!MainGui.Instance.Config.Moving) return;

            Plugin.VerticalAnchors.Children.Clear();
            Plugin.HorizontalAnchors.Children.Clear();

            if (!Screen.Config.AllowDiscontinuity) Screen.Config.Compact();
            if (!Screen.Config.AllowOverlaps) Screen.Config.Expand();

            MainGui.Instance.Config.Moving = false;
            ReleaseMouseCapture();

            MainGui.Instance.Config.UpdatePhysicalOutsideBounds();

            //Todo : Plugin.ActivateConfig();
        }

        private IEnumerable<ScreenGuiSizer> OtherGuis
        {
            get
            {
                foreach (ScreenGuiControl ctrl in MainGui.Instance.ScreensPresenter.AllControlGuis)
                {
                    ScreenGuiSizer sz = ctrl as ScreenGuiSizer;

                    if (sz != null && sz != this) yield return sz;
                }
            }
        }

        public MultiScreensGui Gui => MainGui.Instance.ScreensPresenter as MultiScreensGui;

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Gui == null)
            {
                return;
            }


            const double maxSnapDistance = 10.0;

            if (!MainGui.Instance.Config.Moving) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                EndMove();
                return;
            }


            var newGuiPosition = e.GetPosition(Gui.ScreensGrid);

            Vector dragOffset = (newGuiPosition - _guiStartPosition) / Gui.Ratio;

            Vector snapOffset = new Vector(double.PositiveInfinity, double.PositiveInfinity);

            List<Anchor> xAnchors = new List<Anchor>();
            List<Anchor> yAnchors = new List<Anchor>();

            Vector shift = ShiftScreen(dragOffset);

            //use anchors when control key is not pressed
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                foreach (ScreenGuiSizer s in OtherGuis)
                {
                    foreach (Anchor xAnchorThis in VerticalAnchors)
                    {
                        foreach (Anchor xAnchorOther in s.VerticalAnchors)
                        {
                            double xOffset = xAnchorOther.Pos - xAnchorThis.Pos;

                            // if new offset is just egual to last, Add the new anchor visualization
                            if (Math.Abs(xOffset - snapOffset.X) < 0.01)
                            {
                                snapOffset.X = xOffset;
                                xAnchors.Add(xAnchorOther);
                            }
                            // if new offset is better than old one, Remove all visuals and Add the new one
                            else if ((Math.Abs(xOffset) < Math.Abs(snapOffset.X)))
                            {
                                snapOffset.X = xOffset;
                                xAnchors.Clear();
                                xAnchors.Add(xAnchorOther);
                            }
                        }
                    }

                    foreach (Anchor yAnchorThis in HorizontalAnchors)
                    {
                        foreach (Anchor yAnchorOther in s.HorizontalAnchors)
                        {
                            double yOffset = yAnchorOther.Pos - yAnchorThis.Pos;
                            // if new offset is just egual to last, Add the new anchor visualization
                            if (Math.Abs(yOffset - snapOffset.Y) < 0.01)
                            {
                                snapOffset.Y = yOffset;
                                yAnchors.Add(yAnchorOther);
                            }
                            // if new offset is better than old one, Remove all visuals and Add the new one
                            else if ((Math.Abs(yOffset) < Math.Abs(snapOffset.Y)))
                            {
                                snapOffset.Y = yOffset;
                                yAnchors.Clear();
                                yAnchors.Add(yAnchorOther);
                            }
                        }
                    }
                }


                //Apply offset if under maximal snap distance
                if (Math.Abs(snapOffset.X) > maxSnapDistance)
                {
                    xAnchors.Clear();
                    snapOffset.X = 0;
                }

                if (Math.Abs(snapOffset.Y) > maxSnapDistance)
                {
                    yAnchors.Clear();
                    snapOffset.Y = 0;
                }

                dragOffset += snapOffset;
            }

            shift = ShiftScreen(dragOffset);

            Plugin.VerticalAnchors.Children.Clear();
            foreach (Anchor anchor in xAnchors)
            {
                double guiX = Gui.PhysicalToUiX(anchor.Pos + shift.X);
                Line l = new Line()
                {
                    X1 = guiX,
                    X2 = guiX,
                    Y1 = 0,
                    Y2 = Gui.ScreensGrid.ActualHeight,
                    Stroke = anchor.Brush,
                    StrokeDashArray = new DoubleCollection { 5, 3 }
                };
                Plugin.VerticalAnchors.Children.Add(l);
            }

            Plugin.HorizontalAnchors.Children.Clear();
            foreach (Anchor anchor in yAnchors)
            {
                double guiY = Gui.PhysicalToUiY(anchor.Pos + shift.Y);
                Line l = new Line()
                {
                    Y1 = guiY,
                    Y2 = guiY,
                    X1 = 0,
                    X2 = Gui.ScreensGrid.ActualWidth,
                    Stroke = anchor.Brush,
                    StrokeDashArray = new DoubleCollection { 5, 3 }
                };
                Plugin.HorizontalAnchors.Children.Add(l);
            }
        }
        public List<Anchor> VerticalAnchors => new List<Anchor>
                {
                     new Anchor(Screen,Screen.PhysicalOutsideBounds.X,new SolidColorBrush(Colors.Chartreuse)),
                     new Anchor(Screen,Screen.PhysicalX,new SolidColorBrush(Colors.LightGreen)),
                     new Anchor(Screen,Screen.PhysicalX + Screen.PhysicalWidth /2,new SolidColorBrush(Colors.Red)),
                     new Anchor(Screen,Screen.PhysicalBounds.Right,new SolidColorBrush(Colors.LightGreen)),
                     new Anchor(Screen,Screen.PhysicalOutsideBounds.Right,new SolidColorBrush(Colors.Chartreuse)),
                };

        public List<Anchor> HorizontalAnchors => new List<Anchor>
                {
                     new Anchor(Screen,Screen.PhysicalOutsideBounds.Y,new SolidColorBrush(Colors.Chartreuse)),
                     new Anchor(Screen,Screen.PhysicalY,new SolidColorBrush(Colors.LightGreen)),
                     new Anchor(Screen,Screen.PhysicalY + Screen.PhysicalHeight /2,new SolidColorBrush(Colors.Red)),
                     new Anchor(Screen,Screen.PhysicalBounds.Bottom,new SolidColorBrush(Colors.LightGreen)),
                     new Anchor(Screen,Screen.PhysicalOutsideBounds.Bottom,new SolidColorBrush(Colors.Chartreuse)),
                };


    }
}
