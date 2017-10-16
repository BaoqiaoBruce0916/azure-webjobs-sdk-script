// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Script.WebHost
{
    public class Program
    {
        private static CancellationTokenSource _applicationCts = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            SetupSecrets();
            BuildWebHost(args)
                .RunAsync(_applicationCts.Token)
                .Wait();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        internal static void InitiateShutdown()
        {
            _applicationCts.Cancel();
        }

        internal static void SetupSecrets()
        {
            try
            {
                var path = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
                if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, "local.settings.json")))
                {
                    var obj = JsonConvert.DeserializeObject<LocalSettingsJson>(File.ReadAllText(Path.Combine(path, "local.settings.json")));
                    if (!obj.IsEncrypted)
                    {
                        foreach (var pair in obj.Values)
                        {
                            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
                        }
                    }
                }
            }
            catch { }
        }
    }

    internal class LocalSettingsJson
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public bool IsEncrypted { get; set; }
    }
}