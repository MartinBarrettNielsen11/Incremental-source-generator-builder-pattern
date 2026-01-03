
using System.Runtime.CompilerServices;
using Incremental_source_generator_builder_pattern;

namespace BimServices.BuilderGenerator
{

    /// <summary>
    /// Adds support for interpolated string syntax on <see cref="ValueStringBuilder"/>.
    /// </summary>
    internal static class ValueStringBuilderExtensions
    {
        /// <summary>
        /// Appends an interpolated string directly to the builder using your custom handler.
        /// </summary>
        internal static void AppendInterpolated(
            this ValueStringBuilder vsb,
            [InterpolatedStringHandlerArgument("vsb")] ValueStringBuilderInterpolatedHandler handler)
        {
            handler.Commit(ref vsb);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class InterpolatedStringHandlerAttribute : Attribute {}

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) => Arguments = arguments;
        public string[] Arguments { get; }
    }
}