using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LionReferrals
{
    public static class Utils
    {
        public static string ADID { get; set; }

        public static void SetAdvertiserId()
        {
#if UNITY_EDITOR
            ADID = "e3a7a422763883844efc27547766b3f1"; //for testing
#elif UNITY_ANDROID
            try
            {
                var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                var adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

                ADID = adInfo.Call<string>("getId");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
#else
            Application.RequestAdvertisingIdentifierAsync(
                (string adid, bool trackingEnabled, string error) =>
                {
                    ADID = adid;
                });
#endif
        }
        
        public static string CreateMD5(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            
            var sb = new StringBuilder();
            
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            
            return sb.ToString().ToLower();
        }
        
        public static IEnumerator SetSharePic(string picPath, Texture2D tex)
        {
            var url = Path.Combine(Application.streamingAssetsPath, picPath);

            byte[] imgData;

            if (url.Contains("://") || url.Contains(":///"))
            {
                var www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                imgData = www.downloadHandler.data;
            }
            else
            {
                imgData = File.ReadAllBytes(url);
            }

            tex.LoadImage(imgData);
        }
    }
}