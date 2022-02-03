using System;
using System.IO;
using UnityEngine;
//using Valve.Newtonsoft.Json;

public class ConfigLoader
{
    private const string CONFIG_FILE_NAME = "config.json";

    private static ClientConfiguration DEFAULT_CONFIG = new ClientConfiguration()
    {
        Enabled = false,
        ClientHrri = "MUC-BLUE",
        ServerIp = "192.168.0.113",
        ServerPort = 7282,
        ClientPort = 2020
    };

    public static ClientConfiguration GetConfig()
    {
        if (_config == null)
        {
            _config = LoadClientConfig();
        }
        return _config;
    }

    private static ClientConfiguration _config;

    private static ClientConfiguration LoadClientConfig()
    {
        Debug.Log("Loading configuration...");

        string data = ReadFile(CONFIG_FILE_NAME);
        if (data == null)
        {
            string defaultConfigData = JsonConvert.SerializeObject(DEFAULT_CONFIG);
            Debug.Log("Can't load config, using default:\n" + defaultConfigData);
            WriteFile(CONFIG_FILE_NAME, defaultConfigData);
            return DEFAULT_CONFIG;
        }
        Debug.Log("Loaded config: " + data);
        ClientConfiguration config;
        try
        {
            config = JsonConvert.DeserializeObject<ClientConfiguration>(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            Debug.Log("Can't deserialize config, using default:\n" + JsonConvert.SerializeObject(DEFAULT_CONFIG));
            return DEFAULT_CONFIG;
        }
        return config;
    }

    private static void WriteFile(string fileName, string data)
    {
        try
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(data);
            sw.Close();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        finally
        {
        }
    }

    private static string ReadFile(string fileName)
    {
        string data = null;
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            StreamReader sr = new StreamReader(path);
            data = sr.ReadToEnd();
            sr.Close();
        }
        catch (DirectoryNotFoundException e)
        {
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        finally
        {
        }
        return data;
    }

    public class ClientConfiguration
    {
        public bool Enabled;
        public string ClientHrri;
        public string ServerIp;
        public int ServerPort;
        public int ClientPort;
    }
}