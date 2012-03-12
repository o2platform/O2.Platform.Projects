using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Ast;

namespace O2.API.AST.ExtensionMethods.CSharp
{
    public static class AbstractNode_ExtensionMethods
    {
        public static bool hasReturnStatement(this AbstractNode abstractNode)
        {
            return abstractNode.isLastChild(typeof(ReturnStatement));
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
            if (abstractNode.hasReturnStatement())
            {
                var returnStatement = (ReturnStatement)abstractNode.lastChild();
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
