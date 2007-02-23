using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using Castle.MicroKernel;

    sealed class UnifiedNetwork
    {
        public void Test()
        {
            IKernel kernel = new DefaultKernel();
            kernel.AddComponentInstance("UN", this);
        }
    }
}
