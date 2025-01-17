﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenGL;
using System.Text;
using System.Threading.Tasks;

namespace Castles
{
    public static class Tools
    {
        public static bool isLineInCylinder(Vector3 linePoint1, Vector3 linePoint2, Vector3 cylinderPoint, double radius, double height)
        {
            Vector2 linePointAv = linePoint1.XZ();
            Vector2 linePointBv = linePoint2.XZ();
            Matrix4 m = Matrix4.CreateRotationY(GetAngle(linePointAv, linePointBv));
            linePoint1 = m * linePoint1;
            linePoint2 = m * linePoint2;
            cylinderPoint = m * cylinderPoint;
            Vector2 linePointA = linePoint1.XZ();
            Vector2 linePointB = linePoint2.XZ();
            Vector2 cylinderPoint2D = cylinderPoint.XZ();
            Vector2 orthogonalPoint = GetOrthogonalPoint(linePointA, linePointB, cylinderPoint2D);
            double x = (orthogonalPoint - cylinderPoint2D).Length();
            if(x > radius)
            {
                return false;
            }
            if(x == radius)
            {
                if (linePoint1.Y > linePoint2.Y)
                {
                    Vector3 temp = linePoint1;
                    linePoint1 = linePoint2;
                    linePoint2 = temp;

                    Vector2 temp1 = linePointA;
                    linePointA = linePointB;
                    linePointB = temp1;
                }
                double n = (linePointA - linePointB).Length();
                double b = (linePointA - orthogonalPoint).Length();
                double relation = b / n;
                double yDifference = linePoint1.Y - linePoint2.Y;
                double yOfPoint = linePoint1.Y + (yDifference * relation);
                if(yOfPoint < cylinderPoint.Y && yOfPoint > cylinderPoint.Y + height)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (linePointA.X > linePointB.X)
                {
                    Vector2 temp = linePointA;
                    linePointA = linePointB;
                    linePointB = temp;
                }
                double pitch1 = (linePointB.Y - linePointA.Y) / (linePointB.X - linePointA.X);
                double f = 2 * Math.Sqrt((radius * radius) - (x * x));
                Vector2 corner1 = new Vector2((float)-f/2, 0);
                Vector2 corner2 = new Vector2(corner1.X, corner1.Y + (float)height);
                Vector2 corner3 = new Vector2(corner1.X + (float)f, corner1.Y + (float)height);
                Vector2 corner4 = new Vector2(corner1.X + (float)f, corner1.Y);
                // So wäre Linie in Relation zu Kreismittelpunkt angelegt, dieser entspricht nicht dem des Vierecks
                Vector2 linePointC = new Vector2(cylinderPoint.X - linePoint1.X, cylinderPoint.Y - linePoint1.Y);
                Vector2 linePointD = new Vector2(cylinderPoint.X - linePoint2.X, cylinderPoint.Y - linePoint2.Y);
                if (isIntersecting(linePointC,linePointD,corner1,corner2)||isIntersecting(linePointC,linePointD,corner2,corner3) || isIntersecting(linePointC, linePointD,corner3,corner4) || isIntersecting(linePointC, linePointD, corner4, corner1))
                {
                    return true;
                }
            }
            return false;


        }

        public static bool isLineInCylinder(float x, float y, float z, float x1, float y1, float z1, float x2, float y2, float z2, float radius, float height)
        {
            return isLineInCylinder(new Vector3(x, y, z), new Vector3(x1, y1, z1), new Vector3(x2, y2, z2), radius, height);
        }

        public static Vector2 GetIntersectionPoint(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out bool isIntersecting)
        {
            Vector2 intersection;

            if (point1.X > point2.X)
            {
                Vector2 temp = point1;
                point1 = point2;
                point2 = temp;
            }
            if (point3.X > point4.X)
            {
                Vector2 temp = point3;
                point3 = point4;
                point4 = temp;
            }
            if (point1.X == point2.X && point3.Y == point4.Y)
            {
                intersection = new Vector2(point1.X, point3.Y);
            }
            else if (point3.X == point4.X && point1.Y == point2.Y)
            {
                intersection = new Vector2(point3.X, point1.Y);
            }
            else if (point1.X == point2.X)
            {
                double pitch = (point4.Y - point3.Y) / (point4.X - point3.X);
                double b = point3.Y - pitch * point3.X;
                intersection = new Vector2((float)point1.X, (float)(pitch * point1.X + b));
            }
            else if (point3.X == point4.X)
            {
                double pitch = (point2.Y - point1.Y) / (point2.X - point1.X);
                double b = point1.Y - pitch * point1.X;
                intersection = new Vector2((float)point3.X, (float)(pitch * point3.X + b));

            }
            else if (point1.Y == point2.Y)
            {
                double pitch = (point4.Y - point3.Y) / (point4.X - point3.X);
                double b = point3.Y - pitch * point3.X;
                intersection = new Vector2((float)((point1.Y - b) / pitch), (float)point1.Y);
            }
            else if (point3.Y == point4.Y)
            {
                double pitch = (point2.Y - point1.Y) / (point2.X - point1.X);
                double b = point1.Y - pitch * point1.X;
                intersection = new Vector2((float)((point3.Y - b) / pitch), (float)point3.Y);

            }
            else
            {
                double pitch1 = (point2.Y - point1.Y) / (point2.X - point1.X);
                double b1 = point1.Y - pitch1 * point1.X;
                double pitch2 = (point4.Y - point3.Y) / (point4.X - point3.X);
                double b2 = point3.Y - pitch2 * point3.X;
                double x = (b2 - b1) / (pitch1 - pitch2);
                intersection = new Vector2((float)x, (float)(pitch1 * x + b1));
            }

            isIntersecting = (intersection.X >= point1.X && intersection.X <= point2.X &&
               intersection.X >= point3.X && intersection.X <= point4.X);

                return intersection;
        }
        public static Vector2 GetIntersectionPoint(float x1, float x2, float x3, float x4, float x5, float x6, float x7, float x8, out bool isIntersecting)
        {
            Vector2 v = GetIntersectionPoint(new Vector2(x1, x2), new Vector2(x3, x4), new Vector2(x5, x6), new Vector2(x7, x8), out bool b);
            isIntersecting = b;
            return v;
        }
        public static bool isIntersecting(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
        {
            Vector2 intersection;

            if (point1.X > point2.X)
            {
                Vector2 temp = point1;
                point1 = point2;
                point2 = temp;
            }
            if (point3.X > point4.X)
            {
                Vector2 temp = point3;
                point3 = point4;
                point4 = temp;
            }
            if (point1.X == point2.X && point3.Y == point4.Y)
            {
                intersection = new Vector2(point1.X, point3.Y);
            }
            else if (point3.X == point4.X && point1.Y == point2.Y)
            {
                intersection = new Vector2(point3.X, point1.Y);
            }
            else if (point1.X == point2.X)
            {
                double pitch = (point4.Y - point3.Y) / (point4.X - point3.X);
                double b = point3.Y - pitch * point3.X;
                intersection = new Vector2((float)point1.X, (float)(pitch * point1.X + b));
            }
            else if (point3.X == point4.X)
            {
                double pitch = (point2.Y - point1.Y) / (point2.X - point1.X);
                double b = point1.Y - pitch * point1.X;
                intersection = new Vector2((float)point3.X, (float)(pitch * point3.X + b));

            }
            else if (point1.Y == point2.Y)
            {
                double pitch = (point4.Y - point3.Y) / (point4.X - point3.X);
                double b = point3.Y - pitch * point3.X;
                intersection = new Vector2((float)((point1.Y - b) / pitch), (float)point1.Y);
            }
            else if (point3.Y == point4.Y)
            {
                double pitch = (point2.Y - point1.Y) / (point2.X - point1.X);
                double b = point1.Y - pitch * point1.X;
                intersection = new Vector2((float)((point3.Y - b) / pitch), (float)point3.Y);

            }
            else
            {
                double pitch1 = (point2.Y - point1.Y) / (point2.X - point1.X);
                double b1 = point1.Y - pitch1 * point1.X;
                double pitch2 = (point4.Y - point3.Y) / (point4.X - point3.X);
                double b2 = point3.Y - pitch2 * point3.X;
                double x = (b2 - b1) / (pitch1 - pitch2);
                intersection = new Vector2((float)x, (float)(pitch1 * x + b1));
            }

            bool isIntersecting = (intersection.X >= point1.X && intersection.X <= point2.X && intersection.X >= point3.X && intersection.X <= point4.X);

            return isIntersecting;
        }
        public static Vector2 GetOrthogonalPoint(Vector2 linePoint1, Vector2 linePoint2, Vector2 point)
        {

            if (linePoint1.X > linePoint2.X)
            {
                Vector2 temp = linePoint1;
                linePoint1 = linePoint2;
                linePoint2 = temp;
            }
            if (linePoint1.X == linePoint2.X)
            {
                return new Vector2(linePoint1.X, point.Y);

            }
            if (linePoint1.Y == linePoint2.Y)
            {
                return new Vector2(point.X, linePoint1.Y);
            }
            double pitch1 = (linePoint2.Y - linePoint1.Y) / (linePoint2.X - linePoint1.X);
            double b1 = linePoint1.Y - pitch1 * linePoint1.X;
            double pitch2 = -1 / pitch1;
            double b2 = point.Y - pitch2 * point.X;
            double x = (b2 - b1) / (pitch1 - pitch2);

            Vector2 orthogonalPoint = new Vector2((float)x, (float)(b2 + pitch2 * x));

            return orthogonalPoint;

        }
        public static Vector2 GetOrthogonalPoint(float x, float y, float x1, float y1, float x2, float y2)
        {
            return GetOrthogonalPoint(new Vector2(x, y), new Vector2(x1, y1), new Vector2(x2, y2));
        }

        public static float BarryCentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 pos)
        {
            float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
            float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Y - p3.Z)) / det;
            float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Y - p3.Z)) / det;
            float l3 = 1.0f - l1 - l2;
            return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
        }

        public static float GetAngle(Vector2 start, Vector2 end)
        {
            Vector2 distance = end - start;
            distance.X = Math.Abs(distance.X);
            float angle = (float)Math.Atan((distance.Y) / (distance.Y));
            if (start.X > end.X)
                angle = (float)Math.PI - angle;
            return angle;
        }

        public static float GetAngle(Vector2 point) => GetAngle(new Vector2(0, 0), point);
        
    }
}
