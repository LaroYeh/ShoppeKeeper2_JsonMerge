# ShoppeKeeper2_JsonMerge
Small Tool for merge languages.

My English is not good, especially at writing.
Please forgive me for what i wrote. QQ

# Environment
OS: Microsoft WIndows

# How to start
(Remember backup your file !)

1. Get application from [GoogleDrive](https://drive.google.com/open?id=1G0kXnMOwVAYunD7k7MZ10t7Wjm7AGXoM) or [Github](https://github.com/LaroYeh/ShoppeKeeper2_JsonMerge/tree/master/SK2_JsonMerge/bin/Release) 
2. Unzip the file and start it (SK2_JsonMerge.exe)
3. Click "..." button behind "Name From" to select reference file from others and click another "..." button behind "Value From" to select localizaion file that you created.
4. Click "Preview Result" for try merge those file.
5. If all goes well, you can click "Export" button at bottom or editing text. 

# Will cause ERROR:
1. Having any Single-Byte quote (") inside name/value.

| syntax | script |
| --- | --- |
| Error | "IAMNOTTHIS'' JEDI ''YOUARELOOKINGFOR.": "I am not this '' jedi '' you are looking for." |
| OK | "IAMNOTTHIS＂ JEDI ＂YOUARELOOKINGFOR.": "I am not this ＂ jedi ＂ you are looking for." |

## How to avoid error? 
1. Using any tool you like (e.g. http://json.parser.online.fr/)
2. Replace any Single-Byte quote (") to Double-Byte (＂) or others word.
3. Save it and try merge again.
