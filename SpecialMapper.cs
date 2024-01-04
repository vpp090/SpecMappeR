using System.Reflection;

namespace SpecMapperR
{
    public class SpecialMapper : ISpecialMapper
    {
        public TDest MapProperties<TSource, TDest>(TSource source, TDest dest) where TDest : new()
        {
            try
            {
                dest ??= new TDest();

                PropertyInfo[] sourceProps = source.GetType().GetProperties();
                PropertyInfo[] destProps = dest.GetType().GetProperties();

                foreach (var sourceProp in sourceProps)
                {
                    var matchinProp = destProps.FirstOrDefault(p => p.Name == sourceProp.Name);
                    if (matchinProp != null)
                    {
                        object value = sourceProp.GetValue(source);
                        matchinProp.SetValue(dest, value);
                    }
                }

                return dest;
            }
            catch(Exception)
            {
                throw;
            } 
        }
    }
}
