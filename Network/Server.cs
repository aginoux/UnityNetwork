﻿using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;

public class Server : MonoBehaviour
{

    public GameObject listContainer;
    public GameObject messagePrefab;
    private GameObject[] prefabTab;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    public int port = 6321;
    private TcpListener server;
    private bool serverStarted;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        prefabTab = new GameObject[16];
        
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Server has been started on port " + port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e);
        }
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (ServerClient c in clients)
        {
            //Is the client still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            //Check for message from the client
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }

        for(int i = 0; i < disconnectList.Count - 1; i++)
        {
            //Broadcast(disconnectList[i].clientName + " has disconnected ", clients);
            RemoveDeviceToPanelList(disconnectList[i].clientName);
            clients.Remove(disconnectList[1]);
            disconnectList.RemoveAt(i);
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar), (clients.Count).ToString()));
        StartListening();

        //Send a message to everyone, say someone has connected
        //Broadcast(clients[clients.Count - 1].clientName + " has connected", clients);

        //Ask for name
        Broadcast("%NAME", new List<ServerClient>() {clients[clients.Count - 1]});
    }
    
    private void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            //Broadcast(c.clientName + " has connected", clients);
            AddDeviceToPanelList(c.clientName);
            return;
        }

        Broadcast(c.clientName + " : " + data,clients);
    }

    private void AddDeviceToPanelList(string name)
    {
        GameObject go = Instantiate(messagePrefab, listContainer.transform) as GameObject;
        go.GetComponentInChildren<Text>().text = name;

        for(int i = 0; i < prefabTab.Length; i++)
        {
            if(prefabTab[i] == null)
            {
                prefabTab[i] = go;
            }
        }

    }

    private void RemoveDeviceToPanelList(string name)
    {
        for (int i = 0; i < prefabTab.Length; i++)
        {
            if(prefabTab[i].GetComponentInChildren<Text>().text == name)
            {
                Destroy(prefabTab[i]);
            }
        }
    }

    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message + " to client : " + c.clientName);
            }
        }
    }

    private void Send(string data)
    {
        Broadcast(" server has sent " + data, clients);        
    }

    public void OnSendButton()
    {
        //string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        string message = "heart.png|image";
        Send(message);
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket, string numero)
    {
        clientName = "Guest" + numero;
        tcp = clientSocket;
    }
}
