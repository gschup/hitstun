using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Assertions;

using HitstunConstants;


public class GameState
{
    public uint frameNumber;
    public uint hitstop;
    public Character[] characters;
    public CharacterData[] characterDatas;

    public void Serialize(BinaryWriter bw)
    {
        // Frame Number
        bw.Write(frameNumber);
        // hitstop
        bw.Write(hitstop);
        // Character State
        for (int i = 0; i < characters.Length; ++i)
        {
            characters[i].Serialize(bw);
        }
    }

    public void Deserialize(BinaryReader br)
    {
        // Frame Number
        frameNumber = br.ReadUInt32();
        // hitstop
        hitstop = br.ReadUInt32();
        // Character State
        characters = new Character[Constants.NUM_PLAYERS];
        for (int i = 0; i < characters.Length; ++i)
        {
            characters[i] = new Character();
            characters[i].Deserialize(br);
        }
    }

    public static NativeArray<byte> ToBytes(GameState gs)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                gs.Serialize(writer);
            }
            return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
        }
    }

    public static void FromBytes(GameState gs, NativeArray<byte> bytes)
    {
        Assert.IsNotNull(gs);
        using (var memoryStream = new MemoryStream(bytes.ToArray()))
        {
            using (var reader = new BinaryReader(memoryStream))
            {
                gs.Deserialize(reader);
            }
        }
    }

    public void Init()
    {
        frameNumber = 0;
        characters = new Character[Constants.NUM_PLAYERS];

        for (int i = 0; i < characters.Length; i++)
        {
            characters[i] = new Character();

            characters[i].position.x = (Constants.BOUNDS_WIDTH / 2) + (2 * i - 1) * Constants.INITIAL_CHARACTER_DISPLACEMENT;
            characters[i].position.y = 0;

            characters[i].facingRight = (i == 0) ? true : false;
            characters[i].onTop = (i == 0) ? true : false;
        }
    }

    public void Update(uint[] inputs, int disconnect_flags)
    {
        frameNumber++;
        // add inputs
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            if ((disconnect_flags & (1 << i)) != 0)
            {
                characters[i].ParseInputsToBuffer(0);
            }
            else
            {
                characters[i].ParseInputsToBuffer(inputs[i]);
            }
        }

        // hitstop
        if (hitstop > 0)
        {
            hitstop--;
            return;
        }

        // update character state, this also updates velocities
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            characters[i].UpdateCharacter(characterDatas[i]);
        }

        // apply velocity
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            characters[i].position.x += characters[i].velocity.x / Constants.FPS;
            characters[i].position.y += characters[i].velocity.y / Constants.FPS;

            // apply projectile velocity
            if (characters[i].projectile.active)
            {
                characters[i].projectile.position.x += characters[i].projectile.velocity.x / Constants.FPS;
                characters[i].projectile.position.y += characters[i].projectile.velocity.y / Constants.FPS;
            }
        }

        // interactions between characters
        // handle hitbox hurtbox interaction
        HandleHitBoxes();

        // handle collision box overlap
        HandleCollisionBoxes();

        // force players to stay within max distance and also within bounds of the stage
        HandleBounds();

        // update the facing direction depending on position and state
        UpdateFacingDirection();
    }

    public void ApplyHitBox(Character attackingChar, Character defendingChar, HitBox hitBox)
    {
        // apply hitstop
        hitstop = hitBox.hitstop;
        // check if blocking
        bool blocked = (hitBox.type == HitBoxType.MID && defendingChar.IsBlockingMid())
                    || (hitBox.type == HitBoxType.LOW && defendingChar.IsBlockingLow())
                    || (hitBox.type == HitBoxType.HIGH && defendingChar.IsBlockingHigh());

        // apply block
        if (blocked)
        {
            // set correct blocking state
            if (defendingChar.IsCrouch())
            {
                defendingChar.SetCharacterState(CharacterState.BLOCK_LOW);
            }
            else if (hitBox.type == HitBoxType.MID)
            {
                defendingChar.SetCharacterState(CharacterState.BLOCK_STAND);
            }
            else
            {
                defendingChar.SetCharacterState(CharacterState.BlOCK_HIGH);
            }
            // apply blockstun
            defendingChar.framesInState = 0;
            defendingChar.blockStun = hitBox.blockstun;
            // apply velocity
            if (defendingChar.IsInCorner()) 
            {
                attackingChar.velocity.x = attackingChar.facingRight ? -hitBox.pushback : hitBox.pushback;
            }
            else 
            {
                defendingChar.velocity.x = attackingChar.facingRight ? hitBox.pushback : -hitBox.pushback;
            }  
        }
        // apply hit
        else
        {   
            // set correct hit state
            if (defendingChar.IsCrouch())
            {
                defendingChar.SetCharacterState(CharacterState.HIT_CROUCH);
            }
            else if (defendingChar.IsStand())
            {
                defendingChar.SetCharacterState(CharacterState.HIT_STAND);
            }
            // apply hitstun
            defendingChar.framesInState = 0;
            defendingChar.hitStun = hitBox.hitstun;
            // apply velocity
            if (defendingChar.IsInCorner()) 
            {
                attackingChar.velocity.x = attackingChar.facingRight ? -hitBox.pushback : hitBox.pushback;
            }
            else 
            {
                defendingChar.velocity.x = attackingChar.facingRight ? hitBox.pushback : -hitBox.pushback;
            }  
        }
    }

    public void HandleHitBoxes()
    {   
        HitBox[] applicableHitboxes = new HitBox[Constants.NUM_PLAYERS];
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            Character thisChar = characters[i];
            Character otherchar = characters[1 - i];
            CharacterData thisData = characterDatas[i];
            CharacterData otherData = characterDatas[1 - i];

            List<Box> hurtBoxes;
            if (thisChar.hitBoxes.Count > 0 && otherchar.GetHurtBoxes(otherData, out hurtBoxes))
            {
                // displace the hurtboxes from relative coordinates to absolute coordinates
                foreach (Box hurtBox in hurtBoxes)
                {
                    hurtBox.Displace(otherchar.position.x, otherchar.position.y, otherchar.facingRight);
                }
                // detect colisions
                bool hitDetected = false;
                foreach (HitBox hitBox in thisChar.hitBoxes)
                {
                    if (hitDetected) break;
                    if (hitBox.used | !hitBox.enabled) continue;
                    HitBox absoluteHitBox = new HitBox(hitBox);
                    absoluteHitBox.Displace(thisChar.position.x, thisChar.position.y, thisChar.facingRight);

                    foreach (Box hurtBox in hurtBoxes)
                    {
                        Box overlap;
                        if (absoluteHitBox.GetOverlap(hurtBox, out overlap))
                        {
                            hitBox.used = true;
                            hitDetected = true;
                            thisChar.onTop = true;
                            otherchar.onTop = false;
                            applicableHitboxes[i] = absoluteHitBox;
                            break;
                        }
                    }
                }
            }
            //check projectile
            if (thisChar.projectile.active && otherchar.GetHurtBoxes(otherData, out hurtBoxes))
            {
                // displace the hurtboxes from relative coordinates to absolute coordinates
                foreach (Box hurtBox in hurtBoxes)
                {
                    hurtBox.Displace(otherchar.position.x, otherchar.position.y, otherchar.facingRight);
                }

                HitBox absoluteHitBox = new HitBox(thisChar.projectile.hitBox);
                absoluteHitBox.Displace(thisChar.projectile.position.x, thisChar.projectile.position.y, thisChar.projectile.facingRight);

                foreach (Box hurtBox in hurtBoxes)
                {
                    Box overlap;
                    if (absoluteHitBox.GetOverlap(hurtBox, out overlap))
                    {
                        thisChar.onTop = true;
                        otherchar.onTop = false;
                        thisChar.projectile.active = false;
                        applicableHitboxes[i] = absoluteHitBox;
                        break;
                    }
                }
            }
        }
        // apply the chosen hitboxes
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            if (applicableHitboxes[i] is null) continue;
            ApplyHitBox(characters[i], characters[1-i], applicableHitboxes[i]);
        }
    }

    public void HandleCollisionBoxes()
    {
        Box box1 = characters[0].GetCollisionBox(characterDatas[0]);
        Box box2 = characters[1].GetCollisionBox(characterDatas[1]);

        Box overlap;
        if (box1.GetOverlap(box2, out overlap))
        {
            bool resolveLeft = false;
            // resolve by x position
            if (characters[0].position.x < characters[1].position.x)
            {
                resolveLeft = true;
            }
            else if (characters[0].position.x > characters[1].position.x)
            {
                resolveLeft = false;
            }
            else
            {
                // if tied, resolve by x velocity
                if (characters[0].velocity.x < characters[1].velocity.x)
                {
                    resolveLeft = true;
                }
                else if (characters[0].velocity.x > characters[1].velocity.x)
                {
                    resolveLeft = false;
                }
                else
                {
                    // if tied, resolve by y position
                    if (characters[0].position.y < characters[1].position.y)
                    {
                        resolveLeft = true;
                    }
                    else if (characters[0].position.y > characters[1].position.y)
                    {
                        resolveLeft = false;
                    }
                    else
                    {
                        // it is getting awkward, just push player1 to the left (might need fixing)
                        Debug.Log("collision box resolution tied");
                        resolveLeft = true;
                    }
                }
            }
            // apply collision resolution
            int pushDistance = (overlap.GetWidth() / 2) + 1;
            characters[0].position.x += resolveLeft ? -pushDistance : pushDistance;
            characters[1].position.x += resolveLeft ? pushDistance : -pushDistance;
        }
    }

    public void UpdateFacingDirection()
    {
        // update facing direction
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            // don't update if the character is busy doing something
            if (!characters[i].IsIdle()) continue;

            bool newFacing = (characters[i].position.x < characters[1 - i].position.x) ? true : false;
            if (newFacing != characters[i].facingRight)
            {
                characters[i].FlipInputBufferInputs();
            }
            characters[i].facingRight = newFacing;
        }
    }

    public void HandleBounds()
    {
        for (int i = 0; i < Constants.NUM_PLAYERS; i++)
        {
            // force players to stay within max distance
            if (Math.Abs(characters[i].position.x - characters[1 - i].position.x) > Constants.MAX_CHARACTER_DISTANCE)
            {
                if (characters[i].position.x > characters[1 - i].position.x)
                {
                    characters[i].position.x = Constants.MAX_CHARACTER_DISTANCE + characters[1 - i].position.x;
                }
                else
                {
                    characters[i].position.x = characters[1 - i].position.x - Constants.MAX_CHARACTER_DISTANCE;
                }
            }

            // force players to stay within bounds
            characters[i].position.x = characters[i].position.x >= 0 ? characters[i].position.x : 0;
            characters[i].position.y = characters[i].position.y >= 0 ? characters[i].position.y : 0;
            characters[i].position.x = characters[i].position.x <= Constants.BOUNDS_WIDTH ? characters[i].position.x : Constants.BOUNDS_WIDTH;
            characters[i].position.y = characters[i].position.y <= Constants.BOUNDS_HEIGHT ? characters[i].position.y : Constants.BOUNDS_HEIGHT;
        }
    }
}

