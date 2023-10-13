using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Tomlyn;
using ZXing;

namespace Shinon.Node
{
    internal class Config
    {
        public CommandLineOptions CommandLine { get; private set; } = new();
        public ConfigFileOptions ConfigFile { get; private set; } = new();

        public Config()
        {
            LoadCommandLine();
            LoadConfig();
        }

        private void LoadCommandLine()
        {
            Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs())
                .WithParsed(o => CommandLine = o)
                .WithNotParsed(err => { Environment.Exit(-1); });
        }

        private void LoadConfig()
        {
            if (!File.Exists(CommandLine.ConfigFile))
                return;
            try
            {
                ConfigFile =
                    Toml.ToModel<ConfigFileOptions>(File.ReadAllText(CommandLine.ConfigFile), CommandLine.ConfigFile);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read config file {CommandLine.ConfigFile}: {e}");
            }
        }

        public class CommandLineOptions
        {
            [Option(
                'c', "config",
                Default = "config.toml", HelpText = "Config TOML file path.",
                Required = false
            )]
            public string ConfigFile { get; set; } = "config.toml";

            [Option(
                'l', "list",
                Default = false, HelpText = "List scanners, but don't run the server.",
                Required = false
            )]
            public bool ListScanners { get; set; } = false;

            [Option(
                't', "test",
                Default = false, HelpText = "Scan a test picture with any scanner, and the exit.",
                Required = false
            )]
            public bool TestScan { get; set; } = false;
        }

        public class ConfigFileOptions
        {
            public ScannerOptions Scanner { get; set; } = new();
            public RpcOptions Rpc { get; set; } = new();
        }

        public class ScannerOptions
        {
            public string Driver { get; set; } = GetPlatformDefaultDriver();
            public List<string> Scanners { get; set; } = new();

            private static string GetPlatformDefaultDriver()
            {
                return Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => "wia",
                    PlatformID.Unix => "sane",
                    _ => "escl"
                };
            }
        }

        public class RpcOptions
        {
            public string Url { get; set; } = "http://localhost:9900";
        }
    }
}