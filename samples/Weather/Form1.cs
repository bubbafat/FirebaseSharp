using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using FirebaseSharp.Portable;

namespace Weather
{
    public partial class Form1 : Form
    {
        private readonly FirebaseApp _app;

        public Form1()
        {
            InitializeComponent();
            _app = new FirebaseApp(new Uri("https://publicdata-weather.firebaseio.com/"));
        }

        private void UpdateChart(IDataSnapshot snap)
        {
            chart1.Series["Weather"].Points.Clear();
            foreach (var update in snap.Children)
            {
                int index = int.Parse(update.Key);
                float temp = update.Child("apparentTemperature").Value<float>();
                chart1.Series["Weather"].Points.AddXY(index, temp);
            }

            chart1.Invalidate();
        }

        public delegate void UpdateDelegate(IDataSnapshot snap);

        private void Form1_Load(object sender, EventArgs e)
        {
            _app.Child("chicago/hourly/data")
                .On("value", (snap, child, context) => { chart1.BeginInvoke(new UpdateDelegate(UpdateChart), snap); });

            chart1.Series.Clear();
            var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Weather",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line,
            };

            this.chart1.Series.Add(series1);
        }
    }
}