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
            Console.WriteLine("########################   Ficht Hämmerli  #############################");
            Console.WriteLine("########################     WEPFI.CH     ##############################");
            Console.WriteLine("####################### Kontakt: ficht@wepfi.ch ########################");
            Console.WriteLine("########################################################################");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Bitte gib den Token ein:");
            string token = Console.ReadLine();

            if (token.Length != 1)
            {
                Console.WriteLine("Der Token sieht nicht korrekt aus, trotzdem weiterfahren? (y/n)");
                string tokenConfirm = Console.ReadKey().ToString();
                if (tokenConfirm != "y")
                {
                    Environment.Exit(0);
                }
            }

            while (true)
            {
                Console.WriteLine("Bitte geb den Pfad ein, wo die Dateien gespeichert werden sollen:");
                string path = Console.ReadLine();
                // check if directory exists
                if (Directory.Exists(path))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Der Pfad existiert nicht, soll er erstellt werden? (y/n)");
                    string pathConfirm = Console.ReadKey().ToString();
                    if (pathConfirm == "y")
                    {
                        Directory.CreateDirectory(path);
                        break;
                    }
                }
            }

            Console.WriteLine("Was möchtest du Downloaden? Wähle:");
            Console.WriteLine("1 - Alles (inklusive Archiv)");
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

            Console.WriteLine(docs.Count + " Dokumente gefunden, starte download..");

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

            Console.WriteLine("Alle downloads fertig, drücke eine Taste zum beenden..");
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
