# Protect your world

The owner of ShroudForge operates **Velaris, a public Enshrouded server where people actually play**. That changes how we approach development: there isn't an automatically available disposable public server to test every modification.

## Our approach

1. **Check first:** identify what is running, which game/mod versions are installed and whether players are connected.
2. **Validate without disrupting:** run source builds, fixtures and read-only diagnostics before a live test.
3. **Ask before acting:** a GitHub merge, CI run or planned update never authorizes a restart, save edit or mod injection.
4. **Make rollback realistic:** preserve original files, document dependencies and protect saves made after deployment.
5. **Don't treat "unknown" as "empty":** if the player roster is unavailable, assume people may be online.

> [!WARNING]
> Replacing an older world backup can erase newer player progress. A byte-verified **live** copy of save files may not be a fully consistent game snapshot.

## Operator documentation

The detailed [public-server safety and desktop handoff changes](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/25) are staged for review. **They are not an automated deployment process.**

[See the project status](../project/README.md) · [Getting started](../start/README.md) · [Back to the guide](../README.md)
