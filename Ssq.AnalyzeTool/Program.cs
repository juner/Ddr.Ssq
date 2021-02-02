using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ssq.Printing;

namespace Ssq.AnalyzeTool
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await CreateDefaultBuilder(args)
                .RunConsoleAppFrameworkAsync<Program>(args);
        }
        static IHostBuilder CreateDefaultBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });

        readonly IHostEnvironment Environment;
        readonly ILogger<Program> Logger;
        public Program(IHostEnvironment Environment, ILogger<Program> Logger, IConfiguration Configuration)
        {
            this.Environment = Environment;
            this.Logger = Logger;
            Console.WriteLine();
            Console.WriteLine("##############################################################");
            Console.WriteLine("### Check DDR SSQ/CSQ Analyze tool (C) pumpCurry 2019-2020 ###");
            Console.WriteLine("##############################################################");
            Console.WriteLine();
            if (Environment.IsDevelopment())
            {
                Console.WriteLine("Configuration:");
                foreach (var c in Configuration.AsEnumerable())
                    Console.WriteLine($"{c.Key} : {c.Value}");
            }
        }

        public int Run([Option(0, "view chunk file.")] string FileName)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
                Directory.SetCurrentDirectory(Environment.ContentRootPath);
            var FullPath = Path.IsPathRooted(FileName) ? FileName : Path.GetFullPath(FileName, Environment.ContentRootPath);
            if (!File.Exists(FullPath))
            {
                Console.WriteLine($"file not found: {FullPath}");
                return 9;
            }
            using var Stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            using var Reader = new ChunkReader(Stream, true, Logger);
            var Chunks = Reader.ReadToEnd().ToList();
            Console.WriteLine();
            Console.WriteLine($"###[ {FileName} , Length: {Stream.Length} Byte(s) ]###");
            Console.Out.WriteChunckSummary(Chunks);
            foreach (var Chunk in Chunks)
                Console.Out.WriteChunkBodyInfo(Chunk);
            return 0;
        }
    }
}
