using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class qkV : MonoBehaviour
{
    private readonly string[] allWords = new[] {"Voilà","View","Vaudevillian","Veteran","Vicariously","Victim","Villian","Vicissitudes","Visage","Veneer","Vanity","Vestige","Vacant","Vanished","Valorous","Visitiation","Vexation","Vivified","Vowed","Vanquish","Venal","Virulent","Vermin","Vanguarding","Vice","Vouchsafing","Violently","Vicious","Voracious","Violation","Volition","Verdict","Vengeance","Vendetta","Votive","Vain","Value","Veracity","Vindicate","Vigilant","Virtuous","Verily","Vichyssoise","Verbiage","Veers","Verbose","Very","V"};
    
    private string[] shuffled = new string[] {};
    private List<string> wordList = new List<string>();

    private List<string> currentWords = new List<string>();
    private bool rule1 = false;
    private bool rule2 = false;
    private bool rule3 = false;
    private bool rule4 = false;

    private string correctWord = "";

    private int moduleId;
    static int moduleIdCounter;

    private bool solved = false;

    private KMSelectable[] Children;
    void Start()
    {
        moduleId=moduleIdCounter++;
        shuffled = allWords;
        wordList = shuffled.Shuffle().ToList();
        Children=GetComponent<KMSelectable>().Children;
        for(int i = 0;i<5;i++)
        {
            int index = rnd.Range(0,wordList.Count);
            currentWords.Add(wordList[index]);
            wordList.RemoveAt(index);
        }
        rule1=GetComponent<KMBombInfo>().GetModuleIDs().ToArray().Intersect(new[] {"WireSequence", "Wires", "WhosOnFirst", "NeedyVentGas", "Simon", "Password", "Morse", "Memory", "Maze", "NeedyKnob", "Keypad", "Venn", "NeedyCapacitor", "BigButton"}).Any();
        rule2=GetComponent<KMBombInfo>().GetOnIndicators().Count()==2;
        rule3=Array.Exists(GetComponent<KMBombInfo>().GetModuleNames().ToArray(), item=>item.ToUpperInvariant().StartsWith("V") && item!="V");
        rule4=GetComponent<KMBombInfo>().GetBatteryCount(Battery.AA)>GetComponent<KMBombInfo>().GetBatteryCount(Battery.D);
        correctWord=rule1 || rule2 || rule3 || rule4 ? getWordInCommon(rule1 ? 3 : rule2 ? 5 : rule3 ? 4 : 1) : currentWords[1];
        Debug.LogFormat("[V #{0}] Words are: {1}, {2}, {3}, {4}, {5}", moduleId, currentWords[0], currentWords[1], currentWords[2], currentWords[3], currentWords[4]);
        Debug.LogFormat("[V #{0}] Rule {1} applied.", moduleId, rule1 ? 1 : rule2 ? 2 : rule3 ? 3 : rule4 ? 4 : 5);
        Debug.LogFormat("[V #{0}] Correct word to press is {1}.", moduleId, correctWord);
        for(int i = 0;i<Children.Length;i++)
        {
            KMSelectable Selectable = Children[i];
            Selectable.transform.Find("Text").GetComponent<TextMesh>().text=currentWords[i];
            Selectable.OnInteract += () => buttonInteract(Selectable, Selectable.transform.Find("Text").GetComponent<TextMesh>().text);
        }
        return;
    }

    bool buttonInteract(KMSelectable btn, string text)
    {
        btn.AddInteractionPunch(.5f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
        if(solved) return false;
        if(text==correctWord)
        {
            foreach(KMSelectable Selectable in Children)
            {
                Selectable.transform.Find("Text").GetComponent<TextMesh>().text="";
            }
            solved=true;
            Debug.LogFormat("[V #{0}] Correct word pressed! Solving module...", moduleId);
            GetComponent<KMBombModule>().HandlePass();
            return false;
        }
        OnStrike();
        return false;
    }

    void OnStrike()
    {
        Reset();
        GetComponent<KMBombModule>().HandleStrike();
        return;
    }
    void Reset()
    {
        Debug.LogFormat("[V #{0}] Strike! Resetting module...", moduleId);
        shuffled=allWords;
        wordList = shuffled.Shuffle().ToList();
        currentWords.Clear();
        for(int i = 0;i<5;i++)
        {
            int index = rnd.Range(0,wordList.Count);
            currentWords.Add(wordList[index]);
            wordList.RemoveAt(index);
        }
        correctWord=rule1 || rule2 || rule3 || rule4 ? getWordInCommon(rule1 ? 3 : rule2 ? 5 : rule3 ? 4 : 1) : currentWords[1];
        for(int i = 0;i<Children.Length;i++)
        {
            KMSelectable Selectable = Children[i];
            Selectable.transform.Find("Text").GetComponent<TextMesh>().text=currentWords[i];
        }
        Debug.LogFormat("[V #{0}] Words are: {1}, {2}, {3}, {4}, {5}", moduleId, currentWords[0], currentWords[1], currentWords[2], currentWords[3], currentWords[4]);
        Debug.LogFormat("[V #{0}] Correct word to press is {1}.", moduleId, correctWord);
        return;
    }

    string getWordInCommon(int index)
    {
        string[] temp = new[] {"Voilà","View","Vaudevillian","Veteran","Vicariously","Victim","Villian","Vicissitudes","Visage","Veneer","Vanity","Vestige","Vacant","Vanished","Valorous","Visitiation","Vexation","Vivified","Vowed","Vanquish","Venal","Virulent","Vermin","Vanguarding","Vice","Vouchsafing","Violently","Vicious","Voracious","Violation","Volition","Verdict","Vengeance","Vendetta","Votive","Vain","Value","Veracity","Vindicate","Vigilant","Virtuous","Verily","Vichyssoise","Verbiage","Veers","Verbose","Very","V"};        int ind = 0;
        foreach(string word in temp)
        {
            if(currentWords.Contains(word))
            {
                ind++;
                if(ind==index)
                {
                    return word;
                }
            }
        }
        return "";
    }

    #pragma warning disable 414
    [HideInInspector]
    public string TwitchHelpMessage = "Use '!{0} press <word>' or '!{0} press <position of button in reading order>' to press a button!";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command=command.ToUpperInvariant();
        if(command.StartsWith("PRESS "))
        {
            command=command.Replace("PRESS ","");
            int num = 0;
            if(int.TryParse(command, out num))
            {
                if(num>0 && num<6)
                {
                    yield return null;
                    Children[num-1].OnInteract();
                    yield break;
                }
                yield return null;
                yield return "sendtochaterror Number out of range!";
                yield break;
            }
            string[] Words = new[] {currentWords[0].ToUpperInvariant(), currentWords[1].ToUpperInvariant(), currentWords[2].ToUpperInvariant(), currentWords[3].ToUpperInvariant(), currentWords[4].ToUpperInvariant()};
            if(Words.Contains(command))
            {
                yield return null;
                Children[Array.IndexOf(Words, command)].OnInteract();
                yield break;
            }
            yield return null;
            yield return "sendtochaterror Word not found!";
            yield break;
        }
    }
}