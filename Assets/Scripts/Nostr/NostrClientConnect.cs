using System;
using UnityEngine;
using Valve.Newtonsoft.Json;
using ElRaccoone.WebSockets;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

public class NostrClientConnect : MonoBehaviour
{
    public string publicKeyHex;
    public string privateKeyHex;
    public string Socket = "wss://gnost.faust.duckdns.org";
    public WSConnection wsConnection;
    public void Start()
    {
        GenerateKeys(out string privateKey, out string publicKey);
        publicKeyHex = publicKey.Replace("-", "").ToLower();
        privateKeyHex = privateKey.Replace("-", "").ToLower();

        Console.WriteLine($"Private Key: {privateKeyHex}");
        Console.WriteLine($"Public Key: {publicKeyHex}");

        // Connect to the relay
        WSConnection wsConnection = new WSConnection(Socket);
        wsConnection.OnConnected(() =>
        {
            Debug.Log("WS Connected!");
        });
        wsConnection.OnDisconnected(() =>
        {
            Debug.Log("WS Disconnected!");
        });

        wsConnection.OnError(error =>
        {
            Debug.Log("WS Error " + error);
        });

        wsConnection.OnMessage(message =>
        {
            Debug.Log("Received message " + message);
        });
        Debug.Log("Sent connection");
        wsConnection.Connect();

        Debug.Log("Sending payload");
        // Queue sending messages, these will always be send in this order.
        wsConnection.SendMessage(ConnectToRelaySendPayload(publicKeyHex));
        //     wsConnection.SendMessage("World!");
    }
    public void GenerateKeys(out string privateKey, out string publicKey)
    {
        // Generate a new private key
        var keyGen = new ECKeyPairGenerator();
        var secureRandom = new SecureRandom();
        var keyGenParam = new KeyGenerationParameters(secureRandom, 256);

        keyGen.Init(keyGenParam);
        AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();

        ECPrivateKeyParameters privateKeyParams = (ECPrivateKeyParameters)keyPair.Private;
        ECPublicKeyParameters publicKeyParams = (ECPublicKeyParameters)keyPair.Public;

        Org.BouncyCastle.Math.BigInteger privateKeyInt = privateKeyParams.D;
        Org.BouncyCastle.Math.EC.ECPoint q = publicKeyParams.Q;

        // Convert private key to hex string
        privateKey = privateKeyInt.ToString(16).PadLeft(64, '0');

        // Convert public key to hex string (compressed format)
        publicKey = BitConverter.ToString(q.GetEncoded(true)).ToLower();
    }
    private void OnDestroy()
    {
        // A provided editor script closes all connections automatically when you
        // exit play mode. Use this method to close the connection manually.
        wsConnection.Disconnect();
    }

    private string ConnectToRelaySendPayload(string PublicKey)
    {
        Debug.Log("Connected to the relay.");

        // Subscribe to messages
        var subscription = new
        {
            type = "REQ",
            id = "1",
            filters = new[]
            {
                new { pubkey = PublicKey }
            }
        };

        var json = JsonConvert.SerializeObject(subscription);
        return json;
    }
}