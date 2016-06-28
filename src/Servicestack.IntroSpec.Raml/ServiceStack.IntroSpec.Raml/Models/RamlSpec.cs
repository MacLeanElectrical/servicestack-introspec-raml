﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.IntroSpec.Raml.Models
{
    using System.Collections.Generic;
    using NamedParameterMap = System.Collections.Generic.Dictionary<string, RamlNamedParameter<string>>;

    /// <summary>
    /// Represents the basic information for RAML output
    /// </summary>
    public class RamlSpec
    {
        // https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md#basic-information
        public string Title { get; set; }
        public string Version { get; set; }
        public string BaseUri { get; set; }
        public string MediaType { get; set; }
        public string[] Protocols { get; set; }
        public string[] Schemas { get; set; }
        // Uri Parameters

        // https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md#user-documentation
        public RamlDocumentation[] Documentation { get; set; }

        // https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md#resources-and-nested-resources
        public Dictionary<string, RamlResource> Resources { get; } = new Dictionary<string, RamlResource>();
    }

    public class RamlDocumentation
    {
        public string Title { get; set; }

        // TODO This may be mardown or !include content
        public string Content { get; set; }
    }

    public class RamlResource
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }

        // https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md#resources-and-nested-resources
        public Dictionary<string, RamlResource> Resources { get; } = new Dictionary<string, RamlResource>();

        // Uri Parameters - same logic as postman parameters
        // Base Uri Parameters

        // https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md#methods
        // Key == method
        public Dictionary<string, RamlMethod> Methods { get; } = new Dictionary<string, RamlMethod>();
    }

    public class RamlMethod
    {
        public string Description { get; set; }

        // Key == full header name (x-my-header)
        public NamedParameterMap Headers { get; } = new Dictionary<string, RamlNamedParameter<string>>();

        public string[] Protocols { get; set; }

        public NamedParameterMap QueryStrings { get; } = new Dictionary<string, RamlNamedParameter<string>>();

        // Body
    }

    // NOTE <T> may not be the best way to handle this
    // TODO Named parameters with multiple types - is that possible in SS?
    public class RamlNamedParameter<T>
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string[] Enum { get; set; }
        public string Pattern { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Example { get; set; }
        public bool Repeat { get; set; }
        public bool Required { get; set; }
        public T Default { get; set; }
    }
}
