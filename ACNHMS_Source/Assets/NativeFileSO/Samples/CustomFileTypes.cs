// 	Copyright (c) 2019 Keiwan Donyagard
// 
//  This Source Code Form is subject to the terms of the Mozilla Public
//  License, v. 2.0. If a copy of the MPL was not distributed with this
//  file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Keiwando.NFSO;

namespace Keiwando.NFSO.Samples { 

	public class CustomFileTypes {

		public static readonly SupportedFileType evol = new SupportedFileType {
			Name = "Evolution Save File",
			Extension = "evol",
			Owner = true,
			AppleUTI = "com.keiwando.Evolution.evol",
			AppleConformsToUTI = "public.plain-text",
			MimeType = "application/octet-stream"
		};

		public static readonly SupportedFileType creat = new SupportedFileType {
			Name = "Evolution Creature Save File",
			Extension = "creat",
			Owner = true,
			AppleUTI = "com.keiwando.Evolution.creat",
			AppleConformsToUTI = "public.plain-text",
			MimeType = "application/octet-stream"
		};
	}
}
