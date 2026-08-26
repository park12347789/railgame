# Main HUD + Station Shop

## Direction

The interface is a compact **Railway Field Workshop**: dark ink plates, warm paper labels,
signal yellow for decisions, mint for confirmed states, and coral only for blocking states.
Gameplay remains visible through the center 45% of the screen.

## Gameplay HUD

| Area | Content | Behaviour |
| --- | --- | --- |
| Top left | Current station + next route | Two lines, route context only |
| Top right | Banked bolts | Updates from the shared shop economy |
| Bottom left | Current objective | One action sentence; no quest log |
| Bottom right | Carried item + contextual hint | Hidden complexity; shows only the current tool/item |
| Bottom center | Interaction prompt | Appears only when an action is available |
| Upper center | Short status banner | Temporary success/block feedback |

The HUD uses independent optional modules. Gameplay code may update one field without knowing
about the canvas or the other fields. Missing data falls back to neutral test copy.

## Physical station shop HUD

The main design authority is `Assets/00.main/PHYSICAL_STATION_SHOP_DESIGN.md`. The station shop
does not open a purchase screen and has no buy buttons. It reuses the gameplay HUD while the
player walks around the station and handles products directly.

1. Top right keeps the shared banked bolt count visible.
2. Bottom center shows the aimed action: pick up, mount, detach, deposit, or depart.
3. Bottom right shows the currently carried product and the next useful action.
4. Bottom left changes to the station objective, such as `MOUNT UPGRADES OR DEPART`.
5. Upper-center status is used only for short confirmation or rejection feedback.
6. Departure interaction includes pending cost and available bolts through the existing checkout
   prompt; insufficient currency never opens a blocking modal.

The legacy `RailgameShopScreen` is not placed in campaign scenes and remains untouched for old
prototype compatibility. Product ownership, pending totals, and checkout stay in the physical
shop systems.

## Visual tokens

- Ink: `#182231`
- Paper: `#F3E5BC`
- Signal: `#F3C94B`
- Success: `#7AD9A3`
- Danger: `#EF645B`
- Dimmer: `#09111CCC`

Reference resolution is 1920×1080 with 5% safe margins. The same hierarchy must remain readable
at 1280×720; primary interaction targets are at least 64×64.

## Test connection

`RailgameHudRuntimeBridge` now projects campaign state, carried physical-shop items, and checkout
pending cost onto these display setters. It is a read-only polling adapter: it makes no scene-flow
decisions, does not pause gameplay, and does not require a train/enemy dependency. The generated
`PF_CasualGameplayUI` prefab includes the adapter and keeps the interaction/status plates hidden
until content exists.
