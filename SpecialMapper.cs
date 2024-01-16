using System.Collections;
using System.Linq.Expressions;
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
                        else if (IsIListType(sourceProp.PropertyType))
                        {
                            var sourceList = sourceProp.GetValue(source) as IEnumerable;
                            if (sourceList == null) throw new ArgumentException("source is null");

                            // Determine the element type for the destination list
                            Type elementType;
                            if (matchingProp.PropertyType.IsGenericType)
                            {
                                // If the destination list type is generic, get its generic argument
                                elementType = matchingProp.PropertyType.GetGenericArguments()[0];
                            }
                            else
                            {
                                // For non-generic lists, use object as the element type
                                elementType = typeof(object);
                            }

                            // Create an instance of List<elementType>
                            var destListType = typeof(List<>).MakeGenericType(elementType);
                            var destList = (IList)Activator.CreateInstance(destListType);

                            // Populate the list
                            foreach (var item in sourceList)
                            {
                                var destItem = IsSimpleType(elementType) ? item : MapPropertiesRecursive(item, elementType);
                                destList.Add(destItem);
                            }

                            matchingProp.SetValue(dest, destList);
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

        private bool IsIListType(Type type)
        {
            return typeof(IList<>).IsAssignableFrom(type) && type.IsGenericType;
        }
    }
}
