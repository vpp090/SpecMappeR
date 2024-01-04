namespace SpecMapperR
{
    public interface ISpecialMapper 
    {
        TDest MapProperties<TSource, TDest>(TSource source, TDest dest) where TDest : new();
    }
}
