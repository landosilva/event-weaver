using System;
using UnityEngine;

namespace Lando.EventWeaver.Editor
{
    [Serializable]
    public class EventWeaverLogSettings
    {
        [field: SerializeField] public bool Message { get; private set; }
        [field: SerializeField] public bool Warning { get; private set; } = true;
        [field: SerializeField] public bool Error { get; private set; } = true;
    }
}