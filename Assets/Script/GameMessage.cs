using System;
using System.Runtime.InteropServices;

public class GameMessage
{
    public interface IMessage
    {
        short GetMessageType();
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct MessageHeader
    {
        public short clientID;
        public short messageLength;
        public MessageHeader(short clientID = 0, short messageLength = 0)
        {
            this.clientID = clientID;
            this.messageLength = messageLength;
        }

        // get byte size of struct
        public static int GetSize()
        {
            return Marshal.SizeOf(typeof(MessageHeader));
        }

        public static MessageHeader FromBytes(byte[] bytes)
        {
            MessageHeader header = new MessageHeader();
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            header = (MessageHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MessageHeader));
            handle.Free();
            return header;
        }
    }

    // enum class for action type
    public enum ActionType
    {
        MOVE = 0,
        JUMP = 1,
        ATTACK = 2,
        SKILL = 3,
        DIE = 4,
        RESPAWN = 5,
        GAMEOVER = 6,
        QUIT = 7,
        ENTER = 8,
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential, Pack =4)]
    public class IngameMessage
    {
        public short actionType;
        // player in-game data
        public float playerPosX;
        public float playerPosY;
        public float faceDirection;

        public IngameMessage(short actionType = 0, float playerPosX = 0.0f, float playerPosY = 0.0f, float faceDirection = 0)
        {
            this.actionType = actionType;
            // player in-game data
            this.playerPosX = playerPosX;
            this.playerPosY = playerPosY;
            this.faceDirection = faceDirection;
        }

        internal static IngameMessage FromBytes(byte[] messageBytes)
        {
            IngameMessage message = new IngameMessage();
            GCHandle handle = GCHandle.Alloc(messageBytes, GCHandleType.Pinned);
            message = (IngameMessage)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(IngameMessage));
            handle.Free();
            return message;
        }
    }
}
