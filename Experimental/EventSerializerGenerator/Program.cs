using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    using System.Reflection;

    // CodeDom is complicated. I use a simpler method. It is Console.WriteLine.
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
                return 1;
            string eventTemplatePath = args[0];
            Assembly template = Assembly.LoadFile(eventTemplatePath);
            Console.WriteLine(@"
using System;
using System.Collections.Generic;
using System.Text;

namespace ");
            foreach (Type t in template.GetTypes())
            {
                Console.WriteLine(t.FullName);
            }
            return 0;
        }
    }
}
