# Universal Prompt Template For Prefab-Generator Agent

Read and follow this guide first:
`Assets/TestPrefabGenerator/Generator/Docs/PrefabGeneratorAuthoringGuide.md`

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

## Required behavior
1. Work only inside the provided workspace root folder.
2. Create one `.cs` file per class.
3. Place scripts in per-prefab/domain folders.
4. Place generator scripts and generator asset in a separate `Generator` folder.
5. Implement generator as:
- `ScriptableObject` profile (runtime-safe)
- custom inspector button `Generate`
- editor generation pipeline that creates all prefabs and assigns all serialized references
6. Keep runtime/editor assembly separation correct (no runtime references to editor classes).
7. If possible, run generation automatically after writing files.
8. If automatic run is not possible, explicitly report this and provide manual run steps.

## Required deliverables
- Full set of source files for views/runtime classes.
- Full set of generator files.
- Generation profile `.asset`.
- Prefabs generated to corresponding folders (when generation run is possible).
- Final report with:
  - created/changed files;
  - whether generation was executed;
  - what is still unverified.

## Quality bar
- Exact hierarchy and component composition according to input.
- All serialized fields wired.
- No extra dependencies outside workspace root.
- Clean, minimal, deterministic code.
