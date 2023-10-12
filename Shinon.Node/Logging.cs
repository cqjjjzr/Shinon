using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Shinon.Node
{
    internal class Logging
    {
        public static ILoggerFactory Factory {get;} = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
    }
}
