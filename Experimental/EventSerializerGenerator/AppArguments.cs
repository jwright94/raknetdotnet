using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace EventSerializerGenerator
{
    class AppArguments
    {
        [Argument(ArgumentType.MultipleUnique, ShortName = "refasm", HelpText = "Referenced assembly.")]
        public string[] ReferencedAssemblies;
        [DefaultArgumentAttribute(ArgumentType.Required, HelpText = "Event template source path.")]
        public string EventTemplatePath;
    }
}
