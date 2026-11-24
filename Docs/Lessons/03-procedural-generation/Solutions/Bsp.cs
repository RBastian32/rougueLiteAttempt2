using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 3 - EXERCISE C: Binary Space Partitioning (BSP).
    //
    // Take the whole map as one rectangle. Cut it in two. Cut each half in two. Keep going
    // until the pieces are too small to cut. The final pieces are called LEAVES; they never
    // overlap and together they cover the whole area exactly. Put one room inside each leaf
    // and you get a dungeon whose rooms are spread out and never collide.
    //
    // The cutting is RECURSIVE: Split calls Split on each half. (Lecture Unit 5.)
    //
    // List<RectInt> is C#'s growable array, like std::vector<RectInt> in C++.
    // It needs "using System.Collections.Generic;" at the top of the file.
    //
    // Check your work: the tests start with "C" in GenerationTests.cs.
    public static class Bsp
    {
        // Splits `area` again and again, and returns every leaf.
        // Every leaf is at least minLeafSize wide AND tall, and can't be split any further
        // (it's less than 2 * minLeafSize in both directions).
        public static List<RectInt> Split(RectInt area, int minLeafSize, System.Random rng)
        {
            // Guard clause (done for you). With 0 or less, cuts could go on forever.
            if (minLeafSize < 1)
                throw new System.ArgumentException($"minLeafSize must be at least 1, got {minLeafSize}");

            // A cut leaves two pieces that must BOTH be at least minLeafSize,
            // so the area must be at least twice that in the direction we cut.
            bool canCutVertically = area.width >= 2 * minLeafSize;     // a vertical line → left + right
            bool canCutHorizontally = area.height >= 2 * minLeafSize;  // a horizontal line → bottom + top

            // SOLUTION C1: base case
            if (!canCutVertically && !canCutHorizontally)
                return new List<RectInt> { area };

            // SOLUTION C2: choose the direction
            bool cutVertically;
            if (canCutVertically && canCutHorizontally)
            {
                if (area.width > area.height) cutVertically = true;
                else if (area.height > area.width) cutVertically = false;
                else cutVertically = rng.Next(2) == 0;
            }
            else
            {
                cutVertically = canCutVertically;
            }

            // SOLUTION C3: choose where, and build the two halves
            RectInt first, second;
            if (cutVertically)
            {
                int cut = rng.Next(minLeafSize, area.width - minLeafSize + 1);
                first = new RectInt(area.xMin, area.yMin, cut, area.height);
                second = new RectInt(area.xMin + cut, area.yMin, area.width - cut, area.height);
            }
            else
            {
                int cut = rng.Next(minLeafSize, area.height - minLeafSize + 1);
                first = new RectInt(area.xMin, area.yMin, area.width, cut);
                second = new RectInt(area.xMin, area.yMin + cut, area.width, area.height - cut);
            }

            // SOLUTION C4: recurse and combine
            List<RectInt> leaves = Split(first, minLeafSize, rng);
            leaves.AddRange(Split(second, minLeafSize, rng));
            return leaves;
        }

        // Picks a random room that fits inside `leaf`, keeping at least `margin` cells of rock
        // between the room and every edge of the leaf (so rooms in neighbouring leaves never touch).
        public static RectInt PlaceRoom(RectInt leaf, int margin, int minRoomSize, System.Random rng)
        {
            // The biggest room that fits once the margin is taken off both sides.
            int maxWidth = leaf.width - 2 * margin;
            int maxHeight = leaf.height - 2 * margin;

            // SOLUTION C5
            if (maxWidth < minRoomSize || maxHeight < minRoomSize)
                throw new System.ArgumentException($"Leaf {leaf} is too small for a {minRoomSize}x{minRoomSize} room with margin {margin}");

            int w = rng.Next(minRoomSize, maxWidth + 1);
            int h = rng.Next(minRoomSize, maxHeight + 1);
            int x = rng.Next(leaf.xMin + margin, leaf.xMax - margin - w + 1);
            int y = rng.Next(leaf.yMin + margin, leaf.yMax - margin - h + 1);
            return new RectInt(x, y, w, h);
        }
    }
}
