using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
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
    public KMRuleSeedable RuleSeedable;
    //reading order vertical, then reading order horizontal (lol)
    public KMSelectable[] wallsDown;

    //top left is 0,0 i think
    private int xPos = 0;
    private int yPos = 0;
    private readonly int[] vars = new int[] { 0, 0, 0, 0, 0 };
    private readonly int[] goalPos = new int[] { 0, 0 };
    private readonly int[] walls = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    private readonly int[] wallsSol = new int[] { 2, 2, 2, 2, 4, 4, 4, 4, 6, 6, 6, 6, 8, 8, 8, 8, 10, 10, 10, 10, 3, 3, 3, 3, 3, 5, 5, 5, 5, 5, 7, 7, 7, 7, 7, 9, 9, 9, 9, 9 };

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

    void OnActivate()
    {
        CalculateLegal();
        InitializeWalls();
        StartEnd();
        SetShape();
        MissionControl();
    }

    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    //good luck debugging
    void StartEnd()
    {
        goalPos[0] = Mod(NumStuck(), 5);
        goalPos[1] = Mod(goalPos[1], 5);

        Debug.LogFormat("[Generated Maze #{2}] Goal position: {0},{1}", goalPos[0], goalPos[1], ModuleId);
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
                MoveStart();
        }
        Debug.LogFormat("[Generated Maze #{2}] Starting position: {0},{1}", xPos, yPos, ModuleId);

    }

    void MoveStart()
    {
        for (int k = 0; k < 10; k++)
        {
            int j = Rnd.Range(0, 4);
            if ((j == 0) && (LegalMoves(xPos, yPos)[0] == 1))
                xPos -= 1;
            else if ((j == 1) && (LegalMoves(xPos, yPos)[1] == 1))
                xPos += 1;
            else if ((j == 2) && (LegalMoves(xPos, yPos)[2] == 1))
                yPos += 1;
            else if ((j == 3) && (LegalMoves(xPos, yPos)[3] == 1))
                yPos -= 1;
        }

    }

    int ab(int x)
    {
        return Mod(x + 1, 2);
    }

    //im an idiot for naming my mission pack how123 but it is what it is
    void MissionControl()
    {
        if (GetMissionID() == "mod_how123_truleseeded")
        {
            if(RuleSeedable.GetRNG().Seed!=1)
            {

                GetComponent<KMBombModule>().HandleStrike();
                GetComponent<KMBombModule>().HandlePass();
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
        Debug.LogFormat("[Generated Maze #{5}] vars: {0},{1},{2},{3},{4}", vars[0], vars[1], vars[2], vars[3], vars[4], ModuleId);
        //vertical walls
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 4; j++)
                wallsSol[i * 4 + j] += Mod(vars[j] + vars[j + 1], 3);
        //down walls
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 5; j++)
                wallsSol[20 + i * 5 + j] += Mod(2 * vars[j], 3);
        foreach (int i in wallsSol)
            if (Mod(i, 3) != 0)
                goalPos[1] += 1;
        for (int i = 0; i < 40; i++)
        {
            wallsSol[i] = Mod(wallsSol[i], 3);
            if (wallsSol[i] == 2)
                wallsSol[i] -= 1;
        }
        Debug.LogFormat("[Generated Maze #{0}] Walls: 1 means not present (white) and 0 means present (black)", ModuleId);
        Debug.LogFormat("[Generated Maze #{4}]) {0} {1} {2} {3}  ", ab(wallsSol[0]), ab(wallsSol[1]), ab(wallsSol[2]), ab(wallsSol[3]), ModuleId);
        Debug.LogFormat("[Generated Maze #{5}]) {0} {1} {2} {3} {4}", ab(wallsSol[20]), ab(wallsSol[21]), ab(wallsSol[22]), ab(wallsSol[23]), ab(wallsSol[24]), ModuleId);
        Debug.LogFormat("[Generated Maze #{4}]) {0} {1} {2} {3}  ", ab(wallsSol[4]), ab(wallsSol[5]), ab(wallsSol[6]), ab(wallsSol[7]), ModuleId);
        Debug.LogFormat("[Generated Maze #{5}]) {0} {1} {2} {3} {4} ", ab(wallsSol[25]), ab(wallsSol[26]), ab(wallsSol[27]), ab(wallsSol[28]), ab(wallsSol[29]), ModuleId);
        Debug.LogFormat("[Generated Maze #{4}]) {0} {1} {2} {3}  ", ab(wallsSol[8]), ab(wallsSol[9]), ab(wallsSol[10]), ab(wallsSol[11]), ModuleId);
        Debug.LogFormat("[Generated Maze #{5}]) {0} {1} {2} {3} {4} ", ab(wallsSol[30]), ab(wallsSol[31]), ab(wallsSol[32]), ab(wallsSol[33]), ab(wallsSol[34]), ModuleId);
        Debug.LogFormat("[Generated Maze #{4}]) {0} {1} {2} {3}  ", ab(wallsSol[12]), ab(wallsSol[13]), ab(wallsSol[14]), ab(wallsSol[15]), ModuleId);
        Debug.LogFormat("[Generated Maze #{5}]) {0} {1} {2} {3} {4} ", ab(wallsSol[35]), ab(wallsSol[36]), ab(wallsSol[37]), ab(wallsSol[38]), ab(wallsSol[39]), ModuleId);
        Debug.LogFormat("[Generated Maze #{4}]) {0} {1} {2} {3}  ", ab(wallsSol[16]), ab(wallsSol[17]), ab(wallsSol[18]), ab(wallsSol[19]), ModuleId);
    }

    // Gets the mission ID - Thanks to S. (and espik because thats who i copied it off)
    private string GetMissionID()
    {
        try
        {
            Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
            Type type = gameplayState.GetType();
            FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
            return fieldMission.GetValue(gameplayState).ToString();
        }

        catch (NullReferenceException)
        {
            return "undefined";
        }
    }

    //lrdu
    //in retrospect i dont know why this takes input
    int[] LegalMoves(int x, int y)
    {
        int[] i = { 0, 0, 0, 0 };
        if ((xPos > 0) && (Mod(wallsSol[yPos * 4 - 1 + xPos], 3)) == 0)
            i[0] = 1;
        if ((xPos < 4) && (Mod(wallsSol[(xPos) + 4 * yPos], 3)) == 0)
            i[1] = 1;
        if ((yPos < 4) && (Mod(wallsSol[20 + 5 * yPos + xPos], 3)) == 0)
            i[2] = 1;
        if ((yPos > 0) && (Mod(wallsSol[20 + 5 * (yPos - 1) + xPos], 3)) == 0)
            i[3] = 1;
        return i;
    }

    //number of totally enclosed squares
    int NumStuck()
    {
        int i = 0;
        for (int j = 0; j < 5; j++)
            for (int k = 0; k < 5; k++)
                if (IsStuck(j, k) == 1)
                    i++;
        return i;
    }

    //sets the shape and color of the player through some super jank
    void SetShape()
    {
        cube.transform.localPosition += new Vector3(xPos * 0.017f, 0, yPos * -0.017f);
        cylinder.transform.localPosition = new Vector3(20, 20, 20);
        plus.transform.localPosition = new Vector3(20, 20, 20);

        if (vars[1] == 0)
            cube.GetComponent<Renderer>().material.color = Color.red;
        else if (vars[1] == 1)
            cube.GetComponent<Renderer>().material.color = Color.green;
        else cube.GetComponent<Renderer>().material.color = Color.blue;

        if (vars[3] == 1)
        {
            cylinder.transform.localPosition = cube.transform.localPosition;
            cylinder.GetComponent<Renderer>().material.color = cube.GetComponent<Renderer>().material.color;
            cube.transform.localScale = new Vector3(0, 0, 0);
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
            button.GetComponent<Renderer>().material.color = Color.black;
    }

    bool WallsTrue()
    {
        for (int i = 0; i < 40; i++)
        {
            wallsSol[i] = Mod(wallsSol[i], 3);
            if (wallsSol[i] == 2)
            {
                wallsSol[i] = 1;
            }
            if (walls[i] != wallsSol[i])
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

    //handles the buttons

    void PressLeft()
    {
        left.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left.transform);
        if ((xPos > 0) && (Mod(wallsSol[yPos * 4 - 1 + xPos], 3)) == 0)
        {
            xPos--;
            cube.transform.localPosition += new Vector3(-0.017f, 0, 0);
            cylinder.transform.localPosition += new Vector3(-0.017f, 0, 0);
            plus.transform.localPosition += new Vector3(-0.017f, 0, 0);
        }
        else
        {
            Debug.LogFormat("[Generated Maze #{2}] Illegal move left at {0},{1}", xPos, yPos, ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }
    void PressRight()
    {
        if ((xPos < 4) && (Mod(wallsSol[(xPos) + 4 * yPos], 3)) == 0)
        {
            right.AddInteractionPunch();
            xPos++;
            cube.transform.localPosition += new Vector3(0.017f, 0, 0);
            cylinder.transform.localPosition += new Vector3(0.017f, 0, 0);
            plus.transform.localPosition += new Vector3(0.017f, 0, 0);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        }
        else
        {
            Debug.LogFormat("[Generated Maze #{2}] Illegal move right at {0},{1}", xPos, yPos, ModuleId);
            right.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        }
    }
    void PressDown()
    {
        if ((yPos < 4) && (Mod(wallsSol[20 + 5 * yPos + xPos], 3)) == 0)
        {
            down.AddInteractionPunch();
            yPos++;
            cube.transform.localPosition += new Vector3(0, 0, -0.017f);
            cylinder.transform.localPosition += new Vector3(0, 0, -0.017f);
            plus.transform.localPosition += new Vector3(0, 0, -0.017f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, down.transform);
        }
        else
        {
            Debug.LogFormat("[Generated Maze #{2}] Illegal move down at {0},{1}", xPos, yPos, ModuleId);
            down.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, down.transform);
        }
    }
    void PressUp()
    {
        if ((yPos > 0) && (Mod(wallsSol[20 + 5 * (yPos - 1) + xPos], 3)) == 0)
        {
            up.AddInteractionPunch();
            yPos--;
            cube.transform.localPosition += new Vector3(0, 0, 0.017f);
            cylinder.transform.localPosition += new Vector3(0, 0, 0.017f);
            plus.transform.localPosition += new Vector3(0, 0, 0.017f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, up.transform);
        }
        else
        {
            Debug.LogFormat("[Generated Maze #{2}] Illegal move up at {0},{1}", xPos, yPos, ModuleId);
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
        else if ((xPos != goalPos[0]) || (yPos != goalPos[1]))
        {
            Debug.LogFormat("[Generated Maze #{4}] Current position, {0},{1}, does not match goal position {2},{3}", xPos, yPos, goalPos[0], goalPos[1], ModuleId);
            sub.AddInteractionPunch();
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, sub.transform);

        }
        else if (!WallsTrue())
        {
            Debug.LogFormat("[Generated Maze #{0}] Not all walls correct", ModuleId);
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
                button.GetComponent<Renderer>().material.color = Color.black;
                walls[i] = 1;
            }
            else if ((wallsDown[i] == button) && (walls[i] == 1))
            {
                button.GetComponent<Renderer>().material.color = Color.white;
                walls[i] = 0;
            }
        }
    }

    //the twitch play, thanks to quinn
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} toggle a1d c3r [Toggle the down wall of A1, then the right wall of C3.] | !{0} move urdl [Move up, right, down, left.] | !{0} submit [Submit.]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        var parameters = command.Trim().ToLowerInvariant().Split(' ');
        if (parameters.Length == 1 && Regex.Match(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
        {
            yield return null;
            sub.OnInteract();
            yield break;
        }
        if (Regex.Match(parameters[0], @"^\s*move\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success && parameters.Length != 1)
        {
            var s = command.Substring(5);
            List<int> list = new List<int>();
            KMSelectable[] btns = new[] { up, down, left, right };
            for (int i = 0; i < s.Length; i++)
            {
                string str = "udlr ";
                int ix = str.IndexOf(s[i]);
                if (ix == 4)
                    continue;
                if (ix == -1)
                    yield break;
                list.Add(ix);
            }
            yield return null;
            for (int i = 0; i < list.Count; i++)
            {
                btns[list[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }
        if (Regex.Match(parameters[0], @"^\s*toggle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
        {
            if (parameters.Length == 1)
                yield break;
            List<int> list = new List<int>();
            for (int i = 1; i < parameters.Length; i++)
            {
                int ix = GetWallFromTwitchCommand(parameters[i]);
                if (ix == -1)
                    yield break;
                list.Add(ix);
            }
            yield return null;
            for (int i = 0; i < list.Count; i++)
            {
                wallsDown[list[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }
    }

    private int GetWallFromTwitchCommand(string str)
    {
        string s = str.ToLowerInvariant();
        string[] leftWalls = new string[] { "b1l", "c1l", "d1l", "e1l", "b2l", "c2l", "d2l", "e2l", "b3l", "c3l", "d3l", "e3l", "b4l", "c4l", "d4l", "e4l", "b5l", "c5l", "d5l", "e5l" };
        string[] rightWalls = new string[] { "a1r", "b1r", "c1r", "d1r", "a2r", "b2r", "c2r", "d2r", "a3r", "b3r", "c3r", "d3r", "a4r", "b4r", "c4r", "d4r", "a5r", "b5r", "c5r", "d5r" };
        string[] upWalls = new string[] { "a2u", "b2u", "c2u", "d2u", "e2u", "a3u", "b3u", "c3u", "d3u", "e3u", "a4u", "b4u", "c4u", "d4u", "e4u", "a5u", "b5u", "c5u", "d5u", "e5u" };
        string[] downWalls = new string[] { "a1d", "b1d", "c1d", "d1d", "e1d", "a2d", "b2d", "c2d", "d2d", "e2d", "a3d", "b3d", "c3d", "d3d", "e3d", "a4d", "b4d", "c4d", "d4d", "e4d" };
        int[] ixs = new[] { Array.IndexOf(leftWalls, s), Array.IndexOf(rightWalls, s), Array.IndexOf(upWalls, s), Array.IndexOf(downWalls, s) };
        if (ixs[0] != -1) return ixs[0];
        if (ixs[1] != -1) return ixs[1];
        if (ixs[2] != -1) return ixs[2] + 20;
        if (ixs[3] != -1) return ixs[3] + 20;
        return -1;
    }

    struct QueueItem
    {
        public int Cell;
        public int Parent;
        public int Direction;
        public QueueItem(int cell, int parent, int dir)
        {
            Cell = cell;
            Parent = parent;
            Direction = dir;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = 0; i < wallsSol.Length; i++)
        {
            if (walls[i] != wallsSol[i])
            {
                wallsDown[i].OnInteract();
                yield return new WaitForSeconds(0.05f);
            }
        }
        var visited = new Dictionary<int, QueueItem>();
        var q = new Queue<QueueItem>();
        var curPos = xPos + yPos * 5;
        var sol = goalPos[0] + goalPos[1] * 5;
        q.Enqueue(new QueueItem(curPos, -1, 0));
        while (q.Count > 0)
        {
            var qi = q.Dequeue();
            if (visited.ContainsKey(qi.Cell))
                continue;
            visited[qi.Cell] = qi;
            if (qi.Cell == sol)
                break;
            var up = GetWallFromTwitchCommand("abcde"[qi.Cell % 5].ToString() + "12345"[qi.Cell / 5].ToString() + "u");
            var down = GetWallFromTwitchCommand("abcde"[qi.Cell % 5].ToString() + "12345"[qi.Cell / 5].ToString() + "d");
            var left = GetWallFromTwitchCommand("abcde"[qi.Cell % 5].ToString() + "12345"[qi.Cell / 5].ToString() + "l");
            var right = GetWallFromTwitchCommand("abcde"[qi.Cell % 5].ToString() + "12345"[qi.Cell / 5].ToString() + "r");
            if (up != -1 && wallsSol[up] == 0)
                q.Enqueue(new QueueItem(qi.Cell - 5, qi.Cell, 0));
            if (down != -1 && wallsSol[down] == 0)
                q.Enqueue(new QueueItem(qi.Cell + 5, qi.Cell, 1));
            if (left != -1 && wallsSol[left] == 0)
                q.Enqueue(new QueueItem(qi.Cell - 1, qi.Cell, 2));
            if (right != -1 && wallsSol[right] == 0)
                q.Enqueue(new QueueItem(qi.Cell + 1, qi.Cell, 3));
        }
        var r = sol;
        var path = new List<int>();
        while (true)
        {
            var nr = visited[r];
            if (nr.Parent == -1)
                break;
            path.Add(nr.Direction);
            r = nr.Parent;
        }
        path.Reverse();
        KMSelectable[] btns = new[] { up, down, left, right };
        for (int i = 0; i < path.Count; i++)
        {
            btns[path[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        sub.OnInteract();
    }
}
