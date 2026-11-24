# Course: Game Development Fundamentals with Unity & C#

This folder is the course: lectures, labs and notes. The **code** you write lives in the real
game folders (`Assets/_Project/Scripts/...`), because every lab builds a piece of the actual game.

## What each lesson contains

| File | What it is | When to use it |
|---|---|---|
| **Lecture slides** (link below) | The main lecture. Self-contained: every unit opens with the problem it solves, then definitions, examples, code, use cases and common mistakes | Your primary study material. |
| **Guided-notes slides** (link below) | The same slides with key words, definitions and answers blanked out. Answers are in each slide's speaker notes | Fill in as you go through the lecture slides. Download as PDF to write on. |
| **Lab preview slides** (from Lesson 3) | Warm-paper slides at the end of each unit: the plan, the API calls and a code skeleton for each TODO, with the key lines left for you | Read with the lecture; come back to them before opening HINTS.md. |
| `LECTURE.md` | Written reference notes covering the same material, plus a glossary and exam-style review problems | For looking things up, and review before the lab. |
| `README.md` | The **lab handout**: pre-lab questions, editor setup, TODO exercises, challenges | After the lecture, when it's time to code. |
| `HINTS.md` | Nudges for each TODO | When stuck for 15+ minutes. |
| `Solutions/` | Full reference answers | Last resort, or to compare after you finish. |

## Recommended study routine (per lesson)

1. **Lecture** (60–90 min): open the lecture slides and the guided-notes slides side by side on the
   same slide number. Read the lecture slide, fill in the guided slide's blanks (from memory where
   you can), then check the speaker notes.
2. **Self-check**: on the guided summary slide, write each idea in one sentence without looking.
   Then try the review questions.
3. **Reference**: skim `LECTURE.md` for anything that still feels shaky.
4. **Pre-lab**: answer the lab's pre-lab questions in your notes.
5. **Lab**: complete the TODOs. Search VS Code with `Cmd+Shift+F` for `TODO B` to find every
   step of exercise B. Check your work with:
   - **Tests**: in Unity, open `Window > General > Test Runner > EditMode > Run All`.
     Red ❌ means not done or a bug; green ✅ means correct. Click a failing test to read why.
   - **Play mode**: press Play and see if it behaves as described.
6. **Debugging log**: when you hit a bug, write down the symptom, cause, fix and which
   lecture section explains it. Lab 2 has an example table.
7. **Commit** when it works (`git add -A`, `git commit -m "..."`, `git push`).
8. **Review** (a few days later): redo the lecture's review problems without notes. Spacing
   out your review is one of the most effective ways to remember things long-term.

## Playing around

`Assets/_Project/Sandbox/` is your scratch space for experiments that shouldn't touch the real game.

## Schedule

| # | Lecture | Lab | Status |
|---|---|---|---|
| 1 | MonoBehaviour lifecycle, physics, vectors | Player movement | ✅ |
| 2 | [Spatial data: time, vectors, grids, Tilemaps](02-grid-and-tilemap/LECTURE.md) · [lecture slides](https://claude.ai/artifact/4WEHL1yUd5v2KUy2oSiKQP) · [guided notes](https://claude.ai/artifact/FwxucXJn12wV3n6R9VXWej) | [Lab 2](02-grid-and-tilemap/README.md): camera, map grid, renderer (+ all 6 challenges) | ✅ |
| 3 | [Procedural generation: seeds, random walk, recursion, BSP](03-procedural-generation/LECTURE.md) · [lecture slides](https://claude.ai/artifact/7yhR4sMjhn72CYKUPDYw9k) · [guided notes](https://claude.ai/artifact/FVufEG56hJnzX9TRcf75UY) | [Lab 3](03-procedural-generation/README.md): random walk, corridors, BSP, generator | 👉 current |
| 4 | Graphs: adjacency, minimum spanning trees, BFS | Linking rooms | |
| 5 | AI: state machines, A* pathfinding, priority queues | Enemies | |
| 6 | Events, delegates, interfaces | Combat & health | |
| 7 | Data-driven design: ScriptableObjects | Enemy and item data | |
| 8 | Probability: weighted random tables, stat modifiers | Loot & upgrades | |
| 9 | Performance: memory, garbage collection, object pooling | Projectiles | |
| 10 | Architecture: state machines for game flow, serialization | Game loop & saving | |
