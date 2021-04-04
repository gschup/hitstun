using UnityEngine;

namespace HitstunConstants {

    public static class Constants {
        // Misc
        public const int FRAME_DELAY = 2;
        public const int NUM_PLAYERS = 2;
        public const int INPUT_BUFFER_SIZE = 60;

        // Camera
        public const float SCALE = 1000.0f;
        public const float CAM_LOWER_BOUND = -4.3f;
        public const float CAM_UPPER_BOUND = 4.3f;

        // Game
        public const int BOUNDS_WIDTH = 12000;
        public const int BOUNDS_HEIGHT = 4000;
        public const int INITIAL_CHARACTER_DISPLACEMENT = 1000;
        public const int MAX_CHARACTER_DISTANCE = 3500;
    }

    public enum KeyPress : uint {
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

    public enum Inputs : uint {
        INPUT_BACK = (1 << 0),
        INPUT_FORWARD = (1 << 1),
        INPUT_UP = (1 << 2),
        INPUT_DOWN = (1 << 3),
        INPUT_LP = (1 << 4),
        INPUT_MP = (1 << 5),
        INPUT_HP = (1 << 6),
        INPUT_LK = (1 << 7),
        INPUT_MK = (1 << 8),
        INPUT_HK = (1 << 9)
    }

    public enum PlayerType {
        LOCAL = 0,
        REMOTE,
    };

    public enum PlayerConnectState {
        CONNECTING = 0,
        SYNCHRONIZING,
        RUNNING,
        DISCONNECTED,
        DISCONNECTING,
    };

    public enum CharacterName {
        KEN = 0
    }

    public enum CharacterState {
        IDLE = 0,
        CROUCH = 1,
        WALK_FORWARD = 2,
        WALK_BACKWARD = 3,
        STAND_TO_CROUCH = 4,
        CROUCH_TO_STAND = 5
    }
}

