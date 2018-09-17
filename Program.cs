using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Trace {

    class Program {
        static Tuple<string, string, string> GetInfo(string ip) {
            using (var client = new HttpClient()) {
                var response = client.GetAsync($"https://api.iptoasn.com/v1/as/ip/{ip}").Result;        
                var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if ((string)json["announced"] == "True")
                   return Tuple.Create((string)json["as_number"], (string)json["as_country_code"], (string)json["as_description"]);
                else
                   return Tuple.Create("None", "None", "None");                                              
            }       
        }

        static void Main(string[] args) {

            var even = new System.Threading.AutoResetEvent(false);
            var process = new Process();
            process.StartInfo.FileName = "tracert.exe";
            process.StartInfo.Arguments = args[0];
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (sender, e) => {
                if (!String.IsNullOrEmpty(e.Data)){               
                    if (String.IsNullOrEmpty(Regex.Match(e.Data, "\\*").Value)) {
                        var id = Regex.Match(e.Data, @"\b(\d+\s+)\b").Value;
                        var ip = Regex.Match(e.Data, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b").Value;
                        if (!String.IsNullOrEmpty(ip) && !String.IsNullOrEmpty(id)) {
                            var info = GetInfo(ip);                         
                            Console.WriteLine($"ID: {int.Parse(id)} \tIP: {ip} \tAS: {info.Item1} \tCOUNTRY {info.Item2} \tDESCRIPTION: {info.Item3};");
                        }
                    } else {                    
                        even.Set();
                    }
                }            
            };
            process.Start();
            process.BeginOutputReadLine();
            even.WaitOne();

        }
    }
}
