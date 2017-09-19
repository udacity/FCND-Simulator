using System;
using MavLink;
using UnityEngine;

namespace MavLink
{
    /// <summary>
    /// Mavlink communication class. 
    /// </summary>
    /// <remarks>
    /// Keeps track of state across send and receive of packets. 
    /// User of this class can just send Mavlink Messsages, and 
    /// receive them by feeding this class bytes off the wire as 
    /// they arrive
    /// </remarks>
   public class Mavlink
    {
       private byte[] leftovers;

       /// <summary>
       /// Event raised when a message is decoded successfully
       /// </summary>
       public event PacketReceivedEventHandler PacketReceived;

       /// <summary>
       /// Total number of packets successfully received so far
       /// </summary>
       public UInt32 PacketsReceived { get; private set; }

       /// <summary>
       /// Total number of packets which have been rejected due to a failed crc
       /// </summary>
       public UInt32 BadCrcPacketsReceived { get; private set; }
      
       /// <summary>
       /// Raised when a packet does not pass CRC
       /// </summary>
        public event PacketCRCFailEventHandler PacketFailedCRC;

       /// <summary>
       /// Raised when a number of bytes are passed over and cannot 
       /// be used to decode a packet
       /// </summary>
        public event PacketCRCFailEventHandler BytesUnused;

        // The current packet sequence number for transmission
        // public so it can be manipulated for testing
       // Normal usage would only read this
        public byte txPacketSequence;

        private byte systemId = 0;
        private byte componentId = 255;

        private byte sequenceNum = 0;

       /// <summary>
       /// Create a new MavlinkLink Object
       /// </summary>
       public Mavlink()
       {
           MavLinkSerializer.SetDataIsLittleEndian(MavlinkSettings.IsLittleEndian);
           leftovers = new byte[] {};
       }

       private int DecodePacketV2(byte[] newlyReceived, int idx) {
            var headerLen = 10;
            var magic = newlyReceived[idx];
            var payloadLen = newlyReceived[idx + 1];
            var incompatFlags = newlyReceived[idx + 2];
            var compatFlags = newlyReceived[idx + 3];
            var seq = newlyReceived[idx + 4];
            var sysid = newlyReceived[idx + 5];
            var compid = newlyReceived[idx + 6];
            var msg = this.Deserialize(newlyReceived, idx + 7);
            var checksumLow = newlyReceived[idx + headerLen + payloadLen + 1];
            var checksumHigh = newlyReceived[idx + headerLen + payloadLen];

            var checksumLen = 2;
            var packetLen = checksumLen + payloadLen + headerLen;

            var s = "";
            for (var i = idx; i < idx + packetLen; i++) {
                s += newlyReceived[i].ToString() + " ";
            }
            Debug.Log(string.Format("start index = {0}, packet len = {1}, total len = {2}, byte contents - {3}", idx, packetLen, newlyReceived.Length, s));


            Debug.Log("before crc");
            // subtract 1 since we start from 1
            var crc1 = Mavlink_Crc.Calculate(newlyReceived, (UInt16)(idx + 1), (UInt16)(headerLen + payloadLen - 1));
            Debug.Log("after crc");

            if (MavlinkSettings.CrcExtra)
            {
                var possibleMsgId = newlyReceived[idx + 7];
                if (!MavLinkSerializer.Lookup.ContainsKey(possibleMsgId))
                {
                    // we have received an unknown message. In this case we don't know the special
                    // CRC extra, so we have no choice but to fail.

                    // The way we do this is to just let the procedure continue
                    // There will be a natural failure of the main packet CRC
                }
                else
                {
                    var extra = MavLinkSerializer.Lookup[possibleMsgId];
                    crc1 = Mavlink_Crc.CrcAccumulate(extra.CrcExtra, crc1);
                }
            }

            byte crcHigh = (byte)(crc1 & 0xFF);
            byte crcLow = (byte)(crc1 >> 8);


            if (crcHigh == checksumHigh && crcLow == checksumLow) {
                var packet = new MavlinkPacket
                {
                    SystemId = sysid,
                    ComponentId = compid,
                    SequenceNumber = seq,
                    Message = msg
                };
                PacketReceived(this, packet);
                PacketsReceived++;
            } else {
                PacketFailedCRC(this, new PacketCRCFailEventArgs(newlyReceived, idx));
                BadCrcPacketsReceived++;
            }
            return idx + packetLen;
       }

        public void ParseBytesV2(byte[] newlyReceived) {
            /* 
            V2 Byte order

            uint8_t magic;              ///< protocol magic marker
            uint8_t len;                ///< Length of payload
            uint8_t incompat_flags;     ///< flags that must be understood
            uint8_t compat_flags;       ///< flags that can be ignored if not understood
            uint8_t seq;                ///< Sequence of packet
            uint8_t sysid;              ///< ID of message sender system/aircraft
            uint8_t compid;             ///< ID of the message sender component
            uint8_t msgid 0:7;          ///< first 8 bits of the ID of the message
            uint8_t msgid 8:15;         ///< middle 8 bits of the ID of the message
            uint8_t msgid 16:23;        ///< last 8 bits of the ID of the message
            uint8_t target_sysid;       ///< Optional field for point-to-point messages, used for payload else
            uint8_t target_compid;      ///< Optional field for point-to-point messages, used for payload else
            uint8_t payload[max 253];   ///< A maximum of 253 payload bytes
            uint16_t checksum;          ///< X.25 CRC
            uint8_t signature[13];      ///< Signature which allows ensuring that the link is tamper-proof
            */


            // byte array may contain multiple packets/messages
            var idx = 0;
            var bytesLen = newlyReceived.Length;
            while (idx < bytesLen) {
                // decode the packet (msg + 10 header + 2 crc), ignoring signature
                idx = DecodePacketV2(newlyReceived, idx);
                Debug.Log(string.Format("idx {0}", idx));
            }
        }


        /// <summary>
       /// Process latest bytes from the stream. Received packets will be raised in the event
       /// </summary>
       public void ParseBytes(byte[] newlyReceived)
       {
           uint i = 0;

           // copy the old and new into a contiguous array
           // This is pretty inefficient...
           var bytesToProcess = new byte[newlyReceived.Length + leftovers.Length];
           int j = 0;

           for (i = 0; i < leftovers.Length; i++)
               bytesToProcess[j++] = leftovers[i];

           for (i = 0; i < newlyReceived.Length; i++)
               bytesToProcess[j++] = newlyReceived[i];

           i = 0;

           // we are going to loop and decode packets until we use up the data
           // at which point we will return. Hence one call to this method could
           // result in multiple packet decode events
           while (true)
           {
               // Hunt for the start char
               int huntStartPos = (int) i;

               while (i < bytesToProcess.Length && bytesToProcess[i] != MavlinkSettings.ProtocolMarker)
                   i++;

               if (i == bytesToProcess.Length)
               {
                   // No start byte found in all our bytes. Dump them, Exit.
                   leftovers = new byte[] { };
                   return;
               }

               if (i > huntStartPos)
               {
                   // if we get here then are some bytes which this code thinks are 
                   // not interesting and would be dumped. For diagnostics purposes,
                   // lets pop these bytes up in an event.
                   if (BytesUnused != null)
                   {
                       var badBytes = new byte[i - huntStartPos];
                       Array.Copy(bytesToProcess, huntStartPos, badBytes, 0, (int)(i - huntStartPos));
                       BytesUnused(this, new PacketCRCFailEventArgs(badBytes, bytesToProcess.Length - huntStartPos));
                   }
               }

               // We need at least the minimum length of a packet to process it. 
               // The minimum packet length is 8 bytes for acknowledgement packets without payload
               // if we don't have the minimum now, go round again
               if (bytesToProcess.Length - i < 8)
               {
                   leftovers = new byte[bytesToProcess.Length - i];
                   j = 0;
                   while (i < bytesToProcess.Length)
                       leftovers[j++] = bytesToProcess[i++];
                   return;
               }

               /*
                * Byte order:
                * 
                * 0  Packet start sign	
                * 1	 Payload length	 0 - 255
                * 2	 Packet sequence	 0 - 255
                * 3	 System ID	 1 - 255
                * 4	 Component ID	 0 - 255
                * 5	 Message ID	 0 - 255
                * 6 to (n+6)	 Data	 (0 - 255) bytes
                * (n+7) to (n+8)	 Checksum (high byte, low byte) for v0.9, lowbyte, highbyte for 1.0
                *
                */

               UInt16 payLoadLength = bytesToProcess[i + 1];

               // Now we know the packet length, 
               // If we don't have enough bytes in this packet to satisfy that packet lenghth,
               // then dump the whole lot in the leftovers and do nothing else - go round again
               if (payLoadLength > (bytesToProcess.Length - i - 8)) // payload + 'overhead' bytes (crc, system etc)
               {
                   // back up to the start char for next cycle
                   j = 0;

                   leftovers = new byte[bytesToProcess.Length - i];

                   for (; i < bytesToProcess.Length; i++)
                   {
                       leftovers[j++] = bytesToProcess[i];
                   }
                   return;
               }

               i++;

               // Check the CRC. Does not include the starting 'U' byte but does include the length
               var crc1 = Mavlink_Crc.Calculate(bytesToProcess, (UInt16)(i), (UInt16)(payLoadLength + 5));

               if (MavlinkSettings.CrcExtra)
               {
                   var possibleMsgId = bytesToProcess[i + 4];

                   if (!MavLinkSerializer.Lookup.ContainsKey(possibleMsgId))
                   {
                       // we have received an unknown message. In this case we don't know the special
                       // CRC extra, so we have no choice but to fail.

                       // The way we do this is to just let the procedure continue
                       // There will be a natural failure of the main packet CRC
                   }
                   else
                   {
                       var extra = MavLinkSerializer.Lookup[possibleMsgId];
                       crc1 = Mavlink_Crc.CrcAccumulate(extra.CrcExtra, crc1);
                   }
               }

               byte crcHigh = (byte)(crc1 & 0xFF);
               byte crcLow = (byte)(crc1 >> 8);

               byte messageCrcHigh = bytesToProcess[i +  5  + payLoadLength];
               byte messageCrcLow = bytesToProcess[i + 6  + payLoadLength];

               if (messageCrcHigh == crcHigh && messageCrcLow == crcLow)
               {
                   // This is used for data drop outs metrics, not packet windows
                   // so we should consider this here. 
                   // We pass up to subscribers only as an advisory thing
                   var rxPacketSequence = bytesToProcess[++i];
                   i++;
                   var packet = new byte[payLoadLength + 3];  // +3 because we are going to send up the sys and comp id and msg type with the data

                   for (j = 0; j < packet.Length; j++)
                       packet[j] = bytesToProcess[i + j];

                   var debugArray = new byte[payLoadLength + 7];
                   Array.Copy(bytesToProcess, (int)(i - 3), debugArray, 0, debugArray.Length);

                   //OnPacketDecoded(packet, rxPacketSequence, debugArray);

                   ProcessPacketBytes(packet, rxPacketSequence);

                   PacketsReceived++;

                   // clear leftovers, just incase this is the last packet
                   leftovers = new byte[] { };

                   //  advance i here by j to avoid unecessary hunting
                   // todo: could advance by j + 2 I think?
                   i = i + (uint)(j + 2);
               }
               else
               {
                   var badBytes = new byte[i + 7 + payLoadLength];
                   Array.Copy(bytesToProcess, (int)(i - 1), badBytes, 0, payLoadLength + 7);

                   if (PacketFailedCRC != null)
                   {
                       PacketFailedCRC(this, new PacketCRCFailEventArgs(badBytes, (int)(bytesToProcess.Length - i - 1)));
                   }

                   BadCrcPacketsReceived++;
               }
           }
       }  

       public byte[] Send(MavlinkPacket mavlinkPacket)
       {
           var bytes = this.Serialize(mavlinkPacket.Message, mavlinkPacket.SystemId, mavlinkPacket.ComponentId);
           return SendPacketLinkLayer(bytes);
       }

        public byte[] SendV2(MavlinkMessage msg)
        {
            /* V2 Byte order
               uint8_t magic;              ///< protocol magic marker
               uint8_t len;                ///< Length of payload
               uint8_t incompat_flags;     ///< flags that must be understood
               uint8_t compat_flags;       ///< flags that can be ignored if not understood
               uint8_t seq;                ///< Sequence of packet
               uint8_t sysid;              ///< ID of message sender system/aircraft
               uint8_t compid;             ///< ID of the message sender component
               uint8_t msgid 0:7;          ///< first 8 bits of the ID of the message
               uint8_t msgid 8:15;         ///< middle 8 bits of the ID of the message
               uint8_t msgid 16:23;        ///< last 8 bits of the ID of the message
               uint8_t target_sysid;       ///< Optional field for point-to-point messages, used for payload else
               uint8_t target_compid;      ///< Optional field for point-to-point messages, used for payload else
               uint8_t payload[max 253];   ///< A maximum of 253 payload bytes
               uint16_t checksum;          ///< X.25 CRC
               uint8_t signature[13];      ///< Signature which allows ensuring that the link is tamper-proof
           */

            // need to first serialize the message itself
            var buff = new byte[256];
            var endPos = 0;
            int msgId = msg.Serialize(buff, ref endPos);
            var serializedMsg = new byte[endPos];
            Array.Copy(buff, serializedMsg, endPos);

            // create the packet (msg + 10 header + 2 crc) not adding + 13 signature since setting the header to mark no signature
            var outBytes = new byte[serializedMsg.Length + 12];

            // add the header to the packet
            outBytes[0] = MavlinkSettings.ProtocolMarker;
            outBytes[1] = (byte) endPos;
            outBytes[2] = 0;  // incompat_flags -> set to 0 to say that the message is not signed!!!
            outBytes[3] = 0; // compat_flags
            outBytes[4] = sequenceNum;
            outBytes[5] = systemId;
            outBytes[6] = componentId;
            outBytes[7] = (byte) (msgId & 0xFF);
            outBytes[8] = (byte)((msgId >> 8) & 0xFF);
            outBytes[9] = (byte)((msgId >> 16) & 0xFF);

            // add the message to the packet
            int i;
            for (i = 0; i < serializedMsg.Length; i++)
            {
                outBytes[i + 10] = serializedMsg[i];
            }

            // add the CRC checksum
            // Check the CRC. Does not include the starting byte but includes up through the message
            var crc1 = Mavlink_Crc.Calculate(outBytes, 1, (UInt16)(serializedMsg.Length + 9));

            if (MavlinkSettings.CrcExtra)
            {
                var possibleMsgId = outBytes[7];
                var extra = MavLinkSerializer.Lookup[possibleMsgId];
                crc1 = Mavlink_Crc.CrcAccumulate(extra.CrcExtra, crc1);
            }

            byte crc_high = (byte)(crc1 & 0xFF);
            byte crc_low = (byte)(crc1 >> 8);

            outBytes[i + 10] = crc_high;
            outBytes[i + 11] = crc_low;



            // increment the sequence number
            // this should naturally overflow and cycle back to 0 after 255 since it's a byte
            sequenceNum++;

            return outBytes;
        }


        // Send a raw message over the link - 
        // this  will add start byte, lenghth, crc and other link layer stuff
        private byte[] SendPacketLinkLayer(byte[] packetData)
        {
            /*
               * Byte order:
               * 
               * 0   Packet start sign	 
               * 1	 Payload length	 0 - 255
               * 2	 Packet sequence	 0 - 255
               * 3	 System ID	 1 - 255
               * 4	 Component ID	 0 - 255
               * 5	 Message ID	 0 - 255
               * 6 to (n+6)	 Data	 (0 - 255) bytes
               * (n+7) to (n+8)	 Checksum (high byte, low byte)
               *
               */
            var outBytes = new byte[packetData.Length + 5];

            outBytes[0] = MavlinkSettings.ProtocolMarker;
            outBytes[1] = (byte)(packetData.Length-3);  // 3 bytes for sequence, id, msg type which this 
                                                        // layer does not concern itself with
            outBytes[2] = unchecked(txPacketSequence++);

            int i;

            for ( i = 0; i < packetData.Length; i++)
            {
                outBytes[i + 3] = packetData[i];
            }

            // Check the CRC. Does not include the starting byte but does include the length
            var crc1 = Mavlink_Crc.Calculate(outBytes, 1, (UInt16)(packetData.Length + 2));

            if (MavlinkSettings.CrcExtra)
            {
                var possibleMsgId = outBytes[5];
                var extra = MavLinkSerializer.Lookup[possibleMsgId];
                crc1 = Mavlink_Crc.CrcAccumulate(extra.CrcExtra, crc1);
            }

            byte crc_high = (byte)(crc1 & 0xFF);
            byte crc_low = (byte)(crc1 >> 8);

            outBytes[i + 3] = crc_high;
            outBytes[i + 4] = crc_low;

            return outBytes;
        }


        // Process a raw packet in it's entirety in the given byte array
        // if deserialization is successful, then the packetdecoded event will be raised
        private void ProcessPacketBytes(byte[] packetBytes, byte rxPacketSequence)
        {
            //	 System ID	 1 - 255
            //	 Component ID	 0 - 255
            //	 Message ID	 0 - 255
            //   6 to (n+6)	 Data	 (0 - 255) bytes
            var packet = new MavlinkPacket
            {
                SystemId = packetBytes[0],
                ComponentId = packetBytes[1],
                SequenceNumber = rxPacketSequence,
                Message = this.Deserialize(packetBytes, 2)
            };

            if (PacketReceived != null)
            {
                PacketReceived(this, packet);
            }

            // else do what?
        }


        public MavlinkMessage Deserialize(byte[] bytes, int offset)
        {
            // TODO: Use the 2nd and 3rd bytes to get the msg id. Currently 
            // assumes the msg id the first byte, which is ok for now because 
            // most messages don't use the other 2 bytes.
            var packetNum = (int)bytes[offset + 0];
            var packetGen = MavLinkSerializer.Lookup[packetNum].Deserializer;
            // we add 3 to the offset because the message id takes up 3 bytes
            return packetGen.Invoke(bytes, offset + 3);
        }

        public byte[] Serialize(MavlinkMessage message, int systemId, int componentId)
        {
            var buff = new byte[256];

            buff[0] = (byte)systemId;
            buff[1] = (byte)componentId;

            var endPos = 3;

            var msgId = message.Serialize(buff, ref endPos);

            buff[2] = (byte)msgId;

            var resultBytes = new byte[endPos];
            Array.Copy(buff, resultBytes, endPos);

            return resultBytes;
        }
    }


   ///<summary>
   /// Describes an occurance when a packet fails CRC
   ///</summary>
   public class PacketCRCFailEventArgs : EventArgs
   {
       ///<summary>
       ///</summary>
       public PacketCRCFailEventArgs(byte[] badPacket, int offset)
       {
           BadPacket = badPacket;
           Offset = offset;
       }

       /// <summary>
       /// The bytes that filed the CRC, including the starting character
       /// </summary>
       public byte[] BadPacket;

       /// <summary>
       /// The offset in bytes where the start of the block begins, e.g 
       /// 50 would mean the block of badbytes would start 50 bytes ago 
       /// in the stread. No negative sign is necessary
       /// </summary>
       public int Offset;
   }

   ///<summary>
   /// Handler for an PacketFailedCRC Event
   ///</summary>
   public delegate void PacketCRCFailEventHandler(object sender, PacketCRCFailEventArgs e);


   public delegate void PacketReceivedEventHandler(object sender, MavlinkPacket e);


    ///<summary>
    /// Represents a Mavlink message - both the message object itself
    /// and the identified sending party
    ///</summary>
    public class MavlinkPacket
    {
        /// <summary>
        /// The sender's system ID
        /// </summary>
        public int SystemId;

        /// <summary>
        /// The sender's component ID
        /// </summary>
        public int ComponentId;

        /// <summary>
        /// The sequence number received for this packet
        /// </summary>
        public byte SequenceNumber;


        /// <summary>
        /// Time of receipt
        /// </summary>
        public DateTime TimeStamp;

        /// <summary>
        /// Object which is the mavlink message
        /// </summary>
        public MavlinkMessage Message;
    }

    /// <summary>
    /// Crc code copied/adapted from ardumega planner code
    /// </summary>
    internal static class Mavlink_Crc
    {
        const UInt16 X25_INIT_CRC = 0xffff;

        public static UInt16 CrcAccumulate(byte b, UInt16 crc)
        {
            unchecked
            {
                byte ch = (byte)(b ^ (byte)(crc & 0x00ff));
                ch = (byte)(ch ^ (ch << 4));
                return (UInt16)((crc >> 8) ^ (ch << 8) ^ (ch << 3) ^ (ch >> 4));
            }
        }


        // For a "message" of length bytes contained in the byte array
        // pointed to by buffer, calculate the CRC
        public static UInt16 Calculate(byte[] buffer, UInt16 start, UInt16 length)
        {
            UInt16 crcTmp = X25_INIT_CRC;

            for (int i = start; i < start + length; i++)
                crcTmp = CrcAccumulate(buffer[i], crcTmp);

            return crcTmp;
        }
    }
}