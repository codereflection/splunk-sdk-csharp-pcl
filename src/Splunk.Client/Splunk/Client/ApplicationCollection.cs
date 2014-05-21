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

//// TODO:
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client
{
    using System.Runtime.Serialization;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an object representation of a collection of Splunk applications.
    /// </summary>
    public class ApplicationCollection : EntityCollection<ApplicationCollection, Application>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCollection"/>
        /// class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="ns">
        /// An object identifying a Splunk services namespace.
        /// </param>
        /// <param name="args">
        /// </param>
        internal ApplicationCollection(Context context, Namespace ns, ApplicationCollectionArgs args = null)
            : base(context, ns, ClassResourceName, args)
        { }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref=
        /// "ApplicationCollection"/> class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code. Use <see cref=
        /// "Service.GetApplicationsAsync"/> to asynchronously retrieve a 
        /// collection of installed Splunk applications.
        /// </remarks>
        public ApplicationCollection()
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously creates a new application from a template.
        /// </summary>
        /// <param name="name">
        /// Name of the application to be created.
        /// </param>
        /// <param name="template">
        /// Name of the template from which to create the application
        /// </param>
        /// <param name="attributes">
        /// Optional attributes for the application to be created.
        /// </param>
        /// <returns>
        /// Information about the application created.
        /// </returns>
        /// <remarks>
        /// This method uses the <a href="http://goo.gl/SzKzNX">POST 
        /// apps/local</a> endpoint to create the current <see cref=
        /// "Application"/>.
        /// </remarks>
        public async Task<Application> CreateAsync(string name, string template,
            ApplicationAttributes attributes = null)
        {
            var resourceName = ApplicationCollection.ClassResourceName;

            var args = new CreationArgs()
            {
                ExplicitApplicationName = name,
                Filename = false,
                Name = name,
                Template = template
            };

            using (var response = await this.Context.PostAsync(this.Namespace, resourceName, args, attributes))
            {
                await response.EnsureStatusCodeAsync(HttpStatusCode.Created);

                var entity = new Application();
                var feed = new AtomFeed();

                await feed.ReadXmlAsync(response.XmlReader);
                entity.Initialize(this.Context, feed);

                return entity;
            }
        }

        /// <summary>
        /// Asynchronously installs an application from a Splunk application
        /// archive file.
        /// </summary>
        /// <param name="path">
        /// Specifies the location of a Splunk application archive file.
        /// </param>
        /// <param name="name">
        /// Optionally overrides the name of the application.
        /// </param>
        /// <param name="update">
        /// <c>true</c> if Splunk should allow the installation to update an
        /// existing application. The default value is <c>false</c>.
        /// </param>
        /// <returns>
        /// The <see cref="Application"/> installed.
        /// </returns>
        /// <remarks>
        /// This method uses the <a href="http://goo.gl/SzKzNX">POST 
        /// apps/local</a> endpoint to install the application from the archive
        /// file on <paramref name="path"/>.
        /// </remarks>
        public async Task<Application> InstallAsync(string path, string name = null, bool update = false)
        {
            var resourceName = ApplicationCollection.ClassResourceName;

            var args = new CreationArgs()
            {
                ExplicitApplicationName = name,
                Filename = true,
                Name = path,
                Update = update
            };

            using (var response = await this.Context.PostAsync(this.Namespace, resourceName, args))
            {
                await response.EnsureStatusCodeAsync(HttpStatusCode.Created);
                
                var entity = new Application();
                var feed = new AtomFeed();

                await feed.ReadXmlAsync(response.XmlReader);
                entity.Initialize(this.Context, feed);
                
                return entity;
            }
        }

        #endregion

        #region Privates/internals

        internal static readonly ResourceName ClassResourceName = new ResourceName("apps", "local");

        class CreationArgs : Args<CreationArgs>
        {
            [DataMember(Name = "explicit_appname", EmitDefaultValue = false)]
            public string ExplicitApplicationName
            { get; set; }

            [DataMember(Name = "filename", IsRequired = true)]
            public bool? Filename
            { get; set; }

            [DataMember(Name = "name", IsRequired = true)]
            public string Name
            { get; set; }

            [DataMember(Name = "template", EmitDefaultValue = false)]
            public string Template
            { get; set; }

            [DataMember(Name = "update", EmitDefaultValue = false)]
            public bool? Update
            { get; set; }
        }

        #endregion
    }
}
