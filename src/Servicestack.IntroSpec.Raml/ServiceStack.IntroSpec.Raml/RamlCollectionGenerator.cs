// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace Servicestack.IntroSpec.Raml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using ServiceStack;
    using ServiceStack.IntroSpec.Extensions;
    using ServiceStack.IntroSpec.Models;
    using ServiceStack.IntroSpec.Raml.JsonSchema;
    using ServiceStack.IntroSpec.Raml.Models;
    using ServiceStack.IntroSpec.Raml.v08;
    using ServiceStack.Logging;
    using ServiceStack.Text;

    public class RamlCollectionGenerator
    {
        private readonly ILog log = LogManager.GetLogger(typeof(RamlCollectionGenerator));
        private readonly HashSet<string> allowedFormats;

        public RamlCollectionGenerator(HashSet<string> allowedFormats)
        {
            this.allowedFormats = allowedFormats;
        }

        public RamlSpec Generate(ApiDocumentation documentation)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            log.Debug($"Generating Raml Spec for service {documentation.Title}");

            var ramlSpec = new RamlSpec();
            SetBasicInformation(documentation, ramlSpec);

            using (var jsConfig = JsConfig.BeginScope())
            {
                jsConfig.EmitCamelCaseNames = true; // Required for serialising JSON schema
                SetResources(documentation, ramlSpec);
            }

            stopwatch.Stop();
            log.Debug($"Generated Raml Spec for resource {documentation.Title}. Took {stopwatch.ElapsedMilliseconds}ms");
            return ramlSpec;
        }

        private void SetBasicInformation(ApiDocumentation documentation, RamlSpec ramlSpec)
        {
            ramlSpec.Title = documentation.Title;
            ramlSpec.Version = documentation.ApiVersion;
            ramlSpec.BaseUri = documentation.ApiBaseUrl;
        }

        private void SetResources(ApiDocumentation documentation, RamlSpec ramlSpec)
        {
            // Iterate through resources
            foreach (var resource in documentation.Resources)
            {
                log.Debug($"Processing resource {resource.Title}");

                // For each resource iterate through it's actions
                foreach (var action in resource.Actions)
                {
                    log.Debug($"Processing action {action.Verb} for resource {resource.Title}");

                    // For each action, go through relative paths
                    foreach (var path in action.RelativePaths)
                    {
                        var workingSet = GenerationUtilities.GenerateWorkingSet(path, resource);

                        log.Debug($"Processing path {path} for action {action.Verb} for resource {resource.Title}");

                        // Check if this path already exists in the map
                        bool isNewResource;
                        var ramlResource = GetRamlResource(ramlSpec, workingSet, resource, out isNewResource);
                        
                        ramlResource.UriParameters = GetUriParameters(ramlResource, action, workingSet, path);

                        var method = GetActionMethod(action, resource, workingSet);

                        ramlResource.Methods.Add(action.Verb.ToLower(), method);

                        if (isNewResource)
                        {
                            var resourcePath = ramlResource.HasMediaTypeExtension()
                                                   ? workingSet.MediaTypeExtensionPath 
                                                   : workingSet.BasePath;

                            ramlSpec.Resources.Add(resourcePath, ramlResource);
                        }
                    }
                }
            }
        }

        private RamlMethod GetActionMethod(ApiAction action, ApiResourceDocumentation resource, RamlWorkingSet ramlWorkingSet)
        {
            var method = new RamlMethod { Description = action.Notes };

            method.Body = new RamlBody
            {
                JsonSchema = new RamlSchema { Schema = JsonSchemaGenerator.Generate(resource).ToJson() }
            };

            var hasRequestBody = action.Verb.HasRequestBody();
            if (!hasRequestBody)
                method.QueryParameters = GenerationUtilities.GetQueryStringLookup(resource, ramlWorkingSet);
            return method;
        }

        private RamlResource GetRamlResource(RamlSpec ramlSpec, RamlWorkingSet ramlWorkingSet, ApiResourceDocumentation resource,
            out bool newResource)
        {
            RamlResource ramlResource;
            newResource = false;

            foreach (var path in ramlWorkingSet.AvailablePaths)
            {
                if (ramlSpec.Resources.TryGetValue(path, out ramlResource))
                {
                    log.Debug($"Found raml resource for path {path}");
                    return ramlResource;
                }
            }

            newResource = true;
            ramlResource = new RamlResource
            {
                DisplayName = resource.Title,
                Description = resource.Description
            };
            log.Debug($"Did not find raml resource for path {ramlWorkingSet.BasePath}");

            return ramlResource;
        }

        private Dictionary<string, RamlNamedParameter> GetUriParameters(RamlResource ramlResource, ApiAction action, RamlWorkingSet ramlWorkingSet, RelativePath path)
        {
            // Process path parameters
            var uriParams = ramlResource.UriParameters ?? new Dictionary<string, RamlNamedParameter>();

            foreach (var pathParam in ramlWorkingSet.PathParams.Distinct())
                uriParams.Add(pathParam.Key, pathParam.NamedParam);

            // TODO - only process these for GET methods?
            if (!path.IsAutoRoute)
            {
                log.Debug($"Path {path.Path} is auto route. Processing media type extensions");
                ProcessMediaTypeExtensions(action, uriParams);
            }

            return uriParams;
        }

        /// <summary>
        /// MediaTypeExtension is a reserved path name which specifies that adding known extension is equivalent of sending accept header
        /// e.g. appending .json == accept:application/json
        /// </summary>
        /// <remarks>see https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md#template-uris-and-uri-parameters </remarks>
        private void ProcessMediaTypeExtensions(ApiAction action, Dictionary<string, RamlNamedParameter> uriParams)
        {
            if (uriParams.ContainsKey(Constants.MediaTypeExtensionKey)) return;

            var extensions = new Dictionary<string, string>();
            foreach (var contentType in action.ContentTypes)
            {
                try
                {
                    // Get friendly name
                    var extension = MimeTypes.GetExtension(contentType);

                    // TODO - filter out soap requests - any others?
                    // Only add mediaTypeExtensions in the path is an auto-route
                    if (allowedFormats.Contains(extension))
                        extensions.Add(contentType, extension);

                    log.Debug($"Found extension {extension} for {contentType}");
                }
                catch (NotSupportedException nse)
                {
                    log.Warn($"Mime Type {contentType} not supported.", nse);
                }
            }

            if (extensions.IsNullOrEmpty()) return;

            // Generates message like "Use .json to specify application/json or .xml to specify text/xml"
            var message = string.Concat("Use ",
                string.Join(" or ", extensions.Select(s => $"{s.Value} to specify {s.Key}")));

            // Create a dummy parameter for mediaTypeExtensions
            uriParams.Add(Constants.MediaTypeExtensionKey,
                new RamlNamedParameter { Enum = extensions.Select(s => s.Value), Description = message });
        }
    }
}