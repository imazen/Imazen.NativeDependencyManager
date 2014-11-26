//
// Image.cs
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

using Mono;

using RVA = System.UInt32;

namespace Mono.Cecil.PE {

	public sealed class Image {

		public ModuleKind Kind;
		public string RuntimeVersion;

        public TargetRuntime ParseRuntime
        {
            get
            {
                if (RuntimeVersion == null) return TargetRuntime.NotDotNet;
                switch (RuntimeVersion[1])
                {
                    case '1':
                        return RuntimeVersion[3] == '0'
                            ? TargetRuntime.Net_1_0
                            : TargetRuntime.Net_1_1;
                    case '2':
                        return TargetRuntime.Net_2_0;
                    case '4':
                    default:
                        return TargetRuntime.Net_4_0;
                }
            }
        }
		public TargetArchitecture Architecture;
		public ModuleCharacteristics Characteristics;
		public string FileName;

		public Section [] Sections;

		public Section MetadataSection;

		public uint EntryPointToken;
		public ModuleAttributes Attributes;

		public DataDirectory Debug;
		public DataDirectory Resources;
		public DataDirectory StrongName;


		readonly int [] coded_index_sizes = new int [13];

		
		public Image ()
		{
		
		}

		public uint ResolveVirtualAddress (RVA rva)
		{
			var section = GetSectionAtVirtualAddress (rva);
			if (section == null)
				throw new ArgumentOutOfRangeException ();

			return ResolveVirtualAddressInSection (rva, section);
		}

		public uint ResolveVirtualAddressInSection (RVA rva, Section section)
		{
			return rva + section.PointerToRawData - section.VirtualAddress;
		}

		public Section GetSection (string name)
		{
			var sections = this.Sections;
			for (int i = 0; i < sections.Length; i++) {
				var section = sections [i];
				if (section.Name == name)
					return section;
			}

			return null;
		}

		public Section GetSectionAtVirtualAddress (RVA rva)
		{
			var sections = this.Sections;
			for (int i = 0; i < sections.Length; i++) {
				var section = sections [i];
				if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.SizeOfRawData)
					return section;
			}

			return null;
		}

		public ImageDebugDirectory GetDebugHeader (out byte [] header)
		{
			var section = GetSectionAtVirtualAddress (Debug.VirtualAddress);
			var buffer = new ByteBuffer (section.Data);
			buffer.position = (int) (Debug.VirtualAddress - section.VirtualAddress);

			var directory = new ImageDebugDirectory {
				Characteristics = buffer.ReadInt32 (),
				TimeDateStamp = buffer.ReadInt32 (),
				MajorVersion = buffer.ReadInt16 (),
				MinorVersion = buffer.ReadInt16 (),
				Type = buffer.ReadInt32 (),
				SizeOfData = buffer.ReadInt32 (),
				AddressOfRawData = buffer.ReadInt32 (),
				PointerToRawData = buffer.ReadInt32 (),
			};

			if (directory.SizeOfData == 0 || directory.PointerToRawData == 0) {
				header = Empty<byte>.Array;
				return directory;
			}

			buffer.position = (int) (directory.PointerToRawData - section.PointerToRawData);

			header = new byte [directory.SizeOfData];
			Buffer.BlockCopy (buffer.buffer, buffer.position, header, 0, header.Length);

			return directory;
		}
	}
}
