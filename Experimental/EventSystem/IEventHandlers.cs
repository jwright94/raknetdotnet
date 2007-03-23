using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    interface IEventHandlers
    {
        void CallHandler(ISimpleEvent e);
    }
}
