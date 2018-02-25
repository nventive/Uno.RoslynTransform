using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno.RoslynTransform
{
	public class TypeTransform : Transform
	{
		public string TypeName { get; set; }
		public string OriginalTypeName { get; set; }
	}
}
