# Prefab Generator Authoring Guide (Universal)

## Purpose
This guide defines a reusable workflow for generating a **complete Unity prefab-generation package** from an input specification that describes:
- final hierarchy tree;
- per-prefab hierarchy list;
- component list per node;
- serialized links for view classes;
- optional UI layout metadata such as reference resolution, size, anchors, offsets, and position.

The result must be a ready-to-run package inside a chosen workspace folder (for example `Assets/TestPrefabGenerator`) that includes:
- one class per file for all required view/runtime classes;
- ScriptableObject-based generator profile;
- Custom Inspector button `Generate`;
- editor-only generation pipeline;
- optional batch entry point;
- generated prefab assets after manual generation run.

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

6. **RectTransform metadata rule**
- If the input includes reference-resolution layout metadata for visible UI nodes, the generator must parse and apply it to `RectTransform`.
- This metadata is authoritative for UI layout unless the user explicitly overrides it.
- The generator must not leave UI nodes at generic default `RectTransform` values when concrete metadata exists in the input.

7. **Nested-prefab composition rule**
- If input defines prefab descriptions for nested structures (where one described structure is contained inside another), generation must be ordered from inner to outer.
- The child (nested) prefab must be generated first.
- Parent prefab generation must use the generated child prefab instance/reference, not a duplicated manual rebuild of the same subtree.
- This rule applies recursively for multi-level nesting.
- When a parent prefab instance already contains structural placeholder nodes from the nested prefab template (for example `*PrefabList` or `*PrefabParent`), those placeholders are part of the contract and must be preserved.
- Do not delete or replace `*PrefabList` nodes inside nested prefab instances. Any generated `*Prefab` children that correspond to that list must be parented under the existing `*PrefabList` node.
- Do not delete or replace `*PrefabParent` nodes inside nested prefab instances.
- If a `*PrefabParent` already contains the corresponding nested `*Prefab` instance, do not create a second instance. Reuse the existing nested instance and wire references to it.
- Creating a new nested `*Prefab` instance is allowed only when the required corresponding instance does not already exist under the expected `*PrefabParent`.
- Name customization and layout application for reused nested instances are allowed, but they must not break prefab-instance linkage or remove required template nodes.

---

8. **Generation completion logging rule**
- After running generation from the `Generate` button, the pipeline must write explicit debug logs to Unity Console.
- Logs must include a final completion message indicating generation finished successfully.
- Logs must include the names (and preferably asset paths) of prefabs created or updated during the run.
- If generation fails, log an error with the failing prefab/stage when identifiable.
`r`n---

## Target output structure (template)

Use this pattern under `<WORKSPACE_ROOT>`:

- `<WORKSPACE_ROOT>/<FeatureA>/Scripts/*.cs`
- `<WORKSPACE_ROOT>/<FeatureB>/Scripts/*.cs`
- ...
- `<WORKSPACE_ROOT>/_Generator/Scripts/PrefabGenerationProfile.cs`
- `<WORKSPACE_ROOT>/_Generator/Editor/PrefabGenerationPipeline.cs`
- `<WORKSPACE_ROOT>/_Generator/Editor/PrefabGenerationProfileEditor.cs`
- `<WORKSPACE_ROOT>/_Generator/Editor/PrefabGenerationBatchRunner.cs` (optional but recommended)
- `<WORKSPACE_ROOT>/_Generator/Asset/PrefabGenerationProfile.asset`

And generated prefabs saved into corresponding feature folders:
- `<WORKSPACE_ROOT>/<FeatureX>/<PrefabName>.prefab`

---

## Runtime and editor separation

### Runtime (`_Generator/Scripts`)
- `PrefabGenerationProfile : ScriptableObject`
  - stores only config data (for example `rootPath`).
  - no direct usage of `UnityEditor` APIs.

### Editor (`_Generator/Editor`)
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
2. Parse UI layout metadata when present:
- `Size: WIDTH x HEIGHT px @ REF_WIDTH x REF_HEIGHT`
- `Anchors: ...`
- `Offsets: ...`
- `FixedWidth: ... px`
- `FixedHeight: ... px`
- `Position: CX x CY px @ REF_WIDTH x REF_HEIGHT`
3. Build a model:
- Nodes (`name`, `components`, `children`)
- View binding requirements (`Class -> fields -> target node/component`)
- Reusable prefab definitions.
- Layout metadata for each `RectTransform` node when present:
  - reference resolution
  - anchors
  - offsets
  - fixed axis sizes
  - position fallback
4. Emit script plan:
- one file per class;
- split by folder domain.
5. Emit generation plan:
- helper builders for reusable subtrees;
- top-level builders for each prefab;
- final save paths per prefab.

---

## RectTransform metadata handling

For UI nodes, metadata from the input must drive concrete `RectTransform` setup in generated prefabs.

### Supported metadata

- `Size: WIDTH x HEIGHT px @ REF_WIDTH x REF_HEIGHT`
- `Anchors: Left, Right, Top, Bottom, CenterX, CenterY` or equivalent side-based combinations
- `Offsets: L=..., R=..., T=..., B=...`
- `FixedWidth: ... px`
- `FixedHeight: ... px`
- `Position: CX x CY px @ REF_WIDTH x REF_HEIGHT`

### Application rules

1. If anchors are present:
- Convert them into `anchorMin` and `anchorMax`.
- Apply side offsets through `offsetMin` and `offsetMax`.
- On stretched axes, prefer offsets over raw `sizeDelta`.
- On non-stretched axes, apply `Size`, `FixedWidth`, and `FixedHeight` through `sizeDelta`.

2. If anchors are absent but `Position` is present:
- Treat the element as fixed-position.
- Set a non-stretched anchor preset consistent with the metadata.
- Apply `anchoredPosition` from the provided center position in the stated reference resolution.
- Apply `sizeDelta` from `Size`.

3. If both stretch metadata and fixed-axis metadata exist:
- Keep offsets on stretched axes.
- Keep fixed size on non-stretched axes.

4. If metadata is partial:
- Use only explicitly provided values.
- Fall back to stable defaults for unspecified `RectTransform` fields.
- Do not invent exact offsets or coordinates that are not present in the input.

5. Reference resolution consistency:
- Treat `@ REF_WIDTH x REF_HEIGHT` as the coordinate space for all values tied to that node set.
- Do not silently mix multiple reference resolutions in one generated layout interpretation.

### Generation expectation

The generator should contain explicit helper logic for applying parsed layout metadata to `RectTransform` instances, instead of relying only on generic centered defaults.

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
- `ApplyRectTransformLayout(rectTransform, layoutMetadata)`
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
- Every provided `RectTransform` metadata block is either applied or explicitly reported as unsupported.

3. Structure
- One class per file.
- Generator scripts + asset in dedicated folder.

4. Output paths
- Prefabs saved into intended folders under workspace root.

5. Execution
- Do not run generation from the agent.
- Provide exact manual run steps.

---

## Agent output contract

When completing a task from this guide, the agent should output:

1. What was created.
2. Absolute paths of key files.
3. Whether generation was executed.
4. Exact manual steps:
- open profile asset;
- press `Generate`.
5. Any unverified points.

---

## Generic implementation notes

- Avoid assumptions about concrete prefab names beyond the parsed input.
- Keep logic data-driven where possible (builders reused by multiple prefabs).
- Do not modify unrelated project files.
- Keep diffs localized to workspace root.

