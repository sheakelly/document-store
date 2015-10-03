using System.Linq.Expressions;
using System.Reflection;

namespace DocumentStore
{
    public class Target
    {
        public PropertyInfo PropertyInfo { get; set; }
        public LambdaExpression TargetExpression { get; set; }
        public string ColumnName { get; set; }

        public object GetValue(object document)
        {
            var realTarget = TargetExpression.Compile().DynamicInvoke(document);
            return realTarget == null ? null : PropertyInfo.GetValue(realTarget);
        }
    }
}