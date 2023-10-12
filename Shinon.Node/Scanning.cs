using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAPS2.Images.ImageSharp;
using NAPS2.Scan;

namespace Shinon.Node
{
    internal class Scanning
    {
        private static readonly ILogger<Scanning> _logger = Logging.Factory.CreateLogger<Scanning>();

        private readonly Config _config;
        private readonly ScanningContext _scanningContext = new(new ImageSharpImageContext());
        private readonly ScanController _scanController;
        private readonly Driver _scanDriver;

        private readonly Dictionary<string, ScanDevice> _devices = new();

        public Scanning(Config config)
        {
            _config = config;
            _scanDriver = ParseScanDriverName(config.ConfigFile.Scanner.Driver);

            _scanController = new ScanController(_scanningContext);
        }

        public async Task<IEnumerable<ScanDevice>> EnumerateDevices()
        {
            return await _scanController.GetDeviceList(_scanDriver);
        }

        public async Task LoadDevices()
        {
            var requestedDevices = _config.ConfigFile.Scanner.Scanners;
            var devices = await _scanController.GetDeviceList(_scanDriver);
            foreach (var deviceId in requestedDevices)
            {
                var device = devices.Find(it => it.ID == deviceId);
                if (device != null)
                    _devices.Add(deviceId, device);
                else
                    _logger.LogError("Device ID not found: {deviceId}", deviceId);
            }
            if (_devices.Count == 0)
                _logger.LogWarning("No device is loaded.");
        }

        private static Driver ParseScanDriverName(string scannerDriver)
        {
            return scannerDriver switch {
                "wia" => Driver.Wia,
                "sane" => Driver.Sane,
                "apple" => Driver.Apple,
                "escl" => Driver.Escl,
                "default" => Driver.Default,
                _ => throw new NotSupportedException($"Invalid scan driver {scannerDriver}! Supported: wia, escl, sane, apple, and default.")
            };
        }
    }
}