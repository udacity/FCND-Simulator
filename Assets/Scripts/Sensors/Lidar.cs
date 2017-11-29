using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Sensors
{
    public class Lidar : MonoBehaviour
    {
        public static List<Collision> Sense(GameObject obj, List<Quaternion> rotations, float sensorRange)
        {
            RaycastHit hit;
            var collisions = new List<Collision>();
            var pos = obj.transform.position;

            foreach (var r in rotations)
            {
                var dir = r * obj.transform.forward;
                Debug.Log(string.Format("object direction {0}, rotation {1}, ray direction {2}", obj.transform.forward, r, dir));
                if (Physics.Raycast(pos, dir, out hit, sensorRange))
                {
                    var c = new Collision(pos, hit.point, r, hit.distance);
                    collisions.Add(c);
                }
            }
            return collisions;
        }
    }
}