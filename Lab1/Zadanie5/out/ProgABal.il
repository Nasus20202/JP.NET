
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.8.3928.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.module extern ProgA.netmodule
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern System
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern JP_NET.Lab1.Zadanie3.lib
{
  .ver 0:0:0:0
}
.assembly ProgABal
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.file ClassA.netmodule
    .hash = (D7 F7 42 68 BD 0A 89 20 2F C2 AD 3A 0E 9A B3 6B   // ..Bh... /..:...k
             C9 8E B8 29 )                                     // ...)
.file ClassB.netmodule
    .hash = (DF E6 36 06 52 12 AD F9 F8 55 FA 8A 11 38 EE B2   // ..6.R....U...8..
             07 4E 03 04 )                                     // .N..
.file ProgA.netmodule
    .hash = (38 27 FD CE 68 37 A6 77 6C 93 CC 1B A0 45 3A 5C   // 8'..h7.wl....E:\
             A7 ED 0C 92 ) 
.class extern public Lab1.Zadanie3.ClassA
{
  .file ClassA.netmodule
  .class 0x02000002
}
.class extern public Lab1.Zadanie3.ClassB
{
  .file ClassB.netmodule
  .class 0x02000002
}
.module ProgABal.exe
// MVID: {CBD3AA45-0C87-4CB1-BBFB-7353ED4ACE7F}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x055F0000


// ================== GLOBAL METHODS =========================

.method privatescope static void  __EntryPoint$PST06000001() cil managed
{
  .entrypoint
  // Code size       8 (0x8)
  .maxstack  8
  IL_0000:  tail.
  IL_0002:  call       void [.module ProgA.netmodule]Lab1.Zadanie3.Program::Main()
  IL_0007:  ret
} // end of global method __EntryPoint


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file out/ProgABal.res
