using System;
using System.Windows;
using System.Windows.Media;

namespace SpeechVisualizer
{
    public static class Amoeba
    {
        public static double GetRadius(PathGeometry obj) => (double)obj.GetValue(RadiusProperty);
        public static void SetRadius(PathGeometry obj, double value) => obj.SetValue(RadiusProperty, value);

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.RegisterAttached("Radius", typeof(double), typeof(Amoeba), new PropertyMetadata(0.0, Update));

        public static Point GetCenter(PathGeometry obj) => (Point)obj.GetValue(CenterProperty);
        public static void SetCenter(PathGeometry obj, Point value) => obj.SetValue(CenterProperty, value);

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.RegisterAttached("Center", typeof(Point), typeof(Amoeba), new PropertyMetadata(new Point(), Update));

        public static int GetCorners(PathGeometry obj) => (int)obj.GetValue(CornersProperty);
        public static void SetCorners(PathGeometry obj, int value) => obj.SetValue(CornersProperty, value);

        public static readonly DependencyProperty CornersProperty =
            DependencyProperty.RegisterAttached("Corners", typeof(int), typeof(Amoeba), new PropertyMetadata(12, Update));

        public static double GetWaveThickness(DependencyObject obj) => (double)obj.GetValue(WaveThicknessProperty);
        public static void SetWaveThickness(DependencyObject obj, double value) => obj.SetValue(WaveThicknessProperty, value);

        public static readonly DependencyProperty WaveThicknessProperty =
            DependencyProperty.RegisterAttached("WaveThickness", typeof(double), typeof(Amoeba), new PropertyMetadata(0.0, Update));

        private static void Update(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is PathGeometry pathGeometry)
            {
                while (pathGeometry.Figures.Count > 1)
                    pathGeometry.Figures.RemoveAt(pathGeometry.Figures.Count - 1);

                if (pathGeometry.Figures.Count == 0)
                    pathGeometry.Figures.Add(new PathFigure());

                Update(pathGeometry.Figures[0], GetCenter(pathGeometry), GetRadius(pathGeometry), GetCorners(pathGeometry), GetWaveThickness(pathGeometry));
            }
        }

        private static void Update(PathFigure pathFigure, Point center, double radius, int corners, double waveThickness)
        {
            var segments = pathFigure.Segments;

            static Point MidPoint(Point a, Point b) => new((a.X + b.X) / 2, (a.Y + b.Y) / 2);
            Point AnglePoint(double angle, double radius) => new(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);

            if (corners < 3)
            {
                segments.Clear();
                return;
            }

            var segmentCount = corners * 2;

            while (segments.Count > segmentCount)
                segments.RemoveAt(segments.Count - 1);

            var unitAngle = Math.PI / corners;
            var convex = radius + waveThickness;
            var concave = radius - waveThickness;
            var angle = -unitAngle;
            var current = AnglePoint(angle, concave);

            pathFigure.StartPoint = MidPoint(AnglePoint(angle * 2, convex), current);
            pathFigure.IsClosed = true;

            for (int i = 0; i < segmentCount; i++)
            {
                QuadraticBezierSegment segment;

                if (segments.Count <= i)
                    segments.Add(segment = new QuadraticBezierSegment());
                else if (segments[i] is QuadraticBezierSegment s && s.CanFreeze)
                    segment = s;
                else
                    segments[i] = segment = new QuadraticBezierSegment();

                angle = unitAngle * i;

                Point next = AnglePoint(angle, i % 2 == 0 ? convex : concave);

                segment.Point1 = current;   // 제어점
                segment.Point2 = MidPoint(current, next);   // 끝점

                current = next;
            }
        }
    }
}
