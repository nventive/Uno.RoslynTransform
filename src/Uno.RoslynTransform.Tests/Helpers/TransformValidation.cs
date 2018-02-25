using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Uno.RoslynTransform.Tests.Helpers
{
    class TransformValidation
	{
		private static readonly HashSet<Assembly> RequiredAssemblies = new HashSet<Assembly>
		{
			typeof(object).Assembly,
			typeof(Enumerable).Assembly,
			typeof(Compilation).Assembly,
			typeof(Semaphore).Assembly,
		};

		internal static readonly string TestProjectName = "TestProject";
		internal static readonly ProjectId TestProjectId = ProjectId.CreateNewId(TestProjectName);

		private static readonly IEnumerable<MetadataReference> RequiredMetadataReferences
			= RequiredAssemblies.Select(a => MetadataReference.CreateFromFile(new Uri(a.CodeBase).LocalPath));


		private static Solution CreateSolutionFromCode(string code, string fileName)
		{
			var baseSolution = CreateBaseSolution();

			var documentId = DocumentId.CreateNewId(TestProjectId, debugName: fileName);
			baseSolution = baseSolution.AddDocument(documentId, fileName, SourceText.From(code));

			var project = baseSolution.GetProject(TestProjectId);
			return baseSolution;
		}

		public static async Task Validate(
			Transform[] transforms,
			[CallerFilePath] string context = null,
			[CallerMemberName] string name = null
		)
		{
			var fileName = "File01.cs";

			var SUT = new SolutionTransform("Debug", "Any CPU");

			var baseSolution = CreateSolutionFromCode(GetCode(Path.GetFileNameWithoutExtension(context), name, "Input"), fileName);

			var updatedSolution = await SUT.TransformSolution(baseSolution, transforms);

			var tree = await updatedSolution.Projects.First().Documents.First().GetSyntaxTreeAsync();

			Assert.AreEqual(GetCode(Path.GetFileNameWithoutExtension(context), name, "Output"), tree.ToString());
		}

		private static string GetCode(string context, string name, string direction)
		{
			string resourceName = $"{typeof(TransformValidation).Assembly.GetName().Name}.Inputs.{context}.{name}.{direction}.cs";

			using (var reader = new StreamReader(typeof(TransformValidation).Assembly.GetManifestResourceStream(resourceName)))
			{
				return reader.ReadToEnd();
			}
		}
		private static Solution CreateBaseSolution()
		{
			var pi = ProjectInfo.Create(
				id: TestProjectId,
				version: VersionStamp.Default,
				name: TestProjectName,
				assemblyName: TestProjectName,
				language: LanguageNames.CSharp,
				compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);

			return new AdhocWorkspace()
				.CurrentSolution
				.AddProject(pi)
				.AddMetadataReferences(TestProjectId, RequiredMetadataReferences);
		}
	}
}
