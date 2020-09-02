using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BexioDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("########################################################################");
            Console.WriteLine("########################################################################");
            Console.WriteLine("######################## BEXIO DOWNLOADER ##############################");
            Console.WriteLine("########################   BY WEPFI GMBH  ##############################");
            Console.WriteLine("########################     WEPFI.CH     ##############################");
            Console.WriteLine("########################################################################");
            Console.WriteLine("########################################################################");
            Console.WriteLine("");
            Console.WriteLine("");

            if (!File.Exists("token.txt"))
            { 
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No token found!");
                Console.ResetColor();
                Console.WriteLine("Create a file named 'token.txt' in the same directory where you run this script and copy your access token in it."); 
                Console.WriteLine("You can create an access token in your bexio settings: https://office.bexio.com/index.php/admin/apiTokens#/");
                Console.ReadLine();
                Environment.Exit(0);
            }

            string token = File.ReadAllText("token.txt");

            Console.WriteLine("What do you want to download? Type:");
            Console.WriteLine("1 - All (including archive)");
            Console.WriteLine("2 - Inbox");
            short option;
            string input = Console.ReadLine();
            if (!Int16.TryParse(input, out option) || option > 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid option");
                Console.ReadLine();
                Environment.Exit(0);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.bexio.com/3.0/files" + (option == 1 ? "?archived_state=all" : ""));
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Headers.Add("Accept", "application/json");

            string result;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }


            List<Document> docs = JsonConvert.DeserializeObject<List<Document>>(result);

            Console.WriteLine("Found " + docs.Count + " documents, starting download..");

            if (!Directory.Exists("\\docs"))
                Directory.CreateDirectory("docs");

            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", "Bearer " + token);

                foreach (Document doc in docs)
                {
                    string filename = MakeValidFileName(doc.name + '.' + doc.extension);
                    Console.WriteLine("Downloading " + filename);
                    client.DownloadFile("https://api.bexio.com/3.0/files/" + doc.id + "/download", "docs\\" + filename);
                }
            }

            Console.WriteLine("All downloads finished");
            Console.ReadLine();
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }

    class Document
    {
        public int id { get; set; }
        public string name { get; set; }
        public string extension { get; set; }
    }
}
