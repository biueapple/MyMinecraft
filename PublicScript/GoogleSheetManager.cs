using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetManager : MonoBehaviour
{
    const string Player =       "https://docs.google.com/spreadsheets/d/1TUQGBHr6HQpJYmRzpnGGwiZumDeAfGWw-UNxsIGiq2M/export?format=tsv&range=A2:N";
    const string testmonster =  "https://docs.google.com/spreadsheets/d/1TUQGBHr6HQpJYmRzpnGGwiZumDeAfGWw-UNxsIGiq2M/export?format=tsv&gid=1119557773&range=A2:N";
    public Unit player;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(GetData());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GetData()
    {
        float[] numbers;
        string[] strings;

        UnityWebRequest www = UnityWebRequest.Get(Player);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        print(data);
        strings = data.Split('\t');
        numbers = new float[strings.Length];
        for(int i = 0; i < strings.Length; i++)
        {
            numbers[i] = float.Parse(strings[i]);
        }
        //numbers[0] numbers[1] 이것은 레벨과 경험치 요구량임
        player.STAT.Init(numbers[2], numbers[3], numbers[4], numbers[5], numbers[6], numbers[7], numbers[8], numbers[9], numbers[10], numbers[11], numbers[12], numbers[13]);

        //www = UnityWebRequest.Get(testmonster);
        //yield return www.SendWebRequest();

        //data = www.downloadHandler.text;
        //print(data);
        //strings = data.Split('\t');
        //numbers = new float[strings.Length];
        //for (int i = 0; i < strings.Length; i++)
        //{
        //    numbers[i] = float.Parse(strings[i]);
        //}
        //monster.nomalStat.Init(numbers[2], numbers[3], numbers[4], numbers[5], numbers[6], numbers[7], numbers[8], numbers[9], numbers[10], numbers[11], numbers[12], numbers[13]);
    }

    public void GetTable(string str, Action<float[]> action)
    {
        StartCoroutine(GetData(str, action));
    }

    private IEnumerator GetData(string str, Action<float[]> action)
    {
        float[] numbers;
        string[] strings;

        UnityWebRequest www = UnityWebRequest.Get(str);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;

        strings = data.Split('\t');
        numbers = new float[strings.Length];
        for (int i = 0; i < strings.Length; i++)
        {
            numbers[i] = float.Parse(strings[i]);
        }
        action(numbers);
    }
}
