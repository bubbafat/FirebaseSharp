using FirebaseSharp.Portable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace FirebaseWpfDraw
{

    public class IncrementalPayload
    {
        public string path;
        public string data;
    }

    public class PaintQueue
    {
        public Point Point;
        public string Color;
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
        private readonly BackgroundWorker _firebaseWorker = new BackgroundWorker();
        private readonly BlockingCollection<PaintQueue> _queue = new BlockingCollection<PaintQueue>();

        public MainWindow()
        {
            InitializeComponent();

            _brushMap = new Dictionary<string, SolidColorBrush>();

            // the colors that the web demo uses
            string[] colors =
            {
                "fff", "000", "f00", "0f0", "00f", "88f", "f8d", "f88", "f05", "f80", "0f8", "cf0", "08f",
                "408", "ff8", "8ff"
            };

            // cache the brushes for the known colors
            foreach (string color in colors)
            {
                GetBrushFromFirebaseColor(color);
            }

            // this is the worker loop that does all the communication with Firebase
            _firebaseWorker = new BackgroundWorker();
            _firebaseWorker.DoWork += _firebaseWorker_DoWork;
        }

        void _firebaseWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // setup streaming
            _firebase.GetStreaming(string.Empty, 
                added: (s, args) => PaintNewitem(args),
                changed: (s, args) => UpdateExistingItem(args),
                removed: (s, args) => RemovedItem(args));

            // changes are queued so that the UI thread doesn't need
            // to do anything expensive
            while (true)
            {
                PaintQueue queue = _queue.Take();

                try
                {
                    _firebase.PutAsync(FirebaseIdFromPoint(queue.Point), 
                                       string.Format("\"{0}\"", queue.Color)).Wait();
                }
                catch (Exception)
                {
                    // This is really robust
                }
            }
        }

        private string FirebaseIdFromPoint(Point p)
        {
            return string.Format("{0}:{1}", p.X, p.Y);
        }

        private void RemovedItem(ValueRemovedEventArgs args)
        {
            PaintCanvas.Dispatcher.Invoke(() =>
            {
                UIElement found = null;

                foreach (UIElement ui in PaintCanvas.Children)
                {
                    Rectangle r = ui as Rectangle;
                    if (r != null)
                    {
                        if (r.Tag.ToString() == args.Path.Substring(1))
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
            });
        }

        private void UpdateExistingItem(ValueChangedEventArgs args)
        {
            PaintCanvas.Dispatcher.Invoke(() =>
            {
                foreach (UIElement ui in PaintCanvas.Children)
                {
                    Rectangle r = ui as Rectangle;
                    if (r != null)
                    {
                        if (r.Tag.ToString() == args.Path.Substring(1))
                        {
                            r.Fill = GetBrushFromFirebaseColor(args.Data);
                            break;
                        }
                    }
                }
            });
        }

        private void PaintNewitem(ValueAddedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            JToken data = JToken.Parse(args.Data);
            if (data is JValue)
            {
                Point p = NormalizedPointFromFirebase(args.Path.Substring(1));
                Brush b = GetBrushFromFirebaseColor(args.Data.ToString());

                PaintPoint(p, b);   
            }
            else
            {
                JObject directions = (JObject) data;
                foreach (var direction in directions.Properties())
                {
                    Point p = NormalizedPointFromFirebase(direction.Name);
                    Brush b = GetBrushFromFirebaseColor(direction.Value.ToString());

                    PaintPoint(p, b);

                }                
            }
        }


        // wait to start the background worker until the canvas is loaded
        private void PaintCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            _firebaseWorker.RunWorkerAsync();
        }

        // paint a point
        private void PaintCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point firebasePoint = FirebasePointFromCanvas(GetNormalizedPoint(e.GetPosition(PaintCanvas)));
            _queue.Add(new PaintQueue
            {
                Point = firebasePoint,
                Color = "000",
            });
        }

        // this is where we'd paint lines if we wanted
        private void PaintCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            // paint a line
        }

        // paint on the canvas
        void PaintPoint(Point point, Brush brush)
        {
            Point normalized = GetNormalizedPoint(point);
            PaintCanvas.Dispatcher.BeginInvoke((Action)(() => PaintCanvas.Children.Add(RectangleFromPoint(normalized, brush))));
        }


        // "000" -> Black brush
        Brush GetBrushFromFirebaseColor(string color)
        {
            SolidColorBrush brush;
            if (!_brushMap.TryGetValue(color, out brush))
            {
                Color c = (Color)ColorConverter.ConvertFromString(ThreeDigitToSixDigitHex(color));
                brush = new SolidColorBrush(c);
                _brushMap.Add(color, brush);
            }

            return brush;
        }

        // "4:12" -> Point(32,96)
        Point NormalizedPointFromFirebase(string firebaseLoc)
        {
            string[] loc = firebaseLoc.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
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
