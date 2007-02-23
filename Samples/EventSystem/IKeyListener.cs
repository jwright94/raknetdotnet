using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    interface IKeyListener
    {
        bool KeyPressed(char key);
    }
}
