﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace BytexDigital.BattlEye.Rcon.HashAlgorithms
{
    public sealed class Crc32 : HashAlgorithm
    {
        public const uint DEFAULT_POLYNOMIAL = 0xedb88320u;
        public const uint DEFAULT_SEED = 0xffffffffu;

        // Make it nullable or default
        private static uint[]? _defaultTable = null;

        private readonly uint _seed;
        private readonly uint[] _table;
        private uint _hash;

        public override int HashSize => 32;

        public Crc32() : this(DEFAULT_POLYNOMIAL, DEFAULT_SEED)
        {
        }

        public Crc32(uint polynomial, uint seed)
        {
            if (!BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("Not supported on Big Endian processors");

            _table = InitializeTable(polynomial);
            _seed = _hash = seed;
        }

        public override void Initialize()
        {
            _hash = _seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public static uint Compute(byte[] buffer)
        {
            return Compute(DEFAULT_SEED, buffer);
        }

        public static uint Compute(uint seed, byte[] buffer)
        {
            return Compute(DEFAULT_POLYNOMIAL, seed, buffer);
        }

        public static uint Compute(uint polynomial, uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DEFAULT_POLYNOMIAL && _defaultTable != null) 
                return _defaultTable;

            var createTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint)i;
                for (var j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                }
                createTable[i] = entry;
            }

            if (polynomial == DEFAULT_POLYNOMIAL)
                _defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, IList<byte> buffer, int start, int size)
        {
            var hash = seed;
            for (var i = start; i < start + size; i++)
                hash = (hash >> 8) ^ table[buffer[i] ^ (hash & 0xff)];

            return hash;
        }

        private static byte[] UInt32ToBigEndianBytes(uint uint32)
        {
            var result = BitConverter.GetBytes(uint32);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }
    }
}
