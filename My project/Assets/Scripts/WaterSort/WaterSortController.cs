using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ColorDrive
{
    /// <summary>
    /// Top-level controller for Water Sort puzzle mode.
    /// Manages tube selection, pouring, win detection, and undo.
    /// </summary>
    public class WaterSortController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject tubePrefab;
        [SerializeField] private Transform tubeContainer;

        private List<Tube> _tubes = new List<Tube>();
        private Tube _selectedTube = null;
        private LevelData _levelData;
        private int _movesUsed = 0;
        private int _moveLimit;
        private bool _gameActive = false;

        // Undo stack
        private Stack<(int fromIndex, int toIndex, int layers, int color)> _undoStack
            = new Stack<(int, int, int, int)>();

        public int MovesUsed => _movesUsed;
        public int MovesRemaining => _moveLimit - _movesUsed;
        public bool IsGameActive => _gameActive;

        public event System.Action OnLevelComplete;
        public event System.Action OnGameOver;
        public event System.Action<int> OnMovesChanged;

        public void StartLevel(LevelData levelData)
        {
            _levelData = levelData;
            _moveLimit = levelData.waterSortMoveLimit;
            _movesUsed = 0;
            _gameActive = true;
            _undoStack.Clear();

            BuildTubes(levelData);
        }

        private void BuildTubes(LevelData levelData)
        {
            // Clear existing tubes
            foreach (Transform child in tubeContainer)
                Destroy(child.gameObject);
            _tubes.Clear();

            int tubeCount = levelData.tubeCount;
            int colorCount = Mathf.Min(levelData.colorPalette.Length, tubeCount - 2); // 2 empty tubes
            int depth = levelData.layerDepth;

            // Parse CSV layout or generate random
            List<List<int>> tubeData;
            if (!string.IsNullOrEmpty(levelData.tubeLayoutCsv))
                tubeData = ParseTubeCsv(levelData.tubeLayoutCsv);
            else
                tubeData = GenerateTubeLayout(tubeCount, colorCount, depth);

            float spacing = 1.3f;
            float totalWidth = (tubeCount - 1) * spacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < tubeCount; i++)
            {
                Vector3 pos = tubeContainer.position + new Vector3(startX + i * spacing, 0, 0);
                GameObject tubeGO;

                if (tubePrefab != null)
                    tubeGO = Instantiate(tubePrefab, pos, Quaternion.identity, tubeContainer);
                else
                {
                    tubeGO = new GameObject($"Tube_{i}");
                    tubeGO.transform.SetParent(tubeContainer);
                    tubeGO.transform.position = pos;
                }

                var tube = tubeGO.GetComponent<Tube>();
                if (tube == null) tube = tubeGO.AddComponent<Tube>();

                tube.Initialize(i, depth, i < tubeData.Count ? tubeData[i] : new List<int>());
                tube.OnTubeSelected += HandleTubeSelected;
                _tubes.Add(tube);
            }
        }

        private List<List<int>> GenerateTubeLayout(int tubeCount, int colorCount, int depth)
        {
            // Create a solvable layout: each color appears exactly `depth` times
            var allLayers = new List<int>();
            for (int c = 0; c < colorCount; c++)
                for (int d = 0; d < depth; d++)
                    allLayers.Add(c);

            // Shuffle
            for (int i = allLayers.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (allLayers[i], allLayers[j]) = (allLayers[j], allLayers[i]);
            }

            var result = new List<List<int>>();
            int idx = 0;
            for (int t = 0; t < tubeCount - 2; t++) // leave 2 empty
            {
                var tube = new List<int>();
                for (int d = 0; d < depth && idx < allLayers.Count; d++, idx++)
                    tube.Add(allLayers[idx]);
                result.Add(tube);
            }
            // 2 empty tubes
            result.Add(new List<int>());
            result.Add(new List<int>());

            return result;
        }

        private List<List<int>> ParseTubeCsv(string csv)
        {
            var result = new List<List<int>>();
            foreach (string row in csv.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                var tube = new List<int>();
                foreach (string val in row.Split(','))
                    if (int.TryParse(val.Trim(), out int c))
                        tube.Add(c);
                result.Add(tube);
            }
            return result;
        }

        private void HandleTubeSelected(int tubeIndex)
        {
            if (!_gameActive) return;
            Tube tapped = _tubes[tubeIndex];

            if (_selectedTube == null)
            {
                // Select non-empty tube
                if (!tapped.IsEmpty)
                {
                    _selectedTube = tapped;
                    tapped.SetSelected(true);
                }
                return;
            }

            // Deselect if same tube tapped
            if (_selectedTube == tapped)
            {
                _selectedTube.SetSelected(false);
                _selectedTube = null;
                return;
            }

            // Try to pour
            if (tapped.CanReceiveFrom(_selectedTube))
            {
                int fromIdx = _selectedTube.TubeIndex;
                int color = _selectedTube.TopColor;
                int poured = tapped.PourFrom(_selectedTube);

                if (poured > 0)
                {
                    _undoStack.Push((fromIdx, tapped.TubeIndex, poured, color));
                    _movesUsed++;
                    OnMovesChanged?.Invoke(MovesRemaining);
                    GameManager.Instance?.AddScore(poured * 20);

                    if (CheckWin())
                        TriggerLevelComplete();
                    else if (_moveLimit > 0 && _movesUsed >= _moveLimit)
                        TriggerGameOver();
                }
            }

            _selectedTube.SetSelected(false);
            _selectedTube = null;
        }

        public void Undo()
        {
            if (!_gameActive || _undoStack.Count == 0) return;

            var (fromIndex, toIndex, layers, color) = _undoStack.Pop();
            Tube from = _tubes[fromIndex];
            Tube to = _tubes[toIndex];

            // Remove layers from destination
            for (int i = 0; i < layers; i++)
                to.PopTop();

            // Push layers back to source using direct Push (avoids pour validation)
            for (int i = 0; i < layers; i++)
                from.Push(color);

            from.UpdateVisuals();
            to.UpdateVisuals();

            _movesUsed = Mathf.Max(0, _movesUsed - 1);
            OnMovesChanged?.Invoke(MovesRemaining);
        }

        private bool CheckWin()
        {
            foreach (var tube in _tubes)
                if (!tube.IsEmpty && !tube.IsComplete)
                    return false;
            return true;
        }

        private void TriggerLevelComplete()
        {
            _gameActive = false;
            OnLevelComplete?.Invoke();
            GameManager.Instance?.CompleteLevel();
        }

        private void TriggerGameOver()
        {
            _gameActive = false;
            OnGameOver?.Invoke();
            GameManager.Instance?.TriggerGameOver();
        }

        public bool HasValidMove()
        {
            for (int i = 0; i < _tubes.Count; i++)
                for (int j = 0; j < _tubes.Count; j++)
                    if (i != j && _tubes[j].CanReceiveFrom(_tubes[i]))
                        return true;
            return false;
        }

        public (int from, int to) GetHint()
        {
            for (int i = 0; i < _tubes.Count; i++)
                for (int j = 0; j < _tubes.Count; j++)
                    if (i != j && _tubes[j].CanReceiveFrom(_tubes[i]))
                        return (i, j);
            return (-1, -1);
        }
    }
}
