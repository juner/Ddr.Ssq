using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
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
            => Host.CreateDefaultBuilder();

        readonly IHostEnvironment Environment;
        public Program(IHostEnvironment Environment)
        {
            this.Environment = Environment;
            Console.WriteLine();
            Console.WriteLine("##############################################################");
            Console.WriteLine("### Check DDR SSQ/CSQ Analyze tool (C) pumpCurry 2019-2020 ###");
            Console.WriteLine("##############################################################");
            Console.WriteLine();
        }

        public int Run([Option(0, "view chunk file.")] string FileName)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
                Directory.SetCurrentDirectory(Environment.ContentRootPath);
            if (!File.Exists(FileName))
            {
                Console.WriteLine($"file not found: {FileName}");
                return 9;
            }
            using var Stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            using var Reader = new ChunkReader(Stream, true);
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
