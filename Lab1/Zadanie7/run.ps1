rm -r out
mkdir out

echo "a)"
sn.exe -k out/paraKluczy.snk
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie7.lib_v1.dll ClassA.cs AssemblyInfo.cs
gacutil.exe -i out/JP_NET.Lab1.Zadanie7.lib_v1.dll

echo "b)"
Copy-Item AssemblyInfo.cs out/AssemblyInfo_original.cs
(Get-Content AssemblyInfo.cs) -replace '1.0.0.0', '1.0.0.1' | Set-Content AssemblyInfo_new.cs
Move-Item AssemblyInfo_new.cs AssemblyInfo.cs -Force

Copy-Item ClassA.cs out/ClassA_original.cs
(Get-Content ClassA.cs) -replace 'Wydruk z.*klasy A\. \{0\}', 'Wydruk z nowej klasy A. {0}' | Set-Content ClassA_new.cs
Move-Item  ClassA_new.cs ClassA.cs -Force

echo "c)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie7.lib_v2.dll ClassA.cs AssemblyInfo.cs

echo "d)"
csc.exe -t:exe -out:out/ProgA_v1.exe -r:out/JP_NET.Lab1.Zadanie7.lib_v1.dll ProgA.cs

echo "e)"
gacutil.exe -i out/JP_NET.Lab1.Zadanie7.lib_v2.dll

echo "f)"
csc.exe -t:exe -out:out/ProgA_v2.exe -r:out/JP_NET.Lab1.Zadanie7.lib_v2.dll ProgA.cs
Remove-Item out/JP_NET.Lab1.Zadanie7.lib_v*.dll

echo "g)"
gacutil.exe /l JP_NET.Lab1.Zadanie7.lib_v1
gacutil.exe /l JP_NET.Lab1.Zadanie7.lib_v2

echo "h)"
out/ProgA_v1.exe
out/ProgA_v2.exe

echo "i)"
dir "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\JP_NET.Lab1.Zadanie7.lib_*\" -ErrorAction SilentlyContinue

echo "j)"
gacutil.exe -u JP_NET.Lab1.Zadanie7.lib_v1
gacutil.exe -u JP_NET.Lab1.Zadanie7.lib_v2
gacutil.exe /l JP_NET.Lab1.Zadanie7.lib_v1
gacutil.exe /l JP_NET.Lab1.Zadanie7.lib_v2

Copy-Item out/AssemblyInfo_original.cs AssemblyInfo.cs -Force
Copy-Item out/ClassA_original.cs ClassA.cs -Force
