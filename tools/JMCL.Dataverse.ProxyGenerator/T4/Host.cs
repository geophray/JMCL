using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator.T4
{
    public class Host : MessagingBase, IProxyTemplateHost
    {
        protected ISettings Settings { get; }

        public string TemplateFile => GetRootedTemplatePath();

        public string OutputPath => GetRootedOutputPath();

        public string FileExtension { get; private set; } = ".txt";
        public Encoding FileEncoding { get; private set; } = Encoding.UTF8;
        public CompilerErrorCollection Errors { get; } = new CompilerErrorCollection();

        public Host(ISettings settings, TextTemplatingSession session)
        {
            this.Settings = settings ?? throw new ArgumentNullException("settings is required.");
            this.Session = session ?? throw new ArgumentNullException("session is required.");
        }

       
        public IList<string> StandardAssemblyReferences => new string[]
        {  
            typeof(System.Uri).Assembly.Location,
            typeof(System.Linq.Enumerable).Assembly.Location,
            typeof(Mono.TextTemplating.Directive).Assembly.Location,
            typeof(JMCL.Dataverse.ProxyGenerator.Model.ProxyModel).Assembly.Location
        };
          

        public IList<string> StandardImports => new string[]
        {
            "System",
            "System.Linq",            
            "JMCL.Dataverse.ProxyGenerator.Model"
        };
            

        public ITextTemplatingSession Session { get; set; }
                
        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            content = System.String.Empty;
            location = System.String.Empty;

            //If the argument is the fully qualified path of an existing file,
            //then we are done.
            //----------------------------------------------------------------
            if (File.Exists(requestFileName))
            {
                content = File.ReadAllText(requestFileName);
                return true;
            }
            //This can be customized to search specific paths for the file.
            //This can be customized to accept paths to search as command line
            //arguments.
            //----------------------------------------------------------------
            else
            {
                return false;
            }
        }
        
        public object GetHostOption(string optionName)
        {
            object returnObject;
            switch (optionName)
            {
                case "CacheAssemblies":
                    returnObject = true;
                    break;
                default:
                    returnObject = null;
                    break;
            }
            return returnObject;
        }
        
        public string ResolveAssemblyReference(string assemblyReference)
        {
            //If the argument is the fully qualified path of an existing file,
            //then we are done. (This does not do any work.)
            //----------------------------------------------------------------
            if (File.Exists(assemblyReference))
            {
                return assemblyReference;
            }
            //Maybe the assembly is in the same folder as the text template that
            //called the directive.
            //----------------------------------------------------------------
            string candidate = Path.Combine(Path.GetDirectoryName(this.TemplateFile), assemblyReference);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            //This can be customized to search specific paths for the file
            //or to search the GAC.
            //----------------------------------------------------------------
            //This can be customized to accept paths to search as command line
            //arguments.
            //----------------------------------------------------------------
            //If we cannot do better, return the original file name.
            return "";
        }
        
        public Type ResolveDirectiveProcessor(string processorName)
        {
            if (string.Compare(processorName, "ModelProcessor", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return typeof(ModelDirectiveProcessor);
            }

            if (string.Compare(processorName, "CodeBlockManager",StringComparison.OrdinalIgnoreCase) == 0)
            {
                return typeof(CodeBlockManagerDirectiveProcessor);
            }

            throw new NotSupportedException(string.Format("Directive processor type of {0} is not type supported.", processorName));
        }
       
        public string ResolvePath(string fileName)
        {
            _ = fileName ?? throw new ArgumentNullException("the file name cannot be null");
                        
            if (File.Exists(fileName))
            {
                return fileName;
            }
            
            string candidate = Path.Combine(Path.GetDirectoryName(this.TemplateFile), fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            
            return fileName;
        }
        
        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            if (directiveId == null)
            {
                throw new ArgumentNullException("the directiveId cannot be null");
            }
            if (processorName == null)
            {
                throw new ArgumentNullException("the processorName cannot be null");
            }
            if (parameterName == null)
            {
                throw new ArgumentNullException("the parameterName cannot be null");
            }
            //Code to provide "hard-coded" parameter values goes here.
            //This code depends on the directive processors this host will interact with.
            //If we cannot do better, return the empty string.
            return String.Empty;
        }
        
        public void SetFileExtension(string extension)
        {
            FileExtension = extension;
        }
        
        public void SetOutputEncoding(System.Text.Encoding encoding, bool fromOutputDirective)
        {
            FileEncoding = encoding;
        }
        
        public void LogErrors(CompilerErrorCollection errors)
        {
            Errors.AddRange(errors);
        }
        
        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            return null;
        }

        public ITextTemplatingSession CreateSession()
        {
            return new TextTemplatingSession();
        }

        private string GetRootedTemplatePath()
        {          
            return Path.GetFullPath(Settings.TemplateFilePath);
        }

        private string GetRootedOutputPath()
        {
            if (string.IsNullOrEmpty(Settings.OutputPath))
                return Path.GetDirectoryName(GetRootedTemplatePath());

            return Path.GetFullPath(Settings.OutputPath);            
        }

        public new void RaiseMessage(string message, string extendedMessage = "", eMessageType messageType = eMessageType.Info)
        {
            base.RaiseMessage(message, extendedMessage, messageType);
        }
    }
}

