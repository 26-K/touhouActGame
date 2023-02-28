using UnityEngine;
using Sirenix.OdinInspector;

namespace PPD
{
    [RequireComponent(typeof(Camera))]
    public class CameraResizer : SingletonMonoBehaviour<CameraResizer>
    {
        [SerializeField]
        private int widthBase = 1080;

        [SerializeField]
        private int heightBase = 1920;

        [SerializeField]
        private float orthographicSizeBase = 5f;

        private float aspectRatioCurrent;

        private void Awake()
        {
            this.aspectRatioCurrent = 0;

            this.SetResize();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.aspectRatioCurrent = 0;
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                this.SetResize();
            }
        }
#endif

        [Button]
        public void SetResize()
        {
            if (Camera.main == null) return;

            var widthCurrent = (float)Screen.width;
            var heightCurrent = (float)Screen.height;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var stringValues = UnityEditor.UnityStats.screenRes.Split('x');

                widthCurrent = float.Parse(stringValues[0]);
                heightCurrent = float.Parse(stringValues[1]);
            }
#endif

            var aspectRatio = widthCurrent / heightCurrent;

            if (aspectRatio == this.aspectRatioCurrent)
            {
                return;
            }

            this.aspectRatioCurrent = aspectRatio;

            // 丸め誤差最小化のために ((float)this.widthBase / (float)this.heightBase) / (widthCurrent / heightCurrent) の順番を変えた
            var rate = (float)this.widthBase * heightCurrent / (float)this.heightBase / widthCurrent;

            Camera.main.orthographicSize = this.orthographicSizeBase * rate;
        }

        [Button]
        public void ClearResize()
        {
            Camera.main.orthographicSize = this.orthographicSizeBase;
        }

        protected override void UnityAwake()
        {
        }
    }
}
