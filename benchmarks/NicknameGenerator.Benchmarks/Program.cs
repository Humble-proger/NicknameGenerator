namespace NicknameGenerator.Benchmarks
{
	using BenchmarkDotNet.Running;

	public static class Program
	{
		public static void Main(string[] args) => BenchmarkRunner.Run<GenerationBenchmarks>(args: args);
	}
}
