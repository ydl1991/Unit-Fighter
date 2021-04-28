using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

/// -----------------------------------------------------------------------------------------
///
/// Summary
///     Packet type sending to client.
///
/// -----------------------------------------------------------------------------------------
public enum ToClientPackets
{
    // Server to clients
    kWelcomeFromServer = 0,
    kClientDisconnected,
    kTcpPunchThrough,
    kAssignClientSlot,
    kClientSetReady,
    kClientLoadGame,
    kClientStartGame,
    kSyncTimer,
    kAssignBattleGroup,
    kCardsSync,
    kPlayerDataSync,
    kGameStateChange,
    kCardPurchaseAction,
    kSellUnitAction,
    kMoveUnitAction,
    kDeclareBattleWinner,
    kTakeBattleDamage,
    kClientLose,
    kUdpTest,

    // Client to client
    kChatMessage,
}

/// -----------------------------------------------------------------------------------------
///
/// Summary
///     Packet type sending to server.
///
/// -----------------------------------------------------------------------------------------
public enum ToServerPackets
{
    kWelcomeReceived = 0,
    kClientSlotChangeRequest,
    kClientReadyButtonClick,
    kClientGameLoaded,
    kCardsRefreshRequest,
    kBuyExpRequest,
    kBuyCardRequest,
    kSellUnit,
    kMoveUnit,
    kUdpTestReceive
}

public class NetworkPacket : IDisposable
{
    private List<byte> m_buffer;
    private byte[] m_readableBuffer;
    private int m_readPos;
    private bool m_disposed;

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Creates a new empty packet (without an ID).
    ///
    /// -----------------------------------------------------------------------------------------
    public NetworkPacket()
    {
        m_buffer = new List<byte>(); // Intitialize buffer
        m_readPos = 0; // Set readPos to 0
        m_disposed = false; // default m_dispose to false
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Creates a new packet with a given ID. Used for sending.
    ///
    /// Param
    ///     id - The packet Id
    ///
    /// -----------------------------------------------------------------------------------------
    public NetworkPacket(int id)
    {
        m_buffer = new List<byte>(); // Intitialize buffer
        m_readPos = 0; // Set readPos to 0
        m_disposed = false; // default m_dispose to false

        Write(id); // Write packet id to the buffer
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Creates a packet from which data can be read. Used for receiving.
    ///
    /// Param
    ///     data - The bytes to add to the packet.
    ///
    /// -----------------------------------------------------------------------------------------
    public NetworkPacket(byte[] data)
    {
        m_buffer = new List<byte>(); // Intitialize buffer
        m_readPos = 0; // Set readPos to 0
        m_disposed = false; // default m_dispose to false

        SetBytes(data);
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sets the packet's content and prepares it to be read.
    ///
    /// Param
    ///     data - The bytes to add to the packet.
    ///
    /// -----------------------------------------------------------------------------------------
    public void SetBytes(byte[] data)
    {
        Write(data);
        m_readableBuffer = m_buffer.ToArray();
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Inserts the length of the packet's content at the start of the buffer.
    ///
    /// -----------------------------------------------------------------------------------------
    public void WriteLength()
    {
        m_buffer.InsertRange(0, BitConverter.GetBytes(m_buffer.Count)); // Insert the byte length of the packet at the very beginning
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Inserts the given int at the start of the buffer.
    ///
    /// Param
    ///     value - The int to insert.
    ///
    /// -----------------------------------------------------------------------------------------
    public void InsertInt(int value)
    {
        m_buffer.InsertRange(0, BitConverter.GetBytes(value)); // Insert the int at the start of the buffer
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Gets the packet's content in array form.
    ///
    /// -----------------------------------------------------------------------------------------
    public byte[] ToArray()
    {
        m_readableBuffer = m_buffer.ToArray();
        return m_readableBuffer;
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Gets the length of the packet's content.
    ///
    /// -----------------------------------------------------------------------------------------
    public int Length()
    {
        return m_buffer.Count; // Return the length of buffer
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Gets the length of the unread data contained in the packet.
    ///
    /// -----------------------------------------------------------------------------------------
    public int UnreadLength()
    {
        return Length() - m_readPos; // Return the remaining length (unread)
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Resets the packet instance to allow it to be reused.
    ///
    /// Param
    ///     shouldReset - Whether or not to reset the packet.
    ///
    /// -----------------------------------------------------------------------------------------
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            m_buffer.Clear(); // Clear buffer
            m_readableBuffer = null;
            m_readPos = 0; // Reset readPos
        }
        else
        {
            m_readPos -= 4; // "Unread" the last read int
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Functions below Add data to the packet.
    ///
    /// -----------------------------------------------------------------------------------------
    public void Write(byte value)
    {
        m_buffer.Add(value);
    }

    public void Write(byte[] value)
    {
        m_buffer.AddRange(value);
    }

    public void Write(short value)
    {
        m_buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(int value)
    {
        m_buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(long value)
    {
        m_buffer.AddRange(BitConverter.GetBytes(value));
    }
   
    public void Write(float value)
    {
        m_buffer.AddRange(BitConverter.GetBytes(value));
    }
 
    public void Write(bool value)
    {
        m_buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(string value)
    {
        Write(value.Length); // Add the length of the string to the packet
        m_buffer.AddRange(Encoding.ASCII.GetBytes(value)); // Add the string itself
    }

    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    public void Write(Card card)
    {
        Write(card.elemAttr);
        Write(card.profAttr);
        Write(card.color.r);
        Write(card.color.g);
        Write(card.color.b);
        Write(card.color.a);
        Write(card.deckId);
        Write((int)card.type);
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Functions below read data from the packet.
    ///
    /// -----------------------------------------------------------------------------------------
    public byte ReadByte(bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            byte value = m_readableBuffer[m_readPos]; // Get the byte at readPos' position
            if (moveReadPos)
            {
                // If _moveReadPos is true
                m_readPos += 1; // Increase readPos by 1
            }
            return value; // Return the byte
        }

        throw new Exception("Could not read value of type 'byte'!");
    }

    public byte[] ReadBytes(int length, bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            byte[] value = m_buffer.GetRange(m_readPos, length).ToArray(); // Get the bytes at readPos' position with a range of _length
            if (moveReadPos)
            {
                // If _moveReadPos is true
                m_readPos += length; // Increase readPos by _length
            }
            return value; // Return the bytes
        }

        throw new Exception("Could not read value of type 'byte[]'!");
    }

    public short ReadShort(bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            short value = BitConverter.ToInt16(m_readableBuffer, m_readPos); // Convert the bytes to a short
            if (moveReadPos)
            {
                // If _moveReadPos is true and there are unread bytes
                m_readPos += 2; // Increase readPos by 2
            }
            return value; // Return the short
        }

        throw new Exception("Could not read value of type 'short'!");
    }

    public int ReadInt(bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            int value = BitConverter.ToInt32(m_readableBuffer, m_readPos); // Convert the bytes to an int
            if (moveReadPos)
            {
                // If _moveReadPos is true
                m_readPos += 4; // Increase readPos by 4
            }
            return value; // Return the int
        }

        throw new Exception("Could not read value of type 'int'!");
    }

    public long ReadLong(bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            long value = BitConverter.ToInt64(m_readableBuffer, m_readPos); // Convert the bytes to a long
            if (moveReadPos)
            {
                // If _moveReadPos is true
                m_readPos += 8; // Increase readPos by 8
            }
            return value; // Return the long
        }

        throw new Exception("Could not read value of type 'long'!");
    }

    public float ReadFloat(bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            float value = BitConverter.ToSingle(m_readableBuffer, m_readPos); // Convert the bytes to a float
            if (moveReadPos)
            {
                // If _moveReadPos is true
                m_readPos += 4; // Increase readPos by 4
            }
            return value; // Return the float
        }

        throw new Exception("Could not read value of type 'float'!");
    }

    public bool ReadBool(bool moveReadPos = true)
    {
        if (m_buffer.Count > m_readPos)
        {
            // If there are unread bytes
            bool value = BitConverter.ToBoolean(m_readableBuffer, m_readPos); // Convert the bytes to a bool
            if (moveReadPos)
            {
                // If _moveReadPos is true
                m_readPos += 1; // Increase readPos by 1
            }
            return value; // Return the bool
        }

        throw new Exception("Could not read value of type 'bool'!");
    }

    public string ReadString(bool moveReadPos = true)
    {
        try
        {
            int length = ReadInt(); // Get the length of the string
            string value = Encoding.ASCII.GetString(m_readableBuffer, m_readPos, length); // Convert the bytes to a string
            if (moveReadPos && value.Length > 0)
            {
                // If _moveReadPos is true string is not empty
                m_readPos += length; // Increase readPos by the length of the string
            }
            return value; // Return the string
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    public Vector3 ReadVector3(bool moveReadPos = true)
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    public Quaternion ReadQuaternion(bool moveReadPos = true)
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    public Card ReadCard(bool moveReadPos = true)
    {
        return new Card(
            ReadString(moveReadPos), 
            ReadString(moveReadPos), 
            new Color32(ReadByte(moveReadPos), ReadByte(moveReadPos), ReadByte(moveReadPos), ReadByte(moveReadPos)), 
            ReadInt(moveReadPos), 
            (CardType)ReadInt(moveReadPos)
        );
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!m_disposed)
        {
            if (disposing)
            {
                m_buffer = null;
                m_readableBuffer = null;
                m_readPos = 0;
            }

            m_disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
