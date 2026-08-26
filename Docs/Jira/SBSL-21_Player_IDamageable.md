# SBSL-21 플레이어 IDamageable 적용

## 작업 목적

적의 근접 공격, 스켈레톤 화살, 박쥐 급강하 공격이 플레이어에게 공통된 방식으로 피해를 줄 수 있도록 플레이어 체력 시스템에 `IDamageable`을 적용한다.

## 구현 내용

- `RailgamePlayerHealth` 컴포넌트 추가
- 공용 `IDamageable.TakeDamage(float amount)` 구현
- 최대 체력 및 현재 체력 관리
- 체력 감소, 회복, 초기화 기능 제공
- 체력 변경 이벤트 `OnHealthChanged` 제공
- 사망 이벤트 `OnDied` 제공
- 사망 후 중복 피해와 중복 사망 이벤트 방지
- 플레이어 프리팹의 기본 최대 체력을 100으로 설정
- 적 AI가 플레이어를 찾을 수 있도록 플레이어 프리팹 태그를 `Player`로 설정

## 적용 파일

- `Assets/00.main/Player/Scripts/RailgamePlayerHealth.cs`
- `Assets/00.main/Player/Prefabs/PF_RailgamePlayer.prefab`
- `Assets/04.seokmin/02_Scripts/Interface/IDamageable.cs`

## 동작 흐름

1. 적 AI가 `Player` 태그로 플레이어를 탐색한다.
2. 근접 공격, 화살 또는 박쥐 급강하가 플레이어의 `IDamageable`을 찾는다.
3. `TakeDamage`가 호출되면 현재 체력에서 공격력이 차감된다.
4. 체력이 변경되면 `OnHealthChanged` 이벤트가 호출된다.
5. 체력이 0이 되면 `OnDied` 이벤트가 한 번 호출된다.

## 완료 조건

- 플레이어 프리팹에 `RailgamePlayerHealth`가 연결되어 있다.
- 플레이어가 좀비 근접 공격에 피해를 받는다.
- 플레이어가 스켈레톤 화살에 피해를 받는다.
- 플레이어가 박쥐 급강하 공격에 피해를 받는다.
- 체력은 0 아래로 내려가지 않는다.
- 사망 이벤트가 중복 호출되지 않는다.
- 회복 시 최대 체력을 초과하지 않는다.

## Unity 에디터 확인 방법

1. 프로젝트를 열고 스크립트 컴파일 오류가 없는지 Console을 확인한다.
2. `PF_RailgamePlayer` 프리팹에 `RailgamePlayerHealth`가 있는지 확인한다.
3. 플레이어 태그가 `Player`인지 확인한다.
4. 테스트 씬에서 각 적의 공격을 맞아 `CurrentHealth`가 감소하는지 확인한다.
5. 체력이 0이 되었을 때 `IsDead`가 활성화되고 `OnDied`가 한 번만 발생하는지 확인한다.

## 참고

이번 작업에서는 체력 데이터와 피해 수신 기반만 구현했다. 체력 UI, 사망 애니메이션, 게임 오버 화면 및 리스폰 처리는 후속 작업에서 `OnHealthChanged`와 `OnDied` 이벤트에 연결할 수 있다.
