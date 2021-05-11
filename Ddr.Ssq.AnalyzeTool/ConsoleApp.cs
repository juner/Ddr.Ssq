using System;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleAppFramework;
using Ddr.Ssq.IO;
using Ddr.Ssq.Printing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Ddr.Ssq.AnalyzeTool
{
    class ConsoleApp : ConsoleAppBase
    {
        readonly IHostEnvironment Environment;
        readonly ILogger<ConsoleApp> Logger;
        readonly ILoggerFactory LoggerFactory;
        readonly IOptions<OutputOptions> OutputOptions;
        readonly IConfiguration Configuration;
        public ConsoleApp(
            IHostEnvironment Environment,
            ILogger<ConsoleApp> Logger,
            IConfiguration Configuration,
            ILoggerFactory LoggerFactory,
            IOptions<OutputOptions> OutputOptions)
        {
            this.OutputOptions = OutputOptions;
            this.Environment = Environment;
            this.Logger = Logger;
            this.LoggerFactory = LoggerFactory;
            this.Configuration = Configuration;
        }
        void InitLog(bool nologo, bool verbose)
        {
            if (!nologo)
                Console.WriteLine(@"
##############################################################
### Check DDR SSQ/CSQ Analyze tool (C) pumpCurry 2019-2020 ###
##############################################################
");
            if (verbose)
            {
                var oo = this.OutputOptions.Value;
                Console.WriteLine("ViewOtherBinary:{0}", oo.ViewOtherBinary);
            }
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
        /// read info from file.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        [Command("read", "reading SSQ/CSQ information.")]
        public int ReadInfo(
            [Option("i", "input chunk filename.")] string input,
            [Option("o", "output filename.", DefaultValue = null)] string? output = null,
            [Option(null, "no logo")] bool nologo = false,
            [Option("v")] bool verbose = false)
        {
            InitLog(nologo, verbose);
            return _ReadInfo(input, output, verbose);
        }
        int _ReadInfo(string input, string? output, bool verbose)
        {
            Logger.LogDebug("input: {input}", input);
            if (verbose)
                Console.WriteLine("input: {0}", input);
            var Writer = Console.Out;
            StreamWriter? OutFileWriter = null;
            bool isOutput = false;
            if (!string.IsNullOrEmpty(output))
            {
                output = Path.IsPathRooted(output) ? output : Path.GetFullPath(output, Environment.ContentRootPath);
                var outdir = Path.GetDirectoryName(output);
                if (!Directory.Exists(outdir))
                    Directory.CreateDirectory(outdir!);
                isOutput = true;
                Writer = OutFileWriter = new StreamWriter(output, false, new UTF8Encoding(false));
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
                    if (isOutput)
                    {
                        Logger.LogDebug("output: {output}", output);
                        if (verbose)
                            Console.WriteLine("output: {0}", output);
                    }
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
        /// <summary>
        /// read info from dir
        /// </summary>
        /// <param name="input">input file pattern. </param>
        /// <param name="dir">input directory. default: ./</param>
        /// <param name="outext">output ext. default: .txt</param>
        /// <param name="outdir">output directory. set enable outputfile.</param>
        /// <param name="verbose">verbose</param>
        /// <returns></returns>
        [Command("read-dir", "reading SSQ/CSQ information. of dir.")]
        public int ReadInfoDir([Option("i", "view chunk file pattern.")] string input,
            [Option("d", "input directory.", DefaultValue = "./")] string dir = "./",
            [Option("e", "output ext", DefaultValue = ".txt")] string outext = ".txt",
            [Option("o", "output dir")] string? outdir = null,
            [Option("s", "error skip")] bool skip = false,
            [Option(null, "no logo")] bool nologo = false,
            [Option("v")] bool verbose = false)
        {
            InitLog(nologo, verbose);
            return _ReadInfoDir(input, dir, outext, outdir, skip, verbose);
        }
        int _ReadInfoDir(string input, string dir, string outext, string? outdir, bool skip, bool verbose)
        {
            var _InputDir = Path.IsPathRooted(dir) ? dir : Path.GetFullPath(dir, Environment.ContentRootPath);
            var _OutputDir = outdir is null ? null : Path.IsPathRooted(outdir) ? outdir : Path.GetFullPath(outdir, Environment.ContentRootPath);
            Logger.LogDebug("inputdir:{inputdir}", _InputDir);
            if (verbose)
                Console.WriteLine("inputdir: {0}", _InputDir);
            Logger.LogDebug("outputdir: {outputdir}", _OutputDir);
            if (verbose)
                Console.WriteLine("outputdir: {0}", _OutputDir);
            int Result = 0;
            foreach (var path in Directory.EnumerateFiles(_InputDir, input))
            {
                var _output = _OutputDir is null ? null : Path.GetFullPath(Path.GetFileNameWithoutExtension(path) + outext, _OutputDir);
                var result = _ReadInfo(path, _output, verbose);
                if (verbose)
                    Console.WriteLine("Result: {0}", result);
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
