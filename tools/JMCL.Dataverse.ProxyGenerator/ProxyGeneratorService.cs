using System;
using System.IO;
using Microsoft.VisualStudio.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator
{
    using Model;
    using T4;

    public class ProxyGeneratorService : MessagingBase, IProxyGeneratorService
    {
        private readonly ISettings Settings;
        public ProxyGeneratorService(ISettings settings)
        {
            this.Settings = settings ?? throw new ArgumentException("settings is required");
        }

        public void BuildProxies(ProxyModel model)
        {            
            _ = model ?? throw new ArgumentNullException("model is required.");

            var session = new TextTemplatingSession
            {
                ["Namespace"] = Settings?.Namespace ?? "Proxy",
                ["Model"] = model
            };

            Host host = new Host(Settings, session);
            host.Message += (sender, args) => { RaiseMessage(args.Message); };

            string input = File.ReadAllText(host.TemplateFile);

            var engine = new Mono.TextTemplating.TemplatingEngine();

            var output = engine.ProcessTemplate(input, host);

            if (host.Errors.Count > 0)
            {
                foreach (var error in host.Errors)
                {
                    RaiseMessage(error.ToString(),"",eMessageType.Error);
                }

                return;
            }
            
            var defaultFileName = Path.ChangeExtension(
                Path.Combine(
                    host.OutputPath, 
                    Path.GetFileName(host.TemplateFile)),
                host.FileExtension);

            if(output?.Length > 0)
            {
                CreateFile(defaultFileName, output);    
            }
            else if (File.Exists(defaultFileName))
            {
                RaiseMessage(string.Format("Deleting {0}.", defaultFileName), "", eMessageType.Info);
                File.Delete(defaultFileName);
            }            
        }


        protected virtual void CreateFile(String fileName, String content)
        {
            if (IsFileContentDifferent(fileName, content))
            {
                var path = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(path))
                {
                    RaiseMessage(string.Format("Creating directory '{0}'.", path), "", eMessageType.Verbose);

                    Directory.CreateDirectory(path);
                }

                RaiseMessage(string.Format("Writing file '{0}'.", fileName), "", eMessageType.Info);
                File.WriteAllText(fileName, content);
            }
            else
            {
                RaiseMessage(string.Format("Skipping unchanged file '{0}'.", fileName), "", eMessageType.Verbose);
            }
        }

        protected bool IsFileContentDifferent(String fileName, String newContent)
        {
            return !(File.Exists(fileName) && File.ReadAllText(fileName) == newContent);
        }

    }
}
