using System.Runtime.InteropServices;

public class GameMessage
{
    public enum MessageTypes
    {
        TypeConnectionMessage = (short)0,
        TypeIngameMessage = (short)1,
    }

    public interface IMessage
    {
        short GetMessageType();
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ConnectionMessage
    {
        // player connection data
        public short clientID;
        public float playerPosX;
        public float playerPosY;
        public ConnectionMessage(short clientID = 0, float playerPosX = 0.0f, float playerPosY = 0.0f)
        {
            this.clientID = clientID;
            this.playerPosX = playerPosX;
            this.playerPosY = playerPosY;
        }
        public short GetMessageType()
        {
            return (short)MessageTypes.TypeConnectionMessage;
        }
    }


    [System.Serializable]
    [StructLayout(LayoutKind.Sequential, Pack =4)]
    public class IngameMessage
    {
        // player in-game data
        public float playerPosX;
        public float playerPosY;

        public IngameMessage(float playerPosX = 0.0f, float playerPosY = 0.0f)
        {
            this.playerPosX = playerPosX;
            this.playerPosY = playerPosY;
        }

        public short GetMessageType()
        {
            return (short)MessageTypes.TypeIngameMessage;
        }
    }
}
