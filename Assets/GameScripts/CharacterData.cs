using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public string name;
    public Params constants;
    public Dictionary<string, Animation> animations;
    public Dictionary<string, Attack> attacks;
    public Dictionary<string, ProjectileData> projectiles;

    public CharacterData()
    {
        animations = new Dictionary<string, Animation>();
        attacks = new Dictionary<string, Attack>();
        projectiles = new Dictionary<string, ProjectileData>();
    }
}


[Serializable]
public class Animation
{
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
public class Attack : Animation
{
    public HitBox[] hitBoxes;
    public uint[] specialCancelWindow;
    public uint spawnsProjectileAt;
}


[Serializable]
public class Params
{
    public int WALK_FORWARD;
    public int WALK_BACKWARD;
    public int JUMP_VELOCITY_X;
}


[Serializable]
public class Box
{
    public int xMin, xMax, yMin, yMax;

    public Box()
    {
        xMin = xMax = yMin = yMax = 0;
    }

    public Box(int _xMin, int _xMax, int _yMin, int _yMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        yMin = _yMin;
        yMax = _yMax;
    }

    public Box(int[] coords)
    {
        xMin = coords[0];
        xMax = coords[1];
        yMin = coords[2];
        yMax = coords[3];
    }

    public bool GetOverlap(Box other, out Box overlap)
    {

        int xMinOverlap = Mathf.Max(xMin, other.xMin);
        int xMaxOverlap = Mathf.Min(xMax, other.xMax);
        int yMinOverlap = Mathf.Max(yMin, other.yMin);
        int yMaxOverlap = Mathf.Min(yMax, other.yMax);

        if (xMinOverlap >= xMaxOverlap || yMinOverlap >= yMaxOverlap)
        {
            overlap = new Box(0, 0, 0, 0);
            return false;
        }
        overlap = new Box(xMinOverlap, xMaxOverlap, yMinOverlap, yMaxOverlap);
        return true;
    }

    public void Displace(int x, int y, bool facingRight)
    {
        int xMinOld = xMin;
        int xMaxOld = xMax;
        if (facingRight)
        {
            xMin = x + xMinOld;
            xMax = x + xMaxOld;
        }
        else
        {
            xMin = x - xMaxOld;
            xMax = x - xMinOld;
        }

        yMin += y;
        yMax += y;
    }

    public int[] GetCoords()
    {
        return new int[] { xMin, xMax, yMin, yMax };
    }

    public int GetWidth()
    {
        return xMax - xMin;
    }

    public int GetHeight()
    {
        return yMax - yMin;
    }

    public override string ToString()
    {
        return "Box: " + xMin.ToString() + ", " + xMax.ToString() + ", " + yMin.ToString() + ", " + yMax.ToString();
    }
}

public enum HitBoxType
{
    MID = 0,
    LOW = 1,
    HIGH = 2
}


[Serializable]
public class HitBox : Box
{

    public int startingFrame;
    public int duration;
    public bool enabled;
    public bool used;
    public HitBoxType type;
    public uint blockstun;
    public uint hitstun;
    public uint hitstop;
    public int pushback;

    public HitBox() : base() { }

    public HitBox(int _xMin, int _xMax, int _yMin, int _yMax) : base(_xMin, _xMax, _yMin, _yMax) { }

    public HitBox(int[] coords) : base(coords) { }

    public HitBox(HitBox copy) {
        xMin = copy.xMin;
        xMax = copy.xMax;
        yMin = copy.yMin;
        yMax = copy.yMax;
        startingFrame = copy.startingFrame;
        duration = copy.duration;
        enabled = copy.enabled;
        used = copy.used;
        type = copy.type;
        blockstun = copy.blockstun;
        hitstun = copy.hitstun;
        hitstop = copy.hitstop;
        pushback = copy.pushback;
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(xMin);
        bw.Write(xMax);
        bw.Write(yMin);
        bw.Write(yMax);
        bw.Write(startingFrame);
        bw.Write(duration);
        bw.Write(enabled);
        bw.Write(used);
        bw.Write((int)type);
        bw.Write(blockstun);
        bw.Write(hitstun);
        bw.Write(hitstop);
        bw.Write(pushback);
    }

    public void Deserialize(BinaryReader br)
    {
        xMin = br.ReadInt32();
        xMax = br.ReadInt32();
        yMin = br.ReadInt32();
        yMax = br.ReadInt32();
        startingFrame = br.ReadInt32();
        duration = br.ReadInt32();
        enabled = br.ReadBoolean();
        used = br.ReadBoolean();
        type = (HitBoxType)br.ReadInt32();
        blockstun = br.ReadUInt32();
        hitstun = br.ReadUInt32();
        hitstop = br.ReadUInt32();
        pushback = br.ReadInt32();
    }
}

[Serializable]
public class Projectile {

    public Vector2Int position;
    public Vector2Int velocity;
    public uint activeSince;
    public bool facingRight;
    public bool active;
    public HitBox hitBox;

    public Projectile() 
    {
        active = false;
        hitBox = new HitBox(-25,25,-100,100);
        hitBox.hitstop = 3;
        hitBox.type = HitBoxType.MID;
        hitBox.pushback = 4000;
        hitBox.blockstun = 15;
        hitBox.hitstun = 15;
        hitBox.used = false;

    }

    public Projectile(Projectile copy) 
    {
        position.x = copy.position.x;
        position.y = copy.position.y;
        velocity.x = copy.velocity.x;
        velocity.y = copy.velocity.y;
        activeSince = copy.activeSince;
        facingRight = copy.facingRight;
        active = copy.active;
    }

    public void Serialize(BinaryWriter bw)
    {
        // position
        bw.Write(position.x);
        bw.Write(position.y);
        // velocity
        bw.Write(velocity.x);
        bw.Write(velocity.y);
        // age in frames
        bw.Write(activeSince);
        // boolean
        bw.Write(facingRight);
        bw.Write(active);
        //hitbox
        hitBox.Serialize(bw);
    }

    public void Deserialize(BinaryReader br)
    {
        // position
        position.x = br.ReadInt32();
        position.y = br.ReadInt32();
        // velocity
        velocity.x = br.ReadInt32();
        velocity.y = br.ReadInt32();
        // age in frames
        activeSince = br.ReadUInt32();
        // booleans
        facingRight = br.ReadBoolean();
        active = br.ReadBoolean();
        //hitbox
        hitBox = new HitBox();
        hitBox.Deserialize(br);
    }
}

[Serializable]
public class ProjectileData {
    public string animationName;
    public int distinctSprites;
    public int totalFrames;
    public int dx;
}