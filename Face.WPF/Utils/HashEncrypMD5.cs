using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace Face.WPF.Utils
{
    public class HashEncrypMD5
    {
        public static string Key { get; } = "@1#9$6*/";

        #region MD5加密解密
        /// <summary>
        /// 16位MD5加密
        /// </summary>
        /// <param name="source">需要加密的明文字符串</param>
        /// <returns></returns>
        public static string MD5Encrypt16(string source)
        {
            var md5 = MD5.Create();
            string cipherText =BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(source)), 4, 8);
            cipherText = cipherText.Replace("-", "");
            return cipherText;
        }

        /// <summary>
        /// 32位MD5加密
        /// </summary>
        /// <param name="source">需要加密的明文字符串</param>
        /// <returns>32位MD5加密密文字符串</returns>
        public static string MD5Encrypt32(string source)
        {
            string rule = "";
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                rule = rule + s[i].ToString("x2"); // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
            }
            return rule;
        }

        /// <summary>
        /// 64位MD5加密
        /// </summary>
        /// <param name="source">需要加密的明文字符串</param>
        /// <returns>64位MD5加密密文字符串</returns>
        public static string MD5Encrypt64(string source)
        {
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
            return Convert.ToBase64String(s);
        }

        /// <summary>
        /// MD5流加密
        /// </summary>
        /// <param name="inputStream">输入流</param>
        /// <returns></returns>
        public static string GenerateMD5(Stream inputStream)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] newBuffer = mi.ComputeHash(inputStream);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// DES数据加密
        /// </summary>
        /// <param name="pToEncrypt">要加密的string字符串</param>
        /// <param name="Key_64">秘钥</param>
        /// <returns></returns>
        public static string Md5Encrypt_Key(string pToEncrypt, string Key_64)
        {
            DES des = DES.Create();
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);

            if (Key_64.Length != 8)
            {
                return "key必须为8位";
            }

            des.Key = Encoding.ASCII.GetBytes(Key_64);
            des.IV = Encoding.ASCII.GetBytes(Key_64);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            var s = ret.ToString();
            return s;
        }

        /// <summary>
        /// DES数据解密
        /// </summary>
        /// <param name="pToDecrypt">解密string</param>
        /// <param name="Key_64">秘钥</param>
        /// <returns></returns>
        public static string Md5Decrypt(string pToDecrypt, string Key_64)
        {
            var des = DES.Create(); //DESCryptoServiceProvider

            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];

            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(Key_64);
            des.IV = Encoding.ASCII.GetBytes(Key_64);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);

            try
            {
                cs.FlushFinalBlock();
            }
            catch (Exception)
            {
                return "无效秘钥";
            }
            return Encoding.Default.GetString(ms.ToArray());
        }

        /// <summary>
        /// DES数据加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Key_64"></param>
        /// <param name="Iv_64"></param>
        /// <returns></returns>
        public static string Md5Encrypt_Key(string pToEncrypt, string Key_64, string Iv_64)
        {
            string KEY_64 = Key_64;// "VavicApp";
            string IV_64 = Iv_64;// "VavicApp";
            try
            {
                byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
                byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
                DES cryptoProvider = DES.Create();
                int i = cryptoProvider.KeySize;
                MemoryStream ms = new MemoryStream();
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cst);
                sw.Write(pToEncrypt);
                sw.Flush();
                cst.FlushFinalBlock();
                sw.Flush();
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
            catch (Exception x)
            {
                return x.Message;
            }
        }

        /// <summary>
        /// DES数据解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Key_64"></param>
        /// <param name="Iv_64"></param>
        /// <returns></returns>
        public static string Md5Decrypt(string pToEncrypt, string Key_64, string Iv_64)
        {
            string KEY_64 = Key_64;// "VavicApp";密钥
            string IV_64 = Iv_64;// "VavicApp"; 向量
            try
            {
                byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
                byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
                byte[] byEnc;
                byEnc = Convert.FromBase64String(pToEncrypt); //把需要解密的字符串转为8位无符号数组
                DES cryptoProvider = DES.Create();
                MemoryStream ms = new MemoryStream(byEnc);
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cst);
                return sr.ReadToEnd();
            }
            catch (Exception x)
            {
                return x.Message;
            }
        }

        #endregion
    }
}