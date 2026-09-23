using System.CodeDom;
using System.Collections.Generic;
using Microsoft.VisualStudio.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator.T4
{
	public sealed class CodeBlockManagerDirectiveProcessor : T4DirectiveProcessor
	{
		public CodeBlockManagerDirectiveProcessor() : base("codeblockmanager")
		{
		}

		public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
		{
			if (!arguments.TryGetValue("name", out string name) || string.IsNullOrEmpty(name))
			{
				throw new DirectiveProcessorException("Codeblockmanager directive has no name argument");
			}

			var thisRef = new CodeThisReferenceExpression();
			var typeRef = new CodeTypeReference(typeof(T4.CodeBlockManager));

			// create field for code block manager object
			string fieldName = "_" + name + "Field";				
			var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName);
			members.Add(new CodeMemberField(typeRef, fieldName));

			// create property for code block manager with creation of object on first use.
			var property = new CodeMemberProperty()
			{
				Name = name,
				Attributes = MemberAttributes.Public | MemberAttributes.Final,
				HasGet = true,
				HasSet = false,
				Type = typeRef
			};

			var hostRef = new CodePropertyReferenceExpression(thisRef, "Host");
			var generationEnvironmentRef = new CodePropertyReferenceExpression(thisRef, "GenerationEnvironment");

			var statement = new CodeConditionStatement(
				IsNull(fieldRef), 
				new CodeAssignStatement(fieldRef, new CodeObjectCreateExpression(typeRef, hostRef, generationEnvironmentRef)));
			
			property.GetStatements.Add(statement);
			property.GetStatements.Add(new CodeMethodReturnStatement(fieldRef));
			
			members.Add(property);			
		}
	}
}
