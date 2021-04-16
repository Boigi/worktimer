using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace work_test_console
{
    class Program
    {
        public class Event
        {
            public string Eventname;
            public DateTime Time;
            public Event(string eventname)
            {
                Eventname = eventname;
                Time = DateTime.Now;
            }
        }

        public class Workday
        {
            public DateTime Day;

            public DateTime Daystart;

            public DateTime Dayend;

            public List<DateTime> Pauses = new List<DateTime>();
        }

        class Data
        {
            public List<Event> Daily = new List<Event>();
            public List<Workday> Days = new List<Workday>();

            
        }

        static Data data = new Data();

        const string FILENAME = "timetracking.json";



        static void Main(string[] args)
        {
            Load();
            var running = true;
            //events.Clear();

            if (data.Daily.Any(e=> e.Time.Date != DateTime.Now.Date))
            {
                var cov = Convert(ref data.Daily);
                if ( data.Days.Count==0 || data.Days.Last().Day.Date != cov.Day.Date)
                {
                    data.Days.Add(cov);
                }
                
            }
            Console.WriteLine("commands \n start \n stop \n pause \n unpause \n checkt \n checkd \n convert \n calculate \n exit");

            while (running)
            {
                var line = Console.ReadLine().Trim();

                switch (line)
                {
                    case "start":
                        {
                            if (!data.Daily.Any(e => e.Eventname == "start"))
                            {
                                data.Daily.Add(new Event("start"));
                            }

                            var lastevent = data.Daily.Last();

                            if (lastevent.Eventname == "stop")
                            {
                                lastevent.Eventname = "pause";
                                data.Daily.Add(new Event("unpause"));

                            }
                            Console.WriteLine("Worktime has started.");
                            break;
                        }

                    case "stop":
                        {
                            var lastevent = data.Daily.Last();
                            if (lastevent.Eventname == "pause")
                            {
                                lastevent.Eventname = "stop";
                                break;
                            }
                                
                            if (lastevent.Eventname == "stop")
                            {
                                lastevent.Time = DateTime.Now;
                                break;
                            }

                            data.Daily.Add(new Event("stop"));
                            Console.WriteLine("Worktime has stopped.");
                            break;
                        }

                    case "pause":
                        if (data.Daily.Any(e => e.Eventname == "start") && !data.Daily.Any(e => e.Eventname == "stop") && data.Daily[data.Daily.Count -1].Eventname != "pause")
                        {
                            data.Daily.Add(new Event("pause"));
                        }
                        Console.WriteLine("Pause has started.");
                        break;

                    case "unpause":
                        if (data.Daily.Last().Eventname == "pause")
                        {
                            data.Daily.Add(new Event("unpause"));
                        }
                        Console.WriteLine("Pause has stopped.");
                        break;

                    case "exit":
                        running = false;
                        break;

                    case "checkt":
                        foreach (var element in data.Daily)
                        {
                            Console.WriteLine($"Eventname: {element.Eventname}, Eventtime: {element.Time}");
                        }
                        break;

                    case "checkd":
                        Console.WriteLine(data.Days.Count);
                        foreach (var element in data.Days)
                        {
                            Console.WriteLine($"Day: {element.Day.ToString("dd.MM.yyyy")}, Daystart: {element.Daystart.ToString("HH:mm")}, Dayend: {element.Dayend.ToString("HH:mm")} Pauses: {element.Pauses.Count()/2}");
                        }
                        break;

                    case "convert":
                        data.Days.Add(Convert(ref data.Daily));
                        break;

                    case "calculate":
                        Console.WriteLine("Please enter Date: DD.MM.YYYY");
                        var date = Console.ReadLine().Trim();
                        
                        var selectedDay = FindDay(data.Days, date);
                        var time = Worktime(selectedDay);
                        Console.WriteLine(time);
                        break;
                }
                Save();
            }

            static Workday FindDay (List<Workday> list, String date)
            {
                foreach (var e in list)
                {
                    var checkdate = e.Day.ToString("dd.MM.yyyy");

                    if (checkdate == date)
                    {
                        return e;
                    }
                }
                Console.WriteLine("Date not found pls try again.");
                return data.Days[0];
            }


            static void Save()
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(FILENAME, json);
            }

            static void Load()
            {
                if (File.Exists(FILENAME))
                {
                    var text = File.ReadAllText(FILENAME);
                    data = JsonConvert.DeserializeObject<Data>(text);
                }

            }

            static Workday Convert (ref List<Event> dailyevents)
            {
                var work = new Workday();
                work.Day = dailyevents.First().Time.Date;
                work.Daystart = dailyevents.First().Time;
                work.Dayend = dailyevents.Last().Time;
                dailyevents.RemoveAt(0);
                dailyevents.RemoveAt(dailyevents.Count - 1);

                foreach (var e in dailyevents)
                {
                    work.Pauses.Add(e.Time);
                }
                dailyevents.Clear();
                
                return work;
            }

            static TimeSpan Worktime(Workday wd)
            {
                var worktime = wd.Dayend - wd.Daystart;
                TimeSpan pausetime = new TimeSpan();
                while (wd.Pauses.Count > 1)
                {
                    pausetime += wd.Pauses[1] - wd.Pauses[0];

                    wd.Pauses.RemoveRange(0, 1);
                }
                worktime = worktime - pausetime;
                return worktime;
            }
        }
    }
}