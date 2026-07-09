using System.Text.Json;
using TestOps.API.Models;

namespace TestOps.API.Services
{
    public class ResultService
    {
        // private readonly string _file =
        //     Path.Combine(AppContext.BaseDirectory, "results.json");

        private readonly List<TestRunResult> _results = new();

        private readonly string _file =
            Path.Combine(Directory.GetCurrentDirectory(), "results.json");

        public List<TestRunResult> GetAll()
        {
            if (!File.Exists(_file))
                return new List<TestRunResult>();

            var json = File.ReadAllText(_file);
            return JsonSerializer.Deserialize<List<TestRunResult>>(json)
                   ?? new List<TestRunResult>();
        }

        public void Add(TestRunResult result)
        {
            var list = GetAll();
            list.Add(result);

            File.WriteAllText(_file,
                JsonSerializer.Serialize(list, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        }

        public List<TestRunResult> GetByRun(Guid runId)
        {
            return GetAll()
                .Where(x => x.RunId == runId)
                .OrderBy(x => x.ExecutedAt)
                .ToList();
        }

        public TestRunResult? Get(Guid id)
        {
            return GetAll()
                .FirstOrDefault(x => x.Id == id);
        }
    }
}