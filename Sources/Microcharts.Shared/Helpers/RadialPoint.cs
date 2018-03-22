using System;
using SkiaSharp;

namespace Microcharts
{
    public class RadialPoint
    {
        private const float Offset = 0.1f;
        private const float Distance = 0.5f;

        public SKPoint PreviousControlPoint { get; }

        public SKPoint Point { get; }

        public SKPoint NextControlPoint { get; }

        public SKPoint BorderPoint { get; }

        public Entry Entry { get; }

        public float ValueRadius { get; }

        public float CorrectedAngle { get; }

        public RadialPoint(SKPoint center, float value, float radius, float currentAngle, float sectorAngle, Entry entry)
        {
            this.CorrectedAngle = currentAngle - 90;
            this.ValueRadius = value * (radius - 2 * Offset * radius) + Offset * radius;
            this.Entry = entry;

            var sectorPart = sectorAngle / 2 * Distance;
            var previousAngle = CorrectedAngle - sectorPart;
            var nextAngle = CorrectedAngle + sectorPart;
            
            this.PreviousControlPoint = CalculateControlPoint(center, this.ValueRadius, sectorPart, previousAngle);
            this.Point = GetPoint(center, CorrectedAngle, this.ValueRadius);
            this.NextControlPoint = CalculateControlPoint(center, this.ValueRadius, sectorPart, nextAngle);
            this.BorderPoint = GetPoint(center, CorrectedAngle, radius);
        }

        public static SKPoint GetPoint(SKPoint center, float angle, float radius)
        {
            var radAngle = DegreesToRadians(angle);
            var x = Math.Cos(radAngle) * radius;
            var y = Math.Sin(radAngle) * radius;
            return new SKPoint((float) x, (float) y) + center;
        }

        private static SKPoint CalculateControlPoint(SKPoint center, float radius, float sectorPart, float angle)
            => new SKPoint(
                   (float) (radius / Math.Cos(DegreesToRadians(sectorPart)) * Math.Cos(DegreesToRadians(angle))),
                   (float) (radius / Math.Cos(DegreesToRadians(sectorPart)) *
                            Math.Cos(DegreesToRadians(90 - angle)))) + center;

        private static float DegreesToRadians(float degrees)
        {
            return (float) (Math.PI / 180 * degrees);
        }
    }
}