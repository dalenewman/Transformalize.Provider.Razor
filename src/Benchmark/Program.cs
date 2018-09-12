using System;
using Autofac;
using Transformalize.Contracts;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Transformalize.Containers.Autofac;
using Transformalize.Logging;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Transforms.Razor.Autofac;

namespace Benchmark {

    
    [LegacyJitX64Job, LegacyJitX86Job]
    public class Benchmarks {

        [Benchmark(Baseline = true, Description = "5000 test rows")]
        public void TestRows() {
            using (var outer = new ConfigurationContainer(new RazorModule()).CreateScope(@"files\bogus.xml?Size=5000")) {
                using (var inner = new TestContainer(new RazorModule(),new BogusModule()).CreateScope(outer, new NullLogger())) {
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                }
            }
        }

        [Benchmark(Baseline = false, Description = "5000 rows with 1 razor")]
        public void CSharpRows() {
            using (var outer = new ConfigurationContainer(new RazorModule()).CreateScope(@"files\bogus-with-transform.xml?Size=5000")) {
                using (var inner = new TestContainer(new RazorModule(), new BogusModule()).CreateScope(outer, new NullLogger())) {
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                }
            }
        }

    }

    public class Program {
        private static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<Benchmarks>();
            Console.WriteLine(summary);
        }
    }
}
