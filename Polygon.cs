using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NOOSE
{
    public class Polygon
    {
        public List<PointF> Points;

        public Polygon(List<PointF> pointList)
        {
            Points = new List<PointF>(pointList);
        }

        public bool Contains(PointF pos)
        {
            var coef = Points.ToArray().Skip(1).Select((p, i) =>
                                           (pos.Y - Points[i].Y) * (p.X - Points[i].X)
                                         - (pos.X - Points[i].X) * (p.Y - Points[i].Y))
                                   .ToList();

            if (coef.Any(p => Math.Abs(p) < 0.000001))
                return true;

            for (int i = 1; i < coef.Count(); i++)
            {
                if (coef[i] * coef[i - 1] < 0)
                    return false;
            }
            return true;
        }
    }
}