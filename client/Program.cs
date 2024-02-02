using Greet;
using Grpc.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50051";

        static async Task Main(string[] args)
        {

            var clientCert = File.ReadAllText("ssl/client.crt");
            var clientKey = File.ReadAllText("ssl/client.key");
            var caCrt = File.ReadAllText("ssl/ca.crt");
            var channelCredentials = new SslCredentials(caCrt, new KeyCertificatePair(clientKey, clientKey));
/*            Channel channel = new Channel("localhost", 50051, channelCredentials);*/

            Channel channel = new Channel(target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task)  =>
            {
                if (task.Status == TaskStatus.RanToCompletion) 
                    Console.WriteLine("client connected successfully");
            });

            var client = new GreetingService.GreetingServiceClient(channel);

            /*            DoSimpleGreet(client);*/
            await DoGreetManyTimes(client);
            /*            await DoLongGreet(client);*/
            /*            await DoGreetEveryone(client);*/
            /*            await DoDeadlineGreeting(client);*/

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        public static void DoSimpleGreet(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Oliver",
                LastName = "Miller"
            };

            var request = new GreetingRequest()
            {
                Greeting = greeting
            };

            var response = client.Greet(request);

            Console.WriteLine(response.Result);
        }

        public static async Task DoGreetManyTimes(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Oliver",
                LastName = "Miller"
            };

            var request = new GreetManyTimesRequest()
            {
                Greeting = greeting
            };

            var response = client.GreetManyTimes(request);
            Console.WriteLine(response);
            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
        }

        public static async Task DoLongGreet(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Oliver",
                LastName = "Miller"
            };

            var request = new LongGreetRequest()
            {
                Greeting = greeting
            };

            var stream = client.LongGreet();

            foreach (int i in Enumerable.Range(1, 10))
            {
                await stream.RequestStream.WriteAsync(request);
            }

            await stream.RequestStream.CompleteAsync();

            var response = await stream.ResponseAsync;
            Console.WriteLine(response.Result);
        }

        public static async Task DoGreetEveryone(GreetingService.GreetingServiceClient client)
        {
            var stream = client.GreetEveryone();

            var responseReaderTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine("recieved : " + stream.ResponseStream.Current.Result);
                }
            });

            Greeting[] greetings =
            {
                new Greeting() { FirstName = "John", LastName = "Doe" },
                new Greeting() { FirstName = "Jane", LastName = "Doe" },
                new Greeting() { FirstName = "Jill", LastName = "Doe" }
            };

            foreach(var greeting in greetings)
            {
                Console.WriteLine("sending: " + greeting.ToString());
                await stream.RequestStream.WriteAsync(new GreetEveryoneRequest()
                {
                    Greeting = greeting
                });
            }

            await stream.RequestStream.CompleteAsync();
            await responseReaderTask;
        }

        public static async Task DoDeadlineGreeting(GreetingService.GreetingServiceClient client)
        {
            try
            {
                var response = client.DeadlineGreeting(new DeadlineGreetRequest()
                {
                    Name = "Oliver"
                },
                deadline: DateTime.UtcNow.AddMilliseconds(400));
                Console.WriteLine(response.Result);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine("error: " +  ex.Status.Detail);
            }
        }
    }
}
