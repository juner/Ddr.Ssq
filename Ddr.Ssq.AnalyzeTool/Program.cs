﻿using System;
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
        public Program(IHostEnvironment Environment, ILogger<Program> Logger, IConfiguration Configuration, ILoggerFactory LoggerFactory)
        {
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
        public int ReadInfo([Option(0, "view chunk file.")] string FileName)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
                Directory.SetCurrentDirectory(Environment.ContentRootPath);
            var FullPath = Path.IsPathRooted(FileName) ? FileName : Path.GetFullPath(FileName, Environment.ContentRootPath);
            if (!File.Exists(FullPath))
            {
                Console.WriteLine($"file not found: {FullPath}");
                return 9;
            }
            var FileInfo = new FileInfo(FullPath);
            using var Stream = FileInfo.Open(FileMode.Open, FileAccess.Read);
            using var Reader = new ChunkReader(Stream, true)
            {
                Logger = LoggerFactory.CreateLogger<ChunkReader>(),
            };
            try
            {
                var Chunks = Reader.ReadToEnd().ToList();
                Console.WriteLine();
                Console.WriteLine($"###[ {FileName} , Length: ({Stream.Length}) Byte(s) ]###");
                Console.Out.WriteChunckSummary(Chunks);
                foreach (var Chunk in Chunks)
                    Console.Out.WriteChunkBodyInfo(Chunk);
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
}
