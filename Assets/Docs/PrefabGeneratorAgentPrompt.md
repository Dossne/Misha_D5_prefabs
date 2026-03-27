# Universal Prompt Template For Prefab-Generator Agent

Read and follow this guide first:
`Assets/TestPrefabGenerator/_Generator/Docs/PrefabGeneratorAuthoringGuide.md`

Only after reading it, execute the task below.

## Mandatory conversation gates (strict order)
1. Confirm user has imported **TextMeshPro Essentials** in the project.
- If not confirmed, ask user to import and stop.
2. Request the working root folder path.
- User response is mandatory.
- Do not proceed without it.
3. After root path is provided, request full input hierarchy data in the same format style as the first prompt of this conversation.
- If user does not provide the structure data, terminate and report that required input is missing.

## Task
Build a complete Unity prefab-generation package from the input specification I provide.

## Input format I will provide
- Final hierarchy tree (full screen/root structure)
- List of reusable prefab definitions
- For each node:
  - object name
  - components
  - view class and serialized-field bindings (if any)
  - optional UI layout metadata: reference resolution, size, anchors, offsets, fixed axis sizes, and/or position

## Required behavior
1. Work only inside the provided workspace root folder.
2. Create one `.cs` file per class.
3. Place scripts in per-prefab/domain folders.
4. Place generator scripts and generator asset in a separate `_Generator` folder.
5. Agent must create the generation profile `.asset` file directly in the target `_Generator/Asset` folder as part of file generation (including valid `.meta` when needed). Do not require creating this asset via Unity menu actions (for example `Assets/Create`).
6. Implement generator as:
- `ScriptableObject` profile (runtime-safe)
- custom inspector button `Generate`
- editor generation pipeline that creates all prefabs and assigns all serialized references
7. Keep runtime/editor assembly separation correct (no runtime references to editor classes).
8. Do not run generation from the agent. Always provide manual run steps.
9. If the input contains prefab descriptions for nested structures, generate child prefabs first and use those generated prefabs inside parent prefab generation (no manual subtree duplication).
10. If the input contains UI layout metadata for `RectTransform` nodes, parse and apply it during prefab generation.
11. Use size, anchors, offsets, fixed axis sizes, and position metadata to configure `RectTransform` values instead of leaving UI nodes at generic defaults.
12. When anchors/offsets are provided, prefer them for stretched axes; when only fixed position is provided, use it to set `anchoredPosition` and `sizeDelta`.

## Required deliverables
- Full set of source files for views/runtime classes.
- Full set of generator files.
- Generation profile `.asset`.
- Prefabs generated to corresponding folders after manual run.
- Final report with:
  - created/changed files;
  - whether generation was executed;
  - what is still unverified.

## Quality bar
- Exact hierarchy and component composition according to input.
- All serialized fields wired.
- No extra dependencies outside workspace root.
- Clean, minimal, deterministic code.


