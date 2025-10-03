
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
    .hash = (E4 15 D4 3B 7A E3 86 31 8C 07 CE FD B4 56 63 2E   // ...;z..1.....Vc.
             2C 4C B0 64 )                                     // ,L.d
.file ClassB.netmodule
    .hash = (E4 AC B1 E0 69 47 C9 DC 3A 1A B7 B5 F4 39 9D 0A   // ....iG..:....9..
             6F 42 E9 2C )                                     // oB.,
.file ProgA.netmodule
    .hash = (51 A8 CF E9 D0 75 BA 87 27 6C 7C C3 A2 88 E4 85   // Q....u..'l|.....
             F2 DE 76 1F )                                     // ..v.
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
// MVID: {1680F538-7924-4CA2-B304-54B8659C5F88}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x05900000


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
