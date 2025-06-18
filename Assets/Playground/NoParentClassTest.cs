using Lando.EventWeaver;
using UnityEngine;

namespace Playground
{
    public class NoParentClassTest : IEventListener<OnTestEvent>
    {
        public void OnListenedTo(OnTestEvent e)
        {
            Debug.Log("OnListenedTo OnTestEvent: " + e.Message);
        }
    }
}