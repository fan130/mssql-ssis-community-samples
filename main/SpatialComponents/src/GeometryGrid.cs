using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;

namespace Microsoft.Samples.SqlServer.SSIS.SpatialComponents
{
    public class GeometryGrid
    {
        SqlGeometry geometry = null;
        double stepX = 1.0;
        double stepY = 1.0;

        public GeometryGrid(SqlGeometry geometry)
        {
            this.geometry = geometry;
        }

        public GeometryGrid(SqlGeometry geometry, double stepX, double stepY)
        {
            this.geometry = geometry;
            this.stepX = stepX;
            this.stepY = stepY;
        }

        public ICollection<SqlGeometry> CutIt()
        {
            SqlGeometry envelope = this.geometry.STEnvelope();

            SqlGeometry point1 = envelope.STPointN(1);
            SqlGeometry point2 = envelope.STPointN(3);

            double minX = (double)(stepX * ((int)(point1.STX / stepX) - (point1.STX < 0.0 ? 1 : 0)));
            double minY = (double)(stepY * ((int)(point1.STY / stepY) - (point1.STY < 0.0 ? 1 : 0)));

            double maxX = (double)(stepX * ((int)(point2.STX / stepX) + (point2.STX > 0.0 ? 1 : 0)));
            double maxY = (double)(stepY * ((int)(point2.STY / stepY) + (point2.STY > 0.0 ? 1 : 0)));

            int countX = (int)((maxX - minX) / stepX);
            int countY = (int)((maxY - minY) / stepY);

            return CutIt(minX, minY, countX, countY);
        }

        private ICollection<SqlGeometry> CutIt(double minX, double minY, int countX, int countY)
        {
            List<SqlGeometry> geoList = new List<SqlGeometry>();

            double currentX = minX;
            double currentY = minY;

            if (countX == 1 && countY == 1)
            {
                geoList.Add(this.geometry);
            }
            else
            {
                for (int i = 0; i <= countY; i++)
                {
                    currentX = minX;
                    for (int j = 0; j <= countX; j++)
                    {
                        SqlGeometry gridRect = GetRect(currentX, currentY);

                        if (this.geometry.STContains(gridRect))
                        {
                            geoList.Add(gridRect);
                        }
                        else if (gridRect.STContains(this.geometry))
                        {
                            geoList.Add(this.geometry);
                        }
                        else
                        {
                            SqlGeometry commonObject = this.geometry.STIntersection(gridRect);

                            if (commonObject != null && !commonObject.STIsEmpty())
                            {
                                geoList.Add(commonObject);
                            }
                        }

                        currentX += stepX;
                    }
                    currentY += stepY;
                }
            }

            return geoList;
        }

        private SqlGeometry GetRect(double currentX, double currentY)
        {
            SqlGeometryBuilder builder = new SqlGeometryBuilder();
            builder.SetSrid(this.geometry.STSrid.Value);
            builder.BeginGeometry(OpenGisGeometryType.Polygon);
            builder.BeginFigure(currentX, currentY);
            builder.AddLine(currentX + stepX, currentY);
            builder.AddLine(currentX + stepX, currentY + stepY);
            builder.AddLine(currentX, currentY + stepY);
            builder.AddLine(currentX, currentY);

            builder.EndFigure();
            builder.EndGeometry();

            return builder.ConstructedGeometry;
        }
    }
}
