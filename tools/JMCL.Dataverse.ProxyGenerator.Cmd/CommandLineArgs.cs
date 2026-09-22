using CmdLine;

[CommandLineArguments(Program = "jmcl-proxybuilder", Title = "JMCL Dataverse Proxy Builder", Description = "Create Dataverse early-bound proxies using T4 templates.")]
public class CommandLineArgs
{
    [CommandLineParameter(Command = "?", Default = false, Description = "Show Help", Name = "Help", IsHelp = true)]
    public bool Help { get; set; }

    [CommandLineParameter(Name = "settingspath", Command = "s", Required = false, Description = "Optional search path for the proxies.json file. When omitted search begins one level above the folder containing the executable.")]
    public string Path { get; set; }

    [CommandLineParameter(Name = "connection", Command = "c", Required = false, Description = "Optional connection string, in Microsoft.PowerPlatform.Dataverse.Client's ServiceClient grammar. When omitted the tool interactively signs in via OAuth and offers saved connections.")]
    public string Connection { get; set; }

    [CommandLineParameter(Name = "Wait for keypress", Command = "w", Required = false, Description = "Optional wait for a key press at the end of task run. When omitted the executable will exit after completion.")]
    public bool WaitForKey { get; set; } = false;
}
