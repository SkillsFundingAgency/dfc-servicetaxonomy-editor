// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Allow views following Orchard Core's naming convention for custom fields", Scope = "namespaceanddescendants", Target = "DFC.ServiceTaxonomy.Editor.Module.Views")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S927:parameter names should match base declaration and other partial definitions", Justification = "Must match partial definitions makes sense, but it's useful to give a more specific name in derived declarations and sometimes base names are not great.", Scope = "namespaceanddescendants", Target = "DFC.ServiceTaxonomy.Editor.Module")]
