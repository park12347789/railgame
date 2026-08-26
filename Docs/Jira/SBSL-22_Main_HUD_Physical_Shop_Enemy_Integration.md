# SBSL-22 · 메인 HUD / 물리 상점 / 적 방해 통합

## 목표

현재 `main` 캠페인에 팀원 구현을 보존한 채 메인 HUD, 물리형 역 상점, 적 방해 시스템을
하나의 최신본으로 정리한다. 적은 기차를 직접 파괴하지 않고 플레이어의 이동과 작업을
방해하는 역할로 제한한다.

## 적용 범위

- 메인 HUD: 현재 노선, 다음 역, 볼트, 목표, 운반 품목, 상호작용, 상태 배너
- 물리 상점: 구매 화면 없이 직접 집기, 장착, 반납, 결제 후 출발
- 적 방해: 외곽 생성 지점에서 등장하며 열차 의존성 없이 동작 가능한 경량 연결
- 팀 회귀 테스트: 열차 레일, 적 스탯, 적 방해, HUD, 캠페인, 상점 동시 확인
- 시각 자료: `Assets/00.main/UI/References/MainShopHud_Sample.png`

## 설계 결정

1. 상점은 전체 화면 UI를 열지 않는다.
2. 기존 상호작용 키와 월드 오브젝트를 그대로 사용한다.
3. HUD 연결부는 게임 진행 결정을 소유하지 않는 읽기 전용 어댑터로 둔다.
4. 중앙 플레이 영역을 비우고 정보판을 화면 모서리에 배치한다.
5. 팀원 적 구현과 공용 인터페이스는 되돌리거나 교체하지 않는다.

## 주요 파일

- `Assets/00.main/UI/Scripts/RailgameHudPresenter.cs`
- `Assets/00.main/UI/Scripts/RailgameHudRuntimeBridge.cs`
- `Assets/00.main/UI/Prefabs/PF_CasualGameplayUI.prefab`
- `Assets/00.main/UI/MAIN_SHOP_HUD_DESIGN.md`
- `Assets/00.main/Enemy/Scripts/RailgameEnemyObstructionDirector.cs`
- `Assets/Tests/PlayMode/TeamFeatureRegressionTests.cs`

## 검증 결과

- Unity: `6000.5.2f1`
- HUD 프리팹 재생성: `RAILGAME_MAIN_HUD_PREFAB_OK`
- PlayMode: `27/27 passed`, `0 failed`, `0 skipped`
- Spring 실제 장면: 노선/볼트/목표/운반 정보 표시 확인
- 레거시 전면 구매창: 캠페인 장면 미사용 유지

## Jira

- 안건: [SBSL-22](https://hansola1234.atlassian.net/browse/SBSL-22)
- 메인 병합 커밋과 최종 상태는 병합 후 Jira 댓글에 기록한다.
