# Ssq

ported [ssqcheck.php](https://github.com/pumpCurry/ssqcheck) to dotnet.

## reading SSQ/CSQ information command:

```cmd
ssqcheck.exe read --input filename.ssq
```

### option:
* --input (-i) : input chunk filename.
* --output (-o) : output filename. default NULL
* --nologo : no logo.
* --verbose (-v) : verbose. 

## reading SSQ/CSQ information. of dir command:

```cmd
ssqcheck.exe read-dir --input *.ssq --dir ./ssq-dir --outdir ./out-dir
```

### options:
* --input (-i) : input chunk file pattern.
* --dir (-d) : input directory. default `./`
* --outext (-o) : output ext. default `.txt`
* --skip (-s) : error skip.
* --nologo : no logo.
* --verbose (-v) : verbose.