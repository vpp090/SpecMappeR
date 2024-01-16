using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Intrinsics.X86;

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
                    var (isCollection, elementType) = IsCollectionType(sourceProp.PropertyType);
                    if (isCollection)
                    {
                        // Handle collection types
                        var collection = HandleCollection(sourceProp.GetValue(source), elementType);
                        matchingProp.SetValue(dest, collection);
                    }
                    else if (IsSimpleType(sourceProp.PropertyType))
                    {
                        // Handle simple types
                        var value = sourceProp.GetValue(source);
                        matchingProp.SetValue(dest, value);
                    }
                    else
                    {
                        // Handle complex types
                        var nestedSourceValue = sourceProp.GetValue(source);
                        var nestedDestValue = MapPropertiesRecursive(nestedSourceValue, matchingProp.PropertyType);
                        matchingProp.SetValue(dest, nestedDestValue);

                       
                    }
                }
            }

            return dest;
        }

        private object HandleCollection(object sourceCollection, Type elementType)
        {
            if (sourceCollection == null) return null;

            // Determine the type of collection to create (e.g., List<elementType>, HashSet<elementType>, etc.)
            Type collectionType = typeof(List<>).MakeGenericType(elementType);
            var destCollection = Activator.CreateInstance(collectionType) as IList;

            // Iterate through each item in the source collection and add to the destination collection
            foreach (var item in (IEnumerable)sourceCollection)
            {
                var destItem = IsSimpleType(elementType) ? item : MapPropertiesRecursive(item, elementType);
                destCollection?.Add(destItem);
            }

            return destCollection;
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type.Equals(typeof(string)) || type.Equals(typeof(decimal)) || type.IsValueType;
        }

        private (bool, Type) IsCollectionType(Type type)
        {
            // String is IEnumerable, but we don't want to treat it as a collection here
            if (type == typeof(string))
                return (false, null);

            // Check if this is an IEnumerable type
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                // Get the generic type of the collection
                var elementType = type.IsGenericType
                    ? type.GetGenericArguments()[0]
                    : type.GetElementType(); // for arrays

                return (true, elementType);
            }

            return (false, null);
        }
    }
}
