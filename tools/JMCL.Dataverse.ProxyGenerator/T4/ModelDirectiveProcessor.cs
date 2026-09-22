using System.CodeDom;
using System.Collections.Generic;
using Microsoft.VisualStudio.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator.T4
{
	using Model;

	/// <summary>
	/// This directive processor implements the proxymodel directive to give the template access to the make the proxy model
	/// provided via the host session.
	/// </summary>
    public sealed class ModelDirectiveProcessor : T4DirectiveProcessor
    {
		public ModelDirectiveProcessor() : base("proxymodel") { }
		public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
		{
			if (!arguments.TryGetValue("name", out string name) || string.IsNullOrEmpty(name))
			{
				throw new DirectiveProcessorException("Proxymodel directive has no name argument");
			}

			if(!arguments.TryGetValue("type", out string type) || string.IsNullOrEmpty(type))
			{
				type = typeof(ProxyModel).ToString();				
			}				

			string fieldName = "_" + name + "Field";
			var typeRef = new CodeTypeReference(type);
			var thisRef = new CodeThisReferenceExpression();
			var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName);

			members.Add(new CodeMemberField(typeRef, fieldName));

			var property = new CodeMemberProperty()
			{
				Name = name,
				Attributes = MemberAttributes.Public | MemberAttributes.Final,
				HasGet = true,
				HasSet = false,
				Type = typeRef
			};
			
			property.GetStatements.Add(new CodeMethodReturnStatement(fieldRef));	
			
			members.Add(property);

			var valRef = new CodeVariableReferenceExpression("data");
			var namePrimitive = new CodePrimitiveExpression(name);
			var sessionRef = new CodePropertyReferenceExpression(thisRef, "Session");
			
			bool hasAcquiredCheck = hostSpecific;


			string acquiredName = "_" + name + "Acquired";
			var acquiredVariable = new CodeVariableDeclarationStatement(typeof(bool), acquiredName, new CodePrimitiveExpression(false));
			var acquiredVariableRef = new CodeVariableReferenceExpression(acquiredVariable.Name);
			if (hasAcquiredCheck)
			{
				postStatements.Add(acquiredVariable);
			}

			//checks the local called "data" can be cast and assigned to the field, and if successful, sets acquiredVariable to true
			var checkCastThenAssignVal = new CodeConditionStatement(
				new CodeMethodInvokeExpression(
					new CodeTypeOfExpression(typeRef), "IsAssignableFrom", new CodeMethodInvokeExpression(valRef, "GetType")),
				hasAcquiredCheck
					? new CodeStatement[] {
						new CodeAssignStatement (fieldRef, new CodeCastExpression (typeRef, valRef)),
						new CodeAssignStatement (acquiredVariableRef, new CodePrimitiveExpression (true)),
					}
					: new CodeStatement[] {
						new CodeAssignStatement (fieldRef, new CodeCastExpression (typeRef, valRef)),
					}
					,
				new CodeStatement[] {
					new CodeExpressionStatement (new CodeMethodInvokeExpression (thisRef, "Error",
					new CodePrimitiveExpression ("The type '" + type + "' of the parameter '" + name +
						"' did not match the type passed to the template"))),
				});


			//tries to gets the value from the session
			var checkSession = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(NotNull(sessionRef), CodeBinaryOperatorType.BooleanAnd,
					new CodeMethodInvokeExpression(sessionRef, "ContainsKey", namePrimitive)),
				new CodeVariableDeclarationStatement(typeof(object), "data", new CodeIndexerExpression(sessionRef, namePrimitive)),
				checkCastThenAssignVal);

			this.postStatements.Add(checkSession);			
			
		}

	}
}
