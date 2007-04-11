using System;
using System.Collections.Generic;

namespace ProtocolGenerator.Helpers
{
    internal static class BitstreamSerializationHelper
    {
        static BitstreamSerializationHelper()
        {
            supportingPrimitives = new List<Type>();
            supportingPrimitives.Add(typeof (bool));
            supportingPrimitives.Add(typeof (byte));
            supportingPrimitives.Add(typeof (double));
            supportingPrimitives.Add(typeof (float));
            supportingPrimitives.Add(typeof (int));
            supportingPrimitives.Add(typeof (sbyte));
            supportingPrimitives.Add(typeof (short));
            supportingPrimitives.Add(typeof (string));
            supportingPrimitives.Add(typeof (uint));
            supportingPrimitives.Add(typeof (ushort));
        }

        public static bool DoesSupportPrimitiveType(Type t)
        {
            return supportingPrimitives.Contains(t);
        }

        private static IList<Type> supportingPrimitives;
    }
}