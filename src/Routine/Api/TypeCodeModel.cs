﻿using System;
using System.Collections.Generic;
using System.Linq;
using Routine.Client;

namespace Routine.Api
{
	public class TypeCodeModel
	{
		private readonly TypeCodeModel actual;
		private readonly ApplicationCodeModel application;
		private readonly bool isList;

		private InitializerCodeModel initializer;
		private readonly List<DataCodeModel> datas;
		private readonly List<OperationCodeModel> operations;

		public Rtype Type { get; private set; }
		public Type ReferencedType { get; private set; }
		public List<TypeCodeModel> ViewModels { get; private set; } 
		public List<TypeCodeModel> ActualModels { get; private set; }
		public InitializerCodeModel Initializer { get { return actual != null ? actual.Initializer : initializer; } }
		public List<DataCodeModel> Datas { get { return actual != null ? actual.Datas : datas; } }
		public List<OperationCodeModel> Operations { get { return actual != null ? actual.Operations : operations; } }

		private ITypeConversionTemplate GetTypeConversionTemplate(int mode)
		{
			return IsReferenced
				? application.Configuration.GetReferencedTypeTemplate(ReferencedType)
				: application.Configuration.GetRenderedTypeTemplate(this, mode);
		}

		internal TypeCodeModel(ApplicationCodeModel application, Rtype type)
			: this(application, null, false, type, null) { }

		internal TypeCodeModel(ApplicationCodeModel application, TypeCodeModel model, bool isList)
			: this(application, model, isList, model.Type, model.ReferencedType) { }

		internal TypeCodeModel(ApplicationCodeModel application, Rtype type, Type referencedType)
			: this(application, null, false, type, referencedType) { }

		private TypeCodeModel(ApplicationCodeModel application, TypeCodeModel actual, bool isList, Rtype type, Type referencedType)
		{
			this.application = application;
			this.actual = actual;
			this.isList = isList;

			Type = type;
			ReferencedType = referencedType;

			ViewModels = new List<TypeCodeModel>();
			ActualModels = new List<TypeCodeModel>();

			datas = new List<DataCodeModel>();
			operations = new List<OperationCodeModel>();
		}

		internal void Load()
		{
			ViewModels.AddRange(Type.ViewTypes
				.Where(t => application.ValidateType(t))
				.Select(t => application.GetModel(t)));
			ActualModels.AddRange(Type.ActualTypes
				.Where(t => application.ValidateType(t))
				.Select(t => application.GetModel(t)));


			if (Type.Initializer != null && application.Configuration.IsRendered(Type.Initializer) && Type.Initializer.Parameters.All(p => application.ValidateType(p.ParameterType)))
			{
				initializer = new InitializerCodeModel(application, Type.Initializer);
			}

			datas.AddRange(Type.Datas
				.Where(m => application.Configuration.IsRendered(m) && application.ValidateType(m.DataType))
				.Select(m => new DataCodeModel(application, m)));

			operations.AddRange(Type.Operations
				.Where(o => application.Configuration.IsRendered(o) && (o.ResultIsVoid || application.ValidateType(o.ResultType)) && o.Parameters.All(p => application.ValidateType(p.ParameterType)))
				.Select(o => new OperationCodeModel(application, o)));
		}

		public ApplicationCodeModel Application { get { return application; } }
		public bool IsVoid { get { return Type.IsVoid; } }
		public bool IsList { get { return isList; } }
		public string Id { get { return Type.Id; } }
		public bool IsReferenced { get { return ReferencedType != null; } }

		public string GetFullName(int mode) { return GetFullName(mode, false); }
		public string GetFullName(int mode, bool ignoreList)
		{
			if (IsList && !ignoreList)
			{
				var listType = typeof(List<>);

				return listType.Namespace + "." + listType.Name.Before("`") + "<" + GetFullName(mode, true) + ">";
			}

			if (IsReferenced)
			{
				return ReferencedType.ToCSharpString();
			}

			var ns = GetNamespace(mode);

			if (string.IsNullOrEmpty(ns)) { return GetName(mode); }

			return string.Format("{0}.{1}", ns, GetName(mode));
		}

		public string GetNamespace(int mode)
		{
			if (IsReferenced)
			{
				return ReferencedType.Namespace;
			}

			return Application.Configuration.GetNamespace(this, mode);
		}

		public string GetName(int mode)
		{
			if (IsReferenced)
			{
				return ReferencedType.Name;
			}

			return application.Configuration.GetName(this, mode);
		}

		public List<Type> GetAttributes(int mode)
		{
			return application.Configuration.GetAttributes(this, mode);
		}

		public string RenderAttributes(int mode)
		{
			return string.Join("\r\n", GetAttributes(mode).Select(t => string.Format("[{0}]", t.ToCSharpString())));
		}

		public bool Initializable { get { return Initializer != null; } }

		public List<int> GetModes()
		{
			return Application.Configuration.GetModes(this);
		}

		public bool HasMode(int mode)
		{
			return GetModes().Contains(mode);
		}

		public bool MarkedAs(string mark)
		{
			return Type.MarkedAs(mark);
		}

		internal TypeCodeModel GetListType()
		{
			return new TypeCodeModel(application, this, true);
		}

		public string RenderRvariableToObject(int mode, string rvariableExpression, string rapplicationExpression)
		{
			if (IsList)
			{
				return string.Format("{0}.AsList(o => {1})",
					rvariableExpression,
					RenderRobjectToObject(mode, "o", rapplicationExpression)
				);
			}

			return string.Format("{0}.As(o => {1})",
				rvariableExpression,
				RenderRobjectToObject(mode, "o", rapplicationExpression)
			);
		}

		public string RenderRobjectToObject(int mode, string robjectExpression, string rapplicationExpression)
		{
			return GetTypeConversionTemplate(mode).RenderRobjectToObject(this, robjectExpression, GetRtypeExpression(rapplicationExpression));
		}

		public string RenderObjectToRvariable(int mode, string rvariableName, string objectExpression, string rapplicationExpression)
		{
			if (IsList)
			{
				return string.Format("{0}.NewVarList(\"{1}\", {2}, o => {3})",
					rapplicationExpression,
					rvariableName,
					objectExpression,
					RenderObjectToRobject(mode, "o", rapplicationExpression)
				);
			}

			return string.Format("{0}.NewVar(\"{1}\", {2}, o => {3})",
				rapplicationExpression,
				rvariableName,
				objectExpression,
				RenderObjectToRobject(mode, "o", rapplicationExpression)
			);
		}

		public string RenderObjectToRobject(int mode, string objectExpression, string rapplicationExpression)
		{
			return GetTypeConversionTemplate(mode).RenderObjectToRobject(this, objectExpression, GetRtypeExpression(rapplicationExpression));
		}

		private string GetRtypeExpression(string rapplicationExpression)
		{
			return string.Format("{0}[\"{1}\"]", rapplicationExpression, Type.Id);
		}

		public override string ToString()
		{
			return string.Format("{0}", Type);
		}

		#region Equality & Hashcode

		protected bool Equals(TypeCodeModel other)
		{
			return Equals(Type, other.Type) && Equals(ReferencedType, other.ReferencedType);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return Equals((TypeCodeModel)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (ReferencedType != null ? ReferencedType.GetHashCode() : 0);
			}
		}

		#endregion
	}
}
