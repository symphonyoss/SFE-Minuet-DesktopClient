using System;
using System.Runtime.InteropServices;

namespace Symphony.Crypto
{
    class UnsafeNativeMethods
    {
        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern Int32 CL_AESEncryptGCM(IntPtr In, Int32 InLen, IntPtr AAD, Int32 AADLen, IntPtr Key, IntPtr IV, Int32 IVLen, IntPtr Out, IntPtr Tag, UInt32 TagLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern Int32 CL_AESDecryptGCM(IntPtr In, Int32 InLen, IntPtr AAD, Int32 AADLen, IntPtr Tag, UInt32 TagLen, IntPtr Key, IntPtr IV, Int32 IVLen, IntPtr Out);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern Int32 CL_PBKDF2(IntPtr Pass, UInt32 PassLen, IntPtr Salt, UInt32 SaltLen, Int32 Iters, IntPtr Out, UInt32 OutLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern IntPtr CL_initRSAKeyPair(IntPtr Seed, UInt32 SeedLen, Int32 KeyLenth);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern void CL_freeRSA(IntPtr RSA);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern Int32 CL_SHA2x256(IntPtr In, UInt32 InLen, IntPtr Out);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern IntPtr CL_serializeRSAPubKey(IntPtr RSA);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern IntPtr CL_serializeRSAKeyPair(IntPtr RSA);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern void CL_freeBuf(IntPtr Buf);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern int CL_getCStrLen(IntPtr Buf);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern IntPtr CL_deserializeRSAPubKey(IntPtr PubKeyBuf, UInt32 PubKeyBufLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern IntPtr CL_deserializeRSAKeyPair(IntPtr KeyPairBuf, UInt32 KeyPairBufLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_getRSAKeySize(IntPtr RSA);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern int CL_decryptRSA(IntPtr RSA, int Pad, IntPtr In, uint InLen, IntPtr Out, uint OutLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_encryptRSA(IntPtr RSAPub, int Pad, IntPtr In, uint InLen, IntPtr Out, uint OutLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_signRSA(IntPtr RSA, int Pad, IntPtr In, uint InLen, IntPtr Out, uint OutLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_verifyRSA(IntPtr RSAPub, int Pad, IntPtr Sig, uint SigLen, IntPtr In, uint InLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_InitSecurePersistence(IntPtr Seed, uint SeedLen, IntPtr Path, uint PathLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_SecurePersist(uint BufID, IntPtr Buf, uint BufLen);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern IntPtr CL_SecureRetrieve(uint BufID, IntPtr retSize);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_SecureDelete(uint BufID);

        [DllImport("SFE-CryptoLib-Project.dll")]
        public static extern uint CL_SecureFree(IntPtr Str, uint StrLen);
    }
}
