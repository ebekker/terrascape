# README - `HC.GoPlugin` library

This project provides a library with some convenience wrappers and
logic for implementing a basic plugin in C#/.NET compatible with the
[go-plugin](https://github.com/hashicorp/go-plugin) system over RPC.

## Building

This project has only been tested with .NET Core.

After downloading and installing the latest [.NET Core SDK](https://dotnet.microsoft.com/download)
from the root of this project, you can build the project by invoking the following:

```pwsh
## First time...
> dotnet build

## You should expect to see some build errors, because gRPC proto compiler
## needs to generate some code from the proto definition file first

## Second time, everything should build just fine
> dotnet build
```
