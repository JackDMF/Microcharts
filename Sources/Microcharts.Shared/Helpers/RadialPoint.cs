// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using SkiaSharp;

    public class RadialPoint
    {
        private const float Offset = 0.1f;
        private const float Distance = 0.5f;

        public RadialPoint(SKPoint center, float value, float radius, float currentAngle, float sectorAngle, Entry entry)
        {
            this.CorrectedAngle = currentAngle - 90;
            this.ValueRadius = RadialPoint.CalculateValueRadius(value, radius);
            this.Entry = entry;

            var sectorPart = sectorAngle / 2 * RadialPoint.Distance;
            var previousAngle = this.CorrectedAngle - sectorPart;
            var nextAngle = this.CorrectedAngle + sectorPart;
            
            this.PreviousControlPoint = RadialPoint.CalculateControlPoint(center, this.ValueRadius, sectorPart, previousAngle);
            this.Point = RadialPoint.GetPoint(center, this.CorrectedAngle, this.ValueRadius);
            this.NextControlPoint = RadialPoint.CalculateControlPoint(center, this.ValueRadius, sectorPart, nextAngle);
            this.BorderPoint = RadialPoint.GetPoint(center, this.CorrectedAngle, radius);
        }

        public SKPoint PreviousControlPoint { get; }

        public SKPoint Point { get; }

        public SKPoint NextControlPoint { get; }

        public SKPoint BorderPoint { get; }

        public Entry Entry { get; }

        public float ValueRadius { get; }

        public float CorrectedAngle { get; }

        public static SKPoint GetPoint(SKPoint center, float angle, float radius)
        {
            var radAngle = RadialPoint.DegreesToRadians(angle);
            var x = Math.Cos(radAngle) * radius;
            var y = Math.Sin(radAngle) * radius;
            return new SKPoint((float)x, (float)y) + center;
        }

        public static float CalculateValueRadius(float value, float radius)
            => (value * (radius - (2 * RadialPoint.Offset * radius))) + (RadialPoint.Offset * radius);

        private static SKPoint CalculateControlPoint(SKPoint center, float radius, float sectorPart, float angle)
            => new SKPoint(
                   (float)(radius / Math.Cos(RadialPoint.DegreesToRadians(sectorPart)) * Math.Cos(RadialPoint.DegreesToRadians(angle))),
                   (float)(radius / Math.Cos(RadialPoint.DegreesToRadians(sectorPart)) * Math.Cos(RadialPoint.DegreesToRadians(90 - angle))))
               + center;

        private static float DegreesToRadians(float degrees)
        {
            return (float)(Math.PI / 180 * degrees);
        }
    }
}