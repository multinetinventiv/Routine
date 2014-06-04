﻿using System.Collections.Generic;
using System.Linq;
using Routine.Core;

namespace Routine.Api
{
	public class OperationCodeModel : CodeModelBase
	{
		public OperationCodeModel(IApiGenerationContext context)
			: base(context) { }

		private OperationModel model;

		internal OperationCodeModel With(OperationModel model)
		{
			this.model = model;

			return this;
		}

		public string Id { get { return model.Id; } }
		public ObjectCodeModel ReturnModel 
		{ 
			get 
			{
				if (model.Result.IsVoid)
				{
					return CreateObject().Void();
				}

				return CreateObject().With(model.Result.ViewModelId, model.Result.IsList);
			} 
		}

		public List<ParameterCodeModel> Parameters { get { return model.Parameters.Select(p => CreateParameter().With(p)).ToList(); } }
		public List<List<ParameterCodeModel>> Groups
		{
			get
			{
				var result = Enumerable.Range(0, model.GroupCount).Select(i => new List<ParameterCodeModel>()).ToList();

				foreach (var param in Parameters)
				{
					foreach (var group in param.Groups)
					{
						result[group].Add(param);
					}
				}

				return result;
			}
		}

		public bool MarkedAs(string mark)
		{
			return model.Marks.Contains(mark);
		}
	}
}
