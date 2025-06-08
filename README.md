# Event Weaver 🪡

<div align="center">
  <img src="https://github.com/user-attachments/assets/33facd18-779a-4a27-ac47-111902647f35" width="160" />
</div>

## 📝 Summary

Event Weaver is a Unity event bus system that simplifies event-driven architecture by automatically injecting listener registration and unregistration at build time. Navigate the sections below to learn more:

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

## 🛠 ️Tooling

> **Event History**  
> _Placeholder for Event History window screenshot_

> **Event Viewer**  
> _Placeholder for Event Viewer window screenshot_

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

## ✅️ Example Usage

```csharp

// Creating an Event
public record OnPlayerScored(int Score) : IEvent
{
    public int Score { get; } = Score;
}

// Listening to an Event
// Listeners are wired automatically — no manual registration calls needed
public class ScoreDisplay : MonoBehaviour, IEventListener<OnPlayerScored>
{
    public void OnListenedTo(PlayerScored e)
    {
        Debug.Log($"Player scored {e.Score} points!");
    }
}

// Raising events.
new OnPlayerScored(10).Raise();

// alternatively
OnPlayerScored onPlayerScored = new (10);
EventRegistry.Raise(onPlayerScored);
// or
EventRegistry.Raise(new OnPlayerScored(10));
```
💡 **Best practice**: Use `record` unless you have a specific performance/memory reason to prefer `struct`, or a polymorphic design that benefits from `class`.

---

*❤️ Thank you for using Event Weaver!*
