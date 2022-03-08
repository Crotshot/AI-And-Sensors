using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crotty.Helpers {
    public static class StaticHelpers {

        public enum Plane { XY, XZ, YZ}
        /// <summary>
        /// Returns distance between Vector3s a & b as a float
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Vector3Distance(Vector3 a, Vector3 b) {
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
        }

        public static float Vector2Distance(Vector2 a, Vector2 b) {
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
        }
        /// <summary>
        /// Calculate the Vector2 distance on a plane between two Vector3s
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="planes"></param>
        /// <returns></returns>
        public static float Vector3PlaneDistance(Vector3 a, Vector3 b, Plane p) {
            if (p == Plane.XZ) {//USE XZ plane
                return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
            }
            else if(p == Plane.YZ) {//USE YZ plane
                return Mathf.Sqrt(Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
            }
            else { //USE XY plane
                return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
            }
        }

        /// <summary>
        /// Checks if a point along a line is above or below the line
        /// </summary>
        /// <param name="startY"></param>
        /// <param name="EndY"></param>
        /// <param name="lineLength"></param>
        /// <param name="distance"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static bool Y_Line_Check_Below(float startY, float EndY, float lineLength, float distance, float height) {
            //Interpolate bewtween the two Y values and get the height of the line at distance, if this is above height return false otherwise return true
            float gap = EndY - startY;
            if ((gap * (distance / lineLength)) + startY > height)
                return true;

            return false;
        }


        public static Transform FindClosestTransform(List<Transform> points, Vector3 position) {
            if (points.Count < 1)
                return null;
            Transform trans = points[0];
            foreach (Transform point in points) {
                if(Vector3Distance(position, point.position) < Vector3Distance(position, trans.position)) {
                    trans = point;
                }
            }
            return trans;
        }


        public static List<Transform> FindTransformsOutsideRadius(List<Transform> points, Vector3 position, float distance) {
            List<Transform> poss = new List<Transform>();
            foreach (Transform point in points) {
                if (Vector3Distance(position, point.position) >= distance) {
                    poss.Add(point);
                }
            }
            return poss;
        }

        public static Transform FindFurthestTransform(List<Transform> points, Vector3 position) {
            Transform trans = points[0];
            foreach (Transform point in points) {
                if (Vector3Distance(position, point.position) > Vector3Distance(position, trans.position)) {
                    trans = point;
                }
            }
            return trans;
        }

        public static Transform RandomTransform(List<Transform> points, Vector3 position) {
            return points[Random.Range(0, points.Count)];
        }
    }
}
