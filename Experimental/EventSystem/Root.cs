using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    sealed class Root
    {
        #region Implementing popular pattern of singleton
        static readonly Root instance = new Root();
        Root() { }
        public static Root Instance { get { return instance; } }
        #endregion
        public void AddFrameListener(IFrameListener newListener)
        {
            frameListeners.Add(newListener);
        }
        public void RemoteFrameListener(IFrameListener oldListener)
        {
            frameListeners.Remove(oldListener);
        }
        public void AddKeyListener(IKeyListener newListener)
        {
            keyListeners.Add(newListener);
        }
        public void RemoveKeyListener(IKeyListener oldListener)
        {
            keyListeners.Remove(oldListener);
        }
        public void StartRendering()
        {
            while (true)
            {
                if (_kbhit() != 0)
                {
                    char key = Console.ReadKey(true).KeyChar;
                    foreach (IKeyListener keyListener in keyListeners)
                    {
                        keyListener.KeyPressed(key);
                    }
                }
                foreach (IFrameListener frameListener in frameListeners)
                {
                    if (!frameListener.FrameStarted())
                        return;
                }
                System.Threading.Thread.Sleep(1);
            }
        }
        ICollection<IFrameListener> frameListeners = new List<IFrameListener>();
        ICollection<IKeyListener> keyListeners = new List<IKeyListener>();

        [System.Runtime.InteropServices.DllImport("crtdll.dll")]
        public static extern int _kbhit();  // I do not want to use this.
    }
}
