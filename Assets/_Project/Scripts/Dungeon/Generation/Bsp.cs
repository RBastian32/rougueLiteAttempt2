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
    // The cutting is RECURSIVE: Split calls Split on each half. (Lecture Unit 6.)
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
            {
                throw new System.ArgumentException($"minLeafSize must be at least 1, got {minLeafSize}");
            }

            RectInt first, second; // the two halves after the cut
        

            // A cut leaves two pieces that must BOTH be at least minLeafSize,
            // so the area must be at least twice that in the direction we cut.
            bool canCutVertically = area.width >= 2 * minLeafSize;     // a vertical line → left + right
            bool canCutHorizontally = area.height >= 2 * minLeafSize;  // a horizontal line → bottom + top

            // TODO C1: BASE CASE. If it can't be cut either way, this area is a leaf:
            //          return a new list containing just `area`.
            //          (List initialiser syntax: new List<RectInt> { area })
            if(!canCutVertically && !canCutHorizontally)
            {
                return new List<RectInt> { area };
            }
            // TODO C2: CHOOSE THE DIRECTION. Make a bool cutVertically:
            //          - if only one direction is possible, use that one
            //          - if both are possible, cut across the LONGER side (wide → vertical cut,
            //            tall → horizontal cut); if it's square, pick randomly with rng.Next(2) == 0
            bool cutVertically;
            if(canCutVertically != canCutHorizontally)
            {
                cutVertically = canCutVertically;
            }
            else
            {
                if(area.width > area.height)
                {
                    cutVertically = true;
                }
                else if(area.width < area.height)
                {
                    cutVertically = false;
                }
                else
                {
                    cutVertically = rng.Next(2) == 0;
                }
            }
        

            // TODO C3: CHOOSE WHERE TO CUT, and build the two halves.
            //          The cut is a distance from the area's left edge (or bottom edge). Both halves
            //          must be at least minLeafSize, so it must be in minLeafSize .. size - minLeafSize.
            //          rng.Next(min, max) EXCLUDES max (half-open!), so pass size - minLeafSize + 1.
            //          Vertical:   first  = new RectInt(area.xMin, area.yMin, cut, area.height)
            //                      second = new RectInt(area.xMin + cut, area.yMin, area.width - cut, area.height)
            //          Horizontal: the same idea on y.
            int cut;
            if(cutVertically)
            {
                cut = rng.Next(minLeafSize, area.width - minLeafSize + 1);
                first = new RectInt(area.xMin, area.yMin, cut, area.height);
                second = new RectInt(area.xMin + cut, area.yMin, area.width - cut, area.height);
            }
            else
            {
                cut = rng.Next(minLeafSize, area.height - minLeafSize + 1);
                first = new RectInt(area.xMin, area.yMin, area.width, cut);
                second = new RectInt(area.xMin, area.yMin + cut, area.width, area.height - cut);
            }

            // TODO C4: RECURSE. Split both halves, put all the leaves into one list, return it.
            //          list.AddRange(otherList) appends every item of otherList.
            //          Then delete the line below.
            
            List<RectInt> leaves = new List<RectInt>();
            leaves.AddRange(Split(first, minLeafSize, rng));
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

            // TODO C5: (1) Guard: if maxWidth or maxHeight is less than minRoomSize, throw an
            //              ArgumentException (the leaf is too small for any room).
            if(maxWidth < minRoomSize || maxHeight < minRoomSize)
            {
                throw new System.ArgumentException($"Leaf is too small for any room. Max room size: {maxWidth} x {maxHeight}, Min room size: {minRoomSize}");
            }
            //          (2) Pick the room's width in minRoomSize .. maxWidth INCLUSIVE, and the
            //              same for height. (Remember Next's max is exclusive.)
            int roomWidth = rng.Next(minRoomSize, maxWidth + 1);
            int roomHeight = rng.Next(minRoomSize, maxHeight + 1);
            //          (3) Pick its x so that it sits inside the margins:
            //              smallest x = leaf.xMin + margin
            //              largest x  = leaf.xMax - margin - width   (so x + width ≤ xMax - margin)
            //              Same for y. Then return the new RectInt.
            //          Then delete the line below.
            int xMin = leaf.xMin + margin;
            int xMax = leaf.xMax - margin - roomWidth + 1;
            int ymin = leaf.yMin + margin;
            int ymax = leaf.yMax - margin - roomHeight + 1;

            return new RectInt(rng.Next(xMin, xMax), rng.Next(ymin, ymax), roomWidth, roomHeight);

        }
    }
}
