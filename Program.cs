using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

public class ExpressionParser
{
    public class DynamicExpression
    {
        public readonly string code;
        private readonly MethodInfo? _method;
        private static bool IsInsideFunction(IdentifierNameSyntax identifier)
        {
            var parent = identifier.Parent;
            return parent is InvocationExpressionSyntax;
        }
        private static bool IsPartOfObjectMember(IdentifierNameSyntax identifier)
        {
            var parent = identifier.Parent;
            return parent is MemberAccessExpressionSyntax || parent is InvocationExpressionSyntax;
        }
        private static bool IsTypeOrMethod(string varName)
        {
            Type type;
            type = Type.GetType($"System.{varName}", false, true);
            if (type != null)
                return true;
            type = Type.GetType(varName);
            return type != null;
        }
        private static List<string> ExtractVariables(SyntaxNode root)
        {
            List<string> variaveis = [];
            var identifiers = root.DescendantNodesAndSelf().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax>();
            foreach (var identifier in identifiers)
            {
                var varName = identifier.Identifier.Text;
                if (
                    !IsPartOfObjectMember(identifier) &&
                    !IsInsideFunction(identifier) && 
                    !IsTypeOrMethod(varName) && 
                    !variaveis.Contains(varName)
                )
                    variaveis.Add(varName);
            }
            return variaveis;
        }

        private static SyntaxNode ReplaceExpressionWithParameters(SyntaxNode root, List<string> variaveis)
        {
            var newRoot = root;
            for (int i = 0; i < variaveis.Count; i++)
            {
                while (true){
                    var identifiers = newRoot.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                        .Where(id => id.Identifier.Text == variaveis[i]).ToList();
                    if (identifiers.Count <= 0)
                        break;
                    newRoot = newRoot.ReplaceNode(identifiers[0], SyntaxFactory.IdentifierName($"parms[{i}]"));
                }
            }
            return newRoot;
        }

        public DynamicExpression(string expression)
        {
            var tree = CSharpSyntaxTree.ParseText(expression);
            var root = tree.GetRoot();
            var variables = ExtractVariables(root);
            var newRoot = ReplaceExpressionWithParameters(root, variables);
            var new_expression = newRoot.ToString();

            code = $@"
using System;
public class DynamicFunction {{public static double Run(double[] parms) {{
{string.Join("\n", variables.Select((v, i) => $"// parms[{i}] -> {v}"))} 

return {new_expression};
}}}}";

            tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "DynamicExpression",
                syntaxTrees: new[] { tree },
                references: new[] {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),                 // System.Object
                    MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location), // System.Linq
                    MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),         // System.Console
                    MetadataReference.CreateFromFile(typeof(System.Double).Assembly.Location)           // System.Double
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                    throw new InvalidOperationException(
                        "Error compiling: \n" + string.Join("\n", result.Diagnostics.Select(d => d.ToString())) + 
                        "\n---" + code + "\n---"
                    );
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                var type = assembly.GetType("DynamicFunction");
                _method = type.GetMethod("Run");
                if (_method == null)
                    throw new InvalidOperationException("Method 'Run' not found.");
            }
        }
        public double Executar(double[] parms)
        {
            if (_method != null)
                return (double)(
                    _method.Invoke(null, new object[] { parms }) 
                    ?? throw new InvalidOperationException("Returned null")
                );
            throw new InvalidOperationException("Method 'Run' not found.");
        }
    }
    public static void Main()
    {
        var test = "X1 + X2 + Double.Sin(X1) / 1234 + Double.Tanh(X2) + Double.Abs(X2)"; 
        var dynExpression = new DynamicExpression(test);
        
        Console.WriteLine("Optimized Code:");
        Console.WriteLine(dynExpression.code);
        
        var values = new double[] { 10, 20 };
        var res = dynExpression.Executar(values);
        Console.WriteLine($"Your C# optimized code: {res}");
    }
}
