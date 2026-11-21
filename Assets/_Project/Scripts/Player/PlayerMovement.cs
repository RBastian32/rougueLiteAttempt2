using UnityEngine;

// LESSON 1: Your first MonoBehaviour.
//
// A "MonoBehaviour" is a script you attach to a GameObject in the scene.
// Unity calls special methods on it for you at the right time:
//   Awake()       -> once, when the object is created (grab references here)
//   Update()      -> once per rendered frame (read input here)
//   FixedUpdate() -> on a fixed timer, 50x/sec by default (move physics objects here)
//
// Why split input and movement across two methods?
// Frame rate varies (30fps, 144fps...), but physics runs on a steady clock.
// Reading input every frame means we never miss a key press; applying it in
// FixedUpdate keeps movement smooth and consistent no matter the frame rate.

// RequireComponent makes Unity automatically add a Rigidbody2D if one is missing,
// and stops you from accidentally removing it. It documents a dependency in code.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // [SerializeField] shows a PRIVATE field in the Inspector so you can tweak it
    // without making it public to every other script. This is the common practice:
    // keep fields private, expose them to the editor only when designers need them.
    [SerializeField] private float moveSpeed = 5f;

    // Cached reference to the physics body. GetComponent is slow-ish, so we
    // look it up once in Awake instead of every frame.
    private Rigidbody2D body;

    // The direction the player wants to move this frame. Vector2 = (x, y).
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        // Top-down game: we don't want gravity pulling the player "down" the screen,
        // and we don't want collisions to spin the player around.
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void Update()
    {
        // "Horizontal"/"Vertical" are axes defined in Edit > Project Settings > Input Manager.
        // They already map to WASD and the arrow keys. GetAxisRaw returns -1, 0, or 1
        // (no smoothing), which feels snappy for action games.
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Normalizing fixes the classic bug where moving diagonally is faster:
        // (1,1) has length ~1.41, but normalized it has length 1.
        moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        // Setting velocity lets the physics engine move us AND handle collisions with walls.
        // (Changing transform.position directly would make us pass through walls.)
        body.velocity = moveInput * moveSpeed;
    }
}
