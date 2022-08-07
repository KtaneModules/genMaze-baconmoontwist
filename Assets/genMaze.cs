using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class genMaze : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    public KMSelectable up;
    public KMSelectable down;
    public KMSelectable left;
    public KMSelectable right;
    public KMSelectable sub;
    public GameObject cube;
    public GameObject[] plush;
    public GameObject cylinder;
    public GameObject plus;
    //reading order vertical, then reading order horizontal (lol)
    public KMSelectable[] wallsDown;

    //top left is 0,0 i think
    private int xPos = 0;
    private int yPos = 0;
    private int[] vars = new int[] { 0, 0, 0, 0, 0 };
    private int[] goalPos = new int[] { 0, 0 };
    private int[] walls = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] wallsSol = new int[] { 2,2,2,2,4,4,4,4,6,6,6,6,8,8,8,8,10,10,10,10,3,3,3,3,3,5,5,5,5,5,7,7,7,7,7,9,9,9,9,9};

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        left.OnInteract += delegate () { PressLeft(); return false; };
        right.OnInteract += delegate () { PressRight(); return false; };
        up.OnInteract += delegate () { PressUp(); return false; };
        down.OnInteract += delegate () { PressDown(); return false; };
        sub.OnInteract += delegate () { PressSub(); return false; };
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        foreach (KMSelectable Arrow in wallsDown)
            Arrow.OnInteract += delegate () { ButtonPress(Arrow); return false; };
    }

    void Start()
    {
        
    }

    void OnActivate()
    {
        CalculateLegal();
        InitializeWalls();
        StartEnd();
        SetShape();

    }

    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    //good luck debugging
    void StartEnd()
    {
        goalPos[0] = Mod(NumStuck(), 5);
        goalPos[1] = Mod(goalPos[1],5);
        
        Debug.LogFormat("[Generated Maze #{2}] Goal position: {0},{1}", goalPos[0], goalPos[1],ModuleId);
        if (IsStuck(goalPos[0], goalPos[1]) == 1)
        {
            xPos = goalPos[0];
            yPos = goalPos[1];
        }
        else
        {
            xPos = goalPos[0];
            yPos = goalPos[1];
            while ((xPos == goalPos[0]) && (yPos == goalPos[1]))
            {
                MoveStart();
            }
        }
        Debug.LogFormat("[Generated Maze #{2}] Starting position: {0},{1}", xPos, yPos,ModuleId);

    }

    void MoveStart()
    {
            for (int k = 0; k < 10; k++)
            {
                int j = Rnd.Range(0, 4);
                if ((j == 0) && (LegalMoves(xPos, yPos)[0] == 1))
                {
                    xPos -= 1;
                }
                else if ((j == 1) && (LegalMoves(xPos, yPos)[1] == 1))
                {
                    xPos += 1;
                }
                else if ((j == 2) && (LegalMoves(xPos, yPos)[2] == 1))
                {
                    yPos += 1;
                }
                else if ((j == 3) && (LegalMoves(xPos, yPos)[3] == 1))
                {
                    yPos -= 1;
                }
            }
        
    }

    //sets up wallsSol and colors the cube
    void CalculateLegal()
    {
        vars[0] = Bomb.GetBatteryCount();
        vars[1] = Rnd.Range(0, 3);
        vars[2] = Bomb.GetPortPlateCount();
        vars[3] = Rnd.Range(0, 3);
        vars[4] = Bomb.GetIndicators().Count();
        Debug.LogFormat("[Generated Maze #{5}] vars: {0},{1},{2},{3},{4}", vars[0], vars[1], vars[2], vars[3], vars[4],ModuleId);
        //vertical walls
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                wallsSol[i*4+j] += Mod(vars[j] + vars[j + 1], 3);
            }
        }
        //down walls
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                wallsSol[20+i * 5 + j] += Mod(2*vars[j], 3);
            }
        }
        foreach (int i in wallsSol)
        {
            if (Mod(i,3)!=0)
            {
                goalPos[1] += 1;
            }
        }
    }

    //lrdu
    //in retrospect i dont know why this takes input
    int[] LegalMoves(int x, int y)
    {
        int[] i = { 0, 0, 0, 0 };
        if ((xPos > 0) && (Mod(wallsSol[yPos * 4 - 1 + xPos], 3)) == 0)
        {
            i[0]=1;
        }
        if ((xPos < 4) && (Mod(wallsSol[(xPos) + 4 * yPos], 3)) == 0)
        {
            i[1] = 1;
        }
        if ((yPos < 4) && (Mod(wallsSol[20 + 5 * yPos + xPos], 3)) == 0)
        {
            i[2] = 1;
        }
        if ((yPos > 0) && (Mod(wallsSol[20 + 5 * (yPos - 1) + xPos], 3)) == 0)
        {
            i[3] = 1;
        }
        return i;
    }

    int NumStuck()
    {
        int i = 0;
        for (int j = 0; j < 5;j++)
        {
            for (int k = 0; k < 5; k++)
            {
                if (IsStuck(j, k) == 1)
                    i++;
            }
        }
        return i;
    }

    void SetShape()
    {
        cube.transform.localPosition += new Vector3(xPos * 0.017f, 0, yPos * -0.017f);
        cylinder.transform.localPosition = new Vector3(20, 20, 20);
        plus.transform.localPosition = new Vector3(20, 20, 20);

        if (vars[1] == 0)
        {
            cube.GetComponent<Renderer>().material.color = Color.red;
        }
        else if (vars[1] == 1)
        {
            cube.GetComponent<Renderer>().material.color = Color.green;
        }
        else cube.GetComponent<Renderer>().material.color = Color.blue;

        if (vars[3] == 1)
        {
            cylinder.transform.localPosition = cube.transform.localPosition;
            cylinder.GetComponent<Renderer>().material.color = cube.GetComponent<Renderer>().material.color;
            cube.transform.localScale = new Vector3(0,0,0);
            plus.transform.localScale = new Vector3(0, 0, 0);
        }
        else if (vars[3] == 2)
        {
            plus.transform.localPosition = cube.transform.localPosition;
            plush[0].GetComponent<Renderer>().material.color = cube.GetComponent<Renderer>().material.color;
            plush[1].GetComponent<Renderer>().material.color = cube.GetComponent<Renderer>().material.color;
            cube.transform.localScale = new Vector3(0, 0, 0);
        }

    }

    void InitializeWalls()
    {
        foreach (KMSelectable button in wallsDown)
        {
            button.GetComponent<Renderer>().material.color = Color.black;

        }
    }

    bool WallsTrue()
    {
        for (int i = 0;i<40;i++)
        {
            wallsSol[i] = Mod(wallsSol[i], 3);
            if (wallsSol[i]==2)
            {
                wallsSol[i] = 1;
            }
            if (walls[i] == wallsSol[i])
                { return false; }
        }
        return true;
    }

    //really ugly but checks if a square is isolated
    int IsStuck(int xPos, int yPos)
    {
        if (((xPos == 0) || (Mod(wallsSol[yPos * 4 - 1 + xPos], 3)) != 0) &&
            ((xPos == 4) || (Mod(wallsSol[xPos + 4 * yPos], 3)) != 0) &&
            ((yPos == 4) || (Mod(wallsSol[20 + 5 * yPos + xPos], 3)) != 0) &&
            ((yPos == 0) || (Mod(wallsSol[20 + 5 * (yPos - 1) + xPos], 3)) != 0))
        {
            return 1;
        }
        else return 0;
    }


    void PressLeft()
    {
        left.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left.transform);
        if ((xPos > 0) && (Mod(wallsSol[yPos*4-1+xPos], 3)) == 0)
        {
            xPos--;
            cube.transform.localPosition += new Vector3(-0.017f, 0, 0);
            cylinder.transform.localPosition += new Vector3(-0.017f, 0, 0);
            plus.transform.localPosition += new Vector3(-0.017f, 0, 0);
        }
        else
        {
            Debug.LogFormat("[Generated Maze #{2}] Illegal move left at {0},{1}", xPos, yPos,ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }
    void PressRight()
    {
        if ((xPos < 4) && (Mod(wallsSol[ (xPos) + 4 * yPos], 3)) == 0)
        {
            right.AddInteractionPunch();
            xPos++;
            cube.transform.localPosition += new Vector3(0.017f, 0, 0);
            cylinder.transform.localPosition += new Vector3(0.017f, 0,0 );
            plus.transform.localPosition += new Vector3(0.017f, 0, 0);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        }
        else
        {
            Debug.LogFormat("[Generated Maze #{2}] Illegal move right at {0},{1}", xPos, yPos,ModuleId);
            right.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        }
    }
    void PressDown()
    {
        if ((yPos < 4) && (Mod(wallsSol[20 + 5*yPos+xPos], 3)) == 0)
        {
            down.AddInteractionPunch();
            yPos++;
            cube.transform.localPosition += new Vector3(0, 0, -0.017f);
            cylinder.transform.localPosition += new Vector3(0, 0, -0.017f);
            plus.transform.localPosition += new Vector3(0, 0, -0.017f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, down.transform);
        }
        else{
            Debug.LogFormat("[Generated Maze #{2}] Illegal move down at {0},{1}", xPos, yPos,ModuleId);
            down.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, down.transform);
        }
    }
    void PressUp()
    {
        if ((yPos > 0) && (Mod(wallsSol[20 + 5*(yPos-1)+xPos], 3)) == 0)
        {
            up.AddInteractionPunch();
            yPos--;
            cube.transform.localPosition += new Vector3(0, 0, 0.017f);
            cylinder.transform.localPosition += new Vector3(0, 0, 0.017f);
            plus.transform.localPosition += new Vector3(0, 0, 0.017f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, up.transform);
        }
        else{
            Debug.LogFormat("[Generated Maze #{2}] Illegal move up at {0},{1}", xPos, yPos,ModuleId);
            up.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, up.transform);
        }
    }
    void PressSub()
    {
        if ((xPos == goalPos[0]) && (yPos == goalPos[1]) && WallsTrue())
        {
            GetComponent<KMBombModule>().HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, sub.transform);
        }
        else if ((xPos != goalPos[0]) || (yPos != goalPos[1])) {
            Debug.LogFormat("[Generated Maze #{4}] Current position, {0},{1}, does not match goal position {2},{3}", xPos, yPos, goalPos[0], goalPos[1],ModuleId);
            sub.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, sub.transform);

        }
        else if (!WallsTrue())
        {
            Debug.LogFormat("[Generated Maze #{0}] Not all walls correct",ModuleId);
            sub.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, sub.transform);
        }
    }
    //changes the color and value of the maze walls when you press them
    void ButtonPress(KMSelectable button)
    {
        for (int i = 0; i < 40; i++)
        {
            if ((wallsDown[i] == button) && (walls[i] == 0))
            {
                button.GetComponent<Renderer>().material.color = Color.white;
                walls[i] = 1;
            }
            else if ((wallsDown[i] == button) && (walls[i] == 1))
            {
                button.GetComponent<Renderer>().material.color = Color.black;
                walls[i] = 0;
            }
        }

    }
    void Update()
    {

    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
