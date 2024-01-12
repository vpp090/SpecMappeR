using System.Reflection;

namespace SpecMapperR
{
    public class SpecialMapper : ISpecialMapper
    {
        public TDest MapProperties<TSource, TDest>(TSource source) where TDest : new()
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return (TDest)MapPropertiesRecursive(source, typeof(TDest));
        }

        private object MapPropertiesRecursive(object source, Type destType)
        {
            if (source == null) return null;

            var dest = Activator.CreateInstance(destType);
            var sourceProps = source.GetType().GetProperties().Where(p => p.CanRead);
            var destProps = destType.GetProperties().Where(p => p.CanWrite);

            foreach (var sourceProp in sourceProps)
            {
                var matchingProp = destProps.FirstOrDefault(p => p.Name == sourceProp.Name);
                if (matchingProp != null)
                {
                    try
                    {
                        if (IsSimpleType(matchingProp.PropertyType))
                        {
                            var value = sourceProp.GetValue(source);
                            matchingProp.SetValue(dest, value);
                        }
                        else
                        {
                            var nestedSourceValue = sourceProp.GetValue(source);
                            var nestedDestValue = MapPropertiesRecursive(nestedSourceValue, matchingProp.PropertyType);
                            matchingProp.SetValue(dest, nestedDestValue);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return dest;
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type.Equals(typeof(string)) || type.Equals(typeof(decimal));
        }
    }
}
