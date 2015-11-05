﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace civstats
{
    class Program
    {
        static string id;
        static string key;
#if DEBUG
        const string SITE_URL = "http://httpbin.org/post";
        const int API_VERSION = -1;
#else
        const string SITE_URL = "http://civstats-byvkf.rhcloud.com/";
        const int API_VERSION = 1; // the version of the API the app is compatible with
#endif

        static void Main(string[] args)
        {
            Console.Title = "CivStats";
            /*if (!IsUpToDate())
            {
                Console.Write("This app needs to be updated. Please update the app.");
                Console.ReadKey();
                return;
            }*/
            
            CheckSettings();

            id = Properties.Settings.Default.id;
            key = Properties.Settings.Default.key;

            IStatsTracker[] trackers = {
                new DemographicsTracker(),
                new PoliciesTracker(),
                new ReligionTracker()
            };

            foreach (IStatsTracker tracker in trackers)
            {
                tracker.Changed += StatsTrackerHandler;
            }

            Console.WriteLine("Reporting civ stats. Please exit after you've finished playing.");
            Console.ReadKey();
        }
        
        static void StatsTrackerHandler(object source, StatsTrackerEventArgs e)
        {
#if DEBUG
            Uri uploadUri = new Uri(SITE_URL);
#else
            Uri uploadUri = new Uri(SITE_URL + "players/" + id + "/update");
#endif
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", "Token " + key);
            client.Headers.Add("Content-Type", "application/json");
            var response = client.UploadString(uploadUri, e.Update.ToJson());
            Console.WriteLine(response);
        }

        static void CheckSettings()
        {
            if (Properties.Settings.Default.id == "" || Properties.Settings.Default.key == "")
                PromptSettings();
            else
            {
                Console.Write("Using existing settings, press a key to enter new settings");
                DateTime start = DateTime.Now;
                while ((DateTime.Now - start).Seconds < 5 && !Console.KeyAvailable)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);
                }
                Console.WriteLine();

                if (Console.KeyAvailable)
                {
                    Console.ReadKey(); // eat up the entered key
                    Console.Clear();
                    PromptSettings();
                }
            }
        }

        static void PromptSettings()
        {
            Console.Write("Enter your id: ");
            Properties.Settings.Default.id = Console.ReadLine();
            Console.Write("Enter your private key: ");
            Properties.Settings.Default.key = Console.ReadLine();
            Properties.Settings.Default.Save();
        }

        static bool IsUpToDate()
        {
            Uri apiUri = new Uri(SITE_URL + "api/version");
            WebClient client = new WebClient();
            var response = client.DownloadString(apiUri);
            Console.WriteLine(response);
            int siteApiVersion = 0;
            int.TryParse(response, out siteApiVersion);

            if (siteApiVersion > API_VERSION)
                return false;

            return true;
        }
    }
}
