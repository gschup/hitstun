using System.IO;
using UnityEngine;
using HitstunConstants;

public class Character {
    public Vector2Int position;
    public Vector2Int velocity;
    public bool facingRight;
    public bool onTop;
    public CharacterState state;
    public uint framesInState;

    // Input Buffer
    private uint[] _inputBuffer;
    private uint _currentBufferPos;

    public Character() {
        // position and velocity
        position = new Vector2Int(0, 0);
        velocity = new Vector2Int(0, 0);
        // character state
        state = CharacterState.IDLE;
        framesInState = 0;
        // input Buffer
        _currentBufferPos = 0;
        _inputBuffer = new uint[Constants.INPUT_BUFFER_SIZE];
        for (int i=0; i<Constants.INPUT_BUFFER_SIZE; i++) {
            _inputBuffer[i] = 0;
        }
    }

    public void Serialize(BinaryWriter bw) {
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
        // input buffer
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; ++i) {
            bw.Write(_inputBuffer[i]);
        }
        bw.Write(_currentBufferPos);
    }

    public void Deserialize(BinaryReader br) {
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
        state = (CharacterState) br.ReadInt32();
        framesInState = br.ReadUInt32();
        // input buffer
        _inputBuffer = new uint[Constants.INPUT_BUFFER_SIZE];
        for (int i = 0; i < Constants.INPUT_BUFFER_SIZE; ++i) {
            _inputBuffer[i] = br.ReadUInt32();
        }
        _currentBufferPos = br.ReadUInt32();
    }

    public void AddInputsToBuffer(uint inputs) {
        uint sanitizedInputs = inputs;
        // if up+down, keep only the one that was pressed previously. if none, register none
        if ((inputs & (uint) KeyPress.KEY_DOWN) != 0 && (inputs & (uint) KeyPress.KEY_UP) != 0) {
            uint lastInputs = GetInputsByRelativeIndex(0);
            if ((lastInputs & (uint) KeyPress.KEY_DOWN) != 0) {
                sanitizedInputs &= ~ (uint) KeyPress.KEY_UP;
            } else if ((lastInputs & (uint) KeyPress.KEY_UP) != 0) {
                sanitizedInputs &= ~ (uint) KeyPress.KEY_DOWN;
            } else {
                sanitizedInputs &= ~ (uint) KeyPress.KEY_UP;
                sanitizedInputs &= ~ (uint) KeyPress.KEY_DOWN;
            }
        }
        //if left+right, keep only the one that was pressed previously. if none, register none
        if ((inputs & (uint) KeyPress.KEY_LEFT) != 0 && (inputs & (uint) KeyPress.KEY_RIGHT) != 0) {
            uint lastInputs = GetInputsByRelativeIndex(0);
            if ((lastInputs & (uint) KeyPress.KEY_LEFT) != 0) {
                sanitizedInputs &= ~ (uint) KeyPress.KEY_RIGHT;
            } else if ((lastInputs & (uint) KeyPress.KEY_RIGHT) != 0) {
                sanitizedInputs &= ~ (uint) KeyPress.KEY_LEFT;
            } else {
                sanitizedInputs &= ~ (uint) KeyPress.KEY_LEFT;
                sanitizedInputs &= ~ (uint) KeyPress.KEY_RIGHT;
            }
        }

        // convert keypresses to inputs (left/right to back/forward, depending on facing direction)
        uint convertedInputs = 0;

        if ((sanitizedInputs & (uint) KeyPress.KEY_LEFT) != 0) {
            if (facingRight) {
                convertedInputs |= (uint) Inputs.INPUT_BACK;
            } else {
                convertedInputs |= (uint) Inputs.INPUT_FORWARD;
            }
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_RIGHT) != 0) {
            if (facingRight) {
                convertedInputs |= (uint) Inputs.INPUT_FORWARD;
            } else {
                convertedInputs |= (uint) Inputs.INPUT_BACK;
            }
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_UP) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_UP;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_DOWN) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_DOWN;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_LP) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_LP;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_MP) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_MP;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_HP) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_HP;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_LK) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_LK;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_MK) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_MK;
        }
        if ((sanitizedInputs & (uint) KeyPress.KEY_HK) != 0) {
            convertedInputs |= (uint) Inputs.INPUT_HK;
        }

        // update buffer position
        _currentBufferPos = (_currentBufferPos + 1) % Constants.INPUT_BUFFER_SIZE;
        _inputBuffer[_currentBufferPos] = convertedInputs;
    }

    // index is relative, 0 is latest, -1 is one in the past etc
    public uint GetInputsByRelativeIndex(int index) {
        int newIndex = (int)_currentBufferPos + index;
        if (newIndex < 0) {
            newIndex += Constants.INPUT_BUFFER_SIZE;
        }
        return _inputBuffer[newIndex % Constants.INPUT_BUFFER_SIZE];
    }

    public void FlipInputBufferInputs() {
        for (int i=0; i<Constants.INPUT_BUFFER_SIZE; i++) {
            bool forward = (_inputBuffer[i] & (uint) Inputs.INPUT_FORWARD) != 0;
            bool back = (_inputBuffer[i] & (uint) Inputs.INPUT_BACK) != 0;

            _inputBuffer[i] &= ~ (uint) Inputs.INPUT_FORWARD;
            _inputBuffer[i] &= ~ (uint) Inputs.INPUT_BACK;

            if (forward) {
                _inputBuffer[i] |= (uint) Inputs.INPUT_BACK;
            }
            if (back) {
                _inputBuffer[i] |= (uint) Inputs.INPUT_FORWARD;
            }
        }
    }

    public bool CheckSequence(uint[] sequence, int maxDuration) {
        //int w = sequence.Length-1;
        //for(int i=0; i<maxDuration; ++i) {
        //    int direction = buffer[(CurrentTick-i+bufferSize) % bufferSize];
        //    if(direction == sequence[w]) 
        //        --w;
        //    if(w == -1) 
        //        return true;
        //}
        return false;
    }

    public void setCharacterState(CharacterState _state) {
        if (state != _state) {
            state = _state;
            framesInState = 0;
        }
    }

    public Box GetCollisionBox(CharacterData data) {
        int xMin, xMax, yMin, yMax;
        if (facingRight) {
            xMin = position.x + data.animations[state.ToString()].collisionBox[0];
            xMax = position.x + data.animations[state.ToString()].collisionBox[1];
        } else {
            xMin = position.x - data.animations[state.ToString()].collisionBox[1];
            xMax = position.x - data.animations[state.ToString()].collisionBox[0];
        }
        yMin = position.y + data.animations[state.ToString()].collisionBox[2];
        yMax = position.y + data.animations[state.ToString()].collisionBox[3];

        Box box = new Box(xMin, xMax, yMin, yMax);
        return box;
    }

    public void UpdateCharacter(CharacterData data) {
        framesInState++;

        uint latestInput = GetInputsByRelativeIndex(0);

        switch (state) {
            // IDLE STATE
            case CharacterState.IDLE:
            // WALK_FORWARD STATE
            case CharacterState.WALK_FORWARD:
            // WALK_BACKWARD STATE
            case CharacterState.WALK_BACKWARD:
                if ((latestInput & (uint) Inputs.INPUT_UP) != 0) {
                    setCharacterState(CharacterState.JUMP_NEUTRAL);
                    velocity.x = 0;
                    velocity.y = Constants.JUMP_VELOCITY;
                    break;
                }
                if ((latestInput & (uint) Inputs.INPUT_DOWN) != 0) {
                    setCharacterState(CharacterState.STAND_TO_CROUCH);
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                } 
                if ((latestInput & (uint) Inputs.INPUT_FORWARD) != 0) {
                    setCharacterState(CharacterState.WALK_FORWARD);
                    velocity.x = data.constants.WALK_FORWARD;
                    if (!facingRight) {
                        velocity.x *= -1;
                    }
                    velocity.y = 0;
                    break;
                }
                if ((latestInput & (uint) Inputs.INPUT_BACK) != 0) {
                    setCharacterState(CharacterState.WALK_BACKWARD);
                    velocity.x = data.constants.WALK_BACKWARD;
                    if (!facingRight) {
                        velocity.x *= -1;
                    }
                    velocity.y = 0;
                    break;
                }
                setCharacterState(CharacterState.IDLE);
                velocity.x = 0;
                velocity.y = 0;
                break;

            // CROUCH_TO_STAND STATE - technically already standing
            case CharacterState.CROUCH_TO_STAND:
                if (framesInState >= data.animations["CROUCH_TO_STAND"].totalFrames) {
                    setCharacterState(CharacterState.IDLE);
                }

                if ((latestInput & (uint) Inputs.INPUT_UP) != 0) {
                    setCharacterState(CharacterState.JUMP_NEUTRAL);
                    velocity.x = 0;
                    velocity.y = Constants.JUMP_VELOCITY;
                    break;
                }
                if ((latestInput & (uint) Inputs.INPUT_DOWN) != 0) {
                    setCharacterState(CharacterState.CROUCH);
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                } 
                if ((latestInput & (uint) Inputs.INPUT_FORWARD) != 0) {
                    setCharacterState(CharacterState.WALK_FORWARD);
                    velocity.x = data.constants.WALK_FORWARD;
                    if (!facingRight) {
                        velocity.x *= -1;
                    }
                    velocity.y = 0;
                    break;
                } 
                if ((latestInput & (uint) Inputs.INPUT_BACK) != 0) {
                    setCharacterState(CharacterState.WALK_BACKWARD);
                    velocity.x = data.constants.WALK_BACKWARD;
                    if (facingRight) {
                        velocity.x *= -1;
                    }
                    velocity.y = 0;
                    break;
                }
                velocity.x = 0;
                velocity.y = 0;
                break;
            // CROUCH STATE
            case CharacterState.CROUCH:
                if ((latestInput & (uint) Inputs.INPUT_UP) != 0) {
                    setCharacterState(CharacterState.JUMP_NEUTRAL);
                    velocity.x = 0;
                    velocity.y = Constants.JUMP_VELOCITY;
                    break;
                }
                if ((latestInput & (uint) Inputs.INPUT_DOWN) == 0) {
                    setCharacterState(CharacterState.CROUCH_TO_STAND);
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                }
                break;
            // STAND_TO_CROUCH STATE - technically already crouching
            case CharacterState.STAND_TO_CROUCH:
                if (framesInState >= data.animations["STAND_TO_CROUCH"].totalFrames) {
                    setCharacterState(CharacterState.CROUCH);
                }
                if ((latestInput & (uint) Inputs.INPUT_UP) != 0) {
                    setCharacterState(CharacterState.JUMP_NEUTRAL);
                    velocity.x = 0;
                    velocity.y = Constants.JUMP_VELOCITY;
                    break;
                }
                if ((latestInput & (uint) Inputs.INPUT_DOWN) == 0) {
                    setCharacterState(CharacterState.CROUCH_TO_STAND);
                    velocity.x = 0;
                    velocity.y = 0;
                    break;
                }
                break;
            // JUMP_NEUTRAL STATE
            case CharacterState.JUMP_NEUTRAL:
            // JUMP_FORWARD STATE
            case CharacterState.JUMP_FORWARD:
            // JUMP_BACKWARD STATE
            case CharacterState.JUMP_BACKWARD:
                velocity.x = 0;       
                velocity.y += Constants.GRAVITY / Constants.FPS;
                if (position.y <= 0) {
                    setCharacterState(CharacterState.IDLE);
                }
                break;
            default:
                Debug.Log("Character State invalid:" + state.ToString());
                break;
        }
    }
}
