﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ViretTool.PresentationLayer.Controls.Common.Sketches;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public partial class SketchCanvas : UserControl
    {
        private readonly List<ColorPoint> mColorPoints;
        private ColorPoint mSelectedColorPoint;
        private ColorPoint mSelectedColorPointEllipse;

        public SketchCanvas()
        {
            InitializeComponent();

            // create sketch canvas
            sketchCanvas.Background = Brushes.White;

            sketchCanvas.MouseDown += Canvas_MouseDown;
            sketchCanvas.MouseUp += Canvas_MouseUp;
            sketchCanvas.MouseMove += Canvas_MouseMove;

            mColorPoints = new List<ColorPoint>();
            mSelectedColorPoint = null;
            mSelectedColorPointEllipse = null;

            DrawGrid();
        }

        public static readonly DependencyProperty QueryResultProperty = DependencyProperty.Register(
            "QueryResult",
            typeof(SketchQueryResult),
            typeof(SketchCanvas),
            new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true });

        public SketchQueryResult QueryResult
        {
            get => (SketchQueryResult)GetValue(QueryResultProperty);
            set => SetValue(QueryResultProperty, value);
        }

        /// <summary>
        /// Clears all colored circles from the canvas. SketchChangedEvent is not raised.
        /// </summary>
        public void Clear()
        {
            foreach (ColorPoint CP in mColorPoints)
            {
                CP.RemoveFromCanvas(sketchCanvas);
            }

            mColorPoints.Clear();
            mSelectedColorPoint = null;

            OnSketchChanged();
        }

        public void DeletePoints()
        {
            foreach (ColorPoint CP in mColorPoints)
            {
                CP.RemoveFromCanvas(sketchCanvas);
            }

            mColorPoints.Clear();
            mSelectedColorPoint = null;
        }
        
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(sketchCanvas);

            mSelectedColorPoint = ColorPoint.IsSelected(mColorPoints, p);
            mSelectedColorPointEllipse = ColorPoint.IsSelectedEllipse(mColorPoints, p);

            // change circle type
            if (e.RightButton == MouseButtonState.Pressed && mSelectedColorPointEllipse != null)
            {
                mSelectedColorPointEllipse.Area = !mSelectedColorPointEllipse.Area;

                OnSketchChanged();

                mSelectedColorPointEllipse = null;
                return;
            }

            // remove circle
            if (e.RightButton == MouseButtonState.Pressed && mSelectedColorPoint != null)
            {
                mColorPoints.Remove(mSelectedColorPoint);
                mSelectedColorPoint.RemoveFromCanvas(sketchCanvas);

                OnSketchChanged();

                mSelectedColorPoint = null;
                return;
            }

            // add new circle
            if (mSelectedColorPoint == null && mSelectedColorPointEllipse == null)
            {
                // once the window is closed it cannot be reopened (consider visibility = hidden)
                ColorPicker colorPicker = new ColorPicker();

                if (colorPicker.Show(Mouse.GetPosition(Application.Current.MainWindow)))
                {
                    mSelectedColorPoint = new ColorPoint(p, colorPicker.SelectedColor, sketchCanvas);
                    mColorPoints.Add(mSelectedColorPoint);
                    mSelectedColorPoint = null;
                    OnSketchChanged();
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(sketchCanvas);
            if (p.X < ColorPoint.Radius || p.X > sketchCanvas.Width - ColorPoint.Radius)
            {
                return;
            }

            if (p.Y < ColorPoint.Radius || p.Y > sketchCanvas.Height - ColorPoint.Radius)
            {
                return;
            }

            if (mSelectedColorPoint != null)
            {
                mSelectedColorPoint.UpdatePosition(p);
            }

            if (mSelectedColorPointEllipse != null)
            {
                mSelectedColorPointEllipse.UpdateEllipse(p);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mSelectedColorPoint != null)
            {
                OnSketchChanged();
            }

            if (mSelectedColorPointEllipse != null)
            {
                OnSketchChanged();
            }

            mSelectedColorPoint = null;
            mSelectedColorPointEllipse = null;
        }

        private void DrawGrid()
        {
            double WX = 20, WY = 15;
            double wx = sketchCanvas.Width / WX, wy = sketchCanvas.Height / WY;

            for (int i = 0; i <= WX; i++)
            {
                Line l = new Line();
                l.X1 = i * wx;
                l.X2 = l.X1;
                l.Y1 = 0;
                l.Y2 = sketchCanvas.Height;
                l.Stroke = Brushes.Lavender;
                if (i % 5 != 0)
                {
                    l.StrokeDashArray = new DoubleCollection() { 3 };
                }

                sketchCanvas.Children.Add(l);
            }

            for (int j = 0; j <= WY; j++)
            {
                Line l = new Line();
                l.X1 = 0;
                l.X2 = sketchCanvas.Width;
                l.Y1 = j * wy;
                l.Y2 = j * wy;
                l.Stroke = Brushes.Lavender;
                if (j % 5 != 0)
                {
                    l.StrokeDashArray = new DoubleCollection() { 3 };
                }

                sketchCanvas.Children.Add(l);
            }
        }

        private void OnSketchChanged()
        {
            List<SketchColorPoint> colorSketches = new List<SketchColorPoint>();

            foreach (ColorPoint CP in mColorPoints)
            {
                Point position = new Point(CP.Position.X / sketchCanvas.Width, CP.Position.Y / sketchCanvas.Height);
                Point ellipseAxis = new Point(CP.SearchRadiusX / sketchCanvas.Width, CP.SearchRadiusY / sketchCanvas.Height);

                colorSketches.Add(new SketchColorPoint(position, CP.FillColor, ellipseAxis, CP.Area));
            }

            QueryResult = new SketchQueryResult(colorSketches);

        }

        private void sketchClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }


        private class ColorPoint
        {
            public const int Radius = 12;
            public readonly Color FillColor;
            public readonly Ellipse FillEllipse;
            public Point Position;
            public readonly Ellipse SearchRadiusEllipse;
            private bool mArea;
            private double mSearchRadiusX = 40;
            private double mSearchRadiusY = 40;

            public ColorPoint(Point p, Color c, Canvas canvas)
            {
                Position = p;
                FillColor = c;

                FillEllipse = new Ellipse();
                FillEllipse.Width = 2 * Radius;
                FillEllipse.Height = 2 * Radius;
                FillEllipse.Fill = new SolidColorBrush(c);
                canvas.Children.Add(FillEllipse);

                SearchRadiusEllipse = new Ellipse();
                SearchRadiusEllipse.Width = 2 * mSearchRadiusX;
                SearchRadiusEllipse.Height = 2 * mSearchRadiusY;
                SearchRadiusEllipse.Stroke = Brushes.LightGray;
                canvas.Children.Add(SearchRadiusEllipse);

                UpdatePosition(p);
                Area = true;
            }

            public bool Area
            {
                get { return mArea; }
                set
                {
                    mArea = value;

                    if (mArea)
                    {
                        SearchRadiusEllipse.Stroke = FillEllipse.Fill;
                        SearchRadiusEllipse.StrokeThickness = 3;
                    }
                    else
                    {
                        SearchRadiusEllipse.Stroke = Brushes.LightGray;
                        SearchRadiusEllipse.StrokeThickness = 1;
                    }
                }
            }

            public double SearchRadiusX
            {
                get { return mSearchRadiusX; }
                set { mSearchRadiusX = value; }
            }

            public double SearchRadiusY
            {
                get { return mSearchRadiusY; }
                set { mSearchRadiusY = value; }
            }

            public static ColorPoint IsSelected(List<ColorPoint> colorPoints, Point p)
            {
                ColorPoint result = null;
                foreach (ColorPoint CP in colorPoints)
                {
                    if ((CP.Position.X - p.X) * (CP.Position.X - p.X) + (CP.Position.Y - p.Y) * (CP.Position.Y - p.Y) <= Radius * Radius)
                    {
                        return CP;
                    }
                }

                return result;
            }

            public static ColorPoint IsSelectedEllipse(List<ColorPoint> colorPoints, Point p)
            {
                ColorPoint result = null;
                foreach (ColorPoint CP in colorPoints)
                {
                    double value = (CP.Position.X - p.X) * (CP.Position.X - p.X) / (CP.SearchRadiusX * CP.SearchRadiusX) +
                                   (CP.Position.Y - p.Y) * (CP.Position.Y - p.Y) / (CP.SearchRadiusY * CP.SearchRadiusY);
                    if (value > 0.6 && value < 1.4)
                    {
                        return CP;
                    }
                }

                return result;
            }

            public void RemoveFromCanvas(Canvas canvas)
            {
                canvas.Children.Remove(FillEllipse);
                canvas.Children.Remove(SearchRadiusEllipse);
            }

            public void UpdateEllipse(Point p)
            {
                double newX = Math.Abs(Position.X - p.X), newY = Math.Abs(Position.Y - p.Y);
                if (newX > Radius + 2)
                {
                    mSearchRadiusX = newX;
                    SearchRadiusEllipse.Width = 2 * mSearchRadiusX;
                    Canvas.SetLeft(SearchRadiusEllipse, Position.X - mSearchRadiusX);
                }

                if (newY > Radius + 2)
                {
                    mSearchRadiusY = newY;
                    SearchRadiusEllipse.Height = 2 * mSearchRadiusY;
                    Canvas.SetTop(SearchRadiusEllipse, Position.Y - mSearchRadiusY);
                }
            }

            public void UpdatePosition(Point p)
            {
                Position = p;
                Canvas.SetTop(FillEllipse, Position.Y - Radius);
                Canvas.SetLeft(FillEllipse, Position.X - Radius);

                Canvas.SetTop(SearchRadiusEllipse, Position.Y - mSearchRadiusY);
                Canvas.SetLeft(SearchRadiusEllipse, Position.X - mSearchRadiusX);
            }
        }


        private class ColorPicker : Window
        {
            public Color SelectedColor = Colors.White;
            private readonly int mColorButtonWidth = 20;
            private readonly Canvas mColorPickerPanel;

            private bool mIsColorSelected = false;


            public ColorPicker()
            {
                SolidColorBrush[] brushes = CreateBrushes();
                mColorPickerPanel = new Canvas();

                Content = mColorPickerPanel;

                int nColorsInRow = 20;
                int nColorsInColumn = ((brushes.Length - 1) / nColorsInRow) + 1; // assumes brushes are not empty
                for (int i = 0; i < brushes.Length; i++)
                {
                    int nthRow = i / nColorsInRow;
                    int nthColumn = i % nColorsInRow;

                    double cellBorderLightness = (i > nColorsInRow - 1)
                                                     ? 1 - (nthRow / (double)(nColorsInColumn))
                                                     : 1 - (nthColumn / (double)(nColorsInRow));
                    cellBorderLightness *= cellBorderLightness;
                    Canvas b = CreateColorCellCanvas(brushes[i], cellBorderLightness);
                    b.MouseDown += LeftClick;

                    mColorPickerPanel.Children.Add(b);
                    Canvas.SetLeft(b, nthColumn * mColorButtonWidth);
                    if (i > nColorsInRow - 1)
                    {
                        Canvas.SetTop(b, 20 + (int)Math.Floor(i / (double)nColorsInRow) * mColorButtonWidth);
                    }
                    else
                    {
                        Canvas.SetTop(b, 10 + (int)Math.Floor(i / (double)nColorsInRow) * mColorButtonWidth);
                    }
                }

                this.Width = nColorsInRow * mColorButtonWidth + 15;
                this.Height = (brushes.Length / nColorsInRow) * mColorButtonWidth + 70;
            }

            public static Color HSLToRGB(float h, float s, float l)
            {
                float r, g, b;

                if (s == 0f)
                {
                    r = g = b = l;
                }
                else
                {
                    float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                    float p = 2 * l - q;
                    r = HueToRgb(p, q, h + 1f / 3f);
                    g = HueToRgb(p, q, h);
                    b = HueToRgb(p, q, h - 1f / 3f);
                }

                return Color.FromRgb(Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
            }

            public bool Show(Point p)
            {
                Left = p.X - 20;
                Top = p.Y - 20;
                ShowDialog();
                return mIsColorSelected;
            }

            private SolidColorBrush[] CreateBrushes()
            {
                List<SolidColorBrush> brushes = new List<SolidColorBrush>();

                for (int x = 0; x < 255; x += 13)
                {
                    brushes.Add(new SolidColorBrush(Color.FromRgb((byte)x, (byte)x, (byte)x)));
                }

                for (float lightness = 0.1f; lightness <= 1; lightness += 0.1f)
                for (float hue = 0; hue <= 1; hue += 0.05f)
                {
                    brushes.Add(new SolidColorBrush(HSLToRGB(hue, 1, lightness)));
                }

                return brushes.ToArray();
            }

            private Canvas CreateColorCellCanvas(SolidColorBrush color, double borderLightness)
            {
                Canvas canvas = new Canvas();
                canvas.Width = mColorButtonWidth;
                canvas.Height = canvas.Width;
                canvas.Background = color;

                double minLightness = 0.2;
                double maxLightness = 0.5;
                double offset = minLightness;
                double denormalizer = maxLightness - minLightness;
                borderLightness *= denormalizer;
                borderLightness += offset;

                byte borderGray = (byte)(borderLightness * 255);


                Color borderColor = Color.FromRgb(borderGray, borderGray, borderGray);

                Rectangle rectangle = new Rectangle
                                      {
                                          Width = canvas.Width,
                                          Height = canvas.Height,
                                          Stroke = new SolidColorBrush(borderColor),
                                          StrokeThickness = 0.5,
                                          //Fill = new SolidColorBrush(Colors.Black),
                                      };

                Canvas.SetLeft(rectangle, 0);
                Canvas.SetTop(rectangle, 0);
                canvas.Children.Add(rectangle);

                return canvas;
            }

            private static float HueToRgb(float p, float q, float t)
            {
                if (t < 0f)
                {
                    t += 1f;
                }

                if (t > 1f)
                {
                    t -= 1f;
                }

                if (t < 1f / 6f)
                {
                    return p + (q - p) * 6f * t;
                }

                if (t < 1f / 2f)
                {
                    return q;
                }

                if (t < 2f / 3f)
                {
                    return p + (q - p) * (2f / 3f - t) * 6f;
                }

                return p;
            }

            private void LeftClick(object sender, RoutedEventArgs e)
            {
                //SelectedColor = ((SolidColorBrush)((Button)sender).Background).Color;
                SelectedColor = ((SolidColorBrush)((Canvas)sender).Background).Color;
                mIsColorSelected = true;
                Close();
            }
        }
    }
}
