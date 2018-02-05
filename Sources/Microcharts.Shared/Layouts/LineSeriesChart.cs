using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Microcharts
{
    ///
    ///
    public class LineSeriesChart : LineChart
    {
        public LineSeriesChart()
        {
        }

        IEnumerable<LineChart> series;
        public IEnumerable<LineChart> Series
        {
            get { return series; }
            set
            {
                this.Entries = series?.SelectMany(x => x.Entries);
                series = value;
            }
        }

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var valueLabelSizes = MeasureValueLabels();
            var footerHeight = CalculateFooterHeight(valueLabelSizes);
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);

            foreach (var chart in Series)
            {

                var points = this.CalculatePoints(itemSize, origin, headerHeight, chart.Entries);

                this.DrawArea(canvas, points, itemSize, origin);
                this.DrawLine(canvas, points, itemSize);
                this.DrawPoints(canvas, points);
                this.DrawFooter(canvas, points, itemSize, height, footerHeight);
                this.DrawValueLabel(canvas, points, itemSize, height, valueLabelSizes);
            }
        }

        protected SKPoint[] CalculatePoints(SKSize itemSize, float origin, float headerHeight, IEnumerable<Entry> entries)
        {
            var result = new List<SKPoint>();

            for (int i = 0; i < entries.Count(); i++)
            {
                var entry = entries.ElementAt(i);

                var x = this.Margin + (itemSize.Width / 2) + (i * (itemSize.Width + this.Margin));
                var y = headerHeight + (((this.MaxValue - entry.Value) / this.ValueRange) * itemSize.Height);
                var point = new SKPoint(x, y);
                result.Add(point);
            }

            return result.ToArray();
        }
    }
}
