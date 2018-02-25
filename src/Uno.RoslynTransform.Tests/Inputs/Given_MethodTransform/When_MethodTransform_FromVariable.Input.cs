using System;

public class MyType
{
	public void Member02() { }
}

public class SUT
{
	public SUT()
	{
		var a = new MyType();
		a.Member01();
	}
}