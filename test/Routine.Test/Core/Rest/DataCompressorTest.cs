﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using NUnit.Framework;
using Routine.Core;
using Routine.Core.Rest;

namespace Routine.Test.Core.Rest
{
	[TestFixture]
	public class DataCompressorTest : CoreTestBase
	{
		#region SetUp & Helpers

		private string Serialize(object @object)
		{
			return new JavaScriptSerializerAdapter(new JavaScriptSerializer()).Serialize(@object);
		}

		private object Deserialize(string json)
		{
			return new JavaScriptSerializerAdapter(new JavaScriptSerializer()).DeserializeObject(json);
		}

		private DataCompressor Compressor() { return Compressor(null); }
		private DataCompressor Compressor(string knownViewModelId)
		{
			return new DataCompressor(ApplicationModel, knownViewModelId);
		}

		private ApplicationModel ApplicationModel { get { return new ApplicationModel { Models = objectModelDictionary.Values.ToList() }; } }

		#endregion

		[Test]
		public void When_compressing__ReferenceData_is_serialized_as_null_when_it_is_null()
		{
			var actual = Serialize(Compressor().Compress((ReferenceData)null));

			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_is_an_object_with_id_and_model_id()
		{
			ModelsAre(Model("mid"));

			var reference = new ReferenceData { Id = "id", ModelId = "mid" };

			var actual = Serialize(Compressor().Compress(reference));

			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\"}", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_consists_of_view_model_id_if_exists()
		{
			ModelsAre(Model("mid"), Model("vmid"));

			var reference = new ReferenceData { Id = "id", ModelId = "mid", ViewModelId = "vmid" };

			var actual = Serialize(Compressor().Compress(reference));

			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\",\"ViewModelId\":\"vmid\"}", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_is_only_id_if_model_id_is_known()
		{
			ModelsAre(Model("mid"));

			var reference = new ReferenceData { Id = "id", ModelId = "mid" };

			var actual = Serialize(Compressor("mid").Compress(reference));

			Assert.AreEqual("\"id\"", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_is_empty_string_if_Id_is_empty_string()
		{
			ModelsAre(Model("mid"));

			var reference = new ReferenceData { Id = "", ModelId = "mid" };

			var actual = Serialize(Compressor("mid").Compress(reference));

			Assert.AreEqual("\"\"", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_is_null_if_given_id_is_null()
		{
			ModelsAre(Model("mid"));

			var reference = new ReferenceData { Id = null, ModelId = "mid" };

			var actual = Serialize(Compressor("mid").Compress(reference));

			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_does_not_consist_of_view_model_id_if_it_is_known()
		{
			ModelsAre(
				Model("mid").ViewModelIds("vmid"),
				Model("vmid").IsView("mid")
			);

			var reference = new ReferenceData { Id = "id", ModelId = "mid", ViewModelId = "vmid" };

			var actual = Serialize(Compressor("vmid").Compress(reference));

			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\"}", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_consists_of_only_id_if_model_id_and_view_model_id_are_the_same_and_it_is_known()
		{
			ModelsAre(Model("mid"));

			var reference = new ReferenceData { Id = "id", ModelId = "mid", ViewModelId = "mid" };

			var actual = Serialize(Compressor("mid").Compress(reference));

			Assert.AreEqual("\"id\"", actual);
		}

		[Test]
		public void When_compressing__ReferenceData_does_not_consist_of_view_model_id_if_it_is_the_same_as_model_id()
		{
			ModelsAre(Model("mid"));

			var reference = new ReferenceData { Id = "id", ModelId = "mid", ViewModelId = "mid" };

			var actual = Serialize(Compressor().Compress(reference));

			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\"}", actual);
		}

		[Test]
		public void When_compressing__ObjectData_is_serialized_as_null_when_its_reference_is_null()
		{
			var actual = Serialize(Compressor().Compress((ObjectData)null));

			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__ObjectData_serializes_id__model_id_and_display_when_it_does_not_have_any_data()
		{
			ModelsAre(Model("mid"));

			var data = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = "value"
			};

			var actual = Serialize(Compressor().Compress(data));

			Assert.AreEqual("{\"Id\":\"id\",\"Display\":\"value\",\"ModelId\":\"mid\"}", actual);
		}

		[Test]
		public void When_compressing__ObjectData_serializes_only_id_and_display_when_it_does_not_have_any_data_and_model_id_is_known()
		{
			ModelsAre(Model("mid"));

			var data = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = "value"
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("{\"Id\":\"id\",\"Display\":\"value\"}", actual);
		}

		[TestCase("id")]
		[TestCase("")]
		[TestCase(null)]
		public void When_compressing__given_that_ObjectData_does_not_have_any_data__it_serializes_only_id_when_display_is_the_same_with_id_or_is_null_or_empty_and_model_id_is_known(string display)
		{
			ModelsAre(Model("mid"));

			var data = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = display
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("\"id\"", actual);
		}

		[Test]
		public void When_compressing__VariableData_serializes_directly_its_only_ObjectData_when_it_is_not_a_list()
		{
			ModelsAre(Model("mid"));

			var data = new VariableData
			{
				IsList = false,
				Values = new List<ObjectData>
				{
					new ObjectData
					{
						Id = "id",
						ModelId = "mid",
						Display = "value"
					}
				}
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("{\"Id\":\"id\",\"Display\":\"value\"}", actual);
		}

		[Test]
		public void When_compressing__VariableData_is_serialized_as_null_when_it_is_not_a_list_and_it_does_not_have_any_value_or_only_have_null_value()
		{
			ModelsAre(Model("mid"));

			var data = new VariableData
			{
				IsList = false,
				Values = new List<ObjectData>()
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("null", actual);

			data.Values.Add(null);

			actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__VariableData_is_serialized_as_List_when_it_is_list()
		{
			ModelsAre(
				Model("mid").ViewModelIds("vmid"),
				Model("mid2").ViewModelIds("vmid"),
				Model("vmid").IsView("mid", "mid2")
			);

			var data = new VariableData { IsList = true };
			data.Values.Add(new ObjectData { Id = "id", ModelId = "mid", Display = "value" });
			data.Values.Add(new ObjectData { Id = "id2", ModelId = "mid2", Display = "value2" });

			var actual = Serialize(Compressor("vmid").Compress(data));

			Assert.AreEqual(
				"[" +
					"{" +
						"\"Id\":\"id\"," +
						"\"Display\":\"value\"," +
						"\"ModelId\":\"mid\"" +
					"}," +
					"{" +
						"\"Id\":\"id2\"," +
						"\"Display\":\"value2\"," +
						"\"ModelId\":\"mid2\"" +
					"}" +
				"]", actual);
		}

		[Test]
		public void When_compressing__VariableData_is_serialized_as_an_empty_list_when_it_is_an_empty_list()
		{
			var data = new VariableData { IsList = true };

			var actual = Serialize(Compressor().Compress(data));

			Assert.AreEqual("[]", actual);
		}

		[Test]
		public void When_compressing__VariableData_still_contains_null_references_when_it_is_serialized_as_list()
		{
			ModelsAre(Model("mid"));

			var data = new VariableData { IsList = true };
			data.Values.Add(new ObjectData { Id = "id", ModelId = "mid" });
			data.Values.Add(null);

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("[\"id\",null]", actual);
		}

		[Test]
		public void When_compressing__ObjectData_serializes_its_data_in_a_separate_dictionary()
		{
			ModelsAre(
				Model("mid")
				.Data("mmid1", "mid2")
				.Data("mmid2", "mid2"),
				Model("mid2").IsValue()
			);

			var data = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = "value",
				Data = new Dictionary<string, VariableData>
				{
					{"mmid1", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid1_id",ModelId="mid2"}}}},
					{"mmid2", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid2_id",ModelId="mid2"}}}}
				}
			};

			var actual = Serialize(Compressor().Compress(data));

			Assert.AreEqual(
				"{" +
					"\"Id\":\"id\"," +
					"\"Display\":\"value\"," +
					"\"ModelId\":\"mid\"," +
					"\"Data\":" +
					"{" +
						"\"mmid1\":\"mmid1_id\"," +
						"\"mmid2\":\"mmid2_id\"" +
					"}" +
				"}",
				actual
			);
		}

		[Test]
		public void When_compressing__ObjectData_ignores_nonexisting_data()
		{
			ModelsAre(
				Model("mid")
				.Data("mmid1", "mid2"),
				Model("mid2").IsValue()
			);

			var data = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = "value",
				Data = new Dictionary<string, VariableData>
				{
					{"mmid1", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid1_id",ModelId="mid2"}}}},
					{"mmid2", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid2_id",ModelId="mid2"}}}}
				}
			};

			var actual = Serialize(Compressor().Compress(data));

			Assert.AreEqual(
				"{" +
					"\"Id\":\"id\"," +
					"\"Display\":\"value\"," +
					"\"ModelId\":\"mid\"," +
					"\"Data\":" +
					"{" +
						"\"mmid1\":\"mmid1_id\"" +
					"}" +
				"}",
				actual
			);
		}

		[TestCase("id")]
		[TestCase("value")]
		public void When_compressing__ObjectData_serializes_only_id_and_display_when_model_id_is_known_and_it_has_data(string display)
		{
			ModelsAre(Model("mid").Data("Name", "mid"));

			var data = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = display,
				Data = new Dictionary<string, VariableData> { { "Name", null } }
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("{\"Id\":\"id\",\"Display\":\"" + display + "\",\"Data\":{\"Name\":null}}", actual);
		}

		[Test]
		public void When_compressing__ParameterData_serializes_as_null_when_it_is_null()
		{
			var actual = Serialize(Compressor().Compress((ParameterData)null));

			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__ParameterData_serializes_id_and_model()
		{
			ModelsAre(Model("mid"));

			var data = new ParameterData { Id = "id", ModelId = "mid" };

			var actual = Serialize(Compressor().Compress(data));

			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\"}", actual);
		}

		[Test]
		public void When_compressing__ParameterData_serializes_only_id_when_model_is_known()
		{
			ModelsAre(Model("mid"));

			var data = new ParameterData { Id = "id", ModelId = "mid" };

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("\"id\"", actual);
		}

		[Test]
		public void When_compressing__ParameterData_serializes_id_and_model_id_when_known_model_id_differs()
		{
			ModelsAre(
				Model("mid").ViewModelIds("vmid"),
				Model("vmid").IsView("mid")
			);

			var data = new ParameterData { Id = "id", ModelId = "mid" };

			var actual = Serialize(Compressor("vmid").Compress(data));

			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\"}", actual);
		}

		[Test]
		public void When_compressing__ParameterValueData_it_serializes_directly_ParameterData_when_it_is_not_list()
		{
			ModelsAre(Model("mid"));

			var data = new ParameterValueData
			{
				Values = new List<ParameterData> { new ParameterData { ModelId = "mid", Id = "id" } }
			};

			var actual = Serialize(Compressor("mid").Compress(data));
			Assert.AreEqual("\"id\"", actual);

			actual = Serialize(Compressor().Compress(data));
			Assert.AreEqual("{\"Id\":\"id\",\"ModelId\":\"mid\"}", actual);

			data = new ParameterValueData
			{
				Values = new List<ParameterData> { null }
			};

			actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__ParameterValueData_is_serialized_as_null_when_it_is_not_a_list_and_it_does_not_have_any_value()
		{
			var data = new ParameterValueData();

			var actual = Serialize(Compressor().Compress(data));
			Assert.AreEqual("null", actual);
		}

		[Test]
		public void When_compressing__ParameterValueData_is_serialized_as_List_when_it_is_list()
		{
			ModelsAre(
				Model("mid").ViewModelIds("vmid"),
				Model("mid2").ViewModelIds("vmid"),
				Model("vmid").IsView("mid", "mid2")
			);

			var data = new ParameterValueData { IsList = true };
			data.Values.Add(new ParameterData { Id = "id", ModelId = "mid" });
			data.Values.Add(new ParameterData { Id = "id2", ModelId = "mid2" });

			var actual = Serialize(Compressor("vmid").Compress(data));

			Assert.AreEqual("[{\"Id\":\"id\",\"ModelId\":\"mid\"},{\"Id\":\"id2\",\"ModelId\":\"mid2\"}]", actual);
		}

		[Test]
		public void When_compressing__ParameterValueData_is_serialized_as_an_empty_list_when_it_is_an_empty_list()
		{
			var data = new ParameterValueData { IsList = true };

			var actual = Serialize(Compressor("vmid").Compress(data));

			Assert.AreEqual("[]", actual);
		}

		[Test]
		public void When_compressing__ParameterValueData_still_contains_null_references_when_it_is_serialized_as_list()
		{
			ModelsAre(Model("mid"));

			var data = new ParameterValueData { IsList = true };
			data.Values.Add(new ParameterData { Id = "id", ModelId = "mid" });
			data.Values.Add(null);

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual("[\"id\",null]", actual);
		}

		[Test]
		public void When_compressing__ParameterData_is_serialized_as_dictionary_when_it_has_initialization_parameters()
		{
			ModelsAre(
				Model("mid")
				.ViewModelIds("vmid")
				.Initializer(
					PModel("p1", "mid"),
					PModel("p2", "mid"),
					PModel("p3", "mid", true),
					PModel("p4", "vmid")
				),
				Model("vmid").IsView("mid")
			);

			var data = new ParameterData
			{
				ModelId = "mid",
				InitializationParameters = new Dictionary<string, ParameterValueData>
				{
					{"p1",new ParameterValueData{Values = new List<ParameterData> {new ParameterData {ModelId = "mid", Id = "id1"}}}},
					{"p2",null},
					{
						"p3",
						new ParameterValueData
						{
							IsList = true,
							Values = new List<ParameterData>
							{
								new ParameterData {ModelId = "mid", Id = "id3.1"},
								new ParameterData {ModelId = "mid", Id = "id3.2"}
							}
						}
					},
					{
						"p4",
						new ParameterValueData
						{
							Values = new List<ParameterData>
							{
								new ParameterData
								{
									ModelId = "mid",
									InitializationParameters = new Dictionary<string, ParameterValueData>
									{
										{"p1",new ParameterValueData{Values = new List<ParameterData> {null}}},
										{"p2",new ParameterValueData{Values = new List<ParameterData> {new ParameterData {ModelId = "mid", Id = "id4.id2"}}}}
									}
								}
							}
						}
					}
				}
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual(
				"{" +
					"\"Data\":" +
					"{" +
						"\"p1\":\"id1\"," +
						"\"p2\":null," +
						"\"p3\":[\"id3.1\",\"id3.2\"]," +
						"\"p4\":" +
						"{" +
							"\"ModelId\":\"mid\"," +
							"\"Data\":" +
							"{" +
								"\"p1\":null," +
								"\"p2\":\"id4.id2\"" +
							"}" +
						"}" +
					"}" +
				"}"
				, actual);
		}

		[Test]
		public void When_decompressing__ParameterData_ignores_nonexisting_initialization_parameters()
		{
			ModelsAre(
				Model("mid")
				.ViewModelIds("vmid")
				.Initializer(
					PModel("p1", "mid")
				),
				Model("vmid").IsView("mid")
			);

			var data = new ParameterData
			{
				ModelId = "mid",
				InitializationParameters = new Dictionary<string, ParameterValueData>
				{
					{"p1", new ParameterValueData {Values = new List<ParameterData> {new ParameterData {ModelId = "mid", Id = "id1"}}}},
					{"p2", new ParameterValueData {Values = new List<ParameterData> {new ParameterData {ModelId = "mid", Id = "id2"}}}},
				}
			};

			var actual = Serialize(Compressor("mid").Compress(data));

			Assert.AreEqual(
				"{" +
					"\"Data\":" +
					"{" +
						"\"p1\":\"id1\"" +
					"}" +
				"}"
				, actual);
		}

		[Test]
		public void When_decompressing__ReferenceData_deserializes_id__model_id_and_view_model_id()
		{
			ModelsAre(
				Model("mid").ViewModelIds("vmid"),
				Model("vmid").IsView("mid")
			);

			var actual = Compressor().DecompressReferenceData(Deserialize("{Id:\"id\",ModelId:\"mid\",ViewModelId:\"vmid\"}"));

			Assert.AreEqual(Id("id", "mid", "vmid"), actual);
		}

		[Test]
		public void When_decompressing__ReferenceData_uses_ModelId_as_ViewModelId_when_ViewModelId_is_not_specified()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor().DecompressReferenceData(Deserialize("{Id:\"id\",ModelId:\"mid\"}"));

			Assert.AreEqual(Id("id", "mid", "mid"), actual);
		}

		[Test]
		public void When_decompressing__ReferenceData_deserializes_only_id_when_model_is_known()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor("mid").DecompressReferenceData(Deserialize("\"id\""));

			Assert.AreEqual(Id("id", "mid"), actual);
		}

		[Test]
		public void When_decompressing__ReferenceData_deserializes_empty_id()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor("mid").DecompressReferenceData(Deserialize("\"\""));

			Assert.AreEqual(Id("", "mid"), actual);
		}

		[Test]
		public void When_decompressing__ReferenceData_deserializes_null_when_given_id_is_null()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor("mid").DecompressReferenceData(Deserialize("{\"Id\":null,\"ModelId\":\"mid\"}"));

			Assert.IsNull(actual);
		}

		[Test]
		public void When_decompressing__ObjectReferenceData_throws_ArgumentException_when_only_id_is_given_and_model_id_is_not_known()
		{
			Assert.Throws<ArgumentException>(() => Compressor().DecompressReferenceData(Deserialize("\"id\"")));
		}

		[Test]
		public void When_decompressing__ReferenceData_is_null_when_given_object_is_null()
		{
			var actual = Compressor().DecompressReferenceData(Deserialize("null"));

			Assert.IsNull(actual);
		}

		[Test]
		public void When_decompressing__ObjectData_deserializes_Dictionary()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor().DecompressObjectData(Deserialize("{\"Id\":\"id\",\"Display\":\"value\",\"ModelId\":\"mid\"}"));

			Assert.AreEqual(Object(Id("id", "mid")).Display("value").Build().Item2, actual);
		}

		[Test]
		public void When_decompressing__ObjectData_sets_Display_with_reference_id_if_value_was_not_given()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor().DecompressObjectData(Deserialize("{\"Id\":\"id\",\"ModelId\":\"mid\"}"));

			Assert.AreEqual(Object(Id("id", "mid")).Display("id").Build().Item2, actual);
		}

		[Test]
		public void When_decompressing__ObjectData_deserializes_null_to_null()
		{
			var actual = Compressor().DecompressObjectData(Deserialize("null"));

			Assert.IsNull(actual);
		}

		[Test]
		public void When_decompressing__ObjectData_deserializes_id_string_when_model_id_is_known()
		{
			ModelsAre(Model("mid"));

			var actual = Compressor("mid").DecompressObjectData(Deserialize("\"id\""));

			Assert.AreEqual(Object(Id("id", "mid")).Display("id").Build().Item2, actual);
		}

		[Test]
		public void When_decompressing__ObjectData_throws_ArgumentException_when_only_id_string_is_available_and_model_id_is_not_known()
		{
			Assert.Throws<ArgumentException>(() => Compressor().DecompressObjectData(Deserialize("\"id\"")));
		}

		[Test]
		public void When_decompressing__VariableData_deserializes_list()
		{
			ModelsAre(Model("mid"));

			var expected = new VariableData
			{
				IsList = true,
				Values = new List<ObjectData>{
					new ObjectData
					{
						Id = "id", ModelId = "mid",
						Display = "value"
					}
				}
			};

			var actual = Compressor().DecompressVariableData(Deserialize("[{\"Id\":\"id\",\"Display\":\"value\",\"ModelId\":\"mid\"}]"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__VariableData_deserializes_dictionary_to_a_nonlist_ValueData()
		{
			ModelsAre(Model("mid"));

			var expected = new VariableData
			{
				IsList = false,
				Values = new List<ObjectData>{
					new ObjectData
					{
						Id = "id", 
						ModelId = "mid",
						Display = "value"
					}
				}
			};

			var actual = Compressor().DecompressVariableData(Deserialize("{\"Id\":\"id\",\"Display\":\"value\",\"ModelId\":\"mid\"}"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__VariableData_deserializes_id_string_to_a_nonlist_VariableData_when_model_id_is_known()
		{
			ModelsAre(Model("mid"));

			var expected = new VariableData
			{
				IsList = false,
				Values = new List<ObjectData>{
					new ObjectData
					{
						Id = "id", 
						ModelId = "mid",
						Display = "id"
					}
				}
			};

			var actual = Compressor("mid").DecompressVariableData(Deserialize("\"id\""));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__VariableData_deserializes_null_to_a_VariableData_with_null_item_in_it()
		{
			var expected = new VariableData
			{
				IsList = false,
				Values = new List<ObjectData> { null }
			};

			var actual = Compressor().DecompressVariableData(Deserialize("null"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__VariableData_throws_ArgumentException_when_only_id_string_is_given_and_model_id_is_not_known()
		{
			Assert.Throws<ArgumentException>(() => Compressor().DecompressVariableData(Deserialize("\"id\"")));
		}

		[Test]
		public void When_decompressing__ObjectData_deserializes_Data_to_a_Dictionary_of_VariableData()
		{
			ModelsAre(Model("mid").Data("mmid1", "mid").Data("mmid2", "mid"));

			var expected = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = "id",
				Data = new Dictionary<string, VariableData>
				{
					{"mmid1", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid1_id", ModelId="mid", Display="mmid1_id"}}}},
					{"mmid2", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid2_id", ModelId="mid", Display="mmid2_id"}}}}
				}
			};

			var actual = Compressor("mid").DecompressObjectData(Deserialize(
				"{" +
					"\"Id\":\"id\"," +
					"Data:" +
					"{" +
						"\"mmid1\":\"mmid1_id\"," +
						"\"mmid2\":\"mmid2_id\"" +
					"}" +
				"}"
			));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ObjectData_ignores_nonexisting_keys()
		{
			ModelsAre(Model("mid").Data("mmid1", "mid"));

			var expected = new ObjectData
			{
				Id = "id",
				ModelId = "mid",
				Display = "id",
				Data = new Dictionary<string, VariableData>
				{
					{"mmid1", new VariableData{Values=new List<ObjectData>{new ObjectData{Id="mmid1_id", ModelId="mid", Display="mmid1_id"}}}}
				}
			};

			var actual = Compressor("mid").DecompressObjectData(Deserialize(
				"{" +
					"\"Id\":\"id\"," +
					"Data:" +
					"{" +
						"\"mmid1\":\"mmid1_id\"," +
						"\"mmid2\":\"mmid2_id\"" +
					"}" +
				"}"
			));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterData_deserializes_id_and_model_id()
		{
			Model("mid");

			var expected = new ParameterData { Id = "id", ModelId = "mid" };
			var actual = Compressor().DecompressParameterData(Deserialize("{\"Id\":\"id\",\"ModelId\":\"mid\"}"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterData_deserializes_id_string_when_model_is_known()
		{
			Model("mid");

			var expected = new ParameterData { Id = "id", ModelId = "mid" };
			var actual = Compressor("mid").DecompressParameterData(Deserialize("\"id\""));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterData_sets_IsNull_when_given_object_is_null()
		{
			var actual = Compressor().DecompressParameterData(Deserialize("null"));

			Assert.IsNull(actual);
		}

		[Test]
		public void When_decompressing__ParameterValueData_deserializes_List()
		{
			Model("mid");

			var expected = new ParameterValueData
			{
				IsList = true,
				Values = new List<ParameterData>{
					new ParameterData { Id = "id", ModelId = "mid" }
				}
			};

			var actual = Compressor("mid").DecompressParameterValueData(Deserialize("[\"id\"]"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterValueData_deserializes_id_string_to_a_nonlist_ParameterValueData_when_model_is_known()
		{
			Model("mid");

			var expected = new ParameterValueData
			{
				IsList = false,
				Values = new List<ParameterData>{
					new ParameterData { Id = "id", ModelId = "mid" }
				}
			};

			var actual = Compressor("mid").DecompressParameterValueData(Deserialize("\"id\""));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterValueData_deserializes_null_to_a_nonlist_ParameterValueData_with_one_null_item()
		{
			var expected = new ParameterValueData
			{
				IsList = false,
				Values = new List<ParameterData> { null }
			};

			var actual = Compressor("mid").DecompressParameterValueData(Deserialize("null"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterData_throws_ArgumentException_when_given_string_does_not_contain_model_id_and_model_id_is_not_known()
		{
			Assert.Throws<ArgumentException>(() => Compressor().DecompressParameterValueData(Deserialize("\"id\"")));
		}

		[Test]
		public void When_decompressing__ParameterData_contains_only_model_id_with_zero_initialization_parameters_when_given_dictionary_does_not_contain_id_nor_initialization_parameters()
		{
			ModelsAre(Model("mid"));

			var expected = new ParameterData
			{
				ModelId = "mid"
			};

			var actual = Compressor().DecompressParameterData(Deserialize("{\"ModelId\":\"mid\"}"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterData_deserializes_Dictionary_to_a_ParameterData_with_initialization_parameters()
		{
			ModelsAre(
				Model("mid")
				.Initializer(
					PModel("p1", "p1_mid"),
					PModel("p2", "p2_mid", true),
					PModel("p3", "p3_mid")
				),
				Model("p1_mid"),
				Model("p2_mid"),
				Model("p3_mid")
			);

			var expected = new ParameterData
			{
				ModelId = "mid",
				InitializationParameters = new Dictionary<string, ParameterValueData>
				{
					{
						"p1",
						new ParameterValueData
						{
							Values = new List<ParameterData> {new ParameterData {Id = "p1_id", ModelId = "p1_mid"}}
						}
					},
					{
						"p2",
						new ParameterValueData
						{
							IsList = true,
							Values = new List<ParameterData> {new ParameterData {Id = "p2_id", ModelId = "p2_mid"}}
						}
					},
					{
						"p3",
						new ParameterValueData
						{
							Values = new List<ParameterData> {null}
						}
					},
				}
			};

			var actual = Compressor("mid").DecompressParameterData(Deserialize(
				"{" +
					"\"Data\":" +
					"{" +
						"\"p1\":\"p1_id\"," +
						"\"p2\":[\"p2_id\"]," +
						"\"p3\":null" +
					"}" +
				"}"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterValueData_ignore_nonexisting_initialization_parameters()
		{
			ModelsAre(
				   Model("mid")
				   .Initializer(
					   PModel("p1", "p1_mid")
				   ),
				   Model("p1_mid")
			   );

			var expected = new ParameterData
			{
				ModelId = "mid",
				InitializationParameters = new Dictionary<string, ParameterValueData>
				{
					{
						"p1",
						new ParameterValueData
						{
							Values = new List<ParameterData> {new ParameterData {Id = "p1_id", ModelId = "p1_mid"}}
						}
					}
				}
			};

			var actual = Compressor("mid").DecompressParameterData(Deserialize(
				"{" +
					"\"Data\":" +
					"{" +
						"\"p1\":\"p1_id\"," +
						"\"p2\":[\"p2_id\"]" +
					"}" +
				"}"));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void When_decompressing__ParameterValueData_deserializes_Dictionary_to_a_nonlist_ParameterValueData()
		{
			ModelsAre(
				Model("mid")
				.Initializer(
					PModel("p1", "p1_mid")
				),
				Model("p1_mid")
			);

			var expected = new ParameterValueData
			{
				IsList = false,
				Values = new List<ParameterData>
				{
					new ParameterData
					{
						ModelId = "mid",
						InitializationParameters = new Dictionary<string, ParameterValueData>
						{
							{
								"p1",
								new ParameterValueData
								{
									Values = new List<ParameterData> {new ParameterData {Id = "p1_id", ModelId = "p1_mid"}}
								}
							}
						}
					}
				}
			};

			var actual = Compressor("mid").DecompressParameterValueData(Deserialize(
				"{" +
					"\"Data\":" +
					"{" +
						"\"p1\":\"p1_id\"" +
					"}" +
				"}"));

			Assert.AreEqual(expected, actual);
		}
	}
}
