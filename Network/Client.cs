using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;


public class Client : MonoBehaviour {

    public GameObject listContainer;
    public GameObject messagePrefab;

    private string clientName = "Guest";

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public void ConnectToServer()
    {
        //If already Connected, ignore this function
        if (socketReady)
        {
            return;
        }

        //Default host / port values
        string host = "127.0.0.1";
        int port = 6321;

        string h;
        int p;
        string n;

        h = GameObject.Find("HostInput").GetComponent<InputField>().text;

        if(h != "")
        {
            host = h;
        }
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if(p != 0)
        {
            port = p;
        }

        n = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (n != "")
        {
            clientName = n;
        }

        //create the socket
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true; 
        }
        catch(Exception e)
        {
            Debug.Log("socket error : " + e.Message);
        }
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if(data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    private void OnIncomingData(string data)
    {
        if(data == "%NAME")
        {
            Send("&NAME|" + clientName);
            return;
        }

        //Debug.Log("Server : " + data);
        GameObject go =  Instantiate(messagePrefab, listContainer.transform) as GameObject;
        go.GetComponentInChildren<Text>().text = data;
    }

    private void Send(string data)
    {
        if (!socketReady)
        {
            return;
        }

        writer.WriteLine(data);
        writer.Flush();
    }
    /*
    public void OnSendButton()
    {
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }
*/
    private void CloseSocket()
    {
        if (!socketReady)
        {
            return;
        }

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }

    private void OnApplicationQuit(){
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }
}