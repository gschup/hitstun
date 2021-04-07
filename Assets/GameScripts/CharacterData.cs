using System;
using System.Collections.Generic;

[Serializable]
public class CharacterData {
    public string name;
    public Params constants;
    public Dictionary<string, Animation> animations;
    public Dictionary<string, Attack> attacks;

    public CharacterData() {
        animations = new Dictionary<string, Animation>();
        attacks = new Dictionary<string, Attack>();
    }
}

[Serializable]
public class Animation {
    public string animationName;
    public int distinctSprites;
    public int totalFrames;
    public int[] frameDuration;
    public int[] dx;
    public int[] dy;
    public int[] collisionBox;
    public bool staticHurtBox;
    public Dictionary<uint, int[][]> hurtBoxes;
}


[Serializable]
public class Attack : Animation {
    public Dictionary<uint, int[][]> hitBoxes;
}

[Serializable]
public class Params {
    public int WALK_FORWARD;
    public int WALK_BACKWARD;
    public int JUMP_VELOCITY_X;
}
