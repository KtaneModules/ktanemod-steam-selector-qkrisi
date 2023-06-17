using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteamSelector
{
    public static class TextMeshUtils
    {
        public const float WaitTime = 0.045f;
        
        private enum Modifier
        {
            Add,
            Remove
        }

        public static IEnumerator WriteText(TextMesh display, string q, Action callback, bool resize, float wait_time = WaitTime)
        {
            yield return StringWriter(() => display.text, (s) => display.text = s, Modifier.Remove, wait_time);
            if (resize)
                display.characterSize = q.Length > 20 ? 63 : 100;
            yield return StringWriter(() => q, (s) => display.text = s, Modifier.Add, wait_time);
            yield return new WaitForSeconds(wait_time);
            callback();
        }
        
        private static List<string> getBaseList(Func<string> bases)
        {
            return new List<string>(bases().Select(c => c.ToString()));
        }
        
        private static IEnumerator StringWriter(Func<string> bases, Action<string> modify, Modifier modifier, float wait_time)
        {
            if (modifier == Modifier.Remove)
            {
                while (bases().Length > 0)
                {
                    List<string> cList = getBaseList(bases);
                    cList.RemoveAt(cList.Count - 1);
                    modify(String.Join("", cList.ToArray()));
                    yield return new WaitForSeconds(wait_time);
                }
                yield break;
            }
            List<string> chList = getBaseList(bases);
            List<string> final = new List<string>();
            while (chList.Count > 0)
            {
                final.Add(chList[0]);
                chList.RemoveAt(0);
                modify(String.Join("", final.ToArray()));
                yield return new WaitForSeconds(wait_time);
            }
        }
    }
}
