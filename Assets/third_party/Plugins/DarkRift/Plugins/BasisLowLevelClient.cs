using DarkRift.Dispatching;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace DarkRift.Client.Unity
{
    public class BasisLowLevelClient : MonoBehaviour
    {

        #region Cache settings
        [SerializeField]
        [Tooltip("The maximum number of DarkRiftWriter instances stored per thread.")]
        public int maxCachedWriters = 2;

        [SerializeField]
        [Tooltip("The maximum number of DarkRiftReader instances stored per thread.")]
        public int MaxCachedReaders = 2;

        [SerializeField]
        [Tooltip("The maximum number of Message instances stored per thread.")]
        public int maxCachedMessages = 8;
        [SerializeField]
        [Tooltip("The maximum number of SocketAsyncEventArgs instances stored per thread.")]
        public int MaxCachedSocketAsyncEventArgs = 32;
        [SerializeField]
        [Tooltip("The maximum number of ActionDispatcherTask instances stored per thread.")]
        public int maxCachedActionDispatcherTasks = 16;
        public ClientObjectCacheSettings ObjectCacheSettings { get; set; }
        /// <summary>
        ///     Serialisable version of the object cache settings for Unity.
        /// </summary>
        [SerializeField]
        [Tooltip("The maximum number of ActionDispatcherTask instances stored per thread.")]
#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        private SerializableObjectCacheSettings objectCacheSettings = new SerializableObjectCacheSettings();
#pragma warning restore IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        #endregion
        /// <summary>
        ///     Event fired when we disconnect form the server.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     The ID the client has been assigned.
        /// </summary>
        public ushort ID
        {
            get
            {
                return Client.ID;
            }
        }
        /// <summary>
        ///     Returns the state of the connection with the server.
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                return Client.ConnectionState;
            }
        }
        public DarkRiftClient Client;

        public LiteNetLibClientConnection LiteNetLibConnnection;
        public void Initialize()
        {
            ObjectCacheSettings = objectCacheSettings.ToClientObjectCacheSettings();
            Client = new DarkRiftClient(ObjectCacheSettings);
            Client.Disconnected += Client_Disconnected;
        }
        private const float interval = 0.0333333333333333f;
        private float timer = 0.0f;
        void Update()
        {
            // Update the timer with the time elapsed since the last frame
            timer += Time.deltaTime;

            // Check if the timer has reached the interval
            if (timer >= interval)
            {
                // Execute your code here
                ReceiveMessages();

                // Reset the timer, keeping any leftover time to maintain precision
                timer -= interval;
            }
        }
        /// <summary>
        /// Call this to manually receive messages
        /// </summary>
        public void ReceiveMessages()
        {
            if (LiteNetLibConnnection == null)
            {
                return;
            }
            LiteNetLibConnnection.PerformUpdate();
        }
        public void OnDestroy()
        {
            //Remove resources
            Close();
            Debug.Log("Destroy");
        }

        public void OnApplicationQuit()
        {
            //Remove resources
            Close();
            Debug.Log("Quit");
        }

        /// <summary>
        ///     Connects to a remote asynchronously.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="callback">The callback to make when the connection attempt completes.</param>
        public void ConnectInBackground(IPAddress ip, int port, DarkRiftClient.ConnectCompleteHandler callback = null)
        {
            LiteNetLibConnnection = new LiteNetLibClientConnection(ip.ToString(), port);
            Client.ConnectInBackground(LiteNetLibConnnection,
                delegate (Exception e)
                {
                    if (callback != null)
                    {
                        callback.Invoke(e);
                    }

                    if (ConnectionState == ConnectionState.Connected)
                    {
                        Debug.Log("Connected to " + ip + " on port " + port);
                    }
                    else
                    {
                        Debug.LogError("Connection failed to " + ip + " on port " + port);
                        Close();
                    }
                }
            );
        }
        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message template to send.</param>
        /// <returns>Whether the send was successful.</returns>
        public bool SendMessage(Message message, byte channel, DeliveryMethod sendMode)
        {
            return Client.SendMessage(message, channel, sendMode);
        }
        /// <summary>
        /// Invoked when the client is disconnected from the server.
        /// </summary>
        /// <param name="sender">The client that got disconnected.</param>
        /// <param name="e">The arguments for the event.</param>
        public void Client_Disconnected(object sender, DisconnectedEventArgs e)
        {
            // If we're handling multithreading then pass the event to the dispatcher
            if (!e.LocalDisconnect)
                Debug.Log("Disconnected from server, error: " + e.Error);

            EventHandler<DisconnectedEventArgs> handler = Disconnected;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        /// <summary>
        ///     Disconnects this client from the server.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public bool Disconnect()
        {
            if (Client != null)
            {
                return Client.Disconnect();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Closes this client.
        /// </summary>
        public void Close()
        {
            Debug.Log("Closing Connection");
            if (Client != null)
            {
                Client.Disconnected -= Client_Disconnected;

                Client.Dispose();
                Client = null;
            }
            if (LiteNetLibConnnection != null)
            {
                LiteNetLibConnnection.Dispose();
                LiteNetLibConnnection = null;
            }
        }
    }
}