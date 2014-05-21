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
    using Splunk.Client.Helpers;
    using Xunit;

    public class TestContext : IUseFixture<AcceptanceTestingSetup>
    {
        [Trait("class", "Context")]
        [Fact]
        public void CanConstructContext()
        {
            client = new Context(SDKHelper.UserConfigure.scheme, SDKHelper.UserConfigure.host, SDKHelper.UserConfigure.port);

            Assert.Equal(client.Scheme, Scheme.Https);
            Assert.Equal(client.Host, "localhost");
            Assert.Equal(client.Port, 8089);
            Assert.Null(client.SessionKey);

            Assert.Equal(client.ToString(), "https://localhost:8089");
        }

        static Context client;

        public void SetFixture(AcceptanceTestingSetup data)
        { }
    }
}
