using UnityEngine;

namespace LiveGameLink.UI
{
    [CreateAssetMenu(fileName = "LiveGameLinkUITheme", menuName = "Live Game Link/UI Theme", order = 10)]
    public class UITheme : ScriptableObject
    {
        public Color background  = new Color(0.10f, 0.08f, 0.16f, 0.92f);
        public Color text        = Color.white;
        public Color accent      = new Color(0.57f, 0.27f, 0.99f);
        public Color success     = new Color(0.42f, 0.78f, 0.42f);
        public Color danger      = new Color(0.82f, 0.34f, 0.34f);
        public float cornerRadius = 8f;
    }
}
