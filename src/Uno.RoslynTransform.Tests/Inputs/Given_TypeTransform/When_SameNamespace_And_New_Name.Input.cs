using System;

namespace MyNameSpace
{
    class MyNewType
    {
		public void Test() { }
    }
}

namespace OtherNamespace
{
	using MyNameSpace;

	class SomeClass
    {
		public void SomeMethod()
		{
			new MyOriginalType().Test();
		}
	}
}