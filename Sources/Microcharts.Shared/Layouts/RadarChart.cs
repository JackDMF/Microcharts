// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Radar.png)
    /// A radar chart.
    /// </summary>
    /// <inheritdoc cref="LineChart"/>
    public class RadarChart : LineChart
    {
        private const float Epsilon = 0.01f;

        public SKColor BorderLineColor { get; set; } = SKColors.LightGray.WithAlpha(110);

        public float BorderLineSize { get; set; } = 2;

        public bool ShowLabels { get; set; }

        public bool ShowCenterBars { get; set; }

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var total = this.Entries.Count();

            if (total > 0)
            {
                var captionHeight =
                    this.ShowLabels
                        ? this.Entries.Max(x => this.GetCaptionHeight(x))
                        : 0;

                var captionWidth =
                    this.ShowLabels
                        ? this.Entries.Max(x => this.GetCaptionWidth(x))
                        : 0;

                var center = new SKPoint((float)width / 2, (float)height / 2);

                var radius = ((Math.Min(width, height) - (2 * this.Margin)) / 2) - (captionHeight >= captionWidth ? captionHeight : captionWidth);

                var angle = 0f;
                var sectorAngle = 360f / total;
                var radialPoints = new List<RadialPoint>();

                var normalizedMax = this.MaxValue - this.MinValue;

                foreach (var entry in this.Entries)
                {
                    var normalizedValue = entry.Value - this.MinValue;
                    radialPoints.Add(new RadialPoint(center, normalizedValue / normalizedMax, radius, angle, sectorAngle, entry));
                    angle += sectorAngle;
                }
                
                this.DrawBorder(canvas, center, radius);
                this.DrawLine(canvas, radialPoints, center);
                this.DrawCircles(canvas, radialPoints, center, radius, 1.0f);
                this.DrawPoints(canvas, radialPoints);
                if (this.ShowCenterBars)
                {
                    this.DrawCenterBars(canvas, radialPoints, center);
                }
                this.DrawArea(canvas, radialPoints, center);
                if (this.ShowLabels)
                {
                    this.DrawLabels(canvas, radialPoints, center, radius);
                }
            }
        }

        private float GetCaptionHeight(Entry x)
        {
            var result = 0.0f;

            var hasLabel = !string.IsNullOrEmpty(x.Label);
            var hasValueLabel = !string.IsNullOrEmpty(x.ValueLabel);
            if (hasLabel || hasValueLabel)
            {
                var hasOffset = hasLabel && hasValueLabel;
                var captionMargin = this.LabelTextSize * 0.60f;
                result += hasOffset ? captionMargin : 0;

                if (hasLabel)
                {
                    result += this.LabelTextSize;
                }

                if (hasValueLabel)
                {
                    result += this.LabelTextSize;
                }
            }

            return result;
        }

        private float GetCaptionWidth(Entry x)
        {
            using (var paint = new SKPaint
                               {
                                   TextSize = this.LabelTextSize,
                                   IsAntialias = true,
                                   IsStroke = false
                               })
            {
                var result = 0.0f;

                var hasLabel = !string.IsNullOrEmpty(x.Label);
                var hasValueLabel = !string.IsNullOrEmpty(x.ValueLabel);

                if (hasLabel || hasValueLabel)
                {
                    result += this.LabelTextSize * 0.6f;
                    var bounds = new SKRect();

                    if (hasLabel)
                    {
                        var text = x.Label;
                        paint.MeasureText(text, ref bounds);
                        result += bounds.Width;
                    }

                    if (hasValueLabel)
                    {
                        var text = x.ValueLabel;
                        paint.MeasureText(text, ref bounds);
                        result += bounds.Width;
                    }
                }
                return result;
            }
        }

        private void DrawBorder(SKCanvas canvas, SKPoint center, float radius)
        {
            using (var paint = new SKPaint
                               {
                                   Style = SKPaintStyle.Stroke,
                                   StrokeWidth = this.BorderLineSize,
                                   Color = this.BorderLineColor,
                                   IsAntialias = true,
                               })
            {
                canvas.DrawCircle(center, radius, paint);
            }
        }

        private void DrawLabels(SKCanvas canvas, IEnumerable<RadialPoint> radialPoints, SKPoint center, float radius)
        {
            foreach (var point in radialPoints)
            {
                var labelPoint = RadialPoint.GetPoint(
                                                      center,
                                                      point.CorrectedAngle,
                                                      radius + this.LabelTextSize + (this.PointSize / 2));
                var alignment = SKTextAlign.Right;

                if ((Math.Abs(point.CorrectedAngle + 90) < RadarChart.Epsilon) || (Math.Abs(point.CorrectedAngle - 90) < RadarChart.Epsilon))
                {
                    alignment = SKTextAlign.Center;
                }
                else if (point.CorrectedAngle > -90 && point.CorrectedAngle < 90)
                {
                    alignment = SKTextAlign.Left;
                }

                canvas.DrawCaptionLabels(
                                         point.Entry.Label,
                                         point.Entry.TextColor,
                                         point.Entry.ValueLabel,
                                         point.Entry.Color,
                                         this.LabelTextSize,
                                         labelPoint,
                                         alignment);
            }
        }

        private void DrawPoints(SKCanvas canvas, IEnumerable<RadialPoint> radialPoints)
        {
            foreach (var radialPoint in radialPoints)
            {
                canvas.DrawPoint(radialPoint.Point, radialPoint.Entry.Color, this.PointSize, this.PointMode);
            }
        }

        private void DrawCircles(
            SKCanvas canvas,
            IEnumerable<RadialPoint> radialPoints,
            SKPoint center,
            float radius,
            float? drawInterval = null)
        {
            var normalizedMax = this.MaxValue - this.MinValue;
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = this.BorderLineSize,
                Color = this.BorderLineColor,
                PathEffect = SKPathEffect.CreateDash(new[] { this.BorderLineSize, this.BorderLineSize * 2 }, 0),
                IsAntialias = true,
            })
            {
                if (drawInterval.HasValue)
                {
                    for (var value = this.MinValue; value <= this.MaxValue; value += drawInterval.Value)
                    {
                        var normalizedValue = value - this.MinValue;
                        canvas.DrawCircle(center, RadialPoint.CalculateValueRadius(normalizedValue / normalizedMax, radius), paint);
                    }
                }
                else
                {
                    foreach (var valueRadius in radialPoints.Select(x => x.ValueRadius).Distinct())
                    {
                        canvas.DrawCircle(center, valueRadius, paint);
                    }
                }
            }
        }

        private void DrawLine(SKCanvas canvas, IReadOnlyList<RadialPoint> points, SKPoint center)
        {
            if (points.Count > 1 && this.LineMode != LineMode.None)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.White,
                    StrokeWidth = this.LineSize,
                    IsAntialias = true,
                })
                {
                    this.DrawPath(canvas, points, center, paint);
                }
            }
        }

        private void DrawArea(SKCanvas canvas, IReadOnlyList<RadialPoint> points, SKPoint center)
        {
            if (this.LineAreaAlpha > 0 && points.Count > 1)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White,
                    IsAntialias = true,
                })
                {
                    this.DrawPath(canvas, points, center, paint);
                }
            }
        }

        private void DrawCenterBars(SKCanvas canvas, IEnumerable<RadialPoint> radialPoints, SKPoint center)
        {
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = this.BorderLineSize,
                Color = this.BorderLineColor,
                IsAntialias = true,
            })
            {
                foreach (var radialPoint in radialPoints)
                {
                    canvas.DrawGradientLine(
                                            center,
                                            radialPoint.Entry.Color.WithAlpha(0),
                                            radialPoint.Point,
                                            radialPoint.Entry.Color.WithAlpha((byte)(radialPoint.Entry.Color.Alpha * 0.75f)),
                                            this.LineSize);
                    canvas.DrawLine(radialPoint.Point, radialPoint.BorderPoint, paint);
                }
            }
        }

        private void DrawPath(SKCanvas canvas, IReadOnlyList<RadialPoint> points, SKPoint center, SKPaint paint)
        {
            using (var shader = this.CreateGradient(center, this.LineAreaAlpha))
            {
                paint.Shader = shader;

                var path = new SKPath();

                path.MoveTo(points.First().Point);

                for (var i = 0; i < points.Count; i++)
                {
                    if (this.LineMode == LineMode.Spline)
                    {
                        var next = points[(i + 1) % points.Count];
                        path.CubicTo(points[i].NextControlPoint, next.PreviousControlPoint, next.Point);
                    }
                    else if (this.LineMode == LineMode.Straight)
                    {
                        path.LineTo(points[(i + 1) % points.Count].Point);
                    }
                }

                path.Close();

                canvas.DrawPath(path, paint);
            }
        }
        
        private SKShader CreateGradient(SKPoint center, byte alpha = 255)
        {
            var rotation = SKMatrix.MakeRotationDegrees(-90, center.X, center.Y);
            var colors = this.Entries.Select(x => x.Color.WithAlpha(alpha)).ToList();
            colors.Add(this.Entries.First().Color.WithAlpha(alpha));
            return SKShader.CreateSweepGradient(
                center,
                colors.ToArray(),
                null,
                rotation);
        }
    }
}