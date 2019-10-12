using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebApi.Models;

namespace WebApi.Services
{
    public class LectureService
    {
        private readonly IMongoCollection<Lecture> _lectures;

        public LectureService(ILectureDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _lectures = database.GetCollection<Lecture>(settings.LecturesCollectionName);
        }

        public List<Lecture> Get() =>
            _lectures.Find(lecture => true).ToList();

        public Lecture Get(string id) =>
            _lectures.Find<Lecture>(lecture => lecture.Id == id).FirstOrDefault();

        public Lecture Create(Lecture lect)
        {
            _lectures.InsertOne(lect);
            return lect;
        }

        public void Update(string id, Lecture lectIn) =>
            _lectures.ReplaceOne(lect => lect.Id == id, lectIn);

        public void Remove(Lecture lectIn) =>
            _lectures.DeleteOne(lect => lect.Id == lectIn.Id);

        public void Remove(string id) =>
            _lectures.DeleteOne(lect => lect.Id == id);
    }
}
