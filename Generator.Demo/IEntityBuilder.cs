namespace Generator.Demo;

public interface IEntityBuilder<out TBuilder, TEntity> where TBuilder : IEntityBuilder<TBuilder, TEntity>
{
    static abstract TBuilder Minimal();
    static abstract TBuilder Typical();
}
