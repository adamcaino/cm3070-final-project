# _Project layout

This folder is reserved for game-specific content owned by this project.

## Recommended conventions
- Put all custom gameplay code and assets here.
- Leave marketplace and third-party packages in their existing top-level folders.
- Prefer Addressables for runtime loading; avoid using Resources unless there is a clear reason.
- Keep scenes split by purpose: bootstrap, gameplay, UI, and test.
- Keep editor-only tooling inside a folder named `Editor`.

## Folder guide
- Art: models, materials, textures, animations
- Audio: music, SFX, mixer assets
- Data: ScriptableObjects and Addressables content
- Prefabs: reusable game objects
- Scenes: boot, game, UI, and test scenes
- Scripts: core systems, gameplay, networking, utilities, UI, and editor tools
- Settings: project-owned configuration assets
- UI: fonts and sprites
- VFX: particles and visual effects
