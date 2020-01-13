# README - `HC.GoPlugin.KVExample` plugin

This project implements a working example plugin in C# that is compatible with the
[KV Example](https://github.com/hashicorp/go-plugin/tree/master/examples/grpc) from
the [`go-plugin`](https://github.com/hashicorp/go-plugin) system over RPC.

## Building

This project has only been tested with the latest (pre-release) .NET Core 3.0.

After downloading and installing the latest pre-release
[.NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0)
from the root of this project, you can build this example as follows:

### First, do an initial build:

We need to generated the gRPC proto-derived code, so invoke the following:

```pwsh

## First time, expect this build to *FAIL*
$ dotnet build

```

You should expect to see **errors** -- this is normal because the gRPC proto compiler needs
to process the `kv.proto` file and generate some base derived code which the rest of this
project depends on.

### Next, build the plugin

You can use the build script at `do-publish.ps1` which is compatible with PowerShell Core (PWSH)
and should run on any platform where PWSH is supported.  Alternatively, you can reproduce the
individual steps manually:

* build & publish the `Release` configuration
* create a `go-plugins` folder in the project root
* copy the results of the publish folder to this folder

### Then, clone and build the example main CLI

Clone the repo for [`go-plugin`](https://github.com/hashicorp/go-plugin), then go into the
`examples/grpc` folder and follow the [directions](https://github.com/hashicorp/go-plugin/blob/master/examples/grpc/README.md)
for building the main CLI for the example.  You may want to follow all the directions in
this example just to make sure you have a working setup just using the built-in
Go-based CLI and Go-based plugin.  Here we'll repeat just the necessary step for
the main CLI:

```pwsh

## From the `examples/grpc` folder under your local clone of go-plugin

# This builds the main CLI (for non-Windows)
$ go build -o kv

# On Windows, you want to adjust it to:
$ go build -o kv.exe

```

### Finally, test out the main CLI interface to a C# plugin

Here again you can use the convenience test script at `do-test.ps1` to run the test exercise
for you.  If you do, you should specify the path to the `kv` CLI executable from up above
using the `-GoPluginProgram` flag.

Additionally, you need to specify what *operation* you want the CLI to perform.  This example
implements a very basic, persistent key-value store and lookup, so you can invoke it either
to store a value using the `-Put` flag and its arguments (key and value) or the `-Get` flag
and its argument (key).

(There are additional options available which you can see by doing `help do-test.ps1` in PWSH.)

```PWSH

## Set this to the path to the kv CLI executable you built up above
$pathToKv = "...";

## Example storing a value
$ ./do-test.ps1 -GoPluginProgram $pathToKv -Put key1 value1


## You should see output similar to this:

Using plugins folder [/path/to/HC.GoPlugin.KVExample/go-plugins]
2019-03-06T11:16:20.972-0500 [DEBUG] plugin: starting plugin: path=/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe args=[/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe]
2019-03-06T11:16:20.980-0500 [DEBUG] plugin: plugin started: path=/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe pid=17488
2019-03-06T11:16:20.980-0500 [DEBUG] plugin: waiting for RPC address: path=/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe
2019-03-06T11:16:21.152-0500 [DEBUG] plugin: using plugin: version=1


## Example reading a value
$ ./do-test.ps1 -GoPluginProgram $pathToKv -Get key1


## You should see output similar to this:

Using plugins folder [/path/to/HC.GoPlugin.KVExample/go-plugins]
2019-03-06T11:20:07.515-0500 [DEBUG] plugin: starting plugin: path=/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe args=[/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe]
2019-03-06T11:20:07.519-0500 [DEBUG] plugin: plugin started: path=/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe pid=38388
2019-03-06T11:20:07.519-0500 [DEBUG] plugin: waiting for RPC address: path=/path/to/HC.GoPlugin.KVExample/go-plugins/HC.GoPlugin.KVExample.exe
2019-03-06T11:20:07.688-0500 [DEBUG] plugin: using plugin: version=1
value1


```

If you wish to test out the plugin manually *without* using the `do-test.ps` script, you need to:

* set the environment variable `KV_PLUGIN` to the path for
  your plugin executable in the `go-plugins` local folder
* invoke the `kv` executable from the `examples/grpc` folder,
  specifying the `put` command and 2 arguments, the key and value
* invoke the `kv` executable from the `examples/grpc` folder,
  specifying the `get` command and 1 argument, the key
  
## Implementation Detail

The sample C# plugin implements the backing store using a simple JSON file named `kv.json`
located in the current working directory from which you invoke the main `kv` CLI executable.
If you examine the file you should see it store all the values you *put* into it.
