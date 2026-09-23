
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator.T4
{
    /// <summary>
    /// Manages the creation of separate files for blocks of code created by a T4 template. Created by 
    /// the <see cref="CodeBlockManagerDirectiveProcessor"/> when a "codemanagerblock" directive is 
    /// added to a T4 template.
    /// <credit>
    /// This code is based on source code originally created from https://github.com/damieng at 
    /// https://github.com/damieng/DamienGKit/tree/master/T4/MultipleOutputHelper and explained 
    /// at http://damieng.com/blog/2009/11/06/multiple-outputs-from-t4-made-easy-revisited.
    /// </credit>
    /// </summary>
    public class CodeBlockManager : ICodeBlockManager
    {
       
        private class Block
        {
            public String Name;
            public int Start, Length;
            public bool IncludeInDefault;
            public bool IncludeFooter;
            public bool IncludeHeader;
        }

        private IProxyTemplateHost Host { get; }
        private StringBuilder GenerationEnvironment { get; }

        private Block currentBlock;
        private readonly List<Block> files = new List<Block>();
        private readonly Block footer = new Block();
        private readonly Block header = new Block();               
        
        public void StartFile(String name, bool includeHeader = true, bool includeFooter = true)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            CurrentBlock = new Block { Name = name, IncludeHeader = includeHeader, IncludeFooter = includeFooter };
        }

        public void StartFooter(bool includeInDefault = true)
        {
            CurrentBlock = footer;
            footer.IncludeInDefault = includeInDefault;
        }

        public void StartHeader(bool includeInDefault = true)
        {
            CurrentBlock = header;
            header.IncludeInDefault = includeInDefault;
        }

        public void EndBlock()
        {
            if (CurrentBlock == null)
                return;
            CurrentBlock.Length = GenerationEnvironment.Length - CurrentBlock.Start;
            if (CurrentBlock != header && CurrentBlock != footer)
                files.Add(CurrentBlock);
            currentBlock = null;
        }

        public virtual void Process(bool split = true, bool createDefault = false)
        {
            if (split)
            {
                EndBlock();
                String headerText = GenerationEnvironment.ToString(header.Start, header.Length);
                String footerText = GenerationEnvironment.ToString(footer.Start, footer.Length);
                String outputPath = Host.OutputPath; 
                files.Reverse();
                if (!footer.IncludeInDefault)
                    GenerationEnvironment.Remove(footer.Start, footer.Length);
                foreach (Block block in files)
                {
                    String fileName = Path.Combine(outputPath, block.Name);
                    if (!Path.HasExtension(fileName))
                    {
                        fileName = Path.ChangeExtension(fileName, Host.FileExtension);
                    }

                    String content = string.Format(
                        "{0}{1}{2}",
                        block.IncludeHeader ? headerText : string.Empty, 
                        GenerationEnvironment.ToString(block.Start, block.Length), 
                        block.IncludeFooter ? footerText : string.Empty);
                   
                    CreateFile(fileName, content);
                    GenerationEnvironment.Remove(block.Start, block.Length);
                }
                if (!header.IncludeInDefault)
                    GenerationEnvironment.Remove(header.Start, header.Length);
            }

            if (!createDefault)
            {
                var length = GenerationEnvironment.Length;
                GenerationEnvironment.Remove(0, length);
            }
        }

        protected virtual void CreateFile(String fileName, String content)
        {
            if (IsFileContentDifferent(fileName, content))
            {
                var path = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(path))
                {
                    Host.RaiseMessage(string.Format("Creating directory '{0}'.", path), "", eMessageType.Verbose);

                    Directory.CreateDirectory(path);
                }

                Host.RaiseMessage(string.Format("Writing file '{0}'.", fileName), "", eMessageType.Info);
                File.WriteAllText(fileName, content);
            }
            else
            {
                Host.RaiseMessage(string.Format("Skipping unchanged file '{0}'.", fileName), "", eMessageType.Verbose);
            }
        }

        public virtual String GetCustomToolNamespace(String fileName)
        {
            return null;
        }

        public virtual String DefaultProjectNamespace
        {
            get { return null; }
        }

        protected bool IsFileContentDifferent(String fileName, String newContent)
        {
            return !(File.Exists(fileName) && File.ReadAllText(fileName) == newContent);
        }

        public CodeBlockManager(ITextTemplatingEngineHost host, StringBuilder generationEnvironment)
        {
            this.Host = (IProxyTemplateHost)host ?? throw new ArgumentNullException("host is required");
            this.GenerationEnvironment = generationEnvironment ?? throw new ArgumentNullException("generationEnvironment is required");

            Host.RaiseMessage("Loaded Code Block Manager.", "", eMessageType.Verbose);
        }

        private Block CurrentBlock
        {
            get { return currentBlock; }
            set
            {
                if (CurrentBlock != null)
                    EndBlock();
                if (value != null)
                    value.Start = GenerationEnvironment.Length;
                currentBlock = value;
            }
        }

    }

}

