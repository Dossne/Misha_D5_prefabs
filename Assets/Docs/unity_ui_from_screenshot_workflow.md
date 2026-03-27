# Unity UI / Gameplay Screen Extraction from a Screenshot

## Goal

Use a **single initial prompt** with an AI agent to turn a screenshot into a **production-oriented screen breakdown** for Unity, including:

1. a **final hierarchical screen structure**,
2. **Unity components** for every node,
3. **custom scripts** and serialized-field signatures only where needed,
4. a **dialog stage for pruning unnecessary entities**,
5. a **final prefab list**, extracted **bottom-up from leaves to root**.

The workflow below is designed to reproduce the exact kind of result we converged on in the conversation that led to this document.

**Important:** the companion master prompt must explicitly instruct the AI agent to read this `.md` file completely before starting. This markdown file is the authoritative specification.

**Important:** if the user did not attach a screenshot in the initial request, the agent must first ask the user to attach the screenshot and wait for it. The agent must not begin structural analysis, cleanup suggestions, or prefab extraction before the screenshot is available.

---

## Input assumptions

- Input is a **single flat screenshot** of a game screen.
- The screen may contain both:
  - **UI** elements (HUD, buttons, panels, progress bars, labels, badges, overlays), and
  - **in-game / world** elements (enemies, projectiles, portals, totems, map parts, decorative world objects).
- The target engine is **Unity**.
- The result must preserve:
  - **visual draw order**, and
  - **hierarchical movement logic**.

---

## Screenshot availability precondition

Before any structural work begins, the agent must verify that a screenshot is actually present in the request.

Rules:
- If the screenshot is attached, proceed normally.
- If the screenshot is not attached, the agent must ask the user to attach it.
- Until the screenshot is attached, the agent must not produce:
  - the initial structure,
  - the cleanup candidate list,
  - the final structure,
  - the prefab list.

Once the screenshot is attached, continue with the normal workflow.

## Core rules

### 1) Preserve render order

Unity draws later siblings over earlier siblings.

Therefore, child order inside each parent must be written **from back to front**:
- earlier child = visually behind,
- later child = visually on top.

### 2) Preserve movement hierarchy

The hierarchy must not be flat when a visual element is composed of layers.

If a lower visual layer is moved, all layers that are visually built on top of it must move with it.

Use this principle recursively:

```text
LowestVisualBase
└─ NextVisualLayer
   └─ NextVisualLayer
      └─ TopVisualLayer
```

If several independent things stand on the same base, they should be siblings under that base, not children of each other.

### 3) Use correct transform type

- For **UI** nodes use `RectTransform`.
- For **in-game/world** nodes use `Transform`.

### 4) Use appropriate standard Unity components

Typical mapping:

- UI visual node: `RectTransform, Image`
- UI text: `RectTransform, TextMeshProUGUI`
- UI click target: `RectTransform, Image, Button`
- UI clipping node: `RectTransform, Image, Mask`
- World visual node: `Transform, SpriteRenderer`
- World multi-part object root: `Transform, SpriteRenderer, SortingGroup`

### 5) Add custom scripts only where meaningful

Do **not** add view scripts to every node.

Avoid scripts and serialized fields for nodes that are usually static and edited once in the prefab unless there is a clear code use case.

Typical candidates for removal during cleanup:
- pure shadow layers,
- pure mask layers,
- purely decorative glow layers,
- static frames / trims / backboards,
- static backgrounds / body / backplate visuals,
- static environment decor.

### 6) No reference jumping across hierarchy levels

A script may reference only:
- its own immediate useful visual children, and/or
- its own immediate child scripts.

Do **not** skip levels by referencing grandchildren or deeper descendants directly.

If a child script survives cleanup, upper levels should reference the **child script**, not the grandchild visuals inside it.

### 7) Group repeated same-type references

When a parent contains repeated child references of the same type, wrap them as:

```text
List<Type>{ item1, item2, item3 }
```

Examples:

```text
List<PortalView>{ leftPortalView, rightPortalView }
List<EnemyView>{ enemyView1, enemyView2, enemyView3 }
List<CrystalSlotView>{ crystalSlotView1, crystalSlotView2, crystalSlotView3 }
```

### 8) Prefabs must be extracted bottom-up

Extract prefabs **starting from leaves and moving upward**.

Rules:
- Do **not** change the internal subtree structure when extracting a prefab.
- Do **not** place bare `PrefabType()` or `List<PrefabType>()` directly as child nodes in a composed higher-level tree.
- For a repeated prefab group, create a wrapper node named `<PrefabType>List` at the point where `List<PrefabType>()` would otherwise appear.
- `<PrefabType>List` must have the correct transform component for that branch:
  - `RectTransform` for UI,
  - `Transform` for world.
- In the nearest parent script, add two adjacent serialized fields for that repeated prefab group in this order:
  1. the wrapper transform field using the matching transform type,
  2. the prefab list field of type `List<PrefabType>`.
- For a single nested prefab instance, create a wrapper node named `<PrefabType>Parent` at the point where `PrefabType()` would otherwise appear.
- `<PrefabType>Parent` must have the correct transform component for that branch:
  - `RectTransform` for UI,
  - `Transform` for world.
- In the nearest parent script, add two adjacent serialized fields for that single nested prefab in this order:
  1. the wrapper transform field using the matching transform type,
  2. the prefab field of type `PrefabType`.
- The wrapper node itself normally carries only the transform component unless the source subtree requires more.
- Output the prefab list **from leaves to root**.

Examples:
- repeated: `PortalPrefabList (Transform)` with parent-script fields `Transform : portalPrefabListTr, List<PortalPrefab> : portalPrefabs`
- single: `TotemPrefabParent (Transform)` with parent-script fields `Transform : totemPrefabParentTr, TotemPrefab : totemPrefab`

### 9) Normalize repeated-entity prefab names

When extracting a prefab from a repeated entity subtree, remove numbering/index fragments such as `_01_`, `_02_`, etc. from the prefab subtree.

This rule applies to:
- the prefab root name,
- all nested node names inside that prefab subtree.

Example:
- source subtree root: `Enemy_01_TopLeftShadow`
- prefab subtree root: `Enemy_TopLeftShadow`

Example:
- source child: `Enemy_01_TopLeftBody`
- prefab child: `Enemy_TopLeftBody`

Rationale:
- the prefab should not encode which numbered instance it came from in the source structure,
- the prefab and its internal parts must be instance-agnostic,
- numbering remains only in the final screen structure where concrete instances exist.

---

## Required output format

The final response must contain **two sections**.

### Section A — Final structure

A single full tree of the screen.

Each node must use this format:

```text
NodeName (Component1, Component2, ...) [ScriptName {Type : fieldName, Type : fieldName, ...}]
```

Rules:
- `()` is required for the node's Unity components.
- `[]` is present only if the node should have a custom script.
- Use the **most specific type** in serialized fields.
  - Example: prefer `TextMeshProUGUI` over `RectTransform` for a text field.
- Use meaningful field-name suffixes such as `Img`, `Tmp`, `Btn`, `Cg`, `Sr`, etc.
- If a script contains child scripts that are logical parts of it, list those **last** in the script signature.

### Section B — Prefab list

Output a list of prefabs.
For each prefab, print:

1. prefab name,
2. prefab tree.

For example:

```text
### ProgressBarPrefab
ProgressBarShadow (RectTransform, Image)
└─ ProgressBarBackPlate (RectTransform, Image)
   └─ ProgressBarTrack (RectTransform, Image) [ProgressBarTrackView {Image : progressBarFillImg, TextMeshProUGUI : progressBarValueTmp}]
      ├─ ProgressBarFillMask (RectTransform, Image, Mask)
      │  └─ ProgressBarFill (RectTransform, Image)
      └─ ProgressBarFrame (RectTransform, Image)
         └─ ProgressBarValueText (RectTransform, TextMeshProUGUI)
```

Wrapper-node rule for composed prefab trees:
- never insert bare `PrefabType()` or `List<PrefabType>()` directly as tree children,
- use `<PrefabType>Parent (Transform/RectTransform)` for a single nested prefab instance,
- use `<PrefabType>List (Transform/RectTransform)` for repeated nested prefab instances,
- in the nearest parent script, place the wrapper transform field first and the prefab or prefab-list field immediately after it.

---

## Required workflow

The agent must follow these stages in order.

### Stage 1 — Initial screenshot decomposition

From the screenshot, build an initial hierarchical breakdown that:
- separates UI from world/gameplay where appropriate,
- preserves visual layering,
- preserves hierarchical movement logic,
- keeps independent branches independent,
- avoids flattening multi-layer elements.

### Stage 2 — Add Unity components

For every node, assign standard Unity components.

Rules:
- UI branch -> `RectTransform`
- World branch -> `Transform`
- Text -> `TextMeshProUGUI`
- Interactive UI -> include `Button`
- Masked fill areas -> include `Mask`
- Multi-part world objects that should sort as one -> include `SortingGroup` at the correct root

### Stage 3 — Add candidate custom scripts and serialized fields

Add scripts only where there is a plausible runtime need.

Rules:
- No script on every decorative node.
- No deep reference jumping.
- Immediate child visuals only.
- Immediate child scripts only.
- If a child script exists and is the logical boundary, the parent should reference that script rather than deeper visuals.

### Stage 4 — Dialog stage for excluding unnecessary entities

This stage is mandatory.

The agent must produce a **numbered candidate-removal list** before locking the final structure.

The list must be concise and formatted like this:

```text
1 : ElementName / ElementName : scripts
2 : ElementName : script
3 : Shadow-elements : references
4 : Mask-elements : references
```

Rules:
- Group identical same-meaning candidates from different branches into one item.
- Keep the list short and practical.
- Do not over-explain.
- Wait for user selection.

The user should be able to answer with keep/remove instructions, for example:

```text
1 keep first only
2 remove
3 remove
4 keep
```

or with plus notation if desired.

### Stage 5 — Cleanup pass after user selection

After the user marks what to keep/remove:
- remove the unwanted scripts and references,
- keep the selected ones,
- if a surviving child script loses its former scripted parent, reattach it to the **nearest surviving parent script**,
- keep hierarchy and draw order intact,
- keep movement logic intact.

### Stage 6 — Finalize repeated references

Convert repeated same-type references to `List<Type>{ ... }` in the final structure.

### Stage 7 — Extract prefabs bottom-up

Identify reusable and composition prefabs starting from the leafiest valid reusable structures and move upward.

Rules:
- Do not mutate the subtree structure when extracting a prefab.
- For repeated entities, remove numbering/index fragments such as `_01_`, `_02_`, etc. from the prefab root name and from all nested node names inside that prefab subtree.
- In higher-level composed trees, do not insert bare `PrefabType()` or `List<PrefabType>()` nodes.
- For repeated nested prefab instances, create `<PrefabType>List (Transform/RectTransform)` at the hierarchy point where the repeated prefab group belongs.
- For a single nested prefab instance, create `<PrefabType>Parent (Transform/RectTransform)` at the hierarchy point where the single prefab belongs.
- In the nearest parent script, serialize the wrapper transform field first and the prefab or prefab-list field immediately after it.
- Output prefab descriptions from leaves to root.

---

## Quality checklist

Before presenting the final result, verify all of the following:

- Draw order is correct.
- Lower visual bases move their upper dependent layers.
- Independent branches are not incorrectly chained.
- UI uses `RectTransform`.
- World elements use `Transform`.
- Text uses `TextMeshProUGUI`.
- Scripts exist only where justified.
- No deep reference jumping.
- Repeated references are grouped into `List<Type>{ ... }`.
- Prefabs are extracted bottom-up.
- Higher composed trees use `<PrefabType>Parent` / `<PrefabType>List` wrapper nodes instead of bare prefab placeholders.
- Parent scripts serialize wrapper transforms immediately before prefab or prefab-list fields.

---

## Master prompt for the AI agent

Use the following as the **single initial prompt** for the AI agent.

```text
Before doing anything else, read the attached `.md` specification file completely and treat it as the authoritative workflow for this task. If this prompt is shorter than the `.md`, the `.md` wins.

You are analyzing a game screenshot and must produce a Unity-oriented final breakdown of the screen.

Your goal is to build:
1. a final hierarchical screen structure,
2. Unity components for every node,
3. custom scripts and serialized fields only where they are genuinely useful,
4. a dialog cleanup stage for excluding unnecessary scripts/references,
5. a final prefab list extracted bottom-up from leaves to root.

Follow these rules strictly:

A. Hierarchy and layering
- Preserve visual draw order.
- In every parent, order children from back to front.
- Preserve hierarchical movement logic.
- If a lower visual layer is moved, all visually dependent upper layers must move with it.
- If several independent elements stand on the same base, they must be siblings under that base, not chained through one another.

B. Unity component rules
- Use RectTransform for UI nodes.
- Use Transform for in-game/world nodes.
- Use TextMeshProUGUI for text.
- Use Image for UI visuals.
- Use Button only on actual interactive UI elements.
- Use Mask only where clipping is clearly needed.
- Use SpriteRenderer for world visuals.
- Use SortingGroup on multi-part world objects that should sort as a unit.

C. Script rules
- Add custom scripts only where meaningful.
- Avoid scripts on purely decorative or static elements unless there is a clear runtime need.
- A script may reference only its immediate useful visual children and immediate child scripts.
- Do not jump across hierarchy levels.
- If a child script exists as the logical boundary, reference that script instead of deeper visuals.

D. Script signature format
For any scripted node, use:
[ScriptName {Type : fieldName, Type : fieldName, ...}]
- Prefer the most specific type.
- Use meaningful suffixes like Img, Tmp, Btn, Cg, Sr, Rt, Tr.
- If the node has child scripts that are its logical parts, list those child scripts last.

E. Repeated references
If a parent has several references of the same type, group them as:
List<Type>{ item1, item2, item3 }

F. Mandatory cleanup dialog stage
Before locking the final structure, produce a concise numbered list of candidate unnecessary entities.
The list must be in this format:
1 : ElementName / ElementName : scripts
2 : ElementName : script
3 : Shadow-elements : references
4 : Mask-elements : references
Group same-meaning candidates together.
Wait for the user to mark what to keep/remove.

After the user answers:
- remove unwanted scripts/references,
- keep the selected ones,
- reattach any surviving child scripts to the nearest surviving parent script,
- keep draw order and hierarchy intact,
- keep movement logic intact.

G. Final structure output format
Output the final screen tree using:
NodeName (Component1, Component2, ...) [ScriptName {Type : fieldName, ...}]
Include () for every node.
Include [] only where a script exists.

H. Prefab extraction and composed-tree rules
After finalizing the cleaned structure, extract prefabs bottom-up from leaves to root.
- Do not change subtree structure when extracting a prefab.
- If the prefab comes from a repeated entity subtree, remove numbering/index fragments such as `_01_`, `_02_`, etc. from the prefab root name and all nested node names inside that prefab subtree.
- Do not place bare `PrefabType()` or `List<PrefabType>()` directly as child nodes in a composed tree.
- For a repeated prefab group, create a wrapper node named `<PrefabType>List` at the point where `List<PrefabType>()` would otherwise appear.
- `<PrefabType>List` must have the correct transform component for that branch: `RectTransform` for UI, `Transform` for world.
- In the nearest parent script, add two adjacent serialized fields for that repeated prefab group in this order:
  1. the wrapper transform field using the matching transform type,
  2. the prefab list field of type `List<PrefabType>`.
- For a single nested prefab instance, create a wrapper node named `<PrefabType>Parent` at the point where `PrefabType()` would otherwise appear.
- `<PrefabType>Parent` must have the correct transform component for that branch: `RectTransform` for UI, `Transform` for world.
- In the nearest parent script, add two adjacent serialized fields for that single nested prefab in this order:
  1. the wrapper transform field using the matching transform type,
  2. the prefab field of type `PrefabType`.
- The wrapper node itself normally carries only the transform component unless the source subtree requires more.
- Output prefab descriptions from leaves to root.

Examples:
- repeated: `PortalPrefabList (Transform)` with parent-script fields `Transform : portalPrefabListTr, List<PortalPrefab> : portalPrefabs`
- single: `TotemPrefabParent (Transform)` with parent-script fields `Transform : totemPrefabParentTr, TotemPrefab : totemPrefab`

I. Final answer structure
Return:
1. Final structure
2. Prefab list

Now analyze the provided screenshot and start with:
Step 1: initial structure with components and candidate scripts.
Step 2: candidate cleanup list for user review.
Do not skip the cleanup dialog stage.
```

---

## Suggested user instruction wrapper

If you want to use the master prompt in a real run, prepend a short instruction like this:

```text
Use the screenshot I attached. Follow the workflow exactly. Do not skip the cleanup dialog stage. After I answer the cleanup list, continue to the final structure and prefab list.
```

