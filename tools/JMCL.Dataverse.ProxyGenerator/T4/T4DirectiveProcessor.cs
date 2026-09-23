using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TextTemplating;
using Mono.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator.T4
{
    public abstract class T4DirectiveProcessor : DirectiveProcessor, IRecognizeHostSpecific
    {

		private readonly string directive;

		protected CodeDomProvider provider;

		protected bool hostSpecific;
		protected readonly List<CodeStatement> postStatements = new List<CodeStatement>();
		protected readonly List<CodeTypeMember> members = new List<CodeTypeMember>();

		#region IRecognizeHostSpecific 

		void IRecognizeHostSpecific.SetProcessingRunIsHostSpecific(bool hostSpecific)
		{
			this.hostSpecific = hostSpecific;
		}

		public virtual bool RequiresProcessingRunIsHostSpecific
		{
			get { return false; }
		}

		#endregion IRecognizeHostSpecific

		protected T4DirectiveProcessor(string directive)
		{
			this.directive = directive;
		}

		public override bool IsDirectiveSupported(string directiveName)
		{
			return string.Compare(directiveName, directive, true) == 0;
		}

		public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
		{
			base.StartProcessingRun(languageProvider, templateContents, errors);
			provider = languageProvider;
			postStatements.Clear();
			members.Clear();
		}

		public override void FinishProcessingRun()
		{
			var statement = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Errors"), "HasErrors"),
					CodeBinaryOperatorType.ValueEquality,
					new CodePrimitiveExpression(false)),
				postStatements.ToArray());

			postStatements.Clear();
			postStatements.Add(statement);
		}

		public override string GetClassCodeForProcessingRun()
		{
			var code = TemplatingEngine.GenerateIndentedClassCode(provider, members);
			return code;
		}

		public override string[] GetImportsForProcessingRun()
		{
			return null;
		}

		public override string GetPostInitializationCodeForProcessingRun()
		{
			return TemplatingEngine.IndentSnippetText(provider, StatementsToCode(postStatements), "            ");
		}

		string StatementsToCode(List<CodeStatement> statements)
		{
			var options = new CodeGeneratorOptions();
			using (var sw = new StringWriter())
			{
				foreach (var statement in statements)
					provider.GenerateCodeFromStatement(statement, sw, options);
				var code = sw.ToString();
				return code;
			}
		}

		public override string GetPreInitializationCodeForProcessingRun()
		{
			return null;
		}

		public override string[] GetReferencesForProcessingRun()
		{
			return null;
		}

		protected static CodeBinaryOperatorExpression IsNull(CodeExpression reference)
		{
			return new CodeBinaryOperatorExpression(reference, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
		}

		protected static CodeBinaryOperatorExpression NotNull(CodeExpression reference)
		{
			return new CodeBinaryOperatorExpression(reference, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
		}

		protected static CodeBinaryOperatorExpression IsFalse(CodeExpression expr)
		{
			return new CodeBinaryOperatorExpression(expr, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
		}

		protected static CodeBinaryOperatorExpression BooleanAnd(CodeExpression expr1, CodeExpression expr2)
		{
			return new CodeBinaryOperatorExpression(expr1, CodeBinaryOperatorType.BooleanAnd, expr2);
		}

	}
}
