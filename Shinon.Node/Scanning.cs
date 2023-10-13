using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAPS2.Images;
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
            _scanController.ScanError += OnScanError;
            _scanController.ScanStart += OnScanStart;
            _scanController.ScanEnd += OnScanEnd;
            _scanController.PageStart += OnPageStart;
            _scanController.PageEnd += OnPageEnd;
            _scanController.PropagateErrors = true;
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

        public async Task TestScan()
        {
            if (_devices.Count == 0)
            {
                _logger.LogError("No device available, not executing test scan.");
                return;
            }

            try
            {
                var device = _devices.First().Value;
                _logger.LogInformation($"Scanning test page using device {device.Name}...");
                var testOptions = new ScanOptions
                {
                    Device = device,
                    UseNativeUI = false,
                    PageSize = PageSize.A4,

                };
                var i = 1;
                await foreach (var image in _scanController.Scan(testOptions))
                {
                    _logger.LogInformation("Test page - Page size: {pageSize}", image.Metadata.PageSize);
                    _logger.LogInformation("Test page - Bit depth: {bitDepth}", image.Metadata.BitDepth);
                    image.Save($"page{i++}.png", ImageFileFormat.Png);
                }

                _logger.LogInformation("Test scan finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scan test page.");
            }
        }

        private void OnScanError(object? sender, ScanErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Failed to scan.");
        }

        private void OnScanStart(object? sender, EventArgs e)
        {
            _logger.LogDebug("Starting scan...");
        }

        private void OnScanEnd(object? sender, EventArgs e)
        {
            _logger.LogDebug("Scan ended.");
        }

        private void OnPageStart(object? sender, PageStartEventArgs e)
        {
            _logger.LogDebug("Starting page {number}...", e.PageNumber);
        }

        private void OnPageEnd(object? sender, PageEndEventArgs e)
        {
            _logger.LogDebug("Page {number} ended.", e.PageNumber);
        }
    }
}