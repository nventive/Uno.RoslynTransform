using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace Uno.RoslynTransform
{
	public class SolutionTransform
	{
		private readonly string _configuration;
		private readonly string _platform;
		private readonly string _solutionFilePath;

		public SolutionTransform(string configuration, string platform)
		{
			_configuration = configuration;
			_platform = platform;
		}

		public async void TransformProject(string solutionFilePath, Transform[] transforms)
		{
			var globalProperties = new Dictionary<string, string> {
				{ "Configuration", _configuration },
			};

			var ws = MSBuildWorkspace.Create(globalProperties);

			var solution = await ws.OpenSolutionAsync(_solutionFilePath);

			await TransformSolution(solution, transforms);
		}

		public async Task<Solution> TransformSolution(Solution solution, Transform[] transforms)
		{
			foreach (var project in solution.Projects)
			{
				var compilation = await project.GetCompilationAsync();
				var diagnostics = compilation.GetDiagnostics();

				var errors = diagnostics
					.Where(d => !d.IsWarningAsError && d.Severity == DiagnosticSeverity.Error);

				var trees = new Dictionary<string, SyntaxTree>();

				foreach (var error in errors)
				{
					if (error.Location.SourceTree.TryGetRoot(out var root))
					{
						if(trees.TryGetValue(error.Location.SourceTree.FilePath, out var updatedTree))
						{
							root = await updatedTree.GetRootAsync();
						}

						if (root.FindNode(error.Location.SourceSpan) is SyntaxNode errorNode)
						{
							var model = compilation.GetSemanticModel(error.Location.SourceTree);

							// https://github.com/dotnet/roslyn/issues/18711
							var editor = new SyntaxEditor(root, solution.Workspace);

							if (errorNode is IdentifierNameSyntax errorIdentifier)
							{
								var p = errorNode.Parent;

								if (p is ObjectCreationExpressionSyntax creationSyntax)
								{
									var typeTransform = transforms
										.OfType<TypeTransform>()
										.FirstOrDefault(t =>
											t.OriginalTypeName == creationSyntax.Type.ToFullString()
										);

									if (typeTransform != null)
									{
										var updatedIdentifierRoot = 
											CSharpSyntaxTree
												.ParseText(typeTransform.TypeName + " a")
												.GetRoot();

										var d = (updatedIdentifierRoot.ChildNodes().First() as FieldDeclarationSyntax).Declaration.Type;

										editor.ReplaceNode(errorNode, d);
									}
								}
								else  if (p is MemberAccessExpressionSyntax exp)
								{
									var owner = GetOwner(model, exp);

									var identifierTransform = transforms
										.OfType<MethodTransform>()
										.FirstOrDefault(t =>
											t.OwnerType == owner.ToString()
											&& t.OriginalMethodName == errorIdentifier.Identifier.Value.ToString()
										);

									if (identifierTransform != null)
									{
										var updatedIdentifier = errorIdentifier.WithIdentifier(
											CSharpSyntaxTree.ParseText(identifierTransform.MethodName).GetRoot().GetFirstToken()
										);

										editor.ReplaceNode(errorNode, updatedIdentifier);
									}
								}
							}

							if (editor.OriginalRoot != editor.GetChangedRoot())
							{
								trees[error.Location.SourceTree.FilePath] = editor.GetChangedRoot().SyntaxTree;
							}
						}
					}
				}

				foreach (var updatedDoc in trees)
				{
					var p = project.RemoveDocument(project.Documents.First(d => GetDocumentFullPath(d) == updatedDoc.Key).Id);
					solution = p.AddDocument(updatedDoc.Key, updatedDoc.Value.GetRoot()).Project.Solution;
				}
			}

			return solution;
		}

		private static string GetDocumentFullPath(Document d) => Path.Combine(d.FilePath ?? "", d.Name);

		private static ISymbol GetOwner(SemanticModel model, MemberAccessExpressionSyntax exp)
		{
			if (exp.Expression is ObjectCreationExpressionSyntax objCreation)
			{
				return model.GetSymbolInfo(objCreation.Type).Symbol;
			}
			else if (exp.Expression is IdentifierNameSyntax identifierName)
			{
				var symbol = model.GetSymbolInfo(identifierName).Symbol;

				if(symbol is ILocalSymbol localSymbol)
				{
					return localSymbol.Type;
				}
			}

			return null;
		}
	}
}
