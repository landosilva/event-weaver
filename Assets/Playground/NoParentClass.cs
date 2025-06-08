using Lando.EventWeaver;
using UnityEngine;

namespace Playground
{
    public class NoParentClass : IEventListener<OnTestEvent>
    {
        public void OnListenedTo(OnTestEvent e)
        {
            Debug.Log("Eita : " + e.Message);
        }
    }
}