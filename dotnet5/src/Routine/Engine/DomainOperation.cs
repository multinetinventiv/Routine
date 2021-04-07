using System.Collections.Generic;
using System.Linq;
using Routine.Core;

namespace Routine.Engine
{
    public class DomainOperation : IDomainParametric<IMethod>
    {
        private readonly ICoreContext ctx;
        private readonly List<DomainParameter.Group<IMethod>> groups;

        public string Name { get; }
        public Marks Marks { get; }
        public bool ResultIsVoid { get; }
        public bool ResultIsList { get; }
        public DomainType ResultType { get; }
        public Dictionary<string, DomainParameter> Parameter { get; }

        public ICollection<DomainParameter> Parameters => Parameter.Values;

        ICoreContext IDomainParametric<IMethod>.Ctx => ctx;
        int IDomainParametric<IMethod>.NextGroupIndex => groups.Count;
        void IDomainParametric<IMethod>.AddGroup(IMethod parametric, IEnumerable<DomainParameter> parameters, int groupIndex) => groups.Add(new DomainParameter.Group<IMethod>(parametric, parameters, groupIndex));

        public DomainOperation(ICoreContext ctx, IMethod method)
        {
            this.ctx = ctx;

            groups = new List<DomainParameter.Group<IMethod>>();

            Name = ctx.CodingStyle.GetName(method);
            Marks = new Marks();
            ResultIsVoid = method.ReturnType.IsVoid;
            ResultIsList = method.ReturnType.CanBeCollection();
            Parameter = new Dictionary<string, DomainParameter>();

            var returnType = ResultIsList ? method.ReturnType.GetItemType() : method.ReturnType;

            if (!ctx.CodingStyle.ContainsType(returnType))
            {
                throw new TypeNotConfiguredException(returnType);
            }

            ResultType = ctx.GetDomainType(returnType);

            AddGroup(method);
        }

        public void AddGroup(IMethod method)
        {
            if (groups.Any() &&
                !method.ReturnType.Equals(groups.Last().Parametric.ReturnType))
            {
                throw new ReturnTypesDoNotMatchException(method, groups.Last().Parametric.ReturnType, method.ReturnType);
            }

            if (groups.Any(g => g.ContainsSameParameters(method)))
            {
                throw new IdenticalSignatureAlreadyAddedException(method);
            }

            DomainParameter.AddGroupToTarget(method, this);

            Marks.Join(ctx.CodingStyle.GetMarks(method));
        }

        public bool MarkedAs(string mark)
        {
            return Marks.Has(mark);
        }

        public OperationModel GetModel()
        {
            return new OperationModel
            {
                Name = Name,
                Marks = Marks.List,
                GroupCount = groups.Count,
                Parameters = Parameters.Select(p => p.GetModel()).ToList(),
                Result = new ResultModel
                {
                    IsList = ResultIsList,
                    IsVoid = ResultIsVoid,
                    ViewModelId = ResultType.Id
                }
            };
        }

        public VariableData Perform(object target, Dictionary<string, ParameterValueData> parameterValues)
        {
            var resolution = new DomainParameterResolver<IMethod>(groups, parameterValues).Resolve();

            var resultValue = resolution.Result.PerformOn(target, resolution.Parameters);

            if (ResultIsVoid)
            {
                return new VariableData();
            }

            return ctx.CreateValueData(resultValue, ResultIsList, ResultType, true);
        }

        #region Formatting & Equality

        protected bool Equals(DomainOperation other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DomainOperation)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("{1} {0}(...)", Name, ResultType);
        }

        #endregion
    }
}
