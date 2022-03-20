using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Xunit;

[assembly: AssemblyDescription("F0.Analyzers code sample.")]
[assembly: AssemblyCopyright("Copyright © f[0] 2022")]

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass)]
