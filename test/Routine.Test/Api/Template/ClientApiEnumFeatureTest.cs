﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Routine.Api.Template;

namespace Routine.Test.Api.Template
{
	[TestFixture]
	public class ClientApiEnumFeatureTest : ClientApiFeatureTestBase
	{
		protected override string DefaultNamespace { get { return "Routine.Test.Api.Template.ClientApiEnumFeatureTest_Generated"; } }

		[Test]
		public void Models_with_no_data__operation_and_initializer_but_static_instances_are_rendered_as_enum()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1", "2")
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1"),
				Object(Id("2", "TestEnum")).Display("Instance2")
			);

			var testEnum = GetRenderedType("TestEnum");

			Assert.IsNotNull(testEnum);
			Assert.IsTrue(testEnum.IsEnum);
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "Instance1"));
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "Instance2"));
		}

		[Test]
		public void Enum_member_names_are_generated_replacing_static_instance_values_camel_case_with_an_upper_case_initial()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1", "2", "3", "4", "5")
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("instance value 1"),
				Object(Id("2", "TestEnum")).Display("instance Value2"),
				Object(Id("3", "TestEnum")).Display("instancevalue3"),
				Object(Id("4", "TestEnum")).Display("4th instance"),
				Object(Id("5", "TestEnum")).Display("5.!@ instance")
			);

			var testEnum = GetRenderedType("TestEnum");

			Assert.IsNotNull(testEnum);
			Assert.IsTrue(testEnum.IsEnum);
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "InstanceValue1"));
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "InstanceValue2"));
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "Instancevalue3"));
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "_4thInstance"));
			Assert.IsTrue(testEnum.GetEnumNames().Any(val => val == "_5___Instance"));
		}

		[Test]
		public void Enum_values_starts_from_1_so_that_it_enables_default_value_to_be_sent()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("10", "20")
			);

			ObjectsAre(
				Object(Id("10", "TestEnum")).Display("Instance1"),
				Object(Id("20", "TestEnum")).Display("Instance2")
			);

			var testEnum = GetRenderedType("TestEnum");

			Assert.IsFalse(testEnum.GetEnumValues().Cast<int>().Contains(0));
			Assert.IsTrue(testEnum.GetEnumValues().Cast<int>().Contains(1));
			Assert.IsTrue(testEnum.GetEnumValues().Cast<int>().Contains(2));
		}

		[Test]
		public void Enums_are_converted_to_robject_using_internal_converter_classes()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1"),
				Model("TestClass").Name("TestClass")
				.Operation("EnumOperation", "s-string", PModel("arg1", "TestEnum")),
				Model("s-string").IsValue()
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1"),
				Object(Id("obj", "TestClass"))
			);

			When(Id("obj", "TestClass")).Performs("EnumOperation", p =>
				p["arg1"].Values[0].ModelId == "TestEnum" &&
				p["arg1"].Values[0].Id == "1"
			).Returns(Result(Id("success", "s-string")));

			var assembly = Generator(c => c
				.Use(p => p.ReferencedTypesPattern(t => "s-string", typeof(string)))
				.Use(p => p.ParseableValueTypePattern())).Generate(DefaultTestTemplate);

			var iTestClass = GetRenderedType(assembly, "TestClass");
			var testEnum = GetRenderedType(assembly, "TestEnum");

			var enumOperation = iTestClass.GetMethod("EnumOperation");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClassImpl"), "obj", "TestClass");
			var instance1 = Enum.GetValues(testEnum).GetValue(0);

			var actual = enumOperation.Invoke(obj, new[] { instance1 });

			Assert.AreEqual("success", actual);
		}

		[Test]
		public void When_default_enum_value_is_sent__service_receives_null()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1"),
				Model("TestClass").Name("TestClass")
				.Operation("EnumOperation", "s-string", PModel("arg1", "TestEnum")),
				Model("s-string").IsValue()
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1"),
				Object(Id("obj", "TestClass"))
			);

			When(Id("obj", "TestClass")).Performs("EnumOperation", p =>
				p["arg1"].Values[0] == null
			).Returns(Result(Id("success", "s-string")));

			var assembly = Generator(c => c
				.Use(p => p.ReferencedTypesPattern(t => "s-string", typeof(string)))
				.Use(p => p.ParseableValueTypePattern())).Generate(DefaultTestTemplate);

			var iTestClass = GetRenderedType(assembly, "TestClass");
			var testEnum = GetRenderedType(assembly, "TestEnum");

			var enumOperation = iTestClass.GetMethod("EnumOperation");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClassImpl"), "obj", "TestClass");
			var @default = Enum.ToObject(testEnum, 0);

			var actual = enumOperation.Invoke(obj, new[] { @default });

			Assert.AreEqual("success", actual);
		}

		[Test]
		public void Enum_robjects_are_converted_to_enum_member_using_internal_converter_classes()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1"),
				Model("TestClass").Name("TestClass")
				.Operation("EnumResultOperation", "TestEnum")
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1"),
				Object(Id("obj", "TestClass"))
			);

			When(Id("obj", "TestClass")).Performs("EnumResultOperation").Returns(Result(Id("1", "TestEnum")));

			var assembly = Generator().Generate(DefaultTestTemplate);

			var iTestClass = GetRenderedType(assembly, "TestClass");
			var testEnum = GetRenderedType(assembly, "TestEnum");

			var enumResultOperation = iTestClass.GetMethod("EnumResultOperation");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClassImpl"), "obj", "TestClass");

			var actual = enumResultOperation.Invoke(obj, new object[0]);

			var instance1 = Enum.GetValues(testEnum).GetValue(0);

			Assert.AreEqual(instance1, actual);
		}

		[Test]
		public void Singleton_and_static_instance_getters_does_not_include_enum_types()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1")
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1"),
				Object(Id("obj", "TestClass"))
			);

			var assembly = Generator().Generate(new ClientApiTemplate("TestApi"));

			var iTestApi = GetRenderedType(assembly, "ITestApi");
			var testEnum = GetRenderedType(assembly, "TestEnum");

			var getStaticInstance = iTestApi.GetMethod("Get", new[] { typeof(Type), typeof(string) });

			var testApiObj = Activator.CreateInstance(GetRenderedType(assembly, "TestApi"), testingRapplication);

			try
			{
				getStaticInstance.Invoke(testApiObj, new object[] { testEnum, "1" });
				Assert.Fail("exception not thrown");
			}
			catch (TargetInvocationException ex)
			{
				Assert.AreEqual("GetInstanceException", ex.InnerException.GetType().Name);
				Assert.IsTrue(ex.InnerException.Message.Contains(testEnum.FullName));
			}

			var getSingleton = iTestApi.GetMethod("Get", new[] { typeof(Type) });
			try
			{
				getSingleton.Invoke(testApiObj, new object[] { testEnum });
				Assert.Fail("exception not thrown");
			}
			catch (TargetInvocationException ex)
			{
				Assert.AreEqual("SingletonException", ex.InnerException.GetType().Name);
				Assert.IsTrue(ex.InnerException.Message.Contains(testEnum.FullName));
			}
		}

		protected override void Referenced_client_api_support_case()
		{
			ModelsAre(
				Model("Module2-TestEnum").Module("Module2").Name("TestEnum")
				.StaticInstanceIds("1", "2"),
				Model("Module1-TestClass").Module("Module1").Name("TestClass")
				.Operation("EnumOperation", "Module2-TestEnum", PModel("arg1", "Module2-TestEnum"))
			);

			ObjectsAre(
				Object(Id("1", "Module2-TestEnum")).Display("instance 1"),
				Object(Id("2", "Module2-TestEnum")).Display("instance 2"),
				Object(Id("obj", "Module1-TestClass"))
			);

			When(Id("obj", "Module1-TestClass"))
				.Performs("EnumOperation", p =>
					p["arg1"].Values[0].ModelId == "Module2-TestEnum" &&
					p["arg1"].Values[0].Id == "1"
				).Returns(Result(Id("2", "Module2-TestEnum"))
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

			var iTestClass = GetRenderedType(assembly, "TestClass");
			var testEnum = GetRenderedType(otherAssembly, "TestEnum");

			var enumOperation = iTestClass.GetMethod("EnumOperation");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClassImpl"), "obj", "Module1-TestClass");

			var arg1 = Enum.GetValues(testEnum).GetValue(0);

			var actual = enumOperation.Invoke(obj, new[] { arg1 });

			var expected = Enum.GetValues(testEnum).GetValue(1);

			Assert.AreEqual(expected, actual);
		}

		protected override void List_input_and_output_case()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1", "2", "3", "4"),
				Model("TestClass").Name("TestClass")
				.Operation("EnumOperation", "TestEnum", true, PModel("arg1", "TestEnum", true))
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1"),
				Object(Id("2", "TestEnum")).Display("Instance2"),
				Object(Id("3", "TestEnum")).Display("Instance3"),
				Object(Id("4", "TestEnum")).Display("Instance4"),
				Object(Id("obj", "TestClass"))
			);

			When(Id("obj", "TestClass")).Performs("EnumOperation", p =>
				p["arg1"].IsList &&
				p["arg1"].Values[0].ModelId == "TestEnum" &&
				p["arg1"].Values[0].Id == "1" &&
				p["arg1"].Values[1].ModelId == "TestEnum" &&
				p["arg1"].Values[1].Id == "2"
			).Returns(Result(Id("3", "TestEnum"), Id("4", "TestEnum")));

			var assembly = Generator().Generate(DefaultTestTemplate);

			var iTestClass = GetRenderedType(assembly, "TestClass");
			var testEnum = GetRenderedType(assembly, "TestEnum");

			var enumOperation = iTestClass.GetMethod("EnumOperation");

			var obj = CreateInstance(GetRenderedType(assembly, "TestClassImpl"), "obj", "TestClass");

			var arg1 = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(testEnum));
			arg1.Add(Enum.GetValues(testEnum).GetValue(0));
			arg1.Add(Enum.GetValues(testEnum).GetValue(1));

			var actual = (IList)enumOperation.Invoke(obj, new object[] { arg1 });

			var instance3 = Enum.GetValues(testEnum).GetValue(2);
			var instance4 = Enum.GetValues(testEnum).GetValue(3);

			Assert.AreEqual(instance3, actual[0]);
			Assert.AreEqual(instance4, actual[1]);
		}

		public class CustomAttribute : Attribute { }

		protected override void Attribute_case()
		{
			ModelsAre(
				Model("TestEnum").Name("TestEnum")
				.StaticInstanceIds("1")
			);

			ObjectsAre(
				Object(Id("1", "TestEnum")).Display("Instance1")
			);

			var assembly = Generator(c => c
				.RenderedTypeAttributes.Add(typeof(CustomAttribute)))
				.AddReference<CustomAttribute>()
				.Generate(DefaultTestTemplate)
				;

			var testEnum = GetRenderedType(assembly, "TestEnum");

			Assert.IsTrue(Attribute.IsDefined(testEnum, typeof(CustomAttribute)));
		}
	}
}