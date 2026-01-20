namespace Generator.Demo;

public interface IEntityBuilder<out TBuilder, TEntity> where TBuilder : IEntityBuilder<TBuilder, TEntity>
{
    static abstract TBuilder Simple();
    static abstract TBuilder Typical();
}
