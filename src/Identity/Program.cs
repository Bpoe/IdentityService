namespace Identity;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

public static class Program
{
    public static void Main(string[] args)
    {
        var isService = !(Debugger.IsAttached || args.Contains("--console"));

        if (isService)
        {
            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            Directory.SetCurrentDirectory(pathToContentRoot);
        }

        var builder = WebHost
            .CreateDefaultBuilder(
                args.Where(arg => arg != "--console").ToArray())
            .UseStartup<Startup>();

        using var host = builder.Build();
        
        if (isService)
        {
            host.RunAsService();
        }
        else
        {
            host.Run();
        }
    }
}