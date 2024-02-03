using Blog;
using Greet;
using Grpc.Core;
using Prime;
using System;
using System.Threading.Tasks;

namespace client_exercise
{
    internal class Program
    {
        const string target = "127.0.0.1:50052";

        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("client connected successfully");
            });

            var client = new BlogService.BlogServiceClient(channel);

            var newBlog = CreateBlog(client);
/*            ReadBlog(client);*/
/*            UpdateBlog(client, newBlog);*/
            DeleteteBlog(client, newBlog);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        private static Blog.Blog CreateBlog(BlogService.BlogServiceClient client)
        {
            var response = client.CreateBlog(new CreateBlogRequest()
            {
                Blog = new Blog.Blog()
                {
                    AuthorId = "Oliver",
                    Title = "Hello",
                    Content = "World",
                }
            });

            Console.WriteLine("the blog " + response.Blog.Id + " was created");

            return response.Blog;
        }

        private static void ReadBlog(BlogService.BlogServiceClient client)
        {
            try
            {
                var response = client.ReadBlog(new ReadBlogRequest()
                {
                    BlogId = "65be76223c3c04539d94a93d"
                });
                Console.WriteLine(response.Blog.ToString());
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private async static void PrimeNumberService(Channel channel)
        {
            var client = new PrimeNumberService.PrimeNumberServiceClient(channel);

            var request = new PrimeNumberDecompositionRequest()
            {
                Number = 120
            };

            var response = client.PrimeNumberDecomposition(request);

            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.PrimeFactor);
                await Task.Delay(200);
            }
        }

        private static void UpdateBlog(BlogService.BlogServiceClient client, Blog.Blog blog)
        {
            try
            {
                blog.AuthorId = "Updated author";
                blog.Title = "Updated title";
                blog.Content = "Updated content";

                var response = client.UpdateBlog(new UpdateBlogRequest()
                {
                    Blog = blog
                });

                Console.WriteLine(response.Blog.ToString());
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static void DeleteteBlog(BlogService.BlogServiceClient client, Blog.Blog blog)
        {
            try
            {
                var response = client.DeleteBlog(new DeleteBlogRequest()
                {
                    BlogId = blog.Id
                });

                Console.Write("blog with id " + response.BlogId + "was deleted");
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

    }
}
