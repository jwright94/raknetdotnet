using CommandLine;

namespace ProtocolGenerator
{
    internal class AppArguments
    {
        [Argument(ArgumentType.MultipleUnique, ShortName = "refasm", HelpText = "Referenced assembly.")] public string[] ReferencedAssemblies;

        [DefaultArgumentAttribute(ArgumentType.Required, HelpText = "Event template source path.")] public string EventTemplatePath;
    }
}