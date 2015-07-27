using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FirebaseSharp.Portable;

namespace FirebaseWpfDraw
{
    public class PaintQueue
    {
        public PaintQueue(Point point, string color)
        {
            Point = point;
            Color = color;
        }

        public readonly Point Point;
        public readonly string Color;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // the web demo is at
        // https://www.firebase.com/tutorial/#session/tsqic8m0ifl

        private readonly Dictionary<string, SolidColorBrush> _brushMap;
        private readonly FirebaseApp _app = new FirebaseApp(new Uri("https://tsqic8m0ifl.firebaseio-demo.com/"));

        private readonly string[] _colors =
        {
            "fff", "000", "f00", "0f0", "00f", "88f", "f8d", "f88", "f05", "f80", "0f8", "cf0", "08f",
            "408", "ff8", "8ff"
        };

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

        private void AddOrUpdate(IDataSnapshot snap)
        {
            Dispatcher.Invoke(() => PaintItem(snap));
        }

        private void Removed(IDataSnapshot snap)
        {
            Dispatcher.Invoke(() => RemoveItem(snap));
        }

        private void PaintItem(IDataSnapshot snap)
        {
            Point p = NormalizedPointFromFirebase(snap.Key);
            PaintPoint(p, GetBrushFromFirebaseColor(snap.Value()));
        }

        private void RemoveItem(IDataSnapshot snap)
        {
            UIElement found = null;

            foreach (UIElement ui in PaintCanvas.Children)
            {
                Rectangle r = ui as Rectangle;
                if (r != null)
                {
                    if (r.Tag.ToString() == snap.Key)
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


        private string FirebaseIdFromPoint(Point p)
        {
            return string.Format("{0}:{1}", p.X, p.Y);
        }

        private void RemoveAll()
        {
            PaintCanvas.Children.Clear();
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
            _app.Child("/").On("child_added", (snap, previous_child, context) => AddOrUpdate(snap));
            _app.Child("/").On("child_changed", (snap, previous_child, context) => AddOrUpdate(snap));
            _app.Child("/").On("child_removed", (snap, previous_child, context) => Removed(snap));
        }

        // paint a point
        private void PaintCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            string path = FirebaseIdFromPoint(FirebasePointFromCanvas(GetNormalizedPoint(e.GetPosition(PaintCanvas))));
            string data = string.Format("\"{0}\"", _colors[_rng.Next(0, _colors.Length)]);

            _app.Child(path).Set(data);
        }

        private string _lastPoint = null;

        // this is where we'd paint lines if we wanted
        private void PaintCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                string path =
                    FirebaseIdFromPoint(FirebasePointFromCanvas(GetNormalizedPoint(e.GetPosition(PaintCanvas))));
                if (_lastPoint != path)
                {
                    string data = string.Format("\"{0}\"", _colors[_rng.Next(0, _colors.Length)]);
                    _app.Child(path).Set(data);
                    _lastPoint = path;
                }
            }
            else
            {
                _lastPoint = null;
            }
        }

        private readonly Random _rng = new Random();

        // paint on the canvas
        private void PaintPoint(Point normalizedCanvasPoint, Brush brush)
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
        private Brush GetBrushFromFirebaseColor(string color)
        {
            SolidColorBrush brush;
            if (!_brushMap.TryGetValue(color, out brush))
            {
                Color c = (Color) ColorConverter.ConvertFromString(ThreeDigitToSixDigitHex(color.Trim(new[] {'\"'})));
                brush = new SolidColorBrush(c);
                _brushMap.Add(color, brush);
            }

            return brush;
        }

        // "4:12" -> Point(32,96)
        private Point NormalizedPointFromFirebase(string firebaseLoc)
        {
            string[] loc = firebaseLoc.Split(new[] {':'}, 2, StringSplitOptions.RemoveEmptyEntries);
            int x = int.Parse(loc[0]);
            int y = int.Parse(loc[1]);

            return GetNormalizedPoint(CanvasPointFromFirebase(x, y));
        }

        // [4,12] -> Point(32,96)
        private Point CanvasPointFromFirebase(int x, int y)
        {
            x = Math.Max(x, 0)*8;
            y = Math.Max(y, 0)*8;

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
        private static Point GetNormalizedPoint(Point point)
        {
            // align to nearest large pixel boundary
            return new Point(
                (((int) point.X/8)*8),
                (((int) point.Y/8)*8)
                );
        }

        // figures out where the point is on the rect and 
        // builds a rectangle that can be displayed
        private Rectangle RectangleFromPoint(Point point, Brush brush)
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