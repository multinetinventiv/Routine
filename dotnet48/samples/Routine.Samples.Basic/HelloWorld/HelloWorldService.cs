﻿namespace Routine.Samples.Basic.HelloWorld
{
	public class HelloWorldService
	{
		public string GetMessage(string name)
		{
			return $"Hello {name}!";
		}
	}
}