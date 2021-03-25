using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Ddr.Ssq.IO;
using Ddr.Ssq.Printing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ddr.Ssq.AnalyzeTool
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            await CreateDefaultBuilder(args)
                .RunConsoleAppFrameworkAsync<Program>(args);
        }
        static IHostBuilder CreateDefaultBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddOptions<OutputOptions>()
                    .Bind(context.Configuration.GetSection("Output"));
            })
            .ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                var LoggingSection = context.Configuration.GetSection("Logging");
                if (LoggingSection.GetChildren().Any())
                {
                    builder.AddConfiguration(LoggingSection);
                    if (LoggingSection.GetSection("Console").GetChildren().Any())
                        builder.AddConsole();
                    if (LoggingSection.GetSection("Debug").GetChildren().Any())
                        builder.AddDebug();
                }
            });

        readonly IHostEnvironment Environment;
        readonly ILogger<Program> Logger;
        readonly ILoggerFactory LoggerFactory;
        readonly IOptions<OutputOptions> OutputOptions;
        public Program(IHostEnvironment Environment, ILogger<Program> Logger, IConfiguration Configuration, ILoggerFactory LoggerFactory, IOptions<OutputOptions> OutputOptions, [Option(null, "no logo")] bool nologo = false)
        {
            this.OutputOptions = OutputOptions;
            if (!nologo)
                Console.WriteLine(@"
##############################################################
### Check DDR SSQ/CSQ Analyze tool (C) pumpCurry 2019-2020 ###
##############################################################
");
            this.Environment = Environment;
            this.Logger = Logger;
            this.LoggerFactory = LoggerFactory;
            {
                Logger.LogDebug(nameof(Environment) + ":");
                using var b = Logger.BeginScope(nameof(Environment));
                Logger.LogDebug("[ContentRootPath]: {ContentRootPath}", Environment.ContentRootPath);
                Logger.LogDebug("[Environment]: {EnvironmentName}", Environment.EnvironmentName);
            }
            {
                Logger.LogDebug(nameof(Configuration) + ":");
                using var b = Logger.BeginScope(nameof(Configuration));
                foreach (var c in Configuration.AsEnumerable())
                    Logger.LogDebug("[{key}]: {value}", c.Key, c.Value);
            }
            Logger.LogDebug($"Console.OutputEncoding.WebName:{Console.OutputEncoding.WebName}");
        }
        /// <summary>
        /// ファイル読み込みして情報を表示する機能
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Command("read", "reading SSQ/CSQ information.")]
        public int ReadInfo([Option("i", "input chunk filename.")] string input, [Option("o", "output filename.", DefaultValue = null)] string? output = null)
        {
            Logger.LogDebug("input: {input}", input);
            var Writer = Console.Out;
            StreamWriter? OutFileWriter = null;
            if (!string.IsNullOrEmpty(output))
            {
                Writer = OutFileWriter = new StreamWriter(Path.IsPathRooted(output) ? output : Path.GetFullPath(output, Environment.ContentRootPath), false, new UTF8Encoding(false));
                Logger.LogDebug("output: {output}", output);
            }
            using (OutFileWriter)
            {
                var FullPath = Path.IsPathRooted(input) ? input : Path.GetFullPath(input, Environment.ContentRootPath);
                if (!File.Exists(FullPath))
                {
                    Console.Error.WriteLine($"file not found: {FullPath}");
                    return 9;
                }
                var FileInfo = new FileInfo(FullPath);
                using var Stream = FileInfo.Open(FileMode.Open, FileAccess.Read);
                using var Reader = new ChunkReader(Stream, true)
                {
                    Logger = LoggerFactory.CreateLogger<ChunkReader>(),
                };
                var Options = OutputOptions.Value;
                try
                {
                    var Chunks = Reader.ReadToEnd().ToList();
                    Writer.WriteLine();
                    Writer.WriteLine($"###[ {input} , Length: ({Stream.Length}) Byte(s) ]###");
                    Writer.WriteChunckSummary(Chunks);
                    foreach (var Chunk in Chunks)
                        Writer.WriteChunkBodyInfo(Chunk, Options);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"input: {input}");
                    Console.Error.WriteLine("read error : {0}", e);
                    Logger.LogError(e, "error: {e}", e);
                    return -1;
                }
            }
        }
        [Command("read-dir", "reading SSQ/CSQ information. of dir.")]
        public int ReadInfoDir([Option("i", "view chunk file pattern.")] string input,
            [Option("d", "input directory.", DefaultValue = "./")] string dir = "./",
            [Option("e", "output ext", DefaultValue = ".txt")] string outext = ".txt",
            [Option("o", "output dir")] string? outdir = null,
            [Option("s", "error skip")] bool skip = false)
        {
            var _InputDir = Path.IsPathRooted(dir) ? dir : Path.GetFullPath(dir, Environment.ContentRootPath);
            var _OutputDir = outdir is null ? null : Path.IsPathRooted(outdir) ? outdir : Path.GetFullPath(outdir, Environment.ContentRootPath);
            Logger.LogDebug("inputdir:{inputdir}", _InputDir);
            Logger.LogDebug("outputdir:{outputdir}", _OutputDir);
            int Result = 0;
            foreach (var path in Directory.EnumerateFiles(_InputDir, input))
            {
                var _output = _OutputDir is null ? null : Path.GetFullPath(Path.GetFileNameWithoutExtension(path) + outext, _OutputDir);
                var result = ReadInfo(path, _output);
                if (result is 0)
                    continue;
                if (skip)
                {
                    Result = result;
                    continue;
                }
                return result;
            }
            return Result;
        }
    }
}
