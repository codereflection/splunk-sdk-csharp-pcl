﻿/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

namespace Splunk.Client.UnitTesting
{
    using Splunk.Client;
    using Splunk.Client.Helpers;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Test helper class
    /// </summary>
    public static class TestHelper
    {
        public static int VersionCompare(Service service, string versionToCompare)
        {
            Version info = service.Server.GetInfoAsync().Result.Version;
            string version = info.ToString();
            return (string.Compare(version, versionToCompare, StringComparison.InvariantCulture));
        }

        public static async Task WaitIndexTotalEventCountUpdated(Index index, long expectEventCount, int seconds = 60)
        {
            Stopwatch watch = Stopwatch.StartNew();
            while (watch.Elapsed < new TimeSpan(0, 0, seconds) && index.TotalEventCount != expectEventCount)
            {
                await Task.Delay(1000);
                await index.GetAsync();
            }

            Console.WriteLine("Sleep {0}s to wait index {2} 'TotalEventCount got updated, current index.TotalEventCount={1}", watch.Elapsed, index.TotalEventCount, index.Name);
            Assert.True(index.TotalEventCount == expectEventCount);
        }

        public static async Task RestartServer()
        {
            Stopwatch watch = Stopwatch.StartNew();

            Service service = await SDKHelper.CreateService();
            try
            {
                await service.Server.RestartAsync();
                Console.WriteLine("{1},  spend {0}s to restart server successfully", watch.Elapsed.TotalSeconds, DateTime.Now);
            }
            catch (Exception e)
            {
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("{1}, spend {0}s to restart server failed:", watch.Elapsed.TotalSeconds, DateTime.Now);
                Console.WriteLine(e);
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }

            watch.Stop();
        }

        /// <summary>
        /// Create a fresh test app with the given name, delete the existing
        /// test app and reboot Splunk.
        /// </summary>
        /// <param name="name">The app name</param>
        public async static void CreateApp(string name)
        {
            //EntityCollection<App> apps;

            Service service = await SDKHelper.CreateService();

            ApplicationCollection apps = service.GetApplicationsAsync(new ApplicationCollectionArgs()).Result;

            if (apps.Any(a => a.ResourceName.Title == name))
            {
                service.RemoveApplicationAsync(name).Wait();
                await RestartServer();
                service = await SDKHelper.CreateService();
                apps = service.GetApplicationsAsync().Result;
            }

            Assert.False(apps.Any(a => a.ResourceName.Title == name));

            //apps.Create(name);
            service.CreateApplicationAsync(name, "sample_app").Wait();

            await RestartServer();

            service = await SDKHelper.CreateService();

            apps = service.GetApplicationsAsync().Result;
            Assert.True(apps.Any(a => a.Name == name));
        }

        /// <summary>
        /// Remove the given app and reboot Splunk if needed.
        /// </summary>
        /// <param name="name">The app name</param>
        public static async void RemoveApp(string name)
        {
            Service service = await SDKHelper.CreateService();

            ApplicationCollection apps = service.GetApplicationsAsync().Result;
            if (apps.Any(a => a.Name == name))
            {
                service.RemoveApplicationAsync(name).Wait();
                await RestartServer();
                service = await SDKHelper.CreateService();
            }

            apps = service.GetApplicationsAsync().Result;
            Assert.False(apps.Any(a => a.Name == name));
        }
    }
}
