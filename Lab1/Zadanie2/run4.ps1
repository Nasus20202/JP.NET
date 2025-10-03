rm -r out4
mkdir out4

echo "a)"

csc.exe -t:library -out:out4/JP_NET.Lab1.Zadanie2.lib.dll ClassA.cs
csc.exe -t:exe -out:out4/JP_NET.Lab1.Zadanie2.exe -r:out4/JP_NET.Lab1.Zadanie2.lib.dll ProgA.cs
ildasm.exe out4/JP_NET.Lab1.Zadanie2.lib.dll /out:out4/JP_NET.Lab1.Zadanie2.lib.il

echo "b)"
(Get-Content "out4/JP_NET.Lab1.Zadanie2.lib.il" -Raw) `
    -replace 'Wydruk z klasy A\. \{0\}', 'Wydruk z klasy A po kompilacji. {0}' |
    Set-Content "out4/JP_NET.Lab1.Zadanie22.lib.il" -Encoding UTF8

echo "c)"
ilasm.exe out4/JP_NET.Lab1.Zadanie22.lib.il /dll /output:out4/JP_NET.Lab1.Zadanie2.lib.dll
out4/JP_NET.Lab1.Zadanie2.exe