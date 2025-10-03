rm -r out
mkdir out

echo "a)"
sn.exe -k out/paraKluczy.snk

echo "c)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie6.lib.dll ClassA.cs AssemblyInfo.cs

echo "d)"
csc.exe -t:library -out:out/lib_unsigned.dll ClassA.cs
csc.exe -t:exe -out:out/ProgA_old.exe -r:out/lib_unsigned.dll ProgA.cs
Copy-Item out/JP_NET.Lab1.Zadanie6.lib.dll out/lib_unsigned.dll -Force
echo "Proba uruchomienia:"
out/ProgA_old.exe

echo "e)"
csc.exe -t:exe -out:out/ProgA_new.exe -r:out/JP_NET.Lab1.Zadanie6.lib.dll ProgA.cs
out/ProgA_new.exe

echo "f)"
sn.exe -v out/JP_NET.Lab1.Zadanie6.lib.dll
sn.exe -v out/ProgA_new.exe
