using System.Linq.Expressions;

namespace MbyteWeb.Server.Helpers
{
    public class Filter<T> {
        private IQueryable<T> queryable;
        public Filter(IQueryable<T> queryable)
        {
            this.queryable = queryable;
        }

        public IQueryable<T> ToQuery()
        {
            return this.queryable;
        }

        public Filter<T> Where(Expression<Func<T, bool>> expression)
        {
            queryable = queryable.Where(expression);
            return this;
        }

        public Filter<T> Eq<R>(Expression<Func<T, R?>> propExtractor, R? value)
        {
            if (value == null) return this;
            return this.Where(EqFunction(propExtractor, value));
        }

        public Filter<T> Gt<R>(Expression<Func<T, R?>> propExtractor, R? value)
        {
            if (value == null) return this;
            return this.Where(GtFunction(propExtractor, value));
        }

        public Filter<T> Lt<R>(Expression<Func<T, R?>> propExtractor, R? value)
        {
            if (value == null) return this;
            return this.Where(LtFunction(propExtractor, value));
        }

        public Filter<T> Neq<R>(Expression<Func<T, R?>> propExtractor, R? value)
        {
            if (value == null) return this;
            return this.Where(NeqFunction(propExtractor, value));
        }

        public Filter<T> Gteq<R>(Expression<Func<T, R?>> propExtractor, R? value)
        {
            if (value == null) return this;
            return this.Where(GteqFunction(propExtractor, value));
        }

        public Filter<T> Lteq<R>(Expression<Func<T, R?>> propExtractor, R? value)
        {
            if (value == null) return this;
            return this.Where(LteqFunction(propExtractor, value));
        }

        public Filter<T> IsNull<R>(Expression<Func<T, R?>> propExtractor)
        {
            return this.Where(IsNullFunction(propExtractor));
        }

        public Filter<T> IsNotNull<R>(Expression<Func<T, R?>> propExtractor)
        {
            return this.Where(IsNotNullFunction(propExtractor));
        }

        public Filter<T> Between<R>(Expression<Func<T, R?>> propExtractor, R? min, R? max) {
            return this.Gteq(propExtractor, min).Lt(propExtractor, max);
        }

        public Filter<T> Contains(Expression<Func<T, string?>> propExtractor, string? value) {
            if (string.IsNullOrEmpty(value)) return this;
            queryable = queryable.Where(ContainsFunction(propExtractor, value));
            return this;
        }


        //PRIVATE

        //Comparison Function
        private static Expression<Func<T, bool>> EqFunction<R>(Expression<Func<T, R?>> propExtractor, R value)
        {
            var param = propExtractor.Parameters;
            var body = Expression.Equal(propExtractor.Body, Expression.Constant(value, typeof(R)));
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> GtFunction<R>(Expression<Func<T, R?>> propExtractor, R value) {
            var param = propExtractor.Parameters;
            var body = Expression.And(
                Expression.NotEqual(propExtractor.Body, Expression.Constant(null)),
                Expression.GreaterThan(propExtractor.Body, Expression.Constant(value, typeof(R)))
            );
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> LtFunction<R>(Expression<Func<T, R?>> propExtractor, R value)
        {
            var param = propExtractor.Parameters;
            var body = Expression.And(
                Expression.NotEqual(propExtractor.Body, Expression.Constant(null)),
                Expression.LessThan(propExtractor.Body, Expression.Constant(value, typeof(R)))
            );
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> NeqFunction<R>(Expression<Func<T, R?>> propExtractor, R value)
        {
            var param = propExtractor.Parameters;
            var body = Expression.NotEqual(propExtractor.Body, Expression.Constant(value, typeof(R)));
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> GteqFunction<R>(Expression<Func<T, R?>> propExtractor, R value)
        {
            var param = propExtractor.Parameters;
            var body = Expression.And(
                Expression.NotEqual(propExtractor.Body, Expression.Constant(null)),
                Expression.GreaterThanOrEqual(propExtractor.Body, Expression.Constant(value, typeof(R)))
            );
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> LteqFunction<R>(Expression<Func<T, R?>> propExtractor, R value)
        {
            var param = propExtractor.Parameters;
            var body = Expression.And(
                Expression.NotEqual(propExtractor.Body, Expression.Constant(null)),
                Expression.LessThanOrEqual(propExtractor.Body, Expression.Constant(value, typeof(R)))
            );
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> IsNullFunction<R>(Expression<Func<T, R?>> propExtractor)
        {
            var param = propExtractor.Parameters;
            var body = Expression.Equal(propExtractor.Body, Expression.Constant(null));
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression<Func<T, bool>> IsNotNullFunction<R>(Expression<Func<T, R?>> propExtractor)
        {
            var param = propExtractor.Parameters;
            var body = Expression.NotEqual(propExtractor.Body, Expression.Constant(null));
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        //String Function
        private static Expression<Func<T, bool>> ContainsFunction(Expression<Func<T, string?>> propExtractor, string value)
        {
            var containsMethodInfo = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);
            var toLowerMethodInfo = typeof(string).GetMethod(nameof(string.ToLower), []);

            if (containsMethodInfo == null || toLowerMethodInfo == null)
                throw new InvalidOperationException("Somehow, \"string\" type did not have \"Contains\" or \"ToLower\" methods.");

            var param = propExtractor.Parameters;
            
            var body = Expression.And(
                Expression.NotEqual(propExtractor.Body, Expression.Constant(null)),
                Expression.Call(
                    Expression.Call(propExtractor.Body, toLowerMethodInfo),
                    containsMethodInfo,
                    Expression.Call(Expression.Constant(value), toLowerMethodInfo)
                )
            );

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
        
    }

    public static class FilterBuilder {
        public static Filter<T> Create<T>(IQueryable<T> queryable) { 
            return new Filter<T>(queryable);
        }
    }
}
