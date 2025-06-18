using System;

namespace Lando.EventWeaver.Events
{
    public record OnEventUnregistered(object Listener, Type EventType) : IEvent
    {
        public object Listener { get; } = Listener;
        public Type EventType { get; } = EventType;
    }
}