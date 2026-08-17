# Project Agent Instructions

## Scope and Precedence

This file is the repository-level entrypoint for coding agents.

Repository-specific instructions in `.agents/docs/` take precedence over broader
workspace or user-level defaults.

## Documents

For published releases, follow:

- [`.agents/docs/release.md`](.agents/docs/release.md)

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
