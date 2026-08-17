# Release Notes Workflow

## Why

Release notes are read by whoever downloads and plays this version. They should
describe what changed for that person, not how the change was produced.

## Format

Write the summary and headings in Korean:

```markdown
한 줄 요약.

## 변경
- 플레이어가 체감하는 변화

## 수정
- 고친 버그

## 주의
- 기존과 달라져 미리 알아야 할 것

전체 변경: #PR번호
```

Drop any heading with no content.

Example:

```markdown
매 프레임 반복되던 불필요한 작업을 정리한 성능 패치.

## 수정
- 발사체와 적 이동 속도가 기기 성능에 따라 느려지던 문제

## 주의
- 발사체가 이전보다 빠르다. 설정된 속도대로 돌아온 결과다

전체 변경: #40
```

## Do Not Include

- **Build duration or CI progress** — untrue within minutes of publishing.
- **Plans for the next version, or remaining work** — belongs in that version's
  notes.
- **Process narrative, or what was left unverified** — belongs in the pull
  request.
- **Internal symbol names** — translate into the observable change. Write
  "맵 화면 프레임 저하", not `GameObject.Find`.

## Download Table

The `links` job in `.github/workflows/release.yml` appends this block to the
notes after builds finish:

```markdown
<!-- release-links:start -->
...
<!-- release-links:end -->
```

It carries the per-platform download links, the web play URL, and the `xattr`
instructions for the unsigned macOS build. Do not hand-write it, and do not
remove it.

`gh release edit --notes` replaces the entire body. Using it on a release whose
build already finished deletes this block, and no job will append it again.
When the block is present, keep it verbatim and change only the text above it.

Pass the body as a file rather than interpolating multiline content:

```bash
gh release edit "$TAG" --notes-file /tmp/release-notes.md
```

Read the release back with `gh release view` afterwards and confirm the block
survived.

## Version

Tags are `vMAJOR.MINOR.PATCH`. Keep `bundleVersion` in
`ProjectSettings/ProjectSettings.asset` equal to the tag without the `v`.

The release workflow reads only the tag, so `bundleVersion` has to be bumped
separately. If it is skipped, the built game reports a version that disagrees
with its tag. Bump it in its own commit and tag that commit.

## Before Publishing

- Confirm the tag points at the intended commit.
- Confirm `bundleVersion` matches the tag.
- Confirm the notes contain no process narrative or forward-looking plans.
