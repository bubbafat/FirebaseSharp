using FirebaseSharp.Portable;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FirebaseSharp.Portable.Cache;

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

        private readonly Random _rng = new Random();
        private readonly string[] _colors = new[]
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

            // this is the worker loop that does all the communication with Firebase
            _firebaseWorker.DoWork += _firebaseWorker_DoWork;
        }

        private string PickRandomColor()
        {
            return _colors[_rng.Next(0, _colors.Length - 1)];
        }

        async void _firebaseWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // setup streaming
            using (var response = await _firebase.GetStreamingAsync(string.Empty))
            {
                bool done = false;

                response.Added += PaintNewitem;
                response.Changed += UpdateExistingItem;
                response.Removed += RemovedItem;
                response.Closed += (o, args) =>
                {
                    done = true;
                };

                response.Timeout += (o, args) =>
                {
                    MessageBox.Show("Operation timed out");
                };

                response.Error += (o, args) =>
                {
                    MessageBox.Show(args.Error.Message);
                };

                response.Listen();

                List<PaintQueue> items = new List<PaintQueue>();


                // changes are queued so that the UI thread doesn't need
                // to do anything expensive
                while (!done)
                {
                    items.Clear();
                    PaintQueue item = _queue.Take();
                    items.Add(item);

                    // now drain any more that are remaining
                    while (_queue.TryTake(out item))
                    {
                        items.Add(item);
                    }

                    try
                    {
                        await _firebase.PatchAsync("/", 
                            string.Format("{{ {0} }}", FirebaseIdFromPoints(items)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        done = true;
                    }
                }
            }
        }

        private string FirebaseIdFromPoint(Point p)
        {
            return string.Format("{0}:{1}", p.X, p.Y);
        }

        private string FirebaseIdFromPoints(List<PaintQueue> items)
        {
            return string.Join(",", items.Select(i => string.Format("\"{0}\" : \"{1}\"", 
                FirebaseIdFromPoint(i.Point), i.Color)));
        }

        private void RemovedItem(object sender, ValueRemovedEventArgs args)
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

        private void UpdateExistingItem(object sender, ValueChangedEventArgs args)
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

        private void PaintNewitem(object sender, ValueAddedEventArgs args)
        {
            Point p = NormalizedPointFromFirebase(args.Path.Substring(1));
            Brush b = GetBrushFromFirebaseColor(args.Data);

            PaintPoint(p, b);
        }


        // wait to start the background worker until the canvas is loaded
        private void PaintCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            _firebaseWorker.RunWorkerAsync();
        }

        // paint a point
        private void PaintCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point localPoint = GetNormalizedPoint(e.GetPosition(PaintCanvas));
            HandlePoint(localPoint);
        }

        // this is where we'd paint lines if we wanted
        private void PaintCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point localPoint = GetNormalizedPoint(e.GetPosition(PaintCanvas));
                HandlePoint(localPoint);
            }
        }

        void HandlePoint(Point localPoint)
        {
            Point firebasePoint = FirebasePointFromCanvas(localPoint);

            if (!_queue.Any(p => p.Point.Equals(firebasePoint)))
            {
                string color = PickRandomColor();

                _queue.Add(new PaintQueue
                {
                    Point = firebasePoint,
                    Color = color,
                });

                PaintPoint(localPoint, GetBrushFromFirebaseColor(color));
            }
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
