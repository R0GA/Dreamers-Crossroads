using UnityEngine;

/// <summary>
/// Shared, ordered list of the premade materials a dance floor panel cycles through.
/// Keep exactly one of these per puzzle (or reuse across puzzles that share a color set) so
/// every panel steps through colors in the same order and a panel's "target color" is just
/// an index into this same list. Create via Assets > Create > Puzzles > Dance Floor Palette.
/// </summary>
[CreateAssetMenu(fileName = "DanceFloorPalette", menuName = "Puzzles/Dance Floor Palette")]
public class DanceFloorPalette : ScriptableObject
{
    [System.Serializable]
    public struct ColorEntry
    {
        [Tooltip("Display name only, shown in the inspector — purely for designer sanity, not read at runtime.")]
        public string name;

        [Tooltip("The premade material actually assigned to the panel's renderer for this color.")]
        public Material material;
    }

    [Tooltip("Cycle order. Walking on a panel advances it to (currentIndex + 1) % colors.Length. " +
             "Index 0 is also what 'starting color index' and 'target color index' on a panel refer to.")]
    [SerializeField] private ColorEntry[] colors;

    public int ColorCount => colors.Length;

    public Material GetMaterial(int index) => colors[index].material;

    public string GetName(int index) => colors[index].name;
}
