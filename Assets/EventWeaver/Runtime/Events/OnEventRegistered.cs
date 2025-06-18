using System;

namespace Lando.EventWeaver.Events
{
    public record OnEventRegistered(object Listener, Type EventType) : IEvent
    {
        public object Listener { get; } = Listener;
        public Type EventType { get; } = EventType;
    }
}