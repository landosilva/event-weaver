namespace Lando.EventWeaver.Events;
public readonly record struct OnEventUnregistered(object Listener, Type EventType) : IEvent;