using System;
using System.IO;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Assertions;

using HitstunConstants;


public class GameState {
    public int frameNumber;
    public Character[] characters;
    public CharacterData[] characterDatas;

    public void Serialize(BinaryWriter bw) {
        // Frame Number
        bw.Write(frameNumber);
        // Character State
        for (int i = 0; i < characters.Length; ++i) {
            characters[i].Serialize(bw);
        }
    }

    public void Deserialize(BinaryReader br) {
        // Frame Number
        frameNumber = br.ReadInt32();
        // Character State
        characters = new Character[Constants.NUM_PLAYERS];
        for (int i = 0; i < characters.Length; ++i) {
            characters[i] = new Character();
            characters[i].Deserialize(br);
        }
    }

    public static NativeArray<byte> ToBytes(GameState gs) {
        using (var memoryStream = new MemoryStream()) {
            using (var writer = new BinaryWriter(memoryStream)) {
                gs.Serialize(writer);
            }
            return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
        }
    }

    public static void FromBytes(GameState gs, NativeArray<byte> bytes) {
        Assert.IsNotNull(gs);
        using (var memoryStream = new MemoryStream(bytes.ToArray())) {
            using (var reader = new BinaryReader(memoryStream)) {
                gs.Deserialize(reader);
            }
        }
    }

    public void Init() {
        frameNumber = 0;
        characters = new Character[Constants.NUM_PLAYERS];

        for (int i = 0; i < characters.Length; i++) {
            characters[i] = new Character();

            characters[i].position.x = (Constants.BOUNDS_WIDTH / 2) + (2*i -1)*Constants.INITIAL_CHARACTER_DISPLACEMENT;
            characters[i].position.y = 0;

            characters[i].facingRight = (i == 0) ? true : false;
            characters[i].onTop = (i == 0) ? true : false;
        }
    }

    public void Update(uint[] inputs, int disconnect_flags) {
        frameNumber++;
        // add inputs and advance character state
        for (int i = 0; i < Constants.NUM_PLAYERS; i++) {
            if ((disconnect_flags & (1 << i)) != 0) {
                characters[i].AddInputsToBuffer(0);
            } else {
                characters[i].AddInputsToBuffer(inputs[i]);
            }
            // this also updates velocities
            characters[i].UpdateCharacter(characterDatas[i]);
        }

        // apply velocity and change facing direction
        for (int i = 0; i < Constants.NUM_PLAYERS; i++) {
            characters[i].position.x += characters[i].velocity.x;
            characters[i].position.y += characters[i].velocity.y;

            // update facing direction
            bool newFacing = (characters[i].position.x < characters[1-i].position.x) ? true : false;
            if (newFacing != characters[i].facingRight) {
                characters[i].FlipInputBufferInputs();
            }
            characters[i].facingRight = newFacing;
        }

        for (int i = 0; i < Constants.NUM_PLAYERS; i++) {
            // force players to stay within max distance
            if (Math.Abs(characters[i].position.x - characters[1-i].position.x) > Constants.MAX_CHARACTER_DISTANCE) {
                if (characters[i].position.x > characters[1-i].position.x) {
                    characters[i].position.x = Constants.MAX_CHARACTER_DISTANCE + characters[1-i].position.x;
                } else {
                    characters[i].position.x = characters[1-i].position.x - Constants.MAX_CHARACTER_DISTANCE;
                }
            }              

            // force players to stay within bounds
            characters[i].position.x = characters[i].position.x >= 0 ? characters[i].position.x : 0;
            characters[i].position.y = characters[i].position.y >= 0 ? characters[i].position.y : 0;
            characters[i].position.x = characters[i].position.x <= Constants.BOUNDS_WIDTH ? characters[i].position.x : Constants.BOUNDS_WIDTH;
            characters[i].position.y = characters[i].position.y <= Constants.BOUNDS_HEIGHT ? characters[i].position.y : Constants.BOUNDS_HEIGHT;
        }

        // interactions between characters
        // handle collision box overlap
        Box box1 = characters[0].GetCollisionBox(characterDatas[0]);
        Box box2 = characters[1].GetCollisionBox(characterDatas[1]);

        Box overlap;
        if (box1.getOverlap(box2, out overlap)) {
            if (characters[0].position.x < characters[1].position.x) {
                characters[0].position.x -= (overlap.getWidth() / 2)+1;
                characters[1].position.x += (overlap.getWidth() / 2)+1;
            } else {
                characters[0].position.x += (overlap.getWidth() / 2)+1;
                characters[0].position.x -= (overlap.getWidth() / 2)+1;
            }
        }
    }
}

