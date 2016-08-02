using System;

namespace Cryptool.DESVisualisation
{
    /// <summary>
    /// Declaration of the DESImplementation class which executes the Data Encryption Standard
    /// algorithm for encryption and saves all the binary strings needed for DESPresentation.xaml.
    /// </summary>
    public class DESImplementation
    {
        //Constructor
        public DESImplementation(byte[] keyInput, byte[] messageInput)
        {
            Inputmessage = messageInput;
            Inputkey = keyInput;
        }

        /////////////////////////////////////////////////////////////
        // Attributes

        public byte[] Inputmessage;
        public byte[] Inputkey;
        public byte[] Outputciphertext;

        //Strings for DESPresentation.xaml
        public String[,] KeySchedule = new String[17, 2];           // Cn, Dn (C0D0 = PC1Result)    
        public String[] RoundKeys = new String[16];                 // Kn = PC2Result

        public String[,] LR_Data = new String[17, 2];               // Ln, Rn (L0R0 = IPResult)
        public String[,] RoundDetails = new String[16, 4];          // E(Rn-1), KeyXOR, SBoxOut, FOut
        public byte[,] SBoxNumberDetails = new byte[16, 24];        // SiRow, SiColumn, SiOut (i = 1 to 8)
        public String[,] SBoxStringDetails = new String[16, 32];    // SiIn, SiRow, SiColumn, SiOut (i = 1 to 8)

        public String ciphertext;                                   // FPResult = R16L16 
        public String message;
        public String key;

        /////////////////////////////////////////////////////////////
        // Constants

        public const int KEY_BYTE_LENGTH = 8;

        public const int BITS_PER_BYTE = 8;

        /////////////////////////////////////////////////////////////
        #region Nested classes

        /// <summary>
        /// Declaration of BLOCK8BYTE class
        /// </summary>
        internal class BLOCK8BYTE
        {

            /////////////////////////////////////////////////////////
            // Constants

            public const int BYTE_LENGTH = 8;

            /////////////////////////////////////////////////////////
            // Attributes

            internal byte[] m_data = new byte[BYTE_LENGTH];

            /////////////////////////////////////////////////////////
            // Operations

            public void Reset()
            {

                // Reset bytes
                Array.Clear(m_data, 0, BYTE_LENGTH);

            }

            public void Set(BLOCK8BYTE Source)
            {

                // Copy source data to this
                this.Set(Source.m_data, 0);

            }

            public void Set(byte[] buffer, int iOffset)
            {

                // Set contents by copying array
                Array.Copy(buffer, iOffset, m_data, 0, BYTE_LENGTH);

            }

            public void Xor(BLOCK8BYTE A, BLOCK8BYTE B)
            {

                // Set byte to A ^ B
                for (int iOffset = 0; iOffset < BYTE_LENGTH; iOffset++)
                    m_data[iOffset] = Convert.ToByte(A.m_data[iOffset] ^ B.m_data[iOffset]);

            }

            public void SetBit(int iByteOffset, int iBitOffset, bool bFlag)
            {

                // Compose mask
                byte mask = Convert.ToByte(1 << iBitOffset);
                if (((m_data[iByteOffset] & mask) == mask) != bFlag)
                    m_data[iByteOffset] ^= mask;

            }

            public bool GetBit(int iByteOffset, int iBitOffset)
            {

                // Call sibling function
                return ((this.m_data[iByteOffset] >> iBitOffset) & 0x01) == 0x01;

            }

            public void ShiftLeftWrapped(BLOCK8BYTE S, int iBitShift)
            {

                // This shift is only applied to the first 32 bits, and parity bit is ignored

                // Declaration of local variables
                int iByteOffset = 0;
                bool bBit = false;

                // Copy byte and shift regardless
                for (iByteOffset = 0; iByteOffset < 4; iByteOffset++)
                    m_data[iByteOffset] = Convert.ToByte((S.m_data[iByteOffset] << iBitShift) & 0xFF);

                // if shifting by 1...
                if (iBitShift == 1)
                {

                    // repair bits on right of BYTE
                    for (iByteOffset = 0; iByteOffset < 3; iByteOffset++)
                    {

                        // get repairing bit offsets
                        bBit = S.GetBit(iByteOffset + 1, 7);
                        this.SetBit(iByteOffset, 1, bBit);

                    }

                    // wrap around the final bit
                    this.SetBit(3, 1, S.GetBit(0, 7));

                }
                else if (iBitShift == 2)
                {

                    // repair bits on right of BYTE
                    for (iByteOffset = 0; iByteOffset < 3; iByteOffset++)
                    {

                        // get repairing bit offsets
                        bBit = S.GetBit(iByteOffset + 1, 7);
                        this.SetBit(iByteOffset, 2, bBit);
                        bBit = S.GetBit(iByteOffset + 1, 6);
                        this.SetBit(iByteOffset, 1, bBit);

                    }

                    // wrap around the final bit
                    this.SetBit(3, 2, S.GetBit(0, 7));
                    this.SetBit(3, 1, S.GetBit(0, 6));

                }

            }

            public String ToBinaryString(int length, int deletePos)
            {
                String tmp = "";
                for (int i = 0; i < BYTE_LENGTH; i++)
                {
                    for (int j = 0; j < BYTE_LENGTH; j++)
                    {
                        if(deletePos-1-j != 0)
                        {
                            if (GetBit(i, 7-j))
                                tmp = tmp + "1";
                            else
                                tmp = tmp + "0";        
                        }     
                    }  
                }
                tmp =tmp.Remove(length-1,tmp.Length-length);
                return tmp;
            }

            public String ToBinaryString2(int deletePos1, int deletePos2)
            {
                String tmp = "";
                for (int i = 0; i < BYTE_LENGTH; i++)
                {
                    for (int j = 0; j < BYTE_LENGTH; j++)
                    {
                        if (deletePos1 - 1 - j != 0 && deletePos2-1-j !=0)
                        {
                            if (GetBit(i, 7 - j))
                                tmp = tmp + "1";
                            else
                                tmp = tmp + "0";        
                        }
                    }  
                }
                return tmp;
            }

            public String ToBinaryString4(int deletePos1, int deletePos2, int deletePos3, int deletePos4)
            {
                String tmp = "";
                for (int i = 0; i < BYTE_LENGTH; i++)
                {
                    for (int j = 0; j < BYTE_LENGTH; j++)
                    {
                        if (deletePos1 - 1 - j != 0 && deletePos2 - 1 - j != 0 && deletePos3 - 1 - j != 0 && deletePos4 - 1 - j != 0)
                        {
                            if (GetBit(i, 7 - j))
                                tmp = tmp + "1";
                            else
                                tmp = tmp + "0";
                        }
                    }                   
                }
                return tmp;
            }

        }

        /// <summary>
        /// Declaration of KEY_SET class
        /// </summary>
        internal class KEY_SET
        {

            /////////////////////////////////////////////////////////
            // Constants

            public const int KEY_COUNT = 17;

            /////////////////////////////////////////////////////////
            // Attributes

            internal BLOCK8BYTE[] m_array;

            /////////////////////////////////////////////////////////
            // Construction

            internal KEY_SET()
            {

                // Create array
                m_array = new BLOCK8BYTE[KEY_COUNT];
                for (int i1 = 0; i1 < KEY_COUNT; i1++)
                    m_array[i1] = new BLOCK8BYTE();

            }

            /////////////////////////////////////////////////////////
            // Operations

            public BLOCK8BYTE GetAt(int iArrayOffset)
            {
                return m_array[iArrayOffset];
            }

        }

        /// <summary>
        /// Declaration of WORKING_SET class
        /// </summary>
        internal class WORKING_SET
        {

            /////////////////////////////////////////////////////////
            // Attributes

            internal BLOCK8BYTE IP = new BLOCK8BYTE();
            internal BLOCK8BYTE[] Ln = new BLOCK8BYTE[17];
            internal BLOCK8BYTE[] Rn = new BLOCK8BYTE[17];
            internal BLOCK8BYTE RnExpand = new BLOCK8BYTE();
            internal BLOCK8BYTE XorBlock = new BLOCK8BYTE();
            internal BLOCK8BYTE SBoxValues = new BLOCK8BYTE();
            internal BLOCK8BYTE f = new BLOCK8BYTE();
            internal BLOCK8BYTE X = new BLOCK8BYTE();

            internal BLOCK8BYTE DataBlockIn = new BLOCK8BYTE();
            internal BLOCK8BYTE DataBlockOut = new BLOCK8BYTE();
            internal BLOCK8BYTE DecryptXorBlock = new BLOCK8BYTE();

            /////////////////////////////////////////////////////////
            // Construction

            internal WORKING_SET()
            {

                // Build the arrays
                for (int i1 = 0; i1 < 17; i1++)
                {
                    Ln[i1] = new BLOCK8BYTE();
                    Rn[i1] = new BLOCK8BYTE();
                }
            }

        }

        #endregion Nested classes

        /////////////////////////////////////////////////////////////
        #region DES Tables

        /* PERMUTED CHOICE 1 (PCl) */
        private static byte[] bytePC1 = {
            57, 49, 41, 33, 25, 17,  9,
            1,  58, 50, 42, 34, 26, 18,
            10,  2, 59, 51, 43, 35, 27,
            19, 11,  3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
            7,  62, 54, 46, 38, 30, 22,
            14,  6, 61, 53, 45, 37, 29,
            21, 13,  5, 28, 20, 12,  4,
        };

        /* PERMUTED CHOICE 2 (PC2) */
        private static byte[] bytePC2 = {
            14, 17, 11, 24,  1,  5,
            3,  28, 15,  6, 21, 10,
            23, 19, 12,  4, 26,  8,
            16,  7, 27, 20, 13,  2,
            41, 52, 31, 37, 47, 55,
            30, 40, 51, 45, 33, 48,
            44, 49, 39, 56, 34, 53,
            46, 42, 50, 36, 29, 32,
        };

        /* INITIAL PERMUTATION (IP) */
        private static byte[] byteIP =  {
            58, 50, 42, 34, 26, 18, 10,  2,
            60, 52, 44, 36, 28, 20, 12,  4,
            62, 54, 46, 38, 30, 22, 14,  6,
            64, 56, 48, 40, 32, 24, 16,  8,
            57, 49, 41, 33, 25, 17,  9,  1,
            59, 51, 43, 35, 27, 19, 11,  3,
            61, 53, 45, 37, 29, 21, 13,  5,
            63, 55, 47, 39, 31, 23, 15,  7
        };

        /* REVERSE FINAL PERMUTATION (IP-1) */
        private static byte[] byteRFP = {
            40,  8,   48,    16,    56,   24,    64,   32,
            39,  7,   47,    15,    55,   23,    63,   31,
            38,  6,   46,    14,    54,   22,    62,   30,
            37,  5,   45,    13,    53,   21,    61,   29,
            36,  4,   44,    12,    52,   20,    60,   28,
            35,  3,   43,    11,    51,   19,    59,   27,
            34,  2,   42,    10,    50,   18,    58,   26,
            33,  1,   41,     9,    49,   17,    57,   25,
        };

        /* E BIT-SELECTION TABLE */
        private static byte[] byteE = {
            32,  1,  2,  3,  4,  5,
            4,   5,  6,  7,  8,  9,
            8,   9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32,  1
        };

        /* PERMUTATION FUNCTION P */
        private static byte[] byteP = {
            16,  7, 20, 21,
            29, 12, 28, 17,
            1,  15, 23, 26,
            5,  18, 31, 10,
            2,   8, 24, 14,
            32, 27,  3,  9,
            19, 13, 30,  6,
            22, 11,  4, 25
        };

        // Schedule of left shifts for C and D blocks
        public static byte[] byteShifts = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };

        // S-Boxes
        private static byte[,] byteSBox = new byte[,] {
            {14,  4, 13,  1,     2, 15, 11,  8, 3, 10,  6, 12,   5,  9,  0,  7},
            { 0, 15,  7,  4,    14,  2, 13,  1, 10,  6, 12, 11,  9,  5,  3,  8},
            { 4,  1, 14,  8,    13,  6,  2, 11, 15, 12,  9,  7,  3, 10,  5,  0},
            {15, 12,  8,  2, 4,  9,  1,  7,   5, 11,  3, 14,    10,  0,  6, 13},

            {15, 1,  8, 14,  6, 11,  3,  4,  9, 7,   2, 13, 12,  0,  5, 10},
            {3, 13,  4,  7, 15,  2,  8, 14, 12, 0,   1, 10,  6,  9, 11,  5},
            {0, 14,  7, 11, 10,  4, 13,  1, 5, 8,   12,  6,  9,  3,  2, 15},
            {13, 8, 10,  1,  3, 15,  4,  2, 11, 6,   7, 12,  0,  5, 14,  9},

            {10,     0,  9, 14,  6,  3, 15,  5,  1, 13, 12,  7, 11,  4,  2,  8},
            {13,     7,  0,  9,  3,  4,  6, 10,  2,  8,  5, 14, 12, 11, 15,  1},
            {13,     6,  4,  9,  8, 15,  3,  0, 11,  1,  2, 12,  5, 10, 14,  7},
            {1, 10, 13,  0,  6,  9,  8,  7,  4, 15, 14,  3, 11,  5,  2, 12},

            {7, 13, 14,  3,  0,  6,  9, 10,  1,  2,  8,  5, 11, 12,  4, 15},
            {13,     8, 11,  5,  6, 15,  0,  3,  4,  7,  2, 12,  1, 10, 14,  9},
            {10,     6,  9,  0, 12, 11,  7, 13, 15,  1,  3, 14,  5,  2,  8,  4},
            {3, 15,  0,  6, 10,  1, 13,  8, 9,   4,  5, 11, 12,  7,  2, 14},

            {2, 12,  4,  1,  7, 10, 11, 6,  8,   5,  3, 15, 13,  0, 14,  9},
            {14,    11,  2, 12,  4,  7, 13,  1, 5,   0, 15, 10,  3,  9,  8,  6},
            {4,  2,  1, 11, 10, 13,  7,  8,15,   9, 12,  5,  6,  3,  0, 14},
            {11,     8, 12,  7,  1, 14,  2, 13, 6,  15,  0,  9, 10,  4,  5,  3},

            {12,     1, 10, 15,  9,  2,  6,  8,0,   13,  3,  4, 14,  7,  5, 11},
            {10,    15,  4,  2,  7, 12,  9,  5,6,    1, 13, 14,  0, 11,  3,  8},
            {9, 14, 15,  5,  2,  8, 12,  3,7,    0,  4, 10,  1, 13, 11,  6},
            {4,  3,  2, 12,  9,  5, 15, 10,11,  14,  1,  7,  6,  0,  8, 13},

            {4, 11,  2, 14, 15,  0,  8, 13, 3,  12,  9,  7,  5, 10,  6,  1},
            {13,     0, 11,  7,  4,  9,  1, 10,14,   3,  5, 12,  2, 15,  8,  6},
            {1,  4, 11, 13, 12,  3,  7, 14,10,  15,  6,  8,  0,  5,  9,  2},
            {6, 11, 13,  8,  1,  4, 10,  7,9,    5,  0, 15, 14,  2,  3, 12},

            {13,     2,  8,  4,  6, 15, 11,  1,10,   9,  3, 14,  5,  0, 12,  7},
            {1, 15, 13,  8, 10,  3,  7,  4, 12,  5,  6, 11,  0, 14,  9,  2},
            {7, 11,  4,  1,  9, 12, 14,  2,0,    6, 10, 13, 15,  3,  5,  8},
            {2,  1, 14,  7,  4, 10,  8, 13,15,  12,  9,  0,  3,  5,  6, 11}
        };

        #endregion DES Tables

        /////////////////////////////////////////////////////////////
        #region Operations - DES

        private bool IsValidDESInput(byte[] Input)
        {
            if (Input == null)
                return false;
            if (Input.Length != KEY_BYTE_LENGTH)
                return false;

            // Return success
            return true;
        }

        public void DES()
        {

            // Shortcuts
            if (!IsValidDESInput(this.Inputkey))
                throw new Exception("ERROR: Invalid DES key.");
            if (!IsValidDESInput(this.Inputmessage))
                throw new Exception("ERROR: Invalid DES message.");

            // Expand the keys into Kn
            KEY_SET[] Kn = new KEY_SET[1] {
                _expandKey(this.Inputkey, 0)
            };

            // Apply DES keys
            _desAlgorithm(this.Inputmessage, Kn);

        }

        #endregion Operations - DES

        /////////////////////////////////////////////////////////////
        #region Low-level Operations

        private KEY_SET _expandKey(byte[] Key, int iOffset)
        {

            //
            // Expand an 8 byte DES key into a set of permuted round keys
            //

            // Declare return variable
            KEY_SET Ftmp = new KEY_SET();

            // Declaration of local variables
            int iTableOffset, iArrayOffset, iPermOffset, iByteOffset, iBitOffset;
            bool bBit;

            // Put key into an 8-bit block
            BLOCK8BYTE K = new BLOCK8BYTE();
            K.Set(Key, iOffset);

            //Fill String Attribute
            key = K.ToBinaryString(64,0);

            // Permutate Kp with PC1
            BLOCK8BYTE Kp = new BLOCK8BYTE();
            for (iArrayOffset = 0; iArrayOffset < bytePC1.Length; iArrayOffset++)
            {

                // Get permute offset
                iPermOffset = bytePC1[iArrayOffset];
                iPermOffset--;

                // Get and set bit
                Kp.SetBit(
                    _bitAddressToByteOffset(iArrayOffset, 7),
                    _bitAddressToBitOffset(iArrayOffset, 7),
                    K.GetBit(
                        _bitAddressToByteOffset(iPermOffset, 8),
                        _bitAddressToBitOffset(iPermOffset, 8)
                    )
                );

            }


            // Create 17 blocks of C and D from Kp
            BLOCK8BYTE[] KpCn = new BLOCK8BYTE[17];
            BLOCK8BYTE[] KpDn = new BLOCK8BYTE[17];
            for (iArrayOffset = 0; iArrayOffset < 17; iArrayOffset++)
            {
                KpCn[iArrayOffset] = new BLOCK8BYTE();
                KpDn[iArrayOffset] = new BLOCK8BYTE();
            }
            for (iArrayOffset = 0; iArrayOffset < 32; iArrayOffset++)
            {

                // Set bit in KpCn
                iByteOffset = _bitAddressToByteOffset(iArrayOffset, 8);
                iBitOffset = _bitAddressToBitOffset(iArrayOffset, 8);
                bBit = Kp.GetBit(iByteOffset, iBitOffset);
                KpCn[0].SetBit(iByteOffset, iBitOffset, bBit);

                // Set bit in KpDn
                bBit = Kp.GetBit(iByteOffset + 4, iBitOffset);
                KpDn[0].SetBit(iByteOffset, iBitOffset, bBit);

            }

            for (iArrayOffset = 1; iArrayOffset < 17; iArrayOffset++)
            {

                // Shift left wrapped
                KpCn[iArrayOffset].ShiftLeftWrapped(KpCn[iArrayOffset - 1], byteShifts[iArrayOffset - 1]);
                KpDn[iArrayOffset].ShiftLeftWrapped(KpDn[iArrayOffset - 1], byteShifts[iArrayOffset - 1]);

            }

            // Cn und Dn in Key Schedule füllen
            for (iArrayOffset = 0; iArrayOffset < 17; iArrayOffset++)
            {
                KeySchedule[iArrayOffset, 0] = KpCn[iArrayOffset].ToBinaryString(29,8).Remove(28, 1);
                KeySchedule[iArrayOffset, 1] = KpDn[iArrayOffset].ToBinaryString(29,8).Remove(28, 1);
            }

            
            // Create 17 keys Kn
            for (iArrayOffset = 0; iArrayOffset < 17; iArrayOffset++)
            {

                // Loop through the bits
                for (iTableOffset = 0; iTableOffset < 48; iTableOffset++)
                {

                    // Get address if bit
                    iPermOffset = bytePC2[iTableOffset];
                    iPermOffset--;

                    // Convert to byte and bit offsets
                    iByteOffset = _bitAddressToByteOffset(iPermOffset, 7);
                    iBitOffset = _bitAddressToBitOffset(iPermOffset, 7);

                    // Get bit
                    if (iByteOffset < 4)
                        bBit = KpCn[iArrayOffset].GetBit(iByteOffset, iBitOffset);
                    else
                        bBit = KpDn[iArrayOffset].GetBit(iByteOffset - 4, iBitOffset);

                    // Set bit
                    iByteOffset = _bitAddressToByteOffset(iTableOffset, 6);
                    iBitOffset = _bitAddressToBitOffset(iTableOffset, 6);
                    Ftmp.GetAt(iArrayOffset).SetBit(iByteOffset, iBitOffset, bBit);
                }

            }

            // Kn Rundenschlüssel in Key Schedule füllen
            for (iArrayOffset = 0; iArrayOffset < 16; iArrayOffset++)
            {
                RoundKeys[iArrayOffset] = Ftmp.GetAt(iArrayOffset+1).ToBinaryString2(7,8);

            }

            // Return variable
            return Ftmp;

        }

        private void _desAlgorithm(byte[] Message, KEY_SET[] KeySets)
        {

            //
            // Apply the DES algorithm to Message
            //

            // Declare a workset set of variables
            WORKING_SET workingSet = new WORKING_SET();
            BLOCK8BYTE msg =new BLOCK8BYTE();
            msg.Set(Message,0);
            workingSet.DataBlockIn.Set(msg);
            message = msg.ToBinaryString(64, 0);
            
            // Apply the algorithm
            _lowLevel_desAlgorithm(workingSet, KeySets);

        }

        private void _lowLevel_desAlgorithm(WORKING_SET workingSet, KEY_SET[] KeySets)
        {

            //
            // Apply 1 or 3 keys to a block of data
            //

            // Declaration of local variables
            int iTableOffset;
            int iArrayOffset;
            int iPermOffset;
            int iByteOffset;
            int iBitOffset;

            // Loop through keys
            for (int iKeySetOffset = 0; iKeySetOffset < KeySets.Length; iKeySetOffset++)
            {

                // Permute with byteIP
                for (iTableOffset = 0; iTableOffset < byteIP.Length; iTableOffset++)
                {

                    // Get perm offset
                    iPermOffset = byteIP[iTableOffset];
                    iPermOffset--;

                    // Get and set bit
                    workingSet.IP.SetBit(
                        _bitAddressToByteOffset(iTableOffset, 8),
                        _bitAddressToBitOffset(iTableOffset, 8),
                        workingSet.DataBlockIn.GetBit(
                            _bitAddressToByteOffset(iPermOffset, 8),
                            _bitAddressToBitOffset(iPermOffset, 8)
                        )
                    );

                }

                // Create Ln[0] and Rn[0]
                for (iArrayOffset = 0; iArrayOffset < 32; iArrayOffset++)
                {
                    iByteOffset = _bitAddressToByteOffset(iArrayOffset, 8);
                    iBitOffset = _bitAddressToBitOffset(iArrayOffset, 8);
                    workingSet.Ln[0].SetBit(iByteOffset, iBitOffset, workingSet.IP.GetBit(iByteOffset, iBitOffset));
                    workingSet.Rn[0].SetBit(iByteOffset, iBitOffset, workingSet.IP.GetBit(iByteOffset + 4, iBitOffset));
                }

                // Loop through 17 interations
                for (int iBlockOffset = 1; iBlockOffset < 17; iBlockOffset++)
                {

                    // Get the array offset
                    int iKeyOffset;
                    if (true != (iKeySetOffset == 1))
                        iKeyOffset = iBlockOffset;
                    else
                        iKeyOffset = 17 - iBlockOffset;

                    // Set Ln[N] = Rn[N-1]
                    workingSet.Ln[iBlockOffset].Set(workingSet.Rn[iBlockOffset - 1]);

                    // Set Rn[N] = Ln[0] + f(R[N-1],K[N])
                    for (iTableOffset = 0; iTableOffset < byteE.Length; iTableOffset++)
                    {

                        // Get perm offset
                        iPermOffset = byteE[iTableOffset];
                        iPermOffset--;

                        // Get and set bit
                        workingSet.RnExpand.SetBit(
                            _bitAddressToByteOffset(iTableOffset, 6),
                            _bitAddressToBitOffset(iTableOffset, 6),
                            workingSet.Rn[iBlockOffset - 1].GetBit(
                                _bitAddressToByteOffset(iPermOffset, 8),
                                _bitAddressToBitOffset(iPermOffset, 8)
                            )
                        );

                    }

                    //Fill String Attribute
                    RoundDetails[iBlockOffset - 1, 0] = workingSet.RnExpand.ToBinaryString2(7, 8);

                    // XOR expanded block with K-block
                    workingSet.XorBlock.Xor(workingSet.RnExpand, KeySets[iKeySetOffset].GetAt(iKeyOffset));

                    //Fill String Attribute
                    RoundDetails[iBlockOffset - 1, 1] = workingSet.XorBlock.ToBinaryString2(7, 8);

                    // Set S-Box values
                    workingSet.SBoxValues.Reset();
                    for (iTableOffset = 0; iTableOffset < 8; iTableOffset++)
                    {
                        
                        //Fill String Attribute
                        String tmp = RoundDetails[iBlockOffset - 1, 1];
                        SBoxStringDetails[iBlockOffset-1, iTableOffset * 4] = Convert.ToString(workingSet.XorBlock.m_data[iTableOffset], 2).PadLeft(8, '0').Remove(6,2);

                        // Calculate m and n
                        int m = ((workingSet.XorBlock.GetBit(iTableOffset, 7) ? 1 : 0) << 1) | (workingSet.XorBlock.GetBit(iTableOffset, 2) ? 1 : 0);
                        int n = (workingSet.XorBlock.m_data[iTableOffset] >> 3) & 0x0F;

                        // Get s-box value
                        iPermOffset = byteSBox[(iTableOffset * 4) + m, n];
                        workingSet.SBoxValues.m_data[iTableOffset] = (byte)(iPermOffset << 4);

                        //Fill String Attributes
                        SBoxNumberDetails[iBlockOffset-1, iTableOffset * 3] = (byte) (m);
                        SBoxStringDetails[iBlockOffset-1, (iTableOffset * 4) + 1] = Convert.ToString(m,2).PadLeft(2,'0');
                        SBoxNumberDetails[iBlockOffset-1, (iTableOffset * 3) + 1] = (byte) (n);
                        SBoxStringDetails[iBlockOffset-1, (iTableOffset * 4) + 2] = Convert.ToString(n, 2).PadLeft(4, '0');
                        SBoxNumberDetails[iBlockOffset-1, (iTableOffset * 3) + 2] = (byte)(workingSet.SBoxValues.m_data[iTableOffset] >> 4);
                        SBoxStringDetails[iBlockOffset-1, (iTableOffset * 4) + 3] = Convert.ToString((byte)(workingSet.SBoxValues.m_data[iTableOffset] >> 4), 2).PadLeft(4, '0');

                    }

                    //Fill String Attributes
                    RoundDetails[iBlockOffset - 1, 2] = workingSet.SBoxValues.ToBinaryString4(5, 6, 7, 8);

                    // Permute with P -> f
                    workingSet.f.Reset();
                    for (iTableOffset = 0; iTableOffset < byteP.Length; iTableOffset++)
                    {

                        // Get perm offset
                        iPermOffset = byteP[iTableOffset];
                        iPermOffset--;

                        // Get and set bit
                        workingSet.f.SetBit(
                            _bitAddressToByteOffset(iTableOffset, 4),
                            _bitAddressToBitOffset(iTableOffset, 4),
                            workingSet.SBoxValues.GetBit(
                                _bitAddressToByteOffset(iPermOffset, 4),
                                _bitAddressToBitOffset(iPermOffset, 4)
                            )
                        );

                    }

                    //Fill String Attributes
                    RoundDetails[iBlockOffset - 1, 3] = workingSet.f.ToBinaryString4(5,6,7, 8);

                    // Rn[N] = Ln[N-1] ^ f
                    workingSet.Rn[iBlockOffset].Reset();
                    for (iTableOffset = 0; iTableOffset < 8; iTableOffset++)
                    {

                        // Get Ln[N-1] -> A
                        byte A = workingSet.Ln[iBlockOffset - 1].m_data[(iTableOffset >> 1)];
                        if ((iTableOffset % 2) == 0)
                            A >>= 4;
                        else
                            A &= 0x0F;

                        // Get f -> B
                        byte B = Convert.ToByte(workingSet.f.m_data[iTableOffset] >> 4);

                        // Update Rn[N]
                        if ((iTableOffset % 2) == 0)
                            workingSet.Rn[iBlockOffset].m_data[iTableOffset >> 1] |= Convert.ToByte((A ^ B) << 4);
                        else
                            workingSet.Rn[iBlockOffset].m_data[iTableOffset >> 1] |= Convert.ToByte(A ^ B);

                    }

                }

                // X = R16 L16
                workingSet.X.Reset();
                for (iTableOffset = 0; iTableOffset < 4; iTableOffset++)
                {
                    workingSet.X.m_data[iTableOffset] = workingSet.Rn[16].m_data[iTableOffset];
                    workingSet.X.m_data[iTableOffset + 4] = workingSet.Ln[16].m_data[iTableOffset];
                }

                // C = X perm IP
                workingSet.DataBlockOut.Reset();
                for (iTableOffset = 0; iTableOffset < byteRFP.Length; iTableOffset++)
                {

                    // Get perm offset
                    iPermOffset = byteRFP[iTableOffset];
                    iPermOffset--;

                    // Get and set bit
                    workingSet.DataBlockOut.SetBit(
                        _bitAddressToByteOffset(iTableOffset, 8),
                        _bitAddressToBitOffset(iTableOffset, 8),
                        workingSet.X.GetBit(
                            _bitAddressToByteOffset(iPermOffset, 8),
                            _bitAddressToBitOffset(iPermOffset, 8)
                        )
                    );

                }
                Outputciphertext = workingSet.DataBlockOut.m_data;

                //Fill String Attribute
                ciphertext = workingSet.DataBlockOut.ToBinaryString(64,0);

            }

            //Fill String Attributes
            for (int i = 0; i < 17; i++) {
                LR_Data[i, 0] = workingSet.Ln[i].ToBinaryString(32, 0);
                LR_Data[i, 1] = workingSet.Rn[i].ToBinaryString(32, 0);
            }
        }

        #endregion Low-level Operations

        /////////////////////////////////////////////////////////////
        // Helper Operations

        private int _bitAddressToByteOffset(int iTableAddress, int iTableWidth)
        {
            int iFtmp = iTableAddress / iTableWidth;
            return iFtmp;
        }

        private int _bitAddressToBitOffset(int iTableAddress, int iTableWidth)
        {
            int iFtmp = BITS_PER_BYTE - 1 - (iTableAddress % iTableWidth);
            return iFtmp;
        }

    }
}

