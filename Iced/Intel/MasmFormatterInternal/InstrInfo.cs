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

#if !NO_MASM_FORMATTER && !NO_FORMATTER
using System;
using System.Diagnostics;

namespace Iced.Intel.MasmFormatterInternal {
	enum InstrOpKind : byte {
		Register = OpKind.Register,
		NearBranch16 = OpKind.NearBranch16,
		NearBranch32 = OpKind.NearBranch32,
		NearBranch64 = OpKind.NearBranch64,
		FarBranch16 = OpKind.FarBranch16,
		FarBranch32 = OpKind.FarBranch32,
		Immediate8 = OpKind.Immediate8,
		Immediate8_Enter = OpKind.Immediate8_Enter,
		Immediate16 = OpKind.Immediate16,
		Immediate32 = OpKind.Immediate32,
		Immediate64 = OpKind.Immediate64,
		Immediate8to16 = OpKind.Immediate8to16,
		Immediate8to32 = OpKind.Immediate8to32,
		Immediate8to64 = OpKind.Immediate8to64,
		Immediate32to64 = OpKind.Immediate32to64,
		MemorySegSI = OpKind.MemorySegSI,
		MemorySegESI = OpKind.MemorySegESI,
		MemorySegRSI = OpKind.MemorySegRSI,
		MemorySegDI = OpKind.MemorySegDI,
		MemorySegEDI = OpKind.MemorySegEDI,
		MemorySegRDI = OpKind.MemorySegRDI,
		MemoryESDI = OpKind.MemoryESDI,
		MemoryESEDI = OpKind.MemoryESEDI,
		MemoryESRDI = OpKind.MemoryESRDI,
		Memory64 = OpKind.Memory64,
		Memory = OpKind.Memory,

		// Extra opkinds
		ExtraImmediate8_Value3,
	}

	[Flags]
	enum InstrOpInfoFlags : byte {
		None						= 0,

		MemSize_Mask				= 3,
		// Use xmmword ptr etc
		MemSize_Sse					= 0,
		// Use mmword ptr etc
		MemSize_Mmx					= 1,
		// use qword ptr, oword ptr
		MemSize_Normal				= 2,
		// show no mem size
		MemSize_Nothing				= 3,

		// AlwaysShowMemorySize is disabled: always show memory size
		ShowNoMemSize_ForceSize		= 4,

		JccNotTaken					= 8,
		JccTaken					= 0x10,
		BndPrefix					= 0x20,
		IgnoreIndexReg				= 0x40,
	}

	struct InstrOpInfo {
		internal const int TEST_RegisterBits = 8;

		public string Mnemonic;
		public InstrOpInfoFlags Flags;
		public byte OpCount;
		public InstrOpKind Op0Kind;
		public InstrOpKind Op1Kind;
		public InstrOpKind Op2Kind;
		public InstrOpKind Op3Kind;
		public byte Op0Register;
		public byte Op1Register;
		public byte Op2Register;
		public byte Op3Register;

		public Register GetOpRegister(int operand) {
			switch (operand) {
			case 0: return (Register)Op0Register;
			case 1: return (Register)Op1Register;
			case 2: return (Register)Op2Register;
			case 3: return (Register)Op3Register;
			default: throw new ArgumentOutOfRangeException(nameof(operand));
			}
		}

		public InstrOpKind GetOpKind(int operand) {
			switch (operand) {
			case 0: return Op0Kind;
			case 1: return Op1Kind;
			case 2: return Op2Kind;
			case 3: return Op3Kind;
			default: throw new ArgumentOutOfRangeException(nameof(operand));
			}
		}

		public InstrOpInfo(string mnemonic, ref Instruction instr, InstrOpInfoFlags flags) {
			Mnemonic = mnemonic;
			Flags = flags;
			OpCount = (byte)instr.OpCount;
			Debug.Assert(instr.OpCount <= 4);
			Op0Kind = (InstrOpKind)instr.Op0Kind;
			Op1Kind = (InstrOpKind)instr.Op1Kind;
			Op2Kind = (InstrOpKind)instr.Op2Kind;
			Op3Kind = (InstrOpKind)instr.Op3Kind;
			Debug.Assert(TEST_RegisterBits == 8);
			Op0Register = (byte)instr.Op0Register;
			Debug.Assert(TEST_RegisterBits == 8);
			Op1Register = (byte)instr.Op1Register;
			Debug.Assert(TEST_RegisterBits == 8);
			Op2Register = (byte)instr.Op2Register;
			Debug.Assert(TEST_RegisterBits == 8);
			Op3Register = (byte)instr.Op3Register;
		}
	}

	abstract class InstrInfo {
		internal readonly Code TEST_Code;
		protected InstrInfo(Code code) => TEST_Code = code;

		public abstract void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info);
	}

	sealed class SimpleInstrInfo : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo(Code code, string mnemonic) : this(code, mnemonic, InstrOpInfoFlags.None) { }

		public SimpleInstrInfo(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) =>
			info = new InstrOpInfo(mnemonic, ref instr, flags);
	}

	sealed class SimpleInstrInfo_mmxmem : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_mmxmem(Code code, string mnemonic)
			: this(code, mnemonic, InstrOpInfoFlags.None) {
		}

		public SimpleInstrInfo_mmxmem(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags | InstrOpInfoFlags.MemSize_Mmx;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) =>
			info = new InstrOpInfo(mnemonic, ref instr, flags);
	}

	sealed class SimpleInstrInfo_memsize : InstrInfo {
		readonly int codeSize;
		readonly string mnemonic;

		static int GetCodeSize(CodeSize codeSize) {
			switch (codeSize) {
			case CodeSize.Code16:	return 16;
			case CodeSize.Code32:	return 32;
			case CodeSize.Code64:	return 64;
			default:
			case CodeSize.Unknown:	return 0;
			}
		}

		public SimpleInstrInfo_memsize(Code code, int codeSize, string mnemonic)
			: base(code) {
			this.codeSize = codeSize;
			this.mnemonic = mnemonic;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			int instrCodeSize = GetCodeSize(instr.CodeSize);
			var flags = instrCodeSize == 0 || (instrCodeSize & codeSize) != 0 ? InstrOpInfoFlags.MemSize_Nothing : InstrOpInfoFlags.MemSize_Normal | InstrOpInfoFlags.ShowNoMemSize_ForceSize;
			info = new InstrOpInfo(mnemonic, ref instr, flags);
		}
	}

	sealed class SimpleInstrInfo_AamAad : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_AamAad(Code code, string mnemonic) : base(code) => this.mnemonic = mnemonic;

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			if (instr.Immediate8 == 10) {
				info = default;
				info.Mnemonic = mnemonic;
			}
			else
				info = new InstrOpInfo(mnemonic, ref instr, InstrOpInfoFlags.None);
		}
	}

	sealed class SimpleInstrInfo_Ib : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_Ib(Code code, string mnemonic) : base(code) => this.mnemonic = mnemonic;

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = default;
			info.Mnemonic = mnemonic;
			info.OpCount = 1;
			info.Op0Kind = InstrOpKind.ExtraImmediate8_Value3;
		}
	}

	sealed class SimpleInstrInfo_YD : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_YD(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op0Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemoryESDI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemoryESEDI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemoryESRDI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op0Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm)
				info = new InstrOpInfo(mnemonic_args, ref instr, flags);
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_DX : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_DX(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op1Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemorySegSI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemorySegESI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemorySegRSI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op1Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm)
				info = new InstrOpInfo(mnemonic_args, ref instr, flags);
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_YX : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_YX(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op0Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemoryESDI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemoryESEDI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemoryESRDI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op0Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm)
				info = new InstrOpInfo(mnemonic_args, ref instr, flags);
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_XY : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_XY(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op1Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemoryESDI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemoryESEDI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemoryESRDI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op1Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm)
				info = new InstrOpInfo(mnemonic_args, ref instr, flags);
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_YA : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_YA(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op0Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemoryESDI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemoryESEDI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemoryESRDI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op0Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm) {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_args;
				info.OpCount = 1;
				info.Op0Kind = (InstrOpKind)instr.Op0Kind;
			}
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_AX : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_AX(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op1Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemorySegSI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemorySegESI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemorySegRSI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op1Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm) {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_args;
				info.OpCount = 1;
				info.Op0Kind = (InstrOpKind)instr.Op1Kind;
			}
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_AY : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_AY(Code code, string mnemonic_args, string mnemonic_no_args, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op1Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemoryESDI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemoryESEDI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemoryESRDI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op1Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm) {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_args;
				info.OpCount = 1;
				info.Op0Kind = (InstrOpKind)instr.Op1Kind;
			}
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_XLAT : InstrInfo {
		readonly string mnemonic_args;
		readonly string mnemonic_no_args;

		public SimpleInstrInfo_XLAT(Code code, string mnemonic_args, string mnemonic_no_args)
			: base(code) {
			this.mnemonic_args = mnemonic_args;
			this.mnemonic_no_args = mnemonic_no_args;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			Register baseReg;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				baseReg = instr.MemoryBase;
				break;
			case CodeSize.Code16:
				baseReg = Register.BX;
				break;
			case CodeSize.Code32:
				baseReg = Register.EBX;
				break;
			case CodeSize.Code64:
				baseReg = Register.RBX;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.MemoryBase == baseReg && instr.PrefixSegment == Register.None;
			if (!shortForm)
				info = new InstrOpInfo(mnemonic_args, ref instr, InstrOpInfoFlags.ShowNoMemSize_ForceSize | InstrOpInfoFlags.IgnoreIndexReg);
			else {
				info = default;
				info.Mnemonic = mnemonic_no_args;
			}
		}
	}

	sealed class SimpleInstrInfo_nop : InstrInfo {
		readonly int codeSize;
		readonly string mnemonic;
		readonly Register register;

		public SimpleInstrInfo_nop(Code code, int codeSize, string mnemonic, Register register)
			: base(code) {
			this.codeSize = codeSize;
			this.mnemonic = mnemonic;
			this.register = register;
		}

		static int GetCodeSize(CodeSize codeSize) {
			switch (codeSize) {
			case CodeSize.Code16:	return 16;
			case CodeSize.Code32:	return 32;
			case CodeSize.Code64:	return 64;
			default:
			case CodeSize.Unknown:	return 0;
			}
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			int instrCodeSize = GetCodeSize(instr.CodeSize);
			if (instrCodeSize == 0 || (instrCodeSize & codeSize) != 0)
				info = new InstrOpInfo(mnemonic, ref instr, InstrOpInfoFlags.None);
			else {
				info = default;
				info.Mnemonic = "xchg";
				info.OpCount = 2;
				info.Op0Kind = InstrOpKind.Register;
				info.Op1Kind = InstrOpKind.Register;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)register;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)register;
			}
		}
	}

	sealed class SimpleInstrInfo_STIG1 : InstrInfo {
		readonly string mnemonic;
		readonly bool pseudoOp;

		public SimpleInstrInfo_STIG1(Code code, string mnemonic) : this(code, mnemonic, false) { }

		public SimpleInstrInfo_STIG1(Code code, string mnemonic, bool pseudoOp)
			: base(code) {
			this.mnemonic = mnemonic;
			this.pseudoOp = pseudoOp;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = default;
			info.Mnemonic = mnemonic;
			Debug.Assert(instr.OpCount == 2);
			Debug.Assert(instr.Op0Kind == OpKind.Register && instr.Op0Register == Register.ST0);
			if (!pseudoOp || !(options.UsePseudoOps && instr.Op1Register == Register.ST1)) {
				info.OpCount = 1;
				Debug.Assert(InstrOpKind.Register == 0);
				//info.Op0Kind = InstrOpKind.Register;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)instr.Op1Register;
			}
		}
	}

	sealed class SimpleInstrInfo_STi_ST2 : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_STi_ST2(Code code, string mnemonic)
			: base(code) {
			this.mnemonic = mnemonic;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			const InstrOpInfoFlags flags = 0;
			if (options.UsePseudoOps && (instr.Op0Register == Register.ST1 || instr.Op1Register == Register.ST1)) {
				info = default;
				info.Mnemonic = mnemonic;
			}
			else {
				info = new InstrOpInfo(mnemonic, ref instr, flags);
				Debug.Assert(info.Op1Register == (int)Register.ST0);
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)Registers.Register_ST;
			}
		}
	}

	sealed class SimpleInstrInfo_ST_STi : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_ST_STi(Code code, string mnemonic) : this(code, mnemonic, InstrOpInfoFlags.None) { }

		public SimpleInstrInfo_ST_STi(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = new InstrOpInfo(mnemonic, ref instr, flags);
			Debug.Assert(info.Op0Register == (int)Register.ST0);
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op0Register = (byte)Registers.Register_ST;
		}
	}

	sealed class SimpleInstrInfo_STi_ST : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_STi_ST(Code code, string mnemonic) : this(code, mnemonic, InstrOpInfoFlags.None) { }

		public SimpleInstrInfo_STi_ST(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = new InstrOpInfo(mnemonic, ref instr, flags);
			Debug.Assert(info.Op1Register == (int)Register.ST0);
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op1Register = (byte)Registers.Register_ST;
		}
	}

	sealed class SimpleInstrInfo_monitor : InstrInfo {
		readonly string mnemonic;
		readonly Register reg1;
		readonly Register reg2;
		readonly Register reg3;

		public SimpleInstrInfo_monitor(Code code, string mnemonic, Register reg1, Register reg2, Register reg3)
			: base(code) {
			this.mnemonic = mnemonic;
			this.reg1 = reg1;
			this.reg2 = reg2;
			this.reg3 = reg3;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = default;
			info.Mnemonic = mnemonic;
			info.OpCount = 3;
			info.Op0Kind = InstrOpKind.Register;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op0Register = (byte)reg1;
			info.Op1Kind = InstrOpKind.Register;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op1Register = (byte)reg2;
			info.Op2Kind = InstrOpKind.Register;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op2Register = (byte)reg3;
		}
	}

	sealed class SimpleInstrInfo_mwait : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_mwait(Code code, string mnemonic) : base(code) => this.mnemonic = mnemonic;

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = default;
			info.Mnemonic = mnemonic;
			info.OpCount = 2;
			info.Op0Kind = InstrOpKind.Register;
			info.Op1Kind = InstrOpKind.Register;

			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)Register.EAX;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)Register.ECX;
				break;
			case CodeSize.Code16:
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)Register.AX;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)Register.ECX;
				break;
			case CodeSize.Code32:
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)Register.EAX;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)Register.ECX;
				break;
			case CodeSize.Code64:
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)Register.RAX;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)Register.RCX;
				break;
			}
		}
	}

	sealed class SimpleInstrInfo_maskmovq : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_maskmovq(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			Debug.Assert(instr.OpCount == 3);

			OpKind shortFormOpKind;
			switch (instr.CodeSize) {
			case CodeSize.Unknown:
				shortFormOpKind = instr.Op0Kind;
				break;
			case CodeSize.Code16:
				shortFormOpKind = OpKind.MemorySegDI;
				break;
			case CodeSize.Code32:
				shortFormOpKind = OpKind.MemorySegEDI;
				break;
			case CodeSize.Code64:
				shortFormOpKind = OpKind.MemorySegRDI;
				break;
			default:
				throw new InvalidOperationException();
			}

			bool shortForm = instr.Op0Kind == shortFormOpKind && instr.PrefixSegment == Register.None;
			if (!shortForm)
				info = new InstrOpInfo(mnemonic, ref instr, flags);
			else {
				info = default;
				info.Flags = flags;
				info.Mnemonic = mnemonic;
				info.OpCount = 2;
				info.Op0Kind = (InstrOpKind)instr.Op1Kind;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op0Register = (byte)instr.Op1Register;
				info.Op1Kind = (InstrOpKind)instr.Op2Kind;
				Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
				info.Op1Register = (byte)instr.Op2Register;
			}
		}
	}

	sealed class SimpleInstrInfo_pblendvb : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_pblendvb(Code code, string mnemonic) : base(code) => this.mnemonic = mnemonic;

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = default;
			Debug.Assert(instr.OpCount == 2);
			info.Mnemonic = mnemonic;
			info.OpCount = 3;
			info.Op0Kind = (InstrOpKind)instr.Op0Kind;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op0Register = (byte)instr.Op0Register;
			info.Op1Kind = (InstrOpKind)instr.Op1Kind;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op1Register = (byte)instr.Op1Register;
			info.Op2Kind = InstrOpKind.Register;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op2Register = (byte)Register.XMM0;
		}
	}

	sealed class SimpleInstrInfo_reverse2 : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_reverse2(Code code, string mnemonic) : base(code) => this.mnemonic = mnemonic;

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = default;
			info.Mnemonic = mnemonic;
			Debug.Assert(instr.OpCount == 2);
			info.OpCount = 2;
			info.Op0Kind = (InstrOpKind)instr.Op1Kind;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op0Register = (byte)instr.Op1Register;
			info.Op1Kind = (InstrOpKind)instr.Op0Kind;
			Debug.Assert(InstrOpInfo.TEST_RegisterBits == 8);
			info.Op1Register = (byte)instr.Op0Register;
		}
	}

	sealed class SimpleInstrInfo_OpSize : InstrInfo {
		readonly CodeSize codeSize;
		readonly string[] mnemonics;

		public SimpleInstrInfo_OpSize(Code code, CodeSize codeSize, string mnemonic, string mnemonic16, string mnemonic32, string mnemonic64)
			: base(code) {
			this.codeSize = codeSize;
			mnemonics = new string[4];
			mnemonics[(int)CodeSize.Unknown] = mnemonic;
			mnemonics[(int)CodeSize.Code16] = mnemonic16;
			mnemonics[(int)CodeSize.Code32] = mnemonic32;
			mnemonics[(int)CodeSize.Code64] = mnemonic64;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			string mnemonic;
			if (instr.CodeSize == codeSize)
				mnemonic = mnemonics[(int)CodeSize.Unknown];
			else
				mnemonic = mnemonics[(int)codeSize];
			info = new InstrOpInfo(mnemonic, ref instr, InstrOpInfoFlags.None);
		}
	}

	sealed class SimpleInstrInfo_OpSize2 : InstrInfo {
		readonly string[] mnemonics;

		public SimpleInstrInfo_OpSize2(Code code, string mnemonic, string mnemonic16, string mnemonic32, string mnemonic64)
			: base(code) {
			mnemonics = new string[4];
			mnemonics[(int)CodeSize.Unknown] = mnemonic;
			mnemonics[(int)CodeSize.Code16] = mnemonic16;
			mnemonics[(int)CodeSize.Code32] = mnemonic32;
			mnemonics[(int)CodeSize.Code64] = mnemonic64;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			var mnemonic = mnemonics[(int)instr.CodeSize];
			info = new InstrOpInfo(mnemonic, ref instr, InstrOpInfoFlags.None);
		}
	}

	sealed class SimpleInstrInfo_OpSize2_bnd : InstrInfo {
		readonly string[] mnemonics;

		public SimpleInstrInfo_OpSize2_bnd(Code code, string mnemonic, string mnemonic16, string mnemonic32, string mnemonic64)
			: base(code) {
			mnemonics = new string[4];
			mnemonics[(int)CodeSize.Unknown] = mnemonic;
			mnemonics[(int)CodeSize.Code16] = mnemonic16;
			mnemonics[(int)CodeSize.Code32] = mnemonic32;
			mnemonics[(int)CodeSize.Code64] = mnemonic64;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			var flags = InstrOpInfoFlags.None;
			if (instr.HasPrefixRepne)
				flags |= InstrOpInfoFlags.BndPrefix;
			var mnemonic = mnemonics[(int)instr.CodeSize];
			info = new InstrOpInfo(mnemonic, ref instr, flags);
		}
	}

	sealed class SimpleInstrInfo_pushm : InstrInfo {
		readonly CodeSize codeSize;
		readonly string mnemonic;

		public SimpleInstrInfo_pushm(Code code, CodeSize codeSize, string mnemonic)
			: base(code) {
			this.codeSize = codeSize;
			this.mnemonic = mnemonic;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			var flags = InstrOpInfoFlags.None;
			if (instr.CodeSize != codeSize && instr.CodeSize != CodeSize.Unknown)
				flags |= InstrOpInfoFlags.ShowNoMemSize_ForceSize;
			info = new InstrOpInfo(mnemonic, ref instr, flags);
		}
	}

	sealed class SimpleInstrInfo_fword : InstrInfo {
		readonly CodeSize codeSize;
		readonly bool forceNoMemSize;
		readonly string mnemonic;
		readonly string mnemonic2;

		public SimpleInstrInfo_fword(Code code, CodeSize codeSize, bool forceNoMemSize, string mnemonic, string mnemonic2)
			: base(code) {
			this.codeSize = codeSize;
			this.forceNoMemSize = forceNoMemSize;
			this.mnemonic = mnemonic;
			this.mnemonic2 = mnemonic2;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			var flags = InstrOpInfoFlags.None;
			string mnemonic;
			if (instr.CodeSize == codeSize || instr.CodeSize == CodeSize.Unknown)
				mnemonic = this.mnemonic;
			else
				mnemonic = mnemonic2;
			if (!forceNoMemSize)
				flags |= InstrOpInfoFlags.ShowNoMemSize_ForceSize;
			info = new InstrOpInfo(mnemonic, ref instr, flags);
		}
	}

	sealed class SimpleInstrInfo_jcc : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_jcc(Code code, string mnemonic) : base(code) => this.mnemonic = mnemonic;

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			var flags = InstrOpInfoFlags.None;
			var prefixSeg = instr.PrefixSegment;
			if (prefixSeg == Register.CS)
				flags |= InstrOpInfoFlags.JccNotTaken;
			else if (prefixSeg == Register.DS)
				flags |= InstrOpInfoFlags.JccTaken;
			if (instr.HasPrefixRepne)
				flags |= InstrOpInfoFlags.BndPrefix;
			info = new InstrOpInfo(mnemonic, ref instr, flags);
		}
	}

	sealed class SimpleInstrInfo_bnd : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_bnd(Code code, string mnemonic) : this(code, mnemonic, InstrOpInfoFlags.None) { }

		public SimpleInstrInfo_bnd(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			var flags = this.flags;
			if (instr.HasPrefixRepne)
				flags |= InstrOpInfoFlags.BndPrefix;
			info = new InstrOpInfo(mnemonic, ref instr, flags);
		}
	}

	sealed class SimpleInstrInfo_pops : InstrInfo {
		readonly string mnemonic;
		readonly string[] pseudo_ops;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_pops(Code code, string mnemonic, string[] pseudo_ops) : this(code, mnemonic, pseudo_ops, InstrOpInfoFlags.None) { }

		public SimpleInstrInfo_pops(Code code, string mnemonic, string[] pseudo_ops, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.pseudo_ops = pseudo_ops;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = new InstrOpInfo(mnemonic, ref instr, flags);
			int imm = instr.Immediate8;
			if (options.UsePseudoOps && (uint)imm < (uint)pseudo_ops.Length) {
				info.OpCount--;
				info.Mnemonic = pseudo_ops[imm];
			}
		}
	}

	sealed class SimpleInstrInfo_pclmulqdq : InstrInfo {
		readonly string mnemonic;
		readonly string[] pseudo_ops;

		public SimpleInstrInfo_pclmulqdq(Code code, string mnemonic, string[] pseudo_ops)
			: base(code) {
			this.mnemonic = mnemonic;
			this.pseudo_ops = pseudo_ops;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = new InstrOpInfo(mnemonic, ref instr, InstrOpInfoFlags.None);
			if (options.UsePseudoOps) {
				int index;
				int imm = instr.Immediate8;
				if (imm == 0)
					index = 0;
				else if (imm == 1)
					index = 1;
				else if (imm == 0x10)
					index = 2;
				else if (imm == 0x11)
					index = 3;
				else
					index = -1;
				if (index >= 0) {
					info.Mnemonic = pseudo_ops[index];
					info.OpCount--;
				}
			}
		}
	}

	sealed class SimpleInstrInfo_imul : InstrInfo {
		readonly string mnemonic;
		readonly InstrOpInfoFlags flags;

		public SimpleInstrInfo_imul(Code code, string mnemonic) : this(code, mnemonic, InstrOpInfoFlags.None) { }

		public SimpleInstrInfo_imul(Code code, string mnemonic, InstrOpInfoFlags flags)
			: base(code) {
			this.mnemonic = mnemonic;
			this.flags = flags;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			info = new InstrOpInfo(mnemonic, ref instr, flags);
			Debug.Assert(info.OpCount == 3);
			if (options.UsePseudoOps && info.Op0Kind == InstrOpKind.Register && info.Op1Kind == InstrOpKind.Register && info.Op0Register == info.Op1Register) {
				info.OpCount--;
				info.Op1Kind = info.Op2Kind;
			}
		}
	}

	sealed class SimpleInstrInfo_Reg16 : InstrInfo {
		readonly string mnemonic;

		public SimpleInstrInfo_Reg16(Code code, string mnemonic)
			: base(code) {
			this.mnemonic = mnemonic;
		}

		public override void GetOpInfo(MasmFormatterOptions options, ref Instruction instr, out InstrOpInfo info) {
			const InstrOpInfoFlags flags = InstrOpInfoFlags.None;
			info = new InstrOpInfo(mnemonic, ref instr, flags);
			if (Register.EAX <= (Register)info.Op0Register && (Register)info.Op0Register <= Register.R15D)
				info.Op0Register = (byte)((Register)info.Op0Register - Register.EAX + Register.AX);
			if (Register.EAX <= (Register)info.Op1Register && (Register)info.Op1Register <= Register.R15D)
				info.Op1Register = (byte)((Register)info.Op1Register - Register.EAX + Register.AX);
		}
	}
}
#endif
