global using System.Reflection;

global using Microsoft.VisualStudio.TestTools.UnitTesting;

#if DEBUG
[assembly: Parallelize(Scope = ExecutionScope.ClassLevel, Workers = 1)]
#else
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 0)]
#endif

[assembly: TestDataSourceDiscovery(TestDataSourceDiscoveryOption.DuringExecution)]
