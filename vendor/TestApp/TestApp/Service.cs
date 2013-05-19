using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Calendar.v3;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Services;
using Google.Apis.Calendar.v3.Data;

namespace TestApp
{
    public class Service
    {
        private const string SERVICE_ACCOUNT_EMAIL = "779125896901@developer.gserviceaccount.com";
        private const string SERVICE_ACCOUNT_PKCS12_FILE_PATH = @"C:\Users\lrymko\Downloads\client.p12";
        private const string CALENDAR_ID = @"idj1e369spvfbe4ae91l8r3b5k";
        private const string API_KEY = @"idj1e369spvfbe4ae91l8r3b5k";

        private CalendarService calendarService;
        private string calendarID;
        private Events existingEvents;

        public Service()
        {
            calendarService = Authorize();
            calendarID = GetCalendarId();

            existingEvents = GetEvents();
        }

        public static CalendarService Authorize()
        {
            X509Certificate2 certificate = new X509Certificate2(SERVICE_ACCOUNT_PKCS12_FILE_PATH, "notasecret",
                    X509KeyStorageFlags.Exportable);


            AssertionFlowClient provider = new AssertionFlowClient(GoogleAuthenticationServer.Description, certificate)
            {
                ServiceAccountId = SERVICE_ACCOUNT_EMAIL,
                Scope = "https://www.googleapis.com/auth/calendar", //CalendarService.Scopes.Calendar.ToString()
            };

            var auth = new OAuth2Authenticator<AssertionFlowClient>(provider, AssertionFlowClient.GetState);

            BaseClientService.Initializer init = new BaseClientService.Initializer();
            init.ApiKey = API_KEY;
            init.Authenticator = auth;
            CalendarService service = new CalendarService(init);
            return service;
        }

        private string GetCalendarId()
        {
            Google.Apis.Calendar.v3.CalendarListResource.ListRequest clrq = calendarService.CalendarList.List();
            var result = clrq.Fetch();
            return result.Items.First(x => x.Id.Contains(CALENDAR_ID)).Id;
        }

        public Event AddEvent(Event ev)
        {
            return AddOnlyIfExists(ev);
        }

        private Event AddOnlyIfExists(Event ev)
        {
            Event existing = FindExisting(ev);
            if (existing == null)
            {
                return calendarService.Events.Insert(ev, calendarID).Fetch();
            }
            return existing;
        }

        private Event FindExisting(Event ev)
        {
            try
            {
                return existingEvents.Items.First(x => x.Start.DateTime.Equals(ev.Start.DateTime));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Events GetEvents()
        {
            return calendarService.Events.List(calendarID).Fetch();
        }
    }
}
