// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using SkiaSharp;

    internal static class RadialHelpers
    {
        #region Constants

        public const float PI = (float)Math.PI;

        private const float UprightAngle = RadialHelpers.PI / 2f;

        private const float TotalAngle = 2f * RadialHelpers.PI;

        #endregion

        #region Sectors

        public static SKPoint GetCirclePoint(float r, float angle)
        {
            return new SKPoint(r * (float)Math.Cos(angle), r * (float)Math.Sin(angle));
        }

        public static SKPath CreateSectorPath(float start, float end, float outerRadius, float innerRadius = 0.0f, float margin = 0.0f)
        {
            var path = new SKPath();

            // if the sector has no size, then it has no path
            if (start == end)
            {
                return path;
            }

            // the the sector is a full circle, then do that
            if (end - start == 1.0f)
            {
                path.AddCircle(0, 0, outerRadius, SKPathDirection.Clockwise);
                path.AddCircle(0, 0, innerRadius, SKPathDirection.Clockwise);
                path.FillType = SKPathFillType.EvenOdd;
                return path;
            }

            // calculate the angles
            var startAngle = (RadialHelpers.TotalAngle * start) - RadialHelpers.UprightAngle;
            var endAngle = (RadialHelpers.TotalAngle * end) - RadialHelpers.UprightAngle;
            var large = endAngle - startAngle > RadialHelpers.PI ? SKPathArcSize.Large : SKPathArcSize.Small;
            var sectorCenterAngle = ((endAngle - startAngle) / 2f) + startAngle;

            // get the radius bits
            var cectorCenterRadius = ((outerRadius - innerRadius) / 2f) + innerRadius;

            // calculate the angle for the margins
            var offsetR = outerRadius == 0 ? 0 : ((margin / (RadialHelpers.TotalAngle * outerRadius)) * RadialHelpers.TotalAngle);
            var offsetr = innerRadius == 0 ? 0 : ((margin / (RadialHelpers.TotalAngle * innerRadius)) * RadialHelpers.TotalAngle);

            // get the points
            var a = RadialHelpers.GetCirclePoint(outerRadius, startAngle + offsetR);
            var b = RadialHelpers.GetCirclePoint(outerRadius, endAngle - offsetR);
            var c = RadialHelpers.GetCirclePoint(innerRadius, endAngle - offsetr);
            var d = RadialHelpers.GetCirclePoint(innerRadius, startAngle + offsetr);

            // add the points to the path
            path.MoveTo(a);
            path.ArcTo(outerRadius, outerRadius, 0, large, SKPathDirection.Clockwise, b.X, b.Y);
            path.LineTo(c);

            if (innerRadius == 0.0f)
            {
                // take a short cut
                path.LineTo(d);
            }
            else
            {
                path.ArcTo(innerRadius, innerRadius, 0, large, SKPathDirection.CounterClockwise, d.X, d.Y);
            }

            path.Close();

            return path;
        }

        #endregion
    }
}
