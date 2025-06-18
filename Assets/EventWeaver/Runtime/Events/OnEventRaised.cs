namespace Lando.EventWeaver.Events
{
    public record OnEventRaised(IEvent Event) : IEvent
    {
        public IEvent Event { get; } = Event;
    }
}