# Project Agent Instructions

## Scope and Precedence

This file is the repository-level entrypoint for coding agents.

Read [`.agents/docs/project.md`](.agents/docs/project.md) before non-trivial
work. Repository-specific instructions in `.agents/docs/` take precedence over
broader workspace or user-level defaults.

## Documents

For non-trivial work, follow:

- [`.agents/docs/workflow.md`](.agents/docs/workflow.md)
- [`.agents/docs/testing.md`](.agents/docs/testing.md)

For tracked Git work, follow:

- [`.agents/docs/issue.md`](.agents/docs/issue.md)
- [`.agents/docs/branch.md`](.agents/docs/branch.md)
- [`.agents/docs/commit.md`](.agents/docs/commit.md)
- [`.agents/docs/pull-request.md`](.agents/docs/pull-request.md)

Follow [`.agents/docs/line-endings.md`](.agents/docs/line-endings.md) when
adding `.gitattributes`, normalizing line endings, or reviewing a diff where
every line changed.

For published releases, follow:

- [`.agents/docs/release.md`](.agents/docs/release.md)

Use project-local skills when installed and applicable. Skill instructions
define their own triggers, formats, and output paths.

## Conventions

- Write commit subjects and bodies, pull requests, and release notes in Korean.
  Leave code identifiers, API names, and error strings in their original form.
- Use Conventional Commits with a Korean summary:

```text
<type>: <한글 변경 사항>
```

Examples:

```text
fix: 대사 연타 시 대화 텍스트 깜박임 수정
perf: 맵 배경 갱신을 스테이지 변경 시에만 수행
```
