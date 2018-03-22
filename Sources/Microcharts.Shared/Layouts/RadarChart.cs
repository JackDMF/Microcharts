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
    /// 
    /// A radar chart.
    /// </summary>
    public class RadarChart : LineChart
    {
        private const float Epsilon = 0.01f;
        
        public SKColor BorderLineColor { get; set; } = SKColors.LightGray.WithAlpha(110);

        public float BorderLineSize { get; set; } = 2;

        private void DrawBorder(SKCanvas canvas, SKPoint center, float radius)
        {
            using (var paint = new SKPaint()
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

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var total = this.Entries.Count();

            if (total > 0)
            {
                var captionHeight = this.Entries.Max(x =>
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
                });

                var center = new SKPoint((float)width/2, (float)height/2);

                var radius = (Math.Min(width, height) - 2 * Margin) / 2 - captionHeight;

                var angle = 0f;
                var sectorAngle = 360f / total;
                var radialPoints = new List<RadialPoint>();

                var minValue = this.Entries.Min(x => x.Value);
                var maxValue = this.Entries.Max(x => x.Value);
                var normalizedMax = maxValue - minValue;

                foreach (var entry in this.Entries)
                {
                    var normalizedValue = entry.Value - minValue;
                    radialPoints.Add(new RadialPoint(center, normalizedValue / normalizedMax, radius , angle, sectorAngle, entry));
                    angle += sectorAngle;
                }
                
                this.DrawBorder(canvas, center, radius);
                this.DrawLine(canvas, radialPoints, center);
                this.DrawCircles(canvas, radialPoints, center);
                this.DrawPoints(canvas, radialPoints);
                this.DrawCenterBars(canvas, radialPoints, center);
                this.DrawArea(canvas, radialPoints, center);
                this.DrawLabels(canvas, radialPoints, center, radius);
            }
        }

        private void DrawLabels(SKCanvas canvas, IEnumerable<RadialPoint> radialPoints, SKPoint center, float radius)
        {
            foreach (var point in radialPoints)
            {
                var labelPoint = RadialPoint.GetPoint(center, point.CorrectedAngle,
                    radius + this.LabelTextSize + (this.PointSize / 2));
                var alignment = SKTextAlign.Right;

                if ((Math.Abs(point.CorrectedAngle + 90) < Epsilon) || (Math.Abs(point.CorrectedAngle - 90) < Epsilon))
                {
                    alignment = SKTextAlign.Center;
                }
                else if (point.CorrectedAngle > -90 && point.CorrectedAngle < 90)
                {
                    alignment = SKTextAlign.Left;
                }

                canvas.DrawCaptionLabels(point.Entry.Label, point.Entry.TextColor, point.Entry.ValueLabel, point.Entry.Color,
                    this.LabelTextSize, labelPoint, alignment);
            }
        }

        private void DrawPoints(SKCanvas canvas, IEnumerable<RadialPoint> radialPoints)
        {
            foreach (var radialPoint in radialPoints)
            {
                canvas.DrawPoint(radialPoint.Point, radialPoint.Entry.Color, this.PointSize, this.PointMode);
            }
        }

        private void DrawCircles(SKCanvas canvas, IEnumerable<RadialPoint> radialPoints, SKPoint center)
        {
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = this.BorderLineSize,
                Color = this.BorderLineColor,
                PathEffect = SKPathEffect.CreateDash(new[] {this.BorderLineSize, this.BorderLineSize * 2}, 0),
                IsAntialias = true,
            })
            {
                foreach (var valueRadius in radialPoints.Select(x => x.ValueRadius).Distinct())
                {
                    canvas.DrawCircle(center, valueRadius, paint);
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
                    canvas.DrawGradientLine(center, radialPoint.Entry.Color.WithAlpha(0), radialPoint.Point,
                        radialPoint.Entry.Color.WithAlpha((byte) (radialPoint.Entry.Color.Alpha * 0.75f)),
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