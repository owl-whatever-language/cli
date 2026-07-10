using System.IO;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Printing.Mermaid;

partial class MermaidControlFlow
{
	#region Nested types
	public sealed class Result
	{
		#region Properties
		public IControlFlowGraph Graph { get; }
		public Settings Settings { get; }
		public string Code { get; }
		public string? Config { get; }
		public string? Stylesheet { get; }
		#endregion

		#region Constructors
		public Result(IControlFlowGraph graph, Settings settings, string code, string? config, string? stylesheet)
		{
			Graph = graph;
			Settings = settings;
			Code = code;
			Config = config;
			Stylesheet = stylesheet;
		}
		#endregion

		#region Methods
		public void Print(string outputPath)
		{
			DirectoryInfo temp = Directory.CreateTempSubdirectory("owl.mermaid.cfg");


			List<string> args =
			[
				"-q",
				"-t", "dark",
				"-b", Settings.Styles?.Background?.ToHex ?? "transparent",
				"-i", "-",
				"-o", outputPath
			];

			if (Config is not null)
			{
				string path = Path.Combine(temp.FullName, "config.json");
				File.WriteAllText(path, Config);

				args.Add("--configFile");
				args.Add(path);
			}

			if (Stylesheet is not null)
			{
				string path = Path.Combine(temp.FullName, "style.css");
				File.WriteAllText(path, Stylesheet);

				args.Add("--cssFile");
				args.Add(path);
			}

			ProcessStartInfo startInfo = new("mmdc", args) { RedirectStandardInput = true };

			Process? process = Process.Start(startInfo);
			if (process is null)
				return;

			using (process.StandardInput)
			{
				process.StandardInput.WriteLine(Code);
				process.StandardInput.Flush();
			}

			process.WaitForExit();
		}
		#endregion
	}
	#endregion
}
