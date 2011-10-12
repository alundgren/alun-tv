using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using AlunTv.Test;
using NLog;
using Raven.Client;
using Raven.Client.Document;

namespace AlunTv.Worker
{
    class Program
    {
        public class Worker : IDisposable
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private readonly int _tickIntervalMilliseconds;
            private readonly Action _periodicWork;
            private readonly System.Threading.Timer _timer;

            public Worker(int tickIntervalMilliseconds, Action periodicWork)
            {
                _tickIntervalMilliseconds = tickIntervalMilliseconds;
                _periodicWork = periodicWork;
                _timer = new System.Threading.Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
            }

            public void Start()
            {
                Logger.Info("Starting worker: {0}ms interval", _tickIntervalMilliseconds);
                _timer.Change(0, _tickIntervalMilliseconds);
            }

            public void Stop()
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            private void OnTick()
            {
                Logger.Info("Worker ticking");
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                try
                {
                    _periodicWork.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error in worker tick", ex);
                }
                finally
                {
                    _timer.Change(_tickIntervalMilliseconds, _tickIntervalMilliseconds);
                }
            }

            public void Dispose()
            {
                _timer.Dispose();
            }
        } 
        static void Main(string[] args)
        {
            const int tenMinutes = 1000*60*10;
            using (var store = new DocumentStore { Url = ConfigurationManager.AppSettings["RavenUrl"] })
            using (var worker = new Worker(tenMinutes, () => Update(store)))
            {
                store.Initialize();
                worker.Start();
                Console.WriteLine("Press any key to terminate...");
                Console.ReadKey();
                worker.Stop();                                      
            }
        }

        public static void Update(IDocumentStore documentStore)
        {
            using(var session = documentStore.OpenSession())
            {
                var updater = new ShowUpdater(session, Console.WriteLine); //TODO: Send to signalr
                updater.UpdateShows();
                session.SaveChanges();
            }
            using (var session = documentStore.OpenSession())
            {
                var updater = new ShowUpdater(session, Console.WriteLine); //TODO: Send to signalr
                updater.UpdateShowNames();
                session.SaveChanges();
            }
        }
    }
}
