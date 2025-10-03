rm -r out
mkdir out 

echo "b)" 
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie1.dll ClassA.cs
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie1.DEB.dll -debug ClassA.cs

echo "c)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie1.O.dll -o ClassA.cs

echo "d)"
ildasm.exe out/JP_NET.Lab1.Zadanie1.dll /out:out/JP_NET.Lab1.Zadanie1.il
ildasm.exe out/JP_NET.Lab1.Zadanie1.DEB.dll /out:out/JP_NET.Lab1.Zadanie1.DEB.il
ildasm.exe out/JP_NET.Lab1.Zadanie1.O.dll /out:out/JP_NET.Lab1.Zadanie1.O.il

diff (cat out/JP_NET.Lab1.Zadanie1.il) (cat out/JP_NET.Lab1.Zadanie1.DEB.il) > out/diff_debug.txt
diff (cat out/JP_NET.Lab1.Zadanie1.il) (cat out/JP_NET.Lab1.Zadanie1.O.il) > out/diff_optimized.txt

echo "e)"
csc.exe -t:library -out:out/JP_NET.Lab1.Zadanie1.2.dll ClassA2.cs
ildasm.exe out/JP_NET.Lab1.Zadanie1.2.dll /out:out/JP_NET.Lab1.Zadanie1.2.il

diff (cat out/JP_NET.Lab1.Zadanie1.il) (cat out/JP_NET.Lab1.Zadanie1.2.il) > out/diff_partial.txt