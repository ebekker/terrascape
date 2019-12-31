using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;

namespace HashiCorp.GoPlugin.KVExample
{
    class Program
    {
        static async Task Go(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            var channel = GrpcChannel.ForAddress("http://127.0.0.1:3000");

            // var h = new Grpc.Health.V1.Health.HealthClient();
            // h.

            var client =  new Proto.KV.KVClient(channel);
            //var reply = await client.GetAsync(new Proto.GetRequest { Key = "Foo" });
            var reply = await client.PutAsync(new Proto.PutRequest
            {
                Key = "Foo",
                Value = ByteString.CopyFromUtf8("FOO1")
            });

            // var reply = await client.SayHelloAsync(
            //                   new HelloRequest { Name = "GreeterClient" });

            //Console.WriteLine("Greeting: " + reply.Value);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
