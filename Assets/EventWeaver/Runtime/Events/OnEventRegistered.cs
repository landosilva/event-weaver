namespace Lando.EventWeaver.Events;
public readonly record struct OnEventRegistered(object Listener, Type EventType) : IEvent;