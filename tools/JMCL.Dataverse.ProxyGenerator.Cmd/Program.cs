using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using CmdLine;
using JMCL.Core;
using JMCL.Dataverse.ProxyGenerator;
using JMCL.Dataverse.ProxyGenerator.Cmd;
using JMCL.Dataverse.ProxyGenerator.Cmd.Extensions;

using JMCL.Dataverse.ProxyGenerator.Cmd.Spkl;

class Program
{
    private static IReadOnlyIocContainer Container;

    static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("JMCL Dataverse Proxy Builder v" + Assembly.GetEntryAssembly().GetName().Version);

        Console.ForegroundColor = ConsoleColor.Gray;
        bool error = false;
        CommandLineArgs arguments = null;

        try
        {
            Init();

            arguments = CommandLine.Parse<CommandLineArgs>();

            Run(arguments);
        }
        catch (CommandLineException exception)
        {
            Console.WriteLine(exception.ArgumentHelp.Message);
            Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
        }
        catch (FaultException<OrganizationServiceFault> ex)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("The application terminated with an error.");
            Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
            Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
            Console.WriteLine("Message: {0}", ex.Detail.Message);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(ex.StackTrace);

            if (!string.IsNullOrEmpty(ex.Detail.TraceText))
            {
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText);
            }
            if (ex.Detail.InnerFault != null)
            {
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            error = true;
            Console.ForegroundColor = ConsoleColor.White;
        }
        catch (TimeoutException ex)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("The application terminated with an error.");
            Console.WriteLine("Message: {0}", ex.Message);
            Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Fault: {0}", ex.InnerException.Message ?? "No Inner Fault");
            }
            error = true;
            Console.ForegroundColor = ConsoleColor.White;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("The application terminated with an error.");
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(ex.StackTrace);

            // Display the details of the inner exception.
            if (ex.InnerException != null)
            {
                Console.WriteLine(ex.InnerException.Message);

                if (ex.InnerException is FaultException<OrganizationServiceFault> fe)
                {
                    Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                    Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                    Console.WriteLine("Message: {0}", fe.Detail.Message);
                    if (!string.IsNullOrEmpty(fe.Detail.TraceText))
                    {
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText);
                    }
                    if (fe.Detail.InnerFault != null)
                    {
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
            error = true;
            Console.ForegroundColor = ConsoleColor.White;
        }
        finally
        {
            if (error)
            {
                Environment.ExitCode = 1;
            }
        }

        if (arguments != null && arguments.WaitForKey == true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static void Init()
    {
        var container = new IocContainer();

        container.Implement<ICache>().Using<AssemblyCache>().AsSingleInstance();
        container.Implement<ISpklSettingsService>().Using<SpklSettingsService>().AsSingleInstance();
        container.Implement<IProxySettingsService>().Using<ProxySettingsService>().AsSingleInstance();
        container.Implement<IDirectoryService>().Using<DirectoryService>().AsSingleInstance();
        container.Implement<ITypeConverterFactory>().Using<TypeConverterFactory>();
        container.Implement<ICDSMetadataServiceFactory>().Using<CDSMetadataServiceFactory>().AsSingleInstance();

        Container = container;
    }

    private static void Run(CommandLineArgs arguments)
    {
        try
        {
            var executingDirectory = Environment.CurrentDirectory;
            
            var searchPath = arguments.Path ?? Path.Combine(executingDirectory, "..\\");

            if (arguments.Connection == null)
            {
                var connection = new DataverseClientConnection();
                var client = connection.Connect();
                arguments.Connection = connection.ConnectionString;

                BuildProxy(client, searchPath);
            }
            else
            {
                // Does the connection contain a password prompt?
                var passwordMatch = Regex.Match(arguments.Connection, "Password=[*]+", RegexOptions.IgnoreCase);
                if (passwordMatch.Success)
                {
                    // Prompt for password
                    Console.WriteLine("Password required for connection {0}", arguments.Connection);
                    Console.Write("Password:");
                    var password = ReadPassword('*');
                    arguments.Connection = arguments.Connection.Replace(passwordMatch.Value, "Password=" + password);
                }

                using (var serviceClient = new ServiceClient(arguments.Connection))
                {
                    if (!serviceClient.IsReady)
                    {
                        throw new Exception(String.Format("Error connecting to Dataverse: {0}", serviceClient.LastError));
                    }

                    BuildProxy(serviceClient, searchPath);
                }
            }
        }
        catch (CommandLineException exception)
        {
            Console.WriteLine(exception.ArgumentHelp.Message);
            Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
        }

    }

    public static string ReadPassword(char mask)
    {
        const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
        int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const

        var pass = new Stack<char>();
        char chr = (char)0;

        while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
        {
            if (chr == BACKSP)
            {
                if (pass.Count > 0)
                {
                    Console.Write("\b \b");
                    pass.Pop();
                }
            }
            else if (chr == CTRLBACKSP)
            {
                while (pass.Count > 0)
                {
                    Console.Write("\b \b");
                    pass.Pop();
                }
            }
            else if (FILTERED.Count(x => chr == x) > 0) { }
            else
            {
                pass.Push((char)chr);
                Console.Write(mask);
            }
        }

        Console.WriteLine();

        return new string(pass.Reverse().ToArray());
    }


    private static void BuildProxy(IOrganizationService organizationService, string searchPath)
    {   
        var settings = GetSettingsFromProxyConfigFile(searchPath) 
            ??  GetSettingsFromSpklConfigFile(searchPath) 
            ?? throw new Exception("Unable to load settings.");

        Console.WriteLine("Found {0} settings files...", settings.Count());

        foreach (var setting in settings)
        {            
            var metadataService = Container.Resolve<ICDSMetadataServiceFactory>().Create(setting);
            metadataService.Message += MessageHandler;
            var entityMetadata = metadataService.GetEntityMetadata(organizationService);
            var sdkMessageMetadata = metadataService.GetMessageMetadata(organizationService);
            metadataService.Message -= MessageHandler;

            var typeConverterFactory = Container.Resolve<ITypeConverterFactory>();
            var typeConverter = typeConverterFactory.Create(setting.TemplateLanguage);

            var modelService = new ProxyModelService(typeConverter);
            modelService.Message += MessageHandler;
            var model = modelService.BuildModel(entityMetadata, sdkMessageMetadata);
            modelService.Message -= MessageHandler;

            var generator = new ProxyGeneratorService(setting);
            generator.Message += MessageHandler;       
            generator.BuildProxies(model);
            generator.Message -= MessageHandler;
        }
    }

    private static IEnumerable<ISettings> GetSettingsFromProxyConfigFile(string searchPath)
    {
        Console.WriteLine("Searching for proxies.json in '{0}'", searchPath);

        var settings = Container.Resolve<IProxySettingsService>().LoadSettings(searchPath);

        return settings?.Select(s => s
           .SetTemplateRelativeTo(s.ConfigurationPath)
           .SetOutputRelativeTo(s.ConfigurationPath));
    }

    private static IEnumerable<ISettings> GetSettingsFromSpklConfigFile(string searchPath)
    {
        Console.WriteLine("Searching for spkl.json in '{0}'", searchPath);

        var settings = Container.Resolve<ISpklSettingsService>().LoadSettings(searchPath);
               
        return settings?.Select(s => s
            .SetTemplateRelativeTo(s.ConfigurationPath)
            .SetOutputRelativeTo(s.ConfigurationPath));       
    }

    private static void MessageHandler(object sender, MessageEventArgs e)
    {
        Console.WriteLine(e.Message);
    }


}

