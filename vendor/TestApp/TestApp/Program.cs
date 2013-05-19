using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Calendar.v3;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Calendar.v3.Data;
using System.Data;
using System.Xml;


namespace TestApp
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                Service serv = new Service();

                SQLiteDatabase sqlite = new SQLiteDatabase();
                DataTable events;
                String query = @"select events.start, events.end, events.notes, user_event_associations.user_id, user_event_associations.state, users.email, users.name, users.surname from events, user_event_associations, users  where events.id = user_event_associations.event_id AND users.id = user_event_associations.user_id";
                events = sqlite.GetDataTable(query);
                foreach (DataRow dr in events.Rows)
                {

                    DateTime start = (DateTime) dr["start"];
                    DateTime end = (DateTime) dr["end"];
                    string notes = dr["notes"] as string;
                    string name = dr["name"] as string;
                    string email = dr["email"] as string;
                    string state = dr["state"] as string;
                    Console.WriteLine("start " + start);
                    Console.WriteLine("end " + end);

                    Event newEvent = new Event()
                    {
                        Summary = "Fundacja Dzieci Niczyje - dyżur",
                        Location = "ul. Katowicka 31 Warszawa",
                        Start = new EventDateTime()
                        {
                            DateTime = XmlConvert.ToString(start, XmlDateTimeSerializationMode.Local),
                        },
                        End = new EventDateTime()
                        {
                            DateTime = XmlConvert.ToString(end, XmlDateTimeSerializationMode.Local),
                        },
                        Description = (notes ?? string.Empty) + " Status: " + state,
                        Creator = new Event.CreatorData()
                        {
                            DisplayName = name,
                            Email = email
                        },
                        Reminders = new Event.RemindersData()
                        {
                            UseDefault = false,
                            Overrides = new List<EventReminder>()
                        }
                    };
                    newEvent.Reminders.Overrides.Add(new EventReminder()
                    {
                        Method = "email",
                        Minutes = 100
                    });
                    
                    Event created = serv.AddEvent(newEvent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
