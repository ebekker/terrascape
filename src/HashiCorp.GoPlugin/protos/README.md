# README - Terraform.Plugin Protos

## GRPC Health Checking Protocol Service Definition

The protobuf description file `health.proto` was pulled from the GRPC [Health Checking Protocol protobuf](https://github.com/grpc/grpc/blob/master/src/proto/grpc/health/v1/health.proto).
A description of this protocol can be found [here](https://github.com/grpc/grpc/blob/master/doc/health-checking.md).

GRPC Health Checking is a requirement of the HashiCorp go-plugin model as described [here](https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#3-add-the-grpc-health-checking-service).

## Plugin Controller

The protobuf description file `grpc_controller.proto` was pulled from
[here](https://github.com/hashicorp/go-plugin/blob/master/internal/plugin/grpc_controller.proto).
There does not appear to be any official documentation for this
optional feature service but the basic usage can be gleaned from
[this sample implementation](https://github.com/hashicorp/go-plugin/blob/master/grpc_controller.go).

## Plugin Broker

The protobuf description file `grpc_broker.proto` was pulled from
[here](https://github.com/hashicorp/go-plugin/blob/master/internal/plugin/grpc_broker.proto).
