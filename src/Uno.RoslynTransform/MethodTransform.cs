using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno.RoslynTransform
{
    public class MethodTransform : Transform
    {
		public string OwnerType { get; set; }

		public string OriginalMethodName { get; set; }

		public string MethodName { get; set; }
		public string TypeName { get; set; }
		public string OriginalTypeName { get; set; }
	}
}
