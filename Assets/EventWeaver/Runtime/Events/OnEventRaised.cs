namespace Lando.EventWeaver.Events;
public readonly record struct OnEventRaised(IEvent Event) : IEvent;