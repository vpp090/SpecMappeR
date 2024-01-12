namespace SpecMapperR
{
    public interface ISpecialMapper 
    {
        TDest MapProperties<TSource, TDest>(TSource source) where TDest : new();
    }
}
