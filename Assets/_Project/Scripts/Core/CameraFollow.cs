using UnityEngine;
using Roguelite.Dungeon;

namespace Roguelite.Core
{
    // LESSON 2 - EXERCISE A: A camera that smoothly follows a target.
    //
    // Attach this to the Main Camera and drag the Player into the "Target" slot.
    // Read "Exercise A" in Docs/Lessons/02-grid-and-tilemap/README.md first.
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("The Transform the camera should follow (usually the Player).")]
        [SerializeField] private Transform target;

        [Tooltip("Roughly how many seconds the camera takes to catch up. 0 = snap instantly.")]
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private MapRenderer mapRenderer;
        [SerializeField] private Camera cam;

        // Vector3.SmoothDamp needs somewhere to remember how fast the camera is
        // currently moving, from one frame to the next. That's this field.
        // You never set it yourself; you pass it to SmoothDamp with the "ref" keyword.
        private Vector3 velocity;

        private void Awake()
        {
            if (mapRenderer == null)
            {
                mapRenderer = FindObjectOfType<MapRenderer>();
            }
            if(cam == null)
            {
                cam = Camera.main;
            }
        }
        
        // LateUpdate runs AFTER every Update() in the scene has finished.
        // Cameras go here so they move after the thing they follow has moved this frame.
        private void LateUpdate()
        {
            // TODO A1: If there is no target, stop here (return).
            //          Otherwise you'll get a NullReferenceException every frame.
            if(target == null) return;
            Vector3 goal = new Vector3 (target.position.x, target.position.y, transform.position.z);

            if(mapRenderer != null && mapRenderer.Grid != null)
            {
                // TODO A4: Clamp the camera's position so it can't show outside the map.
                //          Use Mathf.Clamp(value, min, max) to clamp each axis separately.
                //          The camera's half-size in world units is cam.orthographicSize vertically,
                //          and cam.orthographicSize * cam.aspect horizontally.
                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;

                float minX = halfWidth;
                float maxX = mapRenderer.Grid.Width - halfWidth;
                float minY = halfHeight;
                float maxY = mapRenderer.Grid.Height - halfHeight;

                
                goal.x = Mathf.Clamp(goal.x, minX, maxX);
                goal.y = Mathf.Clamp(goal.y, minY, maxY);
                
            }

            // TODO A2: Work out where the camera WANTS to be.
            //          It should match the target's x and y, but KEEP the camera's own z.
            //          (A 2D camera sits at z = -10 looking "into" the screen. If you copy
            //          the player's z of 0, the camera would be inside the sprites and show nothing.)
            
            // TODO A3: Move the camera part of the way there using Vector3.SmoothDamp,
            //          and assign the result to transform.position.
            //          Signature: Vector3.SmoothDamp(current, targetPosition, ref velocity, smoothTime)
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
            
        }
    }
}
