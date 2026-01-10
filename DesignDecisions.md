## Design Decisions

The incremental source generator is deliberately engineered around **Roslyn’s incremental execution and memoization model**, with a primary goal of **maximizing cache reuse while minimizing recomputation across IDE edits**.  
Rather than optimizing for raw generation speed alone, the design prioritizes **predictable incremental behavior**, stable value semantics, and strict avoidance of pipeline invalidation.

Central to this approach is an emphasis on **incremental cacheability**.  
Roslyn’s incremental pipeline only reuses prior results when intermediate values are *structurally comparable* and demonstrably unchanged. To ensure this, the generator adopts **explicit, value-based data models** throughout the pipeline. Every intermediate representation is deterministic, immutable, and free of Roslyn symbols or syntax nodes, making it safe for memoization and resilient to spurious invalidation.

### Value-Equatable Models and Structural Equality

A key enabler of reliable cache reuse is the introduction of the `EquatableArray<T>` abstraction.  
While arrays are a natural fit for representing ordered collections of symbols, reference equality would defeat Roslyn’s incremental comparison logic. By wrapping arrays in a lightweight, immutable, equatable container, equality is defined purely in terms of contents rather than identity.

This ensures that semantically equivalent property sets—such as when a user edits unrelated code—do not cause downstream pipeline stages to re-execute. In practice, this significantly improves cache hit rates and prevents unnecessary regeneration when the underlying type structure is unchanged.

All higher-level models (e.g. `BuilderToGenerate`, `PropertyInfoModel`) are defined as value-equatable record structs composed exclusively of primitive types and equatable collections. This guarantees that every pipeline boundary is stable, comparable, and incrementally safe.
