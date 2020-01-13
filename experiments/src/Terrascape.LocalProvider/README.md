# README - `LocalProvider` TF Plugin

This is the first fully functional TF Plugin Provider implemented in C#.

It implements a few local Data Sources and Resources to showcase proper operation
and interaction with the Terraform entry-point process.

This plugin leverages the start of a general-purpose framework for developing C#/.NET-based TF
plugins, [HC.TFPlugin](../HC.TFPlugin).  The general approach is to create your model classes
that are decorated with attributes to help describe and qualify your plugin's schema for the
main Provider and its Resources/Data Sources.

From this, the framework can generate a proper schema description to provide to the main TF
engine.  Additionally, the framework will handle incoming requests from the engine, handle
all the necessary (de)serialization, and invoke the corresponding handler methods on the
provider class instance for each stage of the TF operations lifecycle.
