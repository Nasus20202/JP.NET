rm -r out
mkdir out

echo "a)"
csc.exe -t:exe -out:out/ProgAB.exe ProgA.cs ClassA.cs ClassB.cs
out/ProgAB.exe

echo "b)"
csc.exe -t:module -out:out/ClassA.netmodule ClassA.cs
csc.exe -t:module -out:out/ClassB.netmodule ClassB.cs

echo "c)"
csc.exe -t:exe -out:out/ProgABmod.exe -addmodule:out/ClassA.netmodule,out/ClassB.netmodule ProgA.cs
out/ProgABmod.exe

mv out/ClassB.netmodule out/ClassB.netmodule.backup
echo "Test po zmianie nazwy ClassB.netmodule:"
out/ProgABmod.exe
mv out/ClassB.netmodule.backup out/ClassB.netmodule

echo "d)"
ildasm.exe out/ProgAB.exe /out:out/ProgAB.il
ildasm.exe out/ProgABmod.exe /out:out/ProgABmod.il
diff (cat out/ProgAB.il) (cat out/ProgABmod.il) > out/diff_prog.txt

echo "e)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie3.lib.dll ClassA.cs
csc.exe -t:module -out:out/ProgA.netmodule ProgA.cs -r:out/JP_NET.Lab1.Zadanie3.lib.dll

echo "f)"
al.exe /target:exe /main:Lab1.Zadanie3.Program.Main /out:out/ProgABal.exe out/ClassA.netmodule out/ClassB.netmodule out/ProgA.netmodule
out/ProgABal.exe

echo "g)"
ildasm.exe out/ProgABal.exe /out:out/ProgABal.il
diff (cat out/ProgAB.il) (cat out/ProgABal.il) > out/diff_al.txt