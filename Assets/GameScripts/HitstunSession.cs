using UnityEngine;

using HitstunConstants;

public static class LocalSession
{
    public static GameState gs;
    public static NonGameState ngs;
    public static CharacterData[] characterDatas;

    public static void Init(GameState _gs, NonGameState _ngs)
    {
        gs = _gs;
        ngs = _ngs;
    }

    public static void Idle(int ms)
    {
    }

    public static void RunFrame()
    {
        var inputs = new uint[ngs.players.Length];
        for (int i = 0; i < inputs.Length; ++i)
        {
            inputs[i] = ReadInputs(ngs.players[i].controllerId);
        }
        gs.Update(inputs, 0);
    }

    public static uint ReadInputs(int controllerId)
    {
        uint input = 0;

        if (controllerId == 0)
        {
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.W))
            {
                input |= (uint)KeyPress.KEY_UP;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.S))
            {
                input |= (uint)KeyPress.KEY_DOWN;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.A))
            {
                input |= (uint)KeyPress.KEY_LEFT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.D))
            {
                input |= (uint)KeyPress.KEY_RIGHT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.U))
            {
                input |= (uint)KeyPress.KEY_LP;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.I))
            {
                input |= (uint)KeyPress.KEY_MP;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.O))
            {
                input |= (uint)KeyPress.KEY_HP;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.J))
            {
                input |= (uint)KeyPress.KEY_LK;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.K))
            {
                input |= (uint)KeyPress.KEY_MK;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.L))
            {
                input |= (uint)KeyPress.KEY_HK;
            }
        }
        else if (controllerId == 1)
        {
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.UpArrow))
            {
                input |= (uint)KeyPress.KEY_UP;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.DownArrow))
            {
                input |= (uint)KeyPress.KEY_DOWN;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftArrow))
            {
                input |= (uint)KeyPress.KEY_LEFT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightArrow))
            {
                input |= (uint)KeyPress.KEY_RIGHT;
            }
        }
        return input;
    }
}


