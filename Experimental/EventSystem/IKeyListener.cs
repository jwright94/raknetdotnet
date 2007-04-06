using System;

namespace EventSystem
{
    [Obsolete]
    internal interface IKeyListener
    {
        bool KeyPressed(char key);
    }
}