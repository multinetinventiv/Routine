using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Routine.Core;

namespace Routine.Test
{
	[TestFixture]
	public class ReflectionExtensionsTest
	{
		[SetUp]
		public void SetUp()
		{
			TypeInfo.AddDomainTypeRootNamespace("Routine.Test");
		}

		#region Helpers

		private IParameter Parameter(TypeInfo parameter)
		{
			var result = new Mock<IParameter>();

			result.Setup(o => o.Type).Returns(parameter);

			return result.Object;
		}

		private IOperation Operation(string name, params TypeInfo[] parameterTypes)
		{
			return Operation(name, type.ofvoid(), parameterTypes);
		}

		private IOperation Operation(TypeInfo returnType, params TypeInfo[] parameterTypes)
		{
			return Operation("Dummy", returnType, parameterTypes);
		}

		private IOperation Operation(string name, TypeInfo returnType, params TypeInfo[] parameterTypes)
		{
			var result = new Mock<IOperation>();

			var parameters = parameterTypes.Select(p => Parameter(p)).ToList();
			result.Setup(o => o.Parameters).Returns(parameters);
			result.Setup(o => o.Type).Returns(returnType);
			result.Setup(o => o.Name).Returns(name);

			return result.Object;
		}

		#endregion

		[Test]
		public void Test_IOperation_HasParameters()
		{
			Assert.IsTrue(Operation(type.ofvoid()).HasNoParameters());
			Assert.IsTrue(Operation(type.ofvoid(), type.of<string>()).HasParameters<string>());
			Assert.IsTrue(Operation(type.ofvoid(), type.of<string>(), type.of<int>()).HasParameters<string, int>());
			Assert.IsTrue(Operation(type.ofvoid(), type.of<string>(), type.of<int>(), type.of<double>()).HasParameters<string, int, double>());
			Assert.IsTrue(Operation(type.ofvoid(), type.of<string>(), type.of<int>(), type.of<double>(), type.of<decimal>()).HasParameters<string, int, double, decimal>());

			Assert.IsFalse(Operation(type.ofvoid(), type.of<string>(), type.of<int>(), type.of<double>()).HasParameters<string, int>());;
		}

		[Test]
		public void Test_IOperation_ReturnsVoid()
		{
			Assert.IsTrue(Operation(type.ofvoid()).ReturnsVoid());
			Assert.IsFalse(Operation(type.of<string>()).ReturnsVoid());
		}

		[Test]
		public void Test_IMember_Returns()
		{
			Assert.IsTrue(Operation(type.of<string>()).Returns(type.of<object>()));
			Assert.IsFalse(Operation(type.of<int>()).Returns(type.of<string>()));

			Assert.IsTrue(Operation(type.of<List<string>>()).ReturnsCollection());
			Assert.IsTrue(Operation(type.of<List<string>>()).ReturnsCollection(type.of<object>()));
			Assert.IsFalse(Operation(type.of<IList>()).ReturnsCollection(type.of<string>()));

			//generics
			Assert.IsTrue(Operation(type.of<string>()).Returns<string>());
			Assert.IsTrue(Operation(type.of<List<string>>()).ReturnsCollection<string>());

			//with name parameter
			Assert.IsFalse(Operation("Right", type.of<string>()).Returns(type.of<string>(), "Wrong"));
			Assert.IsFalse(Operation("Right", type.of<List<string>>()).ReturnsCollection(type.of<string>(), "Wrong"));
		}
	}
}

