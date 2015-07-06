using FirebaseSharp.Portable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace FirebaseWpfDraw
{

    public class PaintQueue
    {
        public PaintQueue(Point point, string color, ChangeSource source)
        {
            Point = point;
            Color = color;
            Source = source;
        }

        public readonly Point Point;
        public readonly string Color;
        public readonly ChangeSource Source;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // the web demo is at
        // https://www.firebase.com/tutorial/#session/tsqic8m0ifl

        private readonly Dictionary<string, SolidColorBrush> _brushMap;
        private readonly Firebase _firebase = new Firebase("https://tsqic8m0ifl.firebaseio-demo.com/");
        private readonly string[] _colors =
            {
                "fff", "000", "f00", "0f0", "00f", "88f", "f8d", "f88", "f05", "f80", "0f8", "cf0", "08f",
                "408", "ff8", "8ff"
            };

        private Response _streamingResponse;


        public MainWindow()
        {
            InitializeComponent();

            _brushMap = new Dictionary<string, SolidColorBrush>();


            // cache the brushes for the known colors
            foreach (string color in _colors)
            {
                GetBrushFromFirebaseColor(color);
            }
        }

        private void HandleEvent(DataChangedEventArgs args)
        {
            PaintCanvas.Dispatcher.Invoke(() =>
            {
                switch (args.Event)
                {
                    case EventType.Added:
                    case EventType.Changed:
                        PaintItems(args);
                        break;
                    case EventType.Removed:
                        RemoveItems(args);
                        break;
                }

            });
        }

        private void PaintItems(DataChangedEventArgs args)
        {
            if (args.Path == "/")
            {
                JToken change = JToken.Parse(args.Data);

                JObject directions = (JObject)change;
                foreach (var direction in directions.Properties())
                {
                    Point p = NormalizedPointFromFirebase(direction.Name);
                    PaintPoint(p, GetBrushFromFirebaseColor(direction.Value.ToString()));
                }
            }
            else
            {
                Point p = NormalizedPointFromFirebase(NormalizePath(args.Path));
                PaintPoint(p, GetBrushFromFirebaseColor(args.Data));
            }

        }

        private void RemoveItems(DataChangedEventArgs args)
        {
            if (args.Path == "/")
            {
                RemoveAll();
            }
            else
            {
                RemoveItem(NormalizePath(args.Path));
            }
        }


        private string FirebaseIdFromPoint(Point p)
        {
            return string.Format("{0}:{1}", p.X, p.Y);
        }

        private string NormalizePath(string path)
        {
            return path.TrimStart(new[] {'/'});
        }

        private void RemoveAll()
        {
            PaintCanvas.Children.Clear();
        }

        private void RemoveItem(string location)
        {
            UIElement found = null;

            foreach (UIElement ui in PaintCanvas.Children)
            {
                Rectangle r = ui as Rectangle;
                if (r != null)
                {
                    if (r.Tag.ToString() == location)
                    {
                        found = r;
                        break;
                    }
                }
            }

            if (found != null)
            {
                PaintCanvas.Children.Remove(found);
            }
        }

        private Rectangle GetRectangleAtPoint(Point normalizedCanvasPoint)
        {
            string tag = FirebaseIdFromPoint(FirebasePointFromCanvas(normalizedCanvasPoint));
            foreach (UIElement ui in PaintCanvas.Children)
            {
                Rectangle r = ui as Rectangle;
                if (r != null)
                {
                    if (r.Tag.ToString() == tag)
                    {
                        return r;
                    }
                }
            }

            return null;
        }


        // wait to start the background worker until the canvas is loaded
        private void PaintCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            _streamingResponse = _firebase.GetStreamingAsync(string.Empty, handler: (s, args) => HandleEvent(args)).Result;
        }

        // paint a point
        private void PaintCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Enqueue(FirebasePointFromCanvas(GetNormalizedPoint(e.GetPosition(PaintCanvas))));
        }

        // this is where we'd paint lines if we wanted
        private void PaintCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Enqueue(FirebasePointFromCanvas(GetNormalizedPoint(e.GetPosition(PaintCanvas))));
            }
        }

        private readonly Random _rng = new Random();
        private Point _lastPoint = new Point(0, 0);
        private void Enqueue(Point firebasePoint)
        {
            if (firebasePoint != _lastPoint)
            {
                string color = _colors[_rng.Next(0, _colors.Length)];
                _firebase.Set(FirebaseIdFromPoint(firebasePoint), string.Format("\"{0}\"", color));
                _lastPoint = firebasePoint;
            }
        }


        // paint on the canvas
        void PaintPoint(Point normalizedCanvasPoint, Brush brush)
        {
            Rectangle r = GetRectangleAtPoint(normalizedCanvasPoint);
            if (r != null)
            {
                r.Fill = brush;
            }
            else
            {
                PaintCanvas.Children.Add(RectangleFromPoint(normalizedCanvasPoint, brush));
            }
        }


        // "000" -> Black brush
        Brush GetBrushFromFirebaseColor(string color)
        {
            SolidColorBrush brush;
            if (!_brushMap.TryGetValue(color, out brush))
            {
                Color c = (Color)ColorConverter.ConvertFromString(ThreeDigitToSixDigitHex(color.Trim(new []{'\"'})));
                brush = new SolidColorBrush(c);
                _brushMap.Add(color, brush);
            }

            return brush;
        }

        // "4:12" -> Point(32,96)
        Point NormalizedPointFromFirebase(string firebaseLoc)
        {
            string[] loc = NormalizePath(firebaseLoc).Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            int x = int.Parse(loc[0]);
            int y = int.Parse(loc[1]);

            return GetNormalizedPoint(CanvasPointFromFirebase(x, y));
        }

        // [4,12] -> Point(32,96)
        private Point CanvasPointFromFirebase(int x, int y)
        {
            x = Math.Max(x, 0) * 8;
            y = Math.Max(y, 0) * 8;

            return new Point(x, y);
        }

        // Point(32,96) -> Point(4,12)
        private Point FirebasePointFromCanvas(Point point)
        {
            point.X = point.X/8;
            point.Y = point.Y/8;

            return point;
        }

        // "0fa" -> "#00ffaa"
        private string ThreeDigitToSixDigitHex(string threeDigit)
        {
            StringBuilder sb = new StringBuilder(7);
            sb.Append('#');
            sb.Append(threeDigit[0]);
            sb.Append(threeDigit[0]);
            sb.Append(threeDigit[1]);
            sb.Append(threeDigit[1]);
            sb.Append(threeDigit[2]);
            sb.Append(threeDigit[2]);

            return sb.ToString();
        }

        // Point(33, 98) -> POint(32, 96)
        // remember, we're showing 8x8 blocks as a single "pixel"
        static Point GetNormalizedPoint(Point point)
        {
            // align to nearest large pixel boundary
            return new Point(
                (((int)point.X / 8) * 8),
                (((int)point.Y / 8) * 8)
                );
        }

        // figures out where the point is on the rect and 
        // builds a rectangle that can be displayed
        Rectangle RectangleFromPoint(Point point, Brush brush)
        {
            Rectangle r = new Rectangle
            {
                Height = 8,
                Width = 8,
                Fill = brush,
            };

            Canvas.SetLeft(r, point.X);
            Canvas.SetTop(r, point.Y);

            r.Tag = FirebaseIdFromPoint(FirebasePointFromCanvas(point));

            return r;
        }
    }
}
