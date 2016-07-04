# ![Logo](assets/logo_notext.png) ServiceStack.IntroSpec.Raml

A plugin for [ServiceStack](https://servicestack.net/) that uses output from [ServiceStack.IntroSpec](https://github.com/MacLeanElectrical/servicestack-introspec) to generate documentation that complies to the [RAML](http://raml.org/) standard.

---

![Formats](assets/MultiFormat-RAML.png)

The [ServiceStack.IntroSpec](https://github.com/MacLeanElectrical/servicestack-introspec) plugin uses introspection on a number of different sources to generate a set of universal documentation DTOs. ServiceStack.IntroSpec.Raml converts these DTOs to the [RAML 0.8](https://github.com/raml-org/raml-spec/blob/master/versions/raml-08/raml-08.md) specification.

[RAML](http://raml.org/about/about-raml) is a YAML-based specification for describing RESTful API's.

## Quick Start

Install the package [https://www.nuget.org/packages/ServiceStack.IntroSpec](https://www.nuget.org/packages/ServiceStack.IntroSpec.Raml/)
```bash
PM> Install-Package ServiceStack.IntroSpec.Raml
```

This plugin has a dependency on the [`ApiSpecFeature`](https://www.nuget.org/packages/ServiceStack.IntroSpec/) from ServiceStack.IntroSpec but apart from that it is added like any other plugin.
```csharp
public override void Configure(Container container)
{
    SetConfig(new HostConfig
    {
        // Required to know base URL
        WebHostUrl = "http://api.example.com:8001",
    });

	// Dependency
	Plugins.Add(new ApiSpecFeature(....));
	
	// Register plugin
    Plugins.Add(new RamlFeature());
}
```

## Services
The plugin registers a service at `/spec/raml/0.8`. Calling this service will return output with content-type of `application/raml+yaml` that conforms to v0.8 of the RAML specification.

*Support for v1.0 will be added at a later date*
