using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class EventArgs<T> : EventArgs
    {
        public T Args { get; set; }

        public EventArgs( T args)
        {
            this.Args = args;
        }
    }
}
