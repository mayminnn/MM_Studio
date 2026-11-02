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
            var suites = JsonSerializer.Deserialize<List<TestSuite>>(json)
                         ?? new List<TestSuite>();

            // Suites saved before orders were assigned have every test at 0
            suites.ForEach(Renumber);

            return suites;
        }

        // Numbers a suite's tests 1..n. Tests keep their saved order; any without one (0) go
        // after them in list order. The suite page and runs both rely on order == position
        public static void Renumber(TestSuite suite)
        {
            suite.Tests = suite.Tests
                .Select((test, index) => (test, index))
                .OrderBy(x => x.test.Order > 0 ? x.test.Order : int.MaxValue)
                .ThenBy(x => x.index)
                .Select(x => x.test)
                .ToList();

            for (int i = 0; i < suite.Tests.Count; i++)
                suite.Tests[i].Order = i + 1;
        }

        public TestSuite Create(TestSuite suite)
        {
            var suites = GetAll();

            Renumber(suite);
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

            Renumber(updated);
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