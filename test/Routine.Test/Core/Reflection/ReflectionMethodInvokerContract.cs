﻿using NUnit.Framework;
using Routine.Core.Reflection;
using System.Threading.Tasks;
using System;

namespace Routine.Test.Core.Reflection;

public abstract class ReflectionMethodInvokerContract
{
    #region InvokerFor Helper

    protected ReflectionMethodInvoker InvokerFor(string method) => new(GetType().GetMethod(method));

    #endregion

    protected abstract object Invoke(IMethodInvoker invoker, object target, params object[] args);

    public string Test(string response) => response;

    [Test]
    public void It_uses_reflection_to_call_a_method()
    {
        var testing = InvokerFor(nameof(Test));

        var actual = Invoke(testing, this, "test");

        Assert.AreEqual("test", actual);
    }

    [Test]
    public void It_supports_constructors()
    {
        var testing = new ReflectionMethodInvoker(typeof(string).GetConstructor(new[] { typeof(char), typeof(int) }));

        var actual = Invoke(testing, null, 'x', 3); // new string('x', 3)

        Assert.AreEqual("xxx", actual);
    }

    [Test]
    public void Given_a_non_static_method_invoker__when_target_is_null__throws_null_reference_exception()
    {
        var testing = InvokerFor(nameof(Test));

        Assert.Throws<NullReferenceException>(() => Invoke(testing, null, string.Empty));
    }

    public class CustomException : Exception { public CustomException(string message) : base(message) { } }
    public void Throw(CustomException ex) => throw ex;
    public async Task ThrowAsync(CustomException ex) { await Task.Delay(10); throw ex; }

    [TestCase(nameof(Throw))]
    [TestCase(nameof(ThrowAsync))]
    public void When_exception_occurs_invoker_should_throw_the_actual_exception(string method)
    {
        var expected = new CustomException("message");

        var testing = InvokerFor(method);

        try
        {
            Invoke(testing, this, expected);
            Assert.Fail("exception not thrown");
        }
        catch (Exception actual)
        {
            Assert.AreSame(expected, actual);
        }
    }

    [TestCase(nameof(Throw))]
    [TestCase(nameof(ThrowAsync))]
    public void When_throwing_actual_exception_it_preserves_stack_trace(string method)
    {
        var expected = new CustomException("message");

        var testing = InvokerFor(method);

        try
        {
            Invoke(testing, this, expected);
            Assert.Fail("exception not thrown");
        }
        catch (CustomException actual)
        {
            Console.WriteLine(actual.StackTrace);

            Assert.IsTrue(actual.StackTrace.Contains($"{nameof(ReflectionMethodInvokerContract)}.{method}"), actual.StackTrace);
        }
    }
}
