using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Ast;
using O2.Kernel.ExtensionMethods;
using O2.DotNetWrappers.ExtensionMethods;

namespace O2.API.AST.ExtensionMethods.CSharp
{
    public static class AbstractNode_ExtensionMethods
    {
        public static bool hasReturnStatement(this AbstractNode abstractNode)
        {
			return abstractNode.iNodes<ReturnStatement>().size() > 0;
        }

        public static INode lastChild(this AbstractNode abstractNode)
        {
            var childrenCount = abstractNode.Children.Count;
            if (childrenCount > 0)
                return abstractNode.Children[childrenCount - 1];
            return null;
        }

        public static bool isLastChild(this AbstractNode abstractNode, Type type)
        {
            var lastChild = abstractNode.lastChild();

            return (lastChild != null) ?
                        lastChild.GetType() == type :
                        false;
        }

        public static object getLastReturnValue(this AbstractNode abstractNode)
        {
			var returnStatements = abstractNode.iNodes<ReturnStatement>();
			if (returnStatements.size() > 0)
            {
				var returnStatement = returnStatements.Last();
                if (returnStatement.Expression is PrimitiveExpression)
                {
                    var primitiveExpression = (PrimitiveExpression)returnStatement.Expression;
                    return primitiveExpression.Value;
                }
                return new object();
            }
            return null;
        }

    }
}
