using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Benchmark_Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MemoryBenchmarker>();

            //   BenchmarkRunner.Run<MemoryBenchmarker>(
            //ManualConfig
            //.Create(DefaultConfig.Instance)
            //.WithOptions(ConfigOptions.DisableOptimizationsValidator));

            MemoryBenchmarker bm = new MemoryBenchmarker();
            bm.Redis();
            bm.Elastic();
            bm.CouchDB();
        }
  
    }

    [MemoryDiagnoser]
    public class MemoryBenchmarker
    {
        List<User> taxPayerList1 = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(@"C:\Users\P1716\source\repos\Benchmark_Test\Benchmark_Test\jsonData.json")).Take(1).ToList();

        [Benchmark]
        public void Redis()
        {
            var rdsm = new RedisManager();

            //Listeyi döngü sayısı kadar tekrar göndermek için oluşturulmuştur.
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in taxPayerList1)
                {
                    string key = $"{typeof(User).Name}_{item.Identifier}";

                    rdsm.SetSerializeBytes(key, item, new TimeSpan(0, 0, 600));
                }
            }
        }

        [Benchmark]
        public void Elastic()
        {
            string _baseUri = "http://localhost:13032";
            IRestClient restClient = new RestClient();
            IRestRequest restRequest = new RestRequest(Method.POST);
            restRequest.AddHeader("Content-Type", "application/json");
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in taxPayerList1)
                {
                    string uri = $"{_baseUri}/{item.Identifier}/_doc";
                    restClient.BaseUrl = new Uri(uri);
                    restRequest.AddParameter(string.Empty, JsonConvert.SerializeObject(item), ParameterType.RequestBody);
                    var response = restClient.Execute(restRequest);
                }
            }
        }

        [Benchmark]
        public void CouchDB()
        {
            var client = new RestClient("http://localhost:5984/dbname/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic cm9vdDpyb290");
            request.AddHeader("Content-Type", "application/json");

            for (int i = 0; i < 100; i++)
            {
                foreach (var item in taxPayerList1)
                {
                    request.AddParameter("application/json", JsonConvert.SerializeObject(item), ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                }
            }
        }
    }

    public class User
    {
        public long Identifier { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }
        public ICollection<UserModuleType> ModuleTypeList { get; set; }
        public ICollection<UserAddress> AliasList { get; set; }
    }

    public class UserAddress
    {
        public long Id { get; set; }
        public long Identifier { get; set; }
        public ModuleType ModuleType { get; set; }

        public string Alias { get; set; }
        public bool IsDeleted { get; set; }

        public string UserRoleCode { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? DeletionTime { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public DateTime CreatedDate { get; set; }

        #region Relations

        public User User { get; set; }

        #endregion
    }

    public enum ModuleType : byte
    {
        [Description("Default")]
        DefaultUser = 0,
        [Description("Default1")]
        PortalUser = 1,
        [Description("Default")]
        SuperUser = 2,
    }

    public class UserModuleType
    {
        public long Id { get; set; }

        public long Identifier { get; set; }
        public ModuleType ModuleType { get; set; }

        public User User { get; set; }
    }

}


