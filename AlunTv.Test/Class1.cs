using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using AlunTv.Test.Users.Updater;
using Microsoft.FSharp.Collections;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace AlunTv.Test
{

    public class Tests
    {

        public void TestFullRun()
        {
            using (var store = new DocumentStore { Url = "http://localhost:8080" }.Initialize())
            {
                IndexCreation.CreateIndexes(typeof(SourceShowInfoCaches_ByName).Assembly, store);

                using (var session = store.OpenSession())
                {
                    var updater = new ShowUpdater(session, x => Console.WriteLine("Updated: " + x));
                    updater.UpdateShowNames();
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new ShowUpdater(session, x => Console.WriteLine("Updated: " + x));
                    updater.UpdateShowNames();
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new ShowUpdater(session, x => Console.WriteLine("Updated: " + x));
                    updater.SeedShow("24496");
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new ShowUpdater(session, x => Console.WriteLine("Updated: " + x));
                    updater.UpdateShows();
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new UserUpdater(session, x => Console.WriteLine("CreateUser: " + x));
                    var user = updater.CreateUser("testve", "2342342434234", "2353465346546");
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new UserUpdater(session, x => Console.WriteLine("AddShow: " + x));
                    updater.AddShow("testve", "24496");
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new UserUpdater(session, x => Console.WriteLine("AddShow: " + x));
                    updater.SetEpisodeWatched("testve", "24496");
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var updater = new UserUpdater(session, x => Console.WriteLine("AddShow: " + x));
                    updater.SetSeasonWatched("testve", "24496");
                    session.SaveChanges();
                }
            }
        }
    }
}
