using Blog;
using Greet;
using Grpc.Core;
using Prime;
using server_exercise;
using System;
using System.IO;

namespace server
{
    internal class Program
    {
        const int Port = 50052;

        static void Main(string[] args)
        {
            Server server = null;

            try
            {
                server = new Server()
                {
/*                    Services = { PrimeNumberService.BindService(new PrimeNumberServiceImp()) },*/
                    Services = { BlogService.BindService(new BlogServiceImpl()) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine("server listening on port : " + Port);
                Console.ReadKey();
            }
            catch (IOException ex)
            {
                Console.WriteLine("server failed to start : " + ex.Message);
                throw;
            }
            finally
            {
                if (server != null)
                {
                    server.ShutdownAsync().Wait();
                }
            }
        }
    }
}
