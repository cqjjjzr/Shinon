using NAPS2.Images.ImageSharp;
using NAPS2.Scan;
using Shinon.Node;

try
{
    var config = new Config();
    var scanning = new Scanning(config);
    if (config.CommandLine.ListScanners)
    {
        var devices = await scanning.EnumerateDevices();
        Console.WriteLine("Device found:");
        foreach (var device in devices)
            Console.WriteLine($" - ID: {device.ID}; Name: {device.Name}");
        return;
    }

    await scanning.LoadDevices();
    if (config.CommandLine.TestScan)
    {
        await scanning.TestScan();
        return;
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
