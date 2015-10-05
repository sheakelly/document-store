using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DocumentStore
{
    public static class Nifty
    {        
        public static IDictionary<Type, IList<Target>> PromotedPropertyExpressions = new Dictionary<Type, IList<Target>>();        

        public static void Promote<T>(Expression<Func<T, object>> expression, string columnName = null)
        {                  
            var targetPropertyInfo = expression.AsPropertyInfo();
            var targetMemberExpression = GetTargetExpression(expression);
            var targetExpression = FindParameterExpression<T>(targetMemberExpression);
            var target = new Target
            {
                TargetExpression = targetExpression, 
                PropertyInfo = targetPropertyInfo, 
                ColumnName = columnName ?? GetColumnName(expression)
            };
            AddPromotedMemberExpression(typeof(T), target);
        }

        public static MemberExpression GetTargetExpression<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression memberExpression = null;            
            WalkMemberExpressionTree(expression, current => memberExpression = current);
            return memberExpression;
        }

        private static string GetColumnName<T>(Expression<Func<T, object>> expression)
        {
            var builder = new StringBuilder();
            WalkMemberExpressionTree(expression, current => builder.Append(current.Member.Name));
            return builder.ToString();
        }

        private static LambdaExpression FindParameterExpression<T>(MemberExpression memberExpression)
        {
            var parameter = GetParameterExpression(memberExpression.Expression);
            return Expression.Lambda(memberExpression.Expression, parameter);
        }

        private static void WalkMemberExpressionTree<T>(Expression<Func<T, object>> expression, Action<MemberExpression> onNode)
        {
            MemberExpression memberExpression;
            if (!TryFindMemberExpression(expression.Body, out memberExpression))
            {
                throw new Exception("Body expression is not a MemberExpression");
            }
            while (!TryFindMemberExpression(memberExpression, out memberExpression))
            {
                onNode(memberExpression);
            }
            onNode(memberExpression);
        }

        private static bool TryFindMemberExpression(Expression expression, out MemberExpression memberExpressions)
        {
            memberExpressions = expression as MemberExpression;
            if (memberExpressions != null)
            {                
                return true;
            }

            // if the compiler created an automatic conversion,
            // it'll look something like...
            // obj => Convert(obj.Property) [e.g., int -> object]
            // OR:
            // obj => ConvertChecked(obj.Property) [e.g., int -> long]
            // ...which are the cases checked in IsConversion
            if (IsConversion(expression) && expression is UnaryExpression)
            {
                memberExpressions = ((UnaryExpression)expression).Operand as MemberExpression;
                if (memberExpressions != null)
                {
                    return true;
                }
            }   
            return false;
        }

        private static bool IsConversion(Expression exp)
        {
            return (
                exp.NodeType == ExpressionType.Convert ||
                exp.NodeType == ExpressionType.ConvertChecked
            );
        }

        private static ParameterExpression GetParameterExpression(Expression expression)
        {
            while (expression.NodeType == ExpressionType.MemberAccess)
            {
                expression = ((MemberExpression)expression).Expression;
            }
            if (expression.NodeType == ExpressionType.Parameter)
            {
                return (ParameterExpression)expression;
            }
            return null;
        }

        private static PropertyInfo AsPropertyInfo(this LambdaExpression expression)
        {
            var member = expression.Body as MemberExpression;
            if (member != null)
            {
                return (PropertyInfo) member.Member;
            }
            var unary = (UnaryExpression) expression.Body;
            return (PropertyInfo) ((MemberExpression) unary.Operand).Member;
        }

        private static void AddPromotedMemberExpression(Type type, Target target)
        {
            IList<Target> targets;
            PromotedPropertyExpressions.TryGetValue(type, out targets);
            if (!PromotedPropertyExpressions.TryGetValue(type, out targets))
            {
                targets = new List<Target>();
                PromotedPropertyExpressions[type] = targets;
            }
            targets.Add(target);   
        }

        public static IEnumerable<Target> EnumeratePromotedPropertyTargets(Type type)
        {
            if (!PromotedPropertyExpressions.ContainsKey(type))
            {
                return new List<Target>();
            }
            return PromotedPropertyExpressions[type];            
        }
    }
}