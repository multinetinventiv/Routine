using Moq;
using NUnit.Framework;
using Routine.Core;
using Routine.Core.Cache;
using Routine.Core.CoreContext;

namespace Routine.Test.Core.CoreContext.Domain
{
	public class CachedBusiness
	{
		public string Id{get;set;}
	}
}

namespace Routine.Test.Core.CoreContext
{
	[TestFixture]
	public class CachedFactoryCoreContextTest : CoreTestBase
	{
		public override string[] DomainTypeRootNamespaces{get{return new[]{"Routine.Test.Core.CoreContext.Domain"};}}

		[Test]
		public void CachesDomainTypesByObjectModelId()
		{
			var factoryMock = new Mock<IFactory>();

			ICodingStyle codingStyle = 
				BuildRoutine.CodingStyle().FromBasic()
					.ModelId.Done(s => s.SerializeBy(t => t.FullName).DeserializeBy(id => id.ToType()))
					.Id.Done(e => e.ByPublicProperty(p => p.Returns<string>("Id")));

			var testing = new CachedFactoryCoreContext(factoryMock.Object, codingStyle, new DictionaryCache());

			factoryMock.Setup(o => o.Create<DomainType>()).Returns(() => new DomainType(testing));

			var modelId = "Routine.Test.Core.CoreContext.Domain.CachedBusiness";

			var expected = testing.GetDomainType(modelId);
			var actual = testing.GetDomainType(modelId);

			Assert.AreSame(expected, actual);

			factoryMock.Verify(o => o.Create<DomainType>(), Times.Once());
		}
	}
}

