// SPOILER: one possible solution for Exercise A. Try it yourself first!
// (This file is outside Assets/, so Unity ignores it.)
using UnityEngine;

namespace Roguelite.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            // A1
            if (target == null)
            {
                return;
            }

            // A2: follow x and y, keep our own z.
            Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);

            // A3
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        }
    }
}
