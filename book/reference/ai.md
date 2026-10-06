---
description: "Nez's built-in StateMachine<T> and a map of the rest of the AI namespace."
---

# AI: state machines

The book hand-rolls enum state machines (chapter 6's dash, chapter 12's
Patrol/Chase). Nez ships a class-based version in `Nez.AI.FSM` — worth
knowing when states grow enter/exit logic.

## `Nez.AI.FSM.StateMachine<T>`

Generic over a **context** — usually the component that owns it:

```csharp
_fsm = new StateMachine<Enemy>(this, new PatrolState());
_fsm.AddState(new ChaseState());
// per frame:
_fsm.Update(Time.DeltaTime);
```

Each state extends `State<T>` and gets the full lifecycle:

| Member | Notes |
|---|---|
| `State<T>.Begin()` | Enter. Timers reset, particles fire, sounds play here instead of at transition sites. |
| `State<T>.End()` | Exit. Cleanup the state started. |
| `State<T>.Reason()` | **Called before `Update` every frame** — the state's "last chance to change state." This is chapter 12's *think* phase, formalized. |
| `State<T>.Update(float dt)` | The per-frame *act*. Abstract — every state implements it. |
| `ChangeState<TState>()` | Transition. Guards against re-entering the same state; calls `End`/`Begin`; fires `OnStateChanged`. |
| `ElapsedTimeInState` | Free per-state timer — the book's `_stateTimer`, built in. |
| `PreviousState` / `ChangeToPreviousState()` | "Go back to what I was doing" (stagger → resume patrol). |
| `OnStateChanged` (event) | Hook AI debugging/logging to transitions, not states. |

`SimpleStateMachine` (same namespace) is the lighter variant for when the
full class-per-state machinery is overkill.

```csharp
// states are classes; the context (Enemy) is typed, not cast
public class ChaseState : State<Enemy>
{
    public override void Begin()
    {
        _context.Speed = 90f;          // setup lives here, not at transition sites
    }

    public override void Reason()
    {
        // last chance to change state, every frame, before Update
        if (!_context.CanSeePlayer())
            _context.Fsm.ChangeState<PatrolState>();
    }

    public override void Update(float deltaTime)
    {
        _context.MoveTowardPlayer(deltaTime);

        // free per-state timer — the book's _stateTimer, built in
        if (_context.Fsm.ElapsedTimeInState > 5f)
            _context.Fsm.ChangeState<PatrolState>(); // gave up
    }

    public override void End() { }
}
```

**When to switch from the book's enum:** at three states, or the first
time a state needs real setup/teardown. Two states with no enter/exit
logic — the book's enum is fine, and simpler.

## The rest of `Nez.AI`

| Namespace | What it is | Reach for it when… |
|---|---|---|
| `Nez.AI.FSM` | This page. | States have enter/exit logic. |
| `Nez.AI.BehaviorTree` | Behavior trees (selectors, sequences, decorators). | AI needs prioritized, interruptible behaviors — "flee if hurt, else chase, else patrol." |
| `Nez.AI.GOAP` | Goal-oriented action planning. | Agents pick multi-step plans from goals ("get weapon, then hunt"). |
| `Nez.AI.UtilityAI` | Utility scoring. | Behaviors need soft blending rather than hard switches. |
| `Nez.AI.Pathfinding` | A\* on grids. | Enemies navigate mazes, not open arenas. (There's a dedicated `Pathfinding` sample scene.) |

For the book's arena game, the enum (chapters 6/12) or `StateMachine<T>`
covers everything. The rest is here when your *next* game needs it.
