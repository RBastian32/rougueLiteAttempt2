# Lab 4 hints

Each TODO has up to three hints. Read one, try again, and only read the next if you're still stuck.

---

## A1: the constructor
1. `neighbours = new List<List<int>>();` makes the outer list, but it's empty. Every room needs its own inner list.
2. `for (int i = 0; i < roomCount; i++) neighbours.Add(new List<int>());`
3. If `A1_NewGraph_HasTheRoomsAndNoEdges` throws a NullReferenceException, you didn't replace the `neighbours = null;` line.

## A2: `HasEdge`
1. `neighbours[a]` is room a's list of neighbours. Is b in it?
2. `return neighbours[a].Contains(b);`

## A3: `AddEdge`
1. Three cases, in order: same room (throw), already joined (return false), new edge.
2. `if (a == b) throw new System.ArgumentException(...);` then `if (HasEdge(a, b)) return false;`
3. New edge: `neighbours[a].Add(b); neighbours[b].Add(a); EdgeCount++; return true;` If `HasEdge(2, 0)` is false right after `AddEdge(0, 2)`, you only added to one list.

## A4: `Neighbours` and `Degree`
1. `Neighbours`: `return neighbours[id];` A `List<int>` *is* an `IReadOnlyList<int>`, so no conversion is needed.
2. `Degree`: `return neighbours[id].Count;`

## B1: `Distance`
1. `return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);`

## B2: set up
1. `bool[] inTree = new bool[n];` already starts all false.
2. `int[] bestCost = new int[n]; int[] bestFrom = new int[n];` start all 0, so fill them in a loop: `bestCost[i] = int.MaxValue; bestFrom[i] = -1;`
3. After the loop: `bestCost[0] = 0;`

## B3: pick the cheapest room
1. `int v = -1;` then loop `i` from 0 to n − 1.
2. Take `i` when `!inTree[i] && (v == -1 || bestCost[i] < bestCost[v])`. The `v == -1` part handles "no candidate yet".
3. After the loop, `v` is never −1 (there's always a room left on each of the n passes).

## B4: add and update
1. `inTree[v] = true;` then `if (bestFrom[v] != -1) tree.AddEdge(bestFrom[v], v);`
2. Then loop `u` over every room. `if (inTree[u]) continue;` skips rooms already in the tree.
3. `int d = Distance(centres[v], centres[u]); if (d < bestCost[u]) { bestCost[u] = d; bestFrom[u] = v; }`. If the lecture example gives 4 edges but the wrong ones, check you're comparing with `bestCost[u]`, not `bestCost[v]`.

## C1: set up
1. `int[] distance = new int[graph.RoomCount];` then a loop setting every entry to −1.
2. `distance[start] = 0; var queue = new Queue<int>(); queue.Enqueue(start);`

## C2: the loop
1. `while (queue.Count > 0) { int room = queue.Dequeue(); ... }`
2. Inside: `foreach (int next in graph.Neighbours(room))`, and only act on rooms with `distance[next] == -1`.
3. `distance[next] = distance[room] + 1; queue.Enqueue(next);` Then after the while loop: `return distance;`. If distances come out too big, check you set the distance when you **enqueue**, not when you dequeue.

## C3: `Farthest`
1. `int best = start;` then loop over every index `i`.
2. `if (distance[i] > distance[best]) best = i;` then `return best;`

## D1: `AddExtra`
1. `int added = 0;` then `for (int a = 0; a < graph.RoomCount; a++) for (int b = a + 1; b < graph.RoomCount; b++) { ... }`
2. `if (graph.HasEdge(a, b)) continue;` and `if (SpanningTree.Distance(centres[a], centres[b]) > maxDistance) continue;`
3. `if (rng.NextDouble() < chance) { graph.AddEdge(a, b); added++; }` then `return added;`. If the "same seed" test fails, make sure `NextDouble` is called *after* both checks.

## D2: spawn and boss
1. `roles[spawn] = RoomType.Spawn;`
2. `int boss = Bfs.Farthest(graph, spawn); if (boss != spawn) roles[boss] = RoomType.Boss;`

## D3: treasure
1. `for (int i = 0; i < graph.RoomCount; i++)` and check two things.
2. `if (roles[i] == RoomType.Normal && graph.Degree(i) == 1) roles[i] = RoomType.Treasure;`

## D4: shop
1. `var candidates = new List<int>();` then add every `i` whose role is still `Normal`.
2. `if (candidates.Count > 0) roles[candidates[rng.Next(candidates.Count)]] = RoomType.Shop;`

## E1: the generator
1. `var centres = new List<Vector2Int>(); foreach (RectInt room in result.Rooms) centres.Add(Center(room));`
2. `result.Graph = SpanningTree.Prim(centres);` then `Loops.AddExtra(result.Graph, centres, loopMaxDistance, loopChance, rng);`
3. `foreach ((int a, int b) in result.Graph.Edges()) { bool horizontalFirst = rng.Next(2) == 0; Corridors.CarveL(result.Grid, centres[a], centres[b], horizontalFirst, corridorThickness); }` then `result.Roles = RoomRoles.Assign(result.Graph, 0, rng);`. If a D test turns red, check you didn't delete D4 (`result.Spawn`).

## E2: Gizmos
1. If nothing is drawn: is `lastBsp` set in the Bsp case? Did you Build Map *after* the code compiled? (A recompile resets non-serialized fields, so rebuild.) Is the Scene view's Gizmos button on?
2. If the outlines are offset by half a tile: `DrawWireCube` wants the **centre**. Use `corner + size / 2f`.
3. If everything is white: check `ColorFor` has a `case` for each role and that you set `Gizmos.color` **before** each `DrawWireCube`.
