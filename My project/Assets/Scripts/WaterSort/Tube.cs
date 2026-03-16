using UnityEngine;
using System.Collections.Generic;

namespace ColorDrive
{
    /// <summary>
    /// Represents a single tube (bottle) in the Water Sort puzzle.
    /// Stores a stack of color layers.
    /// </summary>
    public class Tube : MonoBehaviour
    {
        [Header("Tube Settings")]
        [SerializeField] private int maxLayers = 4;

        private Stack<int> _layers = new Stack<int>(); // color indices
        private int _tubeIndex;
        private bool _isSelected = false;
        private float _restY;

        [Header("Visual")]
        [SerializeField] private Transform[] layerSlots; // visual positions bottom-to-top
        [SerializeField] private SpriteRenderer tubeRenderer;
        [SerializeField] private SpriteRenderer selectionGlow;

        public int TubeIndex => _tubeIndex;
        public int LayerCount => _layers.Count;
        public int MaxLayers => maxLayers;
        public bool IsEmpty => _layers.Count == 0;
        public bool IsFull => _layers.Count >= maxLayers;
        public bool IsComplete => _layers.Count == maxLayers && AllSameColor();
        public int TopColor => _layers.Count > 0 ? _layers.Peek() : -1;

        public event System.Action<int> OnTubeSelected;

        void Start()
        {
            if (selectionGlow != null)
                selectionGlow.enabled = false;
        }

        public void Initialize(int index, int capacity, List<int> initialLayers)
        {
            _tubeIndex = index;
            maxLayers = capacity;
            _restY = transform.position.y;
            _layers.Clear();

            // Push from bottom to top
            for (int i = 0; i < initialLayers.Count; i++)
                _layers.Push(initialLayers[i]);

            UpdateVisuals();
        }

        /// <summary>
        /// How many top layers share the same color as the TopColor.
        /// </summary>
        public int TopColorCount()
        {
            if (_layers.Count == 0) return 0;
            int topColor = TopColor;
            int count = 0;
            foreach (int c in _layers)
            {
                if (c == topColor) count++;
                else break;
            }
            return count;
        }

        public bool CanReceiveFrom(Tube source)
        {
            if (source == null || source.IsEmpty) return false;
            if (IsFull) return false;
            if (IsEmpty) return true;
            return TopColor == source.TopColor;
        }

        /// <summary>
        /// Pour from source tube into this tube. Returns number of layers poured.
        /// </summary>
        public int PourFrom(Tube source)
        {
            if (!CanReceiveFrom(source)) return 0;

            int poured = 0;
            int sourceTopColor = source.TopColor;

            while (!source.IsEmpty && !IsFull && source.TopColor == sourceTopColor)
            {
                int layer = source.PopTop();
                _layers.Push(layer);
                poured++;
            }

            UpdateVisuals();
            source.UpdateVisuals();

            return poured;
        }

        public int PopTop()
        {
            if (_layers.Count == 0) return -1;
            return _layers.Pop();
        }

        public void Push(int colorIndex)
        {
            if (!IsFull)
                _layers.Push(colorIndex);
        }

        private bool AllSameColor()
        {
            if (_layers.Count == 0) return false;
            int first = -1;
            foreach (int c in _layers)
            {
                if (first == -1) first = c;
                else if (c != first) return false;
            }
            return true;
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (selectionGlow != null)
                selectionGlow.enabled = selected;

            TweenHelper.MoveY(gameObject, selected ? _restY + 0.5f : _restY, 0.15f);
        }

        public void UpdateVisuals()
        {
            if (layerSlots == null) return;

            // Build current layer array (bottom to top)
            int[] layerArray = new int[maxLayers];
            for (int i = 0; i < maxLayers; i++) layerArray[i] = -1;

            // Stack iterates top to bottom, so reverse
            var temp = new List<int>(_layers);
            temp.Reverse();
            for (int i = 0; i < temp.Count && i < maxLayers; i++)
                layerArray[i] = temp[i];

            for (int i = 0; i < layerSlots.Length && i < maxLayers; i++)
            {
                if (layerSlots[i] == null) continue;
                var sr = layerSlots[i].GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                if (layerArray[i] >= 0)
                {
                    sr.enabled = true;
                    Color c = ThemeManager.Instance != null
                        ? ThemeManager.Instance.GetBlockColor(layerArray[i])
                        : Color.white;
                    sr.color = c;
                }
                else
                {
                    sr.enabled = false;
                }
            }
        }

        void OnMouseDown()
        {
            OnTubeSelected?.Invoke(_tubeIndex);
        }
    }
}
