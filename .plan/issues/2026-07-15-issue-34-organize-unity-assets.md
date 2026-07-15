# 2026-07-15 — Unity 프로젝트 asset 구조 및 namespace 정리

- Date: 2026-07-15
- GitHub Issue: #34
- Status: Complete

## Goal

- first-party C# script와 Unity asset을 역할별 최상위 폴더에 배치한다.
- namespace를 `WordVenture.<Domain>` 규칙으로 통일한다.
- `.meta` GUID와 serialized asset 참조를 유지한다.

## Non-goals

- gameplay 동작 변경
- third-party 내부 코드/구조 수정
- asmdef 경계 도입
- public API와 serialized field의 광범위한 이름 변경

## Context / Constraints

- 기존 asset은 작성자 이니셜, scene, resource 폴더에 혼재되어 있다.
- fork `main`은 upstream보다 앞서 있어 이번 commit만 `upstream/main`에 재적용해야 한다.
- Unity Editor가 열린 상태에서는 asset import가 `.meta`와 package lock을 다시 쓸 수 있다.

## Approach (Checklist)
- [x] **Step 0: Recon** (script, namespace, scene, prefab, image, ScriptableObject, vendor 경계 확인)
- [x] **Step 1: Implementation** (`Assets/Scripts/<Domain>` 이동과 namespace 갱신)
- [x] **Step 2: Asset organization** (`Art`, `Prefabs`, `Scenes`, `ScriptableObjects`, `ThirdParty` 정리)
- [x] **Step 3: Tests** (원본 asset 복원, namespace/placement 검사, Unity compile)
- [x] **Step 4: Rollout / Rollback** (upstream 기반 branch, commit, fork push, upstream PR)

## Validation
- **Commands to run:** `git diff --check`, namespace/type reference 검사, asset extension 위치 검사, Unity compile 검사
- **Expected output:** compile error, legacy namespace, misplaced first-party asset, unintended asset content change 없음

## Risks & Rollback
- **Risks:** `.meta` 재직렬화, scene path 누락, fork-only commits의 upstream PR 혼입
- **Rollback steps:** PR commit revert 또는 `.meta` 원본 복원 후 다시 import

## Open Questions
- 없음.
