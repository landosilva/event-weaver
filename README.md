# Event Weaver 🪡

## 📝 Summary

Event Weaver is a simplified event-driven architecture that automatically injects listener registration and unregistration at build time. Navigate the sections below to learn more:

- [Weaver Integration](#-weaver-integration)
- [Tooling](#-tooling)
- [Installation](#-installation)
- [Example Usage](#-example-usage)

---

## ⚙️ Weaver Integration

The build-time Weaver (via Mono.Cecil) handles all listener wiring:

1. **Detection**  
   Scans compiled assemblies for types implementing `IEventListener<T>`.
2. **Injection for MonoBehaviours**
   - Inserts `EventRegistry.Register<T>(this)` call in `OnEnable`.
   - Inserts `EventRegistry.Unregister<T>(this)` call in `OnDisable`.
3. **Injection for Plain Types**
   - Inserts `EventRegistry.Register<T>(this)` call in `Constructor`.
   - Inserts `EventRegistry.Unregister<T>(this)` call in `Finalizer`.

---

## 🛠️ Tooling

Accessed via **Tools > Event Weaver**, this tool offers two complementary views for debugging events in your Unity project.

<table>
  <tr>
    <td><strong>Event Viewer</strong><br/><br/>
      Displays a hierarchical view of all current event subscriptions, grouped by type and source object. Useful for quickly inspecting which objects are listening to which events.
    </td>
    <td><strong>Event History</strong><br/><br/>
      Shows a chronological log of event activity — including registration, invocation, and unregistration — with timestamps and source objects. Ideal for debugging the flow of events during runtime.
    </td>
  </tr>
  <tr>
    <td><img src="https://github.com/user-attachments/assets/7bb38800-51aa-42e3-9083-5f1b73df6bfe" width="350"/></td>
    <td><img src="https://github.com/user-attachments/assets/e7e835b7-626f-4ba0-a71d-13072f3e614a" width="350"/></td>
  </tr>
</table>

---

## 📦 Installation

Install via Unity Package Manager using Git URL:

1. Open **Window > Package Manager**.
2. Click **+** and select **Add package from Git URL...**
3. Paste:
   ```
   https://github.com/landosilva/event-weaver.git?path=/Assets/EventWeaver
   ```  
4. Click **Add**.

---

## ⚡ Declaring and Using Events

You can define events using a variety of C# types, depending on your needs.

### ✅ Basic Struct Event

```csharp
// Using a struct to declare a simple event
public struct OnPlayerScored(int Score) : IEvent
{
    public int Score { get; } = Score;
}
```

This is a lightweight and allocation-free way to define events, ideal for high-frequency use cases where performance is key.

### 🔒 With Immutability and Value Semantics

```csharp
// Using a readonly record struct (C# 10+)
public readonly record struct OnPlayerScored(int Score) : IEvent;
```

> **Why use `readonly record struct`?**  
> - Value type (stack-allocated, no GC pressure)  
> - Immutable by default  
> - Built-in value equality and `ToString()`  
> - Concise syntax with positional parameters  

This is often the **best option** for events: it's safe, fast, and expressive.

### 🧠 When You Need Class Semantics

```csharp
public class OnPlayerScored : IEvent
{
    public int Score { get; }

    public OnPlayerScored(int score) => Score = score;
}
```

Use a class if:
- You need reference semantics (e.g., shared state)
- You want to inherit from a base class
- You need mutable data

---

### 👂 Listening to Events

Implement `IEventListener<T>` on any `class` to respond to events. No manual registration is required — listeners are wired up automatically.

```csharp
public class ScoreDisplay : MonoBehaviour, IEventListener<OnPlayerScored>
{
    public void OnListenedTo(OnPlayerScored e)
    {
        Debug.Log($"Player scored {e.Score} points!");
    }
}
```

---

### 🚀 Raising Events

There are multiple ways to raise events:

```csharp
// Instantiate and raise
OnPlayerScored onPlayerScored = new (Score: 10);
EventRegistry.Raise(onPlayerScored);

// Or raise inline
EventRegistry.Raise(new OnPlayerScored(Score: 10));

// Or use an extension method
new OnPlayerScored(10).Raise();
```

---

*❤️ Thank you for using Event Weaver!*
