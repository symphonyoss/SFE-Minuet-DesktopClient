using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Paragon.Plugins;

namespace Symphony.Crypto
    {
        [JavaScriptPlugin(Name = "appbridge.CryptoLib", IsBrowserSide = true)]
        public class CryptoPlugin
        {
            [JavaScriptPluginMember(Name = "AESGCMEncrypt")]
            public string AESGCMEncrypt(
                string Base64IV,
                string Base64AAD,
                string Base64Key,
                string Base64In)
            {
                return this.EncryptDecrypt("AESGCMEncrypt", Base64IV, Base64AAD, Base64Key, Base64In);
            }

            [JavaScriptPluginMember(Name = "AESGCMDecrypt")]
            public string AESGCMDecrypt(
                string Base64IV,
                string Base64AAD,
                string Base64Key,
                string Base64In)
            {
                return this.EncryptDecrypt("AESGCMDecrypt", Base64IV, Base64AAD, Base64Key, Base64In);
            }

            [JavaScriptPluginMember(Name = "PBKDF2")]
            public string PBKDF2(string Base64Salt, int Iterations, string StrPW)
            {
                // For a 256-bit key,
                int OutLen = 32;

                // Convert encoded buffers to raw data.
                byte[] Salt = Convert.FromBase64String(Base64Salt);
                byte[] PW = System.Text.Encoding.UTF8.GetBytes(StrPW);

                // Marshal unmamanged play space for native execution.
                int MarshalLen = OutLen + Salt.Length + PW.Length;
                IntPtr Space = Marshal.AllocHGlobal(MarshalLen);
                if (Space == null)
                {
                    throw new Exception("Marshal.AllocHGlobal of length " + MarshalLen + " failed. (out of memory?)");
                }

                // Copy  raw data into unsafe (unmanaged) play space.
                IntPtr SaltPtr = Space;
                Marshal.Copy(Salt, 0, SaltPtr, Salt.Length);
                int CurOffset = Salt.Length;

                IntPtr PWPtr = new IntPtr(Space.ToInt64() + CurOffset);
                Marshal.Copy(PW, 0, PWPtr, PW.Length);
                CurOffset += PW.Length;

                IntPtr OutPtr = new IntPtr(Space.ToInt64() + CurOffset);

                int Ret = UnsafeNativeMethods.CL_PBKDF2(PWPtr, System.Convert.ToUInt32(PW.Length), SaltPtr, System.Convert.ToUInt32(Salt.Length), Iterations, OutPtr, System.Convert.ToUInt32(OutLen));
                if (Ret != 0)
                {
                    Marshal.FreeHGlobal(Space);
                    throw new Exception("PBKDF2 failed with exit code: " + Ret);
                }

                // Copy the output to a byte array.
                byte[] Out = new byte[OutLen];
                Marshal.Copy(OutPtr, Out, 0, OutLen);

                // Encode in Base64.
                string OutBase64Encoded = System.Convert.ToBase64String(Out, 0, OutLen);

                Marshal.FreeHGlobal(Space);

                return OutBase64Encoded;
            }

            [JavaScriptPluginMember(Name = "SHA256Digest")]
            public string SHA256Digest(string Base64In)
            {
                // For a 256-bit hash,
                int OutLen = 32;

                // Convert encoded buffers to raw data.
                byte[] In = System.Text.Encoding.UTF8.GetBytes(Base64In);

                // Marshal unmamanged play space for native execution.
                int MarshalLen = OutLen + In.Length;
                IntPtr Space = Marshal.AllocHGlobal(MarshalLen);
                if (Space == null)
                {
                    throw new Exception("Marshal.AllocHGlobal of length " + MarshalLen + " failed. (out of memory?)");
                }

                // Copy raw data into unsafe (unmanaged) play space.
                IntPtr InPtr = Space;
                Marshal.Copy(In, 0, InPtr, In.Length);
                int CurOffset = In.Length;

                IntPtr OutPtr = new IntPtr(Space.ToInt64() + CurOffset);

                int Ret = UnsafeNativeMethods.CL_SHA2x256(InPtr, System.Convert.ToUInt32(In.Length), OutPtr);
                if (Ret != 0)
                {
                    Marshal.FreeHGlobal(Space);
                    throw new Exception("SHA256Digest failed with exit code: " + Ret);
                }

                // Copy the output to a byte array.
                byte[] Out = new byte[OutLen];
                Marshal.Copy(OutPtr, Out, 0, OutLen);

                // Encode in Base64.
                string OutBase64Encoded = System.Convert.ToBase64String(Out, 0, OutLen);

                Marshal.FreeHGlobal(Space);

                return OutBase64Encoded;
            }

            [JavaScriptPluginMember(Name = "RSAGenerateKeyPair")]
            public string RSAGenerateKeyPair(string Base64Seed, int KeyLen, string StrCallback)
            {
                byte[] Seed = Convert.FromBase64String(Base64Seed);

                if (KeyLen < 1024 || KeyLen > 16384)
                {
                    throw new Exception("KeyLen of " + KeyLen + " not supported [1024, 16384]");
                }

                // Verify that we're not implicitly tuncating the key space with a shitty seed.
                if ((KeyLen / 8) < Seed.Length)
                {
                    throw new Exception("In RSAGenerateKeyPair, the a seed of len " + Seed.Length + " is too short for a key of bit length " + KeyLen);
                }

                // Construct a new RSA key.
                IntPtr SeedPtr = Marshal.AllocHGlobal(Seed.Length);
                Marshal.Copy(Seed, 0, SeedPtr, Seed.Length);
                IntPtr RSAKeyPair = UnsafeNativeMethods.CL_initRSAKeyPair(SeedPtr, System.Convert.ToUInt32(Seed.Length), KeyLen);
                Marshal.FreeHGlobal(SeedPtr);

                // Serialize it to a C str.
                IntPtr PubKeyPEM = UnsafeNativeMethods.CL_serializeRSAPubKey(RSAKeyPair);
                if (PubKeyPEM == null)
                {
                    throw new Exception("In RSAGenerateKeyPair, failed to serialize RSAPubKey");
                }

                IntPtr PrivKeyPEM = UnsafeNativeMethods.CL_serializeRSAKeyPair(RSAKeyPair);
                if (PrivKeyPEM == null)
                {
                    throw new Exception("In RSAGenerateKeyPair, failed to serialize RSAPrivKey");
                }

                // Get the C str's length.
                Int32 PubKeyPEMLen = UnsafeNativeMethods.CL_getCStrLen(PubKeyPEM);
                Int32 PrivKeyPEMLen = UnsafeNativeMethods.CL_getCStrLen(PrivKeyPEM);

                // Construct strings of the keys.
                byte[] PubKeyPEMBytes = new byte[PubKeyPEMLen];
                Marshal.Copy(PubKeyPEM, PubKeyPEMBytes, 0, PubKeyPEMLen);
                string PubKeyPEMStr = System.Text.Encoding.UTF8.GetString(PubKeyPEMBytes);

                byte[] PrivKeyPEMBytes = new byte[PrivKeyPEMLen];
                Marshal.Copy(PrivKeyPEM, PrivKeyPEMBytes, 0, PrivKeyPEMLen);
                string PrivKeyPEMStr = System.Text.Encoding.UTF8.GetString(PrivKeyPEMBytes);

                // Free the C str.
                UnsafeNativeMethods.CL_freeBuf(PubKeyPEM);
                UnsafeNativeMethods.CL_freeBuf(PrivKeyPEM);

                // Constrcut the return JSON string.
                Dictionary<string, string> Root = new Dictionary<string, string>();
                Root["privateKey"] = PrivKeyPEMStr;
                Root["publicKey"] = PubKeyPEMStr;
                return String.Format("{{\"privateKey\":\"{0}\",\"publicKey\":\"{1}\"}}", PrivKeyPEMStr, PubKeyPEMStr);
            }

            [JavaScriptPluginMember(Name = "RSAEncrypt")]
            public string RSAEncrypt(string PEMKey, string InputStr)
            {
                return this.RSACryptDecrypt("RSAEncrypt", PEMKey, InputStr);
            }

            [JavaScriptPluginMember(Name = "RSADecrypt")]
            public string RSADecrypt(string PEMKey, string InputStr)
            {
                return this.RSACryptDecrypt("RSADecrypt", PEMKey, InputStr); ;
            }

            [JavaScriptPluginMember(Name = "RSASign")]
            public string RSASign(string PEMKey, string InputStr)
            {
                IntPtr RSAKey = getRSAKeyFromPEM(PEMKey);
                if (RSAKey == null)
                {
                    throw new Exception("In RSASign, Failed to parse RSA PEM formatted key: " + PEMKey);
                }

                byte[] Input = System.Text.Encoding.UTF8.GetBytes(InputStr);

                uint OutLen = UnsafeNativeMethods.CL_getRSAKeySize(RSAKey);
                IntPtr InputPtr = Marshal.AllocHGlobal(Input.Length + System.Convert.ToInt32(OutLen));
                Marshal.Copy(Input, 0, InputPtr, Input.Length);
                IntPtr OutPtr = new IntPtr(InputPtr.ToInt64() + Input.Length);

                uint Ret = UnsafeNativeMethods.CL_signRSA(RSAKey, 0, InputPtr, System.Convert.ToUInt32(Input.Length), OutPtr, OutLen);

                // Copy the output to a byte array.
                byte[] Out = new byte[OutLen];
                Marshal.Copy(OutPtr, Out, 0, System.Convert.ToInt32(OutLen));
                Marshal.FreeHGlobal(InputPtr);

                // Encode signature in base64.
                string OutStr = System.Convert.ToBase64String(Out, 0, System.Convert.ToInt32(OutLen));
                return OutStr;
            }

            [JavaScriptPluginMember(Name = "RSAVerify")]
            public bool RSAVerify(string PEMKey, String SigStr, string InputStr)
            {
                // Get our inputs.

                byte[] Input = System.Text.Encoding.UTF8.GetBytes(InputStr);


                IntPtr RSAKey = getRSAKeyFromPEM(PEMKey);
                if (RSAKey == null)
                {
                    throw new Exception("In RSAVerify, Failed to parse RSA PEM formatted key: " + PEMKey);
                }

                byte[] SigBytes = System.Convert.FromBase64String(SigStr);

                int RSAKeyLen = System.Convert.ToInt32(UnsafeNativeMethods.CL_getRSAKeySize(RSAKey));
                if (RSAKeyLen != SigBytes.Length)
                {
                    Marshal.FreeHGlobal(RSAKey);
                    throw new Exception("In RSAVerify, signature of length " + SigBytes.Length + " is a bad length for key length of " + RSAKeyLen);
                }

                // Copy to unmanaged memory.
                IntPtr InputPtr = Marshal.AllocHGlobal(Input.Length + SigBytes.Length);
                Marshal.Copy(Input, 0, InputPtr, Input.Length);
                IntPtr SigPtr = new IntPtr(InputPtr.ToInt64() + Input.Length);
                Marshal.Copy(SigBytes, 0, SigPtr, SigBytes.Length);

                uint Ret = UnsafeNativeMethods.CL_verifyRSA(RSAKey, 0, SigPtr, System.Convert.ToUInt32(SigBytes.Length), InputPtr, System.Convert.ToUInt32(Input.Length));

                Marshal.FreeHGlobal(RSAKey);
                Marshal.FreeHGlobal(InputPtr);

                if (Ret != 0 && Ret != 1)
                {
                    throw new Exception("In RSAVerify, an error occured, Error code: " + Ret);
                }

                return (Ret == 0);
            }

            [JavaScriptPluginMember(Name = "SecureGetItem")]
            public string SecureGetItem(uint BufID)
            {
                uint ret = doSecurePersistenceInit(DefaultSeed);
                if (ret != 0)
                {
                    throw new Exception("In, SecureGetItem, failed to init secure persistence, err code: " + ret);
                }

                IntPtr RetSize = Marshal.AllocHGlobal(4);
                if (RetSize.ToInt64() == 0)
                {
                    throw new Exception("In, SecureGetItem, malloc failed.");
                }

                // Invoke secure retrieval.
                IntPtr Ret = UnsafeNativeMethods.CL_SecureRetrieve(BufID, RetSize);

                if (Ret.ToInt64() == 0)
                {
                    Marshal.FreeHGlobal(RetSize);
                    throw new Exception("SecureGetItem, failed to retrieve anything.");
                }

                uint LenOfRet = (uint)Marshal.PtrToStructure(RetSize, typeof(uint));

                Marshal.FreeHGlobal(RetSize);

                // Construct strings of the keys.
                byte[] RetrievedValBytes = new byte[LenOfRet];
                Marshal.Copy(Ret, RetrievedValBytes, 0, System.Convert.ToInt32(LenOfRet));
                string RetrievedValStr = System.Text.Encoding.UTF8.GetString(RetrievedValBytes);

                UnsafeNativeMethods.CL_SecureFree(Ret, LenOfRet);

                return RetrievedValStr;
            }

            [JavaScriptPluginMember(Name = "SecureSetItem")]
            public void SecureSetItem(uint BufID, string ValStr)
            {
                // Init secure persistence.
                uint ret = doSecurePersistenceInit(DefaultSeed);
                if (ret != 0)
                {
                    throw new Exception("In, SecureSetItem, failed to init secure persistence, err code: " + ret);
                }

                byte[] ValBytes = System.Text.Encoding.UTF8.GetBytes(ValStr);

                // Copy to unmanaged memory.
                IntPtr ValPtr = Marshal.AllocHGlobal(ValBytes.Length);
                Marshal.Copy(ValBytes, 0, ValPtr, ValBytes.Length);

                uint Ret = UnsafeNativeMethods.CL_SecurePersist(BufID, ValPtr, System.Convert.ToUInt32(ValBytes.Length));

                Marshal.FreeHGlobal(ValPtr);

                if (Ret != 0)
                {
                    throw new Exception("In SecureSetItem, an error occured, Error code: " + Ret);

                }
            }

            [JavaScriptPluginMember(Name = "SecureEraseItem")]
            public void SecureEraseItem(uint BufID)
            {
                uint ret = doSecurePersistenceInit(DefaultSeed);
                if (ret != 0)
                {
                    throw new Exception("In, SecureEraseItem, failed to init secure persistence, err code: " + ret);

                }

                // Invoke secure erase.
                uint Ret = UnsafeNativeMethods.CL_SecureDelete(BufID);

                if (Ret != 0)
                {
                    throw new Exception("SecureEraseItem, failed with error code: " + Ret);
                }
            }

            // private helper functions
            private string EncryptDecrypt(string name,
               string Base64IV,
               string Base64AAD,
               string Base64Key,
               string Base64In)
            {
                uint TagLen = 16;

                // ToDo: temporary fix for SFE-1088
                if (Base64In == null)
                    Base64In = "";

                // Convert base64 encoded buffers to raw data.
                byte[] IV = Convert.FromBase64String(Base64IV);
                byte[] AAD = Convert.FromBase64String(Base64AAD);
                byte[] Key = Convert.FromBase64String(Base64Key);
                byte[] In = Convert.FromBase64String(Base64In);

                // Encryption output includes ciphertext + tag.
                int OutLen = In.Length + System.Convert.ToInt32(TagLen);
                // Decryption output is only plaintext.
                if (name == "AESGCMDecrypt")
                    OutLen = In.Length - System.Convert.ToInt32(TagLen);

                // Create unmanaged memory for C call.
                int MarshalLen = OutLen + IV.Length + AAD.Length + Key.Length + In.Length + System.Convert.ToInt32(TagLen);
                IntPtr Space = Marshal.AllocHGlobal(MarshalLen);
                if (Space == null)
                {
                    throw new Exception("Marshal.AllocHGlobal of length " + MarshalLen + " failed. (out of memory?)");
                }

                // Copy from buffers from managed memory to C-safe unmanaged memory.
                // Save a pointer to each component copied buffer's location.
                Marshal.Copy(IV, 0, Space, IV.Length);
                IntPtr IVPtr = Space;
                int CurOffset = IV.Length;

                IntPtr AADPtr = new IntPtr(Space.ToInt64() + CurOffset);
                Marshal.Copy(AAD, 0, AADPtr, AAD.Length);
                CurOffset += AAD.Length;

                IntPtr KeyPtr = new IntPtr(Space.ToInt64() + CurOffset);
                Marshal.Copy(Key, 0, KeyPtr, Key.Length);
                CurOffset += Key.Length;

                IntPtr InPtr = new IntPtr(Space.ToInt64() + CurOffset);
                Marshal.Copy(In, 0, InPtr, In.Length);
                CurOffset += In.Length;

                IntPtr OutPtr = new IntPtr(Space.ToInt64() + CurOffset);
                int OutBufOffset = CurOffset;
                CurOffset += OutLen;

                // Different places on encrypt / decrypt.
                IntPtr Tag;

                int Ret = 0;
                if (name == "AESGCMEncrypt")
                {
                    Tag = new IntPtr(OutPtr.ToInt64() + In.Length);
                    Ret = UnsafeNativeMethods.CL_AESEncryptGCM(InPtr, In.Length, AADPtr, AAD.Length, KeyPtr, IVPtr, IV.Length, OutPtr, Tag, TagLen);
                }
                else // Decrypt.
                {
                    int CipherTextLen = In.Length - System.Convert.ToInt32(TagLen);
                    Tag = new IntPtr(InPtr.ToInt64() + CipherTextLen);
                    Ret = UnsafeNativeMethods.CL_AESDecryptGCM(InPtr, CipherTextLen, AADPtr, AAD.Length, Tag, TagLen, KeyPtr, IVPtr, IV.Length, OutPtr);
                }

                if (Ret < 0)
                {
                    Marshal.FreeHGlobal(Space);
                    throw new Exception(name + " failed with exit code: " + Ret);
                }

                // Copy the output to a byte array (and maybe the tag too, in encryption).
                byte[] Out = new byte[OutLen];
                Marshal.Copy(new IntPtr(Space.ToInt64() + OutBufOffset), Out, 0, OutLen);

                // Encode in base 64.
                string OutBase64Encoded = System.Convert.ToBase64String(Out, 0, OutLen);

                Marshal.FreeHGlobal(Space);
                return OutBase64Encoded;
            }

            private string RSACryptDecrypt(string name, string PEMKey, string InputStr)
            {
                IntPtr RSAKey = getRSAKeyFromPEM(PEMKey);
                if (RSAKey == null)
                {
                    throw new Exception("In " + name + ", Failed to parse RSA PEM formatted key: " + PEMKey);
                }

                byte[] Input;
                if (name == "RSADecrypt")
                    Input = System.Convert.FromBase64String(InputStr);
                else
                    Input = System.Text.Encoding.UTF8.GetBytes(InputStr);


                int OutLen = System.Convert.ToInt32(UnsafeNativeMethods.CL_getRSAKeySize(RSAKey));
                IntPtr InputPtr = Marshal.AllocHGlobal(Input.Length + OutLen);
                Marshal.Copy(Input, 0, InputPtr, Input.Length);
                IntPtr OutPtr = new IntPtr(InputPtr.ToInt64() + Input.Length);

                int Ret = 0;
                if (name == "RSAEncrypt")
                {
                    Ret = System.Convert.ToInt32(UnsafeNativeMethods.CL_encryptRSA(RSAKey, 0, InputPtr, System.Convert.ToUInt32(Input.Length), OutPtr, System.Convert.ToUInt32(OutLen)));
                }
                else // Decrypt
                {
                    OutLen = UnsafeNativeMethods.CL_decryptRSA(RSAKey, 0, InputPtr, System.Convert.ToUInt32(Input.Length), OutPtr, System.Convert.ToUInt32(OutLen));

                    // On error, set special Ret code.
                    if (OutLen < 0)
                        Ret = OutLen;
                }
                if (Ret != 0)
                {
                    Marshal.FreeHGlobal(InputPtr);
                    throw new Exception("In " + name + ", failed with error code: " + Ret);
                }

                // Copy the output to a byte array.
                byte[] Out = new byte[OutLen];
                Marshal.Copy(OutPtr, Out, 0, System.Convert.ToInt32(OutLen));
                Marshal.FreeHGlobal(InputPtr);

                // Return the answer.
                return System.Convert.ToBase64String(Out, 0, System.Convert.ToInt32(OutLen));
            }

            private static IntPtr getRSAKeyFromPEM(string PEMKey)
            {
                byte[] PEMKeyBytes = System.Text.Encoding.UTF8.GetBytes(PEMKey);
                IntPtr PEMKeyPtr = Marshal.AllocHGlobal(PEMKeyBytes.Length);
                Marshal.Copy(PEMKeyBytes, 0, PEMKeyPtr, PEMKeyBytes.Length);
                IntPtr RSAKey;
                if (PEMKey.StartsWith("-----BEGIN PUBLIC KEY-----"))
                {
                    RSAKey = UnsafeNativeMethods.CL_deserializeRSAPubKey(PEMKeyPtr, System.Convert.ToUInt32(PEMKeyBytes.Length));
                }
                else
                {
                    RSAKey = UnsafeNativeMethods.CL_deserializeRSAKeyPair(PEMKeyPtr, System.Convert.ToUInt32(PEMKeyBytes.Length));
                }
                Marshal.FreeHGlobal(PEMKeyPtr);
                return RSAKey;
            }

            public static String DefaultSeed = "6af0257cd3835d81133d693d3fa628bbfd8d054d9a8bbf02803764809d6c86d57d6a04c23c9fa7335320c0582e039b3622a12e09cdc8f3215dc9e42b24956615";
            public static Boolean hasSecureLocalStorageInit = false;

            public static uint doSecurePersistenceInit(String Seed)
            {
                // This only happens once.
                if (hasSecureLocalStorageInit)
                    return 0;

                String Path = Defaults.getSymphonyDataDir();

                byte[] SeedBytes = System.Text.Encoding.UTF8.GetBytes(Seed);
                byte[] PathBytes = System.Text.Encoding.UTF8.GetBytes(Path);

                // Copy to unmanaged memory.
                IntPtr SeedPtr = Marshal.AllocHGlobal(SeedBytes.Length + PathBytes.Length);
                Marshal.Copy(SeedBytes, 0, SeedPtr, SeedBytes.Length);
                IntPtr PathPtr = new IntPtr(SeedPtr.ToInt64() + Seed.Length);
                Marshal.Copy(PathBytes, 0, PathPtr, PathBytes.Length);

                uint ret = UnsafeNativeMethods.CL_InitSecurePersistence(SeedPtr, System.Convert.ToUInt32(Seed.Length), PathPtr, System.Convert.ToUInt32(Path.Length));

                if (ret == 0)
                {
                    hasSecureLocalStorageInit = true;
                }

                Marshal.FreeHGlobal(SeedPtr);

                return ret;

            }
        }
    }

