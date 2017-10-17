using System;
using System.Collections.Generic;
using System.IO;
using Contracts.Models;
using Contracts.Repository;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Infra.Data.Repository
{
    public class FileRepository : IFilesRepository
    {
        private readonly IMongoDatabase _database;

        public FileRepository(IConfigurationRoot configurationRoot)
        {
            var con = configurationRoot.GetConnectionString("MongoDBFileServer");
            var client = new MongoClient(con);
            
             _database = client.GetDatabase("btfileattached");
        }

        public string AddFile(Stream fileStream, string fileName, string contentType, string userId)
        {

            var bucket = new GridFSBucket(_database, new GridFSBucketOptions() { BucketName = userId});
            var metadata = new BsonDocument
            {
                {new BsonElement("ContentType", contentType) },
                {new BsonElement("UserId", userId) }
            };

            return bucket.UploadFromStream(fileName, fileStream, new GridFSUploadOptions()
            {
                Metadata = metadata
            }).ToString();
        }

        public FileModel GetFile(string objectId, string userId)
        {
            var bucket = new GridFSBucket(_database, new GridFSBucketOptions() { BucketName = userId });
            var file = bucket.OpenDownloadStream(ObjectId.Parse(objectId));
            
            var model = new FileModel()
            {
                FileStream = file,
                ContentType = file.FileInfo.Metadata["ContentType"].AsString,
                FileName = file.FileInfo.Filename,
                Id = objectId
              };
            return model;
        }
    }
}