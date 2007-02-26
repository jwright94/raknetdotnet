using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;

    sealed class UnifiedNetwork : IDisposable
    {
        #region Ogre-like singleton implementation.
        static UnifiedNetwork instance;
        public UnifiedNetwork()
        {
            Debug.Assert(instance == null);
            instance = this;
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;
        }
        public static UnifiedNetwork Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion
    }
}
