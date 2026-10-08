# Branch Workflow

## Why

Changes reach `main` through `develop`, so `main` only receives versions that
were already integrated and verified together. This follows the branch flow in
[`CONTRIBUTING.md`](../../CONTRIBUTING.md#브랜치-흐름).

## Flow

1. Create one branch per feature or fix.
2. When the work is done, merge `develop` into your branch first and resolve
   conflicts and errors there.
3. Merge the verified change into `develop`.
4. Merge a stabilized version from `develop` into `main`.

When `develop` does not exist on the remote, ask the developer before creating
it or targeting `main` directly.

## Naming

Name the branch after the change type and the feature:

```text
<type>/<feature>
```

Examples:

```text
feat/elemental-status
fix/combine-card-return
```

Allowed types match [`commit.md`](commit.md):
- `feat`: user-visible capability
- `fix`: defect correction
- `refactor`: behavior-preserving structure change
- `perf`: measured performance improvement
- `test`: test-only change
- `docs`: documentation-only change
- `build`: build or dependency change
- `ci`: automation change
- `chore`: maintenance not covered above

## Lifecycle

- Keep one feature or fix per branch.
- Never force-push a shared branch without coordination.
- Delete the branch after merge when no follow-up work depends on it.
