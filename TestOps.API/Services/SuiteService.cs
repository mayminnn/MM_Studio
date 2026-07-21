using System.Text.Json;
using TestOps.API.Models;

namespace TestOps.API.Services
{
    public class SuiteService
    {
        private readonly string _filePath =
            Path.Combine("Data", "suites.json");

        public List<TestSuite> GetAll()
        {
            if (!File.Exists(_filePath))
                return new List<TestSuite>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<TestSuite>>(json)
                   ?? new List<TestSuite>();
        }

        public TestSuite Create(TestSuite suite)
        {
            var suites = GetAll();

            suites.Add(suite);

            File.WriteAllText(
                _filePath,
                JsonSerializer.Serialize(suites, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );

            return suite;
        }

        public TestSuite? GetById(string id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public void Delete(string id)
        {
            var suites = GetAll();

            var updated = suites.Where(x => x.Id != id).ToList();

            File.WriteAllText(
                _filePath,
                JsonSerializer.Serialize(updated, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }

        public void Update(string id, TestSuite updated)
        {
            var suites = GetAll();

            // var index = suites.FindIndex(x => x.Id == updated.Id);
            var index = suites.FindIndex(x => x.Id == id);

            if (index == -1)
                return;

            suites[index] = updated;

            File.WriteAllText(
                _filePath,
                JsonSerializer.Serialize(suites, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }
    }
}