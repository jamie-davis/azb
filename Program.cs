using ConsoleToolkit.ApplicationStyles;
using azb.Properties;

namespace azb
{
    class Program : CommandDrivenApplication
    {
        static void Main(string[] args)
        {
            ConsoleToolkit.Toolkit.Execute<Program>(args);
        }

        protected override void Initialise()
        {
            HelpCommand<HelpCommand>(h => h.Command);
            base.Initialise();
        }

        protected override void OnCommandSuccess()
        {
            Settings.Default.Save();
            base.OnCommandSuccess();
        }
    }
}
