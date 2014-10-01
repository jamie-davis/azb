using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;

namespace azb
{
    [Command]
    [Description("Display help text and exit.")]
    class HelpCommand
    {
        [Positional(DefaultValue = null)]
        [Description("The command on which help is required.")]
        public string Command { get; set; }
    }
}