using System.Collections;
using UnityEngine;

namespace LegendChef.Effects
{
    /// <summary>
    /// 플로팅 데미지 텍스트를 생성하는 싱글톤
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 데미지 텍스트 생성. 크리티컬은 빨간색, 일반은 노란색.
        /// 위로 50px(월드 단위 환산) 이동하며 0.5초 동안 페이드.
        /// </summary>
        public void SpawnFloatingText(Vector3 worldPos, float amount, bool isCrit)
        {
            GameObject go = new GameObject("FloatingText");
            go.transform.position = worldPos;

            TextMesh textMesh = go.AddComponent<TextMesh>();
            textMesh.text = Mathf.CeilToInt(amount).ToString();
            textMesh.characterSize = isCrit ? 0.15f : 0.12f;
            textMesh.fontSize = 100;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = isCrit ? FontStyle.Bold : FontStyle.Normal;

            // 크리티컬: 빨간색, 일반: 노란색
            textMesh.color = isCrit
                ? new Color(1f, 0.2f, 0.2f, 1f)
                : new Color(1f, 0.9f, 0.1f, 1f);

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 100;

            StartCoroutine(FloatAndFade(go, textMesh));
        }

        private IEnumerator FloatAndFade(GameObject go, TextMesh textMesh)
        {
            float duration = 0.5f;
            float riseWorldUnits = 0.5f; // 약 50px 상당
            Vector3 startPos = go.transform.position;
            Color startColor = textMesh.color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 위로 이동
                go.transform.position = startPos + new Vector3(0f, riseWorldUnits * t, 0f);

                // 페이드 아웃
                Color c = startColor;
                c.a = 1f - t;
                textMesh.color = c;

                yield return null;
            }

            Destroy(go);
        }
    }
}
