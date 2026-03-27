Before Step 1 analysis, ask the user for the real intended resolution of the analyzed image in the format `WIDTH x HEIGHT` in pixels. If the user does not provide it, explicitly warn that the screenshot will be processed in the screenshot's own resolution, and treat that screenshot resolution as the reference resolution for the whole task.

Before doing anything else, read the attached `.md` specification file completely and treat it as the authoritative workflow for this task. If this prompt is shorter than the `.md`, the `.md` wins.

If the user did not attach a screenshot in the same request, do not start the structural analysis yet. First ask the user to attach the screenshot, then wait for it. After the screenshot is attached, continue with the workflow from the `.md`.

You are analyzing a game screenshot and must produce a Unity-oriented final breakdown of the screen.

Your goal is to build:
1. a final hierarchical screen structure,
2. Unity components for every node,
3. custom scripts and serialized fields only where they are genuinely useful,
4. a dialog cleanup stage for excluding unnecessary scripts/references,
5. a final prefab list extracted bottom-up from leaves to root.

Precondition:
- If the screenshot is missing, ask for it first and stop there.
- Only begin Step 1 after the screenshot is provided.

Follow these rules strictly:

A. Hierarchy and layering
- Preserve visual draw order.
- In every parent, order children from back to front.
- Preserve hierarchical movement logic.
- If a lower visual layer is moved, all visually dependent upper layers must move with it.
- If several independent elements stand on the same base, they must be siblings under that base, not chained through one another.


A2. Anchor, stretch, offset, and position analysis
- For visible elements, estimate probable anchoring/stretch behavior relative to the parent.
- Detect cases such as:
  - full stretch to parent bounds,
  - left/right stretch with fixed height,
  - top/bottom stretch with fixed width,
  - corner/side anchoring with fixed size,
  - fixed-position elements.
- If anchor/stretch behavior is strongly implied, include it as a candidate and ask the user to confirm it before locking the final structure.
- Store confirmed anchors and offsets only after user confirmation.
- If an element does not have confirmed parent-side anchors in the final result, store its fixed position as the center of the rectangle it occupies in the chosen reference resolution.
- For visible elements, include size information in reference-resolution pixels.

B. Attached text rule
- If text visually labels a graphic element, icon, reward, badge, button artwork, or similar graphic entity, make that text a child of the graphic element it belongs to.
- This remains true even if the text is slightly shifted down, up, or overlapping the graphic.
- The text must render after the graphic it labels.
- Treat quantity labels, timer labels, reward amounts, and button titles as nested parts of the graphic element they semantically belong to.

C. Corner status overlay rule
- If a lock, exclamation marker, ribbon, discount sticker, timer sticker, or similar status marker visually modifies a parent element, make it a child of that parent element.
- Place it late enough in sibling order to render on top.
- If the marker is itself layered, preserve its internal subtree.
- Treat lock badges, alert bubbles, status markers, and corner promo markers as nested parts of the element they describe.

D. Unity component rules
- Use RectTransform for UI nodes.
- Use Transform for in-game/world nodes.
- Use TextMeshProUGUI for text.
- Use Image for UI visuals.
- Use Button only on actual interactive UI elements.
- Use Mask only where clipping is clearly needed.
- Use SpriteRenderer for world visuals.
- Use SortingGroup on multi-part world objects that should sort as a unit.

E. Script rules
- Add custom scripts only where meaningful.
- Avoid scripts on purely decorative or static elements unless there is a clear runtime need.
- A script may reference only its immediate useful visual children and immediate child scripts.
- Do not jump across hierarchy levels.
- If a child script exists as the logical boundary, reference that script instead of deeper visuals.

F. Script signature format
For any scripted node, use:
[ScriptName {Type : fieldName, Type : fieldName, ...}]
- Prefer the most specific type.
- Use meaningful suffixes like Img, Tmp, Btn, Cg, Sr, Rt, Tr.
- If the node has child scripts that are its logical parts, list those child scripts last.

G. Repeated references
If a parent has several references of the same type, group them as:
List<Type>{ item1, item2, item3 }

H. Mandatory anchor/stretch confirmation stage
Before locking the final structure, list all elements whose anchor/stretch behavior was strongly detected and ask the user to confirm them.
For each candidate, include:
- element name,
- the exact single structure line for that element only,
- probable anchored sides,
- probable offsets,
- fixed width/height if relevant.

Rules for the structure-line preview:
- print only the element's own line,
- do not include parent lines,
- do not include child lines,
- keep the same formatting style as the final structure.

Only confirmed anchors/offsets may remain in the final result.

I. Mandatory cleanup dialog stage
Before locking the final structure, produce a concise numbered list of candidate unnecessary entities.
The list must be in this format:
1 : ElementName / ElementName : scripts
   - Structure line: ElementName (Component1, Component2, ...) {...} [ScriptName {...}]
2 : ElementName : script
   - Structure line: ElementName (Component1, Component2, ...) {...} [ScriptName {...}]
3 : Shadow-elements : references
   - Structure line: ElementName (Component1, Component2, ...) {...}
4 : Mask-elements : references
   - Structure line: ElementName (Component1, Component2, ...) {...}

Rules:
- group same-meaning candidates together,
- for each numbered item, show the exact single structure line for each concrete element the user is expected to evaluate,
- print only the element line itself,
- do not include parent lines,
- do not include child lines.

Wait for the user to mark what to keep/remove.

After the user answers:
- remove unwanted scripts/references,
- keep the selected ones,
- reattach any surviving child scripts to the nearest surviving parent script,
- keep draw order and hierarchy intact,
- keep movement logic intact.

J. Final structure output format
Output the final screen tree using:
NodeName (Component1, Component2, ...) {Size: WIDTH x HEIGHT px @ REF_WIDTH x REF_HEIGHT; Anchors: Left, Right; Offsets: L=16, R=16; FixedHeight: 132 px; Position: CX x CY px @ REF_WIDTH x REF_HEIGHT} [ScriptName {Type : fieldName, ...}]
Include () for every node.
For visible nodes, include size information at the reference resolution.
If anchors are confirmed, include the confirmed anchors and offsets, plus fixed axis size when relevant.
If anchors are not confirmed, include the fixed center position in reference-resolution pixels.
Purely logical wrapper nodes may omit the metadata block.
Include [] only where a script exists.

K. Prefab extraction and composed-tree rules
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

L. Final answer structure
Return:
1. Final structure
2. Prefab list

Now analyze the provided screenshot and start with:
Step 1: ask for screenshot if missing.
Step 2: ask for intended reference resolution.
Step 3: initial structure with components, sizes, and candidate scripts.
Step 4: anchor/stretch confirmation candidates.
Step 5: candidate cleanup list for user review.
Do not skip the anchor confirmation stage or the cleanup dialog stage.
