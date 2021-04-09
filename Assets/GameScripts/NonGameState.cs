using HitstunConstants;

public struct PlayerConnectionInfo
{
    public int handle;
    public PlayerType type;
    public PlayerConnectState connectState;
    public int controllerId;
};

public struct ChecksumInfo
{
    public int frameNumber;
    public int checksum;
};

public class NonGameState
{
    public PlayerConnectionInfo[] players;
    public string status;
    public ChecksumInfo currentChecksum;

    public void SetConnectState(int handle, PlayerConnectState state)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].handle == handle)
            {
                players[i].connectState = state;
                break;
            }
        }
    }
}
