using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.RoslynTransform.Tests.Helpers;

namespace Uno.RoslynTransform.Tests
{
	[TestClass]
    public class Given_TypeTransform
    {
		[TestMethod]
		public async Task When_SameNamespace_And_New_Name()
		{
			var transforms = new TypeTransform[] {
				new TypeTransform {
					TypeName = "MyNameSpace.MyNewType",
					OriginalTypeName = "MyOriginalType"
				}
			};

			await TransformValidation.Validate(transforms);		
		}
	}
}
