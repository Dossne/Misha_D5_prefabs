# Prefab Generator Authoring Guide (Universal)

## Purpose
This guide defines a reusable workflow for generating a **complete Unity prefab-generation package** from an input specification that describes:
- final hierarchy tree;
- per-prefab hierarchy list;
- component list per node;
- serialized links for view classes.

The result must be a ready-to-run package inside a chosen workspace folder (for example `Assets/TestPrefabGenerator`) that includes:
- one class per file for all required view/runtime classes;
- ScriptableObject-based generator profile;
- Custom Inspector button `Generate`;
- editor-only generation pipeline;
- optional batch entry point for automated generation;
- generated prefab assets (if Unity generation run is possible in the current environment).

---

## Mandatory interaction protocol (must be followed in order)

1. **Check TMPro Essentials prerequisite**
- Agent must explicitly verify that user has imported **TextMeshPro Essentials** into the project.
- If not confirmed, agent must ask user to import it first and stop further implementation until confirmed.

2. **Request workspace root (user answer required)**
- Agent must request the exact working folder path where generation package should be created.
- Agent must not continue without user response.

3. **Request input hierarchy spec (user answer required)**
- After workspace root is provided, agent must request full input data in the same structure style as the first prompt in this conversation:
  - final hierarchy tree;
  - prefab list;
  - components per node;
  - serialized-field bindings.
- If user does not provide this input, agent must terminate the task with a clear message that required input is missing.

---

## Mandatory constraints

1. **Single workspace root**
- Work only inside the provided root folder.
- Do not scan or reuse assets/scripts from other folders unless explicitly allowed.

2. **Class/file rule**
- Exactly one class per `.cs` file.
- Use clear namespaces and avoid cross-assembly invalid references.

3. **Folder layout rule**
- Each prefab/domain has its own folder with its scripts.
- Generator scripts and generator asset are in a separate dedicated folder.

4. **Generator architecture rule**
- Runtime side: `ScriptableObject` profile with settings only.
- Editor side: generation pipeline + custom inspector with `Generate` button.
- Runtime script must not directly reference editor-only classes.

5. **UI text rule**
- Use `TextMeshProUGUI` for player-facing UI texts.
- If `Assets/Font/bangerscyrillic SDF.asset` exists, assign it where text is created in generation logic.

6. **Prefab completeness rule**
- Prefab hierarchies must be fully created according to input.
- All serialized fields declared by view classes must be assigned in generator code.

---

## Target output structure (template)

Use this pattern under `<WORKSPACE_ROOT>`:

- `<WORKSPACE_ROOT>/<FeatureA>/Scripts/*.cs`
- `<WORKSPACE_ROOT>/<FeatureB>/Scripts/*.cs`
- ...
- `<WORKSPACE_ROOT>/Generator/Scripts/PrefabGenerationProfile.cs`
- `<WORKSPACE_ROOT>/Generator/Editor/PrefabGenerationPipeline.cs`
- `<WORKSPACE_ROOT>/Generator/Editor/PrefabGenerationProfileEditor.cs`
- `<WORKSPACE_ROOT>/Generator/Editor/PrefabGenerationBatchRunner.cs` (optional but recommended)
- `<WORKSPACE_ROOT>/Generator/Asset/PrefabGenerationProfile.asset`

And generated prefabs saved into corresponding feature folders:
- `<WORKSPACE_ROOT>/<FeatureX>/<PrefabName>.prefab`

---

## Runtime and editor separation

### Runtime (`Generator/Scripts`)
- `PrefabGenerationProfile : ScriptableObject`
  - stores only config data (for example `rootPath`).
  - no direct usage of `UnityEditor` APIs.

### Editor (`Generator/Editor`)
- `PrefabGenerationPipeline`:
  - receives `PrefabGenerationProfile`;
  - builds all object trees;
  - assigns all serialized references;
  - saves prefab assets.
- `PrefabGenerationProfileEditor`:
  - custom inspector for profile;
  - button `Generate` calling pipeline.
- `PrefabGenerationBatchRunner` (optional):
  - static method to run generation via `-executeMethod`.

---

## Input normalization algorithm

Given user input with a hierarchy schema:

1. Parse all mentioned classes/components/serialized fields.
2. Build a model:
- Nodes (`name`, `components`, `children`)
- View binding requirements (`Class -> fields -> target node/component`)
- Reusable prefab definitions.
3. Emit script plan:
- one file per class;
- split by folder domain.
4. Emit generation plan:
- helper builders for reusable subtrees;
- top-level builders for each prefab;
- final save paths per prefab.

---

## Binding strategy

Use explicit `Bind(...)` methods on view classes to assign serialized fields.

Benefits:
- deterministic field wiring in code;
- no reflection needed;
- easy compile-time checks.

Pattern:
1. Create hierarchy nodes.
2. Add view components.
3. Capture references to required components.
4. Call `Bind(...)` for each view in parent-to-child order.

---

## Build helpers (recommended)

In pipeline code define helpers:
- `CreateUiNode(name, parent, extraComponents...)`
- `CreateWorldNode(name, parent, addSortingGroup)`
- `CreateEmptyNode(name, parent)`
- `SavePrefab(root, path)`
- `EnsureFolder(path)`
- `ApplyPreferredTmpFont(text)`

Keep node transforms initialized to stable defaults.

---

## Validation checklist

Before finalizing output:

1. Compile-safety
- No runtime -> editor direct type references.
- Editor files wrapped with `#if UNITY_EDITOR` when needed.

2. Completeness
- Every requested prefab has a generator function.
- Every requested serialized field is assigned.

3. Structure
- One class per file.
- Generator scripts + asset in dedicated folder.

4. Output paths
- Prefabs saved into intended folders under workspace root.

5. Execution
- If environment allows, run generation automatically.
- If not possible, report that explicitly and provide manual run steps.

---

## Agent output contract

When completing a task from this guide, the agent should output:

1. What was created.
2. Absolute paths of key files.
3. Whether generation was auto-run.
4. If auto-run failed/unavailable, exact manual steps:
- open profile asset;
- press `Generate`.
5. Any unverified points.

---

## Generic implementation notes

- Avoid assumptions about concrete prefab names beyond the parsed input.
- Keep logic data-driven where possible (builders reused by multiple prefabs).
- Do not modify unrelated project files.
- Keep diffs localized to workspace root.
