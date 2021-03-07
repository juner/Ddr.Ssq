using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Ddr.Ssq.IO;
using Ddr.Ssq.Printing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
            .ConfigureServices((context, services) => {
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
        /// <param name="FileName"></param>
        /// <returns></returns>
        [Command("read", "reading SSQ/CSQ information.")]
        public int ReadInfo([Option(0, "view chunk file.")] string FileName, [Option("o", "output filename.", DefaultValue = null)] string? output = null)
        {
            var Writer = Console.Out;
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
                Directory.SetCurrentDirectory(Environment.ContentRootPath);
            StreamWriter? OutFileWriter = null;
            if (!string.IsNullOrEmpty(output))
                Writer = OutFileWriter = new StreamWriter(Path.IsPathRooted(output) ? output : Path.GetFullPath(output, Environment.ContentRootPath), false, new UTF8Encoding(false));
            using (OutFileWriter)
            {   
                var FullPath = Path.IsPathRooted(FileName) ? FileName : Path.GetFullPath(FileName, Environment.ContentRootPath);
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
                    Writer.WriteLine($"###[ {FileName} , Length: ({Stream.Length}) Byte(s) ]###");
                    Writer.WriteChunckSummary(Chunks);
                    foreach (var Chunk in Chunks)
                        Writer.WriteChunkBodyInfo(Chunk, Options);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("read error : {0}", e);
                    Logger.LogError(e, "error: {e}", e);
                    return -1;
                }
            }
        }
        [Command("adjust", "reading SSQ/CSQ information.")]
        public int Adjust([Option("i", "input chunk file.")] string input, [Option("o", "output chunk file.")]string output)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
                Directory.SetCurrentDirectory(Environment.ContentRootPath);
            var InputFullPath = Path.IsPathRooted(input) ? input : Path.GetFullPath(input, Environment.ContentRootPath);
            var OutputFullPath = Path.IsPathRooted(output) ? output : Path.GetFullPath(output, Environment.ContentRootPath);
            if (!File.Exists(InputFullPath))
            {
                Console.Error.WriteLine($"input file not found: {InputFullPath}");
                return -1;
            }
            var OutDir = Path.GetDirectoryName(OutputFullPath)!;
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);
            Chunk[] Chunks;
            {
                // read file
                var InputFile = new FileInfo(InputFullPath);
                using var InputStream = InputFile.OpenRead();
                using var Reader = new ChunkReader(InputStream)
                {
                    Logger = LoggerFactory.CreateLogger<ChunkReader>(),
                };
                Chunks = Reader.ReadToEnd().ToArray();
            }
            {
                foreach(var Chunk in Chunks)
                {
                    if (Chunk.Body is BiginFinishConfigBody Body)
                    {
                        var Entries = Body.GetEntries();
                        var Node = Entries.First;
                        if (Node is null)
                            continue;
                        // TODO 
                    }
                }
            }
            {
                var OutputFile = new FileInfo(OutputFullPath);
                using var OutputStream = OutputFile.Create();

            }
            return 0;
        }
    }
}
