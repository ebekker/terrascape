# README - Terraform.Plugin Protos

## GRPC Health Checking Protocol Service Definition

The protobuf description file `health.proto` was pulled from the GRPC [Health Checking Protocol protobuf](https://github.com/grpc/grpc/blob/master/src/proto/grpc/health/v1/health.proto).
A description of this protocol can be found [here](https://github.com/grpc/grpc/blob/master/doc/health-checking.md).

GRPC Health Checking is a requirement of the HashiCorp go-plugin model as described [here](https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#3-add-the-grpc-health-checking-service).


## Terraform Plugin Service Definition

The protobuf description file `tfplugin5.1.proto` was pulled in from
the [official source](https://raw.githubusercontent.com/hashicorp/terraform/v0.12.18/docs/plugin-protocol/tfplugin5.1.proto),
namely the TF plugin protocol documentation of the latest tagged TF
release at the time, `0.12.18`.
