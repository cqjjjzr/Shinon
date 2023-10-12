using NAPS2.Images.ImageSharp;
using NAPS2.Scan;
using Shinon.Node;

var config = new Config();
if (config.CommandLine.ListScanners)
{
    using var scanningContext = new ScanningContext(new ImageSharpImageContext());
    var controller = new ScanController(scanningContext);

    var devices = await controller.GetDeviceList();
    Console.WriteLine("Device found:");
    foreach (var device in devices)
    {
        Console.WriteLine($" - ID: {device.ID}; Name: {device.Name}");
    }
}


var options = new ScanOptions
{
};