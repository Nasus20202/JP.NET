rm -r out
mkdir out

echo "b)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie2.lib.dll ClassA.cs
csc.exe -t:exe -out:out/JP_NET.Lab1.Zadanie2.exe -r:out/JP_NET.Lab1.Zadanie2.lib.dll ProgA.cs
out/JP_NET.Lab1.Zadanie2.exe

echo "c)"
ildasm.exe out/JP_NET.Lab1.Zadanie2.exe /out:out/JP_NET.Lab1.Zadanie2.il

echo "e)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie2.lib.dll ClassA.cs AssemblyInfo.cs
out/JP_NET.Lab1.Zadanie2.exe