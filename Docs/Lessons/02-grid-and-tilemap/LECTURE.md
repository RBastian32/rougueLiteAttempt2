# Lecture 2: Spatial Data: Time, Vectors, Grids, and Tilemaps

**Course:** Game Development Fundamentals with Unity & C#
**Prerequisite:** Lecture 1 (MonoBehaviours, `Update` / `FixedUpdate`, `Rigidbody2D`)
**Lab:** [Lab 2](README.md): Exercises A (camera), B (map grid), C (map renderer)
**Companion material:** [Lecture slides](https://claude.ai/artifact/4WEHL1yUd5v2KUy2oSiKQP) (52 slides, the main lecture) · [Guided-notes slides](https://claude.ai/artifact/FwxucXJn12wV3n6R9VXWej) (the same slides with blanks to fill in)

---

## Learning objectives

By the end of this lecture you should be able to:

1. **Describe** the order in which Unity runs `Update`, `FixedUpdate` and `LateUpdate` in a frame, and choose the right one for a given task.
2. **Explain** the difference between a *value type* and a *reference type* in C#, and predict what happens when each is copied.
3. **Distinguish** assignment (`=`) from equality (`==`), and explain why `if (target = null)` compiles in Unity.
4. **Compute** linear interpolation by hand and explain why naive `Lerp` smoothing depends on frame rate.
5. **Implement** a 2D grid using a rectangular array, and convert between 2D coordinates and a 1D index.
6. **Analyse** the time and memory cost of common grid operations using Big-O notation.
7. **Apply** defensive programming: bounds checks, guard clauses and sentinel values.
8. **Enumerate** a cell's 4- and 8-neighbourhoods using direction arrays.
9. **Write** a unit test following the Arrange–Act–Assert pattern.
10. **Identify** the components of Unity's Tilemap system and convert between grid and world coordinates.

---

## Lecture map

| Unit | Topic | Used in lab |
|---|---|---|
| 1 | Time in a game engine | Exercise A |
| 2 | Values, references, and operators | Exercises A & B |
| 3 | Interpolation and smoothing | Exercise A |
| 4 | Arrays and grids | Exercise B |
| 5 | Coordinate systems | Exercises B & C |
| 6 | Defensive programming | Exercise B |
| 7 | Neighbourhoods and iteration | Exercise B |
| 8 | Designing a data class | Exercise B |
| 9 | Unit testing | Exercise B |
| 10 | Unity's Tilemap system | Exercise C |

---

# Unit 1: Time in a game engine

### 1.1 The game loop

> **Definition: Game loop.** The never-ending cycle at the heart of every game:
> *read input → update the world → draw the world → repeat.* One pass through the loop is
> one **frame**.

```
while (gameIsRunning)
{
    ReadInput();
    UpdateWorld();
    Render();
}
```

You never write this loop in Unity, because the engine owns it. Instead, you write methods that the engine *calls* at specific points in the loop. This is known as **inversion of control**: "don't call us, we'll call you."

> **Definition: Frame rate.** How many frames are completed per second (FPS). It is *not*
> constant: it changes with hardware, scene complexity and what else the computer is doing.

> **Definition: Delta time (`Time.deltaTime`).** The number of seconds the previous frame
> took. At 60 FPS, `deltaTime ≈ 0.0167`. At 30 FPS, `deltaTime ≈ 0.033`.

**Example.** Moving 5 units per second regardless of frame rate:

```csharp
// WRONG: moves 5 units per FRAME. At 144 FPS that's 720 units/sec; at 30 FPS it's 150.
transform.position += Vector3.right * 5f;

// RIGHT: multiply by deltaTime to convert "per frame" into "per second".
transform.position += Vector3.right * 5f * Time.deltaTime;
```

Check: at 60 FPS, the right version moves 5 × 0.0167 ≈ 0.083 units per frame, and × 60 frames gives 5 units. ✔

### 1.2 Two clocks: variable and fixed

Unity actually runs **two** clocks:

| Clock | Callback | Rate | Use it for |
|---|---|---|---|
| Variable (render) | `Update()` | Once per frame, rate varies | Input, timers, non-physics movement |
| Fixed (physics) | `FixedUpdate()` | Exactly every `Time.fixedDeltaTime` (default 0.02 s, which is 50 Hz) | Anything touching a `Rigidbody2D` |
| After update | `LateUpdate()` | Once per frame, after **all** `Update`s | Cameras, things that follow other things |

Physics needs a fixed step because physics engines are only stable and *deterministic* (same input gives the same output) when time advances in equal slices.

### 1.3 The order within one frame (simplified)

```
┌──────────────── one rendered frame ─────────────────┐
│ FixedUpdate()  ×0, 1, 2 … times (catch up physics)  │
│ Physics simulation step(s)                          │
│ Update()        on every script                     │
│ LateUpdate()    on every script                     │
│ Render                                              │
└─────────────────────────────────────────────────────┘
```

`FixedUpdate` may run **zero** times in a frame (fast computer) or **several** times (slow computer catching up).

**Worked example.** At 120 FPS, each frame lasts 1/120 ≈ 0.0083 s. Physics steps every 0.02 s, so it runs about once every 2.4 frames. In some frames the player's physics position changes, and in others it doesn't, so it appears to move in a stop–start pattern. That's the stutter you saw in Lab 2A.

### 1.4 Interpolation

> **Definition: Rigidbody interpolation.** A setting that makes Unity draw a physics object
> at a position *blended between its last two physics positions*, based on how far the
> current frame is between physics steps. The physics stays at 50 Hz, and the visuals look smooth.

| Setting | What is drawn | Trade-off |
|---|---|---|
| None | Last physics position | Can stutter |
| Interpolate | Blend of previous and current physics position | Always ≤ 1 physics step (20 ms) behind |
| Extrapolate | Prediction of the *next* position | Can overshoot when the object stops |

**Rule of thumb:** turn on *Interpolate* for the player and anything the camera follows.

### ✅ Check your understanding (Unit 1)

1. You want a door to open when the player presses **E**. Which callback reads the key, and why?
2. Why does a camera script use `LateUpdate` rather than `Update`?
3. At 25 FPS, roughly how many times does `FixedUpdate` run per frame?

<details><summary>Answers</summary>

1. `Update`. Input is checked once per frame. In `FixedUpdate` you could miss a press that happens between physics steps (or catch it twice).
2. So it runs after the target has moved this frame. Otherwise the camera may move first and lag a frame behind.
3. Each frame lasts 0.04 s and physics steps every 0.02 s, so about **2** times per frame.
</details>

---

# Unit 2: Values, references, and operators

### 2.1 Value types vs reference types

This is one of the most important ideas in C#.

> **Definition: Value type.** A type whose variables *contain the data itself*. Assigning one
> variable to another **copies** the data. In C#, `struct`s and `enum`s are value types, and so
> are primitive types like `int`, `float` and `bool`.
>
> **Definition: Reference type.** A type whose variables contain a *reference* (an address)
> pointing to an object stored elsewhere. Assigning one variable to another copies the
> **reference**, so both now point to the *same* object. In C#, `class`es are reference types,
> and so are arrays and `string`.

| Unity type | Kind |
|---|---|
| `Vector2`, `Vector3`, `Vector2Int`, `Vector3Int`, `RectInt`, `Color` | **struct** (value) |
| `Transform`, `GameObject`, `Rigidbody2D`, `Tilemap`, any `MonoBehaviour` | **class** (reference) |
| `int[]`, `TileType[,]` | array (reference) |

**Example A: a value type is copied.**

```csharp
Vector3 a = new Vector3(1, 2, 3);
Vector3 b = a;      // b is a COPY
b.x = 99;
Debug.Log(a.x);     // 1   (a is unchanged)
```

**Example B: a reference type is shared.**

```csharp
Transform a = player.transform;
Transform b = a;    // b points to the SAME Transform
b.position = Vector3.zero;
// The player has moved, and a.position is also (0,0,0).
```

**Example C: a common beginner error.**

```csharp
transform.position.x = 5f;   // ❌ compile error CS1612
```

`transform.position` hands you a *copy* of a `Vector3` (a value type). Changing the copy's `x` would be pointless, so C# refuses. The fix is to copy, modify, and assign back:

```csharp
Vector3 p = transform.position;
p.x = 5f;
transform.position = p;      // ✔
```

### 2.2 The `ref` keyword

> **Definition: `ref` parameter.** A parameter passed *by reference*: the method receives the
> caller's actual variable, not a copy, so changes made inside the method stick.

```csharp
void Increment(int n)     { n++; }  // changes a copy
void IncrementRef(ref int n) { n++; }  // changes the caller's variable

int score = 10;
Increment(score);          // score is still 10
IncrementRef(ref score);   // score is now 11
```

`Vector3.SmoothDamp(current, target, ref velocity, smoothTime)` uses `ref` so it can *update your `velocity` field* every frame. That's how it "remembers" its speed between calls.

### 2.3 Assignment vs equality

| Operator | Name | Meaning | Result |
|---|---|---|---|
| `=` | Assignment | *Store* the right side into the left side | The assigned value |
| `==` | Equality | *Ask*: are these equal? | `bool` |
| `!=` | Inequality | *Ask*: are these different? | `bool` |

In plain C#, `if (x = null)` is a compile error, because `if` needs a `bool`. **Unity is an exception:** `UnityEngine.Object` (the base of `Transform`, `GameObject` and so on) defines an *implicit conversion to `bool`* that means "does this object exist?" So:

```csharp
if (target = null) return;   // compiles! Assigns null, converts null to false, never returns.
```

This sets `target` to null every frame, and the next line crashes with a `NullReferenceException`. Many developers prefer to write the explicit form, `if (target == null)`, and to read every `=` inside an `if` with suspicion.

> **Definition: `NullReferenceException`.** A runtime error thrown when you use a reference
> variable that points to nothing (`null`), for example by reading `target.position` when
> `target == null`.

### 2.4 Enums

> **Definition: Enum (enumeration).** A value type that defines a named set of constants.
> Each name maps to an integer.

```csharp
public enum TileType
{
    Wall = 0,
    Floor = 1,
}

TileType t = TileType.Floor;
int raw = (int)t;              // 1
if (t == TileType.Wall) { }    // reads like English
```

> **Definition: Default value.** The value a variable has before you assign anything:
> `0` for numbers, `false` for `bool`, `null` for references, and the enum member equal to
> `0` for enums.

This is why `Wall = 0` comes first. A freshly created `TileType[,]` array is filled with `0`, which is `Wall`, so a new map starts as solid rock.

### ✅ Check your understanding (Unit 2)

1. After `Vector2Int a = new Vector2Int(1,1); Vector2Int b = a; b.x = 5;`, what is `a.x`?
2. After `int[] a = {1,2}; int[] b = a; b[0] = 9;`, what is `a[0]`?
3. Why is `if (target = null)` a bug, and why doesn't the compiler stop you?

<details><summary>Answers</summary>

1. `1`: `Vector2Int` is a struct, so `b` is a copy.
2. `9`: arrays are reference types, so `a` and `b` are the same array.
3. It *assigns* null instead of *comparing*. Unity objects convert implicitly to `bool`, so the expression type-checks.
</details>

---

# Unit 3: Interpolation and smoothing

### 3.1 Linear interpolation (lerp)

> **Definition: Linear interpolation.** Finding the value a fraction `t` of the way from `a`
> to `b`:
>
> **lerp(a, b, t) = a + (b − a) · t**, where t ∈ [0, 1]
>
> t = 0 gives a, t = 1 gives b, and t = 0.5 gives the midpoint.

**Worked example.** lerp(10, 20, 0.25) = 10 + (20 − 10)(0.25) = 12.5

It works component-wise on vectors: lerp((0,0), (4,8), 0.5) = (2, 4).

```csharp
float f = Mathf.Lerp(10f, 20f, 0.25f);                 // 12.5
Vector3 v = Vector3.Lerp(Vector3.zero, new Vector3(4, 8, 0), 0.5f);  // (2, 4, 0)
```

### 3.2 The "lerp toward the target" trick and its flaw

A popular smoothing idiom moves a fixed fraction of the remaining distance each frame:

```csharp
transform.position = Vector3.Lerp(transform.position, goal, 0.1f);  // 10% per frame
```

It looks nice, but it is **frame-rate dependent**. At 144 FPS it takes the 10% step 144 times a second; at 30 FPS, only 30 times. The camera feels much "tighter" on a fast machine.

### 3.3 `SmoothDamp`: a spring with a damper

> **Definition: `Vector3.SmoothDamp`.** Moves a value toward a target like a *critically
> damped spring*: it accelerates, then eases in, without overshooting. It reaches the target
> in roughly `smoothTime` seconds **regardless of frame rate**, because it uses
> `Time.deltaTime` internally.

```csharp
private Vector3 velocity;   // SmoothDamp's memory between frames

void LateUpdate()
{
    transform.position = Vector3.SmoothDamp(
        transform.position,   // current: where WE are now
        goal,                 // target: where we want to be
        ref velocity,         // state, updated by SmoothDamp
        0.15f);               // about how long to get there
}
```

The first argument must be the position of the thing **being moved**. Passing the target's position restarts the motion from the target every frame, so there's no smoothing.

### ✅ Check your understanding (Unit 3)

1. Compute lerp(−4, 4, 0.75).
2. Why is `Lerp(current, goal, 0.1f)` in `Update` frame-rate dependent?

<details><summary>Answers</summary>

1. −4 + 8 × 0.75 = **2**.
2. It moves 10% of the remaining distance *per frame*, so more frames per second means faster convergence.
</details>

---

# Unit 4: Arrays and grids

### 4.1 Arrays

> **Definition: Array.** A fixed-size, ordered collection of elements of the same type,
> stored **contiguously** (side by side) in memory, and accessed by an integer **index**
> starting at **0**.

```csharp
int[] scores = new int[5];      // [0, 0, 0, 0, 0]  (default values)
scores[0] = 10;                 // first element
scores[4] = 50;                 // last element: index Length - 1
int n = scores.Length;          // 5
scores[5] = 1;                  // ❌ IndexOutOfRangeException
```

Why is indexing fast? Because elements are contiguous, the computer finds element *i* with arithmetic: `address = start + i × elementSize`. No searching is needed, whatever the array's size. That's what **O(1)** means (Unit 4.4).

### 4.2 Representing a 2D grid: three options

**Option 1: rectangular (multidimensional) array `T[,]`**

```csharp
TileType[,] tiles = new TileType[width, height];
tiles[3, 5] = TileType.Floor;
int w = tiles.GetLength(0);   // size of dimension 0 (width)
int h = tiles.GetLength(1);   // size of dimension 1 (height)
```

**Option 2: jagged array `T[][]`** (an array of arrays)

```csharp
TileType[][] tiles = new TileType[width][];
for (int x = 0; x < width; x++)
    tiles[x] = new TileType[height];   // each column allocated separately
tiles[3][5] = TileType.Floor;
```

**Option 3: flat 1D array with an index formula**

```csharp
TileType[] tiles = new TileType[width * height];
int Index(int x, int y) => y * width + x;
tiles[Index(3, 5)] = TileType.Floor;
```

**Deriving the index formula.** Picture a 4 × 3 grid, numbered row by row from the bottom-left:

```
y=2 │  8  9 10 11
y=1 │  4  5  6  7
y=0 │  0  1  2  3
    └────────────
      x=0 1  2  3
```

To reach row `y`, you skip `y` complete rows of `width` cells, then move `x` more: **index = y·width + x**. For (2, 1): 1·4 + 2 = **6** ✔.

Going backwards: **x = index % width**, **y = index / width** (integer division). For 6: 6 % 4 = 2 and 6 / 4 = 1, giving (2, 1) ✔.

> **Definition: Row-major order.** Storing a grid row by row, so cells in the same row are
> neighbours in memory. The formula above is row-major.

**Option 4: dictionary (sparse grid)**

```csharp
var tiles = new Dictionary<Vector2Int, TileType>();
tiles[new Vector2Int(3, 5)] = TileType.Floor;
```

It only stores cells that exist, which is ideal for huge or infinite worlds where most space is empty.

**Comparison**

| Representation | Lookup | Memory | Good for |
|---|---|---|---|
| `T[,]` | O(1) | width × height | Clarity; fixed-size maps ← **we use this** |
| `T[][]` | O(1) | width × height (+ small overhead) | Rows of different lengths |
| Flat `T[]` | O(1) | width × height | Performance, saving to files, GPUs |
| `Dictionary` | O(1) on average | Only cells that exist | Sparse or infinite maps |

### 4.3 The `[x, y]` convention

Mathematics and spreadsheets index **`[row, column]`**, which is `[y, x]`. Games usually index **`[x, y]`** to match screen and world coordinates. Neither is "right", but you must be **consistent**: the size order in `new T[a, b]` must match the index order in `tiles[a, b]` everywhere. Mixing them is the most common grid bug. It often goes unnoticed on square grids and crashes on rectangular ones, which is why Lab 2's test `B3_XAndY_AreNotSwapped` uses a 4 × 2 grid.

### 4.4 Big-O notation (a first look)

> **Definition: Big-O notation.** A way of describing how the cost (time or memory) of an
> operation **grows** as the input size *n* grows, ignoring constant factors.

| Notation | Name | Example in our grid |
|---|---|---|
| O(1) | Constant | `Get(x, y)`, `Set`, `InBounds` |
| O(k) | Linear in *k* | `CountNeighbors`, where *k* = 4 or 8 (a constant, so effectively O(1)) |
| O(w · h) | Linear in the number of cells | `Fill`, drawing the whole map, `ToString` |
| O(rw · rh) | Linear in the rectangle's area | `FillRect` |

**Memory.** An enum is stored as an `int` (4 bytes). A 100 × 100 map is 10,000 × 4 B = **40 KB**, which is tiny. A 10,000 × 10,000 map would be 400 MB. That's where sparse structures or chunks become necessary.

### ✅ Check your understanding (Unit 4)

1. In a flat array for a 10 × 8 grid, what index holds (7, 3)?
2. Index 45 in that same grid is which (x, y)?
3. Why does an x/y swap bug often go unnoticed on a 10 × 10 map?

<details><summary>Answers</summary>

1. 3 × 10 + 7 = **37**.
2. x = 45 % 10 = 5 and y = 45 / 10 = 4, giving **(5, 4)**.
3. Width equals height, so swapped indexes are still in range. Nothing crashes; the map is just mirrored along its diagonal.
</details>

---

# Unit 5: Coordinate systems

> **Definition: Coordinate system.** A rule for describing a position with numbers. A game
> uses several at once, and much of game programming is *converting between them*.

| System | Type | Origin | Example | Unity type |
|---|---|---|---|---|
| **Grid** (cell) | integers | bottom-left cell (0, 0) | "cell (3, 5)" | `Vector2Int` / `Vector3Int` |
| **World** | floats | scene origin | (3.5, 5.5, 0) | `Vector3` |
| **Screen** | pixels | bottom-left of the window | (812, 433) | `Vector3` |

Unity's 2D world is **y-up**: +x is right, +y is up, and the camera looks along +z. (Many image formats and UI systems are y-*down*; watch for this when you read other tutorials.)

**Conversions** (Unit 10 covers the Tilemap ones):

```csharp
Vector3 world  = tilemap.GetCellCenterWorld(new Vector3Int(3, 5, 0)); // grid  → world
Vector3Int cell = tilemap.WorldToCell(somePosition);                   // world → grid
Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);  // screen → world
```

Why not just assume cell (3, 5) is at world (3.5, 5.5)? Because that only holds while the Grid sits at the origin with 1-unit cells. Move or scale the Grid and hard-coded maths breaks silently. **Let the object that owns the coordinate system do the conversion.**

---

# Unit 6: Defensive programming

> **Definition: Defensive programming.** Writing code that anticipates invalid input or
> states, and handles them predictably instead of failing mysteriously later.

### 6.1 Exceptions

> **Definition: Exception.** An object representing an error. *Throwing* it stops the current
> method and unwinds the call stack until something *catches* it, or the program reports it.
> Unity prints uncaught exceptions in red in the Console.

| Exception | Thrown when |
|---|---|
| `IndexOutOfRangeException` | An array index is < 0 or ≥ Length |
| `NullReferenceException` | You use a reference that is null |
| `ArgumentException` | A method received an invalid argument (you throw this yourself) |
| `NotImplementedException` | A placeholder for "not written yet" |

### 6.2 Bounds checking

> **Definition: Bounds check.** Testing that an index is valid *before* using it.

For a grid: **0 ≤ x < width and 0 ≤ y < height**. Note the `<`. The last valid index is `width − 1`.

> **Definition: Off-by-one error.** A bug where a loop or check goes one step too far or
> stops one step short, for example writing `x <= width` instead of `x < width`. Also called
> a *fencepost error*: a 10-metre fence with posts every metre needs **11** posts, not 10.

### 6.3 Guard clauses

> **Definition: Guard clause.** A check at the start of a method that exits early (by
> returning or throwing) when its preconditions aren't met.

```csharp
public Room(int width, int height)
{
    if (width < 1 || height < 1)
        throw new ArgumentException($"Room must be at least 1x1, got {width}x{height}.");

    // ...the rest can now safely assume a valid size
}
```

Guard clauses **fail fast**: the error appears at the exact line where the bad value entered, with a clear message, instead of 300 lines later as a confusing crash.

### 6.4 Sentinel values

> **Definition: Sentinel value.** A special value returned to represent "nothing here" or
> "outside", chosen so that the calling code needs no special cases.

In our grid, `Get` returns `Wall` for out-of-bounds cells. Code that asks "is my neighbour a floor?" then works at the edges automatically, because the world outside the map behaves like solid rock.

### 6.5 Choosing a policy: throw, ignore, or clamp?

| Policy | Example | Good when | Risk |
|---|---|---|---|
| **Throw** | Constructor rejects size 0 | The input is *definitely* a programmer error | Crashes if you're wrong |
| **Ignore** | `Set` off the map does nothing | Off-grid writes are common and harmless (e.g. carving a room near the edge) | Can hide real bugs |
| **Sentinel** | `Get` off the map returns `Wall` | There's a natural "outside" value | The caller can't tell outside from a real wall |
| **Clamp** | Index −3 is treated as 0 | Rarely, as with UI sliders | Usually hides bugs |

Good engineers choose a policy *deliberately* and **document it** in a comment.

---

# Unit 7: Neighbourhoods and iteration

### 7.1 Neighbourhoods

> **Definition: Von Neumann neighbourhood (4-neighbourhood).** The 4 cells sharing an
> *edge* with a cell: up, down, left, right.
>
> **Definition: Moore neighbourhood (8-neighbourhood).** The 8 cells sharing an edge *or a
> corner*: the 4 above plus the diagonals.

```
 4-neighbourhood         8-neighbourhood
    .  N  .                NW  N  NE
    W  ■  E                 W  ■  E
    .  S  .                SW  S  SE
```

Related distance measures (useful in later lectures):

| Distance | Formula | Matches |
|---|---|---|
| Manhattan | \|dx\| + \|dy\| | 4-directional movement |
| Chebyshev | max(\|dx\|, \|dy\|) | 8-directional movement (diagonal costs 1) |
| Euclidean | √(dx² + dy²) | Real-world straight lines |

### 7.2 Direction arrays

> **Definition: Direction (offset) array.** An array of `(dx, dy)` offsets. Adding each one to
> a cell's coordinates visits that cell's neighbours, which replaces repetitive `if`s with a loop.

**Without** a direction array, there are four near-identical blocks (and eight for diagonals):

```csharp
if (Get(x, y + 1) == type) count++;
if (Get(x + 1, y) == type) count++;
if (Get(x, y - 1) == type) count++;
if (Get(x - 1, y) == type) count++;
```

**With** one:

```csharp
static readonly Vector2Int[] Directions4 =
    { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

foreach (Vector2Int d in Directions4)
{
    Vector2Int neighbour = new Vector2Int(x + d.x, y + d.y);
    // ...
}
```

> **Definition: `static readonly`.** `static` means the field belongs to the *class* itself,
> so there's one shared copy, not one per object. `readonly` means it can't be reassigned after
> initialisation. Together they suit constant lookup tables.

### 7.3 Loops

```csharp
// for: when you need the index
for (int i = 0; i < 5; i++) { }      // i = 0, 1, 2, 3, 4

// foreach: when you just need each element
foreach (Vector2Int d in Directions4) { }

// nested for: visit every cell of a grid, O(w·h)
for (int x = 0; x < width; x++)
    for (int y = 0; y < height; y++)
        Visit(x, y);
```

### 7.4 Half-open ranges

> **Definition: Half-open range [a, b).** Includes `a`, excludes `b`. It contains `b − a` elements.

`for (int i = 0; i < n; i++)` iterates the half-open range [0, n). Unity's `RectInt` is half-open too:

```csharp
var r = new RectInt(x: 2, y: 1, width: 3, height: 2);
// r.xMin = 2, r.xMax = 5  → x ∈ {2, 3, 4}
// r.yMin = 1, r.yMax = 3  → y ∈ {1, 2}
// area = 3 × 2 = 6 cells
```

Why half-open? Lengths are a simple subtraction (`xMax − xMin`), and adjacent ranges like [0,5) and [5,10) join with no gap or overlap. This removes a whole family of off-by-one errors.

### 7.5 The conditional (ternary) operator

> **Definition: `condition ? a : b`.** An expression that evaluates to `a` if the condition is
> true, otherwise `b`. It's a compact if/else that *produces a value*.

```csharp
string label = health > 0 ? "Alive" : "Dead";
Vector2Int[] dirs = includeDiagonals ? Directions8 : Directions4;
```

### ✅ Check your understanding (Unit 7)

1. How many cells does `new RectInt(0, 0, 4, 4)` cover? What is its `xMax`?
2. A cell in the corner of a map has how many in-bounds 8-neighbours?
3. What's the Chebyshev distance from (1, 1) to (4, 3)? The Manhattan distance?

<details><summary>Answers</summary>

1. **16** cells; `xMax` = **4**.
2. **3**. The other 5 are off the map (and our `Get` reports them as walls).
3. Chebyshev = max(3, 2) = **3**. Manhattan = 3 + 2 = **5**.
</details>

---

# Unit 8: Designing a data class

### 8.1 Classes, objects and constructors

> **Definition: Class.** A blueprint describing data (fields) and behaviour (methods).
> **Object (instance).** One concrete thing built from that blueprint with `new`.
> **Constructor.** A special method, with the same name as the class, that runs once when an
> object is created, to put it into a valid starting state.

```csharp
public class Counter
{
    private int value;                  // field: the data

    public Counter(int start)           // constructor
    {
        value = start;
    }

    public void Add(int amount)         // method: the behaviour
    {
        value += amount;
    }

    public int Value => value;          // read-only property (see 8.2)
}

var c = new Counter(10);   // calls the constructor
c.Add(5);
```

### 8.2 Properties and encapsulation

> **Definition: Encapsulation.** Hiding an object's internal data and exposing only a
> controlled interface. Outsiders can't put the object into an invalid state.

> **Definition: Property.** A member that looks like a field from outside, but is
> accessed through `get` / `set` methods.

```csharp
public int Width { get; }                 // read-only; settable only in the constructor
public MapGrid Grid { get; private set; } // anyone can read; only this class can set
private TileType[,] tiles;                // hidden; outsiders must use Get/Set
```

Because `tiles` is private, every access goes through `Get` and `Set`, which *always* enforce the bounds policy. That's encapsulation paying off.

### 8.3 Plain C# class vs MonoBehaviour

| | Plain C# class | MonoBehaviour |
|---|---|---|
| Created with | `new MapGrid(20, 12)` | Attached to a GameObject (never `new`) |
| Lives in a scene? | No | Yes |
| Gets `Update` / `Start`? | No | Yes |
| Shown in the Inspector? | No (unless serialised inside one) | Yes |
| Testable without a scene? | ✅ easily | Harder |

### 8.4 Separating data from presentation

> **Definition: Separation of concerns.** Each part of a program should have one clear
> responsibility. A common form in games is **model / view**: the *model* holds the game
> state and rules, and the *view* displays it.

```
            reads                 paints
MapGrid  ─────────►  MapRenderer ─────────►  Tilemaps (on screen)
(model:              (view: a                (Unity components)
 plain C#)            MonoBehaviour)
     ▲
     └─ later read by: dungeon generator, pathfinding, minimap, save system
```

Benefits: testable logic, one map shared by many systems, and visuals you can swap without touching the rules.

### 8.5 `ToString()` overrides

Every C# object has a `ToString()` method. *Overriding* it lets `Debug.Log(grid)` print something useful:

```
####
#..#
####
```

That turns an invisible data structure into something you can inspect, which is a huge debugging aid.

---

# Unit 9: Unit testing

> **Definition: Unit test.** A small, automated piece of code that checks that one *unit*
> (usually a method) behaves correctly for a specific input.

> **Definition: Test framework.** A library that finds, runs and reports tests. Unity uses
> **NUnit**, run through **Window → General → Test Runner**.

| Test kind | Runs | Use for |
|---|---|---|
| **EditMode** | In the editor, no Play mode | Plain C# logic (like `MapGrid`) |
| **PlayMode** | Inside a running scene | MonoBehaviours, physics, multi-frame behaviour |

### 9.1 Arrange – Act – Assert

```csharp
using NUnit.Framework;

public class CounterTests
{
    [Test]                                   // marks a method as a test
    public void Add_IncreasesValue()
    {
        // Arrange: set up the situation
        var c = new Counter(10);

        // Act: do the thing being tested
        c.Add(5);

        // Assert: check the outcome
        Assert.AreEqual(15, c.Value);        // (expected, actual)
    }
}
```

### 9.2 Common assertions

| Assertion | Passes when |
|---|---|
| `Assert.AreEqual(expected, actual)` | The two are equal |
| `Assert.IsTrue(cond)` / `Assert.IsFalse(cond)` | The condition is true / false |
| `Assert.Throws<T>(() => code)` | The code throws exactly exception type `T` |
| `Assert.DoesNotThrow(() => code)` | The code runs without throwing |
| `CollectionAssert.AllItemsAreUnique(list)` | No duplicates in the list |

`() => code` is a **lambda**: a tiny unnamed function. It's passed so the assertion can *run* your code inside a safety net and watch for exceptions.

### 9.3 Red → Green → Refactor

> **Definition: Test-driven development (TDD).** Write a failing test first (**red**), write the
> simplest code that passes it (**green**), then clean up the code while keeping the tests
> green (**refactor**).

Good tests target **edge cases**: zero, one, the last valid index, one past the end, negative numbers, and non-square shapes. Most bugs live at the edges.

---

# Unit 10: Unity's Tilemap system

### 10.1 Components

| Piece | What it is |
|---|---|
| **Grid** (component) | Defines cell size and layout. It's the parent object for Tilemaps. |
| **Tilemap** (component) | One layer of tiles, storing which tile is in each cell. |
| **TilemapRenderer** | Draws a Tilemap. `Order in Layer` controls what's drawn on top. |
| **TileBase / Tile** (asset) | What goes *in* a cell: a sprite, a colour, a collider shape. |
| **Tile Palette** (editor window) | A paint tool for placing tiles by hand. |

```
Grid (GameObject + Grid component)
 ├── Floor  (Tilemap + TilemapRenderer)               Order in Layer 0
 └── Walls  (Tilemap + TilemapRenderer + colliders)   Order in Layer 1
Player (SpriteRenderer)                               Order in Layer 10
```

### 10.2 Painting from code

```csharp
using UnityEngine.Tilemaps;

[SerializeField] private Tilemap tilemap;
[SerializeField] private TileBase tile;

void PaintExample()
{
    tilemap.ClearAllTiles();                          // remove everything
    tilemap.SetTile(new Vector3Int(2, 3, 0), tile);   // place one tile
    tilemap.SetTile(new Vector3Int(2, 3, 0), null);   // erase one tile
    TileBase t = tilemap.GetTile(new Vector3Int(2, 3, 0));
}
```

Tilemap positions are `Vector3Int`. Use `z = 0` for 2D.

### 10.3 Collision for tilemaps

| Component | Role |
|---|---|
| `TilemapCollider2D` | One collider shape per tile. On its own, the player can snag on the seams between tiles. |
| `CompositeCollider2D` | Merges the shapes into one smooth outline (requires **Used By Composite** on the TilemapCollider2D). |
| `Rigidbody2D` (Static) | Required by the composite. **Static** means "never moves," which is also cheaper for physics. |

> **Definition: Static vs dynamic body.** A *dynamic* body is moved by physics forces (your
> player). A *static* body never moves (walls). A *kinematic* body is moved only by your code.

### 10.4 Serialized references

> **Definition: Serialization.** Saving an object's data in a storable format. Unity serialises
> `public` and `[SerializeField]` fields into the scene file. That's how references you drag
> into the Inspector survive saving and reloading.

A field the Inspector expects but nobody filled in stays `null`, which is the #1 cause of `NullReferenceException` in Unity. That's why `MapRenderer.Build()` checks its references up front (a guard clause).

---

## Summary

- Unity runs **two clocks**: per-frame `Update`/`LateUpdate`, and fixed-rate `FixedUpdate`. **Interpolation** hides the mismatch between them.
- **Structs copy, classes share.** `ref` lets a method change the caller's variable.
- **`=` assigns, `==` compares.** Unity objects implicitly convert to `bool`, so mix-ups compile.
- **lerp(a, b, t) = a + (b − a)t.** `SmoothDamp` gives frame-rate-independent smoothing.
- A grid is a **2D array** with **O(1)** access. The flat form uses **index = y·w + x**.
- Pick an indexing convention (`[x, y]`) and **never mix it**.
- Defend your code with **bounds checks**, **guard clauses** and **sentinel values**, and choose a failure policy on purpose.
- **Direction arrays** turn neighbour logic into one loop. **Half-open ranges** prevent off-by-one errors.
- Keep the **model** (plain C# data) separate from the **view** (MonoBehaviours, Tilemaps).
- **Unit tests** (Arrange–Act–Assert) prove each piece works, especially at the edges.

---

## Glossary

| Term | Definition |
|---|---|
| Array | Fixed-size, zero-indexed, contiguous collection of same-type elements |
| Big-O | Growth rate of cost as input size grows |
| Bounds check | Verifying an index is valid before use |
| Class | Blueprint for objects; a reference type |
| Constructor | Method that initialises a new object |
| Default value | Value before assignment: 0, false, null, or the 0 enum member |
| Delta time | Duration of the previous frame in seconds |
| Direction array | Array of (dx, dy) offsets used to visit neighbours |
| Encapsulation | Hiding internal data behind a controlled interface |
| Enum | Named set of integer constants; a value type |
| Exception | Error object that unwinds the call stack when thrown |
| Frame | One pass through the game loop |
| Guard clause | Early exit when preconditions fail |
| Half-open range | [a, b): includes a, excludes b |
| Interpolation (Rigidbody) | Drawing a physics object between its last two physics positions |
| Lerp | a + (b − a)·t |
| Moore neighbourhood | 8 surrounding cells |
| Off-by-one error | Loop or check one step too far or short |
| Property | Field-like member accessed via get/set |
| Reference type | Variable holds a reference; copies share one object |
| Row-major | Grid stored row by row; index = y·w + x |
| Sentinel | Special value meaning "nothing" or "outside" |
| Serialization | Saving object data; how Inspector values persist |
| Struct | Value type; copied on assignment |
| Unit test | Automated check of one unit of code |
| Value type | Variable holds the data; copies are independent |
| Von Neumann neighbourhood | 4 edge-adjacent cells |

---

## Review problems (exam-style)

1. **(Timing)** A game runs at 40 FPS with the default fixed timestep. In one second, how many times are `Update` and `FixedUpdate` each called? Describe what a player without interpolation looks like on screen.
2. **(Types)** Predict the output and explain:
   ```csharp
   Vector3 p = transform.position;
   p.y += 10;
   Debug.Log(transform.position.y == p.y);
   ```
3. **(Indexing)** Write the functions `int ToIndex(int x, int y, int width)` and `Vector2Int ToCoord(int index, int width)`. Test them by hand on a 5 × 5 grid with (4, 2).
4. **(Big-O)** A function calls `CountNeighbors` (8-way) on every cell of a w × h grid. What is its Big-O? How many `Get` calls does it make on a 50 × 40 grid?
5. **(Design)** `Set` silently ignores out-of-bounds writes. Give one situation where that's helpful and one where it would hide a bug. What alternative policy would you choose for a *debug build*?
6. **(Testing)** Write (on paper) an NUnit test that proves `FillRect` doesn't touch the cell at `(rect.xMax, rect.yMin)`.
7. **(Tilemaps)** Why do we use two Tilemaps instead of one? What would go wrong if the floor Tilemap had a `TilemapCollider2D`?

<details><summary>Answer sketches</summary>

1. `Update` ≈ 40 calls, `FixedUpdate` = 50. Some frames run physics twice, so the player appears to move in uneven jumps.
2. `False` (unless y was something odd). `p` is a copy of a struct, so changing it doesn't move the Transform.
3. `ToIndex = y * width + x` gives 2·5 + 4 = 14. `ToCoord`: (14 % 5, 14 / 5) = (4, 2) ✔
4. O(w·h), because 8 is a constant. 50 × 40 × 8 = **16,000** calls.
5. Helpful: carving a room that overlaps the edge. Harmful: an off-by-one in a generator writes outside the map and you never notice. In debug builds, log a warning (or throw) for out-of-bounds writes.
6. Arrange a grid of walls; Act `FillRect(rect, Floor)`; Assert `Get(rect.xMax, rect.yMin) == Wall`.
7. To give floors and walls different collision and draw order. In top-down 2D, "standing on" the floor really means *overlapping* it. A collider on the floor would make every floor tile solid: the player would start inside a collider, get shoved out, and be unable to walk anywhere.
</details>
