# 물리 역 상점 설계

## 확정 범위

```text
START
-> Spring: 승인 시드 5개 중 하나
-> 역 도착
-> 물리 상점
-> 장착 상품 일괄 결제
-> Summer: 승인 시드 5개 중 하나
-> 역 도착
-> 결과
```

- Spring과 Summer는 각각 스테이지 하나다. 시드 5개는 스테이지가 아니라 후보 맵이다.
- 두 시드는 새 게임 시작 때 한 번 선택하고 Retry 동안 유지한다.
- 선로 끝 도달은 실패다. 역 도착 때만 상점을 활성화한다.
- 상점 화면과 구매 버튼은 사용하지 않는다.
- Summer 이후 다음 스테이지가 없으므로 상점을 다시 열지 않는다.

## 월드 배치

```text
Spring GoalStation
|-- ShopDisplay
|   |-- OfferSlot_01
|   |   `-- PhysicalShopItem
|   |-- OfferSlot_02
|   |   `-- PhysicalShopItem
|   `-- OfferSlot_03
|       `-- PhysicalShopItem
|-- DeparturePoint
`-- Train
    |-- UpgradeSocket_01
    |-- UpgradeSocket_02
    |-- UpgradeSocket_03
    `-- BoltDepositPoint
```

- 객체는 프리팹과 씬 히어라키에 직접 배치한다.
- 참조는 인스펙터로 연결한다. `Find`, 자동 대체, 누락 참조 fallback은 금지한다.
- 누락·중복·호환 불가 상태는 출발을 막고 명확한 오류 로그를 남긴다.

## E 키 상호작용

기존 `ShoulderInteractor`와 `IShoulderInteractable`의 `E` 입력을 재사용한다.

| 플레이어 상태 | 대상 | E 결과 |
|---|---|---|
| 빈손 | 진열 상품 | 집기 |
| 상품 운반 중 | 호환되는 빈 슬롯 | 장착 |
| 빈손 | 장착 상품 | 분리 후 집기 |
| 상품 운반 중 | 점유/비호환 슬롯 | 거부 + 오류 로그 |

- 집기, 장착, 분리는 모두 같은 키다.
- 분리는 빈손일 때만 가능하다.
- 조준 대상 하나만 처리한다. 한 입력으로 두 상태를 바꾸지 않는다.
- 미구매 상품을 들고 있는 동안에는 출발할 수 없다.

## 상품 상태

```text
Displayed -> CarriedUnowned -> MountedPending
     ^              |                |
     `--------------+<--- detach ----+

MountedPending -- checkout --> OwnedMounted
OwnedMounted -- detach --> CarriedOwned -- mount --> OwnedMounted
```

- `Displayed`, `CarriedUnowned`, `MountedPending`는 아직 미구매다.
- `Owned` 상품은 분리·재장착해도 다시 결제하지 않는다.
- 미구매 상품을 분리하면 결제 합계에서 즉시 제외한다.
- 환불, 판매, 부분 결제는 없다.

## 결제와 출발

```text
pendingTotal = 장착됨 && 미소유인 상품의 가격 합계
canDepart = StationShop 상태
            && 미구매 상품을 들고 있지 않음
            && 현재 볼트 >= pendingTotal
            && 결제 진행 중이 아님
```

- 장착 시 가격을 고정한다. 출발 직전 표시 가격과 차감 가격이 달라지지 않는다.
- 상품을 하나도 장착하지 않으면 합계는 0이며 출발 가능하다.
- 기존 소유 상품 가격은 합계에서 제외한다.
- 같은 상품 인스턴스가 두 슬롯에 잡히면 출발을 막는다.
- 볼트 부족 시 자동 분리하거나 일부만 구매하지 않는다.

출발 처리는 한 번에 수행한다.

```text
1. 출발 입력 잠금
2. 슬롯과 상품 참조 검증
3. pendingTotal 재계산
4. 보유 볼트 재검사
5. 볼트 차감
6. pending 상품 소유 확정
7. Summer 시드 적용
8. Summer 씬 로드
```

- 어느 검증이든 실패하면 볼트, 소유권, 스테이지 상태를 변경하지 않는다.
- 현재 범위에는 저장/Continue 기능이 없으므로 메모리 내 `departureInProgress`로 중복 출발만 막는다.
- 영구 저장이 추가될 때만 출발 ID와 원자 저장을 추가한다.

## 캠페인 상태

```text
Lobby
-> LoadingSpring
-> SpringPlaying
-> SpringFailed -> LoadingSpring
-> StationShop
-> LoadingSummer
-> SummerPlaying
-> SummerFailed -> LoadingSummer
-> Results
```

- Spring Retry는 같은 Spring 시드를 다시 사용한다.
- Summer Retry는 같은 Summer 시드를 다시 사용한다.
- 잘못된 상태에서 결제·출발·결과 호출은 거부하고 로그를 남긴다.

## 기존 코드 처리

### 재사용

- `Assets/01.hansol/ShoulderView/Runtime/IShoulderInteractable.cs`
- `Assets/01.hansol/ShoulderView/Runtime/ShoulderInteractor.cs`
- `Assets/01.hansol/ShoulderView/Runtime/ShoulderShopEconomy.cs`
- `Assets/00.main/Map/Scripts/ProceduralMapGenerator.cs`의 승인 variant 선택

### 제거 대상

- `RailgameShopScreen` 버튼 구매 흐름
- `RailgameGameMenuController`의 언제든 열리는 `Shopping` 상태
- `OpenShopButton`
- Builder가 생성하는 상점 패널과 테스트 상품

Builder가 씬과 프리팹을 재생성하므로 Builder와 생성 결과를 함께 변경해야 한다.

## 소유 경계

- Main: 캠페인 상태, 물리 상품, 결제, 역 활성화, 다음 씬 전환.
- Hansol: 기존 E 조준 상호작용과 볼트 표시/경제 컴포넌트. 직접 수정하지 않는다.
- Bogyeong: 열차 이동과 `TrainSection`. 직접 수정하지 않는다.
- Seokmin: 적 방해 이벤트. 상점 결제 상태를 소유하지 않는다.

현재 `TrainBehaviour`는 `Awake`에서 `TrainSection[]`을 한 번 캐시한다. 런타임에 새 화차를 꼬리에 추가해도 이동·틱 대상에 자동 포함되지 않는다.

따라서 1차 구현은 기존 열차의 고정 `UpgradeSocket`에 상품을 장착한다. 실제 화차 증설은 열차 담당자가 런타임 추가·제거 API와 재배열 규칙을 제공한 뒤 연결한다. 임시 시각 전용 화차 fallback은 만들지 않는다.

## 최소 검증

1. 새 게임은 Spring/Summer variant를 각각 0..4에서 선택한다.
2. 각 스테이지 Retry는 같은 variant와 seed/hash를 유지한다.
3. Spring 목표 도착 전에는 상점 상품과 출발 지점을 사용할 수 없다.
4. E로 집기, 장착, 분리를 순서대로 수행한다.
5. 보유 4, 신규 장착 2+3이면 출발 실패하고 상태가 변하지 않는다.
6. 보유 5, 신규 장착 2+3이면 잔액 0, 상품 소유 확정 후 Summer로 이동한다.
7. 기존 소유 상품은 결제 합계에 포함되지 않는다.
8. 신규 상품을 분리하면 결제 합계에서 제외된다.
9. 상품 0개, 볼트 0개여도 Summer로 이동할 수 있다.
10. Summer 선로 끝은 실패, 목표 도착은 결과 상태가 된다.

## 구현 순서

1. 캠페인 런 상태와 Spring/Summer 전환.
2. Spring 역의 상점 활성화와 출발 지점.
3. 상품 집기·장착·분리 및 고정 슬롯 프리팹.
4. 볼트 합계·일괄 결제·소유 확정.
5. Builder 반영.
6. PlayMode와 CLI 검증.

## 구현 상태 (2026-08-25)

완료:

- 단일 START와 Spring -> StationShop -> Summer -> Results 캠페인 상태.
- 새 게임에서 계절별 승인 시드 선택, Retry 시 동일 시드 유지.
- E 집기·장착·분리, 타입 호환 고정 슬롯.
- 장착 미소유품 합계 계산, 부족 시 전 상태 유지, 출발 시 일괄 결제.
- 월드 볼트 3개/계절, 운반 후 반납할 때만 캠페인 볼트 증가.
- 물리 상점·상품·볼트·플레이어 운반 지점 프리팹 및 씬 배치.
- 기존 구매창과 언제든 여는 SHOP TEST 흐름 제거.
- PlayMode 20/20, 승인 맵 CLI 10/10 통과.

팀 연결 대기:

- 메인 캠페인 씬에 실제 팀 열차 프리팹이 없어 `BoltDepositPoint_ConnectToTrain`은 시작 지점에 배치했다. 열차 인계 후 자식으로 이동한다.
- `TrainBehaviour`가 `TrainSection[]`을 Awake에서 한 번만 캐시하므로 실제 화차 런타임 증설·분리는 아직 연결하지 않았다.
- 열차의 역 도착/선로 끝 이벤트가 메인 씬에 연결되지 않아 `CompleteAtStation`·`FailAtRailEnd` 자동 호출은 대기 상태다.
- 임시 화차 이동이나 자동 성공 판정 fallback은 추가하지 않았다.
