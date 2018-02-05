// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Specialized;

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Point.png) 
    /// Point chart.
    /// </summary>
    public class PointChart : Chart
    {
        #region Properties

        public float PointSize { get; set; } = 14;

        public PointMode PointMode { get; set; } = PointMode.Circle;

        public byte PointAreaAlpha { get; set; } = 100;

        protected float ValueRange => this.MaxValue - this.MinValue;

        protected float Value2Range => this.MaxValue2 - this.MinValue2;

        public bool UseValue2 { get; set; } = false;


        /// <summary>
        /// Gets or sets the minimum value from entries. If not defined, it will be the minimum between zero and the 
        /// minimal entry value.
        /// </summary>
        /// <value>The minimum value.</value>
        public float MinValue2
        {
            get
            {
                if (!this.Entries.Any())
                {
                    return 0;
                }

                if (cachedMinValue2.HasValue)
                    return cachedMinValue2.Value;

                if (this.InternalMinValue2 == null)
                {
                    return (cachedMinValue2 = Math.Min(0, this.Entries.Min(x => x.Value2))).Value;
                }

                return (cachedMinValue2 = Math.Min(this.InternalMinValue2.Value, this.Entries.Min(x => x.Value2))).Value;
            }

            set => this.InternalMinValue2 = value;
        }

        /// <summary>
        /// Gets or sets the maximum value from entries. If not defined, it will be the maximum between zero and the 
        /// maximum entry value.
        /// </summary>
        /// <value>The minimum value.</value>
        public float MaxValue2
        {
            get
            {
                if (!this.Entries.Any())
                {
                    return 0;
                }
                if (cachedMaxValue2.HasValue)
                    return cachedMaxValue2.Value;

                if (this.InternalMaxValue2 == null)
                {
                    return (cachedMaxValue2 = Math.Max(0, this.Entries.Max(x => x.Value2))).Value;
                }

                return (cachedMaxValue2 = Math.Max(this.InternalMaxValue2.Value, this.Entries.Max(x => x.Value2))).Value;
            }

            set => this.InternalMaxValue = value;
        }

        float? cachedMaxValue2;
        float? cachedMinValue2;
        /// <summary>
        /// Gets or sets the internal minimum value (that can be null).
        /// </summary>
        /// <value>The internal minimum value.</value>
        protected float? InternalMinValue2 { get; set; }

        /// <summary>
        /// Gets or sets the internal max value (that can be null).
        /// </summary>
        /// <value>The internal max value.</value>
        protected float? InternalMaxValue2 { get; set; }

        #endregion

        #region Methods

        public float CalculateYOrigin(float itemHeight, float headerHeight)
        {
            if (this.MaxValue <= 0)
            {
                return headerHeight;
            }

            if (this.MinValue > 0)
            {
                return headerHeight + itemHeight;
            }

            return headerHeight + ((this.MaxValue / this.ValueRange) * itemHeight);
        }

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var valueLabelSizes = this.MeasureValueLabels();
            var footerHeight = this.CalculateFooterHeight(valueLabelSizes);
            var headerHeight = this.CalculateHeaderHeight(valueLabelSizes);
            var itemSize = this.CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = this.CalculateYOrigin(itemSize.Height, headerHeight);
            var points = this.CalculatePoints(itemSize, origin, headerHeight);

            this.DrawPointAreas(canvas, points, origin);
            this.DrawPoints(canvas, points);
            this.DrawFooter(canvas, points, itemSize, height, footerHeight);
            this.DrawValueLabel(canvas, points, itemSize, height, valueLabelSizes);
        }
        ///
        ///
        protected SKSize CalculateItemSize(int width, int height, float footerHeight, float headerHeight)
        {
            var total = this.Entries.Count();
            var w = this.UseValue2 ? (width / this.Value2Range) : (width - ((total + 1) * this.Margin)) / total;
            var h = height - this.Margin - footerHeight - headerHeight;
            return new SKSize(w, h);
        }

        protected SKPoint[] CalculatePoints(SKSize itemSize, float origin, float headerHeight)
        {
            var result = new List<SKPoint>();
            var value2Range = this.Value2Range;
            for (int i = 0; i < this.Entries.Count(); i++)
            {
                var entry = this.Entries.ElementAt(i);

                var x = this.UseValue2 ? (itemSize.Width * (entry.Value2 - MinValue2)) : this.Margin + (itemSize.Width / 2) + (i * (itemSize.Width + this.Margin));
                var y = headerHeight + (((this.MaxValue - entry.Value) / this.ValueRange) * itemSize.Height);
                var point = new SKPoint(x, y);
                result.Add(point);
            }

            return result.ToArray();
        }

        protected void DrawFooter(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight)
        {
            this.DrawLabels(canvas, points, itemSize, height, footerHeight);
        }

        protected void DrawLabels(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight)
        {
            for (int i = 0; i < this.Entries.Count(); i++)
            {
                var entry = this.Entries.ElementAt(i);
                var point = points[i];

                if (!string.IsNullOrEmpty(entry.Label))
                {
                    using (var paint = new SKPaint())
                    {
                        paint.TextSize = this.LabelTextSize;
                        paint.IsAntialias = true;
                        paint.Color = entry.TextColor;
                        paint.IsStroke = false;

                        var bounds = new SKRect();
                        var text = entry.Label;
                        paint.MeasureText(text, ref bounds);

                        if (bounds.Width > itemSize.Width)
                        {
                            text = text.Substring(0, Math.Min(3, text.Length));
                            paint.MeasureText(text, ref bounds);
                        }

                        if (bounds.Width > itemSize.Width)
                        {
                            text = text.Substring(0, Math.Min(1, text.Length));
                            paint.MeasureText(text, ref bounds);
                        }

                        canvas.DrawText(text, point.X - (bounds.Width / 2), height - this.Margin + (this.LabelTextSize / 2), paint);
                    }
                }
            }
        }

        protected void DrawPoints(SKCanvas canvas, SKPoint[] points)
        {
            if (points.Length > 0 && PointMode != PointMode.None)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = this.Entries.ElementAt(i);
                    var point = points[i];
                    canvas.DrawPoint(point, entry.Color, this.PointSize, this.PointMode);
                }
            }
        }

        protected void DrawPointAreas(SKCanvas canvas, SKPoint[] points, float origin)
        {
            if (points.Length > 0 && this.PointAreaAlpha > 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = this.Entries.ElementAt(i);
                    var point = points[i];
                    var y = Math.Min(origin, point.Y);

                    using (var shader = SKShader.CreateLinearGradient(new SKPoint(0, origin), new SKPoint(0, point.Y), new[] { entry.Color.WithAlpha(this.PointAreaAlpha), entry.Color.WithAlpha((byte)(this.PointAreaAlpha / 3)) }, null, SKShaderTileMode.Clamp))
                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = entry.Color.WithAlpha(this.PointAreaAlpha),
                    })
                    {
                        paint.Shader = shader;
                        var height = Math.Max(2, Math.Abs(origin - point.Y));
                        canvas.DrawRect(SKRect.Create(point.X - (this.PointSize / 2), y, this.PointSize, height), paint);
                    }
                }
            }
        }

        protected void DrawValueLabel(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float height, SKRect[] valueLabelSizes)
        {
            if (points.Length > 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = this.Entries.ElementAt(i);
                    var point = points[i];
                    var isAbove = point.Y > (this.Margin + (itemSize.Height / 2));

                    if (!string.IsNullOrEmpty(entry.ValueLabel))
                    {
                        using (new SKAutoCanvasRestore(canvas))
                        {
                            using (var paint = new SKPaint())
                            {
                                paint.TextSize = this.LabelTextSize;
                                paint.FakeBoldText = true;
                                paint.IsAntialias = true;
                                paint.Color = entry.Color;
                                paint.IsStroke = false;

                                var bounds = new SKRect();
                                var text = entry.ValueLabel;
                                paint.MeasureText(text, ref bounds);

                                canvas.RotateDegrees(90);
                                canvas.Translate(this.Margin, -point.X + (bounds.Height / 2));

                                canvas.DrawText(text, 0, 0, paint);
                            }
                        }
                    }
                }
            }
        }

        protected float CalculateFooterHeight(SKRect[] valueLabelSizes)
        {
            var result = this.Margin;

            if (this.Entries.Any(e => !string.IsNullOrEmpty(e.Label)))
            {
                result += this.LabelTextSize + this.Margin;
            }

            return result;
        }

        protected float CalculateHeaderHeight(SKRect[] valueLabelSizes)
        {
            var result = this.Margin;

            if (this.Entries.Any())
            {
                var maxValueWidth = valueLabelSizes.Max(x => x.Width);
                if (maxValueWidth > 0)
                {
                    result += maxValueWidth + this.Margin;
                }
            }

            return result;
        }

        protected SKRect[] MeasureValueLabels()
        {
            using (var paint = new SKPaint())
            {
                paint.TextSize = this.LabelTextSize;
                return this.Entries.Select(e =>
                {
                    if (string.IsNullOrEmpty(e.ValueLabel))
                    {
                        return SKRect.Empty;
                    }

                    var bounds = new SKRect();
                    var text = e.ValueLabel;
                    paint.MeasureText(text, ref bounds);
                    return bounds;
                }).ToArray();
            }
        }
        protected override void EntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            cachedMaxValue2 = cachedMinValue2 = null;
            base.EntriesCollectionChanged(sender, e);
        }
        #endregion
    }
}
