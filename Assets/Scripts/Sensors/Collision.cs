using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// A `Collision` struct in the this context consists of four components:
    /// 
    /// * The `origin` which can be thought as the current
    /// location of the drone.
    /// * The `target` which is the location of the collision.
    /// * The `rotation` is the direction the drone must fly in order
    /// to collide.
    /// * The `distance` is the distance the drone must fly in order to collide.
    /// </summary>
    public struct Collision
    {
        public Vector3 origin {get;}
        public Vector3 target {get;}
        public Quaternion rotation {get;}
        public float distance {get;}

        public Collision(Vector3 origin, Vector3 target, Quaternion rotation, float distance)
        {
            this.origin = origin;
            this.target = target;
            this.distance = distance;
            this.rotation = rotation;
        }
    }
}