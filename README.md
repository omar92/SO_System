# SO_System

A comprehensive **Scriptable Object-based architecture framework** for Unity that promotes loose coupling, data-driven design, and event-driven communication in your games.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Variable System](#variable-system)
  - [Creating Variables](#creating-variables)
  - [Using Variables in Scripts](#using-variables-in-scripts)
  - [Numeric Variable Operations](#numeric-variable-operations)
  - [UI Binding](#ui-binding)
  - [Variable Caching](#variable-caching)
  - [Custom Enum Variables](#custom-enum-variables)
- [Event System](#event-system)
  - [Creating Events](#creating-events)
  - [Raising Events](#raising-events)
  - [Listening to Events](#listening-to-events)
- [Helper Tools](#helper-tools)
  - [Invoke Helpers](#invoke-helpers)
  - [Coroutine Utilities](#coroutine-utilities)
  - [UI Utilities](#ui-utilities)
  - [Debug Utilities](#debug-utilities)
- [Editor Tools](#editor-tools)
  - [Event Manager Window](#event-manager-window)
  - [Custom Property Drawers](#custom-property-drawers)
  - [Settings](#settings)
- [API Reference](#api-reference)
- [Configuration](#configuration)

---

## Overview

SO_System is a Unity framework built around ScriptableObjects that helps you create maintainable, scalable game architectures by:

- **Decoupling** systems through ScriptableObject-based events and variables
- **Centralizing** shared data in assets rather than singletons
- **Simplifying** event-driven communication between GameObjects and systems
- **Accelerating** development with ready-to-use helper components

---

## Features

- 🗂 **Type-safe SO Variables** — Bool, Int, Float, String, Vector2, Vector3, GameObject, and custom Enum types
- 📢 **SO Event System** — Raise and listen to events without direct references between objects
- 🔗 **UI Binding** — Bind SO variables directly to Text, Slider, Toggle, InputField, and TextMeshPro components
- ⏱ **Invoke Helpers** — Components for delayed, conditional, and lifecycle-triggered invocations
- 🛠 **Editor Tools** — Event Manager Window and custom property drawers for rapid development
- 🎨 **Custom Icons** — Distinctive icons for each SO type in the Project view
- 💾 **Optional Caching** — Persist variable values across play sessions with PlayerPrefs

---

## Installation

1. **Download or clone** this repository:
   ```bash
   git clone https://github.com/omar92/SO_System.git
   ```

2. **Copy the folder** into your Unity project's `Assets` directory:
   ```
   YourUnityProject/
   └── Assets/
       └── SO_System/   ← place the contents here
   ```

3. **Open your Unity project.** The framework will compile automatically.

> **Requirements:** Unity 2018.4 or later. TextMeshPro is optional (needed only for TMPro UI binding features).

---

## Getting Started

### Quick Setup

1. Create a new ScriptableObject variable via `Assets → Create → SO → Variables → Integer` (or any type you need).
2. Create a new ScriptableObject event via `Assets → Create → SO → Event`.
3. Add an `EventListenerSO` component to a GameObject in your scene.
4. Assign the event asset to the listener and wire up the response using the Unity Events inspector.
5. Raise the event from a script or component — the listener will respond.

---

## Variable System

### Creating Variables

Right-click in the **Project window** and navigate to `Create → SO → Variables` to create a variable asset:

| Type | Menu Path |
|------|-----------|
| Bool | `SO/Variables/Bool` |
| Integer | `SO/Variables/Integer` |
| Float | `SO/Variables/Float` |
| String | `SO/Variables/String` |
| Vector2 | `SO/Variables/Vector2` |
| Vector3 | `SO/Variables/Vector3` |
| GameObject | `SO/Variables/GameObject` |
| Enum | (See [Custom Enum Variables](#custom-enum-variables)) |

### Using Variables in Scripts

Reference SO variable assets directly in your MonoBehaviour fields:

```csharp
using SO;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private IntSO health;

    void Start()
    {
        // Get the current value
        int currentHealth = health.GetValue();

        // Set a new value
        health.SetValue(100);

        // Implicit conversion (read-only shorthand)
        int hp = health; // equivalent to health.GetValue()
    }
}
```

### Subscribing to Value Changes

```csharp
void OnEnable()
{
    health.Subscripe(OnHealthChanged);
}

void OnDisable()
{
    health.UnSubscripe(OnHealthChanged);
}

void OnHealthChanged(object sender, System.EventArgs e)
{
    Debug.Log("Health changed to: " + health.GetValue());
}
```

### Numeric Variable Operations

`IntSO` and `FloatSO` support arithmetic operations:

```csharp
[SerializeField] private IntSO score;

public void AddPoints(int points)
{
    score.Add(points);      // score += points
    score.Sub(points);      // score -= points
    score.Mull(points);     // score *= points
    score.Divide(points);   // score /= points
}
```

### UI Binding

Bind a variable's value to UI components directly from code:

```csharp
using UnityEngine.UI;
using TMPro;

[SerializeField] private FloatSO playerSpeed;
[SerializeField] private Slider speedSlider;
[SerializeField] private TMP_Text speedLabel;

void Start()
{
    playerSpeed.CopyToSlider(speedSlider);
    playerSpeed.CopyToTMP_Text(speedLabel);
}
```

You can also use the **`SetUiText`** component in the Inspector to bind one or more variables to a formatted text string without writing any code.

### Variable Caching

Enable **`allowCache`** on a variable asset in the Inspector to automatically persist its value in PlayerPrefs across play sessions. The cache key is `SOV{VariableName}`.

To reset a variable to its default value:

```csharp
health.ResetValue();
```

### Custom Enum Variables

Subclass `SOEnumVariable<T>` to create a ScriptableObject variable for your own enum types:

```csharp
using SO;

[CreateAssetMenu(menuName = "SO/Variables/GameState")]
public class GameStateSO : SOEnumVariable<GameState> { }

public enum GameState { MainMenu, Playing, Paused, GameOver }
```

---

## Event System

### Creating Events

Right-click in the **Project window** and select `Create → SO → Event` to create an event asset.

### Raising Events

Raise an event from any script that holds a reference to the asset:

```csharp
using SO.Events;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private EventSO onEnemyDied;

    void Die()
    {
        onEnemyDied.Raise();                    // Raise with no value
        onEnemyDied.Raise(gameObject);          // Raise with a value (object)
        onEnemyDied.RaiseValue(10);             // Raise with an int
        onEnemyDied.RaiseValue(3.5f);           // Raise with a float
        onEnemyDied.RaiseValue(true);           // Raise with a bool
        onEnemyDied.RaiseValue("EnemyDied");    // Raise with a string
    }
}
```

### Listening to Events

#### Via Inspector (No Code)

1. Add an **`EventListenerSO`** component to any GameObject.
2. In the Inspector, assign the `EventSO` asset to the **Event** field.
3. Add response callbacks under **On Event Raised**.

#### Via Code

```csharp
using SO.Events;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private EventSO onEnemyDied;

    void OnEnable()
    {
        onEnemyDied.RegisterListener(GetComponent<EventListenerSO>(), HandleEnemyDied);
    }

    void OnDisable()
    {
        onEnemyDied.UnregisterListener(GetComponent<EventListenerSO>());
    }

    void HandleEnemyDied(object value)
    {
        Debug.Log("Enemy died, received: " + value);
    }
}
```

---

## Helper Tools

### Invoke Helpers

Drag-and-drop components for common Unity invocation patterns — no code required:

| Component | Behavior |
|-----------|----------|
| `InvokeOnAwake` | Fires a UnityEvent during `Awake()` |
| `InvokeOnStart` | Fires a UnityEvent during `Start()` |
| `InvokeOnEnable` | Fires a UnityEvent during `OnEnable()` |
| `InvokeOnDisable` | Fires a UnityEvent during `OnDisable()` |
| `InvokeAfterSeconds` | Fires a UnityEvent after a specified delay |
| `InvokeAfterRandomSeconds` | Fires a UnityEvent after a random delay within a range |
| `InvokeOnValueChange` | Fires a UnityEvent whenever a referenced `VariableSO` changes |
| `InvokeIf` | Fires a UnityEvent only if a `BoolSO` is `true` |
| `CompareIntegerSOVariables` | Compares two `IntSO` values with a chosen operator |

### Coroutine Utilities

The static `Z` class provides coroutine helpers that work without requiring a MonoBehaviour:

```csharp
using SO.tools;

// Invoke an action after a delay
Z.InvokeAfterDelay(2f, () => Debug.Log("2 seconds later"));

// Invoke at the end of the current frame
Z.InvokeEndOfFrame(() => Debug.Log("End of frame"));

// Invoke repeatedly while a condition is true
Z.InvokeWhile(() => isAlive, () => Debug.Log("Still alive"));

// Invoke when a condition becomes true
Z.InvokeWhen(() => isReady, () => Debug.Log("Ready!"));
```

`CoRef` is a persistent singleton MonoBehaviour (survives scene loads) that `Z` uses internally to run coroutines.

### UI Utilities

| Component | Description |
|-----------|-------------|
| `SetUiText` | Bind one or more `VariableSO` values to a formatted Text/TMP string |
| `ConsoleToGUI` | Render Unity console logs on-screen as an in-game overlay |

### Debug Utilities

`Debuger` wraps Unity's `Debug.Log` and is automatically **disabled in Release builds**, so you can leave debug calls in your code without affecting shipped builds:

```csharp
using SO.tools;

Debuger.Log("This only appears in development builds");
```

---

## Editor Tools

### Event Manager Window

Open via **Window → SOEventManager** to see a live view of all `EventSO` assets referenced in the active scene, along with their registered listeners. This helps you quickly identify unconnected events or missing listeners.

### Custom Property Drawers

All `VariableSO` and `EventSO` fields in the Inspector display enhanced drawers that show:
- The **current value** of the variable
- An **Assign** button to quickly create and assign a new asset
- A **Reset** button to restore the default value
- Inline value editing (no need to ping the asset)

### Settings

Open the settings panel via **Edit → Project Settings → SO System** (or from the EventManagerWindow). The following options are available:

| Setting | Description |
|---------|-------------|
| `ShowAssignButton` | Show/hide the Assign button in property drawers |
| `SOCreatePath` | Default folder for new SO assets |
| `EventSOCreatePath` | Default folder for new Event assets |
| `VarSOCreatePath` | Default folder for new Variable assets |
| `ShowEventDescription` | Display the event description field in the inspector |
| `EventSOListenerDefaultView` | Expand listeners by default in the event inspector |
| `AllowEditListenersFromEvents` | Allow editing listener responses from the event inspector |
| `ShowVarSOValue` | Display the current value in the variable inspector |

Settings are persisted in PlayerPrefs under the key `SO_SYS_Settings`.

---

## API Reference

### `VariableSO<T>`

```csharp
// Value access
T       GetValue()
void    SetValue(T newValue, bool forceUpdate = false, bool log = false)
void    SetValue(VariableSO<T> variableSO)
void    SetValue(string value)
void    ResetValue()
T       GetDefaultValue()

// Change notifications
void    Subscripe(EventHandler onValChanged)
void    UnSubscripe(EventHandler onValChanged)
void    UnSubscripeAll()

// UI binding
void    CopyToText(Text textComponent)
void    CopyToTMP_Text(TMP_Text textComponent)
void    CopyToInputField(InputField inputField)
void    CopyToSlider(Slider slider)
void    CopyToScrollbar(Scrollbar scrollbar)
```

### `SONumericVariable<T>` (extends `VariableSO<T>`)

```csharp
void    Add(T num)
void    Sub(T num)
void    Mull(T num)
void    Divide(T num)
```

### `EventSO`

```csharp
// Raising
void    Raise()
void    Raise(object value)
void    Raise(object value, Action preRaise, Action postRaise)
void    RaiseImmediately(object value)
void    RaiseValue(int value)
void    RaiseValue(float value)
void    RaiseValue(bool value)
void    RaiseValue(string value)

// Listener management
void    RegisterListener(EventListenerSO listener, ObjectEvent callback)
void    UnregisterListener(EventListenerSO listener)
```

---

## Configuration

All configuration is done through Unity's Inspector on the relevant assets:

- **Variable assets**: Set the default value, toggle `allowCache`, choose reset behavior.
- **Event assets**: Write a description, toggle debug logging.
- **`EventListenerSO` component**: Assign the event and response callbacks in the Inspector.
- **SO_SystemSettings**: Accessible from the settings menu; controls Editor UI appearance.

---

## License

This project does not currently specify a license. Please contact the repository owner before using it in commercial projects.
