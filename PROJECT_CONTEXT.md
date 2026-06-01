# Dice King Project Context

This file records stable design decisions and implementation notes for future conversations in this Unity project.

For complete gameplay flow, read `GAME_FLOW.md` before making design or implementation changes.
For confirmed dice archetype designs, read `DICE_ARCHETYPES.md`.

## Product Direction

- Working title: `骰子王`.
- Genre target: roguelite score-builder built around dice rolls, inspired by the score-engine feel of `Balatro`, but using readable dice types, long-term dice state, and board-like encounter rules.
- Tone: light, witty, and relaxed, but not childish. Prefer dry humor, tabletop tavern, odd royal bureaucracy, ledgers, seals, dice cups, and small-citizen absurdity. Avoid overly cute or preschool wording.
- Core fantasy: the player is not simply gambling. They are building a dice engine that turns randomness into a controlled scoring system.

## Core Loop

The current end-to-end flow is tracked in `GAME_FLOW.md`. Keep this section as the high-level roguelite scoring loop only.

1. Enter a small encounter with a target score.
2. Roll the current six-dice set for the encounter.
3. Roll, apply dice type effects, temporary dice effects, hand scoring, encounter rules, and any later modifiers.
4. If the score reaches the target, resolve the stage reward formula and enter the inter-stage dice market.
5. Inter-stage market events mainly let the player buy dice, sell dice, or skip.
6. Continue through a chapter of small encounters, a boss encounter, and the chapter shop.
7. Any small-stage failure ends the run immediately and clears run progress.

## Confirmed Demo Scope

- A complete first-chapter demo should exist in `Assets/Scripts/DiceKingDemo.cs`.
- The demo self-bootstraps at scene load via `RuntimeInitializeOnLoadMethod`, so `Assets/Scenes/SampleScene.unity` can be opened and played without manually wiring a component.
- The current prototype covers the confirmed flow in `GAME_FLOW.md`: main menu, settings placeholder, first-entry opening, default starter dice bag, small-stage loop, result decision, one cheat edit per stage, inter-stage events, boss, and chapter shop.
- Prototype continue/save uses `PlayerPrefs` and restores to the start of the saved stage with current gold, dice collection, die faces, dice type/state, growth counters, settings, and opening-seen state.
- Current generated art assets live under `Assets/Resources/Art/`:
  - `table_background.png`: full-screen tabletop UI background.
  - `dice_cup.png`: transparent dice cup prop used during rolling.
- Current chapter structure:
  - 6 normal encounters.
  - 1 boss encounter: `Boss 双面裁判`.
- Current start:
  - Formal design should start from one default dice bag based on `DICE_ARCHETYPES.md`.
  - The old material/trait starter routes are legacy prototype behavior and should be replaced during the next implementation pass.

## Design Pillars

- Dice should be readable by `type` first:
  - Faces: raw probability and base values.
  - Type: the main mechanical identity, such as piggy, turtle, double, odd, even, basic, tree, and gambler.
  - State: long-term counters or round configuration, such as piggy target face, tree growth, tree hit face, and gambler threshold.
- Dice types should create different play patterns without needing a large material/trait matrix in the first implementation pass.
- Rewards should support both committed routes and cross-route experimentation.
- Bosses should be rule distortions, not just larger target numbers.
- Dice archetypes are tracked in `DICE_ARCHETYPES.md`. Current confirmed archetype directions include:
  - Piggy economy: hit a random target face to generate wallet value for the dice market.
  - Turtle iteration: generate shrinking temporary dice for extra single-die score.
  - Odd/even control: shape the six-dice hand toward parity and repeated-value outcomes.
  - Tree growth: reward random hit-face success by increasing the tree die's own faces.
  - Double/gambler burst: provide direct high-score choices with clear expected value boundaries.

## Current Dice Types

- Basic: no special effect; cheap market filler and replacement target.
- Piggy: each roll gets a random target face; hitting it earns gold.
- Turtle: generates a temporary smaller die with half the maximum point value, iterating until it naturally stops.
- Double: doubles its own base die score.
- Odd: rolls only odd values.
- Even: rolls only even values.
- Tree: each roll gets a random hit face; hitting it permanently raises its own faces by 1, and values above 6 still participate in hand recognition.
- Gambler: each roll gets a random threshold from 1 to 5; values below it score zero, equal values score normally, and values above it use an expectation-preserving multiplier.

## Scoring Notes

- Future scoring implementation baseline:
  - Roll score uses `round(sum of six individual die scores * final hand multiplier)`.
  - The six dice are evaluated as one poker-like hand.
  - Each settlement takes only one exclusive main hand multiplier by priority.
  - All odd / all even are side hands; they use `max(main hand multiplier, parity multiplier)` instead of multiplying with the main hand.
  - The first-chapter fair-dice baseline average multiplier is about `x1.366`.
  - The full multiplier table and probability notes are tracked in `GAME_FLOW.md`.
- Individual die scoring should combine:
  - Individual die value.
  - Dice type effects.
  - Temporary small dice effects.
  - Long-term dice state such as tree growth or piggy counters.
  - Encounter rule modifiers.
  - Later relic or modifier systems if reintroduced.
- Useful combo families:
  - Pair.
  - Three/four of a kind.
  - Full-house-like patterns.
  - Four-run and five-run straights.
  - All odd / all even.
  - All unique.
- The fun target is for players to understand basics in 5 minutes, form a route in 15 minutes, and hit a memorable score spike in 25 minutes.

## UX Notes

- The first playable screen should be the actual game experience, not a marketing page.
- UI should feel like a compact tabletop tool: restrained, readable, dense enough for repeated decisions.
- Avoid in-game tutorial walls. Short actionable labels are better than explaining every system in paragraphs.
- Keep the current demo no-asset and self-contained unless a future task explicitly adds production UI/art.
- The start-of-run build selection screen has been removed. New game enters a single default starter dice bag.
- Confirmed roll interaction follows `GAME_FLOW.md`: rolling is not triggered by a clickable button. The player presses `Space` to start the shake phase, then repeatedly taps `Space` to delay the shake timer up to a cap. When the timer expires, the cup enters a short rapid stop/falloff phase before locking the result.
- After results are locked, the result decision state allows direct settlement with `Space` and one cheat edit. Scoring then animates left to right: each die briefly scales up and shows a quick floating `+score` text before hand multiplier score lands.

## Art Direction Notes

- Use the project-specific Codex skill `$wabish-art-assets` for future generated art assets. A normal request can be just an asset type plus keywords.
- Stable visual direction: `Bright Ledger Boardgame` / `明亮账本桌游风`, a bright 2D flat board-game illustration style with clean rounded shapes, soft cel shading, subtle paper texture, and playful royal-bureaucracy props.
- Preferred motifs: dice, ledgers, wax seals, official stamps, small crowns, coin pouches, ribbons, stamped forms, desk props, and tidy tabletop game pieces.
- Tone should be bright, cute, witty, and relaxed, but not childish. Avoid chibi mascots, baby-face expressions, sticker-sheet looks, plush-toy rendering, casino glam, photorealistic 3D, neon sci-fi, and preschool styling.
- Production assets should favor chunky readable silhouettes, light clear colors, controlled dark outlines, minimal texture, visible material cues, and minimal/no generated text so Unity UI can own final copy.

## Near-Term Optimization Points

- Replace `OnGUI` prototype UI with proper Unity UI Toolkit or uGUI once mechanics stabilize.
- Split `DiceKingDemo.cs` into separate runtime modules:
  - Data definitions.
  - Run state.
  - Scoring engine.
  - Reward generation.
  - UI presentation.
- Add deterministic seed support for reproducible balancing.
- Add automated scoring tests outside scene UI.
- Replace the old reward generation with a dice market:
  - Generate three buy offers.
  - Support unlimited buy/sell operations while keeping dice capacity fixed at six.
  - Disable purchases while full and block leaving the market below six dice.
  - Keep prices and sell values non-exploitable.
- Add encounter preview text that highlights why a route may care about the rule.
- Tune chapter targets after playtesting the default starter dice bag across the new dice type routes.
- Track per-run stats: highest score, best combo, total gold earned, and route-defining dice.

## Implementation Cautions

- This is currently a prototype, not a production architecture.
- Keep future changes scoped: avoid introducing a large framework before the scoring and reward loop are validated.
- When adding mechanics, prefer adding a small number of readable route interactions over many isolated effects.
- If a change affects scoring, update this file with the intent and add or update tests when a test harness exists.
- If a change affects a dice archetype, update `DICE_ARCHETYPES.md` with the route intent, settlement boundary, and anti-abuse rule.
- The current code still contains legacy material/trait prototype mechanics. The next implementation pass should migrate those mechanics to the type-based dice model rather than layering the new rules on top of the old matrix.
