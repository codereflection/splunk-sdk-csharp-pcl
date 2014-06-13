﻿   /*
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

namespace Splunk.Client.Helpers
{
    using Splunk.Client;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public static class SDKHelper
    {
        /// <summary>
        /// Initializes the <see cref="SDKHelper" /> class.
        /// </summary>
        static SDKHelper()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                return true;
            };

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            splunk = new SplunkRC(Path.Combine(home, ".splunkrc"));
        }

        /// <summary>
        /// Gets the user configure.
        /// </summary>
        /// <value>
        /// The user configure.
        /// </value>
        public static SplunkRC Splunk
        {
            get { return splunk; }
        }

        /// <summary>
        /// Create a Splunk 
        /// <see cref="Service" /> and login using the command
        /// line options (or .splunkrc)
        /// </summary>
        /// <param name="ns">The ns.</param>
        /// <returns>
        /// The service created.
        /// </returns>
        public static async Task<Service> CreateService(Namespace ns = null, 
            [CallerFilePath]string callerFilePath = null, 
            [CallerLineNumber]int? callerLineNumber = null,
            [CallerMemberName]string callerMemberName = null)
        {
            string callerId = string.Join(".", Path.GetFileNameWithoutExtension(callerFilePath), callerMemberName);
            Service service;

            if (MockContext.IsEnabled)
            {
                var context = new FakeContext(Splunk.Scheme, Splunk.Host, Splunk.Port, callerId);
                service = new Service(context);
            }
            else
            {
                service = new Service(Splunk.Scheme, Splunk.Host, Splunk.Port, ns);
            }

            await service.LoginAsync(Splunk.Username, Splunk.Password);
            return service;
        }

        #region Privates/internals

        static readonly SplunkRC splunk;

        #endregion

        #region Types

        /// <summary>
        /// 
        /// </summary>
        public sealed class SplunkRC
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SplunkRC"/> class.
            /// </summary>
            /// <param name="path">
            /// The location of a .splunkrc file.
            /// </param>
            internal SplunkRC(string path)
            {
                var reader = new StreamReader(path);

                List<string> argList = new List<string>(4);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("#", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    if (line.Length == 0)
                    {
                        continue;
                    }

                    argList.Add(line);
                }

                foreach (string arg in argList)
                {
                    string[] pair = arg.Split('=');

                    switch (pair[0].ToLower().Trim())
                    {
                        case "scheme": 
                            this.Scheme = pair[1].Trim() == "https" ? Scheme.Https : Scheme.Http; 
                            break;
                        case "host": 
                            this.Host = pair[1].Trim(); 
                            break;
                        case "port": 
                            this.Port = int.Parse(pair[1].Trim()); 
                            break;
                        case "username": 
                            this.Username = pair[1].Trim(); 
                            break;
                        case "password": 
                            this.Password = pair[1].Trim(); 
                            break;
                    }
                }
            }

            /// <summary>
            /// The scheme
            /// </summary>
            public Scheme Scheme = Scheme.Https;

            /// <summary>
            /// The host
            /// </summary>
            public string Host = "localhost";

            /// <summary>
            /// The port
            /// </summary>
            public int Port = 8089;

            /// <summary>
            /// The username
            /// </summary>
            public string Username = "admin";
            
            /// <summary>
            /// The password
            /// </summary>
            public string Password = "changeme";
        }

        #endregion
    }
}
