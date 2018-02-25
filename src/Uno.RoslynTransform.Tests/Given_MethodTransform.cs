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
    public class Given_MethodTransform
    {
		[TestMethod]
		public async Task When_MethodTransform_ObjectCreation()
		{
			var transforms = new MethodTransform[] {
				new MethodTransform {
					MethodName = "Member02",
					OwnerType = "MyType",
					OriginalMethodName = "Member01"
				}
			};

			await TransformValidation.Validate(transforms);		
		}

		[TestMethod]
		public async Task When_MethodTransform_FromVariable()
		{
			var transforms = new MethodTransform[] {
				new MethodTransform {
					MethodName = "Member02",
					OwnerType = "MyType",
					OriginalMethodName = "Member01"
				}
			};

			await TransformValidation.Validate(transforms);
		}
	}
}
