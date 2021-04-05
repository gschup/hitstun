using System;
using System.Collections.Generic;

[Serializable]
public class CharacterData {
    public string name;
    public Params constants;
    public Dictionary<string, Animation> animations;

    public CharacterData() {
        animations = new Dictionary<string, Animation>();
    }
}

[Serializable]
public class Animation {
    public string animationName;
    public int distinctSprites;
    public int totalFrames;
    public int[] frameDuration;
    public int[] collisionBox;
}

[Serializable]
public class Params {
    public int WALK_FORWARD;
    public int WALK_BACKWARD;
    public int JUMP_VELOCITY_X;
}
