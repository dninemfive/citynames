// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1860:Avoid using 'Enumerable.Any()' extension method", 
                           Justification = "It reads better to me and i don't care about the performance impacts here", 
                           Scope = "namespaceanddescendants", 
                           Target = "~N:citynames")]
[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization",
                           Justification = "new() instead of [] is more readable to me",
                           Scope = "namespaceanddescendants",
                           Target = "~N:citynames")]
