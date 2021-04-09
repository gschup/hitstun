using System.IO;
using System.Collections.Generic;
using UnityEngine;
using HitstunConstants;

public class Character
{
    public Vector2Int position;
    public Vector2Int velocity;
    public bool facingRight;
    public bool onTop;
    public CharacterState state;
    public uint framesInState;
    public uint blockStun;
    public uint hitStun;
    public List<HitBox> hitBoxes;

    // Input Buffer
    private uint[] inputBuffer;
    private uint currentBufferPos;

    public Character()
    {
        // position and velocity
        position = new Vector2Int(0, 0);
        velocity = new Vector2Int(0, 0);
        // character state
        state = CharacterState.IDLE;
        framesInState = 0;
        // input Buffer
        currentBufferPos = 0;
        inputBuffer = new uint[Constants.INPUT_BUFFER_SIZE];
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
        {
            inputBuffer[i] = 0;
        }
        // hitboxes list
        hitBoxes = new List<HitBox>();
    }

    public void Serialize(BinaryWriter bw)
    {
        // position
        bw.Write(position.x);
        bw.Write(position.y);
        // velocity
        bw.Write(velocity.x);
        bw.Write(velocity.y);
        // booleans
        bw.Write(facingRight);
        bw.Write(onTop);
        // state
        bw.Write((int)state);
        bw.Write(framesInState);
        bw.Write(blockStun);
        bw.Write(hitStun);
        // input buffer
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
        {
            bw.Write(inputBuffer[i]);
        }
        bw.Write(currentBufferPos);
        // hitbox list
        bw.Write(hitBoxes.Count);
        foreach (HitBox hitBox in hitBoxes)
        {
            hitBox.Serialize(bw);
        }
    }

    public void Deserialize(BinaryReader br)
    {
        // position
        position.x = br.ReadInt32();
        position.y = br.ReadInt32();
        // velocity
        velocity.x = br.ReadInt32();
        velocity.y = br.ReadInt32();
        // booleans
        facingRight = br.ReadBoolean();
        onTop = br.ReadBoolean();
        // state
        state = (CharacterState)br.ReadInt32();
        framesInState = br.ReadUInt32();
        blockStun = br.ReadUInt32();
        hitStun = br.ReadUInt32();
        // input buffer
        inputBuffer = new uint[Constants.INPUT_BUFFER_SIZE];
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; ++i)
        {
            inputBuffer[i] = br.ReadUInt32();
        }
        currentBufferPos = br.ReadUInt32();
        // hitbox list
        int hitBoxCount = br.ReadInt32();
        for (int i = 0; i < hitBoxCount; i++)
        {
            HitBox hitbox = new HitBox();
            hitBoxes.Add(hitbox);
            hitbox.Deserialize(br);
        }
    }

    public void AddInputsToBuffer(uint inputs)
    {
        uint sanitizedInputs = inputs;
        // if up+down, register none of them
        if ((inputs & (uint)KeyPress.KEY_DOWN) != 0 && (inputs & (uint)KeyPress.KEY_UP) != 0)
        {
            sanitizedInputs &= ~(uint)KeyPress.KEY_UP;
            sanitizedInputs &= ~(uint)KeyPress.KEY_DOWN;
        }
        //if left+right, register none of them
        if ((inputs & (uint)KeyPress.KEY_LEFT) != 0 && (inputs & (uint)KeyPress.KEY_RIGHT) != 0)
        {
            sanitizedInputs &= ~(uint)KeyPress.KEY_LEFT;
            sanitizedInputs &= ~(uint)KeyPress.KEY_RIGHT;
        }

        // convert keypresses to inputs (left/right to back/forward, depending on facing direction)
        uint convertedInputs = 0;

        if ((sanitizedInputs & (uint)KeyPress.KEY_LEFT) != 0)
        {
            convertedInputs |= facingRight ? (uint)Inputs.INPUT_BACK : (uint)Inputs.INPUT_FORWARD;
        }
        if ((sanitizedInputs & (uint)KeyPress.KEY_RIGHT) != 0)
        {
            convertedInputs |= facingRight ? (uint)Inputs.INPUT_FORWARD : (uint)Inputs.INPUT_BACK;
        }
        if ((sanitizedInputs & (uint)KeyPress.KEY_UP) != 0)
        {
            convertedInputs |= (uint)Inputs.INPUT_UP;
        }
        if ((sanitizedInputs & (uint)KeyPress.KEY_DOWN) != 0)
        {
            convertedInputs |= (uint)Inputs.INPUT_DOWN;
        }
        convertedInputs |= ((sanitizedInputs & (uint)KeyPress.KEY_LP) != 0) ? (uint)Inputs.INPUT_LP : (uint)Inputs.INPUT_nLP;
        convertedInputs |= ((sanitizedInputs & (uint)KeyPress.KEY_MP) != 0) ? (uint)Inputs.INPUT_MP : (uint)Inputs.INPUT_nMP;
        convertedInputs |= ((sanitizedInputs & (uint)KeyPress.KEY_HP) != 0) ? (uint)Inputs.INPUT_HP : (uint)Inputs.INPUT_nHP;
        convertedInputs |= ((sanitizedInputs & (uint)KeyPress.KEY_LK) != 0) ? (uint)Inputs.INPUT_LK : (uint)Inputs.INPUT_nLK;
        convertedInputs |= ((sanitizedInputs & (uint)KeyPress.KEY_MK) != 0) ? (uint)Inputs.INPUT_MK : (uint)Inputs.INPUT_nMK;
        convertedInputs |= ((sanitizedInputs & (uint)KeyPress.KEY_HK) != 0) ? (uint)Inputs.INPUT_HK : (uint)Inputs.INPUT_nHK;

        // update buffer position
        currentBufferPos = (currentBufferPos + 1) % Constants.INPUT_BUFFER_SIZE;
        inputBuffer[currentBufferPos] = convertedInputs;
    }

    // index is relative, 0 is latest, -1 is one in the past etc
    public uint GetInputsByRelativeIndex(int index)
    {
        int newIndex = (int)currentBufferPos + index;
        if (newIndex < 0)
        {
            newIndex += Constants.INPUT_BUFFER_SIZE;
        }
        return inputBuffer[newIndex % Constants.INPUT_BUFFER_SIZE];
    }

    public void FlipInputBufferInputs()
    {
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
        {
            bool forward = (inputBuffer[i] & (uint)Inputs.INPUT_FORWARD) != 0;
            bool back = (inputBuffer[i] & (uint)Inputs.INPUT_BACK) != 0;

            inputBuffer[i] &= ~(uint)Inputs.INPUT_FORWARD;
            inputBuffer[i] &= ~(uint)Inputs.INPUT_BACK;

            if (forward)
            {
                inputBuffer[i] |= (uint)Inputs.INPUT_BACK;
            }
            if (back)
            {
                inputBuffer[i] |= (uint)Inputs.INPUT_FORWARD;
            }
        }
    }

    public void FlushBuffer()
    {
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; i++)
        {
            inputBuffer[i] = 0;
        }
    }

    public bool CheckSequence(uint[] sequence, int maxDuration)
    {
        int w = sequence.Length - 1;
        for (int i = 0; i < maxDuration; i++)
        {
            uint inputs = GetInputsByRelativeIndex(-i);

            // remove either motions or buttons from input in order to compare
            if (Motions.isMotionInput(sequence[w]))
            {
                // remove all buttons from input
                inputs &= ~(uint)Inputs.INPUT_LP;
                inputs &= ~(uint)Inputs.INPUT_MP;
                inputs &= ~(uint)Inputs.INPUT_HP;
                inputs &= ~(uint)Inputs.INPUT_LK;
                inputs &= ~(uint)Inputs.INPUT_MK;
                inputs &= ~(uint)Inputs.INPUT_HK;
                inputs &= ~(uint)Inputs.INPUT_nLP;
                inputs &= ~(uint)Inputs.INPUT_nMP;
                inputs &= ~(uint)Inputs.INPUT_nHP;
                inputs &= ~(uint)Inputs.INPUT_nLK;
                inputs &= ~(uint)Inputs.INPUT_nMK;
                inputs &= ~(uint)Inputs.INPUT_nHK;
            }

            if (Motions.isMotionInput(sequence[w]))
            {
                // after removing buttons, motions need to match exactly
                if (inputs == sequence[w]) w--;
            }
            else
            {
                // buttons only need to be pressed
                if ((inputs & (uint)sequence[w]) != 0) w--;
            }

            if (w == -1) return true;
        }
        return false;
    }

    public void SetCharacterState(CharacterState _state)
    {
        if (state != _state)
        {
            state = _state;
            framesInState = 0;
            hitBoxes.Clear();
        }
    }

    public Box GetCollisionBox(CharacterData data)
    {
        int xMin, xMax, yMin, yMax;
        int[] collisionBox = isAttacking() ? data.attacks[state.ToString()].collisionBox : data.animations[state.ToString()].collisionBox;
        if (facingRight)
        {
            xMin = position.x + collisionBox[0];
            xMax = position.x + collisionBox[1];
        }
        else
        {
            xMin = position.x - collisionBox[1];
            xMax = position.x - collisionBox[0];
        }
        yMin = position.y + collisionBox[2];
        yMax = position.y + collisionBox[3];

        Box box = new Box(xMin, xMax, yMin, yMax);
        return box;
    }

    public bool GetHurtBoxes(CharacterData data, out List<Box> boxes)
    {
        boxes = new List<Box>();
        Animation animationData;
        if (isAttacking())
        {
            animationData = data.attacks[state.ToString()];
        }
        else
        {
            animationData = data.animations[state.ToString()];
        }
        if (animationData.hurtBoxes is null) return false;

        // check if the hurtboxes are static in this state
        uint index = animationData.staticHurtBox ? 0 : framesInState;

        // get the hurtboxes
        if (animationData.hurtBoxes.ContainsKey(index))
        {
            int[][] hurtBoxes = animationData.hurtBoxes[index];
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                boxes.Add(new Box(hurtBoxes[i]));
            }
            return true;
        }
        return false;
    }

    public bool isAttacking()
    {
        return state == CharacterState.CROUCH_MK;
    }

    public bool IsAirborne()
    {
        return (state == CharacterState.JUMP_NEUTRAL || state == CharacterState.JUMP_FORWARD || state == CharacterState.JUMP_BACKWARD) && framesInState > Constants.PREJUMP_FRAMES;
    }

    public bool IsIdle()
    {
        return state == CharacterState.IDLE ||
               state == CharacterState.WALK_BACKWARD ||
               state == CharacterState.WALK_FORWARD ||
               state == CharacterState.CROUCH ||
               state == CharacterState.CROUCH_TO_STAND ||
               state == CharacterState.STAND_TO_CROUCH;
    }

    public bool IsCrouch()
    {
        return state == CharacterState.CROUCH ||
               state == CharacterState.STAND_TO_CROUCH ||
               state == CharacterState.BLOCK_LOW ||
               state == CharacterState.HIT_CROUCH ||
               state == CharacterState.CROUCH_MK;
    }

    public bool IsStand()
    {
        return state == CharacterState.IDLE ||
               state == CharacterState.WALK_BACKWARD ||
               state == CharacterState.WALK_FORWARD ||
               state == CharacterState.CROUCH_TO_STAND ||
               state == CharacterState.DASH_FORWARD ||
               state == CharacterState.DASH_BACKWARD ||
               state == CharacterState.BLOCK_STAND ||
               state == CharacterState.BlOCK_HIGH ||
               state == CharacterState.HIT_STAND ||
               (state == CharacterState.JUMP_NEUTRAL && framesInState <= Constants.PREJUMP_FRAMES) ||
               (state == CharacterState.JUMP_FORWARD && framesInState <= Constants.PREJUMP_FRAMES) ||
               (state == CharacterState.JUMP_BACKWARD && framesInState <= Constants.PREJUMP_FRAMES);
    }

    public bool IsInCorner()
    {
        return position.x < Constants.PUSHBACK_CORNER_THRESH || position.x > Constants.BOUNDS_WIDTH - Constants.PUSHBACK_CORNER_THRESH;
    }

    public bool IsBlockingLow()
    {
        return IsIdle() && CheckSequence(new uint[] { (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_BACK }, 1);
    }
    public bool IsBlockingHigh()
    {
        return IsIdle() && CheckSequence(new uint[] { (uint)Inputs.INPUT_BACK }, 1);
    }

    public bool IsBlockingMid()
    {
        return IsBlockingHigh() || IsBlockingLow();
    }

    public void UpdateCharacter(CharacterData data)
    {
        framesInState++;
        // update hitboxes
        foreach (HitBox hitBox in hitBoxes)
        {
            hitBox.enabled = hitBox.startingFrame <= framesInState && hitBox.startingFrame + hitBox.duration >= framesInState;
        }

        switch (state)
        {
            // IDLE STATE
            case CharacterState.IDLE:
            // WALK_FORWARD STATE
            case CharacterState.WALK_FORWARD:
            // WALK_BACKWARD STATE
            case CharacterState.WALK_BACKWARD:
            // CROUCH_TO_STAND STATE - technically already standing
            case CharacterState.CROUCH_TO_STAND:
                if (CheckGroundedSpecials(data)) break;
                if (CheckStandingAttacks(data)) break;
                if (CheckDash()) break;
                if (CheckJump()) break;
                if (CheckCrouch()) break;
                if (CheckWalk(data)) break;
                // default idle
                if (state == CharacterState.CROUCH_TO_STAND)
                {
                    if (framesInState >= data.animations[state.ToString()].totalFrames)
                    {
                        SetCharacterState(CharacterState.IDLE);
                    }
                }
                else
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                velocity.x = 0;
                velocity.y = 0;
                break;
            // CROUCH STATE
            case CharacterState.CROUCH:
            // STAND_TO_CROUCH STATE - technically already crouching
            case CharacterState.STAND_TO_CROUCH:
                if (CheckGroundedSpecials(data)) break;
                if (CheckCrouchingAttacks(data)) break;
                if (CheckDash()) break;
                if (CheckJump()) break;
                if (CheckStand()) break;
                // default crouch
                if (state == CharacterState.STAND_TO_CROUCH)
                {
                    if (framesInState >= data.animations[state.ToString()].totalFrames)
                    {
                        SetCharacterState(CharacterState.CROUCH);
                    }
                }
                velocity.x = 0;
                velocity.y = 0;
                break;
            // JUMP_NEUTRAL STATE
            case CharacterState.JUMP_NEUTRAL:
                if (framesInState < Constants.PREJUMP_FRAMES)
                {
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                }
                if (framesInState == Constants.PREJUMP_FRAMES)
                {
                    velocity.x = 0;
                    velocity.y = Constants.JUMP_VELOCITY_Y;
                    break;
                }
                velocity.x = 0;
                velocity.y += Constants.GRAVITY / Constants.FPS;

                if (position.y <= 0)
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                break;
            // JUMP_FORWARD STATE
            case CharacterState.JUMP_FORWARD:
                if (framesInState < Constants.PREJUMP_FRAMES)
                {
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                }
                if (framesInState == Constants.PREJUMP_FRAMES)
                {
                    velocity.x = facingRight ? data.constants.JUMP_VELOCITY_X : -data.constants.JUMP_VELOCITY_X;
                    velocity.y = Constants.JUMP_VELOCITY_Y;
                    break;
                }
                velocity.x = velocity.x;
                velocity.y += Constants.GRAVITY / Constants.FPS;

                if (position.y <= 0)
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                break;
            // JUMP_BACKWARD STATE
            case CharacterState.JUMP_BACKWARD:
                if (framesInState < Constants.PREJUMP_FRAMES)
                {
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                }
                if (framesInState == Constants.PREJUMP_FRAMES)
                {
                    velocity.x = facingRight ? -data.constants.JUMP_VELOCITY_X : data.constants.JUMP_VELOCITY_X;
                    velocity.y = Constants.JUMP_VELOCITY_Y;
                    break;
                }
                velocity.x = velocity.x;
                velocity.y += Constants.GRAVITY / Constants.FPS;

                if (position.y <= 0)
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                break;
            // DASH_FORWARD STATE
            case CharacterState.DASH_FORWARD:
            // DASH_BACKWARD STATE
            case CharacterState.DASH_BACKWARD:
                int dx = data.animations[state.ToString()].dx[framesInState];
                velocity.x = facingRight ? dx : -dx;
                velocity.y = 0;
                if (framesInState >= data.animations[state.ToString()].totalFrames - 1)
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                break;
            // CROUCH_MK STATE
            case CharacterState.CROUCH_MK:
                velocity.x += facingRight ? Constants.FRICTION : -Constants.FRICTION;
                velocity.x = facingRight ? Mathf.Min(velocity.x, 0) : Mathf.Max(velocity.x, 0);
                // check for cancels :O
                if (framesInState >= data.attacks[state.ToString()].totalFrames - 1)
                {
                    SetCharacterState(CharacterState.CROUCH);
                }
                break;
            // BLOCK_HIGH STATE
            case CharacterState.BlOCK_HIGH:
            // BLOCK_STAND STATE
            case CharacterState.BLOCK_STAND:
                if (blockStun > (data.animations[state.ToString()].distinctSprites - 1) * 4)
                {
                    blockStun--;
                    framesInState--;
                }
                else if (blockStun > 0)
                {
                    blockStun--;
                }
                else
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                velocity.x += facingRight ? Constants.FRICTION : -Constants.FRICTION;
                velocity.x = facingRight ? Mathf.Min(velocity.x, 0) : Mathf.Max(velocity.x, 0);
                velocity.y = velocity.y;
                break;
            // BLOCK_LOW STATE
            case CharacterState.BLOCK_LOW:
                if (blockStun > (data.animations[state.ToString()].distinctSprites - 1) * 4)
                {
                    blockStun--;
                    framesInState--;
                }
                else if (blockStun > 0)
                {
                    blockStun--;
                }
                else
                {
                    SetCharacterState(CharacterState.CROUCH);
                }
                velocity.x += facingRight ? Constants.FRICTION : -Constants.FRICTION;
                velocity.x = facingRight ? Mathf.Min(velocity.x, 0) : Mathf.Max(velocity.x, 0);
                velocity.y = velocity.y;
                break;
            // HIT_STAND STATE
            case CharacterState.HIT_STAND:
                if (hitStun > (data.animations[state.ToString()].distinctSprites - 1) * 4)
                {
                    hitStun--;
                    framesInState--;
                }
                else if (hitStun > 0)
                {
                    hitStun--;
                }
                else
                {
                    SetCharacterState(CharacterState.IDLE);
                }
                velocity.x += facingRight ? Constants.FRICTION : -Constants.FRICTION;
                velocity.x = facingRight ? Mathf.Min(velocity.x, 0) : Mathf.Max(velocity.x, 0);
                velocity.y = velocity.y;
                break;
            // HIT_CROUCH STATE
            case CharacterState.HIT_CROUCH:
                if (hitStun > (data.animations[state.ToString()].distinctSprites - 1) * 4)
                {
                    hitStun--;
                    framesInState--;
                }
                else if (hitStun > 0)
                {
                    hitStun--;
                }
                else
                {
                    SetCharacterState(CharacterState.CROUCH);
                }
                velocity.x += facingRight ? Constants.FRICTION : -Constants.FRICTION;
                velocity.x = facingRight ? Mathf.Min(velocity.x, 0) : Mathf.Max(velocity.x, 0);
                velocity.y = velocity.y;
                break;
            default:
                Debug.Log("Character State invalid:" + state.ToString());
                velocity.x = 0;
                velocity.y = 0;
                break;
        }
    }

    public bool CheckGroundedSpecials(CharacterData data)
    {
        if (CheckSequence(Motions.DOUBLE_QCF, Constants.LENIENCY_DOUBLE_QF))
        {
            FlushBuffer();
            Debug.Log("DOUBLE_QCF");
            return true;
        }
        if (CheckSequence(Motions.DP, Constants.LENIENCY_DP))
        {
            FlushBuffer();
            Debug.Log("DP");
            return true;
        }
        if (CheckSequence(Motions.QCB, Constants.LENIENCY_QF))
        {
            FlushBuffer();
            Debug.Log("QCB");
            return true;
        }
        if (CheckSequence(Motions.QCF, Constants.LENIENCY_QF))
        {
            FlushBuffer();
            Debug.Log("QCF");
            return true;
        }
        return false;
    }

    public bool CheckStandingAttacks(CharacterData data)
    {
        return false;
    }

    public bool CheckCrouchingAttacks(CharacterData data)
    {
        if (CheckSequence(new uint[] { (uint)Inputs.INPUT_nMK, (uint)Inputs.INPUT_MK }, Constants.LENIENCY_BUFFER))
        {
            SetCharacterState(CharacterState.CROUCH_MK);
            // prepare the hitboxes
            foreach (HitBox hb in data.attacks[state.ToString()].hitBoxes)
            {
                HitBox hitBox = new HitBox(hb);
                hitBox.enabled = false;
                hitBox.used = false;
                hitBoxes.Add(hitBox);
            }
            return true;
        }
        return false;
    }

    public bool CheckDash()
    {
        if (CheckSequence(Motions.DASH_FORWARD, Constants.LENIENCY_DASH))
        {
            FlushBuffer();
            SetCharacterState(CharacterState.DASH_FORWARD);
            return true;
        }
        if (CheckSequence(Motions.DASH_BACKWARD, Constants.LENIENCY_DASH))
        {
            FlushBuffer();
            SetCharacterState(CharacterState.DASH_BACKWARD);
            return true;
        }
        return false;
    }

    public bool CheckJump()
    {
        if (CheckSequence(new uint[] { (uint)Inputs.INPUT_UP | (uint)Inputs.INPUT_FORWARD }, Constants.LENIENCY_BUFFER))
        {
            SetCharacterState(CharacterState.JUMP_FORWARD);
            velocity.x = 0;
            velocity.y = 0;
            return true;
        }
        if (CheckSequence(new uint[] { (uint)Inputs.INPUT_UP | (uint)Inputs.INPUT_BACK }, Constants.LENIENCY_BUFFER))
        {
            SetCharacterState(CharacterState.JUMP_BACKWARD);
            velocity.x = 0;
            velocity.y = 0;
            return true;
        }
        if (CheckSequence(new uint[] { (uint)Inputs.INPUT_UP }, Constants.LENIENCY_BUFFER))
        {
            SetCharacterState(CharacterState.JUMP_NEUTRAL);
            velocity.x = 0;
            velocity.y = 0;
            return true;
        }
        return false;
    }

    public bool CheckCrouch()
    {
        uint latestInput = GetInputsByRelativeIndex(0);
        if ((latestInput & (uint)Inputs.INPUT_DOWN) != 0)
        {
            SetCharacterState(CharacterState.STAND_TO_CROUCH);
            velocity.x = 0;
            velocity.y = 0;
            return true;
        }
        return false;
    }

    public bool CheckStand()
    {
        uint latestInput = GetInputsByRelativeIndex(0);
        if ((latestInput & (uint)Inputs.INPUT_DOWN) == 0)
        {
            SetCharacterState(CharacterState.CROUCH_TO_STAND);
            velocity.x = 0;
            velocity.y = 0;
            return true;
        }
        return false;
    }

    public bool CheckWalk(CharacterData data)
    {
        uint latestInput = GetInputsByRelativeIndex(0);
        if ((latestInput & (uint)Inputs.INPUT_FORWARD) != 0)
        {
            SetCharacterState(CharacterState.WALK_FORWARD);
            velocity.x = facingRight ? data.constants.WALK_FORWARD : -data.constants.WALK_FORWARD;
            velocity.y = 0;
            return true;
        }
        if ((latestInput & (uint)Inputs.INPUT_BACK) != 0)
        {
            SetCharacterState(CharacterState.WALK_BACKWARD);
            velocity.x = facingRight ? data.constants.WALK_BACKWARD : -data.constants.WALK_BACKWARD;
            velocity.y = 0;
            return true;
        }
        return false;
    }
}
