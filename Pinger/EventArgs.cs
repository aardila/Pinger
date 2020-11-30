using System;

namespace Pinger
{
    public class EventArgs<T> : EventArgs
    {
        public T Payload { get; private set; }

        public EventArgs(T payload)
        {
            this.Payload = payload;
        }   
    }
}
