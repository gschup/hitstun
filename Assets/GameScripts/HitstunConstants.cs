using System;

namespace HitstunConstants
{

    public static class Constants
    {
        // Misc
        public const int FRAME_DELAY = 2;
        public const int NUM_PLAYERS = 2;
        public const int INPUT_BUFFER_SIZE = 60;
        public const int FPS = 60;

        // Camera
        public const float SCALE = 1000.0f;
        public const float CAM_LOWER_BOUND = -4.3f;
        public const float CAM_UPPER_BOUND = 4.3f;

        // Game
        public const int BOUNDS_WIDTH = 12000;
        public const int BOUNDS_HEIGHT = 4000;
        public const int INITIAL_CHARACTER_DISPLACEMENT = 1000;
        public const int MAX_CHARACTER_DISTANCE = 3500;

        // Leniencies
        public const int LENIENCY_BUFFER = 5;
        public const int LENIENCY_DASH = 10;
        public const int LENIENCY_QF = 10;
        public const int LENIENCY_DP = 15;
        public const int LENIENCY_DOUBLE_QF = 20;

        // Jump parameters
        public const int PREJUMP_FRAMES = 3;
        public const int JUMP_HEIGHT = 1000;
        public const float TIME_TO_PEAK = 0.3f;
        public const int GRAVITY = (int)(-(2 * JUMP_HEIGHT) / (TIME_TO_PEAK * TIME_TO_PEAK));
        public const int JUMP_VELOCITY_Y = (int)(2 * JUMP_HEIGHT / TIME_TO_PEAK);

        // pushback
        public const int FRICTION = 300;
        public const int PUSHBACK_CORNER_THRESH = 100;
    }

    public static class Motions
    {
        public static readonly uint[] DASH_FORWARD = { (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_NEUTRAL, (uint)Inputs.INPUT_FORWARD };
        public static readonly uint[] DASH_BACKWARD = { (uint)Inputs.INPUT_BACK, (uint)Inputs.INPUT_NEUTRAL, (uint)Inputs.INPUT_BACK };
        public static readonly uint[] QCF = { (uint)Inputs.INPUT_DOWN, (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_FORWARD };
        public static readonly uint[] HADOUKEN = { (uint)Inputs.INPUT_DOWN, (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_FORWARD, (uint) Inputs.INPUT_nMP, (uint) Inputs.INPUT_MP };
        public static readonly uint[] QCB = { (uint)Inputs.INPUT_DOWN, (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_BACK, (uint)Inputs.INPUT_BACK };
        public static readonly uint[] DOUBLE_QCF = { (uint)Inputs.INPUT_DOWN, (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_DOWN, (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_FORWARD };
        public static readonly uint[] DP = { (uint)Inputs.INPUT_FORWARD, (uint)Inputs.INPUT_DOWN, (uint)Inputs.INPUT_DOWN | (uint)Inputs.INPUT_FORWARD };

        public static bool isMotionInput(Inputs input)
        {
            return isMotionInput((uint)input);
        }
        public static bool isMotionInput(uint input)
        {
            return input < 16;
        }
    }

    [Flags]
    public enum KeyPress : uint
    {
        KEY_LEFT = (1 << 0),
        KEY_RIGHT = (1 << 1),
        KEY_UP = (1 << 2),
        KEY_DOWN = (1 << 3),
        KEY_LP = (1 << 4),
        KEY_MP = (1 << 5),
        KEY_HP = (1 << 6),
        KEY_LK = (1 << 7),
        KEY_MK = (1 << 8),
        KEY_HK = (1 << 9)
    }

    [Flags]
    public enum Inputs : uint
    {
        INPUT_NEUTRAL = 0,
        INPUT_BACK = (1 << 0),
        INPUT_FORWARD = (1 << 1),
        INPUT_UP = (1 << 2),
        INPUT_DOWN = (1 << 3),
        INPUT_LP = (1 << 4),
        INPUT_MP = (1 << 5),
        INPUT_HP = (1 << 6),
        INPUT_LK = (1 << 7),
        INPUT_MK = (1 << 8),
        INPUT_HK = (1 << 9),
        INPUT_nLP = (1 << 10),
        INPUT_nMP = (1 << 11),
        INPUT_nHP = (1 << 12),
        INPUT_nLK = (1 << 13),
        INPUT_nMK = (1 << 14),
        INPUT_nHK = (1 << 15)
    }

    public enum PlayerType
    {
        LOCAL = 0,
        REMOTE,
    };

    public enum PlayerConnectState
    {
        CONNECTING = 0,
        SYNCHRONIZING,
        RUNNING,
        DISCONNECTED,
        DISCONNECTING,
    };

    public enum CharacterName
    {
        KEN = 0
    }

    public enum CharacterState
    {
        // animations
        STAND = 0,
        CROUCH,
        WALK_FORWARD,
        WALK_BACKWARD,
        STAND_TO_CROUCH,
        CROUCH_TO_STAND,
        JUMP_NEUTRAL,
        JUMP_FORWARD,
        JUMP_BACKWARD,
        DASH_FORWARD,
        DASH_BACKWARD,
        BlOCK_HIGH,
        BLOCK_STAND,
        BLOCK_LOW,
        HIT_STAND,
        HIT_CROUCH,

        // attacks
        CROUCH_MK,
        HADOUKEN
    }
}

