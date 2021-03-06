﻿/*
    Copyright (C) 2018 de4dot@gmail.com

    This file is part of Iced.

    Iced is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Iced is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with Iced.  If not, see <https://www.gnu.org/licenses/>.
*/

#if !NO_ENCODER
using System;
using System.Diagnostics;

namespace Iced.Intel.EncoderInternal {
	enum OpCodeTable {
		Normal					= 0,
		Table0F					= 1,
		Table0F38				= 2,
		Table0F3A				= 3,
		// Update the flags (eg. LegacyFlags) if more values are added
	}

	enum Encodable {
		Unknown,// Don't use it, it's to catch bugs in the Code table
		Any,
		Only1632,
		Only64,
	}

	[Flags]
	enum EncFlags1 : uint {
		CodeMask			= (1U << Instruction.TEST_CodeBits) - 1,
		OpCodeShift			= 16,
		EncodingShift		= 13,
		EncodingMask		= 3,
		Legacy				= EncodingKind.Legacy << (int)EncodingShift,
		VEX					= EncodingKind.VEX << (int)EncodingShift,
		EVEX				= EncodingKind.EVEX << (int)EncodingShift,
	}

	[Flags]
	enum LegacyFlags3 : uint {
		OpMask				= 0xFF,
		Op1Shift			= 8,
		Op2Shift			= 16,
	}

	[Flags]
	enum VexFlags3 : uint {
		OpMask				= 0x3F,
		Op1Shift			= 6,
		Op2Shift			= 12,
		Op3Shift			= 18,
	}

	[Flags]
	enum EvexFlags3 : uint {
		OpMask				= 0x3F,
		Op1Shift			= 6,
		Op2Shift			= 12,
		Op3Shift			= 18,
	}

	[Flags]
	enum LegacyFlags : uint {
		None							= 0,

		MandatoryPrefixMask				= 3,
		MandatoryPrefixShift			= 0,
		P66								= MandatoryPrefix.P66 << (int)MandatoryPrefixShift,
		PF3								= MandatoryPrefix.PF3 << (int)MandatoryPrefixShift,
		PF2								= MandatoryPrefix.PF2 << (int)MandatoryPrefixShift,

		OpCodeTableMask					= 3,
		OpCodeTableShift				= 2,
		Table0F							= OpCodeTable.Table0F << (int)OpCodeTableShift,
		Table0F38						= OpCodeTable.Table0F38 << (int)OpCodeTableShift,
		Table0F3A						= OpCodeTable.Table0F3A << (int)OpCodeTableShift,

		EncodableMask					= 3,
		EncodableShift					= 4,
		Encodable_Any					= Encodable.Any << (int)EncodableShift,
		Encodable_Only1632				= Encodable.Only1632 << (int)EncodableShift,
		Encodable_Only64				= Encodable.Only64 << (int)EncodableShift,

		HasGroupIndex					= 1 << 6,
		GroupShift						= 7,
		Group0							= HasGroupIndex | (0 << (int)GroupShift),
		Group1							= HasGroupIndex | (1 << (int)GroupShift),
		Group2							= HasGroupIndex | (2 << (int)GroupShift),
		Group3							= HasGroupIndex | (3 << (int)GroupShift),
		Group4							= HasGroupIndex | (4 << (int)GroupShift),
		Group5							= HasGroupIndex | (5 << (int)GroupShift),
		Group6							= HasGroupIndex | (6 << (int)GroupShift),
		Group7							= HasGroupIndex | (7 << (int)GroupShift),

		Legacy_OpSizeShift				= 26,
		Legacy_OperandSizeMask			= 3,
		Legacy_OpSize16					= OperandSize.Size16 << (int)Legacy_OpSizeShift,
		Legacy_OpSize32					= OperandSize.Size32 << (int)Legacy_OpSizeShift,
		Legacy_OpSize64					= OperandSize.Size64 << (int)Legacy_OpSizeShift,
		Legacy_AddrSizeShift			= 28,
		Legacy_AddressSizeMask			= 3,
		Legacy_AddrSize16				= AddressSize.Size16 << (int)Legacy_AddrSizeShift,
		Legacy_AddrSize32				= AddressSize.Size32 << (int)Legacy_AddrSizeShift,
		Legacy_AddrSize64				= AddressSize.Size64 << (int)Legacy_AddrSizeShift,
		Legacy_REX						= 0x40000000,
		Legacy_REX_b					= 0x80000000,
		a16 = Legacy_AddrSize16,
		a16_o16 = a16 | o16,
		a16_o32 = a16 | o32,
		a32 = Legacy_AddrSize32,
		a32_o16 = a32 | o16,
		a32_o32 = a32 | o32,
		a64 = Legacy_AddrSize64,
		o16 = Legacy_OpSize16,
		o16_rexb = o16 | rexb,
		o32 = Legacy_OpSize32,
		o32_rexb = o32 | rexb,
		rex = Legacy_REX,
		rexb = Legacy_REX_b,
		rexw = Legacy_OpSize64,
		rexw_rexb = rexw | rexb,
	}

	[Flags]
	enum VexFlags : uint {
		None							= 0,

		MandatoryPrefixMask				= 3,
		MandatoryPrefixShift			= 0,
		P66								= MandatoryPrefix.P66 << (int)MandatoryPrefixShift,
		PF3								= MandatoryPrefix.PF3 << (int)MandatoryPrefixShift,
		PF2								= MandatoryPrefix.PF2 << (int)MandatoryPrefixShift,

		OpCodeTableMask					= 3,
		OpCodeTableShift				= 2,
		Table0F							= OpCodeTable.Table0F << (int)OpCodeTableShift,
		Table0F38						= OpCodeTable.Table0F38 << (int)OpCodeTableShift,
		Table0F3A						= OpCodeTable.Table0F3A << (int)OpCodeTableShift,

		EncodableMask					= 3,
		EncodableShift					= 4,
		Encodable_Any					= Encodable.Any << (int)EncodableShift,
		Encodable_Only1632				= Encodable.Only1632 << (int)EncodableShift,
		Encodable_Only64				= Encodable.Only64 << (int)EncodableShift,

		HasGroupIndex					= 1 << 6,
		GroupShift						= 7,
		Group0							= HasGroupIndex | (0 << (int)GroupShift),
		Group1							= HasGroupIndex | (1 << (int)GroupShift),
		Group2							= HasGroupIndex | (2 << (int)GroupShift),
		Group3							= HasGroupIndex | (3 << (int)GroupShift),
		Group4							= HasGroupIndex | (4 << (int)GroupShift),
		Group5							= HasGroupIndex | (5 << (int)GroupShift),
		Group6							= HasGroupIndex | (6 << (int)GroupShift),
		Group7							= HasGroupIndex | (7 << (int)GroupShift),

		VEX_DDS							= 0,
		VEX_NDS							= 0,
		VEX_NDD							= 0,

		VEX_LShift						= 30,
		VEX_L128						= 0,
		VEX_L256						= 0x40000000,
		VEX_L0							= VEX_L128,
		VEX_L1							= VEX_L256,
		VEX_LIG							= VEX_L128,

		VEX_W0							= 0,
		VEX_W1							= 0x80000000,
		VEX_WIG							= VEX_W0,

		VEX_128_W0 = VEX_L128 | VEX_W0,
		VEX_128_W1 = VEX_L128 | VEX_W1,
		VEX_128_WIG = VEX_L128 | VEX_WIG,
		VEX_256_W0 = VEX_L256 | VEX_W0,
		VEX_256_W1 = VEX_L256 | VEX_W1,
		VEX_256_WIG = VEX_L256 | VEX_WIG,
		VEX_DDS_128_W0 = VEX_DDS | VEX_L128 | VEX_W0,
		VEX_DDS_128_W1 = VEX_DDS | VEX_L128 | VEX_W1,
		VEX_DDS_256_W0 = VEX_DDS | VEX_L256 | VEX_W0,
		VEX_DDS_256_W1 = VEX_DDS | VEX_L256 | VEX_W1,
		VEX_DDS_LIG_W0 = VEX_DDS | VEX_LIG | VEX_W0,
		VEX_DDS_LIG_W1 = VEX_DDS | VEX_LIG | VEX_W1,
		VEX_L0_W0 = VEX_L0 | VEX_W0,
		VEX_L0_W1 = VEX_L0 | VEX_W1,
		VEX_L0_WIG = VEX_L0 | VEX_WIG,
		VEX_LIG_W0 = VEX_LIG | VEX_W0,
		VEX_LIG_W1 = VEX_LIG | VEX_W1,
		VEX_LIG_WIG = VEX_LIG | VEX_WIG,
		VEX_NDD_128_WIG = VEX_NDD | VEX_L128 | VEX_WIG,
		VEX_NDD_256_WIG = VEX_NDD | VEX_L256 | VEX_WIG,
		VEX_NDD_L0_W0 = VEX_NDD | VEX_L0 | VEX_W0,
		VEX_NDD_L0_W1 = VEX_NDD | VEX_L0 | VEX_W1,
		VEX_NDS_128_W0 = VEX_NDS | VEX_L128 | VEX_W0,
		VEX_NDS_128_W1 = VEX_NDS | VEX_L128 | VEX_W1,
		VEX_NDS_128_WIG = VEX_NDS | VEX_L128 | VEX_WIG,
		VEX_NDS_256_W0 = VEX_NDS | VEX_L256 | VEX_W0,
		VEX_NDS_256_W1 = VEX_NDS | VEX_L256 | VEX_W1,
		VEX_NDS_256_WIG = VEX_NDS | VEX_L256 | VEX_WIG,
		VEX_NDS_L0_W0 = VEX_NDS | VEX_L0 | VEX_W0,
		VEX_NDS_L0_W1 = VEX_NDS | VEX_L0 | VEX_W1,
		VEX_NDS_L1_W0 = VEX_NDS | VEX_L1 | VEX_W0,
		VEX_NDS_L1_W1 = VEX_NDS | VEX_L1 | VEX_W1,
		VEX_NDS_LIG_W0 = VEX_NDS | VEX_LIG | VEX_W0,
		VEX_NDS_LIG_W1 = VEX_NDS | VEX_LIG | VEX_W1,
		VEX_NDS_LIG_WIG = VEX_NDS | VEX_LIG | VEX_WIG,
	}

	[Flags]
	enum EvexFlags : uint {
		None							= 0,

		MandatoryPrefixMask				= 3,
		MandatoryPrefixShift			= 0,
		P66								= MandatoryPrefix.P66 << (int)MandatoryPrefixShift,
		PF3								= MandatoryPrefix.PF3 << (int)MandatoryPrefixShift,
		PF2								= MandatoryPrefix.PF2 << (int)MandatoryPrefixShift,

		OpCodeTableMask					= 3,
		OpCodeTableShift				= 2,
		Table0F							= OpCodeTable.Table0F << (int)OpCodeTableShift,
		Table0F38						= OpCodeTable.Table0F38 << (int)OpCodeTableShift,
		Table0F3A						= OpCodeTable.Table0F3A << (int)OpCodeTableShift,

		EncodableMask					= 3,
		EncodableShift					= 4,
		Encodable_Any					= Encodable.Any << (int)EncodableShift,
		Encodable_Only1632				= Encodable.Only1632 << (int)EncodableShift,
		Encodable_Only64				= Encodable.Only64 << (int)EncodableShift,

		HasGroupIndex					= 1 << 6,
		GroupShift						= 7,
		Group0							= HasGroupIndex | (0 << (int)GroupShift),
		Group1							= HasGroupIndex | (1 << (int)GroupShift),
		Group2							= HasGroupIndex | (2 << (int)GroupShift),
		Group3							= HasGroupIndex | (3 << (int)GroupShift),
		Group4							= HasGroupIndex | (4 << (int)GroupShift),
		Group5							= HasGroupIndex | (5 << (int)GroupShift),
		Group6							= HasGroupIndex | (6 << (int)GroupShift),
		Group7							= HasGroupIndex | (7 << (int)GroupShift),

		TupleTypeBits					= 6,
		TupleTypeMask					= (1 << (int)TupleTypeBits) - 1,
		TupleTypeShift					= 10,

		EVEX_DDS						= 0,
		EVEX_NDS						= 0,
		EVEX_NDD						= 0,

		EVEX_LShift						= 24,
		EVEX_L128						= VectorLength.L128 << (int)EVEX_LShift,
		EVEX_L256						= VectorLength.L256 << (int)EVEX_LShift,
		EVEX_L512						= VectorLength.L512 << (int)EVEX_LShift,
		EVEX_LIG						= EVEX_L128,

		EVEX_W0							= 0,
		EVEX_W1							= 0x04000000,
		EVEX_WIG						= EVEX_W0,

		EVEX_b							= 0x08000000,
		EVEX_er							= 0x10000000,
		EVEX_sae						= 0x20000000,
		EVEX_k1							= 0x40000000,
		EVEX_z							= 0x80000000,
		er = EVEX_er,
		k1 = EVEX_k1,
		k1_b = EVEX_k1 | EVEX_b,
		k1_sae = EVEX_k1 | EVEX_sae,
		k1_sae_b = EVEX_k1 | EVEX_sae | EVEX_b,
		k1z = EVEX_k1 | EVEX_z,
		k1z_b = EVEX_k1 | EVEX_z | EVEX_b,
		k1z_er = EVEX_k1 | EVEX_z | EVEX_er,
		k1z_er_b = EVEX_k1 | EVEX_z | EVEX_er | EVEX_b,
		k1z_sae = EVEX_k1 | EVEX_z | EVEX_sae,
		k1z_sae_b = EVEX_k1 | EVEX_z | EVEX_sae | EVEX_b,
		sae = EVEX_sae,
		sae_b = EVEX_sae | EVEX_b,
		EVEX_128_W0 = EVEX_L128 | EVEX_W0,
		EVEX_128_W1 = EVEX_L128 | EVEX_W1,
		EVEX_128_WIG = EVEX_L128 | EVEX_WIG,
		EVEX_256_W0 = EVEX_L256 | EVEX_W0,
		EVEX_256_W1 = EVEX_L256 | EVEX_W1,
		EVEX_256_WIG = EVEX_L256 | EVEX_WIG,
		EVEX_512_W0 = EVEX_L512 | EVEX_W0,
		EVEX_512_W1 = EVEX_L512 | EVEX_W1,
		EVEX_512_WIG = EVEX_L512 | EVEX_WIG,
		EVEX_LIG_W0 = EVEX_LIG | EVEX_W0,
		EVEX_LIG_W1 = EVEX_LIG | EVEX_W1,
		EVEX_DDS_128_W0 = EVEX_DDS | EVEX_L128 | EVEX_W0,
		EVEX_DDS_128_W1 = EVEX_DDS | EVEX_L128 | EVEX_W1,
		EVEX_DDS_256_W0 = EVEX_DDS | EVEX_L256 | EVEX_W0,
		EVEX_DDS_256_W1 = EVEX_DDS | EVEX_L256 | EVEX_W1,
		EVEX_DDS_512_W0 = EVEX_DDS | EVEX_L512 | EVEX_W0,
		EVEX_DDS_512_W1 = EVEX_DDS | EVEX_L512 | EVEX_W1,
		EVEX_DDS_LIG_W0 = EVEX_DDS | EVEX_LIG | EVEX_W0,
		EVEX_DDS_LIG_W1 = EVEX_DDS | EVEX_LIG | EVEX_W1,
		EVEX_NDD_128_W0 = EVEX_NDD | EVEX_L128 | EVEX_W0,
		EVEX_NDD_128_W1 = EVEX_NDD | EVEX_L128 | EVEX_W1,
		EVEX_NDD_128_WIG = EVEX_NDD | EVEX_L128 | EVEX_WIG,
		EVEX_NDD_256_W0 = EVEX_NDD | EVEX_L256 | EVEX_W0,
		EVEX_NDD_256_W1 = EVEX_NDD | EVEX_L256 | EVEX_W1,
		EVEX_NDD_256_WIG = EVEX_NDD | EVEX_L256 | EVEX_WIG,
		EVEX_NDD_512_W0 = EVEX_NDD | EVEX_L512 | EVEX_W0,
		EVEX_NDD_512_W1 = EVEX_NDD | EVEX_L512 | EVEX_W1,
		EVEX_NDD_512_WIG = EVEX_NDD | EVEX_L512 | EVEX_WIG,
		EVEX_NDS_128_W0 = EVEX_NDS | EVEX_L128 | EVEX_W0,
		EVEX_NDS_128_W1 = EVEX_NDS | EVEX_L128 | EVEX_W1,
		EVEX_NDS_128_WIG = EVEX_NDS | EVEX_L128 | EVEX_WIG,
		EVEX_NDS_256_W0 = EVEX_NDS | EVEX_L256 | EVEX_W0,
		EVEX_NDS_256_W1 = EVEX_NDS | EVEX_L256 | EVEX_W1,
		EVEX_NDS_256_WIG = EVEX_NDS | EVEX_L256 | EVEX_WIG,
		EVEX_NDS_512_W0 = EVEX_NDS | EVEX_L512 | EVEX_W0,
		EVEX_NDS_512_W1 = EVEX_NDS | EVEX_L512 | EVEX_W1,
		EVEX_NDS_512_WIG = EVEX_NDS | EVEX_L512 | EVEX_WIG,
		EVEX_NDS_LIG_W0 = EVEX_NDS | EVEX_LIG | EVEX_W0,
		EVEX_NDS_LIG_W1 = EVEX_NDS | EVEX_LIG | EVEX_W1,
	}

	delegate bool TryConvertToDisp8N(Encoder encoder, ref Instruction instr, EncoderOpCodeHandler handler, int displ, out sbyte compressedValue);

	abstract class EncoderOpCodeHandler {
		internal readonly Code TEST_Code;
		internal readonly uint OpCode;
		internal readonly int GroupIndex;
		internal Encodable Encodable;
		internal OperandSize OpSize;
		internal AddressSize AddrSize;
		internal readonly TryConvertToDisp8N TryConvertToDisp8N;
		internal readonly Op[] Operands;
		protected EncoderOpCodeHandler(Code code, uint opCode, int groupIndex, Encodable encodable, OperandSize opSize, AddressSize addrSize, TryConvertToDisp8N tryConvertToDisp8N, Op[] operands) {
			TEST_Code = code;
			OpCode = opCode;
			GroupIndex = groupIndex;
			Encodable = encodable;
			OpSize = opSize;
			AddrSize = addrSize;
			TryConvertToDisp8N = tryConvertToDisp8N;
			Operands = operands;
		}

		protected static Code GetCode(uint dword1) => (Code)(dword1 & (uint)EncFlags1.CodeMask);
		protected static uint GetOpCode(uint dword1) => dword1 >> (int)EncFlags1.OpCodeShift;
		public abstract void Encode(Encoder encoder, ref Instruction instr);
	}

	sealed class InvalidHandler : EncoderOpCodeHandler {
		internal const string ERROR_MESSAGE = "Can't encode an invalid instruction";

		public InvalidHandler(Code code) : base(code, 0, 0, Encodable.Any, OperandSize.None, AddressSize.None, null, Array.Empty<Op>()) => Debug.Assert(code == Code.INVALID);

		public override void Encode(Encoder encoder, ref Instruction instr) =>
			encoder.ErrorMessage = ERROR_MESSAGE;
	}

	sealed class LegacyHandler : EncoderOpCodeHandler {
		readonly byte tableByte1, tableByte2;
		readonly byte mandatoryPrefix;
		readonly uint rexBits;

		static int GetGroupIndex(uint dword2) {
			if ((dword2 & (uint)LegacyFlags.HasGroupIndex) == 0)
				return -1;
			return (int)((dword2 >> (int)LegacyFlags.GroupShift) & 7);
		}

		static Op[] CreateOps(uint dword3) {
			var op0 = (LegacyOpKind)(dword3 & (uint)LegacyFlags3.OpMask);
			var op1 = (LegacyOpKind)((dword3 >> (int)LegacyFlags3.Op1Shift) & (uint)LegacyFlags3.OpMask);
			var op2 = (LegacyOpKind)((dword3 >> (int)LegacyFlags3.Op2Shift) & (uint)LegacyFlags3.OpMask);
			if (op2 != LegacyOpKind.None) {
				Debug.Assert(op0 != LegacyOpKind.None && op1 != LegacyOpKind.None);
				return new Op[] { LegacyOps.Ops[(int)op0], LegacyOps.Ops[(int)op1], LegacyOps.Ops[(int)op2] };
			}
			if (op1 != LegacyOpKind.None) {
				Debug.Assert(op0 != LegacyOpKind.None);
				return new Op[] { LegacyOps.Ops[(int)op0], LegacyOps.Ops[(int)op1] };
			}
			if (op0 != LegacyOpKind.None)
				return new Op[] { LegacyOps.Ops[(int)op0] };
			return Array.Empty<Op>();
		}

		public LegacyHandler(uint dword1, uint dword2, uint dword3)
			: base(GetCode(dword1), GetOpCode(dword1), GetGroupIndex(dword2), (Encodable)((dword2 >> (int)LegacyFlags.EncodableShift) & (int)LegacyFlags.EncodableMask), (OperandSize)((dword2 >> (int)LegacyFlags.Legacy_OpSizeShift) & (int)LegacyFlags.Legacy_OperandSizeMask), (AddressSize)((dword2 >> (int)LegacyFlags.Legacy_AddrSizeShift) & (int)LegacyFlags.Legacy_AddressSizeMask), null, CreateOps(dword3)) {
			switch ((OpCodeTable)((dword2 >> (int)LegacyFlags.OpCodeTableShift) & (int)LegacyFlags.OpCodeTableMask)) {
			case OpCodeTable.Normal:
				tableByte1 = 0;
				tableByte2 = 0;
				break;

			case OpCodeTable.Table0F:
				tableByte1 = 0x0F;
				tableByte2 = 0;
				break;

			case OpCodeTable.Table0F38:
				tableByte1 = 0x0F;
				tableByte2 = 0x38;
				break;

			case OpCodeTable.Table0F3A:
				tableByte1 = 0x0F;
				tableByte2 = 0x3A;
				break;

			default:
				throw new InvalidOperationException();
			}

			switch ((MandatoryPrefix)((dword2 >> (int)LegacyFlags.MandatoryPrefixShift) & (int)LegacyFlags.MandatoryPrefixMask)) {
			case MandatoryPrefix.None:	mandatoryPrefix = 0x00; break;
			case MandatoryPrefix.P66:	mandatoryPrefix = 0x66; break;
			case MandatoryPrefix.PF3:	mandatoryPrefix = 0xF3; break;
			case MandatoryPrefix.PF2:	mandatoryPrefix = 0xF2; break;
			default:					throw new InvalidOperationException();
			}

			rexBits = 0;
			if ((dword2 & (uint)LegacyFlags.Legacy_REX) != 0)
				rexBits |= 0x40;
			if ((dword2 & (uint)LegacyFlags.Legacy_REX_b) != 0)
				rexBits |= 1;
		}

		public override void Encode(Encoder encoder, ref Instruction instr) {
			byte b;
			if ((b = mandatoryPrefix) != 0)
				encoder.WriteByte(b);

			Debug.Assert((int)EncoderFlags.B == 0x01);
			Debug.Assert((int)EncoderFlags.X == 0x02);
			Debug.Assert((int)EncoderFlags.R == 0x04);
			Debug.Assert((int)EncoderFlags.W == 0x08);
			Debug.Assert((int)EncoderFlags.REX == 0x40);
			uint rex = ((uint)encoder.EncoderFlags & 0x4F) | rexBits;
			if (rex != 0) {
				if ((encoder.EncoderFlags & EncoderFlags.HighLegacy8BitRegs) != 0)
					encoder.ErrorMessage = "Registers AH, CH, DH, BH can't be used if there's a REX prefix. Use AL, CL, DL, BL, SPL, BPL, SIL, DIL, R8L-R15L instead.";
				rex |= 0x40;
				encoder.WriteByte((byte)rex);
			}

			if ((b = tableByte1) != 0) {
				encoder.WriteByte(b);
				if ((b = tableByte2) != 0)
					encoder.WriteByte(b);
			}
		}
	}

	sealed class VexHandler : EncoderOpCodeHandler {
		readonly OpCodeTable opCodeTable;
		readonly MandatoryPrefix mandatoryPrefix;
		readonly bool W1;
		readonly uint lastByte;

		static int GetGroupIndex(uint dword2) {
			if ((dword2 & (uint)LegacyFlags.HasGroupIndex) == 0)
				return -1;
			return (int)((dword2 >> (int)LegacyFlags.GroupShift) & 7);
		}

		static Op[] CreateOps(uint dword3) {
			var op0 = (VexOpKind)(dword3 & (uint)VexFlags3.OpMask);
			var op1 = (VexOpKind)((dword3 >> (int)VexFlags3.Op1Shift) & (uint)VexFlags3.OpMask);
			var op2 = (VexOpKind)((dword3 >> (int)VexFlags3.Op2Shift) & (uint)VexFlags3.OpMask);
			var op3 = (VexOpKind)((dword3 >> (int)VexFlags3.Op3Shift) & (uint)VexFlags3.OpMask);
			if (op3 != VexOpKind.None) {
				Debug.Assert(op0 != VexOpKind.None && op1 != VexOpKind.None && op2 != VexOpKind.None);
				return new Op[] { VexOps.Ops[(int)op0], VexOps.Ops[(int)op1], VexOps.Ops[(int)op2], VexOps.Ops[(int)op3] };
			}
			if (op2 != VexOpKind.None) {
				Debug.Assert(op0 != VexOpKind.None && op1 != VexOpKind.None);
				return new Op[] { VexOps.Ops[(int)op0], VexOps.Ops[(int)op1], VexOps.Ops[(int)op2] };
			}
			if (op1 != VexOpKind.None) {
				Debug.Assert(op0 != VexOpKind.None);
				return new Op[] { VexOps.Ops[(int)op0], VexOps.Ops[(int)op1] };
			}
			if (op0 != VexOpKind.None)
				return new Op[] { VexOps.Ops[(int)op0] };
			return Array.Empty<Op>();
		}

		public VexHandler(uint dword1, uint dword2, uint dword3)
			: base(GetCode(dword1), GetOpCode(dword1), GetGroupIndex(dword2), (Encodable)((dword2 >> (int)VexFlags.EncodableShift) & (int)VexFlags.EncodableMask), OperandSize.None, AddressSize.None, null, CreateOps(dword3)) {
			opCodeTable = (OpCodeTable)((dword2 >> (int)VexFlags.OpCodeTableShift) & (int)VexFlags.OpCodeTableMask);
			mandatoryPrefix = (MandatoryPrefix)((dword2 >> (int)VexFlags.MandatoryPrefixShift) & (int)VexFlags.MandatoryPrefixMask);
			W1 = (dword2 & (uint)VexFlags.VEX_W1) != 0;
			lastByte = (dword2 >> ((int)VexFlags.VEX_LShift - 2)) & 4;
			if (W1)
				lastByte |= 0x80;
		}

		public override void Encode(Encoder encoder, ref Instruction instr) {
			uint encoderFlags = (uint)encoder.EncoderFlags;

			Debug.Assert((int)MandatoryPrefix.None == 0);
			Debug.Assert((int)MandatoryPrefix.P66 == 1);
			Debug.Assert((int)MandatoryPrefix.PF3 == 2);
			Debug.Assert((int)MandatoryPrefix.PF2 == 3);
			uint b = (uint)mandatoryPrefix;
			b |= (~encoderFlags >> ((int)EncoderFlags.VvvvvShift - 3)) & 0x78;

			if (W1 || opCodeTable != OpCodeTable.Table0F || (encoderFlags & (uint)(EncoderFlags.X | EncoderFlags.B | EncoderFlags.W)) != 0) {
				encoder.WriteByte(0xC4);
				Debug.Assert((int)OpCodeTable.Normal == 0);
				Debug.Assert((int)OpCodeTable.Table0F == 1);
				Debug.Assert((int)OpCodeTable.Table0F38 == 2);
				Debug.Assert((int)OpCodeTable.Table0F3A == 3);
				uint b2 = (uint)opCodeTable;
				Debug.Assert((int)EncoderFlags.B == 1);
				Debug.Assert((int)EncoderFlags.X == 2);
				Debug.Assert((int)EncoderFlags.R == 4);
				b2 |= (~encoderFlags & 7) << 5;
				encoder.WriteByte((byte)b2);
				b |= lastByte;
				encoder.WriteByte((byte)b);
			}
			else {
				encoder.WriteByte(0xC5);
				Debug.Assert((int)EncoderFlags.R == 4);
				b |= (~encoderFlags & 4) << 5;
				b |= lastByte;
				encoder.WriteByte((byte)b);
			}
		}
	}

	sealed class EvexHandler : EncoderOpCodeHandler {
		readonly EvexFlags flags;
		readonly TupleType tupleType;
		readonly OpCodeTable opCodeTable;
		readonly uint p1Bits;
		readonly uint llBits;

		static int GetGroupIndex(uint dword2) {
			if ((dword2 & (uint)LegacyFlags.HasGroupIndex) == 0)
				return -1;
			return (int)((dword2 >> (int)LegacyFlags.GroupShift) & 7);
		}

		static Op[] CreateOps(uint dword3) {
			var op0 = (EvexOpKind)(dword3 & (uint)EvexFlags3.OpMask);
			var op1 = (EvexOpKind)((dword3 >> (int)EvexFlags3.Op1Shift) & (uint)EvexFlags3.OpMask);
			var op2 = (EvexOpKind)((dword3 >> (int)EvexFlags3.Op2Shift) & (uint)EvexFlags3.OpMask);
			var op3 = (EvexOpKind)((dword3 >> (int)EvexFlags3.Op3Shift) & (uint)EvexFlags3.OpMask);
			if (op3 != EvexOpKind.None) {
				Debug.Assert(op0 != EvexOpKind.None && op1 != EvexOpKind.None && op2 != EvexOpKind.None);
				return new Op[] { EvexOps.Ops[(int)op0], EvexOps.Ops[(int)op1], EvexOps.Ops[(int)op2], EvexOps.Ops[(int)op3] };
			}
			if (op2 != EvexOpKind.None) {
				Debug.Assert(op0 != EvexOpKind.None && op1 != EvexOpKind.None);
				return new Op[] { EvexOps.Ops[(int)op0], EvexOps.Ops[(int)op1], EvexOps.Ops[(int)op2] };
			}
			if (op1 != EvexOpKind.None) {
				Debug.Assert(op0 != EvexOpKind.None);
				return new Op[] { EvexOps.Ops[(int)op0], EvexOps.Ops[(int)op1] };
			}
			if (op0 != EvexOpKind.None)
				return new Op[] { EvexOps.Ops[(int)op0] };
			return Array.Empty<Op>();
		}

		static readonly TryConvertToDisp8N tryConvertToDisp8N = new TryConvertToDisp8NImpl().TryConvertToDisp8N;

		public EvexHandler(uint dword1, uint dword2, uint dword3)
			: base(GetCode(dword1), GetOpCode(dword1), GetGroupIndex(dword2), (Encodable)((dword2 >> (int)EvexFlags.EncodableShift) & (int)EvexFlags.EncodableMask), OperandSize.None, AddressSize.None, tryConvertToDisp8N, CreateOps(dword3)) {
			flags = (EvexFlags)dword2;
			tupleType = (TupleType)((dword2 >> (int)EvexFlags.TupleTypeShift) & (uint)EvexFlags.TupleTypeMask);
			opCodeTable = (OpCodeTable)((dword2 >> (int)EvexFlags.OpCodeTableShift) & (int)EvexFlags.OpCodeTableMask);
			Debug.Assert((int)MandatoryPrefix.None == 0);
			Debug.Assert((int)MandatoryPrefix.P66 == 1);
			Debug.Assert((int)MandatoryPrefix.PF3 == 2);
			Debug.Assert((int)MandatoryPrefix.PF2 == 3);
			p1Bits = 4 | ((dword2 >> (int)EvexFlags.MandatoryPrefixShift) & (int)EvexFlags.MandatoryPrefixMask);
			if ((dword2 & (uint)EvexFlags.EVEX_W1) != 0)
				p1Bits |= 0x80;
			llBits = (dword2 >> ((int)EvexFlags.EVEX_LShift - 5)) & 0x60;
		}

		sealed class TryConvertToDisp8NImpl {
			public bool TryConvertToDisp8N(Encoder encoder, ref Instruction instr, EncoderOpCodeHandler handler, int displ, out sbyte compressedValue) {
				var evexHandler = (EvexHandler)handler;
				int n;
				switch (evexHandler.tupleType) {
				case TupleType.None:
					n = 1;
					break;

				case TupleType.Full_128:
					if ((encoder.EncoderFlags & EncoderFlags.b) != 0)
						n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 8 : 4;
					else
						n = 16;
					break;

				case TupleType.Full_256:
					if ((encoder.EncoderFlags & EncoderFlags.b) != 0)
						n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 8 : 4;
					else
						n = 32;
					break;

				case TupleType.Full_512:
					if ((encoder.EncoderFlags & EncoderFlags.b) != 0)
						n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 8 : 4;
					else
						n = 64;
					break;

				case TupleType.Half_128:
					n = (encoder.EncoderFlags & EncoderFlags.b) != 0 ? 4 : 8;
					break;

				case TupleType.Half_256:
					n = (encoder.EncoderFlags & EncoderFlags.b) != 0 ? 4 : 16;
					break;

				case TupleType.Half_512:
					n = (encoder.EncoderFlags & EncoderFlags.b) != 0 ? 4 : 32;
					break;

				case TupleType.Full_Mem_128:
					n = 16;
					break;

				case TupleType.Full_Mem_256:
					n = 32;
					break;

				case TupleType.Full_Mem_512:
					n = 64;
					break;

				case TupleType.Tuple1_Scalar:
					n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 8 : 4;
					break;

				case TupleType.Tuple1_Scalar_1:
					n = 1;
					break;

				case TupleType.Tuple1_Scalar_2:
					n = 2;
					break;

				case TupleType.Tuple1_Scalar_4:
					n = 4;
					break;

				case TupleType.Tuple1_Scalar_8:
					n = 8;
					break;

				case TupleType.Tuple1_Fixed:
					n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 8 : 4;
					break;

				case TupleType.Tuple1_Fixed_4:
					n = 4;
					break;

				case TupleType.Tuple1_Fixed_8:
					n = 8;
					break;

				case TupleType.Tuple2:
					n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 16 : 8;
					break;

				case TupleType.Tuple4:
					n = (evexHandler.flags & EvexFlags.EVEX_W1) != 0 ? 32 : 16;
					break;

				case TupleType.Tuple8:
					Debug.Assert((evexHandler.flags & EvexFlags.EVEX_W1) == 0);
					n = 32;
					break;

				case TupleType.Tuple1_4X:
					n = 16;
					break;

				case TupleType.Half_Mem_128:
					n = 8;
					break;

				case TupleType.Half_Mem_256:
					n = 16;
					break;

				case TupleType.Half_Mem_512:
					n = 32;
					break;

				case TupleType.Quarter_Mem_128:
					n = 4;
					break;

				case TupleType.Quarter_Mem_256:
					n = 8;
					break;

				case TupleType.Quarter_Mem_512:
					n = 16;
					break;

				case TupleType.Eighth_Mem_128:
					n = 2;
					break;

				case TupleType.Eighth_Mem_256:
					n = 4;
					break;

				case TupleType.Eighth_Mem_512:
					n = 8;
					break;

				case TupleType.Mem128:
					n = 16;
					break;

				case TupleType.MOVDDUP_128:
					n = 8;
					break;

				case TupleType.MOVDDUP_256:
					n = 32;
					break;

				case TupleType.MOVDDUP_512:
					n = 64;
					break;

				default:
					throw new InvalidOperationException();
				}

				int res = displ / n;
				if (res * n == displ && sbyte.MinValue <= res && res <= sbyte.MaxValue) {
					compressedValue = (sbyte)res;
					return true;
				}

				compressedValue = 0;
				return false;
			}
		}

		public override void Encode(Encoder encoder, ref Instruction instr) {
			uint encoderFlags = (uint)encoder.EncoderFlags;

			encoder.WriteByte(0x62);

			Debug.Assert((int)OpCodeTable.Normal == 0);
			Debug.Assert((int)OpCodeTable.Table0F == 1);
			Debug.Assert((int)OpCodeTable.Table0F38 == 2);
			Debug.Assert((int)OpCodeTable.Table0F3A == 3);
			uint b = (uint)opCodeTable;
			Debug.Assert((int)EncoderFlags.B == 1);
			Debug.Assert((int)EncoderFlags.X == 2);
			Debug.Assert((int)EncoderFlags.R == 4);
			b |= (encoderFlags & 7) << 5;
			Debug.Assert((int)EncoderFlags.R2 == 0x00000200);
			b |= (encoderFlags >> (9 - 4)) & 0x10;
			b ^= ~0xFU;
			encoder.WriteByte((byte)b);

			b = p1Bits;
			b |= (~encoderFlags >> ((int)EncoderFlags.VvvvvShift - 3)) & 0x78;
			encoder.WriteByte((byte)b);

			b = instr.InternalOpMask;
			if (b != 0 && (flags & EvexFlags.k1) == 0)
				encoder.ErrorMessage = "The instruction doesn't support opmask registers";
			b |= (encoderFlags >> ((int)EncoderFlags.VvvvvShift + 4 - 3)) & 8;
			if (instr.SuppressAllExceptions) {
				if ((flags & EvexFlags.EVEX_sae) == 0)
					encoder.ErrorMessage = "The instruction doesn't support suppress-all-exceptions";
				b |= 0x10;
			}
			var rc = instr.RoundingControl;
			if (rc != RoundingControl.None) {
				if ((flags & EvexFlags.EVEX_er) == 0)
					encoder.ErrorMessage = "The instruction doesn't support rounding control";
				b |= 0x10;
				Debug.Assert((int)RoundingControl.RoundToNearest == 1);
				Debug.Assert((int)RoundingControl.RoundDown == 2);
				Debug.Assert((int)RoundingControl.RoundUp == 3);
				Debug.Assert((int)RoundingControl.RoundTowardZero == 4);
				b |= (uint)(rc - RoundingControl.RoundToNearest) << 5;
			}
			else if ((flags & EvexFlags.EVEX_sae) == 0 || !instr.SuppressAllExceptions)
				b |= llBits;
			if ((encoderFlags & (uint)EncoderFlags.b) != 0) {
				if ((flags & EvexFlags.EVEX_b) == 0)
					encoder.ErrorMessage = "The instruction doesn't support broadcasting";
				b |= 0x10;
			}
			if (instr.ZeroingMasking) {
				if ((flags & EvexFlags.EVEX_z) == 0)
					encoder.ErrorMessage = "The instruction doesn't support zeroing masking";
				b |= 0x80;
			}
			b ^= 8;
			encoder.WriteByte((byte)b);
		}
	}
}
#endif
