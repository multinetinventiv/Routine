﻿using Routine.Core;

namespace Routine.Interception.Context
{
	public class ObjectReferenceInterceptionContext : InterceptionContext
	{
		protected readonly IObjectService objectService;

		public ObjectReferenceInterceptionContext(string target, IObjectService objectService, ReferenceData targetReference)
			: base(target)
		{
			this.objectService = objectService;

			TargetReference = targetReference;
		}

		public ReferenceData TargetReference
		{
			get { return this["TargetReference"] as ReferenceData; }
			set { this["TargetReference"] = value; }
		}

		public ObjectModel Model { get { return objectService.ApplicationModel.Model[TargetReference.ModelId]; } }
		public ObjectModel ViewModel { get { return objectService.ApplicationModel.Model[TargetReference.ViewModelId]; } }
	}
}
