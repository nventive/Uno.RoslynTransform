using System;

public class MyType
{
	public void Member02() { }
}

public class SUT
{
	public SUT()
	{
		new MyType().Member01();
	}
}