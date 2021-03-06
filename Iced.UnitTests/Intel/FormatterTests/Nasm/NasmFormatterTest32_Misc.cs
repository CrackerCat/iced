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

#if !NO_NASM_FORMATTER && !NO_FORMATTER
using System.Collections.Generic;
using Xunit;

namespace Iced.UnitTests.Intel.FormatterTests.Nasm {
	public sealed class NasmFormatterTest32_Misc : FormatterTest {
		[Theory]
		[MemberData(nameof(Format_Data))]
		void Format(int index, InstructionInfo info, string formattedString) => FormatBase(index, info, formattedString, NasmFormatterFactory.Create());
		public static IEnumerable<object[]> Format_Data => GetFormatData(infos, formattedStrings);

		static readonly InstructionInfo[] infos = InstructionInfos32_Misc.AllInfos;
		static readonly string[] formattedStrings = new string[InstructionInfos32_Misc.AllInfos_Length] {
			"cs jo 0x7ffffff3",
			"cs jno 0x7ffffff3",
			"cs jb 0x7ffffff3",
			"cs jae 0x7ffffff3",
			"cs je 0x7ffffff3",
			"cs jne 0x7ffffff3",
			"cs jbe 0x7ffffff3",
			"cs ja 0x7ffffff3",
			"cs js 0x7ffffff3",
			"cs jns 0x7ffffff3",
			"cs jp 0x7ffffff3",
			"cs jnp 0x7ffffff3",
			"cs jl 0x7ffffff3",
			"cs jge 0x7ffffff3",
			"cs jle 0x7ffffff3",
			"cs jg 0x7ffffff3",
			"ds jo 0x7ffffff3",
			"ds jno 0x7ffffff3",
			"ds jb 0x7ffffff3",
			"ds jae 0x7ffffff3",
			"ds je 0x7ffffff3",
			"ds jne 0x7ffffff3",
			"ds jbe 0x7ffffff3",
			"ds ja 0x7ffffff3",
			"ds js 0x7ffffff3",
			"ds jns 0x7ffffff3",
			"ds jp 0x7ffffff3",
			"ds jnp 0x7ffffff3",
			"ds jl 0x7ffffff3",
			"ds jge 0x7ffffff3",
			"ds jle 0x7ffffff3",
			"ds jg 0x7ffffff3",
			"cs jo 0x7ffffff7",
			"cs jno 0x7ffffff7",
			"cs jb 0x7ffffff7",
			"cs jae 0x7ffffff7",
			"cs je 0x7ffffff7",
			"cs jne 0x7ffffff7",
			"cs jbe 0x7ffffff7",
			"cs ja 0x7ffffff7",
			"cs js 0x7ffffff7",
			"cs jns 0x7ffffff7",
			"cs jp 0x7ffffff7",
			"cs jnp 0x7ffffff7",
			"cs jl 0x7ffffff7",
			"cs jge 0x7ffffff7",
			"cs jle 0x7ffffff7",
			"cs jg 0x7ffffff7",
			"ds jo 0x7ffffff7",
			"ds jno 0x7ffffff7",
			"ds jb 0x7ffffff7",
			"ds jae 0x7ffffff7",
			"ds je 0x7ffffff7",
			"ds jne 0x7ffffff7",
			"ds jbe 0x7ffffff7",
			"ds ja 0x7ffffff7",
			"ds js 0x7ffffff7",
			"ds jns 0x7ffffff7",
			"ds jp 0x7ffffff7",
			"ds jnp 0x7ffffff7",
			"ds jl 0x7ffffff7",
			"ds jge 0x7ffffff7",
			"ds jle 0x7ffffff7",
			"ds jg 0x7ffffff7",
			"bnd jo 0x7ffffff3",
			"bnd jno 0x7ffffff3",
			"bnd jb 0x7ffffff3",
			"bnd jae 0x7ffffff3",
			"bnd je 0x7ffffff3",
			"bnd jne 0x7ffffff3",
			"bnd jbe 0x7ffffff3",
			"bnd ja 0x7ffffff3",
			"bnd js 0x7ffffff3",
			"bnd jns 0x7ffffff3",
			"bnd jp 0x7ffffff3",
			"bnd jnp 0x7ffffff3",
			"bnd jl 0x7ffffff3",
			"bnd jge 0x7ffffff3",
			"bnd jle 0x7ffffff3",
			"bnd jg 0x7ffffff3",
			"bnd jo 0x7ffffff7",
			"bnd jno 0x7ffffff7",
			"bnd jb 0x7ffffff7",
			"bnd jae 0x7ffffff7",
			"bnd je 0x7ffffff7",
			"bnd jne 0x7ffffff7",
			"bnd jbe 0x7ffffff7",
			"bnd ja 0x7ffffff7",
			"bnd js 0x7ffffff7",
			"bnd jns 0x7ffffff7",
			"bnd jp 0x7ffffff7",
			"bnd jnp 0x7ffffff7",
			"bnd jl 0x7ffffff7",
			"bnd jge 0x7ffffff7",
			"bnd jle 0x7ffffff7",
			"bnd jg 0x7ffffff7",
			"bnd jmp 0x7ffffff6",
			"bnd jmp [eax]",
			"bnd jmp eax",
			"bnd call 0x7ffffff6",
			"bnd call [eax]",
			"bnd call eax",
			"bnd ret 0",
			"bnd ret",
		};
	}
}
#endif
