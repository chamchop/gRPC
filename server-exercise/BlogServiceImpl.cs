using Blog;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using static Blog.BlogService;

namespace server_exercise
{
    public class BlogServiceImpl : BlogServiceBase
    {
        private static MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        private static IMongoDatabase mongoDatabase = mongoClient.GetDatabase("local");
        private static IMongoCollection<BsonDocument> mongoCollection = mongoDatabase.GetCollection<BsonDocument>("blog");

        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
        {
            var blog = request.Blog;
            BsonDocument doc = new BsonDocument("author_id", blog.AuthorId)
                                                .Add("title", blog.Title)
                                                .Add("content", blog.Content);
            mongoCollection.InsertOne(doc);
            String id = doc.GetValue("_id").ToString();
            return Task.FromResult(new CreateBlogResponse()
            {
                Blog = blog
            });
        }

        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = mongoCollection.Find(filter).FirstOrDefault();
            if (result == null)
                throw new RpcException(new Status(StatusCode.NotFound, "the blog id " + blogId + "wasn't found"));

            Blog.Blog blog = new Blog.Blog()
            {
                AuthorId = result.GetValue("author_id").AsString,
                Title = result.GetValue("title").AsString,
                Content = result.GetValue("content").AsString,
            };

            return new ReadBlogResponse() { Blog = blog };
        }

        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context)
        {
            var blogId = request.Blog.Id;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = mongoCollection.Find(filter).FirstOrDefault();

            if (result == null)
                throw new RpcException(new Status(StatusCode.NotFound, "the blog is" + blogId + " wasn't found"));

            BsonDocument doc = new BsonDocument("author_id", request.Blog.AuthorId)
                                    .Add("title", request.Blog.Title)
                                    .Add("content", request.Blog.Content);

            mongoCollection.ReplaceOne(filter, doc);

            Blog.Blog blog = new Blog.Blog()
            {
                AuthorId = result.GetValue("author_id").AsString,
                Title = result.GetValue("title").AsString,
                Content = result.GetValue("content").AsString,
            };

            blog.Id = blogId;

            return new UpdateBlogResponse() { Blog = blog };
        }

        public override async Task<DeletedBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = mongoCollection.DeleteOne(filter);
            if (result.DeletedCount == 0)
                throw new RpcException(new Status(StatusCode.NotFound, "blog with id: " + blogId + " wasn't found"));

            return new DeletedBlogResponse() { BlogId = blogId };
        }
    }
}
