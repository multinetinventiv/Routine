﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Routine.Api.Template;

namespace Routine.Test.Api.Template
{
	[TestFixture]
	public class ClientApiViewModelFeatureTest : ClientApiFeatureTestBase
	{
		protected override string DefaultNamespace { get { return "Routine.Test.Api.Template.ClientApiViewModelFeatureTest_Generated"; } }
		
		[Test]
		public void View_models_are_rendered_as_interfaces_with_concrete_classes()
		{
			ModelsAre(
				Model("TestView").Name("TestView").IsView("TestClass")
				.Operation("Dummy", true),
				Model("TestClass").Name("TestClass")
				.ViewModelIds("TestView")
			);

			var assembly = Generator().Generate(DefaultTestTemplate);

			var iTestView = GetRenderedType(assembly, "TestView");
			var testView = GetRenderedType(assembly, "TestViewImpl");

			Assert.IsNotNull(iTestView);
			Assert.IsNotNull(testView);

			Assert.IsTrue(iTestView.IsAssignableFrom(testView));
		}

		[Test]
		public void Potential_actual_interface_name_and_client_interface_name_conflict_is_resolved()
		{
			ModelsAre(
				Model("ITestClass").Name("ITestClass").IsView("TestClass")
				.Operation("Dummy", true),
				Model("TestClass").Name("TestClass")
				.ViewModelIds("ITestClass")
				.Operation("Dummy", true)
			);

			var assembly = Generator().Generate(DefaultTestTemplate);

			var iITestClass = GetRenderedType(assembly, "ITestClass");
			var iTestClassImpl = GetRenderedType(assembly, "ITestClassImpl");

			var iTestClass = GetRenderedType(assembly, "TestClass");
			var testClass = GetRenderedType(assembly, "TestClassImpl");

			Assert.IsTrue(iITestClass.IsAssignableFrom(iTestClassImpl));
			Assert.IsTrue(iTestClass.IsAssignableFrom(testClass));
		}

		[Test]
		public void View_model_instances_keep_actual_model_to_perform_operations()
		{
			ModelsAre(
				Model("ITestClass").Name("ITestClass").IsView("TestClass")
				.Operation("Operation", "s-string"),
				Model("TestClass").Name("TestClass")
				.ViewModelIds("ITestClass"),
				Model("s-string").IsValue()
			);

			ObjectsAre(
				Object(Id("obj", "TestClass", "ITestClass"))
			);

			When(Id("obj", "TestClass", "ITestClass"))
				.Performs("Operation")
				.Returns(Result(Id("success", "s-string")));

			var testing = Generator(c => c
				.TypeIsRendered.Set(false, rt => rt.Id == "s-string")
				.ReferencedType.Set(typeof(string), rt => rt.Id == "s-string")
			);

			var assembly = testing.Generate(DefaultTestTemplate);

			var iITestClass = GetRenderedType(assembly, "ITestClass");

			var operation = iITestClass.GetMethod("Operation");

			var obj = CreateInstance(GetRenderedType(assembly, "ITestClassImpl"), "obj", "TestClass", "ITestClass");

			var actual = operation.Invoke(obj, new object[0]);

			Assert.AreEqual("success", actual);
		}

		[Test][Ignore]
		public void Static_instance_support()
		{
			Assert.Fail("to be designed");
		}

		/// <summary>
		/// Accessing static instance of a view model requires its actual model to be given along with the id.
		/// This is not yet supported due to the complexity of inter client api reference feature. 
		/// Actual model could've been rendered in another client api dll. But static instance getter is implemented
		/// in the dll that contains view model.
		/// </summary>
		[Test]
		public void There_is_no_instance_support_for_view_models()
		{
			ModelsAre(
				Model("ITestClass").Name("ITestClass").IsView("TestClass")
				.Operation("Operation", true)
				.StaticInstanceId("instance1", "TestClass")
				.StaticInstanceId("instance2", "TestClass"),
				Model("TestClass").Name("TestClass")
				.ViewModelIds("ITestClass")
			);

			ObjectsAre(
				Object(Id("instance1", "TestClass", "ITestClass")),
				Object(Id("instance2", "TestClass", "ITestClass"))
			);

			var assembly = Generator().Generate(new ClientApiTemplate("TestApi"));

			var iTestApi = GetRenderedType(assembly, "ITestApi");
			var iITestClass = GetRenderedType(assembly, "ITestClass");
			var get = iTestApi.GetMethod("Get", new[] { typeof(Type), typeof(string) });

			var testApiObj = Activator.CreateInstance(GetRenderedType(assembly, "TestApi"), testingRapplication);

			try
			{
				get.Invoke(testApiObj, new object[] { iITestClass, "instance1" });
				Assert.Fail("exception not thrown");
			}
			catch (TargetInvocationException ex)
			{
				Assert.AreEqual("GetInstanceException", ex.InnerException.GetType().Name);
				Assert.IsTrue(ex.InnerException.Message.Contains(iITestClass.FullName));
			}
		}

		[Test]
		public void Singleton_support()
		{
			ModelsAre(
				Model("ITestClass").Name("ITestClass").IsView("TestClass")
				.Operation("Operation", true)
				.StaticInstanceId("instance", "TestClass"),
				Model("TestClass").Name("TestClass")
				.ViewModelIds("ITestClass")
			);

			ObjectsAre(
				Object(Id("instance", "TestClass", "ITestClass")).Display("instance")
			);

			var assembly = Generator().Generate(new ClientApiTemplate("TestApi"));

			var iTestApi = GetRenderedType(assembly, "ITestApi");
			var iITestClass = GetRenderedType(assembly, "ITestClass");
			var get = iTestApi.GetMethod("Get", new[] { typeof(Type) });

			var testApiObj = Activator.CreateInstance(GetRenderedType(assembly, "TestApi"), testingRapplication);

			var instance = get.Invoke(testApiObj, new object[] { iITestClass });

			Assert.AreEqual("instance", instance.ToString());
		}

		[Test]
		public void Client_type_interfaces_have_conversion_methods_for_their_view_models()
		{
			ModelsAre(
				Model("ITestClass").Name("ITestClass").IsView("TestClass")
				.Operation("Operation", true),
				Model("TestClass").Name("TestClass").ViewModelIds("ITestClass")
				.Operation("Operation", true)
			);

			ObjectsAre(
				Object(Id("instance", "TestClass")).Display("instance"),
				Object(Id("instance", "TestClass", "ITestClass")).Display("view instance")
			);

			var assembly = Generator().Generate(new ClientApiTemplate("TestApi"));

			var testClass = GetRenderedType(assembly, "TestClassImpl");
			var iTestClass = GetRenderedType(assembly, "TestClass");
			var iITestClass = GetRenderedType(assembly, "ITestClass");

			var asIITestClass = iTestClass.GetMethod("AsITestClass");

			Assert.IsNotNull(asIITestClass);
			Assert.AreEqual(iITestClass, asIITestClass.ReturnType);
			Assert.AreEqual(0, asIITestClass.GetParameters().Length);

			var instance = CreateInstance(testClass, "instance", "TestClass");
			var viewInstance = asIITestClass.Invoke(instance, new object[0]);

			Assert.AreEqual("view instance", viewInstance.ToString());
		}

		[Test]
		public void Client_type_interfaces_have_is_methods_for_their_actual_models()
		{
			ModelsAre(
				Model("ITestClass").Name("ITestClass").IsView("TestClass", "TestClass2")
				.Operation("Operation", true),
				Model("TestClass").Name("TestClass").ViewModelIds("ITestClass")
				.Operation("Operation", true),
				Model("TestClass2").Name("TestClass2").ViewModelIds("ITestClass")
				.Operation("Operation", true)
			);

			ObjectsAre(
				Object(Id("instance", "TestClass")).Display("instance"),
				Object(Id("instance", "TestClass", "ITestClass")).Display("view instance"),
				Object(Id("instance2", "TestClass2")).Display("instance 2"),
				Object(Id("instance2", "TestClass2", "ITestClass")).Display("view instance 2")
			);

			var assembly = Generator().Generate(new ClientApiTemplate("TestApi"));

			var iTestClass = GetRenderedType(assembly, "ITestClassImpl");
			var iITestClass = GetRenderedType(assembly, "ITestClass");

			var isITestClass = iITestClass.GetMethod("IsTestClass");
			Assert.IsNotNull(isITestClass);
			Assert.AreEqual(typeof(bool), isITestClass.ReturnType);
			Assert.AreEqual(0, isITestClass.GetParameters().Length);

			var isITestClass2 = iITestClass.GetMethod("IsTestClass2");
			Assert.IsNotNull(isITestClass2);
			Assert.AreEqual(typeof(bool), isITestClass2.ReturnType);
			Assert.AreEqual(0, isITestClass2.GetParameters().Length);

			var instance = CreateInstance(iTestClass, "instance", "TestClass", "ITestClass");
			var instance2 = CreateInstance(iTestClass, "instance2", "TestClass2", "ITestClass");

			Assert.IsTrue((bool)isITestClass.Invoke(instance, new object[0]));
			Assert.IsFalse((bool)isITestClass2.Invoke(instance, new object[0]));

			Assert.IsFalse((bool)isITestClass.Invoke(instance2, new object[0]));
			Assert.IsTrue((bool)isITestClass2.Invoke(instance2, new object[0]));
		}

		[Test][Ignore]
		public void Struct_support()
		{
			//When an interface parameter has struct implementation, client structs should be able to convert to interface parameter
			Assert.Fail();
		}

		protected override void Referenced_client_api_support_case()
		{
			ModelsAre(
				Model("Module2-ITestClass1").Module("Module2").Name("ITestClass1").IsView("Module1-TestClass1")
				.Operation("Dummy", true),
				Model("Module2-ITestClass2").Module("Module2").Name("ITestClass2").IsView("Module2-TestClass2")
				.Operation("Dummy", true),
				Model("Module2-TestClass2").Module("Module2").Name("TestClass2").ViewModelIds("Module2-ITestClass2"),
				Model("Module1-TestClass1").Module("Module1").Name("TestClass1").ViewModelIds("Module2-ITestClass1")
				.Operation("Operation", "Module2-ITestClass2", PModel("arg1", "Module2-ITestClass2"))
			);

			ObjectsAre(
				Object(Id("test2_1", "Module2-TestClass2", "Module2-ITestClass2")).Display("test2_1_value"),
				Object(Id("test2_2", "Module2-TestClass2", "Module2-ITestClass2")).Display("test2_2_value"),
				Object(Id("obj", "Module1-TestClass1")),
				Object(Id("obj", "Module1-TestClass1", "Module2-ITestClass1")).Display("view obj")
			);

			When(Id("obj", "Module1-TestClass1"))
				.Performs("Operation", p =>
					p["arg1"].Values[0].ModelId == "Module2-TestClass2" &&
					p["arg1"].Values[0].Id == "test2_1"
				).Returns(Result(Id("test2_2", "Module2-TestClass2", "Module2-ITestClass2"))
			);

			var otherApiGenerator = Generator(c => c
				.FriendlyAssemblyNames.Add(DefaultNamespace + ".Module1")
				.DefaultNamespace.Set(DefaultNamespace + ".Module2")
				.InMemory.Set(false)
				.TypeIsRendered.Set(true, m => m.Module == "Module2")
				.TypeIsRendered.Set(false)
			);

			var otherAssembly = otherApiGenerator.Generate(DefaultTestTemplate);

			var testing = Generator(c => c
				.DefaultNamespace.Set(DefaultNamespace + ".Module1")
				.TypeIsRendered.Set(true, m => m.Module == "Module1")
				.TypeIsRendered.Set(false)
				.InMemory.Set(false)
				.Use(p => p.ReferenceOtherClientApiPattern(otherAssembly, otherApiGenerator.Context))
			).AddReference(otherAssembly);

			var assembly = testing.Generate(DefaultTestTemplate);

			var iTestClass1 = GetRenderedType(assembly, "TestClass1");
			var iITestClass1 = GetRenderedType(otherAssembly, "ITestClass1");

			var asIITestClass1 = iTestClass1.GetMethod("AsITestClass1");
			
			Assert.IsNotNull(asIITestClass1);
			Assert.AreEqual(iITestClass1, asIITestClass1.ReturnType);
			Assert.AreEqual(0, asIITestClass1.GetParameters().Length);

			var operation = iTestClass1.GetMethod("Operation");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClass1Impl"), "obj", "Module1-TestClass1");
			var testObj2_1 = CreateInstance(GetRenderedType(otherAssembly, "ITestClass2Impl"), "test2_1", "Module2-TestClass2", "Module2-ITestClass2");
			var testObj2_2 = CreateInstance(GetRenderedType(otherAssembly, "ITestClass2Impl"), "test2_2", "Module2-TestClass2", "Module2-ITestClass2");

			var actual = operation.Invoke(obj, new[] { testObj2_1 });
			Assert.AreEqual(testObj2_2, actual);

			var viewObj = asIITestClass1.Invoke(obj, new object[0]);
			Assert.AreEqual("view obj", viewObj.ToString());
		}

		protected override void List_input_and_output_case()
		{
			ModelsAre(
				Model("ITestClass1").Name("ITestClass1").IsView("TestClass1")
				.Operation("Dummy", true),
				Model("TestClass1").Name("TestClass1").ViewModelIds("ITestClass1"),
				Model("TestClass2").Name("TestClass2")
				.Operation("ListOperation", "ITestClass1", true, PModel("arg1", "ITestClass1", true))
			);

			ObjectsAre(
				Object(Id("obj", "TestClass2")),
				Object(Id("test1", "TestClass1", "ITestClass1")),
				Object(Id("test2", "TestClass1", "ITestClass1")),
				Object(Id("test3", "TestClass1", "ITestClass1")),
				Object(Id("test4", "TestClass1", "ITestClass1"))
			);

			When(Id("obj", "TestClass2")).Performs("ListOperation", p =>
				p["arg1"].IsList &&
				p["arg1"].Values[0].ModelId == "TestClass1" &&
				p["arg1"].Values[0].Id == "test1" &&
				p["arg1"].Values[1].ModelId == "TestClass1" &&
				p["arg1"].Values[1].Id == "test2"
			).Returns(Result(Id("test3", "TestClass1", "ITestClass1"), Id("test4", "TestClass1", "ITestClass1")));

			var assembly = Generator().Generate(DefaultTestTemplate);

			var iTestClass2 = GetRenderedType(assembly, "TestClass2");
			var iITestClass1 = GetRenderedType(assembly, "ITestClass1");

			var listOperation = iTestClass2.GetMethod("ListOperation");

			var test1 = CreateInstance(GetRenderedType(assembly, "ITestClass1Impl"), "test1", "TestClass1", "ITestClass1");
			var test2 = CreateInstance(GetRenderedType(assembly, "ITestClass1Impl"), "test2", "TestClass1", "ITestClass1");
			var test3 = CreateInstance(GetRenderedType(assembly, "ITestClass1Impl"), "test3", "TestClass1", "ITestClass1");
			var test4 = CreateInstance(GetRenderedType(assembly, "ITestClass1Impl"), "test4", "TestClass1", "ITestClass1");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClass2Impl"), "obj", "TestClass2");

			var arg1 = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(iITestClass1));
			arg1.Add(test1);
			arg1.Add(test2);

			var actual = (IList)listOperation.Invoke(obj, new object[] { arg1 });

			Assert.AreEqual(test3, actual[0]);
			Assert.AreEqual(test4, actual[1]);
		}

		protected override void Attribute_case()
		{
			Assert.Pass("This feature has nothing to do with attribute case");
		}
	}
}