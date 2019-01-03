using System;
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
        private readonly List<SketchPoint> mColorPoints;
        private SketchPoint mSelectedColorPoint;
        private SketchPoint mSelectedColorPointEllipse;

        public SketchCanvas()
        {
            InitializeComponent();

            // create sketch canvas
            sketchCanvas.Background = Brushes.White;

            sketchCanvas.MouseDown += Canvas_MouseDown;
            sketchCanvas.MouseUp += Canvas_MouseUp;
            sketchCanvas.MouseMove += Canvas_MouseMove;

            mColorPoints = new List<SketchPoint>();
            mSelectedColorPoint = null;
            mSelectedColorPointEllipse = null;

            DrawGrid();
        }

        public static readonly DependencyProperty QueryResultProperty = DependencyProperty.Register(
            nameof(QueryResult),
            typeof(SketchQueryResult),
            typeof(SketchCanvas),
            new FrameworkPropertyMetadata(
                (obj, args) =>
                {
                    if (args.NewValue == null)
                    {
                        ((SketchCanvas)obj).Clear();
                    }
                }) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty CanvasWidthProperty = DependencyProperty.Register(
            nameof(CanvasWidth),
            typeof(int),
            typeof(SketchCanvas),
            new FrameworkPropertyMetadata(default(int)) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty CanvasHeightProperty = DependencyProperty.Register(
            nameof(CanvasHeight),
            typeof(int),
            typeof(SketchCanvas),
            new FrameworkPropertyMetadata(default(int)) { BindsTwoWayByDefault = true });

        public SketchQueryResult QueryResult
        {
            get => (SketchQueryResult)GetValue(QueryResultProperty);
            set => SetValue(QueryResultProperty, value);
        }

        public int CanvasWidth
        {
            get => (int)GetValue(CanvasWidthProperty);
            set => SetValue(CanvasWidthProperty, value);
        }

        public int CanvasHeight
        {
            get => (int)GetValue(CanvasHeightProperty);
            set => SetValue(CanvasHeightProperty, value);
        }

        /// <summary>
        /// Clears all colored circles from the canvas. SketchChangedEvent is not raised.
        /// </summary>
        public void Clear()
        {
            foreach (SketchPoint CP in mColorPoints)
            {
                CP.RemoveFromCanvas(sketchCanvas);
            }

            mColorPoints.Clear();
            mSelectedColorPoint = null;

            OnSketchChanged();
        }

        public void DeletePoints()
        {
            foreach (SketchPoint CP in mColorPoints)
            {
                CP.RemoveFromCanvas(sketchCanvas);
            }

            mColorPoints.Clear();
            mSelectedColorPoint = null;
        }
        
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(sketchCanvas);

            mSelectedColorPoint = SketchPoint.IsSelected(mColorPoints, p);
            mSelectedColorPointEllipse = SketchPoint.IsSelectedEllipse(mColorPoints, p);

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
                SketchColorPicker colorPicker = new SketchColorPicker();

                if (colorPicker.Show(Mouse.GetPosition(Application.Current.MainWindow)))
                {
                    mSelectedColorPoint = colorPicker.SelectedImage == null
                                              ? (SketchPoint)new ColorPoint(p, colorPicker.SelectedColor, sketchCanvas)
                                              : new ImagePoint(p, colorPicker.SelectedImage, sketchCanvas);
                    mColorPoints.Add(mSelectedColorPoint);
                    mSelectedColorPoint = null;
                    OnSketchChanged();
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(sketchCanvas);
            if (p.X < SketchPoint.Radius || p.X > sketchCanvas.Width - SketchPoint.Radius)
            {
                return;
            }

            if (p.Y < SketchPoint.Radius || p.Y > sketchCanvas.Height - SketchPoint.Radius)
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

            foreach (SketchPoint sketchPoint in mColorPoints)
            {
                Point position = new Point(sketchPoint.Position.X / sketchCanvas.Width, sketchPoint.Position.Y / sketchCanvas.Height);
                Point ellipseAxis = new Point(sketchPoint.SearchRadiusX / sketchCanvas.Width, sketchPoint.SearchRadiusY / sketchCanvas.Height);

                colorSketches.Add(new SketchColorPoint(position, sketchPoint.FillColor, ellipseAxis, sketchPoint.SketchType, sketchPoint.Area));
            }

            QueryResult = new SketchQueryResult(colorSketches);

        }

        private abstract class SketchPoint
        {
            public const int Radius = 12;

            private readonly Ellipse SearchRadiusEllipse;
            private bool mArea;
            private double mSearchRadiusX = 40;
            private double mSearchRadiusY = 40;

            protected SketchPoint(Point point, Color color, Canvas canvas)
            {
                Position = point;
                FillColor = color;

                SearchRadiusEllipse = new Ellipse();
                SearchRadiusEllipse.Width = 2 * mSearchRadiusX;
                SearchRadiusEllipse.Height = 2 * mSearchRadiusY;
                SearchRadiusEllipse.Stroke = Brushes.LightGray;
                canvas.Children.Add(SearchRadiusEllipse);

                Area = true;
            }

            public Point Position { get; private set; }
            public Color FillColor { get; }
            public abstract SketchType SketchType { get; }

            public bool Area
            {
                get { return mArea; }
                set
                {
                    mArea = value;

                    if (mArea)
                    {
                        SearchRadiusEllipse.Stroke = new SolidColorBrush(FillColor);
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

            public static SketchPoint IsSelected(List<SketchPoint> colorPoints, Point p)
            {
                SketchPoint result = null;
                foreach (SketchPoint CP in colorPoints)
                {
                    if ((CP.Position.X - p.X) * (CP.Position.X - p.X) + (CP.Position.Y - p.Y) * (CP.Position.Y - p.Y) <= Radius * Radius)
                    {
                        return CP;
                    }
                }

                return result;
            }

            public static SketchPoint IsSelectedEllipse(List<SketchPoint> colorPoints, Point p)
            {
                SketchPoint result = null;
                foreach (SketchPoint CP in colorPoints)
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

            public virtual void RemoveFromCanvas(Canvas canvas)
            {
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

            public virtual void UpdatePosition(Point p)
            {
                Position = p;

                Canvas.SetTop(SearchRadiusEllipse, Position.Y - mSearchRadiusY);
                Canvas.SetLeft(SearchRadiusEllipse, Position.X - mSearchRadiusX);
            }
        }


        private class ColorPoint : SketchPoint
        {
            private readonly Ellipse FillEllipse;

            public ColorPoint(Point point, Color color, Canvas canvas) : base(point, color, canvas)
            {
                FillEllipse = new Ellipse
                              {
                                  Width = 2 * Radius,
                                  Height = 2 * Radius,
                                  Fill = new SolidColorBrush(color)
                              };
                canvas.Children.Add(FillEllipse);

                UpdatePosition(point);
            }

            public override void UpdatePosition(Point p)
            {
                base.UpdatePosition(p);
                Canvas.SetTop(FillEllipse, Position.Y - Radius);
                Canvas.SetLeft(FillEllipse, Position.X - Radius);
            }

            public override SketchType SketchType { get; } = SketchType.Color;

            public override void RemoveFromCanvas(Canvas canvas)
            {
                base.RemoveFromCanvas(canvas);
                canvas.Children.Remove(FillEllipse);
            }
        }

        private class ImagePoint : SketchPoint
        {
            private readonly double _scalingConstant = Math.Sqrt(2);

            public ImagePoint(Point point, Image image, Canvas canvas) : base(point, Colors.Black, canvas)
            {
                Image = new Image
                        {
                            Source = image.Source,
                            Width = Radius * _scalingConstant,
                            Height = Radius * _scalingConstant
                        };
                RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.HighQuality);
                SketchType = (SketchType)image.Tag;
                canvas.Children.Add(Image);

                UpdatePosition(point);
            }

            
            public Image Image { get; }

            public override void UpdatePosition(Point p)
            {
                base.UpdatePosition(p);
                Canvas.SetTop(Image, Position.Y - Radius / _scalingConstant);
                Canvas.SetLeft(Image, Position.X - Radius / _scalingConstant);
            }

            public override SketchType SketchType { get; }

            public override void RemoveFromCanvas(Canvas canvas)
            {
                base.RemoveFromCanvas(canvas);
                canvas.Children.Remove(Image);
            }
        }
    }
}
