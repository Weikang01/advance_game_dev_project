using System.Collections.Generic;

public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate(string name, object udata);
    private Dictionary<string, EventDelegate> _eventTable = new Dictionary<string, EventDelegate>();

    public void Init()
    {
        _eventTable.Clear();
    }

    public void AddEventListener(string name, EventDelegate handler)
    {
        if (_eventTable.ContainsKey(name))
        {
            _eventTable[name] += handler;
        }
        else
        {
            _eventTable[name] = handler;
        }
    }

    public void RemoveEventListener(string name, EventDelegate handler)
    {
        if (_eventTable.ContainsKey(name))
        {
            _eventTable[name] -= handler;
            if (_eventTable[name] == null)
            {
                _eventTable.Remove(name);
            }
        }
    }

    public void DispatchEvent(string name, object udata)
    {
        if (_eventTable.ContainsKey(name))
        {
            _eventTable[name](name, udata);
        }
    }
}
