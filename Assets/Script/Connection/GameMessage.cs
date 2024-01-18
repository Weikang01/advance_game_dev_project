using System;
using System.Runtime.InteropServices;

public class GameMessage
{
    // enum class for action type
    public enum MessageType
    {
        CLIENT_MESSAGE = 0,
        SCREENSHOT = 1,
        SYSTEM_MESSAGE = 2,
    }

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
        public short messageType;
        public MessageHeader(short clientID = 0, short messageLength = 0, short messageType = 0)
        {
            this.clientID = clientID;
            this.messageLength = messageLength;
            this.messageType = messageType;
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
        MOVE = 9,
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
    public class clientMessage
    {
        public short actionType;
        // player in-game data
        public float playerPosX;
        public float playerPosY;
        public float faceDirection;

        public clientMessage(short actionType = 0, float playerPosX = 0.0f, float playerPosY = 0.0f, float faceDirection = 0)
        {
            this.actionType = actionType;
            // player in-game data
            this.playerPosX = playerPosX;
            this.playerPosY = playerPosY;
            this.faceDirection = faceDirection;
        }

        internal static clientMessage FromBytes(byte[] messageBytes)
        {
            clientMessage message = new clientMessage();
            GCHandle handle = GCHandle.Alloc(messageBytes, GCHandleType.Pinned);
            message = (clientMessage)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(clientMessage));
            handle.Free();
            return message;
        }
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class ScreenShotHeaderMessage
    {
        public short width;
        public short height;

        public ScreenShotHeaderMessage(short width = 0, short height = 0)
        {
            this.width = width;
            this.height = height;
        }

        internal static ScreenShotHeaderMessage FromBytes(byte[] messageBytes)
        {
            ScreenShotHeaderMessage message = new ScreenShotHeaderMessage();
            GCHandle handle = GCHandle.Alloc(messageBytes, GCHandleType.Pinned);
            message = (ScreenShotHeaderMessage)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ScreenShotHeaderMessage));
            handle.Free();
            return message;
        }

        public static int GetSize()
        {
            return Marshal.SizeOf(typeof(ScreenShotHeaderMessage));
        }
    }
}
