namespace Lando.EventWeaver.Editor.History;
public readonly record struct EventLog(string Message, EventLogType Type, DateTime Timestamp);