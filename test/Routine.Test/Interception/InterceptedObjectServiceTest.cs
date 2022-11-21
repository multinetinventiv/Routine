﻿using Routine.Core;
using Routine.Interception.Configuration;
using Routine.Interception.Context;
using Routine.Interception;
using Routine.Test.Core;
using Routine.Test.Engine.Stubs.ObjectServiceInvokers;

namespace Routine.Test.Interception;

[TestFixture(typeof(Sync))]
[TestFixture(typeof(Async))]
public class InterceptedObjectServiceTest<TObjectServiceInvoker> : CoreTestBase
    where TObjectServiceInvoker : IObjectServiceInvoker, new()
{
    private Mock<IObjectService> mock;
    private IObjectServiceInvoker invoker;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        mock = new Mock<IObjectService>();
        mock.Setup(os => os.ApplicationModel).Returns(GetApplicationModel);
        mock.Setup(os => os.Get(It.IsAny<ReferenceData>())).Returns((ReferenceData id) => _objectDictionary[id]);
        mock.Setup(os => os.GetAsync(It.IsAny<ReferenceData>())).ReturnsAsync((ReferenceData id) => _objectDictionary[id]);

        invoker = new TObjectServiceInvoker();
    }

    private InterceptedObjectService Build(
        Func<InterceptionConfigurationBuilder, IInterceptionConfiguration> interceptionConfiguration
    ) => new(mock.Object, interceptionConfiguration(BuildRoutine.InterceptionConfig()));

    [Test]
    public void ApplicationModel_property_is_intercepted_with_default_context()
    {
        var hit = false;
        var testing = Build(ic => ic.FromBasic()
            .Interceptors.Add(c => c.Interceptor(i => i.Before(ctx =>
                {
                    Assert.AreEqual($"{InterceptionTarget.ApplicationModel}", ctx.Target);
                    Assert.IsInstanceOf<InterceptionContext>(ctx);

                    hit = true;
                }
            )))
        );

        var _ = testing.ApplicationModel;

        Assert.IsTrue(hit);
    }

    [Test]
    public void Get_method_is_intercepted_with_object_reference_context()
    {
        ModelsAre(Model("model"));
        ObjectsAre(Object(Id("id", "model")));

        var hit = false;

        var testing = Build(ic => ic.FromBasic()
            .Interceptors.Add(c => c.Interceptor(i => i.Before(ctx =>
                {
                    Assert.AreEqual($"{InterceptionTarget.Get}", ctx.Target);
                    Assert.IsInstanceOf<ObjectReferenceInterceptionContext>(ctx);

                    var orCtx = (ObjectReferenceInterceptionContext)ctx;
                    Assert.AreEqual(Id("id", "model"), orCtx.TargetReference);
                    Assert.AreEqual("model", orCtx.Model.Id);

                    hit = true;
                }
            )))
        );

        invoker.InvokeGet(testing, Id("id", "model"));

        Assert.IsTrue(hit);
    }

    [Test]
    public void An_interceptor_can_be_defined_to_both_methods()
    {
        ModelsAre(Model());
        ObjectsAre(Object(Id("id")));

        var hitCount = 0;

        var testing = Build(ic => ic.FromBasic()
            .Interceptors.Add(c => c.Interceptor(i => i.Before(() => hitCount++)))
        );

        var _ = testing.ApplicationModel;
        invoker.InvokeGet(testing, Id("id"));

        Assert.AreEqual(2, hitCount);
    }

    [Test]
    public void An_interceptor_can_be_defined_for_a_specific_method()
    {
        ModelsAre(Model());
        ObjectsAre(Object(Id("id")));

        var hitCount = 0;

        var testing = Build(ic => ic.FromBasic()
            .Interceptors.Add(c => c
                .Interceptor(i => i.Before(() => hitCount++))
                .When(InterceptionTarget.Get))
        );

        var _ = testing.ApplicationModel;
        invoker.InvokeGet(testing, Id("id"));

        Assert.AreEqual(1, hitCount);
    }
}
