// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: kv.proto
// </auto-generated>
// Original file comments:
// From:
//  https://github.com/hashicorp/go-plugin/blob/master/examples/grpc/proto/kv.proto
//
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Proto {
  public static partial class KV
  {
    static readonly string __ServiceName = "proto.KV";

    static readonly grpc::Marshaller<global::Proto.GetRequest> __Marshaller_proto_GetRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Proto.GetRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Proto.GetResponse> __Marshaller_proto_GetResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Proto.GetResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Proto.PutRequest> __Marshaller_proto_PutRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Proto.PutRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Proto.Empty> __Marshaller_proto_Empty = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Proto.Empty.Parser.ParseFrom);

    static readonly grpc::Method<global::Proto.GetRequest, global::Proto.GetResponse> __Method_Get = new grpc::Method<global::Proto.GetRequest, global::Proto.GetResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Get",
        __Marshaller_proto_GetRequest,
        __Marshaller_proto_GetResponse);

    static readonly grpc::Method<global::Proto.PutRequest, global::Proto.Empty> __Method_Put = new grpc::Method<global::Proto.PutRequest, global::Proto.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Put",
        __Marshaller_proto_PutRequest,
        __Marshaller_proto_Empty);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Proto.KvReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of KV</summary>
    public abstract partial class KVBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Proto.GetResponse> Get(global::Proto.GetRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Proto.Empty> Put(global::Proto.PutRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for KV</summary>
    public partial class KVClient : grpc::ClientBase<KVClient>
    {
      /// <summary>Creates a new client for KV</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public KVClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for KV that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public KVClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected KVClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected KVClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Proto.GetResponse Get(global::Proto.GetRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Get(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Proto.GetResponse Get(global::Proto.GetRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Get, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Proto.GetResponse> GetAsync(global::Proto.GetRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Proto.GetResponse> GetAsync(global::Proto.GetRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Get, null, options, request);
      }
      public virtual global::Proto.Empty Put(global::Proto.PutRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Put(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Proto.Empty Put(global::Proto.PutRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Put, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Proto.Empty> PutAsync(global::Proto.PutRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return PutAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Proto.Empty> PutAsync(global::Proto.PutRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Put, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override KVClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new KVClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(KVBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Get, serviceImpl.Get)
          .AddMethod(__Method_Put, serviceImpl.Put).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, KVBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_Get, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Proto.GetRequest, global::Proto.GetResponse>(serviceImpl.Get));
      serviceBinder.AddMethod(__Method_Put, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Proto.PutRequest, global::Proto.Empty>(serviceImpl.Put));
    }

  }
}
#endregion