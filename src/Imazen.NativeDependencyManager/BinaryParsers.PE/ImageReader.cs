//
// ImageReader.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2011 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;


using RVA = System.UInt32;

namespace Mono.Cecil.PE {



	public sealed class ImageReader : BinaryStreamReader {

		readonly Image image;

		DataDirectory cli;
		DataDirectory metadata;

        bool RequireCLI = false;

		public ImageReader (Stream stream)
			: base (stream)
		{
			image = new Image ();

			image.FileName = stream.GetFullyQualifiedName ();
		}

		void MoveTo (DataDirectory directory)
		{
			BaseStream.Position = image.ResolveVirtualAddress (directory.VirtualAddress);
		}

		void MoveTo (uint position)
		{
			BaseStream.Position = position;
		}

		void ReadImage ()
		{
			if (BaseStream.Length < 128)
				throw new BadImageFormatException ();

			// - DOSHeader

			// PE					2
			// Start				58
			// Lfanew				4
			// End					64

			if (ReadUInt16 () != 0x5a4d)
				throw new BadImageFormatException ();

			Advance (58);

			MoveTo (ReadUInt32 ());

			if (ReadUInt32 () != 0x00004550)
				throw new BadImageFormatException ();

			// - PEFileHeader

			// Machine				2
			image.Architecture = ReadArchitecture ();

			// NumberOfSections		2
			ushort sections = ReadUInt16 ();

			// TimeDateStamp		4
			// PointerToSymbolTable	4
			// NumberOfSymbols		4
			// OptionalHeaderSize	2
			Advance (14);

			// Characteristics		2
			ushort characteristics = ReadUInt16 ();

			ushort subsystem, dll_characteristics;
			ReadOptionalHeaders (out subsystem, out dll_characteristics);
			ReadSections (sections);
			if (!cli.IsZero) ReadCLIHeader ();
			if (!metadata.IsZero) ReadMetadata ();

			image.Kind = GetModuleKind (characteristics, subsystem);
			image.Characteristics = (ModuleCharacteristics) dll_characteristics;
		}

		TargetArchitecture ReadArchitecture ()
		{
			var machine = ReadUInt16 ();
			switch (machine) {
			case 0x014c:
				return TargetArchitecture.I386;
			case 0x8664:
				return TargetArchitecture.AMD64;
			case 0x0200:
				return TargetArchitecture.IA64;
			case 0x01c4:
				return TargetArchitecture.ARMv7;
			}

			throw new NotSupportedException ();
		}

		static ModuleKind GetModuleKind (ushort characteristics, ushort subsystem)
		{
			if ((characteristics & 0x2000) != 0) // ImageCharacteristics.Dll
				return ModuleKind.Dll;

			if (subsystem == 0x2 || subsystem == 0x9) // SubSystem.WindowsGui || SubSystem.WindowsCeGui
				return ModuleKind.Windows;

			return ModuleKind.Console;
		}

		void ReadOptionalHeaders (out ushort subsystem, out ushort dll_characteristics)
		{
			// - PEOptionalHeader
			//   - StandardFieldsHeader

			// Magic				2
			bool pe64 = ReadUInt16 () == 0x20b;

			//						pe32 || pe64

			// LMajor				1
			// LMinor				1
			// CodeSize				4
			// InitializedDataSize	4
			// UninitializedDataSize4
			// EntryPointRVA		4
			// BaseOfCode			4
			// BaseOfData			4 || 0

			//   - NTSpecificFieldsHeader

			// ImageBase			4 || 8
			// SectionAlignment		4
			// FileAlignement		4
			// OSMajor				2
			// OSMinor				2
			// UserMajor			2
			// UserMinor			2
			// SubSysMajor			2
			// SubSysMinor			2
			// Reserved				4
			// ImageSize			4
			// HeaderSize			4
			// FileChecksum			4
			Advance (66);

			// SubSystem			2
			subsystem = ReadUInt16 ();

			// DLLFlags				2
			dll_characteristics = ReadUInt16 ();
			// StackReserveSize		4 || 8
			// StackCommitSize		4 || 8
			// HeapReserveSize		4 || 8
			// HeapCommitSize		4 || 8
			// LoaderFlags			4
			// NumberOfDataDir		4

			//   - DataDirectoriesHeader

			// ExportTable			8
			// ImportTable			8
			// ResourceTable		8
			// ExceptionTable		8
			// CertificateTable		8
			// BaseRelocationTable	8

			Advance (pe64 ? 88 : 72);

			// Debug				8
			image.Debug = ReadDataDirectory ();

			// Copyright			8
			// GlobalPtr			8
			// TLSTable				8
			// LoadConfigTable		8
			// BoundImport			8
			// IAT					8
			// DelayImportDescriptor8
			Advance (56);

			// CLIHeader			8
			cli = ReadDataDirectory ();

            if (cli.IsZero && RequireCLI)
				throw new BadImageFormatException ();

			// Reserved				8
			Advance (8);
		}


		string ReadZeroTerminatedString (int length)
		{
			int read = 0;
			var buffer = new char [length];
			var bytes = ReadBytes (length);
			while (read < length) {
				var current = bytes [read];
				if (current == 0)
					break;

				buffer [read++] = (char) current;
			}

			return new string (buffer, 0, read);
		}

		void ReadSections (ushort count)
		{
			var sections = new Section [count];

			for (int i = 0; i < count; i++) {
				var section = new Section ();

				// Name
				section.Name = ReadZeroTerminatedString (8);

				// VirtualSize		4
				Advance (4);

				// VirtualAddress	4
				section.VirtualAddress = ReadUInt32 ();
				// SizeOfRawData	4
				section.SizeOfRawData = ReadUInt32 ();
				// PointerToRawData	4
				section.PointerToRawData = ReadUInt32 ();

				// PointerToRelocations		4
				// PointerToLineNumbers		4
				// NumberOfRelocations		2
				// NumberOfLineNumbers		2
				// Characteristics			4
				Advance (16);

				sections [i] = section;

				ReadSectionData (section);
			}

			image.Sections = sections;
		}

		void ReadSectionData (Section section)
		{
			var position = BaseStream.Position;

			MoveTo (section.PointerToRawData);

			var length = (int) section.SizeOfRawData;
			var data = new byte [length];
			int offset = 0, read;

			while ((read = Read (data, offset, length - offset)) > 0)
				offset += read;

			section.Data = data;

			BaseStream.Position = position;
		}

		void ReadCLIHeader ()
		{
			MoveTo (cli);

			// - CLIHeader

			// Cb						4
			// MajorRuntimeVersion		2
			// MinorRuntimeVersion		2
			Advance (8);

			// Metadata					8
			metadata = ReadDataDirectory ();
			// Flags					4
			image.Attributes = (ModuleAttributes) ReadUInt32 ();
			// EntryPointToken			4
			image.EntryPointToken = ReadUInt32 ();
			// Resources				8
			image.Resources = ReadDataDirectory ();
			// StrongNameSignature		8
			image.StrongName = ReadDataDirectory ();
			// CodeManagerTable			8
			// VTableFixups				8
			// ExportAddressTableJumps	8
			// ManagedNativeHeader		8
		}

		void ReadMetadata ()
		{
			MoveTo (metadata);

			if (ReadUInt32 () != 0x424a5342)
				throw new BadImageFormatException ();

			// MajorVersion			2
			// MinorVersion			2
			// Reserved				4
			Advance (8);

			var s = ReadZeroTerminatedString (ReadInt32 ());
            image.DotNetRuntimeVersionString = s;
            image.DotNetRuntime = ParseDotNetRuntimeVersion(s);

			// Flags		2
			Advance (2);

			var streams = ReadUInt16 ();

			var section = image.GetSectionAtVirtualAddress (metadata.VirtualAddress);
			if (section == null)
				throw new BadImageFormatException ();

			image.MetadataSection = section;

		}


        private TargetRuntime ParseDotNetRuntimeVersion(string s)
        {
            if (string.IsNullOrEmpty(s)) return TargetRuntime.NotDotNet;
            switch (s[1])
            {
                case '1':
                    return s[3] == '0'
                        ? TargetRuntime.Net_1_0
                        : TargetRuntime.Net_1_1;
                case '2':
                    return TargetRuntime.Net_2_0;
                case '4':
                default:
                    return TargetRuntime.Net_4_0;
            }
            
        }

		public static Image ReadImageFrom (Stream stream)
		{
			try {
				var reader = new ImageReader (stream);
				reader.ReadImage ();
				return reader.image;
			} catch (EndOfStreamException e) {
				throw new BadImageFormatException (stream.GetFullyQualifiedName (), e);
			}
		}
	}
}
