using UnityEngine;
using System.Threading;
using DarkRift.Server;
namespace Basis.Scripts.Networking
{
    [AddComponentMenu("DarkRift/Server")]
    public class BasisNetworkServer : MonoBehaviour
    {
        /// <summary>
        ///     The actual server.
        /// </summary>
        public DarkRiftServer Server = null;
        [SerializeField]
        [Tooltip("Indicates whether the server events will be routed through the dispatcher or just invoked.")]
        private bool eventsFromDispatcher = true;
        private void Update()
        {
            //Execute all queued dispatcher tasks
            if (Server != null)
            {
                Server.ExecuteDispatcherTasks();
            }
        }

        /// <summary>
        ///     Creates the server.
        /// </summary>
        public void Create()
        {
            if(Server != null)
            {
                Server.Dispose();
            }
            Server = null;
            // Create spawn data from config
            ServerSpawnData spawnData = new ServerSpawnData
            {
                // Allow only this thread to execute dispatcher tasks to enable deadlock protection
                DispatcherExecutorThreadID = Thread.CurrentThread.ManagedThreadId,

                // Inaccessible from XML, set from inspector
                EventsFromDispatcher = eventsFromDispatcher,

                Listeners = new ServerSpawnData.ListenersSettings()

            };

            // Add types
             spawnData.PluginSearch.PluginTypes.AddRange(UnityServerHelper.SearchForPlugins());
            // Create server
            Server = new DarkRiftServer(spawnData);
            Server.StartServer();
        }

        private void OnDisable()
        {
            Close();
        }

        private void OnApplicationQuit()
        {
            Close();
        }

        /// <summary>
        ///     Closes the server.
        /// </summary>
        public void Close()
        {
            if (Server != null)
            {
                Server.Dispose();
            }
        }
    }
}